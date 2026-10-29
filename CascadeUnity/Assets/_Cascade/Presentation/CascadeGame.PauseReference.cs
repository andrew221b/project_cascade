using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        const string PauseArtPath="CascadeArt/PauseReferenceOct14/";
        Sprite PauseArt(string name)=>CleanMenuArt.Get(name)??Resources.Load<Sprite>(PauseArtPath+name);
        void DrawReferencePause()
        {
            var backdrop=PauseArt("Backdrop");
            var stage=ReferenceShopSurface(backdrop);
            // Mika, her bag and lantern share one uninterrupted stone terrace.
            stage.GetComponent<ReferenceShopLayout>().BindContinuousGround(925,"ContinuousStoneGround");
            var corners=new[]{new Vector2(64,105),new Vector2(564,105)};
            for(int i=0;i<2;i++){
                var button=ReferenceButton(i==0?"Home":"Settings",stage,corners[i].x,corners[i].y,96,92,i==0?(System.Action)ShowHome:ShowSettings);
                var body=ReferenceButtonBody(button);
                ReferencePicture("Pause square face "+i,body,CleanMenuArt.Get("SquareButton"),0,0,96,92);
                var symbol=ReferenceRect("Pause corner icon "+i,body,14,10,66,66).gameObject.AddComponent<PauseGlyphGraphic>();symbol.Configure(i);CentreContent((RectTransform)body,symbol.rectTransform);
            }
            var titleBoard=ReferencePicture("Pause wooden title board",stage,ReferenceShopArt("HeaderBoard"),103,198,529,155,false);titleBoard.type=Image.Type.Sliced;titleBoard.pixelsPerUnitMultiplier=4;
            var title=ReferenceText("pause heading",stage,ReferenceCopy("Take a break?","Передохнём?","Перепочинемо?"),179,238,377,75,58);
            CentreHeading(titleBoard,title);title.characterSpacing=-1;title.color=new Color(.19f,.055f,.014f);
            var halo=ReferenceRect("Pause lantern warm halo",stage,536,771,169,221).gameObject.AddComponent<PauseLanternLight>();halo.Configure(true);
            var lantern=ReferencePicture("Pause brass lantern",stage,PauseArt("Lantern"),554,718,131,272);
            var flame=ReferenceRect("Pause lantern flame shimmer",stage,582,826,69,95).gameObject.AddComponent<PauseLanternLight>();flame.Configure(false);
            // The apple pose is a separate fully opaque cutout; no sky tint or screen filter.
            var resting=ReferencePicture("Pause resting Mika and bag",stage,PauseArt("RestingMika"),4,407,635,635);
            CharacterPoseView.Attach(this,resting,animateAppleBite:false);
            var flowers=new List<RectTransform>();
            flowers.Add(PauseFlower(stage,"left garden",0,930,94,102,false));
            flowers.Add(PauseFlower(stage,"left daisies",30,945,72,88,true));
            flowers.Add(PauseFlower(stage,"right tall daisies",661,810,70,135,true));
            flowers.Add(PauseFlower(stage,"right garden",651,923,70,105,false));
            flowers.Add(PauseHeaderDaisy(stage,152,224,14));
            var retry=ReferenceButton("Retry",stage,120,1138,480,104,RetryFromPause);
            var amber=ReferencePicture("Pause amber retry face",ReferenceButtonBody(retry),ParcelArt("AmberAction"),-8,0,305,124,false);
            amber.gameObject.AddComponent<PrimaryButtonSkin>().Configure(ParcelArt("AmberAction"));
            var retryIcon=ReferenceRect("Pause retry icon",ReferenceButtonBody(retry),43,25,61,67).gameObject.AddComponent<PauseGlyphGraphic>();retryIcon.Configure(2);
            var retryCaption=ReferenceText("pause retry",ReferenceButtonBody(retry),ReferenceCopy("Restart","Заново","Заново"),115,25,143,67,34);retryCaption.color=new Color(.19f,.055f,.014f);CentreContent((RectTransform)ReferenceButtonBody(retry),retryIcon.rectTransform,retryCaption.rectTransform);
            RegisterButtonDesign(retry,GameButtonRole.SecondaryYellow);
            retry.interactable=Interaction.AllowsRetry&&!saveBlocked&&!HasSaveError;
            var resume=ReferenceButton("Resume",stage,120,1018,480,104,ShowGame,"GreenAction");
            var green=ReferenceButtonBody(resume).GetComponentInChildren<Image>();green.rectTransform.sizeDelta=new Vector2(334,124);green.rectTransform.anchoredPosition=new Vector2(-13,0);
            var play=ReferenceRect("Pause resume icon",ReferenceButtonBody(resume),38,27,54,64).gameObject.AddComponent<PauseGlyphGraphic>();play.Configure(3);
            var caption=ReferenceText("pause resume",ReferenceButtonBody(resume),ReferenceCopy("Continue","Продолжить","Продовжити"),87,26,210,65,32,true);caption.characterSpacing=-1;CentreContent((RectTransform)ReferenceButtonBody(resume),play.rectTransform,caption.rectTransform);
            var view=stage.gameObject.AddComponent<PauseReferenceAmbiance>();view.Initialize(this,halo,flame,flowers.ToArray());
        }
        RectTransform PauseFlower(RectTransform stage,string key,float x,float y,float width,float height,bool flip)
        {
            var image=ReferencePicture("Pause daisies "+key,stage,PauseArt("Daisies"),x,y,width,height);
            var rect=image.rectTransform;rect.pivot=new Vector2(.5f,0);rect.anchoredPosition=new Vector2(x+width*.5f,-y-height);
            if(flip)rect.localScale=new Vector3(-1,1,1);return rect;
        }
        void PauseHeaderLeaf(RectTransform stage,float x,float y,float width,float height,float angle)
        {
            var image=ReferencePicture("Pause header attached leaf",stage,ReferenceShopArt("MenuGreenLeaf"),x,y,width,height);
            var rect=image.rectTransform;rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(x+width*.5f,-y-height*.5f);rect.localRotation=Quaternion.Euler(0,0,angle);
        }
        RectTransform PauseHeaderDaisy(RectTransform stage,float x,float y,float size)
        {
            var rect=ReferenceRect("Pause header daisy",stage,x,y,size,size);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(x+size*.5f,-y-size*.5f);
            rect.gameObject.AddComponent<PauseGlyphGraphic>().Configure(4);return rect;
        }
        void RetryFromPause()
        {
            if(CurrentScreen!=GameScreen.Pause||!Interaction.AllowsRetry||saveBlocked||HasSaveError)return;
            SetScreen(GameScreen.Playing);Retry();
        }
    }
}
