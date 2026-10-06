using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        const float InstructionDelay=3.2f;
        readonly HashSet<string> dismissedInstructions=new HashSet<string>();
        CanvasGroup instructionOpacity;
        UnityEngine.UI.Button instructionClose;
        HintMark hintMark;
        Canvas tutorialPointerCanvas;
        Material tutorialOverlayMaterial;
        string instructionKey;
        float instructionAge;

        void BuildInstructionControls()
        {
            instructionOpacity=statusPaper.gameObject.AddComponent<CanvasGroup>();
            HideInstruction();
            var close=Rect("Dismiss instruction",statusPaper,new Vector2(1,0),Vector2.one);
            var hit=Background(close,Color.clear,true);
            instructionClose=close.gameObject.AddComponent<UnityEngine.UI.Button>();
            instructionClose.targetGraphic=hit;
            instructionClose.onClick.AddListener(()=>{dismissedInstructions.Add(instructionKey??"");HideInstruction();});
            var mark=Rect("Close mark",close,Vector2.zero,Vector2.one).gameObject.AddComponent<InstructionCloseMark>();
            mark.color=Ink;mark.raycastTarget=false;
        }

        void LayoutInstruction(float unit)
        {
            if(instructionClose==null)return;
            var close=(RectTransform)instructionClose.transform;
            close.offsetMin=new Vector2(-44*unit,0);close.offsetMax=Vector2.zero;
            status.rectTransform.offsetMin=new Vector2(14*unit,4*unit);
            status.rectTransform.offsetMax=new Vector2(-44*unit,-4*unit);
            status.enableAutoSizing=true;status.fontSizeMin=13*unit;
            status.fontSizeMax=16*unit*(Preferences.LargeText?1.12f:1);
        }

        void LayoutGameplayHeader(float boardWidth)
        {
            float unit=1/Mathf.Min(1,header.GetComponentInParent<Canvas>().scaleFactor);
            // Both fixed upper rope anchors are at +/- .475 of the rail width.
            // Leave room for the rope radius and its independent wind excursion.
            header.anchorMin=header.anchorMax=new Vector2(.5f,0);
            header.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,boardWidth*.95f-24*unit);
            header.anchoredPosition=new Vector2(0,safeRoot.rect.height-36*unit);
            float control=48*unit,gap=6*unit;
            var left=(RectTransform)hint.transform;
            left.anchorMin=Vector2.zero;left.anchorMax=new Vector2(0,1);
            left.offsetMin=Vector2.zero;left.offsetMax=new Vector2(control,0);
            var right=(RectTransform)pause.transform;
            right.anchorMin=new Vector2(1,0);right.anchorMax=Vector2.one;
            right.offsetMin=new Vector2(-control,0);right.offsetMax=Vector2.zero;
            var plaque=(RectTransform)header.Find("Carved chapter plaque");
            plaque.anchorMin=Vector2.zero;plaque.anchorMax=Vector2.one;
            plaque.offsetMin=new Vector2(control+gap,4*unit);plaque.offsetMax=new Vector2(-control-gap,-4*unit);
            levelLabel.rectTransform.offsetMin=new Vector2(4*unit,5*unit);
            levelLabel.rectTransform.offsetMax=new Vector2(-4*unit,-5*unit);
            levelLabel.fontSizeMin=12*unit;
            levelLabel.fontSizeMax=17*unit*(Preferences.LargeText?1.12f:1);
            levelLabel.alignment=TextAlignmentOptions.Center;
            levelLabel.textWrappingMode=levelLabel.GetComponent<LocalizedCaption>().SourceIdentity.StartsWith("Level ",System.StringComparison.Ordinal)
                ?TextWrappingModes.NoWrap:TextWrappingModes.Normal;
        }

        void ResetInstruction()
        {
            dismissedInstructions.Clear();instructionKey=null;instructionAge=0;HideInstruction();
        }
        void HideInstruction()
        {
            if(instructionOpacity==null)return;
            instructionOpacity.alpha=0;instructionOpacity.interactable=false;instructionOpacity.blocksRaycasts=false;
        }
        void UpdateInstruction(bool foreground,float seconds)
        {
            if(instructionOpacity==null)return;
            string key=status.text??"";
            if(instructionKey!=key){instructionKey=key;instructionAge=0;HideInstruction();}
            bool playing=foreground&&CurrentScreen==GameScreen.Playing&&!Interaction.IsPaused&&!zoomOpen;
            if(playing&&!IsHoldingToken&&!Interaction.IsPresenting)instructionAge+=seconds;
            bool show=playing&&!IsHoldingToken&&!Interaction.IsPresenting&&reactionBanner?.IsShowing!=true
                &&key.Length>0&&!dismissedInstructions.Contains(key)&&(instructionAge>=InstructionDelay||preparation.HasFailed);
            instructionOpacity.alpha=ReducedMotion?(show?1:0):Mathf.MoveTowards(instructionOpacity.alpha,show?1:0,seconds/.18f);
            instructionOpacity.interactable=instructionOpacity.blocksRaycasts=show&&instructionOpacity.alpha>.9f;
        }
    }
}
