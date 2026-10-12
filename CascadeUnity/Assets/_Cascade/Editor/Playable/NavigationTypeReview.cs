using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Actual controls are exercised only against the temporary native review profile.
    public static class NavigationTypeReview
    {
        const string Phase="NavigationType-Oct08";
        public static bool Running {get;private set;}
        static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        static Button Control(CascadeGame game,string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name.Replace(" ","")==name.Replace(" ","")&&b.gameObject.activeInHierarchy);
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Check(EditorApplication.isPlaying&&!Running&&game!=null&&game.ProfilePath.Contains("CascadeNavigationReview"),"Temporary review profile required.");
            Running=true;game.StartCoroutine(Run(game));
        }
        static IEnumerator Run(CascadeGame game)
        {
            var log=new StringBuilder();bool completed=false;
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            game.SetUiLocale("ru");game.ReducedMotion=true;game.Preferences.LargeText=false;
            try{
                game.ShowHome();game.ShowParcel();yield return null;
                var receipt=game.Rewards.Parcels.First(p=>!p.Claimed);int claimed=game.Rewards.Parcels.Count(p=>p.Claimed);
                string puzzle=game.Interaction.Attempt.CurrentStateHash;
                Check(game.CurrentScreen==GameScreen.Parcel,"Parcel must open.");
                yield return PlayableCapture.CaptureSettled(Phase,"54-parcel-final.png",390,844,g=>CheckParcel(g,0));
                Control(game,"Openparcelgift").onClick.Invoke();yield return null;
                Check(!receipt.Claimed&&game.Rewards.Parcels.Count(p=>p.Claimed)==claimed,"Opening reveals but does not claim the parcel.");
                yield return PlayableCapture.CaptureSettled(Phase,"55-parcel-reveal.png",390,844,g=>CheckParcel(g,1));
                Control(game,"Claimgift").onClick.Invoke();yield return null;
                Check(receipt.Claimed&&game.Rewards.Parcels.Count(p=>p.Claimed)==claimed+1,"Claim durably commits one receipt.");
                yield return PlayableCapture.CaptureSettled(Phase,"56-prize-final.png",390,844,g=>CheckParcel(g,2));
                Control(game,"Keepgift").onClick.Invoke();yield return null;
                Check(game.CurrentScreen==GameScreen.Home,"Next must return to the originating Home screen.");
                Check(game.Rewards.Parcels.Count(p=>p.Claimed)==claimed+1&&game.Interaction.Attempt.CurrentStateHash==puzzle,"Leaving prize must not duplicate the claim or change the puzzle.");
                log.AppendLine("PASS open -> reveal -> Claim -> one saved receipt -> Continue to original Home; puzzle hash unchanged.");
                game.ShowHowToPlay();yield return null;
                yield return PlayableCapture.CaptureSettled(Phase,"57-guide-final.png",390,844,CheckPager);
                Control(game,"Next example").onClick.Invoke();yield return null;
                Check(game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Navigation page count").text.StartsWith("22 /"),"Next page must advance.");
                Control(game,"Previous example").onClick.Invoke();yield return null;
                Check(game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Navigation page count").text.StartsWith("21 /"),"Previous page must restore.");
                log.AppendLine("PASS actual guide arrows advance/restore the page; generated PNG and shared 48-unit row.");
                game.Preferences.LargeText=true;game.SetUiLocale("uk");game.ShowHowToPlay();yield return null;
                yield return PlayableCapture.CaptureSettled(Phase,"58-guide-large.png",320,693,CheckPager);
                log.AppendLine("PASS narrow UK Large Text: page count >= 22 physical units, aligned arrow centres, no title/counter overflow.");
                completed=true;
            }
            finally{
                Running=false;UnityEngine.Application.runInBackground=false;
                string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
                Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"controls.txt"),log+"Completed: "+completed);
            }
        }
        static void CheckParcel(CascadeGame game,int phase)
        {
            var root=game.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Menu");
            var buttons=root.GetComponentsInChildren<Button>();
            string expected=phase==0?"Openparcelgift":phase==1?"Claimgift":"Keepgift";
            Check(buttons.Any(b=>b.name==expected),"Parcel action must match the reveal/claim state.");
            Check(buttons.Count(b=>b.name=="Parcel bonus video")==((game.AdProvider?.RewardReady==true&&phase>0)?1:0),"Rewarded offer appears only when a video is ready.");
            Check(buttons.Count(b=>b.name!="Parcel bonus video"&&b.name!="View collected trophy")==1,"Parcel exposes one primary action.");
            foreach(var title in root.GetComponentsInChildren<TextMeshProUGUI>()){
                title.ForceMeshUpdate();Check(!title.isTextOverflowing,"Parcel type overflow: "+title.name);
            }
            Check(phase>0||!root.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.name=="Gift name"),"Unopened parcel must not expose an unexplained object name.");
        }
        static void CheckPager(CascadeGame game)
        {
            var count=game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Navigation page count");
            var previous=Control(game,"Previous example");var next=Control(game,"Next example");
            Vector3 Centre(RectTransform r)=>r.TransformPoint(r.rect.center);
            float y=Centre(count.rectTransform).y;
            Check(Mathf.Abs(Centre((RectTransform)previous.transform).y-y)<.1f&&Mathf.Abs(Centre((RectTransform)next.transform).y-y)<.1f,"Both arrows and count share one centre line.");
            float scale=count.GetComponentInParent<Canvas>().scaleFactor;
            Check(count.fontSize*scale>=21.9f,"Counter needs readable physical size.");
            foreach(var button in new[]{previous,next}){
                var arrow=button.GetComponentInChildren<ChevronGraphic>();
                Check(arrow!=null&&arrow.mainTexture.name=="ArrowPairOct08","Directional control must use the authored PNG.");
                Check(((RectTransform)button.transform).rect.height*scale>=47.9f,"Pager keeps a 48-unit touch target.");
            }
            foreach(var title in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name=="Navigation page count"||t.name=="Menu title")){
                title.ForceMeshUpdate();Check(!title.isTextOverflowing,"Guide type overflow: "+title.name);
            }
        }
    }
}
