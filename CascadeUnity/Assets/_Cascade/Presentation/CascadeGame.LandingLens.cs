using System.Linq;
using Cascade.Domain;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        void OpenLandingPreview(TransitionResult prediction)
        {
            if(prediction==null||!prediction.Accepted)return;
            CloseLandingPreview();Interaction.SetPaused(true);machine.SetAmbientPaused(true);
            landingPreview=Rect("Landing prediction",safeRoot,Vector2.zero,Vector2.one);
            var canvas=landingPreview.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=35;
            landingPreview.gameObject.AddComponent<GraphicRaycaster>();Background(landingPreview,new Color(.035f,.12f,.13f,.72f),true);
            var panel=Rect("Landing preview cloth",landingPreview,new Vector2(.04f,.25f),new Vector2(.96f,.75f));var plate=Background(panel,Color.white,false);plate.sprite=enamelControls[0];plate.type=Image.Type.Sliced;
            var heading=Text("Prediction title",panel,new Vector2(.06f,.81f),new Vector2(.94f,.96f),24,"Next landing");heading.font=art.titleFont;heading.color=MenuPaper;
            Picture("Lens",panel,new Vector2(.38f,.61f),new Vector2(.62f,.82f),ToolArt(4),true);
            var row=Rect("Predicted row",panel,new Vector2(.07f,.39f),new Vector2(.93f,.59f));
            for(int i=0;i<prediction.State.Row.Count;i++){
                float width=1f/prediction.State.Row.Count;var symbol=prediction.State.Row[i];
                var cell=Rect("Predicted symbol",row,new Vector2(i*width+.015f,.03f),new Vector2((i+1)*width-.015f,.97f));
                if(symbol.Kind==SymbolKind.Shape){var face=cell.gameObject.AddComponent<ShapeFaceGraphic>();face.raycastTarget=false;face.SetShape(art.SymbolMesh(symbol),art.SymbolMaterial(symbol).GetColor("_BaseColor"),true);}
                else {Panel(cell,new Color(1,.94f,.78f));var glyph=Text("Mini glyph",cell,Vector2.zero,Vector2.one,26,UiCopy.Raw(symbol.Value));glyph.font=art.titleFont;glyph.alignment=TextAlignmentOptions.Center;}
            }
            UiCopy outcome=prediction.State.Status==PuzzleStatus.Won?"The row would arrive home.":prediction.State.Status==PuzzleStatus.Lost?"This landing would lose the row. Try another move.":prediction.State.Row.SequenceEqual(Interaction.Attempt.Level.TargetSymbolIds)?"The row will match the target on the next board.":"This is where the pieces will land. Keep arranging below.";
            var explanation=Text("Prediction explanation",panel,new Vector2(.08f,.22f),new Vector2(.92f,.36f),18,outcome);explanation.color=MenuPaper;explanation.enableAutoSizing=true;explanation.fontSizeMin=16*MenuUnit;explanation.fontSizeMax=18*MenuUnit;
            var close=Button("Keep playing",panel,new Vector2(.12f,0),new Vector2(.88f,0),CloseLandingPreview);close.name="Close landing preview";var closeRect=(RectTransform)close.transform;closeRect.pivot=new Vector2(.5f,0);closeRect.anchoredPosition=new Vector2(0,12*MenuUnit);closeRect.sizeDelta=new Vector2(0,48*MenuUnit);close.GetComponent<Image>().sprite=enamelControls[0];close.GetComponent<Image>().color=Color.white;close.GetComponentInChildren<TMP_Text>().color=MenuPaper;StandardAction(close);
            if(!ReducedMotion)panel.DOScale(1,.32f).From(.96f).SetEase(Ease.OutQuint).SetUpdate(true).SetLink(landingPreview.gameObject);
            RenderControls();
        }
        public void CloseLandingPreview()
        {
            if(landingPreview==null)return;Destroy(landingPreview.gameObject);landingPreview=null;
            if(Interaction!=null){Interaction.SetPaused(CurrentScreen!=GameScreen.Playing);machine.SetAmbientPaused(CurrentScreen!=GameScreen.Playing);RenderControls();}
        }
    }
}
