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
        Material woodMaterial,ropeMaterial;
        readonly List<Mesh> woodMeshes=new List<Mesh>();
        readonly Vector3[] dynamicOffsets=new Vector3[8],visualOffsets=new Vector3[8];
        readonly Vector2[] visualStretch=new Vector2[8];
        readonly float[] visualAngles=new float[8];
        readonly PiecePose[] piecePoses=new PiecePose[8];
        int heldWeight=-1;int? forecastHp;
        public Vector3 TokenVisualPosition(int index)=>tokens[index].transform.position;
        public float TokenVisualAngle(int index)=>tokens[index].transform.eulerAngles.z;
        public PiecePose TokenPose(int index)=>piecePoses[index];
        public float LoadedSag=>Frame!=null&&Frame.ActiveFloor<floors.Count?floors[Frame.ActiveFloor].sag:0;
        public void SetHeldWeight(int index){heldWeight=index;}
        public void LandWeight(int floor)
        {if(!reducedMotion&&floor>=0&&floor<floors.Count)floors[floor].sagVelocity+=.25f;}
        public void SetPiecePose(int index,PiecePose pose,float angle=0)
        {piecePoses[index]=pose;visualAngles[index]=reducedMotion?0:angle;ApplyPiece(index);}
        public void ResetWoodPoses()
        {
            heldWeight=-1;forecastHp=null;
            foreach(var rig in floors){rig.sag=rig.sagVelocity=rig.sway=rig.swayVelocity=0;}
            for(int i=0;i<tokens.Count;i++){dynamicOffsets[i]=Vector3.zero;visualAngles[i]=0;piecePoses[i]=Frame.Status==PuzzleStatus.Playing?PiecePose.Resting:PiecePose.Delivered;}
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
            var texture=Resources.Load<Texture2D>("CascadeArt/Woodland/WoodBoard");
            woodMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name="Painted honey wood"};
            woodMaterial.SetTexture("_BaseMap",texture);
            var frameSource=Resources.Load<TextAsset>("CascadeArt/Woodland/WoodBoardFrame");
            if(frameSource!=null){var frame=JsonUtility.FromJson<WoodBodyFrame>(frameSource.text);woodMaterial.SetTextureScale("_BaseMap",new Vector2(frame.width,frame.height));woodMaterial.SetTextureOffset("_BaseMap",new Vector2(frame.left,frame.bottom));}
            woodMaterial.SetColor("_BaseColor",texture!=null?Color.white:new Color(.65f,.35f,.13f));
            woodMaterial.SetFloat("_Surface",1);woodMaterial.SetFloat("_ZWrite",0);woodMaterial.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
            woodMaterial.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);woodMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");woodMaterial.renderQueue=3000;
            ropeMaterial=new Material(art.gold){name="Warm braided hemp"};ropeMaterial.SetColor("_BaseColor",new Color(.73f,.51f,.26f));
        }
        Mesh BoardMesh()
        {
            const int columns=17;var vertices=new Vector3[columns*2];var uv=new Vector2[vertices.Length];var triangles=new int[(columns-1)*6];
            for(int x=0;x<columns;x++)for(int y=0;y<2;y++){int i=x*2+y;vertices[i]=new Vector3(x/(columns-1f)-.5f,y-.5f,0);uv[i]=new Vector2(x/(columns-1f),y);}
            for(int x=0;x<columns-1;x++){int i=x*6,v=x*2;triangles[i]=v;triangles[i+1]=v+1;triangles[i+2]=v+2;triangles[i+3]=v+1;triangles[i+4]=v+3;triangles[i+5]=v+2;}
            var mesh=new Mesh{name="Flexible wooden strip"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.bounds=new Bounds(Vector3.zero,new Vector3(1,2,1));woodMeshes.Add(mesh);return mesh;
        }
        static void Spring(ref float position,ref float velocity,float target,float stiffness,float damping,float seconds)
        {
            // Bounded substeps keep the decorative spring stable across slow Editor frames.
            int steps=Mathf.Max(1,Mathf.CeilToInt(seconds/.012f));float dt=seconds/steps;
            for(int i=0;i<steps;i++){velocity+=(target-position)*stiffness*dt;velocity*=Mathf.Exp(-damping*dt);position+=velocity*dt;}
        }
        public void TickWoodDynamics(float seconds)
        {
            if(interaction==null||Frame==null||interaction.IsPaused||interaction.IsSuspended)return;
            float dt=Mathf.Clamp(seconds,0,.05f);
            for(int f=0;f<floors.Count;f++) {
                var rig=floors[f];bool loaded=Frame.Status==PuzzleStatus.Playing&&Frame.ActiveFloor==f;
                int grounded=0;for(int i=0;i<tokens.Count;i++)if(heldWeight!=i&&piecePoses[i]!=PiecePose.Falling)grounded++;
                float weight=loaded?grounded/(float)Mathf.Max(1,tokens.Count):0;
                float targetSag=reducedMotion?0:.045f*weight;float wind=reducedMotion?0:Mathf.Sin(windTime*.65f+f*.52f)*.025f;
                Spring(ref rig.sag,ref rig.sagVelocity,targetSag,120,14,dt);Spring(ref rig.sway,ref rig.swayVelocity,wind,45,9,dt);
                for(int i=0;i<rig.vertices.Length;i++){float x=(i/2)/16f;rig.vertices[i].y=(i%2)-.5f-Mathf.Sin(x*Mathf.PI)*rig.sag/Mathf.Max(.01f,rig.board.transform.localScale.y);}
                rig.bendMesh.vertices=rig.vertices; // Static bounds already enclose the small cosmetic deflection.
                foreach(var part in rig.parts) {
                    float bend=part==rig.board||rig.rope.Contains(part)?0:-Mathf.Sin(Mathf.Clamp01(part.point.x)*Mathf.PI)*rig.sag;
                    if(rig.rope.Contains(part)) {
                        part.transform.position=part.basePosition+new Vector3(rig.sway*.5f,floorOffsets[f]*.5f,0);
                        part.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(rig.sway,Mathf.Max(.01f,part.baseScale.y))*Mathf.Rad2Deg);
                    } else {
                        part.transform.position=part.basePosition+new Vector3(part.transform.name=="Upper rope peg"?0:rig.sway,bend+floorOffsets[f],0);
                        part.transform.rotation=part.baseRotation;
                    }
                }
                if(rig.bombFlame!=null&&!reducedMotion)rig.bombFlame.transform.localScale=rig.bombFlame.baseScale*(1+Mathf.Sin(windTime*7)*.025f);
            }
            UpdateRopeRig();
            for(int i=0;i<tokens.Count;i++) {
                if(Frame.Status==PuzzleStatus.Playing&&piecePoses[i]!=PiecePose.Falling&&piecePoses[i]!=PiecePose.Exchanging) {
                    var rig=floors[Frame.ActiveFloor];float x=(i+.5f)/tokens.Count;
                    dynamicOffsets[i]=new Vector3(rig.sway,heldWeight==i?0:-Mathf.Sin(x*Mathf.PI)*rig.sag,0);
                } else dynamicOffsets[i]=Vector3.zero;
                ApplyPiece(i);
            }
        }
        void ApplyPiece(int index)
        {
            if(index>=tokens.Count)return;
            var stretch=visualStretch[index];if(stretch==Vector2.zero)stretch=Vector2.one;
            var token=tokens[index].transform;token.position=restPositions[index]+dynamicOffsets[index]+visualOffsets[index];
            token.localScale=Vector3.Scale(restScales[index],new Vector3(stretch.x,stretch.y,1));token.rotation=Quaternion.Euler(0,0,visualAngles[index]);
            if(index<tokenShadows.Count) {
                var shadow=tokenShadows[index];float lift=Mathf.Max(0,visualOffsets[index].y);
                shadow.transform.position=shadow.basePosition+dynamicOffsets[index]+Vector3.right*visualOffsets[index].x*.25f;
                shadow.transform.localScale=shadow.baseScale*(1+Mathf.Min(lift,.6f)*.45f);
                shadow.renderer.enabled=piecePoses[index]!=PiecePose.Falling&&lift<.9f;
            }
        }
        void OnDestroy()
        {
            if(continuousRopeMaterial!=null)Destroy(continuousRopeMaterial);if(woodMaterial!=null)Destroy(woodMaterial);if(ropeMaterial!=null)Destroy(ropeMaterial);
            foreach(var mesh in woodMeshes)if(mesh!=null)Destroy(mesh);
        }
    }
}
