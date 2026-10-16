using UnityEngine;
namespace Cascade.Presentation
{
    // Existing themed cutouts grow from a measured support. Only the stem bends;
    // no scenery or flower is positioned independently in the open sky.
    public sealed class AttachedPlants:MonoBehaviour
    {
        CascadeGame game;RectTransform support;WorldFrameFit fit;
        UnityEngine.UI.Image[] flowers;float[] angle,velocity;bool heading,gameplay;int world;float age=16;
        public float AnimationAge=>age;
        public void Initialize(CascadeGame owner,int biome,bool onHeading,bool inGameplay=false)
        {
            game=owner;world=biome;heading=onHeading;gameplay=inGameplay;support=(RectTransform)transform;fit=GetComponent<WorldFrameFit>();
            int count=heading?2:4;flowers=new UnityEngine.UI.Image[count];angle=new float[count];velocity=new float[count];
            for(int i=0;i<count;i++){
                var go=new GameObject("Rooted decorative sprig "+i,typeof(RectTransform),typeof(UnityEngine.UI.Image));go.transform.SetParent(transform,false);
                var image=go.GetComponent<UnityEngine.UI.Image>();image.sprite=FailureReferenceArt.Sprig(world);image.preserveAspect=true;image.raycastTarget=false;flowers[i]=image;
                var rect=image.rectTransform;rect.anchorMin=rect.anchorMax=Vector2.one*.5f;rect.pivot=new Vector2(.5f,.12f);
            }
            Pose(0,false);
        }
        public void SetWorld(int biome)
        {
            if(world==biome)return;world=biome;foreach(var flower in flowers)flower.sprite=FailureReferenceArt.Sprig(world);
        }
        void LateUpdate()
        {
            if(game==null||flowers==null)return;
            bool moving=game.CelebrationMotionAllowed&&!game.ReducedMotion&&game.Preferences.EffectQuality>0
                &&(!gameplay||game.CurrentScreen==GameScreen.Playing&&!game.Interaction.IsPaused&&!game.Interaction.IsSuspended);
            float dt=moving?Mathf.Min(Time.unscaledDeltaTime,.05f):0;if(moving)age+=dt;Pose(dt,moving);
        }
        void Pose(float dt,bool moving)
        {
            if(support==null)return;float unit=CenteredContentRow.Unit(support);var light=MenuWorldArt.Light(world,age,moving);
            for(int i=0;i<flowers.Length;i++){
                bool right=i%2==1;var rect=flowers[i].rectTransform;Vector2 point;
                if(heading)point=new Vector2(right?.90f:.11f,.79f);
                else if(fit!=null)point=fit.ProjectSourcePoint(i<2?(right?new Vector2(.986f,.967f):new Vector2(.032f,.974f)):(right?new Vector2(.957f,.029f):new Vector2(.028f,.034f)));
                else point=WorldFrameArt.FrameContact(right,i<2?0:.75f,support.rect.width/Mathf.Max(1,support.rect.height));
                rect.anchoredPosition=Vector2.Scale(point-Vector2.one*.5f,support.rect.size);
                float size=(heading?18:22)*unit;rect.sizeDelta=Vector2.one*size;rect.localScale=new Vector3(right?-1:1,1,1);
                float target=moving?(Mathf.Sin(age*.83f+i*1.7f)*7+Mathf.Sin(age*1.39f+i*.7f)*2)*(world==6?.4f:1):0;
                angle[i]=moving?Mathf.SmoothDamp(angle[i],target,ref velocity[i],.24f,Mathf.Infinity,dt):0;
                rect.localRotation=Quaternion.Euler(0,0,angle[i]);flowers[i].color=light;flowers[i].enabled=game.Preferences.EffectQuality>0&&(game.Preferences.EffectQuality>1||i<2);
            }
        }
    }
}
