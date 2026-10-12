using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Presentation;
using Cascade.Persistence;
using Cascade.Solver;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    // An isolated visual fixture: saved player progress is never written.
    public static class ReadabilityCapture
    {
        const BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
        const string Phase="Readability";
        static string capturePhase=Phase;
        static string Folder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports",capturePhase));
        static readonly List<string> findings=new List<string>();
        static void Field(CascadeGame g,string field,object value)=>typeof(CascadeGame).GetField(field,Private).SetValue(g,value);
        static void Draw(CascadeGame g)=>typeof(CascadeGame).GetMethod("DrawMenu",Private).Invoke(g,null);
        public static void Run()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required.");
            capturePhase=Phase;
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Shots());
        }
        public static void RunFinalReview()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required.");
            capturePhase="FinalReview";
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Shots());
        }
        public static void RunMenuReset()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required.");
            capturePhase="MenuReset";UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Shots());
        }
        public static void ConfirmMenuReset()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Compiled Play Mode required.");
            capturePhase="MenuReset";UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(MenuResetFixes());
        }
        static IEnumerator MenuResetFixes()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);findings.Clear();
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            try {
                g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.Preferences.LargeText=false;g.ReducedMotion=false;yield return Ready(g);Seed(g);
                g.SetUiLocale("ru");g.ShowTravelBag();yield return Shot(g,"bag-ru");g.ShowWorkshop();yield return Shot(g,"workshop-ru");
                g.ShowPowerupChallenges();yield return Shot(g,"powerups-ru");g.ShowHowToPlay();yield return Shot(g,"basics-ru");
                g.ShowSettings();yield return Shot(g,"settings-sound-ru");Field(g,"settingsPage",2);Draw(g);yield return Shot(g,"settings-language-ru");
                g.ProposePuzzleLocale("uk");yield return Shot(g,"language-confirmation-ru");g.ConfirmPuzzleLocale();
                g.SetUiLocale("en");g.ShowSettings();Field(g,"settingsPage",2);Draw(g);yield return Shot(g,"settings-language-en");
                g.ProposePuzzleLocale("ru");yield return Shot(g,"language-confirmation-en");g.ConfirmPuzzleLocale();
                yield return LocalisedMenus(g,"uk",true);g.Preferences.LargeText=false;g.SetEditorSafeInsets(Vector4.zero);
                g.SetUiLocale("ru");g.Catalog.TryGetLocalizedBinding("C13","ru",out var words);g.LoadLevel(words.Definition,"ru");g.ShowGame();yield return Ready(g);g.Interaction.SetSuspended(false);
                g.Interaction.Execute(Cascade.Domain.PuzzleCommand.Swap(0));double until=Time.realtimeSinceStartupAsDouble+10;
                while(g.EventPlayer.CurrentEvent!=Cascade.Domain.PuzzleEventKind.BombDefused&&g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                yield return Shot(g,"bomb-defuse-flourish",390,844,0);while(g.Interaction.IsPresenting)yield return null;yield return Shot(g,"bomb-defused",390,844);
                File.WriteAllLines(Path.Combine(Folder,"text-overflow.txt"),findings);Debug.Log("CASCADE menu defect confirmation complete; overflow findings: "+findings.Count);
            }finally{UnityEngine.Application.runInBackground=background;}
        }
        public static void ConfirmMetaNavigation()
        {
            capturePhase="MenuReset";UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(MetaNavigationShots());
        }
        static IEnumerator MetaNavigationShots()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);findings.Clear();yield return Ready(g);Seed(g);
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            try {
                g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;
                foreach(string locale in new[]{"ru","en","uk"}) {
                    g.Preferences.LargeText=locale=="uk";g.SetUiLocale(locale);g.SetEditorSafeInsets(locale=="uk"?new Vector4(0,.045f,0,.07f):Vector4.zero);
                    string suffix=locale=="uk"?"-large-safe-uk":"-"+locale;
                    g.ShowRequests();yield return Shot(g,"requests"+suffix);g.ShowContracts();yield return Shot(g,"contracts"+suffix);g.ShowRecords();yield return Shot(g,"records"+suffix);
                }
                g.Preferences.LargeText=false;g.SetEditorSafeInsets(Vector4.zero);g.SetUiLocale("en");g.ShowTravelBag();yield return Shot(g,"bag-wide",1440,900);
                g.SetUiLocale("ru");g.Catalog.TryGetLocalizedBinding("C13","ru",out var words);g.LoadLevel(words.Definition,"ru");g.ShowGame();yield return Ready(g);g.Interaction.SetSuspended(false);
                g.Interaction.Execute(Cascade.Domain.PuzzleCommand.Swap(0));double until=Time.realtimeSinceStartupAsDouble+10;
                while(g.EventPlayer.CurrentEvent!=Cascade.Domain.PuzzleEventKind.BombDefused&&g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                yield return Shot(g,"bomb-defuse-flourish",390,844,0);while(g.Interaction.IsPresenting)yield return null;
                File.WriteAllLines(Path.Combine(Folder,"text-overflow.txt"),findings);Debug.Log("CASCADE meta navigation overlap confirmation complete; overflow findings: "+findings.Count);
            }finally{UnityEngine.Application.runInBackground=background;}
        }
        public static void ConfirmButtons(){capturePhase=Phase;UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(ButtonShots());}
        public static void ConfirmBombMark()
        {
            capturePhase="FinalReview";UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(BombShots());
        }
        static IEnumerator BombShots()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            try{
                findings.Clear();findings.AddRange(File.ReadAllLines(Path.Combine(Folder,"text-overflow.txt")).Where(s=>!s.StartsWith("bomb-letter-")&&!s.StartsWith("bomb-defused |")));
                yield return Ready(g);g.ReducedMotion=false;g.Preferences.LargeText=false;g.SetUiLocale("ru");g.SendMessage("OnApplicationFocus",true);
                g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;
                if(g.Catalog.TryGetLocalizedBinding("C13","ru",out var words))g.LoadLevel(words.Definition,"ru");
                g.ShowGame();yield return Ready(g);g.Interaction.SetSuspended(false);
                var motion=new List<string>{"frame,utc,bomb_angle,state_hash"};
                for(int i=0;i<2;i++){
                    yield return new WaitForSecondsRealtime(.75f);yield return Shot(g,"bomb-letter-"+i,390,844);
                    var bomb=g.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.name=="DeviceBomb");motion.Add(i+","+DateTime.UtcNow.ToString("o")+","+bomb.transform.eulerAngles.z.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+g.Interaction.Attempt.CurrentStateHash);
                }
                g.Interaction.Execute(Cascade.Domain.PuzzleCommand.Swap(0));while(g.Interaction.IsPresenting)yield return null;
                yield return Shot(g,"bomb-defused",390,844);
                File.WriteAllLines(Path.Combine(Folder,"text-overflow.txt"),findings);File.WriteAllLines(Path.Combine(Folder,"bomb-motion.csv"),motion);
                Debug.Log("CASCADE final bomb mark confirmation complete; overflow findings: "+findings.Count);
            }finally{UnityEngine.Application.runInBackground=background;}
        }
        static IEnumerator ButtonShots()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            findings.Clear();var file=Path.Combine(Folder,"text-overflow.txt");
            if(File.Exists(file))findings.AddRange(File.ReadAllLines(file).Where(s=>!s.StartsWith("journal-ru |")&&!s.StartsWith("powerups-ru |")&&!s.StartsWith("settings-sound-ru |")&&!s.StartsWith("bag-wide |")));
            try{
                yield return Ready(g);Seed(g);g.Preferences.LargeText=false;g.SetUiLocale("ru");g.SendMessage("OnApplicationFocus",true);
                g.ShowJournal();yield return Shot(g,"journal-ru");g.ShowPowerupChallenges();yield return Shot(g,"powerups-ru");g.ShowSettings();yield return Shot(g,"settings-sound-ru");
                g.SetUiLocale("en");g.ShowTravelBag();yield return Shot(g,"bag-wide",1440,900);
                File.WriteAllLines(file,findings);Debug.Log("CASCADE button space confirmation complete; overflow findings: "+findings.Count);
            }finally{UnityEngine.Application.runInBackground=background;}
        }
        static void Seed(CascadeGame g)
        {
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
            foreach(var content in g.Catalog.PrimaryLevels.Take(9)){
                var a=new AttemptService(content.Definition,content.Locale);
                foreach(var command in PuzzleSolver.FindShortest(a.Resolver).Path)a.Execute(command);
                profile.RecordClear(a);profile.Rewards.RecordDelivery(a);
            }
            profile.Rewards.Synchronize(profile.Clears.Select(c=>c.CampaignNodeId));
            foreach(var receipt in profile.Rewards.Parcels.Take(capturePhase==Phase?3:2))profile.Rewards.ClaimParcel(receipt.Order);
            ((HashSet<string>)typeof(CascadeGame).GetField("sessionClears",Private).GetValue(g)).UnionWith(profile.Clears.Select(c=>c.CampaignNodeId));
        }
        static IEnumerator Shot(CascadeGame g,string name,int width=320,int height=693,float delay=.22f)
        {
            if(delay>0)yield return new WaitForSecondsRealtime(delay);
            yield return PlayableCapture.CaptureSettled(capturePhase,name+".png",width,height,pose=>{
                foreach(var text in pose.GetComponentsInChildren<TextMeshProUGUI>()){
                    if(string.IsNullOrEmpty(text.text))continue;text.ForceMeshUpdate();
                    if(text.isTextOverflowing)findings.Add(name+" | "+text.name+" | "+text.text.Replace('\n',' '));
                    if(text.name=="Label")foreach(var word in text.textInfo.wordInfo.Take(text.textInfo.wordCount)){
                        if(text.textInfo.characterInfo[word.firstCharacterIndex].lineNumber!=text.textInfo.characterInfo[word.lastCharacterIndex].lineNumber)
                            findings.Add(name+" | word split | "+word.GetWord());
                    }
                }
            });
        }
        static IEnumerator LocalisedMenus(CascadeGame g,string locale,bool large)
        {
            g.Preferences.LargeText=large;g.SetUiLocale(locale);
            if(large)g.SetEditorSafeInsets(new Vector4(0,.045f,0,.07f));
            Action[] screens={g.ShowTravelBag,g.ShowJournal,g.ShowWorkshop,g.ShowRequests,g.ShowRecords,g.ShowContracts,g.ShowParcel,g.ShowPowerupChallenges,g.ShowReachChallenges,g.ShowHowToPlay,g.ShowDevices,g.ShowLessons};
            string[] names={"bag","journal","workshop","requests","records","contracts","parcel","powerups","ribbon","basics","tools","lessons"};
            for(int i=0;i<screens.Length;i++){screens[i]();yield return Shot(g,names[i]+(large?"-large-safe-":"-")+locale);}
            g.ShowSettings();yield return Shot(g,"settings-sound"+(large?"-large-safe-":"-")+locale);
            Field(g,"settingsPage",1);Draw(g);yield return Shot(g,"settings-comfort"+(large?"-large-safe-":"-")+locale);
            Field(g,"settingsPage",2);Draw(g);yield return Shot(g,"settings-language"+(large?"-large-safe-":"-")+locale);
            g.ProposePuzzleLocale("ru");yield return Shot(g,"language-confirmation"+(large?"-large-safe-":"-")+locale);g.ConfirmPuzzleLocale();
        }
        static IEnumerator Ready(CascadeGame g)
        {
            double until=Time.realtimeSinceStartupAsDouble+30;
            while(!g.PreparationReady&&Time.realtimeSinceStartupAsDouble<until)yield return null;
            if(!g.PreparationReady)throw new InvalidOperationException("Preparation timed out.");
        }
        static IEnumerator Shots()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();Field(g,"persistenceReady",false);
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.ReducedMotion=false;g.Preferences.LargeText=false;
            findings.Clear();bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            try {
                yield return Ready(g);Seed(g);g.SetUiLocale("ru");g.SendMessage("OnApplicationFocus",true);
                if(capturePhase!=Phase){
                    g.ShowJourney();yield return Shot(g,"journey-ru");g.ShowLessons();yield return Shot(g,"lessons-ru");g.ShowParcel();yield return Shot(g,"parcel-ru");
                }
                g.ShowTravelBag();yield return Shot(g,"bag-ru");
                g.ShowJournal();yield return Shot(g,"journal-ru");
                g.ShowCollection();yield return Shot(g,"collection-ru");Field(g,"giftPage",3);Draw(g);yield return Shot(g,"collection-locked-ru");
                g.ShowWorkshop();yield return Shot(g,"workshop-ru");
                g.ShowRequests();Field(g,"requestPage",2);Draw(g);yield return Shot(g,"requests-ru");
                g.ShowRecords();yield return Shot(g,"records-ru");
                g.ShowContracts();yield return Shot(g,"contracts-ru");
                g.ShowPowerupChallenges();yield return Shot(g,"powerups-ru");
                g.ShowReachChallenges();yield return Shot(g,"ribbon-ru");
                g.ShowHowToPlay();yield return Shot(g,"basics-ru");
                g.ShowDevices();Field(g,"deviceIndex",2);Draw(g);yield return Shot(g,"bomb-guide-ru");
                g.ShowSettings();yield return Shot(g,"settings-sound-ru");Field(g,"settingsPage",1);Draw(g);yield return Shot(g,"settings-comfort-ru");Field(g,"settingsPage",2);Draw(g);yield return Shot(g,"settings-language-ru");
                if(capturePhase=="MenuReset"){
                    g.ProposePuzzleLocale("uk");yield return Shot(g,"language-confirmation-ru");g.ConfirmPuzzleLocale();g.SetUiLocale("ru");
                }
                g.SendMessage("TogglePause");yield return Shot(g,"pause-ru");
                g.SetUiLocale("en");g.ShowTravelBag();yield return Shot(g,"bag-en",390,844);yield return Shot(g,"bag-wide",1440,900);
                g.Preferences.LargeText=true;g.SetUiLocale("uk");g.ShowCollection();yield return Shot(g,"collection-large-uk");g.Preferences.LargeText=false;
                if(capturePhase=="MenuReset"){
                    g.Preferences.LargeText=true;g.SetEditorSafeInsets(new Vector4(0,.045f,0,.07f));g.ShowSettings();Field(g,"settingsPage",2);Draw(g);yield return Shot(g,"settings-language-large-safe-uk");g.Preferences.LargeText=false;g.SetEditorSafeInsets(Vector4.zero);
                    yield return LocalisedMenus(g,"en",false);yield return LocalisedMenus(g,"uk",true);g.Preferences.LargeText=false;g.SetEditorSafeInsets(Vector4.zero);
                }
                g.LoadNode("C13");g.ShowGame();yield return Ready(g);g.Interaction.SetSuspended(false);yield return Shot(g,"bomb-shape",390,844);
                g.SetUiLocale("ru");if(g.Catalog.TryGetLocalizedBinding("C13","ru",out var words))g.LoadLevel(words.Definition,"ru");g.ShowGame();yield return Ready(g);g.Interaction.SetSuspended(false);
                var motion=new List<string>{"frame,utc,bomb_angle,state_hash"};
                for(int i=0;i<2;i++){
                    yield return new WaitForSecondsRealtime(.75f);yield return Shot(g,"bomb-letter-"+i,390,844);
                    var bomb=g.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.name=="DeviceBomb");motion.Add(i+","+DateTime.UtcNow.ToString("o")+","+bomb.transform.eulerAngles.z.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+g.Interaction.Attempt.CurrentStateHash);
                }
                if(capturePhase!=Phase){
                    g.Interaction.Execute(Cascade.Domain.PuzzleCommand.Swap(0));
                    if(capturePhase=="MenuReset"){
                        double until=Time.realtimeSinceStartupAsDouble+10;
                        while(g.EventPlayer.CurrentEvent!=Cascade.Domain.PuzzleEventKind.BombDefused&&g.Interaction.IsPresenting&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                        yield return Shot(g,"bomb-defuse-flourish",390,844,0);
                    }
                    while(g.Interaction.IsPresenting)yield return null;yield return Shot(g,"bomb-defused",390,844);
                }
                Directory.CreateDirectory(Folder);File.WriteAllLines(Path.Combine(Folder,"text-overflow.txt"),findings);File.WriteAllLines(Path.Combine(Folder,"bomb-motion.csv"),motion);
                Debug.Log("CASCADE "+capturePhase+" native batch complete; overflow findings: "+findings.Count);
            }finally{UnityEngine.Application.runInBackground=background;}
        }
    }
}
