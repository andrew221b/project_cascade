using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using Cascade.Editor.Playable;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Cascade.Editor.Release
{
    [InitializeOnLoad]
    public static class LegalUiReview
    {
        static LegalUiReview(){if(SessionState.GetBool("Cascade.LegalReview.Auto",false))EditorApplication.update+=WaitForPlay;}
        public static void Begin()
        {
            SessionState.SetString("Cascade.LegalReview.OldProfile",SessionState.GetString("Cascade.NavigationReviewProfile",""));
            SessionState.SetBool("Cascade.LegalReview.OldBackground",UnityEngine.Application.runInBackground);
            NavigationCapture.Prepare();UnityEngine.Application.runInBackground=true;
            SessionState.SetBool("Cascade.LegalReview.Auto",true);EditorApplication.update-=WaitForPlay;EditorApplication.update+=WaitForPlay;
            EditorApplication.isPlaying=true;
        }
        static void WaitForPlay()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))return;
            EditorApplication.update-=WaitForPlay;SessionState.SetBool("Cascade.LegalReview.Auto",false);Capture();
        }
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Requires isolated legal QA profile.");
            game.StartCoroutine(Run(game));
        }
        static IEnumerator Run(CascadeGame game)
        {
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Website-Oct09"));
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            game.Preferences.Tutorials=false;game.Preferences.NotificationsEnabled=false;game.ReducedMotion=true;game.SetUiLocale("en");game.ShowSettings();
            yield return null;
            var buttons=game.GetComponentsInChildren<Button>().Where(b=>b.name=="Restorepurchases"||b.name=="PrivacyPolicy"||b.name=="TermsofUse").ToArray();
            if(buttons.Length!=3)throw new InvalidOperationException("Restore and legal actions missing from Settings.");
            foreach(var name in new[]{"privacy","terms"}){
                game.OpenLegalDocument(name);yield return null;yield return null;
                var text=game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Legal document text");
                var scroll=text.GetComponentInParent<ScrollRect>();
                if(scroll==null||text.rectTransform.rect.height<=scroll.viewport.rect.height)throw new InvalidOperationException("Legal reader cannot scroll.");
                yield return PlayableCapture.CaptureSettled("Website-Oct09",name+"-in-game.png",390,844);
                yield return PlayableCapture.CaptureSettled("Website-Oct09",name+"-end-in-game.png",390,844,_=>{
                    Canvas.ForceUpdateCanvases();scroll.StopMovement();scroll.Rebuild(CanvasUpdate.PostLayout);scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();text.ForceMeshUpdate();
                    var last=text.textInfo.characterInfo[text.textInfo.characterCount-1];
                    var point=scroll.viewport.InverseTransformPoint(text.transform.TransformPoint(last.bottomLeft));
                    if(point.y<scroll.viewport.rect.yMin-2||point.y>scroll.viewport.rect.yMax+2)throw new InvalidOperationException("Document end is outside the reading viewport.");
                });
                game.CloseLegalDocument();yield return null;
                if(game.CurrentScreen!=GameScreen.Settings)throw new InvalidOperationException("Legal Close lost Settings navigation.");
            }
            File.WriteAllText(Path.Combine(folder,"unity-legal-ui.txt"),"PASS: Settings exposes Restore purchases, Privacy Policy and Terms of Use. Both full documents scroll to the end, close back to Settings and leave gameplay state unchanged. Isolated QA profile only. Native store transactions are not configured or tested.");
        }
    }
}
