using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly HashSet<string> dismissedInstructions=new HashSet<string>();
        CanvasGroup instructionOpacity;
        CanvasGroup chapterOpacity;
        HintMark hintMark;
        Canvas tutorialPointerCanvas;
        Material tutorialOverlayMaterial;
        string instructionKey;
        string chapterKey;
        float chapterAge;

        void BuildInstructionControls()
        {
            instructionOpacity=statusPaper.gameObject.AddComponent<CanvasGroup>();
            chapterOpacity=header.Find("Carved chapter plaque").gameObject.AddComponent<CanvasGroup>();
            chapterOpacity.alpha=0;chapterOpacity.interactable=chapterOpacity.blocksRaycasts=false;
            HideInstruction();

        }

        void LayoutInstruction(float unit)
        {
            status.rectTransform.offsetMin=new Vector2(14*unit,4*unit);
            status.rectTransform.offsetMax=new Vector2(-14*unit,-4*unit);
            status.enableAutoSizing=true;status.fontSizeMin=16*unit;
            status.fontSizeMax=(Preferences.LargeText?20:18)*unit;
        }

        void LayoutGameplayHeader(float boardWidth)
        {
            float unit=1/Mathf.Min(1,header.GetComponentInParent<Canvas>().scaleFactor);
            // Top controls keep their own inset; the square footer cards sit further inside the posts.
            header.anchorMin=header.anchorMax=new Vector2(.5f,0);
            header.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,safeRoot.rect.width*.90f);
            header.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,54*unit);
            header.anchoredPosition=new Vector2(0,safeRoot.rect.height-36*unit);
            float control=54*unit,gap=4*unit;
            var left=(RectTransform)pause.transform;
            left.anchorMin=Vector2.zero;left.anchorMax=new Vector2(0,1);
            left.offsetMin=Vector2.zero;left.offsetMax=new Vector2(control,0);
            var right=(RectTransform)help.transform;
            right.anchorMin=new Vector2(1,0);right.anchorMax=Vector2.one;
            right.offsetMin=new Vector2(-control,0);right.offsetMax=Vector2.zero;
            bool extraHelp=help.gameObject.activeSelf;
            float rightInset=extraHelp?control+8*unit:0;
            var lamp=(RectTransform)hint.transform;lamp.anchorMin=new Vector2(1,0);lamp.anchorMax=Vector2.one;lamp.offsetMin=new Vector2(-control-rightInset,0);lamp.offsetMax=new Vector2(-rightInset,0);
            var plaque=(RectTransform)header.Find("Carved chapter plaque");
            plaque.anchorMin=plaque.anchorMax=new Vector2(.5f,.5f);
            plaque.anchoredPosition=Vector2.left*rightInset*.5f;
            plaque.sizeDelta=new Vector2(Mathf.Min(boardWidth*.95f-24*unit,header.rect.width-2*(control+gap)-rightInset),44*unit);
            // Narrow the ceiling tether, not the playing rail or its touch targets.
            machine.SetUpperRopeHalfWidth(header.TransformVector(Vector3.right*(safeRoot.rect.width*.5f-82*unit)).magnitude);
            levelLabel.rectTransform.offsetMin=new Vector2(4*unit,5*unit);
            levelLabel.rectTransform.offsetMax=new Vector2(-4*unit,-5*unit);
            levelLabel.fontSizeMin=12*unit;
            levelLabel.fontSizeMax=17*unit*(Preferences.LargeText?1.12f:1);
            levelLabel.alignment=TextAlignmentOptions.Midline;
            levelLabel.textWrappingMode=TextWrappingModes.Normal;
        }

        void ResetInstruction()
        {
            if(profile?.SeenCoachPrompts!=null)dismissedInstructions.UnionWith(profile.SeenCoachPrompts);instructionKey=null;observedCoachRoute=Interaction.Attempt.RouteLength;HideInstruction();
            chapterKey=null;chapterAge=0;if(chapterOpacity!=null)chapterOpacity.alpha=0;
        }
        void RetireInstruction(string key)
        {
            if(string.IsNullOrEmpty(key)||!dismissedInstructions.Add(key))return;
            if(profile!=null){if(profile.SeenCoachPrompts==null)profile.SeenCoachPrompts=new List<string>();profile.SeenCoachPrompts.Add(key);PersistProgress();}
            HideInstruction();
        }
        int observedCoachRoute;
        void ObserveCoachingProgress()
        {
            int route=Interaction.Attempt.RouteLength;
            if(route>observedCoachRoute&&!string.IsNullOrEmpty(instructionKey)&&Coach.Prompt(Interaction.Attempt.Current)!=instructionKey)
                RetireInstruction(instructionKey);
            observedCoachRoute=route;
        }
        void HideInstruction()
        {
            if(instructionOpacity==null)return;
            instructionOpacity.alpha=0;instructionOpacity.interactable=false;instructionOpacity.blocksRaycasts=false;
        }
        void UpdateInstruction(bool foreground,float seconds)
        {
            UpdateChapter(foreground,seconds);
            if(instructionOpacity==null)return;
            // First-use teaching lives in the film and guided finger, never in a timed text strip.
            // Keep only blocking preparation failures readable beside the existing Retry control.
            bool show=foreground&&CurrentScreen==GameScreen.Playing&&!Interaction.IsPaused&&!zoomOpen
                &&(preparation.HasFailed||descentPlanFailed);
            instructionKey=show?status.GetComponent<LocalizedCaption>()?.SourceIdentity:null;
            instructionOpacity.alpha=show?1:0;
            instructionOpacity.interactable=instructionOpacity.blocksRaycasts=false;
        }
        void UpdateChapter(bool foreground,float seconds)
        {
            if(chapterOpacity==null)return;
            string key=levelLabel.text??"";
            if(chapterKey!=key){chapterKey=key;chapterAge=0;chapterOpacity.alpha=0;}
            bool playing=foreground&&CurrentScreen==GameScreen.Playing&&!Interaction.IsPaused&&!zoomOpen;
            if(playing&&Interaction.CanPlay)chapterAge+=seconds;
            const float fadeIn=.28f,hold=2.2f,fadeOut=.45f;
            float alpha=chapterAge<fadeIn?Mathf.SmoothStep(0,1,chapterAge/fadeIn):
                1-Mathf.SmoothStep(0,1,(chapterAge-fadeIn-hold)/fadeOut);
            chapterOpacity.alpha=playing&&key.Length>0?(ReducedMotion?(chapterAge<fadeIn+hold?1:0):alpha):0;
        }
    }
}
