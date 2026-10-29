using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed class DescentGhost:MonoBehaviour
    {
        public readonly List<Mesh> OwnedMeshes=new List<Mesh>();
        public readonly List<Material> OwnedMaterials=new List<Material>();
        readonly Dictionary<Material,Material> materials=new Dictionary<Material,Material>();
        readonly List<Vector3> ropeStarts=new List<Vector3>(2),ropeEnds=new List<Vector3>(2);
        Material ropeMaterial;float ropeWidth;
        internal int UiLayer;
        public void Capture(Renderer source,Camera camera)
        {
            if(!source.enabled||!source.gameObject.activeInHierarchy)return;
            Mesh mesh=null;Renderer copy;
            var part=new GameObject(source.name);part.transform.SetParent(transform,false);
            part.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);part.transform.localScale=source.transform.lossyScale;
            if(source is SpriteRenderer sprite){
                var image=part.AddComponent<SpriteRenderer>();image.sprite=sprite.sprite;image.drawMode=sprite.drawMode;image.size=sprite.size;
                image.color=sprite.color;image.flipX=sprite.flipX;image.flipY=sprite.flipY;image.tileMode=sprite.tileMode;copy=image;
            } else {
                copy=part.AddComponent<MeshRenderer>();
                if(source is LineRenderer line){
                    if(line.positionCount<2){Destroy(part);return;}
                    mesh=new Mesh{name=source.name+" descent snapshot"};line.BakeMesh(mesh,camera,false);
                    ropeStarts.Add(line.useWorldSpace?line.GetPosition(0):line.transform.TransformPoint(line.GetPosition(0)));
                    ropeEnds.Add(line.useWorldSpace?line.GetPosition(line.positionCount-1):line.transform.TransformPoint(line.GetPosition(line.positionCount-1)));
                    ropeWidth=line.startWidth*line.widthMultiplier;
                } else {
                    var filter=source.GetComponent<MeshFilter>();if(filter==null||filter.sharedMesh==null){Destroy(part);return;}
                    mesh=Instantiate(filter.sharedMesh);
                }
                OwnedMeshes.Add(mesh);part.AddComponent<MeshFilter>().sharedMesh=mesh;
            }
            var copied=source.sharedMaterials;
            for(int i=0;i<copied.Length;i++){
                var original=copied[i];if(original==null)continue;
                if(!materials.TryGetValue(original,out var owned)){owned=new Material(original);materials.Add(original,owned);OwnedMaterials.Add(owned);}
                copied[i]=owned;
            }
            copy.sharedMaterials=copied;
            var properties=new MaterialPropertyBlock();source.GetPropertyBlock(properties);copy.SetPropertyBlock(properties);
            for(int i=0;i<copied.Length;i++){properties.Clear();source.GetPropertyBlock(properties,i);if(!properties.isEmpty)copy.SetPropertyBlock(properties,i);}
            copy.shadowCastingMode=source.shadowCastingMode;copy.receiveShadows=source.receiveShadows;
            copy.sortingLayerID=source.sortingLayerID;copy.sortingOrder=source.sortingOrder;
            if(source is LineRenderer&&copied.Length>0)ropeMaterial=copied[0];
        }
        public void ConnectRopes(DescentGhost next,float distance)
        {
            // Continue the same two ropes between snapshots, outside both rigs.
            // The bridge leaves the viewport before the live rig is revealed.
            for(int side=0;side<Mathf.Min(ropeEnds.Count,next.ropeStarts.Count);side++){
                var start=ropeEnds[side];var end=next.ropeStarts[side]-Vector3.up*distance;
                if(start.y<=end.y||ropeMaterial==null)continue;
                var line=new GameObject(side==0?"Left continuous descent span":"Right continuous descent span",typeof(LineRenderer)).GetComponent<LineRenderer>();
                line.transform.SetParent(transform,false);line.useWorldSpace=false;line.sharedMaterial=ropeMaterial;
                line.startWidth=line.endWidth=ropeWidth;line.numCapVertices=8;line.numCornerVertices=8;
                line.textureMode=LineTextureMode.Tile;line.alignment=LineAlignment.View;line.generateLightingData=true;line.positionCount=17;
                for(int i=0;i<17;i++){float t=i/16f;line.SetPosition(i,Vector3.Lerp(start,end,t)+Vector3.right*(Mathf.Sin(t*Mathf.PI)*.025f*(side==0?1:-1)));}
            }
        }
        void OnDestroy(){foreach(var mesh in OwnedMeshes)if(mesh!=null)Destroy(mesh);foreach(var material in OwnedMaterials)if(material!=null)Destroy(material);}
    }
    // Camera-like travel over an already committed checkpoint. Never writes puzzle/save state.
    public sealed class DescentTransition:MonoBehaviour
    {
        public const float Duration=1.18f;
        CascadeGame game;
        DescentGhost departing,arriving;
        float elapsed,height;
        public bool Playing {get;private set;}
        public float Progress {get;private set;}
#if UNITY_EDITOR
        float preview=-1;
        public void SetPreview(float value){preview=value;}
#endif
        public void Begin(CascadeGame owner,DescentGhost oldRig,DescentGhost newRig)
        {
            Settle();game=owner;departing=oldRig;arriving=newRig;elapsed=Progress=0;
            height=Camera.main.orthographicSize*2;Playing=true;
            if(game.ReducedMotion){Settle();return;}
            departing.ConnectRopes(arriving,height);
            game.ShowDescentRig(false);Pose(0);
        }
        void Update()
        {
            if(!Playing)return;
            if(game==null||game.ReducedMotion||!game.DescentMotionAllowed){Settle();return;}
#if UNITY_EDITOR
            if(preview>=0){Progress=Mathf.Clamp01(preview);Pose(Progress);return;}
#endif
            elapsed+=Time.unscaledDeltaTime;Progress=Mathf.Clamp01(elapsed/Duration);
            Pose(Progress);if(Progress>=1){Settle();game.RevealArrivalWind();}
        }
        void Pose(float t)
        {
            // Minimum-jerk travel: zero velocity and acceleration at both joins.
            float eased=t*t*t*(t*(t*6-15)+10);
            departing.transform.position=Vector3.up*height*eased;
            arriving.transform.position=Vector3.down*height*(1-eased);
            game.Machine.LivingWorld.DescentDepth=game.Descent.StageIndex-1+eased;
            game.PaintDescentHud(Mathf.SmoothStep(0,1,Mathf.InverseLerp(.72f,1,t)));
        }
        public void Settle()
        {
            if(departing!=null){departing.gameObject.SetActive(false);Destroy(departing.gameObject);}
            if(arriving!=null){arriving.gameObject.SetActive(false);Destroy(arriving.gameObject);}
            departing=arriving=null;bool wasPlaying=Playing;Playing=false;Progress=1;
#if UNITY_EDITOR
            preview=-1;
#endif
            if(wasPlaying&&game!=null)game.ShowDescentRig(true);
        }
        void OnDisable(){Settle();}
    }
}
