using System;
using System.Collections;
using System.IO;
using Cascade.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Bounded native render review; does not use or write the player's profile.
    public static class AtmosphereCapture
    {
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(game==null)throw new InvalidOperationException("Requires running CASCADE.");
            game.StartCoroutine(Review(game));
        }
        static IEnumerator Review(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=true;game.ReducedMotion=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.LoadNode("C02");
            yield return null;yield return null;
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            var render=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","Atmosphere");
            var world=game.Machine.LivingWorld;string hash=game.Interaction.Attempt.CurrentStateHash;
            float initialPhase=world.Phase;var island=world.IslandPosition(0);var scale=world.GliderScale;yield return new WaitForSecondsRealtime(1);
            string movement="Real-time phase="+initialPhase.ToString("F6")+" -> "+world.Phase.ToString("F6")+"; island motion="+(world.IslandPosition(0)-island).ToString("F6")+"; glider scale change="+(world.GliderScale-scale).ToString("F6")+"\n";
            try {
                scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;
                yield return null;game.RefreshLayout();yield return null;game.RefreshLayout();
                game.Machine.SetAmbientPaused(true);world.SetQuietFlight(false);
                foreach(var pose in new[]{(name:"phone-near.png",phase:0f),(name:"phone-turn.png",phase:15f),(name:"phone-far.png",phase:Mathf.PI/.08f)}) {
                    world.Paint(pose.phase);camera.Render();RenderTexture.active=render;
                    pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,pose.name),pixels.EncodeToPNG());
                }
            } finally {
                game.Machine.SetAmbientPaused(false);camera.targetTexture=oldTarget;RenderTexture.active=oldActive;
                UnityEngine.Object.Destroy(render);UnityEngine.Object.Destroy(pixels);game.RefreshLayout();
            }
            game.SetEditorSafeInsets(Vector4.zero);
            yield return PlayableCapture.CaptureSettled("Atmosphere","wide.png",1440,900,g=>world.Paint(10));
            yield return new WaitForSecondsRealtime(1.8f);
            movement+="Score="+game.Feedback.CurrentMusicClip.name+"; crossfade="+game.Feedback.MusicCrossfade+"; wind volume="+game.Feedback.WindVolume+"\n";
            game.ShowHome();yield return new WaitForSecondsRealtime(1.8f);
            movement+="Menu score="+game.Feedback.CurrentMusicClip.name+"; unchanged puzzle hash="+(hash==game.Interaction.Attempt.CurrentStateHash)+"\n";
            File.WriteAllText(Path.Combine(folder,"runtime.txt"),movement+"Native Editor render and engine checks only. No phone build, device listening or performance proof. Flight snapshots use explicit orbit phases.\n");
            Debug.Log("CASCADE atmosphere capture complete.");
        }
    }
}
