using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly Sprite[] enamelControls=new Sprite[3];
        Image undoMeaning;
        void BuildControlArt()
        {
            var atlas=Resources.Load<Texture2D>("CascadeArt/Reactions/EnamelControls");
            for(int i=0;i<3;i++)enamelControls[i]=WorkshopSpriteSet.Control(atlas,i);
            void Dress(Button b,int style){var image=b.GetComponent<Image>();image.sprite=enamelControls[style];image.type=Image.Type.Sliced;image.color=Color.white;}
            Dress(undo,0);Dress(hint,0);Dress(help,0);Dress(drop,1);Dress(pause,0);Dress(retry,1);
            foreach(var b in new[]{undo,hint,help,pause}){var face=b.GetComponent<Image>();face.sprite=CleanMenuArt.Get("SquareButton");b.gameObject.AddComponent<PrimaryButtonSkin>().Configure(face.sprite);}
            hint.transform.SetParent(header,false);StylePrimaryFace(drop.GetComponent<Image>());
            undoMeaning=Picture("Undo meaning",undo.transform,new Vector2(.10f,.16f),new Vector2(.40f,.84f),ResultMedal(2),true);
            var instruction=statusPaper.GetComponent<Image>();instruction.sprite=enamelControls[0];instruction.type=Image.Type.Sliced;instruction.color=Color.white;
            Dress(targetZoom.GetComponentInChildren<Button>(true),1);
            var plaque=header.Find("Carved chapter plaque").GetComponent<Image>();plaque.sprite=enamelControls[2];plaque.type=Image.Type.Sliced;plaque.color=Color.white;
            plaque.pixelsPerUnitMultiplier=3;
            levelLabel.rectTransform.SetParent(plaque.transform,false);levelLabel.rectTransform.anchorMin=Vector2.zero;levelLabel.rectTransform.anchorMax=Vector2.one;levelLabel.rectTransform.offsetMin=new Vector2(10,3);levelLabel.rectTransform.offsetMax=new Vector2(-10,-3);
            var pauseGlyph=pause.GetComponentInChildren<PauseMark>();pauseGlyph.color=new Color(.20f,.055f,.014f);help.GetComponentInChildren<TextMeshProUGUI>().color=new Color(.20f,.055f,.014f);
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
                label.color=button==drop?Color.white:new Color(.20f,.055f,.014f);button.GetComponent<Image>().color=Color.white;
                float unit=1/Mathf.Min(1,button.GetComponentInParent<Canvas>().scaleFactor);label.margin=button==undo||button==hint?Vector4.zero:new Vector4(14*unit,5*unit,14*unit,5*unit);
            }
        }
        void DrawUndoCaption(bool tools)
        {
            var label=undo.GetComponentInChildren<TextMeshProUGUI>();
            label.SetCopy(tools?UiCopy.Raw(""):UiCopy.Raw("× "+Interaction.Attempt.FreeUndoRemaining));
            label.textWrappingMode=TextWrappingModes.NoWrap;
            label.rectTransform.anchorMin=new Vector2(.42f,0);label.rectTransform.anchorMax=new Vector2(.94f,1);
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            if(undo.transform.parent==actions){
                var face=undo.GetComponent<Image>();face.sprite=CleanMenuArt.Get("SquareButton");face.type=Image.Type.Sliced;face.preserveAspect=false;
                undoMeaning.gameObject.SetActive(true);undoMeaning.sprite=ItemsToolArt(1);undoMeaning.rectTransform.anchorMin=new Vector2(.10f,.13f);undoMeaning.rectTransform.anchorMax=new Vector2(.90f,.88f);
                label.SetCopy(UiCopy.Raw(Interaction.Attempt.FreeUndoRemaining.ToString()));
                label.rectTransform.anchorMin=new Vector2(.60f,-.02f);label.rectTransform.anchorMax=new Vector2(1.02f,.34f);label.fontSize=13/Mathf.Min(1,undo.GetComponentInParent<Canvas>().scaleFactor);
                return;
            }
            if(undoMeaning==null)return;undoMeaning.gameObject.SetActive(true);
            if(tools)LoadNavigationArt();
            undoMeaning.sprite=tools?navigationIcons[1]:ResultMedal(2);
            undoMeaning.rectTransform.anchorMin=new Vector2(tools?.22f:.10f,.16f);
            undoMeaning.rectTransform.anchorMax=new Vector2(tools?.78f:.40f,.84f);
        }
    }
}
