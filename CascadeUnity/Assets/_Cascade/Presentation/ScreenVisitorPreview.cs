using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // A preview uses the same authored wing poses, with a fixed torso anchor.
    public sealed class ScreenVisitorPreview:MonoBehaviour
    {
        CascadeGame game;Image picture;RectTransform rect,parent;
        int species,scenery=-1;float clock,poke;bool focused=true;
        public void Initialize(CascadeGame owner,Image image,string id,int row)
        {
            game=owner;picture=image;rect=image.rectTransform;parent=(RectTransform)rect.parent;
            species=row;scenery=CascadeGame.ScreenSceneryFamily(id);rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);Paint();
        }
        public void Poke(){poke=1;}
        void LateUpdate()
        {
            if(game==null||picture==null)return;
            if(focused&&game.CosmeticAmbianceMotionAllowed&&!game.ReducedMotion)clock+=Mathf.Min(Time.unscaledDeltaTime,.08f);
            poke=Mathf.MoveTowards(poke,0,Time.unscaledDeltaTime*2);Paint();
        }
        void Paint()
        {
            if(parent==null)return;
            if(scenery>=0){PaintScenery();return;}
            int frame=game.ReducedMotion?0:Mathf.FloorToInt(Mathf.Repeat(clock*(species==1?2.4f:1.15f),1)*4);
            var sprite=game.VisitorPose(species,frame);if(sprite==null)return;picture.sprite=sprite;
            float scale=Mathf.Min(parent.rect.width*.64f/220,parent.rect.height*.50f/195)*(1+poke*.10f);
            rect.sizeDelta=new Vector2(sprite.rect.width,sprite.rect.height)*scale;
            float phase=game.ReducedMotion?0:clock;
            var body=new Vector2(Mathf.Sin(phase*.55f)*parent.rect.width*.09f,Mathf.Sin(phase*.8f)*parent.rect.height*.035f);
            float bank=game.ReducedMotion?0:Mathf.Sin(phase*.55f)*4;
            rect.localRotation=Quaternion.Euler(0,0,bank);
            rect.anchoredPosition=body+(Vector2)(rect.localRotation*(Vector3)(game.VisitorBodyOffset(species,frame)*scale));
        }
        void PaintScenery()
        {
            int frame=game.ReducedMotion||scenery!=1?0:Mathf.FloorToInt(Mathf.Repeat(clock*.8f,1)*4);
            var sprite=game.ScreenSceneryPose(scenery,frame);if(sprite==null)return;picture.sprite=sprite;
            var maximum=CascadeGame.ScreenSceneryMaximumSize(scenery);
            float scale=Mathf.Min(parent.rect.width*.62f/maximum.x,parent.rect.height*.68f/maximum.y)*(1+poke*.10f);
            rect.sizeDelta=sprite.rect.size*scale;
            float phase=game.ReducedMotion?0:clock;
            float bob=scenery==1?Mathf.Sin(phase*.9f)*.027f:scenery==2?Mathf.Sin(phase*.45f)*.045f:Mathf.Sin(phase*.55f)*.024f;
            var body=new Vector2(Mathf.Sin(phase*.42f)*parent.rect.width*.045f,bob*parent.rect.height);
            float bank=scenery==2?0:Mathf.Sin(phase*.5f)*(scenery==3?3:6);
            rect.localRotation=Quaternion.Euler(0,0,bank);
            float breathe=scenery==2||scenery==3?1+Mathf.Sin(phase*.45f)*.025f:1;
            rect.localScale=new Vector3(breathe,breathe,1);
            rect.anchoredPosition=body+(Vector2)(rect.localRotation*(Vector3)(CascadeGame.ScreenSceneryBodyOffset(scenery,frame)*scale));
        }
        void OnApplicationFocus(bool value){focused=value;}
    }
}
