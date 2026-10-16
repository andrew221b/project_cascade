using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    // The panel keeps its authored aspect; reading and touch controls stay inside the safe UI.
    public sealed class WorldComicLayout : MonoBehaviour
    {
        CascadeGame game;
        Canvas canvas;
        RectTransform menu, header, speech, scene, portrait, nameRibbon, reading, next;
        RectTransform[] woodRails;
        UnityEngine.UI.Image sceneImage;
        UnityEngine.UI.ScrollRect scroll;
        TextMeshProUGUI speaker, body, count, nextLabel;
        float previousTextWidth,previousFontSize,previousReadingHeight,textHeight;
        string previousCopy;
        string[] pageLines;
        bool initialized,subscribed;

        public void Initialize(CascadeGame owner, RectTransform root, RectTransform heading,
            UnityEngine.UI.Image illustration, RectTransform[] rails, UnityEngine.UI.Image bust, RectTransform name,
            TextMeshProUGUI speakerName, TextMeshProUGUI spokenLine, UnityEngine.UI.ScrollRect reader,
            UnityEngine.UI.Button advance, TextMeshProUGUI progress,string[] localizedPageLines)
        {
            game=owner;menu=root;header=heading;speech=(RectTransform)transform;
            sceneImage=illustration;scene=(RectTransform)illustration.transform.parent;woodRails=rails;portrait=bust.rectTransform;
            nameRibbon=name;speaker=speakerName;body=spokenLine;scroll=reader;reading=(RectTransform)reader.transform;
            next=(RectTransform)advance.transform;nextLabel=advance.GetComponentInChildren<TextMeshProUGUI>();count=progress;
            pageLines=localizedPageLines;
            canvas=root.GetComponentInParent<Canvas>();
            // This component owns the measured text height, including its breathing room.
            var fitter=body.GetComponent<UnityEngine.UI.ContentSizeFitter>();if(fitter!=null)fitter.enabled=false;
            initialized=true;Subscribe();
            RefreshLayout();
        }

        void OnEnable(){if(initialized)Subscribe();}
        void OnDisable()=>Unsubscribe();
        void OnDestroy()=>Unsubscribe();
        void Subscribe()
        {
            if(subscribed||!isActiveAndEnabled)return;
            Canvas.willRenderCanvases+=RefreshLayout;subscribed=true;
        }
        void Unsubscribe()
        {
            if(!subscribed)return;
            Canvas.willRenderCanvases-=RefreshLayout;subscribed=false;
        }

        public void RefreshLayout()
        {
            if(!initialized||this==null||!isActiveAndEnabled)return;
            // Starting a puzzle can rebuild its Canvas before the old comic is destroyed.
            if(game==null||game.CurrentScreen!=GameScreen.Prologue||game.ComicWorld<0){Unsubscribe();return;}
            // Menu siblings are disabled and destroyed one by one. A pre-render callback
            // can run after the old heading/scene is removed but before this ribbon is.
            if(game==null||menu==null||canvas==null||header==null||speech==null||scene==null||portrait==null||nameRibbon==null||reading==null||next==null
                ||sceneImage==null||scroll==null||speaker==null||body==null||count==null||nextLabel==null
                ||!menu.gameObject.activeInHierarchy||!header.gameObject.activeInHierarchy||!scene.gameObject.activeInHierarchy||!speech.gameObject.activeInHierarchy){Unsubscribe();return;}
            float unit=1/Mathf.Min(1,canvas.scaleFactor),width=menu.rect.width,height=menu.rect.height;
            var viewport=(RectTransform)canvas.transform;
            float panelWidth=Mathf.Min(width+16*unit,viewport.rect.width*.882f);
            float portraitWidth=64*unit;
            float textWidth=Mathf.Max(unit,panelWidth-48*unit);
            float fontSize=(game.Preferences.LargeText?20:18)*unit;
            body.enableAutoSizing=false;body.fontSize=fontSize;body.margin=Vector4.zero;
            float ratio=sceneImage.sprite==null?1.5f:sceneImage.sprite.rect.width/sceneImage.sprite.rect.height;
            float longest=0;
            foreach(string line in pageLines)longest=Mathf.Max(longest,body.GetPreferredValues(line,textWidth,Mathf.Infinity).y);
            float desiredSpeechHeight=Mathf.Max(218*unit,Mathf.Ceil(longest)+136*unit);
            float available=Mathf.Max(300*unit,height-118*unit);
            float sceneHeight=Mathf.Min(viewport.rect.width*.882f/ratio,Mathf.Max(100*unit,available-desiredSpeechHeight-14*unit));
            float sceneWidth=sceneHeight*ratio;
            float maximumSpeechHeight=Mathf.Min(desiredSpeechHeight,available-sceneHeight-14*unit);
            string fitted=game.FitComicCopy(textWidth,Mathf.Max(24*unit,maximumSpeechHeight-128*unit),body);
            if(body.text!=fitted)body.SetCopy(UiCopy.Raw(fitted));
            var advanceCopy=game.ComicAdvanceCaption;if(nextLabel.text!=advanceCopy.ToString())nextLabel.SetCopy(advanceCopy);
            bool changedCopy=previousTextWidth!=textWidth||previousFontSize!=fontSize||previousCopy!=body.text;
            if(changedCopy){
                textHeight=body.GetPreferredValues(body.text,textWidth,Mathf.Infinity).y;
                textHeight=Mathf.Ceil(textHeight)+2*unit;
            }
            // Reserve the name row, the 44pt action and the gaps before measuring the line.
            // Reserve the same dialogue band for every speaker on this panel.
            // Shorter lines must not move the illustration up and down.
            float speechHeight=maximumSpeechHeight;
            float groupHeight=sceneHeight+speechHeight+14*unit;
            float groupBottom=12*unit+Mathf.Max(0,height-98*unit-groupHeight)*.5f;
            float speechBottom=groupBottom;
            float screenCentre=menu.InverseTransformPoint(viewport.TransformPoint(viewport.rect.center)).x-menu.rect.xMin;
            Place(header,width*.5f,height-44*unit,width*.88f,80*unit);
            Place(scene,screenCentre,speechBottom+speechHeight+14*unit+sceneHeight*.5f,sceneWidth,sceneHeight);
            if(woodRails!=null&&woodRails.Length==4){
                Place(woodRails[0],sceneWidth*.5f,sceneHeight-5*unit,sceneWidth+10*unit,14*unit);
                Place(woodRails[1],sceneWidth*.5f,5*unit,sceneWidth+10*unit,14*unit);
                Place(woodRails[2],5*unit,sceneHeight*.5f,sceneHeight+8*unit,14*unit);
                Place(woodRails[3],sceneWidth-5*unit,sceneHeight*.5f,sceneHeight+8*unit,14*unit);
                woodRails[2].localEulerAngles=new Vector3(0,0,90);
                woodRails[3].localEulerAngles=new Vector3(0,0,90);
            }
            Place(speech,screenCentre,speechBottom+speechHeight*.5f,panelWidth,speechHeight);
            var paper=speech.GetComponent<UnityEngine.UI.Image>();paper.pixelsPerUnitMultiplier=8/unit;
            Place(portrait,portraitWidth*.5f+16*unit,speechHeight-40*unit,portraitWidth,64*unit);
            float nameLeft=portraitWidth+26*unit,nameWidth=Mathf.Max(36*unit,panelWidth-nameLeft-70*unit);
            Place(nameRibbon,nameLeft+nameWidth*.5f,speechHeight-31*unit,nameWidth,34*unit);
            speaker.enableAutoSizing=true;speaker.fontSizeMin=14*unit;speaker.fontSizeMax=19*unit;speaker.textWrappingMode=TextWrappingModes.NoWrap;
            speaker.overflowMode=TextOverflowModes.Truncate;
            float readingHeight=Mathf.Max(35*unit,speechHeight-128*unit);
            Place(reading,panelWidth*.5f,speechHeight-74*unit-readingHeight*.5f,textWidth,readingHeight);
            var bodySize=new Vector2(0,textHeight);if(body.rectTransform.sizeDelta!=bodySize)body.rectTransform.sizeDelta=bodySize;
            scroll.vertical=textHeight>readingHeight+.5f*unit;
            if(changedCopy||previousReadingHeight!=readingHeight)scroll.verticalNormalizedPosition=1;
            game.ApplyControlCaption(nextLabel,unit);
            nextLabel.textWrappingMode=TextWrappingModes.NoWrap;
            nextLabel.fontSize=nextLabel.fontSizeMin=nextLabel.fontSizeMax=(game.Preferences.LargeText?20:18)*unit;
            nextLabel.margin=new Vector4(16,6,16,6)*unit;
            float actionWidth=Mathf.Clamp(nextLabel.GetPreferredValues(nextLabel.text,Mathf.Infinity,Mathf.Infinity).x+32*unit,110*unit,panelWidth-60*unit);
            Place(next,panelWidth-30*unit-actionWidth*.5f,30*unit,actionWidth,48*unit);
            next.GetComponent<CenteredContentRow>()?.Refresh();
            Place(count.rectTransform,panelWidth-35*unit,speechHeight-21*unit,60*unit,20*unit);count.fontSize=13*unit;
            previousTextWidth=textWidth;previousFontSize=fontSize;previousReadingHeight=readingHeight;previousCopy=body.text;
        }

        static void Place(RectTransform rect,float x,float y,float width,float height)
        {
            if(rect==null)return;
            if(rect.anchorMin!=Vector2.zero)rect.anchorMin=Vector2.zero;if(rect.anchorMax!=Vector2.zero)rect.anchorMax=Vector2.zero;
            var pivot=new Vector2(.5f,.5f);if(rect.pivot!=pivot)rect.pivot=pivot;
            Vector2 position=new Vector2(x,y),size=new Vector2(width,height);
            if(rect.anchoredPosition!=position)rect.anchoredPosition=position;if(rect.sizeDelta!=size)rect.sizeDelta=size;
        }
    }
}
