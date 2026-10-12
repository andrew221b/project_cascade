using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
namespace Cascade.Editor.Playable
{
    public static class MapInputReview
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static string Folder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/MapInput"));
        static object Get(CascadeGame g,string key)=>typeof(CascadeGame).GetField(key,Flags).GetValue(g);
        static void Set(CascadeGame g,string key,object value)=>typeof(CascadeGame).GetField(key,Flags).SetValue(g,value);
        static object Call(CascadeGame g,string key,params object[] args)=>typeof(CascadeGame).GetMethod(key,Flags).Invoke(g,args);
        public static string Status="Not started";
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);File.AppendAllText(Path.Combine(Folder,"checks.txt"),"PASS "+message+"\n");}
        public static void Run(){var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated QA profile required");Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"checks.txt"),"");File.WriteAllText(Path.Combine(Folder,"status.txt"),"Running");g.StartCoroutine(Inspect(g));}
        static IEnumerator Inspect(CascadeGame g)
        {
            Status="Running";UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.SetUiLocale("ru");g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            var profile=(PlayerProfile)Get(g,"profile");profile.Completion=null;
            foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
            var checkpoint=JsonConvert.DeserializeObject<DescentRun>(JsonConvert.SerializeObject(g.Descent));checkpoint.Completed.Clear();
            var source=g.Catalog.PrimaryLevels.First(c=>c.Definition.ContentHash==checkpoint.SourceHash).Definition;
            foreach(float above in new[]{0f,18f}){
                g.LoadLevel(g.Catalog.GetPrimary("C01").Definition);g.ShowGame();yield return Ready();
                var camera=Camera.main;var start=(Vector2)camera.WorldToScreenPoint(g.Machine.TokenVisualPosition(0)+g.Machine.TokenBodyCentre(0));
                var target=(Vector2)camera.WorldToScreenPoint(g.Machine.TokenVisualPosition(1)+g.Machine.TokenBodyCentre(1))+Vector2.up*above;
                Require(g.TokenAtScreenPoint(target,-1,0)==1,"body hit at offset "+above);
                var pointer=new PointerEventData(EventSystem.current){pointerId=22,position=start,button=PointerEventData.InputButton.Left};
                g.SlotViews[0].OnPointerDown(pointer);pointer.position=target;g.SlotViews[0].OnBeginDrag(pointer);g.SlotViews[0].OnDrag(pointer);g.SlotViews[0].OnPointerUp(pointer);
                Require(g.Interaction.Attempt.RouteLength==1,"one atomic swap with empty UI raycast at offset "+above);yield return Ready();
            }
            g.LoadLevel(g.Catalog.GetPrimary("C01").Definition);g.ShowGame();yield return Ready();
            string hash=g.Interaction.Attempt.CurrentStateHash;g.SendMessage("TogglePause");g.ShowGame();Require(g.CurrentScreen==GameScreen.Playing&&hash==g.Interaction.Attempt.CurrentStateHash,"pause on active puzzle stays Playing");
            Require(new ProfileStore(g.ProfilePath).Load().Profile.SeenCoachPrompts.Contains("Drag a piece onto its neighbour."),"successful swap learns instruction on disk");
            Call(g,"RetireInstruction","Tap Descend to take the row down.");
            Require(new ProfileStore(g.ProfilePath).Load().Profile.SeenCoachPrompts.Contains("Tap Descend to take the row down."),"dismissed instruction saved to disk");
            g.LoadLevel(g.Catalog.GetPrimary("C02").Definition);g.ShowGame();yield return Ready();
            Require(((System.Collections.Generic.HashSet<string>)Get(g,"dismissedInstructions")).Contains("Drag a piece onto its neighbour."),"dismissal survives next level");
            var attempt=new AttemptService(checkpoint.Level(source,0));
            foreach(string token in checkpoint.Stages[0].Witness){var parts=token.TrimStart('R').Split(':');var command=token=="D"?PuzzleCommand.Drop:token.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(parts[0]),int.Parse(parts[1])):PuzzleCommand.Swap(int.Parse(token));Require(attempt.Execute(command).Accepted,"checkpoint witness "+token);}
            Call(g,"MountAttempt",attempt,checkpoint);g.ShowGame();yield return Ready();
            Require(g.CurrentScreen==GameScreen.Checkpoint,"actual solved stage gives Checkpoint");hash=g.Interaction.Attempt.CurrentStateHash;
            g.SendMessage("TogglePause");g.ShowGame();yield return null;g.RefreshLayout();
            var undo=(Button)Get(g,"undo");var root=(RectTransform)Get(g,"resultRoot");var map=(RectTransform)root.Find("Resulthome");
            Require(g.CurrentScreen==GameScreen.Checkpoint&&hash==g.Interaction.Attempt.CurrentStateHash,"checkpoint pause/resume preserves completed stage");
            Require(Mathf.Abs(((RectTransform)undo.transform).rect.height-map.rect.height)<.1f,"Undo and Map have equal height after resume");
            yield return PlayableCapture.CaptureSettled("MapInput","checkpoint-resumed.png",390,844);
            profile.Completion=null;g.ShowJourney();yield return new WaitForSecondsRealtime(.5f);
            var menu=(RectTransform)Get(g,"menuRoot");var rest=menu.anchoredPosition;
            foreach(var mode in new[]{PuzzleMode.Words,PuzzleMode.Mixed,PuzzleMode.Shapes}){
                g.SelectMode(mode);Require(g.PreferredMode==mode,"switch to "+mode);Require(Vector2.Distance(menu.anchoredPosition,rest)<.01f,"mode switch has no page slide "+mode);yield return null;
            }
            yield return PlayableCapture.CaptureSettled("MapInput","map-ru.png",390,844);
            var scenery=g.GetComponentInChildren<SoftSceneryGraphic>();var surface=scenery.rectTransform;float wanted=scenery.GetComponent<AspectRatioFitter>().aspectRatio;
            Require(Mathf.Abs(surface.rect.width/surface.rect.height-wanted)<.002f,"map respects artwork aspect");
            var island=g.GetComponentsInChildren<MapIslandAnchor>().First();var before=island.transform.localPosition;
            var cloud=g.GetComponentsInChildren<RectTransform>().First(r=>r.name=="Map cloud 0");var cloudBefore=cloud.localPosition;
            yield return new WaitForSecondsRealtime(1.5f);Require(Vector3.Distance(before,island.transform.localPosition)>.01f,"island floats");Require(Vector3.Distance(cloudBefore,cloud.localPosition)>.1f,"cloud drifts");
            yield return PlayableCapture.CaptureSettled("MapInput","map-motion.png",390,844);
            g.Preferences.LargeText=true;g.SetUiLocale("uk");yield return PlayableCapture.CaptureSettled("MapInput","map-uk-small.png",320,693);
            Set(g,"mapPage",3);Call(g,"DrawMenu");yield return PlayableCapture.CaptureSettled("MapInput","map-world4.png",390,844);
            g.ReducedMotion=true;yield return new WaitForEndOfFrame();Require(g.GetComponentsInChildren<MapIslandAnchor>().Length==5&&g.GetComponentsInChildren<MapIslandAnchor>().All(a=>a.transform.localPosition.sqrMagnitude<.0001f),"reduced motion stops island drift");g.ReducedMotion=false;
            Status="PASS";File.WriteAllText(Path.Combine(Folder,"status.txt"),Status);Debug.Log("CASCADE MapInput review PASS");
            IEnumerator Ready(){float end=Time.realtimeSinceStartup+20;while(g.Interaction.IsPreparing||g.Interaction.IsPresenting||g.MechanicPopupOpen){if(g.MechanicPopupOpen)g.AcknowledgeMechanic();if(Time.realtimeSinceStartup>end)throw new InvalidOperationException("Input readiness timeout");yield return null;}yield return new WaitForSecondsRealtime(.15f);}
        }
    }
}
