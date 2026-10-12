using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class HudCapture
    {
        public static void Capture()
        {
            if(!EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Requires the compiled running game.");
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.StartCoroutine(Review(game));
        }
        static IEnumerator Review(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.ReducedMotion=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("en");game.ChooseStory();game.SendMessage("BeginJourney",(object)0);
            for(int i=0;i<8&&game.CurrentScreen==GameScreen.Dialogue;i++){game.SendMessage("AdvanceDialogue");yield return null;}
            yield return Ready(game);
            var report=new StringBuilder();
            yield return PlayableCapture.CaptureSettled("CompactHud","gameplay-en.png",390,844,g=>{
                g.SendMessage("ResetInstruction");g.Coach.Advance(16,true);g.SendMessage("UpdateCoaching",true);Measure(g,report,"390 EN fresh instruction / replayed teaching gesture");
            });
            yield return new WaitForSecondsRealtime(3.5f);
            yield return PlayableCapture.CaptureSettled("CompactHud","instruction-en.png",390,844,g=>Measure(g,report,"390 EN delayed instruction"));
            game.SetUiLocale("ru");game.Preferences.LargeText=true;
            // A short three-piece rail is the strictest header-width case.
            var shortest=game.Catalog.PrimaryLevels.First(c=>c.Definition.InitialSymbolIds.Count==3);
            game.LoadNode(shortest.Definition.CampaignNodeId);yield return Ready(game);yield return new WaitForSecondsRealtime(3.5f);
            yield return PlayableCapture.CaptureSettled("CompactHud","gameplay-ru-small.png",320,693,g=>Measure(g,report,"320 RU large text / three symbols"));
            var close=game.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name=="Dismiss instruction");close.onClick.Invoke();
            game.Preferences.LargeText=false;game.SetEditorSafeInsets(Vector4.zero);
            yield return PlayableCapture.CaptureSettled("CompactHud","gameplay-wide.png",1440,900,g=>Measure(g,report,"1440 wide after dismissal"));
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","CompactHud");
            File.WriteAllText(Path.Combine(folder,"runtime.txt"),report.ToString());
            Debug.Log("CASCADE compact HUD capture complete.");
        }
        static IEnumerator Ready(CascadeGame game)
        {
            float until=Time.realtimeSinceStartup+12;
            while((!game.PreparationReady||!game.Interaction.CanPlay)&&Time.realtimeSinceStartup<until)yield return null;
            if(!game.PreparationReady||!game.Interaction.CanPlay)throw new InvalidOperationException("Puzzle preparation did not finish.");
            yield return null;
        }
        static void Measure(CascadeGame game,StringBuilder report,string name)
        {
            var rects=game.GetComponentsInChildren<RectTransform>(true);var camera=Camera.main;var corners=new Vector3[4];
            var header=rects.Single(r=>r.name=="Header");
            var ropes=game.Machine.GetComponentsInChildren<LineRenderer>();
            float left=camera.WorldToScreenPoint(ropes.Single(r=>r.name=="Left braided rope").GetPosition(0)).x;
            float right=camera.WorldToScreenPoint(ropes.Single(r=>r.name=="Right braided rope").GetPosition(0)).x;
            header.GetWorldCorners(corners);float min=camera.WorldToScreenPoint(corners[0]).x,max=camera.WorldToScreenPoint(corners[2]).x;
            if(min<left+6||max>right-6)throw new InvalidOperationException("Header escaped the inner rope padding.");
            report.AppendLine(name+": header="+min.ToString("F1")+".."+max.ToString("F1")+"; ropes="+left.ToString("F1")+".."+right.ToString("F1"));
            foreach(var id in new[]{"Hint","Carved chapter plaque","Pause","Undo","Drop","Dismiss instruction"}){
                var r=rects.Single(t=>t.name==id);r.GetWorldCorners(corners);
                var a=camera.WorldToScreenPoint(corners[0]);var b=camera.WorldToScreenPoint(corners[2]);
                report.AppendLine(id+" parent="+r.parent.name+" bounds="+a.ToString("F1")+".."+b.ToString("F1"));
            }
            var title=rects.Single(r=>r.name=="Level").GetComponent<TMP_Text>();title.ForceMeshUpdate();
            if(title.isTextOverflowing)throw new InvalidOperationException("The centred chapter title overflows.");
            if(title.text.StartsWith("Уровень ")&&title.textInfo.lineCount!=1)throw new InvalidOperationException("The short Russian level caption should fit on one line.");
            var hand=rects.Single(r=>r.name=="First exchange hand").GetComponent<UnityEngine.UI.Image>();
            var opacity=rects.Single(r=>r.name=="Quiet instruction paper").GetComponent<CanvasGroup>();
            if(hand.gameObject.activeSelf&&(!hand.GetComponent<Canvas>().overrideSorting||hand.GetComponent<Canvas>().sortingOrder!=30))throw new InvalidOperationException("The tutorial pointer lost its foreground layer.");
            if(rects.Any(r=>r.name=="Shortest route"||r.name=="Device focus"))throw new InvalidOperationException("A removed HUD decoration survived.");
            report.AppendLine("instruction alpha="+opacity.alpha+"; raycast="+opacity.blocksRaycasts+"; pointer shader="+hand.material.shader.name+"; sorting="+hand.GetComponent<Canvas>().sortingOrder+"; title pixels="+(title.fontSize*title.canvas.scaleFactor).ToString("F1"));
        }
    }
}
