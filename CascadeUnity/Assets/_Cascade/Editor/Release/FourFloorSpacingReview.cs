using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Editor.Playable;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Release
{
    // Real camera/layout/suspension checks against packaged stages, in an isolated profile.
    [InitializeOnLoad]
    public static class FourFloorSpacingReview
    {
        const string Prefix="Cascade.FourFloorSpacing.",Phase="FourFloorSpacing-Oct10";
        const BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
        static readonly Vector3[] Corners=new Vector3[4];
        static int layouts,contacts,samples;static float smallestGap=float.PositiveInfinity,smallestClearance=float.PositiveInfinity;
        static FourFloorSpacingReview(){if(SessionState.GetBool(Prefix+"Finishing",false))EditorApplication.update+=Finish;else if(SessionState.GetBool(Prefix+"Auto",false)){SessionState.SetBool(Prefix+"Started",false);EditorApplication.update+=Tick;UnityEngine.Application.logMessageReceived+=RecordError;}}
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            NavigationCapture.Prepare();Directory.CreateDirectory(Output);
            SessionState.SetBool(Prefix+"Auto",true);SessionState.SetBool(Prefix+"Started",false);SessionState.SetInt(Prefix+"Errors",0);
            SessionState.SetFloat(Prefix+"Began",(float)EditorApplication.timeSinceStartup);
            UnityEngine.Application.logMessageReceived+=RecordError;EditorApplication.update+=Tick;UnityEngine.Application.runInBackground=true;EditorApplication.isPlaying=true;
        }
        static void RecordError(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)SessionState.SetInt(Prefix+"Errors",SessionState.GetInt(Prefix+"Errors",0)+1);}
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup-SessionState.GetFloat(Prefix+"Began",0)>240){Debug.LogError("Four-floor spacing review timed out");EditorApplication.Exit(2);return;}
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))return;
            EditorApplication.QueuePlayerLoopUpdate();if(SessionState.GetBool(Prefix+"Started",false))return;
            SessionState.SetBool(Prefix+"Started",true);g.StartCoroutine(Run(g));
        }
        static void Finish()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;
            EditorApplication.update-=Tick;EditorApplication.update-=Finish;UnityEngine.Application.logMessageReceived-=RecordError;
            SessionState.SetBool(Prefix+"Auto",false);SessionState.SetBool(Prefix+"Finishing",false);NavigationCapture.Clear();
            EditorApplication.Exit(SessionState.GetInt(Prefix+"Errors",0)==0?0:2);
        }
        static object Read(object owner,string name)=>owner.GetType().GetField(name,Private|BindingFlags.Public).GetValue(owner);
        static T Read<T>(object owner,string name)=>(T)Read(owner,name);
        static void Check(bool ok,string error){if(!ok)throw new InvalidOperationException(error);}
        static float Bottom(RectTransform rect,RectTransform relative){rect.GetWorldCorners(Corners);return Corners.Min(p=>relative.InverseTransformPoint(p).y);}
        static void Inspect(CascadeGame g,string label)
        {
            var floors=Read<List<RectTransform>>(g,"floorViews");var area=Read<RectTransform>(g,"floorArea");
            float unit=1/Mathf.Min(1,floors[0].GetComponentInParent<Canvas>().scaleFactor);
            float minimum=Mathf.Min(area.rect.height*.84f/3,floors[0].rect.height*.6f+14*unit);
            for(int i=0;i<3;i++){
                float gap=area.InverseTransformPoint(floors[i].position).y-area.InverseTransformPoint(floors[i+1].position).y;
                Check(!float.IsNaN(gap)&&gap>=minimum-.2f,label+" squeezed deck "+i+": "+gap+" < "+minimum);smallestGap=Mathf.Min(smallestGap,gap);
            }
            var overlays=Read<List<RectTransform>>(g,"deviceOverlays");
            foreach(var dock in Read<IEnumerable>(g,"deviceDocks")){
                int floor=Read<int>(dock,"floor");var root=Read<RectTransform>(dock,"root");
                if(!root.gameObject.activeInHierarchy)continue;
                Check(floor>=g.VisibleFrame.ActiveFloor,label+" retired contact stayed visible");
                var body=Read<HangingPropBody>(dock,"body");var card=overlays[floor];
                Check(float.IsFinite(body.Length)&&body.Length>=0&&root.rect.width>0,label+" invalid hanging body");
                float ceiling;
                if(floor<3){ceiling=Enumerable.Range(0,3).Max(side=>card.InverseTransformPoint(g.Machine.SurfacePoint(floor+1,side*.5f,.445f)).y);}
                else {var receiver=Read<TextMeshProUGUI>(g,"receiverLabel").rectTransform;receiver.GetWorldCorners(Corners);ceiling=Corners.Max(p=>card.InverseTransformPoint(p).y);}
                var icon=Read<Image>(dock,"icon");float clearance=Bottom(icon.rectTransform,card)-ceiling;
                var timer=Read<TextMeshProUGUI>(dock,"countdown");if(timer!=null)clearance=Mathf.Min(clearance,Bottom(timer.rectTransform,card)-ceiling);
                Check(clearance>=4*unit,label+" contact/timer crossed next deck: "+clearance);smallestClearance=Mathf.Min(smallestClearance,clearance/unit);contacts++;
            }
            samples++;
        }
        static IEnumerator Ready(CascadeGame g){float deadline=Time.realtimeSinceStartup+18;while(!g.Interaction.CanPlay){Check(Time.realtimeSinceStartup<deadline,"Stage blocked");yield return null;}}
        static IEnumerator Settled(CascadeGame g){float deadline=Time.realtimeSinceStartup+18;while(g.Interaction.IsPresenting){Check(Time.realtimeSinceStartup<deadline,"Descent blocked");yield return null;}}
        static void Capture(CascadeGame g,string name,int width,int height)
        {
            var camera=Camera.main;var active=RenderTexture.active;var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{camera.Render();RenderTexture.active=camera.targetTexture;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Output,name+".png"),image.EncodeToPNG());}
            finally{RenderTexture.active=active;UnityEngine.Object.Destroy(image);}
        }
        static IEnumerator Run(CascadeGame g)
        {
            var camera=Camera.main;var old=camera.targetTexture;RenderTexture portrait=null;
            try{
                g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
                g.Preferences.NotificationsEnabled=false;g.Preferences.Tutorials=false;g.ReducedMotion=false;
                var profile=Read<PlayerProfile>(g,"profile");foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
                profile.SeenCoachPrompts.Add("controls-two-ways-v1");profile.SeenCoachPrompts.Add("controls-undo-v1");profile.SeenCoachPrompts.Add("closed-path-v2");
                typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(g,false);
                var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Private).GetValue(g);
                economy.Ledger.WayfinderScrolls=economy.Ledger.UndoSpools=economy.Ledger.ReturnHourglasses=economy.Ledger.SignalLanterns=economy.Ledger.LandingLenses=economy.Ledger.RepairKits=1;
                var recipes=CloudJson.Decode<Dictionary<string,DescentStage[]>>(Resources.Load<TextAsset>("CascadeContent/production_triad_recipes").text);
                var cases=new List<(LevelDefinition level,DescentStage recipe)>();
                foreach(var pair in recipes){var source=g.Catalog.GetPrimary(pair.Key).Definition;var run=new DescentRun{Version=6,SourceHash=source.ContentHash,Stages=pair.Value};for(int stage=0;stage<3;stage++)if(pair.Value[stage].Triad.Boards.Length==4)cases.Add((run.Level(source,stage),pair.Value[stage]));}
                Check(cases.Count==97,"Published four-floor census changed: "+cases.Count);
                foreach(int width in new[]{390,320}){
                    int height=width==390?844:693;portrait=new RenderTexture(width,height,24);camera.targetTexture=portrait;
                    yield return null;g.RefreshLayout();
                    foreach(var fixture in cases){
                        g.LoadLevel(fixture.level);g.ShowGame();yield return null;g.RefreshLayout();yield return null;Inspect(g,fixture.level.CampaignNodeId+"/"+width);layouts++;
                    }
                    // Three dense arrangements, including a contact on the bottom deck.
                    foreach(var fixture in cases.Where(c=>c.recipe.Triad.Boards.Count(b=>b.BombSlot>=0||b.KeySlot>=0||b.RepairSlot>=0||b.StampSlot>=0||b.PowerSlot>=0)>=2).Take(2)
                        .Concat(cases.Where(c=>{var b=c.recipe.Triad.Boards[3];return b.BombSlot>=0||b.KeySlot>=0||b.RepairSlot>=0||b.StampSlot>=0||b.PowerSlot>=0;}).Take(1))){
                        g.LoadLevel(fixture.level);g.ShowGame();yield return Ready(g);string hash=g.Interaction.Attempt.CurrentStateHash;
                        for(int frame=0;frame<40;frame++){if(frame%10==0){g.Machine.CueWind((frame/10)%2==0);g.CueDeviceWind((frame/10)%2==0);}yield return null;Inspect(g,"wind/"+width);}
                        Check(hash==g.Interaction.Attempt.CurrentStateHash,"Cosmetic spacing changed puzzle state");
                        g.ToggleFieldToolTray();Check(g.FieldToolTrayOpen,"Bag did not open");float deadline=Time.realtimeSinceStartup+3;
                        while(Read<float>(g,"fieldTrayProgress")<.999f){Check(Time.realtimeSinceStartup<deadline,"Bag tween blocked");yield return null;Inspect(g,"bag/"+width);}Capture(g,"dense-bag-"+width,width,height);
                        g.ToggleFieldToolTray();deadline=Time.realtimeSinceStartup+3;
                        while(Read<float>(g,"fieldTrayProgress")>.001f){Check(Time.realtimeSinceStartup<deadline,"Close tween blocked");yield return null;Inspect(g,"close/"+width);}
                        Capture(g,"dense-"+width,width,height);
                        bool descended=false;
                        foreach(string step in fixture.recipe.Witness){
                            Check(g.Interaction.Execute(CampaignTriadAudit.Parse(step)).Accepted,"Witness rejected");yield return Settled(g);
                            if(g.Interaction.Attempt.Current.ActiveFloor>0&&g.Interaction.Attempt.Current.Status==PuzzleStatus.Playing){Inspect(g,"arrived/"+width);descended=true;break;}
                        }
                        if(descended){string beforeUndo=g.Interaction.Attempt.CurrentStateHash;g.Undo();yield return Settled(g);Check(beforeUndo!=g.Interaction.Attempt.CurrentStateHash,"Undo after descent rejected");Inspect(g,"undo/"+width);}
                    }
                    camera.targetTexture=old;portrait.Release();UnityEngine.Object.Destroy(portrait);portrait=null;
                }
                Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Unity recorded review errors");
                File.WriteAllText(Path.Combine(Output,"native-review.txt"),$"PASS: {cases.Count} packaged four-deck stages at 390x844 and 320x693 ({layouts} layouts); {samples} geometry samples / {contacts} contact samples, dense alternating wind, bag reveal/close, real witness descent and Undo. Minimum center gap {smallestGap:F1} canvas points; minimum art/timer clearance {smallestClearance:F1} logical points. Cosmetic stress leaves puzzle hash unchanged. No phone build, physical-device/performance or human-enjoyment assessment.");
                Debug.Log("CASCADE four-floor spacing review passed.");SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
            }finally{camera.targetTexture=old;if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}}
        }
    }
}
