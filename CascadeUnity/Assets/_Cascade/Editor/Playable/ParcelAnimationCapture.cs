using System;
using System.Collections;
using System.IO;
using Cascade.Presentation;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class ParcelAnimationCapture
    {
        public static IEnumerator Run(CascadeGame game,ParcelRevealView view,string name)
        {
            const int width=360,height=640,fps=60;const float duration=6.55f;
            string folder=Path.Combine(Path.GetTempPath(),"cascade-parcel-"+name+"-frames");Directory.CreateDirectory(folder);
            foreach(var file in Directory.GetFiles(folder,"frame-*.png"))File.Delete(file);
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>(true);var original=camera.targetTexture;
            var active=RenderTexture.active;var target=new RenderTexture(width,height,24);target.Create();var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            string hash=game.Interaction.Attempt.CurrentStateHash;
            try{
                scaler.enabled=false;camera.targetTexture=target;scaler.enabled=true;yield return null;game.RefreshLayout();
                for(int frame=0;frame<Mathf.RoundToInt(duration*fps);frame++){
                    float t=frame/(float)fps;
                    if(t<5.1f)view.SetReviewPose(Mathf.Min(4.65f,t));else view.SetReviewPose(t-5.1f,true);
                    yield return null;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
                    texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,"frame-"+frame.ToString("D4")+".png"),texture.EncodeToPNG());
                }
                if(game.Interaction.Attempt.CurrentStateHash!=hash)throw new InvalidOperationException("Animation capture mutated puzzle state");
                File.WriteAllText(Path.Combine(folder,"capture.txt"),"Native Unity renders: 393 frames, 60 fps, 360×640; rendering did not change puzzle hash.\n");
            }finally{
                scaler.enabled=false;camera.targetTexture=original;scaler.enabled=true;RenderTexture.active=active;game.RefreshLayout();UnityEngine.Object.Destroy(texture);target.Release();UnityEngine.Object.Destroy(target);
            }
        }
    }
}
