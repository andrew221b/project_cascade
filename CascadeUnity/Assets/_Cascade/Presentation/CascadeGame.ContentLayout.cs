using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        CenteredContentRow CentreContent(RectTransform area,params RectTransform[] elements)
        {
            var layout=area.GetComponent<CenteredContentRow>()??area.gameObject.AddComponent<CenteredContentRow>();if(layout.Elements==null)layout.Initialize(area,elements);return layout;
        }
        void CentreHeading(Image board,TextMeshProUGUI label)
        {
            CentreContent(board.rectTransform,label.rectTransform);
            string key=board.name.ToLowerInvariant();
            if((key.Contains("title")||key.Contains("header")||key.Contains("welcome"))&&board.GetComponent<AttachedPlants>()==null)
                board.gameObject.AddComponent<AttachedPlants>().Initialize(this,EnvironmentWorld,true);
        }
        void CentreStack(RectTransform area,params RectTransform[] elements)
        {
            var layout=area.GetComponent<CenteredContentRow>()??area.gameObject.AddComponent<CenteredContentRow>();layout.InitializeVertical(area,elements);
        }
        void CentreInfoColumn(RectTransform column)
        {
            var copy=column.GetComponentsInChildren<TextMeshProUGUI>();float width=0;
            foreach(var label in copy){label.fontSize=label.fontSizeMax;width=Mathf.Max(width,label.GetPreferredValues(label.text,Mathf.Infinity,Mathf.Infinity).x+1);}
            column.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,Mathf.Min(column.rect.width,width));
            var layout=column.gameObject.AddComponent<CenteredContentRow>();layout.Padding=0;
            var content=new RectTransform[copy.Length];for(int i=0;i<copy.Length;i++)content[i]=copy[i].rectTransform;
            layout.InitializeVertical(column,content);
        }
        void StandardAction(Button button,bool amber=false)
        {
            var input=button.GetComponent<Image>();var face=button.transform.Find("Unified action surface")?.GetComponent<Image>();
            if(face==null){
                var rect=Rect("Unified action surface",button.transform,Vector2.zero,Vector2.one);rect.SetAsFirstSibling();face=rect.gameObject.AddComponent<Image>();face.raycastTarget=false;
            }
            var skin=face.GetComponent<PrimaryButtonSkin>()??face.gameObject.AddComponent<PrimaryButtonSkin>();var wanted=amber?ParcelArt("AmberAction"):HomeArt("PlayButton");if(face.sprite!=wanted)skin.Configure(wanted);
            input.color=Color.clear;button.targetGraphic=face;button.transition=Selectable.Transition.ColorTint;
            var colors=button.colors;colors.normalColor=colors.highlightedColor=Color.white;colors.pressedColor=new Color(.93f,.98f,.91f);colors.disabledColor=new Color(.72f,.76f,.71f);button.colors=colors;
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.color=amber?new Color(.24f,.075f,.012f):MenuPaper;
            if(!amber&&!navigationLightLabels.Contains(label))navigationLightLabels.Add(label);
            if(button.GetComponentsInChildren<Graphic>().Length<=3)CentreSingleAction(button);
        }
        void CentreSingleAction(Button button)
        {
            var label=button.GetComponentInChildren<TextMeshProUGUI>();if(label==null)return;
            var row=CentreContent((RectTransform)button.transform,label.rectTransform);row.Refresh();
        }
        void StandardMenuActions()
        {
            foreach(var button in menuRoot.GetComponentsInChildren<Button>()){
                var face=button.GetComponent<Image>();if(face==null||face.color.a==0||face.sprite!=enamelControls[0]&&face.sprite!=art.panel&&face.sprite!=HomeArt("PlayButton"))continue;
                var rect=(RectTransform)button.transform;if(rect.rect.width<rect.rect.height*2)continue;
                StandardAction(button);var captions=button.GetComponentsInChildren<TextMeshProUGUI>();
                if(captions.Length==1&&button.GetComponentsInChildren<Graphic>().Length<=3)CentreSingleAction(button);
            }
        }
    }
}
