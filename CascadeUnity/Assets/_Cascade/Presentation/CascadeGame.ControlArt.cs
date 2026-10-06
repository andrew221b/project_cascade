using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly Sprite[] enamelControls=new Sprite[3];
        void BuildControlArt()
        {
            var atlas=Resources.Load<Texture2D>("CascadeArt/Reactions/EnamelControls");
            for(int i=0;i<3;i++)enamelControls[i]=WorkshopSpriteSet.Control(atlas,i);
            void Dress(Button b,int style){var image=b.GetComponent<Image>();image.sprite=enamelControls[style];image.type=Image.Type.Sliced;image.color=Color.white;}
            Dress(undo,0);Dress(hint,0);Dress(drop,1);Dress(pause,0);
            var instruction=statusPaper.GetComponent<Image>();instruction.sprite=enamelControls[0];instruction.type=Image.Type.Sliced;instruction.color=Color.white;
            Dress(targetZoom.GetComponentInChildren<Button>(true),1);
            var plaque=header.Find("Carved chapter plaque").GetComponent<Image>();plaque.sprite=enamelControls[2];plaque.type=Image.Type.Sliced;plaque.color=Color.white;
            plaque.pixelsPerUnitMultiplier=3;
            levelLabel.rectTransform.SetParent(plaque.transform,false);levelLabel.rectTransform.anchorMin=Vector2.zero;levelLabel.rectTransform.anchorMax=Vector2.one;levelLabel.rectTransform.offsetMin=new Vector2(10,3);levelLabel.rectTransform.offsetMax=new Vector2(-10,-3);
            var pauseGlyph=pause.GetComponentInChildren<PauseMark>();pauseGlyph.color=new Color(1,.97f,.84f);
        }
        void PaintControlLabels()
        {
            // Keep pale type on the teal enamel after locale/accessibility refreshes.
            if(status!=null)status.color=MenuPaper;
            foreach(var button in new[]{undo,hint,drop})if(button!=null){
                var label=button.GetComponentInChildren<TextMeshProUGUI>();
                label.color=button==drop?Ink:new Color(1,.97f,.84f);button.GetComponent<Image>().color=Color.white;
                float unit=1/Mathf.Min(1,button.GetComponentInParent<Canvas>().scaleFactor);label.margin=new Vector4(14*unit,5*unit,14*unit,5*unit);
            }
        }
    }
}
