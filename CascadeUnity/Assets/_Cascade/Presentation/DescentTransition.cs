using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed class DescentGhost:MonoBehaviour
    {
        public readonly List<Mesh> OwnedMeshes=new List<Mesh>();
        public readonly List<Material> OwnedMaterials=new List<Material>();
        void OnDestroy(){foreach(var mesh in OwnedMeshes)if(mesh!=null)Destroy(mesh);foreach(var material in OwnedMaterials)if(material!=null)Destroy(material);}
    }
    // Short-lived snapshots explain descent. No commands, checkpoint state or rewards are written here.
    public sealed class DescentTransition:MonoBehaviour
    {
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
            game.ShowDescentRig(false);Pose(0);
        }
        void Update()
        {
            if(!Playing)return;
            if(game==null||game.ReducedMotion||!game.DescentMotionAllowed){Settle();return;}
#if UNITY_EDITOR
            if(preview>=0){Progress=Mathf.Clamp01(preview);Pose(Progress);return;}
#endif
            elapsed+=Time.unscaledDeltaTime;Progress=Mathf.Clamp01(elapsed/.65f);
            Pose(Progress);if(Progress>=1)Settle();
        }
        void Pose(float t)
        {
            float eased=t*t*(3-2*t);
            departing.transform.position=Vector3.up*height*eased;
            arriving.transform.position=Vector3.down*height*(1-eased);
            game.Machine.LivingWorld.DescentDepth=game.Descent.StageIndex-1+eased;
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
