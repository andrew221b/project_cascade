using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable {
public static class WorldMenuMotionCapture {
public static IEnumerator Run(CascadeGame game){
var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var old=camera.targetTexture;var active=RenderTexture.active;float oldTime=Time.captureDeltaTime;int oldRate=UnityEngine.Application.targetFrameRate;
string folder=Path.Combine(Path.GetTempPath(),"cascade-world-menu-frames");Directory.CreateDirectory(folder);var target=new RenderTexture(360,640,24);var pixels=new Texture2D(360,640,TextureFormat.RGB24,false);string hash=game.Interaction.Attempt.CurrentStateHash;float error=0;
try{UnityEngine.Application.targetFrameRate=60;Time.captureDeltaTime=1f/60;scaler.enabled=false;camera.targetTexture=target;scaler.enabled=true;yield return null;game.RefreshLayout();
int frame=0;foreach(int world in new[]{1,6,8}){WorldMenusReview.ConfigureWorld(game,world);if(world==1)game.ShowJourney();else game.ShowHome();yield return new WaitForSecondsRealtime(.45f);game.RefreshLayout();yield return null;
var env=game.GetComponentsInChildren<MenuWorldEnvironment>().Single();var map=game.GetComponentsInChildren<MapReferenceAmbiance>().SingleOrDefault();var leaves=game.GetComponentsInChildren<MenuLeafDrift>();foreach(var leaf in leaves)leaf.SetReviewMode(true);float start=env.AnimationAge;float mapStart=map==null?0:map.AnimationAge;
for(int i=0;i<180;i++){yield return null;env.SetReviewAge(start+i/60f);if(map!=null)map.SetReviewAge(mapStart+i/60f);foreach(var leaf in leaves)leaf.AdvanceReview(1f/60);Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,360,640),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,(frame++).ToString("D4")+".png"),pixels.EncodeToPNG());}
env.EndReview();if(map!=null)map.EndReview();foreach(var leaf in leaves)leaf.SetReviewMode(false);
}
if(hash!=game.Interaction.Attempt.CurrentStateHash||error>.001f)throw new InvalidOperationException("Atmosphere changed puzzle or detached bridge");File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports","WorldMenusOct14","motion-native.json"),"{\"frames\":540,\"fps\":60,\"duration_seconds\":9,\"bridge_error\":"+error.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"puzzle_unchanged\":true}");
}finally{Time.captureDeltaTime=oldTime;UnityEngine.Application.targetFrameRate=oldRate;scaler.enabled=false;camera.targetTexture=old;scaler.enabled=true;RenderTexture.active=active;game.RefreshLayout();UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);}
}
}
}
