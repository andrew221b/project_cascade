using System.Collections.Generic;
using System.Linq;
using Cascade.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    [System.Serializable] public sealed class WoodBodyFrame {public float left,bottom,width,height;}
    public enum PiecePose { Resting,Held,Exchanging,Falling,Settling,Delivered }
    public sealed partial class ToyMachineView
    {
        Material woodMaterial,ropeMaterial,branchMaterial;
        readonly List<Mesh> woodMeshes=new List<Mesh>();
        readonly Vector3[] dynamicOffsets=new Vector3[8],visualOffsets=new Vector3[8];
        readonly Vector2[] visualStretch=new Vector2[8];
        readonly float[] visualAngles=new float[8];
        readonly float[] rocking=new float[8],rockVelocity=new float[8];
        readonly PiecePose[] piecePoses=new PiecePose[8];
        int heldWeight=-1;int? forecastHp;
        public Vector3 TokenVisualPosition(int index)=>tokens[index].transform.position;
        public Vector3 TokenVisualScale(int index)=>tokens[index].transform.localScale;
        public Vector3 TokenBaseScale(int index)=>restScales[index];
        public Vector3 TokenBodySize(int index)=>Vector3.Scale(tokens[index].mesh.bounds.size,restScales[index]);
        public Vector3 TokenBodyCentre(int index)=>Vector3.Scale(tokens[index].mesh.bounds.center,restScales[index]);
        public float TokenVisualAngle(int index)=>tokens[index].transform.eulerAngles.z;
        public PiecePose TokenPose(int index)=>piecePoses[index];
        public float LoadedSag=>Frame!=null&&Frame.ActiveFloor<floors.Count?floors[Frame.ActiveFloor].sag:0;
        public void SetHeldWeight(int index){
            if(index!=heldWeight&&Frame!=null&&Frame.Status==PuzzleStatus.Playing){
                int slot=index>=0?index:heldWeight;
                if(slot>=0)ContactImpulse(Frame.ActiveFloor,slot,index>=0?-.16f:.24f,0);
            }
            heldWeight=index;
        }
        public void LandWeight(int floor)
        {ContactImpulse(floor,tokens.Count/2,1,0);}
        public void ContactImpulse(int floor,int slot,float strength,float lateral)
        {
            if(reducedMotion||floor<0||floor>=floors.Count||tokens.Count==0)return;
            float mass=PieceMass(slot),point=(slot+.5f)/tokens.Count-.5f;var rig=floors[floor];
            float impulse=strength*mass;rig.sagVelocity+=impulse*.28f;rig.rollVelocity-=point*impulse*3;
            rig.swayVelocity+=lateral*mass*.12f;
            rockVelocity[slot]+=lateral*5+point*impulse*3;
            for(int side=0;side<2;side++)for(int span=floor;span<=floor+1;span++)
                ropeSpans[side,span]?.Impulse(new Vector2(lateral*.07f+(side==0?-1:1)*impulse*.04f,-impulse*.12f)*(side==0?.5f-point:.5f+point));
        }
        float PieceMass(int slot)=>Frame.Row[slot].Kind==SymbolKind.Glyph?1.2f:1+(art.ShapeIndex(Frame.Row[slot])%3)*.12f;
        public bool DebrisSupport(int floor,float x,out Vector3 position,out float width,out float angle)
        {
            position=Vector3.zero;width=angle=0;
            if(Frame==null||floor>=floors.Count||!Frame.FloorVisible(floor)||floors[floor].fracture>.20f)return false;
            position=SurfacePoint(floor,x,.365f);position.z=-.7f;width=floors[floor].board.baseScale.x*.95f/3;angle=floors[floor].roll;return true;
        }
        public void SetPiecePose(int index,PiecePose pose,float angle=0)
        {piecePoses[index]=pose;dynamicOffsets[index]=(pose==PiecePose.Falling||pose==PiecePose.Exchanging)?Vector3.zero:TokenSupportOffset(index);visualAngles[index]=reducedMotion?0:angle;ApplyPiece(index);}
        public Vector3 TokenSupportOffset(int index)=>Frame.Status==PuzzleStatus.Playing?FloorPointOffset(Frame.ActiveFloor,restPositions[index],heldWeight!=index):Vector3.zero;
        public void ResetWoodPoses()
        {
            heldWeight=-1;forecastHp=null;
            foreach(var rig in floors)rig.fracture=0;
            for(int i=0;i<tokens.Count;i++){dynamicOffsets[i]=Vector3.zero;visualAngles[i]=rocking[i]=rockVelocity[i]=0;piecePoses[i]=Frame.Status==PuzzleStatus.Playing?PiecePose.Resting:PiecePose.Delivered;}
        }
        public void PreviewForecast(TransitionResult prediction)
        {
            forecastHp=null;
            if(prediction!=null&&prediction.Accepted)foreach(var e in prediction.Events)
                if(e.Kind==PuzzleEventKind.DurabilityResolved&&e.Floor==Frame.ActiveFloor)forecastHp=Mathf.Max(0,e.Value);
            Sync();
        }
        void InitializeWoodMaterials()
        {
            if(woodMaterial!=null)return;
            var texture=Resources.Load<Texture2D>("CascadeArt/Reactions/TimberStates");
            woodMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Painted honey wood"};
            woodMaterial.SetTexture("_BaseMap",texture);
            var frame=WorkshopSpriteSet.WoodUv(0);woodMaterial.SetTextureScale("_BaseMap",new Vector2(frame.x,frame.y));woodMaterial.SetTextureOffset("_BaseMap",new Vector2(frame.z,frame.w));
            woodMaterial.SetColor("_BaseColor",texture!=null?Color.white:new Color(.65f,.35f,.13f));
            woodMaterial.SetFloat("_Smoothness",.32f);
            woodMaterial.SetFloat("_AlphaClip",1);woodMaterial.SetFloat("_Cutoff",.12f);woodMaterial.EnableKeyword("_ALPHATEST_ON");woodMaterial.SetOverrideTag("RenderType","TransparentCutout");woodMaterial.renderQueue=2450;
            ropeMaterial=new Material(art.gold){name="Warm braided hemp"};ropeMaterial.SetColor("_BaseColor",new Color(.73f,.51f,.26f));
            branchMaterial=new Material(woodMaterial){name="Natural honey-wood branch stops"};
            branchMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("CascadeArt/Woodland/BranchStops"));
            branchMaterial.SetTextureScale("_BaseMap",Vector2.one);branchMaterial.SetTextureOffset("_BaseMap",Vector2.zero);
            branchMaterial.SetFloat("_Cutoff",.45f);branchMaterial.SetFloat("_Smoothness",.24f);
        }
        Mesh BoardMesh()
        {
            // Flexible strip only; cracks and broken chunks come from authored texture states.
            const int columns=18;var vertices=new Vector3[columns*2];var uv=new Vector2[vertices.Length];var triangles=new int[16*6];int triangle=0;
            for(int x=0;x<columns;x++)for(int y=0;y<2;y++){int i=x*2+y;float u=(x<9?x:x-1)/16f;vertices[i]=new Vector3(u-.5f,y-.5f,0);uv[i]=new Vector2(u,y);}
            for(int x=0;x<columns-1;x++){if(x==8)continue;int v=x*2;triangles[triangle++]=v;triangles[triangle++]=v+1;triangles[triangle++]=v+2;triangles[triangle++]=v+1;triangles[triangle++]=v+3;triangles[triangle++]=v+2;}
            var mesh=new Mesh{name="Flexible wooden strip"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.bounds=new Bounds(Vector3.zero,new Vector3(1.1f,22,1));woodMeshes.Add(mesh);return mesh;
        }
        static void Spring(ref float position,ref float velocity,float target,float stiffness,float damping,float seconds)
        {
            // Bounded substeps keep the decorative spring stable across slow Editor frames.
            int steps=Mathf.Max(1,Mathf.CeilToInt(seconds/.012f));float dt=seconds/steps;
            for(int i=0;i<steps;i++){velocity+=(target-position)*stiffness*dt;velocity*=Mathf.Exp(-damping*dt);position+=velocity*dt;}
        }
        Mesh BoardTopMesh()
        {
            const int columns=17;var vertices=new Vector3[columns*2];var uv=new Vector2[vertices.Length];var triangles=new int[(columns-1)*6];
            for(int x=0;x<columns;x++)for(int d=0;d<2;d++){int i=x*2+d;vertices[i]=new Vector3(x/16f-.5f,0,d-.5f);uv[i]=new Vector2(x/16f,.46f+d*.10f);}
            for(int x=0;x<columns-1;x++){int v=x*2,t=x*6;triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v+1;triangles[t+4]=v+3;triangles[t+5]=v+2;}
            var mesh=new Mesh{name="Depth and shadow receiving timber top"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.bounds=new Bounds(Vector3.zero,new Vector3(1.1f,22,1.1f));woodMeshes.Add(mesh);return mesh;
        }
        Vector3 FloorPointOffset(int floor,Vector3 point,bool bend)
        {
            var rig=floors[floor];var local=point-rig.board.basePosition;
            var rotation=Quaternion.Euler(0,0,rig.roll);
            float x=Mathf.Clamp01(local.x/Mathf.Max(.01f,rig.board.baseScale.x)+.5f);
            var bowed=local+(bend?Vector3.down*Mathf.Sin(x*Mathf.PI)*rig.sag:Vector3.zero);
            return rotation*bowed-local+new Vector3(rig.sway,floorOffsets[floor],0);
        }
        public void TickWoodDynamics(float seconds)
        {
            if(interaction==null||Frame==null||!gameplayView)return;
            if(seconds>0&&(interaction.IsPaused||interaction.IsSuspended||ambientPaused))return;
            float dt=Mathf.Clamp(seconds,0,.05f);
            for(int f=0;f<floors.Count;f++) {
                var rig=floors[f];bool loaded=Frame.Status==PuzzleStatus.Playing&&Frame.ActiveFloor==f;
                float grounded=0,total=0;for(int i=0;i<tokens.Count;i++){float mass=PieceMass(i);total+=mass;if(heldWeight!=i&&piecePoses[i]!=PiecePose.Falling)grounded+=mass;}
                float weight=loaded?grounded/Mathf.Max(1,total):0;
                float targetSag=(.10f+.17f*rig.wear)*weight+rig.fracture*.19f;float wind=WindAt(f);
                float tilt=reducedMotion?0:wind*1.2f+(loaded&&heldWeight>=0?((heldWeight+.5f)/tokens.Count-.5f)*.45f:0);
                if(reducedMotion){rig.sag=targetSag;rig.sagVelocity=rig.sway=rig.swayVelocity=rig.roll=rig.rollVelocity=0;}
                Spring(ref rig.sag,ref rig.sagVelocity,targetSag,120,14,dt);
                Spring(ref rig.sway,ref rig.swayVelocity,wind*.12f,38,7.2f,dt);
                Spring(ref rig.roll,ref rig.rollVelocity,tilt,32,5.5f,dt);
                rig.sway=Mathf.Clamp(rig.sway,-.22f,.22f);rig.roll=Mathf.Clamp(rig.roll,-2.4f,2.4f);
                for(int i=0;i<rig.vertices.Length;i++){
                    int column=i/2;float x=(column<9?column:column-1)/16f;
                    float width=rig.board.baseScale.x,height=Mathf.Max(.01f,rig.board.baseScale.y);
                    float side=column<9?-1:1,pivot=side*.475f;
                    var point=new Vector3((x-.5f-pivot)*width,((i%2)-.5f)*height-Mathf.Sin(x*Mathf.PI)*rig.sag,0);
                    rig.vertices[i]=new Vector3(pivot+point.x/width,point.y/height,0);
                }
                int stage=rig.fracture>.22f?3:rig.wear>=.60f?2:rig.wear>0||interaction.Attempt.Level.Floors[f].FragileEdges.Count>0?1:0;
                rig.woodProperties.SetVector("_BaseMap_ST",WorkshopSpriteSet.WoodUv(stage));
                rig.woodProperties.SetColor("_BaseColor",new Color(1,1,1,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.36f,.82f,rig.fracture))));rig.board.renderer.SetPropertyBlock(rig.woodProperties);
                rig.bendMesh.vertices=rig.vertices; // Static bounds already enclose the small cosmetic deflection.
                for(int i=0;i<rig.topVertices.Length;i++){float x=i/2/16f;rig.topVertices[i].y=((i%2==0?-.035f:.035f)-Mathf.Sin(x*Mathf.PI)*rig.sag)/Mathf.Max(.001f,rig.top.baseScale.y);}
                rig.topMesh.vertices=rig.topVertices;rig.topMesh.RecalculateNormals();rig.top.renderer.SetPropertyBlock(rig.woodProperties);
                foreach(var part in rig.parts) {
                    if(rig.rope.Contains(part)) {
                        part.transform.position=part.basePosition+new Vector3(rig.sway*.5f,floorOffsets[f]*.5f,0);
                        part.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(rig.sway,Mathf.Max(.01f,part.baseScale.y))*Mathf.Rad2Deg);
                    } else {
                        part.transform.position=part.basePosition+FloorPointOffset(f,part.basePosition,part!=rig.board&&part!=rig.top);
                        part.transform.rotation=Quaternion.Euler(0,0,rig.roll)*part.baseRotation;
                    }
                }
                if(rig.bombFlame!=null&&!reducedMotion)rig.bombFlame.transform.localScale=rig.bombFlame.baseScale*(1+Mathf.Sin(windTime*7)*.025f);
            }
            UpdateRopeRig(dt);
            for(int i=0;i<tokens.Count;i++) {
                float target=reducedMotion?0:WindAt(Mathf.Min(Frame.ActiveFloor,floors.Count-1))*.65f/PieceMass(i);
                if(reducedMotion)rocking[i]=rockVelocity[i]=0;
                else Spring(ref rocking[i],ref rockVelocity[i],target,58,8,dt);
                rocking[i]=Mathf.Clamp(rocking[i],-2,2);
                if(Frame.Status==PuzzleStatus.Playing&&piecePoses[i]!=PiecePose.Falling&&piecePoses[i]!=PiecePose.Exchanging) {
                    dynamicOffsets[i]=FloorPointOffset(Frame.ActiveFloor,restPositions[i],heldWeight!=i);
                } else dynamicOffsets[i]=Vector3.zero;
                ApplyPiece(i);
            }
        }
        void ApplyPiece(int index)
        {
            if(index>=tokens.Count)return;
            var stretch=visualStretch[index];if(stretch==Vector2.zero)stretch=Vector2.one;
            var token=tokens[index].transform;token.position=restPositions[index]+dynamicOffsets[index]+visualOffsets[index];
            bool supported=Frame.Status==PuzzleStatus.Playing&&piecePoses[index]!=PiecePose.Falling&&piecePoses[index]!=PiecePose.Exchanging;
            float supportAngle=supported?floors[Frame.ActiveFloor].roll:0;
            float rock=supported&&piecePoses[index]!=PiecePose.Held?rocking[index]:0;
            token.position+=Vector3.up*(Mathf.Abs(Mathf.Sin(rock*Mathf.Deg2Rad))*tokens[index].mesh.bounds.extents.x*restScales[index].x);
            token.localScale=Vector3.Scale(restScales[index],new Vector3(stretch.x,stretch.y,1));token.rotation=Quaternion.Euler(0,0,visualAngles[index]+supportAngle+rock);
            UpdateTouchContour(index);
            if(index<tokenShadows.Count) {
                var shadow=tokenShadows[index];float lift=Mathf.Max(0,visualOffsets[index].y);
                var shadowOffset=supported?FloorPointOffset(Frame.ActiveFloor,shadow.basePosition,true):dynamicOffsets[index];
                shadow.transform.position=shadow.basePosition+shadowOffset+Vector3.right*visualOffsets[index].x*.25f;
                shadow.transform.rotation=Quaternion.Euler(0,0,supported?floors[Frame.ActiveFloor].roll:0);
                shadow.transform.localScale=shadow.baseScale*(1+Mathf.Min(lift,.6f)*.45f);
                shadow.renderer.enabled=piecePoses[index]!=PiecePose.Falling&&lift<.9f;
            }
        }
        void OnDestroy()
        {
            if(continuousRopeMaterial!=null)Destroy(continuousRopeMaterial);if(woodMaterial!=null)Destroy(woodMaterial);if(ropeMaterial!=null)Destroy(ropeMaterial);if(branchMaterial!=null)Destroy(branchMaterial);
            DisposePieceMaterials();
            if(touchGlowMaterial!=null)Destroy(touchGlowMaterial);
            foreach(var mesh in woodMeshes)if(mesh!=null)Destroy(mesh);
        }
    }
}
