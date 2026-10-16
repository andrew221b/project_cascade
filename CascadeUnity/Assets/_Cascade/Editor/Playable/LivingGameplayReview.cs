using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Isolated native review; no player/cloud data, gameplay commands or generated art.
    public static class LivingGameplayReview
    {
        const string Folder="LivingGameplay-Oct08";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static string FilePath(string name)=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder,name));
        public static bool Running{get;private set;}
        static object Read(object owner,string name)=>owner.GetType().GetField(name,Private).GetValue(owner);
        static void Set(object owner,string name,object value)=>owner.GetType().GetField(name,Private).SetValue(owner,value);
        static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        public static void Capture()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Check(!Running&&EditorApplication.isPlaying&&g!=null&&g.ProfilePath.Contains("CascadeNavigationReview"),"Isolated native profile required.");
            Running=true;g.StartCoroutine(Run(g));
        }
        static IEnumerator Run(CascadeGame g)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath("validation.txt")));
            bool done=false;var log=new StringBuilder();
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;Set(g,"persistenceReady",false);
            g.SetUiLocale("ru");g.ReducedMotion=false;g.Preferences.EffectQuality=2;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            try{
                g.LoadNode("C03");g.ShowGame();yield return new WaitForSecondsRealtime(1);
                Check(g.Interaction.CanPlay,"Prepared review round must be playable.");
                g.Machine.SetAmbientPaused(true);var world=g.Machine.LivingWorld;
                var life=g.Machine.GetComponent<WorldLifeView>();var extra=g.Machine.GetComponentInChildren<WorldLifeExtraView>();
                var rigs=(Array)Read(life,"creatures");string hash=g.Interaction.Attempt.CurrentStateHash;
                Check(rigs.Length==4,"Four pooled world residents expected.");
                g.SetPresentationFeedback(UiCopy.Raw("Drag a piece onto its neighbour."));
                yield return new WaitForSecondsRealtime(3.5f);
                Check(((CanvasGroup)Read(g,"instructionOpacity")).alpha==0,"Legacy top coaching must stay hidden after its old delay.");
                float bestPhase=0;int best=-1;var birds=new HashSet<WildlifeSpecies>();var insects=new HashSet<WildlifeSpecies>();
                for(int zone=0;zone<9;zone++){
                    Check(WorldLifeView.BirdSpecies(zone,false)!=WorldLifeView.BirdSpecies(zone,true),"Two distinct birds per world.");
                    g.Machine.SetZone(zone);int peak=0;
                    for(int step=0;step<300;step++){
                        float phase=step*.4f;Pose(phase,true);
                        int active=life.ActiveCreatureCount+life.ExtraCreatureCount;peak=Math.Max(peak,active);Check(active<=7,"Bounded seven-rig pool.");
                        foreach(var rig in rigs){
                            var type=rig.GetType();if(!(bool)type.GetProperty("Visible").GetValue(rig))continue;
                            var species=(WildlifeSpecies)type.GetProperty("Species").GetValue(rig);
                            if((int)species<8)birds.Add(species);else insects.Add(species);
                        }
                        if(zone==0){int score=VisibleCount(rigs)+VisibleCount((Array)Read(extra,"visitors"));
                            bool pair=Visible(rigs.GetValue(0))&&Visible(rigs.GetValue(1));score+=pair?8:0;
                            if(score>best){best=score;bestPhase=phase;}}
                    }
                    Check(peak>=3,"Visitors must populate each world intermittently.");
                    world.ReducedMotion=true;world.Paint(60);Check(life.ActiveCreatureCount+life.ExtraCreatureCount==0,"Reduced Motion hides visitors.");world.ReducedMotion=false;
                    world.EffectQuality=0;world.Paint(60);Check(life.ActiveCreatureCount+life.ExtraCreatureCount==0,"Effects Off hides visitors.");world.EffectQuality=2;
                    log.AppendLine("PASS world "+zone+": two bird species, intermittent residents, bounded pool, motion/effects toggles.");
                }
                Check(birds.Count>=6&&insects.Count>=4,"Worlds retain distinct bird/insect families.");
                g.Machine.SetZone(0);yield return Shot("day-life.png",390,844,x=>Pose(bestPhase,true));
                yield return Shot("day-life-small.png",320,693,x=>Pose(bestPhase,true));
                yield return Shot("day-life-later.png",390,844,x=>Pose(bestPhase+1,true));
                var legend=g.GetComponentInChildren<PlatformLegend>();var rect=(RectTransform)legend.transform;
                Check(Mathf.Abs(rect.rect.height*rect.lossyScale.y-g.Machine.FloorFaceHeight(0))<.001f,"Plaque height matches timber.");
                Check(Vector3.Distance(rect.position,new Vector3(g.Machine.SurfacePoint(0,.5f,.30f).x,g.Machine.SurfacePoint(0,.5f,.30f).y,rect.position.z))<.001f,"Plaque follows timber centre.");
                float low=1,high=0;var slots=new HashSet<int>();WildlifePerchTarget? previous=null;
                for(int i=0;i<80;i++){
                    float seed=WildlifeMotion.Hash(i*139+47);
                    if(g.Machine.TryChooseWildlifePerch(0,seed,false,out var target,previous)){
                        Check(target.Kind==WildlifePerchKind.EmptyPlatform,"Occupied supports excluded before long idle.");
                        low=Math.Min(low,target.X);high=Math.Max(high,target.X);
                        if(previous.HasValue&&target.Index==previous.Value.Index)Check(Mathf.Abs(target.X-previous.Value.X)>=.16f,"Nearby landing must not repeat.");previous=target;
                    }
                    if(g.Machine.TryChooseWildlifePerch(2,seed,true,out var shape)&&shape.Kind==WildlifePerchKind.Shape)slots.Add(shape.Index);
                }
                Check(high-low>.4f&&slots.Count>=2,"Different wood positions and shapes must be eligible.");
                log.AppendLine("PASS fitted proportional plaque; varied landing positions and shape indices; consecutive nearby supports excluded.");
                var perch=Read(life,"perch");Set(life,"perchZone",0);Set(perch,"revision",g.Machine.WildlifeRevision);Set(perch,"nextVisit",0f);
                Pose(200,false);Pose(201.71f,false);Pose(202.2f,false);
                Check(life.BirdState==WildlifePerchState.Resting,"Bird lands and folds its wings.");
                yield return Shot("perched.png",390,844,x=>Pose(202.2f,false));
                g.Machine.NotifyWildlifeInput();Set(g.Machine,"windTime",202.21f);world.Paint(202.21f);
                Check(life.BirdState==WildlifePerchState.Departing,"Input startles the resting bird.");
                Set(g.Machine,"windTime",204f);world.Paint(204);Check(life.BirdState==WildlifePerchState.None,"Departure releases its slot.");
                g.Machine.SetZone(8);yield return Shot("night-life.png",390,844,x=>Pose(bestPhase,true));
                // Required-action finger remains independent of the removed coaching strip.
                var profile=(PlayerProfile)Read(g,"profile");profile.SeenMechanics.Remove(MechanicLesson.Swap);
                typeof(CascadeGame).GetMethod("BeginLessonPractice",Private).Invoke(g,new object[]{MechanicLesson.Swap});
                yield return new WaitForSecondsRealtime(.7f);
                Check(g.PendingLesson==MechanicLesson.Swap&&g.Interaction.PracticeRestricted,"First-use real action stays restricted.");
                Check(g.IsTutorialHandVisible,"Guided finger remains visible without top prose.");
                yield return Shot("finger-only.png",390,844,x=>Pose(bestPhase,true));
                Check(((CanvasGroup)Read(g,"instructionOpacity")).alpha==0,"Finger does not restore top coaching text.");
                Check(hash==g.Interaction.Attempt.CurrentStateHash,"Ambient/UI review must not change the puzzle.");
                log.AppendLine("PASS input departure, guided finger/action gate and unchanged puzzle hash. Native 390/320 day, night, perch and finger captures.");
                typeof(CascadeGame).GetMethod("ClearLessonPractice",Private).Invoke(g,null);
                g.Machine.SetZone(0);yield return Film(bestPhase);
                File.WriteAllText(FilePath("validation.txt"),log.ToString());done=true;

                void Pose(float phase,bool touch){
                    Set(g.Machine,"windTime",phase);if(touch)g.Machine.NotifyWildlifeInput();else Set(g.Machine,"wildlifeLastInput",phase-12);
                    world.SetQuietFlight(false);world.Paint(phase);
                }
                IEnumerator Film(float phase){
                    var camera=Camera.main;var scaler=g.GetComponentInChildren<CanvasScaler>();var old=camera.targetTexture;var active=RenderTexture.active;
                    var render=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
                    try{
                        scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;yield return null;g.RefreshLayout();yield return null;g.RefreshLayout();
                        Directory.CreateDirectory(FilePath("frames"));
                        for(int f=0;f<144;f++){
                            Pose(phase+f/24f,true);camera.Render();RenderTexture.active=render;
                            pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();
                            File.WriteAllBytes(FilePath("frames/"+f.ToString("D4")+".png"),pixels.EncodeToPNG());
                            if(f%2==0)yield return null;
                        }
                    }finally{
                        RenderTexture.active=active;scaler.enabled=false;camera.targetTexture=old;scaler.enabled=true;g.RefreshLayout();render.Release();UnityEngine.Object.Destroy(render);UnityEngine.Object.Destroy(pixels);
                    }
                }
            }finally{
                Running=false;g.Machine.SetAmbientPaused(false);g.SetEditorSafeInsets(Vector4.zero);g.ShowHome();
                if(!done)File.WriteAllText(FilePath("failure.txt"),"Interrupted; inspect native Console.");
            }
        }
        public static void CaptureLessonInset()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            Check(EditorApplication.isPlaying&&g!=null&&g.ProfilePath.Contains("CascadeNavigationReview"),"Isolated lesson capture required.");
            g.StartCoroutine(LessonInset(g));
        }
        static IEnumerator LessonInset(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            g.Feedback.MusicEnabled=g.Feedback.SoundEnabled=false;Set(g,"persistenceReady",false);
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.ReducedMotion=false;g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            g.LoadNode("C03");g.ShowGame();yield return new WaitForSecondsRealtime(.6f);
            typeof(CascadeGame).GetMethod("OpenMechanicPopup",Private).Invoke(g,new object[]{MechanicLesson.Swap,null,default(UiCopy),null});
            yield return new WaitForSecondsRealtime(.45f);
            yield return Shot("lesson-inset.png",390,844,_=>{
                var film=UnityEngine.Object.FindFirstObjectByType<LessonFilm>();film.enabled=false;
                Set(film,"elapsed",1.2f);typeof(LessonFilm).GetMethod("Paint",Private).Invoke(film,null);
            });
            typeof(CascadeGame).GetMethod("CloseMechanicPopup",Private).Invoke(g,null);
            g.ShowHome();g.SetEditorSafeInsets(Vector4.zero);
        }
        static IEnumerator Shot(string name,int w,int h,Action<CascadeGame> pose)=>PlayableCapture.CaptureSettled(Folder,name,w,h,pose);
        static bool Visible(object rig){
            var t=rig.GetType();if(!(bool)t.GetProperty("Visible").GetValue(rig))return false;
            var root=(Transform)t.GetField("Root").GetValue(rig);var renderer=root.GetComponentInChildren<MeshRenderer>();
            var p=Camera.main.WorldToViewportPoint(root.position);var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);
            return p.x>.015f&&p.x<.985f&&block.GetColor("_BaseColor").a>.35f;
        }
        static int VisibleCount(Array rigs){int n=0;foreach(var rig in rigs)if(Visible(rig))n++;return n;}
    }
}
