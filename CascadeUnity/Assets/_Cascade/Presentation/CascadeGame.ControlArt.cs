using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly Sprite[] enamelControls=new Sprite[3];
        Image undoMeaning;RectTransform undoBadge;
        void BuildControlArt()
        {
            var atlas=Resources.Load<Texture2D>("CascadeArt/Reactions/EnamelControls");
            for(int i=0;i<3;i++)enamelControls[i]=WorkshopSpriteSet.Control(atlas,i);
            void Dress(Button b,int style){var image=b.GetComponent<Image>();image.sprite=enamelControls[style];image.type=Image.Type.Sliced;image.color=Color.white;}
            Dress(undo,0);Dress(hint,0);Dress(help,0);Dress(drop,1);Dress(pause,0);Dress(retry,1);
            foreach(var b in new[]{undo,hint,help,pause})StyleGameplayControl(b,b==undo);
            hint.transform.SetParent(header,false);StylePrimaryFace(drop.GetComponent<Image>());
            var retryGlyph=Rect("Gameplay retry icon",retry.transform,Vector2.one*.5f,Vector2.one*.5f).gameObject.AddComponent<RetryArrowGraphic>();retryGlyph.rectTransform.sizeDelta=Vector2.one*48;retryGlyph.color=new Color(.19f,.055f,.014f);retryGlyph.raycastTarget=false;
            RegisterButtonDesign(undo,GameButtonRole.IconPrimary);RegisterButtonDesign(hint,GameButtonRole.IconSecondary);RegisterButtonDesign(help,GameButtonRole.IconSecondary);RegisterButtonDesign(pause,GameButtonRole.IconSecondary);RegisterButtonDesign(retry,GameButtonRole.IconPrimary);RegisterButtonDesign(drop,GameButtonRole.PrimaryGreen);
            undoMeaning=Picture("Undo meaning",undo.transform,new Vector2(.10f,.16f),new Vector2(.40f,.84f),ResultMedal(2),true);
            undoBadge=Rect("Undo stock badge",undo.transform,new Vector2(.68f,-.03f),new Vector2(1.04f,.33f));undoBadge.gameObject.AddComponent<RoundHudBadge>().raycastTarget=false;
            undo.GetComponentInChildren<TextMeshProUGUI>().transform.SetAsLastSibling();
            var instruction=statusPaper.GetComponent<Image>();instruction.sprite=enamelControls[0];instruction.type=Image.Type.Sliced;instruction.color=Color.white;
            Dress(targetZoom.GetComponentInChildren<Button>(true),1);
            var plaque=header.Find("Carved chapter plaque").GetComponent<Image>();plaque.sprite=enamelControls[2];plaque.type=Image.Type.Sliced;plaque.color=Color.white;
            plaque.pixelsPerUnitMultiplier=3;
            levelLabel.rectTransform.SetParent(plaque.transform,false);levelLabel.rectTransform.anchorMin=Vector2.zero;levelLabel.rectTransform.anchorMax=Vector2.one;levelLabel.rectTransform.offsetMin=new Vector2(10,3);levelLabel.rectTransform.offsetMax=new Vector2(-10,-3);
            var pauseGlyph=pause.GetComponentInChildren<PauseMark>();pauseGlyph.color=new Color(.20f,.055f,.014f);help.GetComponentInChildren<TextMeshProUGUI>().color=new Color(.20f,.055f,.014f);
        }
        void StyleGameplayControl(Button button,bool cream)
        {
            (button.GetComponent<PrimaryButtonSkin>()??button.gameObject.AddComponent<PrimaryButtonSkin>()).Configure(CleanMenuArt.Get(cream?"InventoryStatCard":"SquareButton"));
            // Disabled tools keep their cream surface opaque against the sky.
            var colors=button.colors;colors.disabledColor=new Color(.80f,.82f,.78f,1);button.colors=colors;
            var sprig=FailureReferenceArt.Sprig(0);
            if(cream){
                foreach(string side in new[]{"left","right"})
                    Picture("Gameplay tool rooted flowers "+side,button.transform,Vector2.zero,Vector2.zero,sprig,true);
                return;
            }
            Picture("Gameplay frame flowers upper",button.transform,new Vector2(-.08f,.74f),new Vector2(.28f,1.10f),sprig,true);
            var lower=Picture("Gameplay frame flowers lower",button.transform,new Vector2(.72f,-.08f),new Vector2(1.08f,.28f),sprig,true);
            lower.rectTransform.localRotation=Quaternion.Euler(0,0,180);
        }
        void SeatGameplayToolFlowers(Button button)
        {
            if(button==null)return;var rim=(RectTransform)button.transform;
            for(int side=0;side<2;side++){
                var flower=button.transform.Find("Gameplay tool rooted flowers "+(side==0?"left":"right"))?.GetComponent<Image>();
                if(flower==null||flower.sprite==null)continue;var rect=flower.rectTransform;
                // This opaque leaf base sits on the rounded green/gold rim. The
                // pivot keeps it attached when ButtonFeel scales the bouquet.
                rect.anchorMin=rect.anchorMax=new Vector2(side==0?.17f:.83f,.84f);
                rect.pivot=new Vector2(.5f,.12f);rect.anchoredPosition=Vector2.zero;
                float width=Mathf.Min(rim.rect.width,rim.rect.height)*.30f;
                rect.sizeDelta=new Vector2(width,width*flower.sprite.rect.height/flower.sprite.rect.width);
                rect.localRotation=Quaternion.Euler(0,0,side==0?75:-75);
            }
        }
        void FitGameplayToolIcon(Image icon)
        {
            if(icon==null)return;if(icon.GetComponentInParent<GameButtonDesign>()?.Bound==true)return;var button=(RectTransform)icon.transform.parent;var rect=icon.rectTransform;
            // The cream field begins 35 source pixels inside the 164px skin.
            // Fit the whole illustration with two logical pixels on every side.
            float inset=button.rect.height*35f/164+2*ControlUnit;
            rect.anchorMin=rect.anchorMax=Vector2.one*.5f;rect.anchoredPosition=Vector2.zero;
            rect.sizeDelta=Vector2.one*Mathf.Max(1,Mathf.Min(button.rect.width,button.rect.height)-2*inset);icon.preserveAspect=true;
        }
        void PaintControlLabels()
        {
            // Keep pale type on the teal enamel after locale/accessibility refreshes.
            if(status!=null)status.color=MenuPaper;
            foreach(var label in baseTextSizes.Keys){
                if(label==null||label.name!="Label")continue;
                var face=label.GetComponentInParent<Button>()?.GetComponent<Image>();
                if(face!=null&&face.sprite==enamelControls[0])label.color=MenuPaper;
            }
            foreach(var button in new[]{undo,hint,drop})if(button!=null){
                var label=button.GetComponentInChildren<TextMeshProUGUI>();
                label.color=button==drop||button==undo&&undo.transform.parent==actions?Color.white:new Color(.20f,.055f,.014f);button.GetComponent<Image>().color=Color.white;
                float unit=1/Mathf.Min(1,button.GetComponentInParent<Canvas>().scaleFactor);label.margin=button==undo||button==hint?Vector4.zero:new Vector4(14*unit,5*unit,14*unit,5*unit);
            }
            if(undoBadge!=null&&undoBadge.gameObject.activeInHierarchy){var stock=undo.GetComponentInChildren<TextMeshProUGUI>();if(stock.transform.parent==undoBadge)FitRoundHudBadge(undoBadge,stock);}
            if(drop!=null){var label=drop.GetComponentInChildren<TextMeshProUGUI>();label.fontSize=label.fontSizeMin=label.fontSizeMax=(Preferences.LargeText?26:24)*ControlUnit;CentreSingleAction(drop);}
            if(help!=null){var label=help.GetComponentInChildren<TextMeshProUGUI>();label.color=new Color(.20f,.055f,.014f);label.fontSize=label.fontSizeMin=label.fontSizeMax=40*ControlUnit;CentreSingleAction(help);}
            if(fieldTools!=null){var plus=fieldTools.transform.Find("Field bag plus badge")?.GetComponentInChildren<TextMeshProUGUI>();if(plus!=null)FitRoundHudBadge((RectTransform)plus.transform.parent,plus);}
        }
        void FitRoundHudBadge(RectTransform badge,TextMeshProUGUI label)
        {
            // A 12px action-button inset consumes almost the entire small circle.
            label.color=Color.white;label.fontStyle=FontStyles.Bold;
            // Counters keep their own readable size when the action shrinks to its
            // minimum touch area; the circle is not a percentage of that button.
            badge.anchorMin=badge.anchorMax=new Vector2(.86f,.15f);badge.pivot=Vector2.one*.5f;badge.anchoredPosition=Vector2.zero;badge.sizeDelta=Vector2.one*(28*ControlUnit);
            label.rectTransform.localScale=Vector3.one;label.font=GameButtonPreset.Load(GameButtonRole.IconPrimary).Font;
            label.enableAutoSizing=false;label.fontSize=label.fontSizeMin=label.fontSizeMax=(label.text=="+"?32:24)*ControlUnit;
            var fit=CentreContent(badge,label.rectTransform);fit.Padding=2;
            if(label.transform.parent!=badge)fit.Initialize(badge,label.rectTransform);fit.Refresh();
        }
        void DrawUndoCaption(bool tools)
        {
            var label=undo.GetComponentInChildren<TextMeshProUGUI>();
            if(undo.transform.parent==actions){
                var face=undo.GetComponent<Image>();face.sprite=CleanMenuArt.Get("InventoryStatCard");face.type=Image.Type.Sliced;face.preserveAspect=false;undoBadge.gameObject.SetActive(true);
                undoMeaning.gameObject.SetActive(true);undoMeaning.sprite=ItemsToolArt(1);FitGameplayToolIcon(undoMeaning);
                label.SetCopy(UiCopy.Raw(Interaction.Attempt.FreeUndoRemaining.ToString()));
                FitRoundHudBadge(undoBadge,label);
                return;
            }
            label.rectTransform.SetParent(undo.transform,false);label.SetCopy(tools?UiCopy.Raw(""):UiCopy.Raw("× "+Interaction.Attempt.FreeUndoRemaining));
            label.textWrappingMode=TextWrappingModes.NoWrap;label.rectTransform.anchorMin=new Vector2(.42f,0);label.rectTransform.anchorMax=new Vector2(.94f,1);label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            if(undoBadge!=null)undoBadge.gameObject.SetActive(false);if(undoMeaning==null)return;undoMeaning.gameObject.SetActive(true);
            if(tools)LoadNavigationArt();
            undoMeaning.sprite=tools?navigationIcons[1]:ResultMedal(2);
            undoMeaning.rectTransform.anchorMin=new Vector2(tools?.22f:.10f,.16f);
            undoMeaning.rectTransform.anchorMax=new Vector2(tools?.78f:.40f,.84f);
        }
    }
}
