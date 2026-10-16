using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class WildlifePerchReview
    {
        const string Folder="WildlifePerch-Oct08";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static string Output=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Folder);
        public static void Prepare()=>WorldAtmosphereReview.Prepare();
        public static void Capture()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeWorldAtmosphereReview"))throw new InvalidOperationException("Isolated review profile required.");
            g.StartCoroutine(Review(g));
        }
        static IEnumerator Review(CascadeGame g)
        {
            Directory.CreateDirectory(Output);string failure=Path.Combine(Output,"failure.txt");if(File.Exists(failure))File.Delete(failure);
            bool done=false;int oldRate=UnityEngine.Application.targetFrameRate;
            var log=new StringBuilder();
            try{
                UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=24;
                g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
                g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;g.ReducedMotion=false;g.Preferences.EffectQuality=2;
                var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
                foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
                if(!profile.Monetization.Cosmetics.Contains("bee_suit"))profile.Monetization.Cosmetics.Add("bee_suit");
                profile.FirstGestureShown=true;typeof(CascadeGame).GetField("firstGestureShown",Private).SetValue(g,true);
                typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(g,false);
                g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");
                g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,"bee_suit"));
                foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(320,693)}){
                    yield return PlayableCapture.CaptureSettled(Folder,"bee-paper-"+size.x+".png",size.x,size.y,game=>{
                        var label=Array.Find(game.GetComponentsInChildren<TextMeshProUGUI>(),t=>t.name=="Selected object title");
                        label.ForceMeshUpdate();Check(label.textInfo.lineCount==1&&!label.isTextOverflowing,"Single-line costume name");
                        Check(label.transform.parent.name=="Trophy name papyrus","Authored paper backing");
                    });
                }
                g.LoadNode("C03");g.ShowGame();g.Machine.SetZone(0);yield return null;yield return null;
                if(g.MechanicPopupOpen)g.AcknowledgeMechanic();
                typeof(CascadeGame).GetMethod("ClearLessonPractice",Private).Invoke(g,null);
                g.Interaction.SetPaused(false);g.Interaction.SetSuspended(false);
                g.Machine.SetAmbientPaused(true);Set(g.Machine,"nextAmbientGust",100000f);
                var life=g.Machine.GetComponent<WorldLifeView>();var world=g.Machine.LivingWorld;
                string hash=g.Interaction.Attempt.CurrentStateHash;
                Check(g.Machine.WildlifeCanLand,"Quiet gameplay accepts perching");
                for(int kind=0;kind<4;kind++){
                    Reset(kind,100);Pose(100);Check(life.BirdState==WildlifePerchState.Approaching&&life.BirdSupportKind==(WildlifePerchKind)kind,"Approach support "+kind);
                    Pose(101.71f);Pose(102.2f);Check(life.BirdState==WildlifePerchState.Resting,"Rest support "+kind);
                    var feet=life.BirdFeet;Pose(102.2f);Check((feet-life.BirdFeet).sqrMagnitude<.000001f,"Stopped phase retains foot contact");
                    int k=kind;
                    yield return PlayableCapture.CaptureSettled(Folder,"support-"+kind+".png",390,844,game=>Pose(102.2f));
                    g.Machine.NotifyWildlifeInput();Pose(102.21f);Check(life.BirdState==WildlifePerchState.Departing,"Input takes off from support "+k);
                    Pose(103.5f);Check(life.BirdState==WildlifePerchState.None,"Takeoff ends without another landing");
                    log.AppendLine("PASS support "+(WildlifePerchKind)kind+": approach, rest, fixed feet at stopped phase, input departure and cooldown.");
                }
                Reset(2,200);Set(g.Machine,"wildlifeLastInput",191f);Pose(200);
                Check(life.BirdSupportKind!=WildlifePerchKind.Shape&&life.BirdSupportKind!=WildlifePerchKind.Board&&life.BirdSupportKind!=WildlifePerchKind.Receiver,"Occupied supports wait for eleven seconds");
                Reset(2,210);Pose(210);Pose(211.71f);Pose(212.2f);g.Machine.CueWind(true,.65f);Pose(212.21f);
                Check(life.BirdState==WildlifePerchState.Departing,"A gust startles a perched bird");
                Set(g.Machine,"gustStarted",-100f);
                Reset(0,220);Pose(220);Pose(221.71f);Pose(222.2f);g.Machine.FractureFloor(1,.5f);Pose(222.21f);
                Check(life.BirdState==WildlifePerchState.Departing,"A fractured support cannot keep a bird");g.Machine.FractureFloor(1,0);
                log.AppendLine("PASS occupied-support idle gate, gust and disappearing support; puzzle hash unchanged.");
                yield return Film();
                Check(hash==g.Interaction.Attempt.CurrentStateHash,"Presentation never changes puzzle state");
                File.WriteAllText(Path.Combine(Output,"complete.txt"),log.ToString());done=true;

                void Reset(int preferred,float phase){
                    var perch=typeof(WorldLifeView).GetField("perch",Private).GetValue(life);
                    perch.GetType().GetMethod("Cancel").Invoke(perch,new object[]{phase});
                    Set(perch,"visit",preferred);Set(perch,"revision",g.Machine.WildlifeRevision);Set(perch,"nextVisit",0f);
                    Set(life,"perchZone",0);Set(g.Machine,"windTime",phase);Set(g.Machine,"wildlifeLastInput",phase-12);
                }
                void Pose(float phase){Set(g.Machine,"windTime",phase);world.SetQuietFlight(false);world.Paint(phase);}
                IEnumerator Film(){
                    Reset(2,300);
                    var scaler=g.GetComponentInChildren<CanvasScaler>();var main=Camera.main;
                    var oldTarget=main.targetTexture;var oldActive=RenderTexture.active;
                    var viewport=new RenderTexture(390,844,24);var render=new RenderTexture(600,450,24);
                    var pixels=new Texture2D(600,450,TextureFormat.RGB24,false);
                    var obj=new GameObject("Temporary perch review camera");var zoom=obj.AddComponent<Camera>();zoom.CopyFrom(main);zoom.enabled=false;
                    try{
                        scaler.enabled=false;main.targetTexture=viewport;scaler.enabled=true;yield return null;g.RefreshLayout();yield return null;g.RefreshLayout();
                        Reset(2,300);Pose(300);
                        var perch=typeof(WorldLifeView).GetField("perch",Private).GetValue(life);
                        var support=(WildlifePerchTarget)perch.GetType().GetField("target",Private).GetValue(perch);
                        g.Machine.TryWildlifeSupport(support,out var focus,out _);
                        zoom.orthographic=true;zoom.orthographicSize=.8f;zoom.aspect=600f/450;zoom.targetTexture=render;
                        zoom.transform.position=new Vector3(focus.x,focus.y+.30f,main.transform.position.z);
                        string frames=Path.Combine(Output,"frames");Directory.CreateDirectory(frames);
                        for(int frame=0;frame<192;frame++){
                            float phase=300+frame/24f;
                            if(frame==108)g.Machine.NotifyWildlifeInput();
                            Pose(phase);zoom.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,600,450),0,0);pixels.Apply();
                            File.WriteAllBytes(Path.Combine(frames,frame.ToString("D4")+".png"),pixels.EncodeToPNG());
                            if(frame%4==0)yield return null;
                        }
                    }finally{
                        RenderTexture.active=oldActive;scaler.enabled=false;main.targetTexture=oldTarget;scaler.enabled=true;g.RefreshLayout();
                        UnityEngine.Object.Destroy(obj);render.Release();viewport.Release();UnityEngine.Object.Destroy(render);UnityEngine.Object.Destroy(viewport);UnityEngine.Object.Destroy(pixels);
                    }
                }
            }finally{
                UnityEngine.Application.targetFrameRate=oldRate;g.Machine.SetAmbientPaused(false);g.SetEditorSafeInsets(Vector4.zero);g.ShowHome();
                if(!done)File.WriteAllText(failure,"Review interrupted; inspect native Unity Console.");
            }
        }
        static void Set(object owner,string field,object value)=>owner.GetType().GetField(field,Private).SetValue(owner,value);
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException("Wildlife perch review: "+message);}
    }
}
