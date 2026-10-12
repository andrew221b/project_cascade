using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Cascade.Presentation;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class TriadWildlifeReview
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated review required.");
            game.StartCoroutine(Review(game));
        }
        static IEnumerator Review(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.Preferences.Tutorials=false;
            game.Preferences.EffectQuality=2;game.Preferences.LargeText=false;game.ReducedMotion=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.LoadNode("C03");game.ShowGame();
            float deadline=Time.realtimeSinceStartup+30;
            while(!game.Interaction.CanControl&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(game.Interaction.CanPlay,"Playable quiet fixture");
            game.Machine.SetZone(0);game.Machine.SetAmbientPaused(true);Set(game.Machine,"nextAmbientGust",100000f);
            var life=game.Machine.GetComponent<WorldLifeView>();var world=game.Machine.LivingWorld;
            var perch=typeof(WorldLifeView).GetField("perch",Hidden).GetValue(life);
            string hash=game.Interaction.Attempt.CurrentStateHash;
            Reset(100);Set(game.Machine,"wildlifeLastInput",96f);Pose(100);
            Check(life.BirdState==WildlifePerchState.Approaching,"Bird approaches after four idle seconds");
            Check(life.BirdSupportKind==WildlifePerchKind.EmptyPlatform||life.BirdSupportKind==WildlifePerchKind.Receiver,"Short idle permits only safe supports");
            Pose(101.71f);Pose(102.2f);Check(life.BirdState==WildlifePerchState.Resting,"Bird lands");
            yield return PlayableCapture.CaptureSettled("LevelTriads-Oct08","wildlife-resting.png",390,844,g=>Pose(102.2f));
            game.Machine.NotifyWildlifeInput();Pose(102.21f);Check(life.BirdState==WildlifePerchState.Departing,"Input prompts departure");
            yield return PlayableCapture.CaptureSettled("LevelTriads-Oct08","wildlife-departing.png",390,844,g=>Pose(102.65f));
            // Sample the same native pooled actors over a quiet minute, without allocating actors or commands.
            Reset(200);int max=0,present=0;
            for(int frame=0;frame<240;frame++){
                float clock=200+frame*.25f;Pose(clock);int count=life.ActiveCreatureCount+life.ExtraCreatureCount;
                max=Math.Max(max,count);if(count>0)present++;
            }
            Check(max<=7&&max>=3,"Existing pool has multiple visitors and retains its cap");
            Check(hash==game.Interaction.Attempt.CurrentStateHash,"Wildlife never changes the puzzle");
            File.WriteAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/LevelTriads-Oct08/wildlife-native.txt")),
                "PASS: safe-support landing after four idle seconds, rest, input departure, unchanged puzzle hash. Native minute sample: maximum "+max+" actors, presence "+present+"/240 samples. No physical-device FPS claim.\n");
            game.Machine.SetAmbientPaused(false);
            void Reset(float clock){perch.GetType().GetMethod("Cancel").Invoke(perch,new object[]{clock});Set(perch,"revision",game.Machine.WildlifeRevision);Set(perch,"nextVisit",0f);Set(life,"perchZone",0);Set(game.Machine,"windTime",clock);Set(game.Machine,"wildlifeLastInput",clock-12);}
            void Pose(float clock){Set(game.Machine,"windTime",clock);world.SetQuietFlight(false);world.Paint(clock);}
        }
        static void Set(object owner,string field,object value)=>owner.GetType().GetField(field,Hidden).SetValue(owner,value);
        static void Check(bool condition,string description){if(!condition)throw new InvalidOperationException("Triad wildlife: "+description);}
    }
}
