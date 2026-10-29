#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed partial class AccountReferenceTests
    {
        const string SpeechReport="CharacterSpeechOct16";
        string SpeechFolder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports",SpeechReport));
        [Serializable] public sealed class SpeechFaceEntry { public string name,source,sheet;public int index,columns,rows;public float aspect;public Vector4 rect,muzzle,opening; }
        [Serializable] public sealed class SpeechFaceBook { public SpeechFaceEntry[] faces; }
        static SpeechFaceEntry SpeechFace(ActorArt art,string sheet,int index,int columns,int rows)
        {
            var p=CharacterPoseProfile.For(art.Sprite);float eyeLeft=Mathf.Min(p.NearEye.x-p.NearEye.z,p.FarEye.x-p.FarEye.z),eyeRight=Mathf.Max(p.NearEye.x+p.NearEye.z,p.FarEye.x+p.FarEye.z);
            float eyeY=Mathf.Max(p.NearEye.y,p.FarEye.y),span=eyeRight-eyeLeft;
            float muzzleX=Mathf.Max(p.Mouth.z*1.65f,span*.70f),muzzleY=Mathf.Max(p.Mouth.w*2.35f,span*.58f);
            float left=Mathf.Min(eyeLeft-.075f,p.Mouth.x-muzzleX-.03f),right=Mathf.Max(eyeRight+.075f,p.Mouth.x+muzzleX+.03f);
            float top=Mathf.Min(p.NearEye.y-p.NearEye.w*1.8f,p.FarEye.y-p.FarEye.w*1.8f)-.04f,bottom=Mathf.Max(p.Neck.y+.065f,p.Mouth.y+muzzleY+.04f);
            float size=Mathf.Max((right-left)*p.Cell.width,(bottom-top)*p.Cell.height),cx=(left+right)*.5f,cy=(top+bottom)*.5f;
            float width=size/p.Cell.width,height=size/p.Cell.height;left=Mathf.Clamp(cx-width*.5f,0,1-width);top=Mathf.Clamp(cy-height*.5f,0,1-height);
            return new SpeechFaceEntry{name=p.Name,source=art.Name,sheet=sheet,index=index,columns=columns,rows=rows,aspect=p.Cell.width/p.Cell.height,rect=new Vector4(left,top,width,height),muzzle=new Vector4(p.Mouth.x,p.Mouth.y+muzzleY*.24f,muzzleX,muzzleY)};
        }
        [UnityTest] public IEnumerator SpeechFullOpenMatchesItsCompleteAuthoredJaw()
        {
            game.ShowHome();game.ReducedMotion=false;game.Preferences.EffectQuality=2;yield return null;
            var book=JsonUtility.FromJson<SpeechFaceBook>(Resources.Load<TextAsset>("CascadeArt/CharacterMotionOct16/SpeechFaceLandmarks").text);
            var oldTarget=camera.targetTexture;var oldClear=camera.clearFlags;var oldColor=camera.backgroundColor;int oldLayers=camera.cullingMask;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
            var target=new RenderTexture(320,320,24);target.Create();var pixels=new Texture2D(320,320,TextureFormat.RGB24,false);
            var host=new GameObject("Whole-jaw pixel comparison",typeof(RectTransform),typeof(Canvas));host.layer=5;var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            string folder=Path.Combine(SpeechFolder,"faces");Directory.CreateDirectory(folder);var results=new List<string>{"pose,mean_full_jaw_rgb_error,compared_pixels,half_dark_area_ratio,half_authored_rgb_error,mouth_state_difference"};var failures=new List<string>();
            Color32[] Shot(string name){Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,320,320),0,0);pixels.Apply();RenderTexture.active=active;File.WriteAllBytes(Path.Combine(folder,name+".png"),pixels.EncodeToPNG());return pixels.GetPixels32();}
            try{
                foreach(var c in canvases)c.enabled=false;camera.targetTexture=target;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.55f,.70f,.76f);camera.cullingMask=1<<5;
                foreach(var entry in book.faces){
                    var art=ActorSprites().Single(x=>x.Name==entry.source);var profile=CharacterPoseProfile.For(art.Sprite);var cell=profile.Cell;var r=entry.rect;
                    var cut=Sprite.Create(art.Sprite.texture,new Rect(cell.x+r.x*cell.width,art.Sprite.texture.height-cell.y-(r.y+r.w)*cell.height,r.z*cell.width,r.w*cell.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
                    var obj=new GameObject(entry.source,typeof(RectTransform),typeof(Image));obj.layer=5;obj.transform.SetParent(host.transform,false);var image=obj.GetComponent<Image>();image.sprite=cut;image.preserveAspect=false;image.raycastTarget=false;image.rectTransform.anchorMin=Vector2.zero;image.rectTransform.anchorMax=Vector2.one;image.rectTransform.offsetMin=image.rectTransform.offsetMax=Vector2.zero;
                    var view=CharacterPoseView.Attach(game,image);view.ResetReview();yield return null;view.SetReviewFace(0,0);Shot(entry.source+"-closed");view.SetReviewFace(0,.5f);var half=Shot(entry.source+"-half");view.SetReviewFace(0,.70f);var intermediate=Shot(entry.source+"-transition");view.SetReviewFace(0,1);var rendered=Shot(entry.source+"-open");obj.SetActive(false);
                    var expectedObj=new GameObject("Authored complete face reference",typeof(RectTransform),typeof(RawImage));expectedObj.layer=5;expectedObj.transform.SetParent(host.transform,false);var raw=expectedObj.GetComponent<RawImage>();raw.texture=Resources.Load<Texture2D>("CascadeArt/CharacterMotionOct16/"+entry.sheet+"Open");raw.uvRect=new Rect(entry.index%entry.columns/(float)entry.columns,1-(entry.index/entry.columns+1f)/entry.rows,1f/entry.columns,1f/entry.rows);raw.rectTransform.anchorMin=Vector2.zero;raw.rectTransform.anchorMax=Vector2.one;raw.rectTransform.offsetMin=raw.rectTransform.offsetMax=Vector2.zero;var expected=Shot(entry.source+"-authored");raw.texture=Resources.Load<Texture2D>("CascadeArt/CharacterMotionOct16/"+entry.sheet+"Half");var expectedHalf=Shot(entry.source+"-authored-half");
                    float error=0,halfError=0,stateDifference=0,ghostError=0;int samples=0,stateSamples=0,darkFull=0,darkHalf=0;float top=Mathf.Max(profile.NearEye.y,profile.FarEye.y)+Mathf.Max(profile.NearEye.w,profile.FarEye.w)*(profile.SoftBrows?.40f:1.1f);
                    for(int y=0;y<320;y++)for(int x=0;x<320;x++){
                        var point=new Vector2(r.x+(x+.5f)/320*r.z,r.y+(1-(y+.5f)/320)*r.w);var delta=Vector2.Scale(point-new Vector2(entry.muzzle.x,entry.muzzle.y),new Vector2(1/entry.muzzle.z,1/entry.muzzle.w));int i=y*320+x;
                        float qx=(x+.5f)/320,qy=1-(y+.5f)/320;
                        if(delta.magnitude>.65f||point.y<top+.022f)continue;
                        if(qx>entry.opening.x&&qx<entry.opening.x+entry.opening.z&&qy>entry.opening.y&&qy<entry.opening.y+entry.opening.w){stateDifference+=Mathf.Abs(rendered[i].r-half[i].r)+Mathf.Abs(rendered[i].g-half[i].g)+Mathf.Abs(rendered[i].b-half[i].b);stateSamples++;if(Mathf.Max(rendered[i].r,Mathf.Max(rendered[i].g,rendered[i].b))<135&&rendered[i].r>rendered[i].g*1.35f)darkFull++;if(Mathf.Max(half[i].r,Mathf.Max(half[i].g,half[i].b))<135&&half[i].r>half[i].g*1.35f)darkHalf++;}
                        ghostError+=Mathf.Abs(intermediate[i].r-half[i].r)+Mathf.Abs(intermediate[i].g-half[i].g)+Mathf.Abs(intermediate[i].b-half[i].b);halfError+=Mathf.Abs(half[i].r-expectedHalf[i].r)+Mathf.Abs(half[i].g-expectedHalf[i].g)+Mathf.Abs(half[i].b-expectedHalf[i].b);error+=Mathf.Abs(rendered[i].r-expected[i].r)+Mathf.Abs(rendered[i].g-expected[i].g)+Mathf.Abs(rendered[i].b-expected[i].b);samples++;
                    }
                    Assert.That(samples,Is.GreaterThan(100));error/=samples*3*255f;halfError/=samples*3*255f;ghostError/=samples*3*255f;stateDifference/=Mathf.Max(1,stateSamples)*3*255f;results.Add(entry.source+","+error.ToString("F6",System.Globalization.CultureInfo.InvariantCulture)+","+samples+","+(darkFull>0?darkHalf/(float)darkFull:0).ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+halfError.ToString("F6",System.Globalization.CultureInfo.InvariantCulture)+","+stateDifference.ToString("F6",System.Globalization.CultureInfo.InvariantCulture));
                    if(error>.035f)failures.Add(entry.source+": full jaw error "+error.ToString("F4"));
                    if(ghostError>.008f)failures.Add(entry.source+": overlapping mouth states during articulation");
                    if(halfError>.035f)failures.Add(entry.source+": half face must match its complete authored state");
                    if(stateSamples>200&&stateDifference<.012f)failures.Add(entry.source+": half/full states must be visibly distinct");
                    UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(expectedObj);UnityEngine.Object.DestroyImmediate(cut);
                }
                File.WriteAllLines(Path.Combine(SpeechFolder,"full-jaw-comparison.csv"),results);Assert.That(failures,Is.Empty,"The complete open jaw must replace the original closed mouth: "+string.Join("; ",failures));
            }finally{camera.targetTexture=oldTarget;camera.clearFlags=oldClear;camera.backgroundColor=oldColor;camera.cullingMask=oldLayers;foreach(var c in canvases)if(c!=null)c.enabled=true;UnityEngine.Object.DestroyImmediate(host);target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
        }
        [UnityTest] public IEnumerator SpeechCaptureNaturalMotionAtPhoneScale()
        {
            game.ShowHome();game.ReducedMotion=false;game.Preferences.EffectQuality=2;yield return null;
            var book=JsonUtility.FromJson<SpeechFaceBook>(Resources.Load<TextAsset>("CascadeArt/CharacterMotionOct16/SpeechFaceLandmarks").text);
            var oldTarget=camera.targetTexture;var oldClear=camera.clearFlags;var oldColor=camera.backgroundColor;int oldLayers=camera.cullingMask;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
            var target=new RenderTexture(640,640,24);target.Create();var pixels=new Texture2D(640,640,TextureFormat.RGB24,false);var host=new GameObject("Native speaking faces",typeof(RectTransform),typeof(Canvas));host.layer=5;var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            var views=new List<CharacterPoseView>();var cuts=new List<Sprite>();string output=Path.Combine(SpeechFolder,"motion");Directory.CreateDirectory(output);float peak=0;
            try{
                foreach(var c in canvases)c.enabled=false;camera.targetTexture=target;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.55f,.70f,.76f);camera.cullingMask=1<<5;
                int i=0;foreach(string name in new[]{"speaker-Mika","speaker-Pip","speaker-Rue","speaker-Aster"}){
                    var entry=book.faces.Single(e=>e.source==name);var art=ActorSprites().Single(x=>x.Name==name);var profile=CharacterPoseProfile.For(art.Sprite);var c=profile.Cell;var r=entry.rect;
                    var cut=Sprite.Create(art.Sprite.texture,new Rect(c.x+r.x*c.width,art.Sprite.texture.height-c.y-(r.y+r.w)*c.height,r.z*c.width,r.w*c.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);cuts.Add(cut);
                    var obj=new GameObject(name,typeof(RectTransform),typeof(Image));obj.layer=5;obj.transform.SetParent(host.transform,false);var image=obj.GetComponent<Image>();image.sprite=cut;image.rectTransform.anchorMin=image.rectTransform.anchorMax=Vector2.one*.5f;image.rectTransform.sizeDelta=Vector2.one*308;image.rectTransform.anchoredPosition=new Vector2(i%2==0?-160:160,i<2?160:-160);var view=CharacterPoseView.Attach(game,image);view.ResetReview();views.Add(view);i++;
                }
                yield return null;
                for(int frame=0;frame<180;frame++){
                    float t=frame/30f;foreach(var view in views){view.AdvanceReview(1f/30,.3f,CharacterPoseView.SpeechShape("Откроем ворота, осмотрим старую пристань и узнаем, кто оставил здесь фонарь. We can open the gate, follow the lanterns and go home together.",t));peak=Mathf.Max(peak,view.Motion.MouthOpen);}
                    Canvas.ForceUpdateCanvases();camera.Render();var active=RenderTexture.active;RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,640,640),0,0);pixels.Apply();RenderTexture.active=active;File.WriteAllBytes(Path.Combine(output,frame.ToString("D4")+".jpg"),pixels.EncodeToJPG(95));if(frame%30==0)yield return null;
                }
                File.WriteAllText(Path.Combine(SpeechFolder,"natural-speech-peak.txt"),peak.ToString("F5",System.Globalization.CultureInfo.InvariantCulture));Assert.That(peak,Is.GreaterThan(.92f),"Real spoken vowels must reach a clearly full opening");
            }finally{camera.targetTexture=oldTarget;camera.clearFlags=oldClear;camera.backgroundColor=oldColor;camera.cullingMask=oldLayers;foreach(var c in canvases)if(c!=null)c.enabled=true;UnityEngine.Object.DestroyImmediate(host);foreach(var cut in cuts)UnityEngine.Object.DestroyImmediate(cut);target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
        }
        [UnityTest] public IEnumerator SpeechExportOriginalFacesAndCurrentPreview()
        {
            game.ShowHome();yield return null;var art=ActorSprites().ToArray();
            var groups=new[]{art.Where(x=>x.Name.StartsWith("speaker-",StringComparison.Ordinal)||x.Name=="thinking"||x.Name=="collection").ToArray(),art.Where(x=>x.Name.EndsWith("-hopeful",StringComparison.Ordinal)).ToArray(),art.Where(x=>x.Name.EndsWith("-thinking",StringComparison.Ordinal)).ToArray()};
            string[] sheets={"DialogueFaces","CostumeHopefulFaces","CostumeThinkingFaces"};var book=new List<SpeechFaceEntry>();
            var originalTarget=camera.targetTexture;var clear=camera.clearFlags;var color=camera.backgroundColor;int layers=camera.cullingMask;
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
            var host=new GameObject("Unmodified source face atlas",typeof(RectTransform),typeof(Canvas));host.layer=5;var canvas=host.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            Directory.CreateDirectory(SpeechFolder);
            try{
                foreach(var c in canvases)c.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<5;
                for(int group=0;group<groups.Length;group++){
                    int columns=group==0?3:4,rows=group==0?4:5,width=columns*384,height=rows*384;
                    var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);rt.Create();camera.targetTexture=rt;var pixels=new Texture2D(width,height,TextureFormat.RGBA32,false);
                    foreach(var source in groups[group].Select((item,index)=>(item,index))){
                        var entry=SpeechFace(source.item,sheets[group],source.index,columns,rows);book.Add(entry);var p=CharacterPoseProfile.For(source.item.Sprite);var cell=p.Cell;var r=entry.rect;
                        var obj=new GameObject(source.item.Name,typeof(RectTransform),typeof(RawImage));obj.layer=5;obj.transform.SetParent(host.transform,false);var raw=obj.GetComponent<RawImage>();raw.texture=source.item.Sprite.texture;raw.raycastTarget=false;
                        raw.uvRect=new Rect((cell.x+r.x*cell.width)/raw.texture.width,1-(cell.y+(r.y+r.w)*cell.height)/raw.texture.height,r.z*cell.width/raw.texture.width,r.w*cell.height/raw.texture.height);
                        raw.rectTransform.anchorMin=raw.rectTransform.anchorMax=Vector2.one*.5f;raw.rectTransform.sizeDelta=Vector2.one*384;raw.rectTransform.anchoredPosition=new Vector2((source.index%columns+.5f)*384-width*.5f,height*.5f-(source.index/columns+.5f)*384);
                    }
                    Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();RenderTexture.active=previous;
                    File.WriteAllBytes(Path.Combine(SpeechFolder,sheets[group]+"-source.png"),pixels.EncodeToPNG());
                    for(int i=host.transform.childCount-1;i>=0;i--)UnityEngine.Object.DestroyImmediate(host.transform.GetChild(i).gameObject);
                    camera.targetTexture=originalTarget;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(pixels);
                }
                File.WriteAllText(Path.Combine(SpeechFolder,"SpeechFaceLandmarks.json"),JsonUtility.ToJson(new SpeechFaceBook{faces=book.ToArray()},true));
            }finally{camera.targetTexture=originalTarget;camera.clearFlags=clear;camera.backgroundColor=color;camera.cullingMask=layers;foreach(var c in canvases)if(c!=null)c.enabled=true;UnityEngine.Object.DestroyImmediate(host);}
            // Export the current renderer alongside unmodified source face inputs.
            var output=Path.Combine(SpeechFolder,"preview");Directory.CreateDirectory(output);WelcomePhone(390,844);game.SetUiLocale("ru");
            foreach(string speaker in new[]{"Mika","Pip","Rue","Aster","Bram","Sora","Neri","Toma","Lumi","Gran"}){
                typeof(CascadeGame).GetMethod("CancelConversation",Private).Invoke(game,null);
                var line=new Cascade.Content.StoryConversation(-1,new Cascade.Content.StoryLine(speaker,"Откроем ворота, а потом найдём дорогу домой."));
                typeof(CascadeGame).GetMethod("OpenConversation",Private).Invoke(game,new object[]{line,(UiCopy)"Back to the map",(Action)(()=>{}),null});yield return null;
                var view=((Image)Field("conversationPortrait")).GetComponent<CharacterPoseView>();view.ResetReview();view.SetReviewFace(0,1);Canvas.ForceUpdateCanvases();camera.Render();
                var previous=RenderTexture.active;RenderTexture.active=portrait;var t=new Texture2D(390,844,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,390,844),0,0);t.Apply();RenderTexture.active=previous;File.WriteAllBytes(Path.Combine(output,speaker+".png"),t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
            }
        }
    }
}
#endif
