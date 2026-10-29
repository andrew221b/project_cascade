using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // The 720x1280 composition scales uniformly through ReferenceShopLayout.
    // Longer authored turns grow the paper; the whole illustration fits above it.
    public sealed class WorldComicLayout:MonoBehaviour
    {
        CascadeGame game;RectTransform stage,scene,timber,portrait,nameRibbon,speech,reading,next;
        Image sceneImage,paper;ScrollRect scroll;TextMeshProUGUI speaker,body,count,nextLabel;TextMeshProUGUI[] darkLabels;
        CenteredContentRow nextGroup,nameGroup;
        string[] pageLines;string previousCopy;float previousFont,previousWidth,previousRoom,longest,textHeight;
        bool initialized,subscribed;
        static readonly Color Ink=new Color(.24f,.095f,.045f);
        public void Initialize(CascadeGame owner,RectTransform composition,Image illustration,Image rim,Image bust,RectTransform name,
            TextMeshProUGUI heading,TextMeshProUGUI speakerName,TextMeshProUGUI spokenLine,ScrollRect reader,Button advance,TextMeshProUGUI advanceCopy,
            TextMeshProUGUI progress,string[] localizedPageLines)
        {
            game=owner;stage=composition;sceneImage=illustration;scene=(RectTransform)illustration.transform.parent;
            timber=rim.rectTransform;portrait=bust.rectTransform;nameRibbon=name;speaker=speakerName;body=spokenLine;scroll=reader;
            reading=(RectTransform)reader.transform;speech=(RectTransform)reading.parent;paper=speech.GetComponent<Image>();next=(RectTransform)advance.transform;nextLabel=advanceCopy;count=progress;darkLabels=new[]{heading,speaker,body,count};
            nextGroup=nextLabel.GetComponentInParent<CenteredContentRow>();nameGroup=speaker.GetComponentInParent<CenteredContentRow>();
            pageLines=localizedPageLines;initialized=true;Subscribe();RefreshLayout();
        }
        void OnEnable(){if(initialized)Subscribe();}
        void OnDisable()=>Unsubscribe();
        void OnDestroy()=>Unsubscribe();
        void Subscribe(){if(!subscribed&&isActiveAndEnabled){Canvas.willRenderCanvases+=RefreshLayout;subscribed=true;}}
        void Unsubscribe(){if(subscribed){Canvas.willRenderCanvases-=RefreshLayout;subscribed=false;}}
        public void RefreshLayout()
        {
            if(!initialized||!isActiveAndEnabled)return;
            if(game==null||game.CurrentScreen!=GameScreen.Prologue||game.ComicWorld<0||stage==null||scene==null||timber==null||portrait==null||speech==null||body==null||!stage.gameObject.activeInHierarchy){Unsubscribe();return;}
            const float textWidth=564;float font=game.Preferences.LargeText?36.72f:34;
            bool reflow=previousFont!=font||previousWidth!=textWidth;
            body.enableAutoSizing=false;body.fontSize=body.fontSizeMin=body.fontSizeMax=font;body.margin=Vector4.zero;
            if(reflow){longest=0;foreach(string line in pageLines)longest=Mathf.Max(longest,body.GetPreferredValues(line,textWidth,Mathf.Infinity).y);}
            float paperHeight=Mathf.Clamp(Mathf.Ceil(longest)+130,240,390),paperTop=1088-paperHeight,room=paperHeight-128;
            string copy=game.FitComicCopy(textWidth,room,body);
            bool changed=previousCopy!=copy||reflow;
            if(body.text!=copy)body.SetCopy(UiCopy.Raw(copy));
            if(changed)textHeight=Mathf.Ceil(body.GetPreferredValues(copy,textWidth,Mathf.Infinity).y)+2;
            float portraitTop=paperTop-164,frameBottom=portraitTop-16;
            float ratio=sceneImage.sprite.rect.width/sceneImage.sprite.rect.height;
            float pictureHeight=Mathf.Min(632/ratio,Mathf.Max(180,frameBottom-184-380/6f));
            float pictureWidth=pictureHeight*ratio,pictureTop=frameBottom-200/6f-pictureHeight;
            Place(scene,360-pictureWidth*.5f,pictureTop,pictureWidth,pictureHeight);
            Place(timber,360-pictureWidth*.5f-30,pictureTop-30,pictureWidth+60,pictureHeight+380/6f);
            Place(speech,36,paperTop,648,paperHeight);paper.pixelsPerUnitMultiplier=8;
            Place(portrait,46,portraitTop,162,152);Place(nameRibbon,28,paperTop-41,300,81);DockPortrait();
            Place(reading,42,64,textWidth,room);
            body.rectTransform.sizeDelta=new Vector2(0,textHeight);body.rectTransform.anchoredPosition=Vector2.zero;
            scroll.vertical=textHeight>room+.5f;if(changed||previousRoom!=room)scroll.verticalNormalizedPosition=1;
            if(!scroll.vertical)CenteredContentRow.CenterWrappedText(body,reading);
            Place(count.rectTransform,534,12,84,46);count.fontSize=count.fontSizeMin=count.fontSizeMax=23;count.enableAutoSizing=false;count.alignment=TextAlignmentOptions.MidlineRight;count.margin=Vector4.zero;
            foreach(var label in darkLabels)if(label.color!=Ink)label.color=Ink;
            ConversationReferenceLayout.PlaceActionPair(next.GetComponent<Button>(),stage.Find("Skip comic").GetComponent<Button>(),1120);
            var caption=game.ComicAdvanceCaption;if(nextLabel.text!=caption.ToString())nextLabel.SetCopy(caption);
            next.GetComponent<GameButtonDesign>()?.Refresh();
            // Shared button binding reparents the caption and replaces its row.
            // Never let the retired row lay out a caption in the new writing field.
            if(nextGroup==null||nextLabel.transform.parent!=nextGroup.Surface)nextGroup=nextLabel.GetComponentInParent<CenteredContentRow>();
            nextGroup?.Refresh();nameGroup?.Refresh();previousFont=font;previousWidth=textWidth;previousRoom=room;previousCopy=copy;
        }
        static readonly float[] residentBottomMargins={7f/512,0,7f/512,20f/512,22f/512,23f/512};
        void DockPortrait()
        {
            var image=portrait.GetComponent<Image>();var sprite=image.sprite;if(sprite==null)return;
            float bottomMargin=0;
            if(sprite.texture.name=="Residents"){
                int column=Mathf.RoundToInt(sprite.rect.xMin/(sprite.texture.width/3f));
                int row=Mathf.RoundToInt((sprite.texture.height-sprite.rect.yMax)/(sprite.texture.height/2f));
                bottomMargin=residentBottomMargins[Mathf.Clamp(row*3+column,0,5)];
            }else if(sprite.texture.name=="Aster")bottomMargin=Mathf.Clamp01((sprite.texture.height*(17f/1312)-sprite.rect.yMin)/sprite.rect.height);
            // Preserve the full sprite/animation UVs. Only its letterboxed image
            // moves: the painted bust base sits behind the board's upper rim.
            var size=portrait.rect.size;if(image.preserveAspect)size=sprite.rect.size*Mathf.Min(size.x/sprite.rect.width,size.y/sprite.rect.height);
            var paintedBase=portrait.rect.center+new Vector2(0,-size.y*.5f+size.y*bottomMargin);
            var writing=CenteredContentRow.ReadingRect(nameRibbon);
            var support=nameRibbon.TransformPoint(new Vector2(writing.center.x,writing.yMax-2*CenteredContentRow.Unit(nameRibbon)));
            var delta=portrait.parent.InverseTransformVector(support-portrait.TransformPoint(paintedBase));
            portrait.anchoredPosition+=new Vector2(0,delta.y);
        }
        static void Place(RectTransform rect,float x,float y,float width,float height)
        {
            var anchor=new Vector2(0,1);if(rect.anchorMin!=anchor)rect.anchorMin=anchor;if(rect.anchorMax!=anchor)rect.anchorMax=anchor;
            if(rect.pivot!=Vector2.one*.5f)rect.pivot=Vector2.one*.5f;
            var position=new Vector2(x+width*.5f,-y-height*.5f);var size=new Vector2(width,height);
            if(rect.anchoredPosition!=position)rect.anchoredPosition=position;if(rect.sizeDelta!=size)rect.sizeDelta=size;
        }
    }
}
