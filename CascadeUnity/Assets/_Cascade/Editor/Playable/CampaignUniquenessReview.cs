using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cascade.Editor.Playable
{
    // Official MCP menu fallback; runtime scene/profile only, never saves an Editor scene.
    [InitializeOnLoad]
    public static class CampaignUniquenessReview
    {
        const string Key="Cascade.UniquenessReview",Previous="Cascade.UniquenessPrevious",Folder="CampaignUniquenessMcpOct15";
        static CampaignUniquenessReview()
        {
            string path=SessionState.GetString(Key,"");if(path.Length>0)CascadeGame.EditorProfileDirectory=path;
            EditorApplication.playModeStateChanged+=state=>{
                if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key,"").Length>0)EditorApplication.delayCall+=Start;
                if(state==PlayModeStateChange.EnteredEditMode)Restore();
            };
        }
        [MenuItem("CASCADE/QA/Review campaign uniqueness")]
        public static void Prepare()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Run from stable Edit Mode.");
            string directory=Path.Combine(Path.GetTempPath(),"CascadeUniquenessMcp-"+Guid.NewGuid().ToString("N"));
            var profile=new PlayerProfile {AccountIntroSeen=true,FirstGestureShown=true};profile.Preferences.Tutorials=false;profile.Missions.NextAfterClears=20000;
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))profile.SeenMechanics.Add(lesson);
            for(int i=0;i<1000;i++)profile.HeardStory.Add(i);
            new ProfileStore(Path.Combine(directory,"cascade-profile.json")).Save(profile);
            SessionState.SetString(Previous,CascadeGame.EditorProfileDirectory??"");SessionState.SetString(Key,directory);
            CascadeGame.EditorProfileDirectory=directory;EditorApplication.isPlaying=true;
        }
        static void Restore()
        {
            if(SessionState.GetString(Key,"").Length==0)return;
            string previous=SessionState.GetString(Previous,"");CascadeGame.EditorProfileDirectory=previous.Length>0?previous:null;
            SessionState.EraseString(Key);SessionState.EraseString(Previous);
        }
        static void Start()
        {
            var host=new GameObject("Temporary campaign uniqueness review") {hideFlags=HideFlags.DontSave};UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<SettingsReferenceLayout>().StartCoroutine(Capture(host));
        }
        static IEnumerator Capture(GameObject host)
        {
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            try{
                yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
                var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
                game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.SetUiLocale("ru");game.SetEditorSafeInsets(new Vector4(0,.00001f,0,.00001f));
                string root=Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
                string folder=Path.Combine(root,"Reports",Folder);Directory.CreateDirectory(folder);
                var expected=JArray.Parse(File.ReadAllText(Path.Combine(root,"Reports","CampaignUniquenessOct15","levels.json")));
                var actualCategories=new JArray();var method=typeof(CascadeGame).GetMethod("PreferredLevels",BindingFlags.Instance|BindingFlags.NonPublic);
                foreach(string locale in new[]{"en","ru","uk"})foreach(PuzzleMode mode in Enum.GetValues(typeof(PuzzleMode))){
                    if(game.ConversationOpen)game.SkipConversation();yield return null;
                    game.Preferences.PuzzleLocale=locale;game.SelectMode(mode);yield return null;
                    if(game.PreferredMode!=mode)throw new InvalidOperationException("Review could not select "+mode);
                    var levels=((IEnumerable<LevelContent>)method.Invoke(game,null)).Select(c=>c.Definition.PuzzleId).ToArray();
                    string reportLocale=mode==PuzzleMode.Shapes?"all":locale;
                    var planned=expected.Where(v=>(string)v["mode"]==mode.ToString()&&(string)v["locale"]==reportLocale).Select(v=>(string)v["puzzle"]).ToArray();
                    if(!levels.SequenceEqual(planned))throw new InvalidOperationException("Audited category differs from actual runtime: "+mode+"/"+locale);
                    actualCategories.Add(new JObject {["mode"]=mode.ToString(),["locale"]=locale,["levels"]=new JArray(levels)});
                }
                if(game.ConversationOpen)game.SkipConversation();yield return null;
                game.Preferences.PuzzleLocale="en";game.SelectMode(PuzzleMode.Shapes);game.ShowHome();
                string Read(string name)=>Resources.Load<TextAsset>("CascadeContent/"+name).text;
                var store=new PublishedTriadStore(Read("production_triad_recipes"),Read("production_triad_manifest"));int captures=0;
                foreach(string node in new[]{"C199","C209","C238","C284","C288","C293","C304","C309"}){
                    var content=game.Catalog.GetPrimary(node);var run=store.Bind(content.Definition,content.Locale,game.Catalog.ContentHash);
                    for(int stage=0;stage<3;stage++){
                        game.LoadLevel(run.Level(content.Definition,stage),content.Locale);game.ShowGame();game.Machine.DelayAmbientGust(1000);
                        yield return new WaitForSecondsRealtime(.35f);
                        yield return PlayableCapture.CaptureSettled(Folder,node+"-stage-"+(stage+1)+".png",390,844);captures++;
                    }
                }
                File.WriteAllText(Path.Combine(folder,"runtime-validation.json"),new JObject {
                    ["passed"]=true,["native_phone_captures"]=captures,["category_sequences_match_audit"]=actualCategories,
                    ["recipes_hash"]=PublishedTriadStore.Hash(Read("production_triad_recipes")),
                    ["temporary_profile"]=SessionState.GetString(Key,""),["unsaved_editor_scene_preserved"]=true,
                    ["scope"]="Imported Resources; actual category selector; native 390x844 renders of all 24 replaced stages. Does not prove human enjoyment or device performance."
                }.ToString()+"\n");
                Debug.Log("CAMPAIGN_UNIQUENESS_MCP_PASS: 9 runtime category/locale sequences match audit; 24 changed-stage phone captures; temporary profile only.");
                EditorApplication.delayCall+=()=>EditorApplication.isPlaying=false;
            }finally{UnityEngine.Application.runInBackground=background;Restore();UnityEngine.Object.Destroy(host);}
        }
    }
}
