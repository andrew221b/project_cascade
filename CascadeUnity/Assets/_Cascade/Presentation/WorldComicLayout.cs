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
        UnityEngine.UI.Image sceneImage;
        UnityEngine.UI.ScrollRect scroll;
        TextMeshProUGUI speaker, body, count, nextLabel;
        float previousTextWidth,previousFontSize,previousReadingHeight,textHeight;
        string previousCopy;
        bool initialized,subscribed;

        public void Initialize(CascadeGame owner, RectTransform root, RectTransform heading,
            UnityEngine.UI.Image illustration, UnityEngine.UI.Image bust, RectTransform name,
            TextMeshProUGUI speakerName, TextMeshProUGUI spokenLine, UnityEngine.UI.ScrollRect reader,
            UnityEngine.UI.Button advance, TextMeshProUGUI progress)
        {
            game=owner;menu=root;header=heading;speech=(RectTransform)transform;
            sceneImage=illustration;scene=(RectTransform)illustration.transform.parent;portrait=bust.rectTransform;
            nameRibbon=name;speaker=speakerName;body=spokenLine;scroll=reader;reading=(RectTransform)reader.transform;
            next=(RectTransform)advance.transform;nextLabel=advance.GetComponentInChildren<TextMeshProUGUI>();count=progress;
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
            // Menu siblings are disabled and destroyed one by one. A pre-render callback
            // can run after the old heading/scene is removed but before this ribbon is.
            if(game==null||menu==null||canvas==null||header==null||speech==null||scene==null||portrait==null||nameRibbon==null||reading==null||next==null
                ||sceneImage==null||scroll==null||speaker==null||body==null||count==null||nextLabel==null
                ||!menu.gameObject.activeInHierarchy||!header.gameObject.activeInHierarchy||!scene.gameObject.activeInHierarchy||!speech.gameObject.activeInHierarchy){Unsubscribe();return;}
            float unit=1/Mathf.Min(1,canvas.scaleFactor),width=menu.rect.width,height=menu.rect.height;
            var viewport=(RectTransform)canvas.transform;
            float panelWidth=Mathf.Min(width*.94f,viewport.rect.width*.882f),textWidth=Mathf.Max(unit,panelWidth-32*unit);
            float fontSize=(game.Preferences.LargeText?22:20)*unit;
            body.enableAutoSizing=false;body.fontSize=fontSize;body.margin=Vector4.zero;
            bool changedCopy=previousTextWidth!=textWidth||previousFontSize!=fontSize||previousCopy!=body.text;
            if(changedCopy)textHeight=Mathf.Ceil(body.GetPreferredValues(body.text,textWidth,Mathf.Infinity).y)+2*unit;
            float ratio=sceneImage.sprite==null?1.5f:sceneImage.sprite.rect.width/sceneImage.sprite.rect.height;
            // The portrait workshop's timber opening runs from 5.9% to 94.1% of the screen.
            // A short landscape viewport contains the complete panel above the safe controls.
            float sceneWidth=Mathf.Min(viewport.rect.width*.882f,Mathf.Max(24*unit,height-288*unit)*ratio);
            float sceneHeight=sceneWidth/ratio;
            float maximumSpeechHeight=Mathf.Max(120*unit,height-168*unit-sceneHeight);
            float speechHeight=Mathf.Min(textHeight+84*unit,maximumSpeechHeight);
            float groupHeight=sceneHeight+speechHeight+98*unit;
            float groupBottom=8*unit+Mathf.Max(0,height-70*unit-groupHeight)*.5f;
            float speechBottom=groupBottom+86*unit;
            float screenCentre=menu.InverseTransformPoint(viewport.TransformPoint(viewport.rect.center)).x-menu.rect.xMin;
            Place(header,width*.5f,height-28*unit,width*.88f,48*unit);
            Place(scene,screenCentre,speechBottom+speechHeight+12*unit+sceneHeight*.5f,sceneWidth,sceneHeight);
            Place(speech,screenCentre,speechBottom+speechHeight*.5f,panelWidth,speechHeight);
            float portraitWidth=Mathf.Min(76*unit,panelWidth*.29f);
            Place(portrait,portraitWidth*.5f+8*unit,speechHeight-36*unit,portraitWidth,68*unit);
            float nameLeft=portraitWidth+14*unit,nameWidth=Mathf.Max(36*unit,panelWidth-nameLeft-12*unit);
            Place(nameRibbon,nameLeft+nameWidth*.5f,speechHeight-34*unit,nameWidth,38*unit);
            speaker.enableAutoSizing=true;speaker.fontSizeMin=16*unit;speaker.fontSizeMax=20*unit;speaker.textWrappingMode=TextWrappingModes.NoWrap;
            float readingHeight=Mathf.Max(28*unit,speechHeight-84*unit);
            Place(reading,panelWidth*.5f,12*unit+readingHeight*.5f,textWidth,readingHeight);
            var bodySize=new Vector2(0,textHeight);if(body.rectTransform.sizeDelta!=bodySize)body.rectTransform.sizeDelta=bodySize;
            scroll.vertical=textHeight>readingHeight+.5f*unit;
            if(changedCopy||previousReadingHeight!=readingHeight)scroll.verticalNormalizedPosition=1;
            Place(next,screenCentre,groupBottom+50*unit,panelWidth,48*unit);
            nextLabel.enableAutoSizing=true;nextLabel.fontSizeMin=16*unit;nextLabel.fontSizeMax=20*unit;
            Place(count.rectTransform,screenCentre,groupBottom+11*unit,width*.5f,22*unit);count.fontSize=15*unit;
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
