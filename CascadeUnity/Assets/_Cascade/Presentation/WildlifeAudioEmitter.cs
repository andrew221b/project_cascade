using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public enum WildlifeCall { Silent, Songbird, Seabird, Owl, Buzz, Cricket }

    public static class WildlifeAcoustics
    {
        public static WildlifeCall CallFor(WildlifeSpecies species)
        {
            switch(species){
                case WildlifeSpecies.Gull:case WildlifeSpecies.Tern:return WildlifeCall.Seabird;
                case WildlifeSpecies.BarnOwl:return WildlifeCall.Owl;
                case WildlifeSpecies.Bumblebee:return WildlifeCall.Buzz;
                case WildlifeSpecies.FieldCricket:return WildlifeCall.Cricket;
                default:return WildlifeMotion.IsBird(species)?WildlifeCall.Songbird:WildlifeCall.Silent;
            }
        }
        public static float Pan(float viewportX)=>Mathf.Clamp((viewportX-.5f)*1.4f,-.7f,.7f);
        public static float VoicePitch(WildlifeSpecies species,int identity)
        {
            float register=species==WildlifeSpecies.Bullfinch?.9f:species==WildlifeSpecies.Swallow?1.10f:
                species==WildlifeSpecies.SnowBunting?1.05f:species==WildlifeSpecies.Kingfisher?.95f:1;
            return register*(.976f+WildlifeMotion.Hash(identity+317)*.048f);
        }
        public static float Gain(float projectedWidth,float alpha,float viewportY)
            =>Mathf.Lerp(.025f,.115f,Mathf.Clamp01((projectedWidth-.012f)/.065f))*Mathf.Clamp01(alpha)*Mathf.Lerp(1,.78f,Mathf.Clamp01(viewportY));
    }

    // The sound owner reads the painted actor; this component owns no animation or puzzle clock.
    public sealed class WildlifeAudioEmitter:MonoBehaviour
    {
        CascadeGame game;Camera cameraView;WildlifeRig rig;Image picture,alternatePicture;WildlifeSpecies species;
        ToyFeedbackPool registered;
        internal float VisibleSince=-1,NextCallAt;
        internal int PhraseIndex;
        internal AudioClip LastPhrase;
        Vector2 lastPosition;bool hasPosition;
        public float FlightSpeed{get;private set;}
        public bool Resting{get;set;}
        public bool IsResting=>rig!=null?rig.Resting:Resting;
        public WildlifeSpecies Species=>rig!=null?rig.Species:species;
        public WildlifeCall Call=>WildlifeAcoustics.CallFor(Species);
        public void Bind(CascadeGame owner,Image image,WildlifeSpecies kind,Image alternate=null)
        {
            game=owner;picture=image;alternatePicture=alternate;species=kind;cameraView=image.GetComponentInParent<Canvas>()?.worldCamera??Camera.main;Register();
        }
        internal void Bind(CascadeGame owner,WildlifeRig creature,Camera camera)
        {
            game=owner;rig=creature;cameraView=camera;Register();
        }
        public void SetSpecies(WildlifeSpecies kind)=>species=kind;
        internal void ObserveMotion(Vector2 position,float seconds)
        {
            float speed=hasPosition&&seconds>0?Vector2.Distance(position,lastPosition)/seconds:0;
            FlightSpeed=Mathf.Lerp(FlightSpeed,Mathf.Min(.5f,speed),1-Mathf.Exp(-seconds/.22f));
            lastPosition=position;hasPosition=true;
        }
        void Register()
        {
            if(registered!=null||!isActiveAndEnabled||game==null||game.Feedback==null)return;
            registered=game.Feedback;registered.RegisterWildlife(this);
        }
        // Gameplay rigs are constructed before the feedback pool in CascadeGame.Awake.
        void Update(){if(registered==null)Register();}
        void OnEnable()=>Register();
        void OnDisable(){if(registered!=null)registered.UnregisterWildlife(this);registered=null;VisibleSince=-1;hasPosition=false;FlightSpeed=0;}
        public bool TrySample(out Vector2 viewport,out float gain)
        {
            viewport=default;gain=0;
            if(!isActiveAndEnabled||cameraView==null||Call==WildlifeCall.Silent)return false;
            Vector3 point;float alpha,width;
            if(rig!=null){
                if(!rig.Visible||!rig.Root.gameObject.activeInHierarchy)return false;
                point=rig.Root.position;alpha=rig.Opacity;width=Mathf.Abs(rig.Root.lossyScale.x);
            }else{
                var drawing=alternatePicture!=null&&alternatePicture.enabled&&alternatePicture.color.a>(picture==null?0:picture.color.a)?alternatePicture:picture;
                if(drawing==null||!drawing.enabled||!drawing.gameObject.activeInHierarchy)return false;
                point=drawing.rectTransform.TransformPoint(drawing.rectTransform.rect.center);alpha=drawing.color.a;
                width=drawing.rectTransform.rect.width*Mathf.Abs(drawing.rectTransform.lossyScale.x);
            }
            var projected=cameraView.WorldToViewportPoint(point);viewport=projected;
            if(projected.z<=0||projected.x<0||projected.x>1||projected.y<0||projected.y>1||alpha<.12f)return false;
            bool resting=rig!=null?rig.Resting:Resting;
            if(Call==WildlifeCall.Buzz&&resting)return false;
            if(Call==WildlifeCall.Cricket&&!resting)return false;
            float extent=Mathf.Abs(cameraView.WorldToViewportPoint(point+cameraView.transform.right*width).x-projected.x);
            gain=WildlifeAcoustics.Gain(extent,alpha,projected.y);return gain>.003f;
        }
    }
}
