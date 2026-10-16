using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    [InitializeOnLoad]
    public static class MapPolishReview
    {
        const string Key="Cascade.MapPolishReview",Previous="Cascade.MapPolishPrevious",Verify="Cascade.MapPolishVerify";
        const string Extras="Cascade.MapPolishExtras";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static MapPolishReview(){
            string path=SessionState.GetString(Key,"");if(path.Length>0)CascadeGame.EditorProfileDirectory=path;
            EditorApplication.playModeStateChanged+=s=>{
                if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key,"").Length>0)EditorApplication.delayCall+=Start;
                if(s==PlayModeStateChange.EnteredEditMode)Restore();
            };
        }
        [MenuItem("CASCADE/QA/Capture map polish baseline")]public static void Baseline()=>Prepare(false);
        [MenuItem("CASCADE/QA/Review map polish")]public static void Final()=>Prepare(true);
        [MenuItem("CASCADE/QA/Review map corners and safe areas")]public static void Details()=>Prepare(true,true);
        static void Prepare(bool verify,bool extras=false){
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Stable Edit Mode required.");
            string path=Path.Combine(Path.GetTempPath(),"CascadeMapPolish-"+Guid.NewGuid().ToString("N"));
            var profile=new PlayerProfile {AccountIntroSeen=true,FirstGestureShown=true};profile.Preferences.Tutorials=false;profile.Missions.NextAfterClears=20000;
            new ProfileStore(Path.Combine(path,"cascade-profile.json")).Save(profile);
            SessionState.SetString(Previous,CascadeGame.EditorProfileDirectory??"");SessionState.SetString(Key,path);SessionState.SetBool(Verify,verify);SessionState.SetBool(Extras,extras);
            CascadeGame.EditorProfileDirectory=path;EditorApplication.isPlaying=true;
        }
        static void Restore(){if(SessionState.GetString(Key,"").Length==0)return;string old=SessionState.GetString(Previous,"");CascadeGame.EditorProfileDirectory=old.Length>0?old:null;SessionState.EraseString(Key);SessionState.EraseString(Previous);}
        static void Start(){var host=new GameObject("Temporary map polish review"){hideFlags=HideFlags.DontSave};UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<SettingsReferenceLayout>().StartCoroutine(Run(host));}
        static IEnumerator Run(GameObject host){
            bool background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
            try{
                yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");
                var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
                bool extras=SessionState.GetBool(Extras,false);
                game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.SetEditorSafeInsets(extras?new Vector4(.02f,.055f,.02f,.05f):new Vector4(0,.00001f,0,.00001f));
                bool verify=SessionState.GetBool(Verify,false);string phase=extras?"MapPolishDetailsOct15":verify?"MapPolishOct15":"MapPolishBeforeOct15";
                var rows=new JArray();int shots=0;
                foreach(int world in verify&&!extras?Enumerable.Range(0,9):new[]{5,8}){
                    WorldMenusReview.ConfigureWorld(game,world);
                    foreach(string locale in verify&&!extras?new[]{"en","ru","uk"}:new[]{"ru"})foreach(bool large in extras?new[]{true}:verify?new[]{false,true}:new[]{false}){
                        game.SetUiLocale(locale);game.Preferences.LargeText=large;
                        typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{GameScreen.JourneyMap});
                        yield return new WaitForSecondsRealtime(.4f);
                        var sizes=extras?new[]{new Vector2Int(320,693),new Vector2Int(390,844)}:verify?new[]{new Vector2Int(320,693),new Vector2Int(390,844),new Vector2Int(720,1280),new Vector2Int(768,1024)}:new[]{new Vector2Int(390,844),new Vector2Int(720,1280)};
                        foreach(var size in sizes){
                            string name="world-"+world+"-"+locale+(large?"-large":"")+"-"+size.x+"x"+size.y;
                            yield return PlayableCapture.CaptureSettled(phase,name+".png",size.x,size.y,g=>rows.Add(Measure(g,name,verify)));shots++;
                        }
                    }
                }
                string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",phase);
                var motion=extras?CornerMotion(game,folder):null;
                File.WriteAllText(Path.Combine(folder,"validation.json"),new JObject {["passed"]=true,["captures"]=shots,["temporary_profile"]=SessionState.GetString(Key,""),["measurements"]=rows,["corner_motion"]=motion,["scope"]="Actual Unity phone/tablet renders and active UI geometry. No player build/install or device performance claim."}.ToString()+"\n");
                Debug.Log("MAP_POLISH_"+(verify?"PASS":"BASELINE")+": "+shots+" native captures; isolated profile; no Editor scene saved.");
                EditorApplication.delayCall+=()=>EditorApplication.isPlaying=false;
            }finally{UnityEngine.Application.runInBackground=background;Restore();UnityEngine.Object.Destroy(host);}
        }
        static JArray CornerMotion(CascadeGame game,string folder)
        {
            var original=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World-specific architectural frame");
            var environment=game.GetComponentInChildren<MenuWorldEnvironment>();
            int quality=game.Preferences.EffectQuality;bool reduced=game.ReducedMotion;
            try{
                game.Preferences.EffectQuality=0;environment.SetReviewAge(3);
                if(original.material.GetFloat("_CornerMotionAllowed")!=0||original.material.GetFloat("_Age")!=0)throw new InvalidOperationException("Low quality did not stop corner motion");
                game.Preferences.EffectQuality=2;game.ReducedMotion=true;environment.SetReviewAge(7);
                if(original.material.GetFloat("_CornerMotionAllowed")!=0||original.material.GetFloat("_Age")!=0)throw new InvalidOperationException("Reduced motion did not stop corner motion");
            }finally{game.Preferences.EffectQuality=quality;game.ReducedMotion=reduced;environment.EndReview();}
            var cameraObject=new GameObject("Temporary corner proof camera");var camera=cameraObject.AddComponent<Camera>();
            camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.orthographic=true;camera.orthographicSize=1;camera.nearClipPlane=.1f;camera.farClipPlane=10;
            var canvasObject=new GameObject("Temporary corner proof canvas",typeof(RectTransform),typeof(Canvas));canvasObject.layer=31;
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            var faceObject=new GameObject("Isolated actual frame",typeof(RectTransform),typeof(Image));faceObject.layer=31;faceObject.transform.SetParent(canvasObject.transform,false);
            var rect=faceObject.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var face=faceObject.GetComponent<Image>();face.sprite=original.sprite;face.type=Image.Type.Sliced;face.raycastTarget=false;
            var material=new Material(original.material);face.material=material;material.SetFloat("_MapCornerWind",1);material.SetFloat("_CornerMotionAllowed",1);
            var render=new RenderTexture(390,844,24,RenderTextureFormat.ARGB32);var pixels=new Texture2D(390,844,TextureFormat.RGBA32,false);var oldActive=RenderTexture.active;camera.targetTexture=render;
            var results=new JArray();
            try{
                Canvas.ForceUpdateCanvases();face.pixelsPerUnitMultiplier=face.sprite.rect.width/rect.rect.width;
                for(int world=0;world<9;world++){
                    material.SetFloat("_World",world);material.SetFloat("_Age",0);Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();var before=pixels.GetPixels32();
                    if(world==5)File.WriteAllBytes(Path.Combine(folder,"flowers-frame-a.png"),pixels.EncodeToPNG());
                    material.SetFloat("_Age",2.1f);camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();var after=pixels.GetPixels32();int changed=0,alphaChanged=0;
                    for(int y=660;y<844;y++)for(int x=0;x<390;x++)if(x<125||x>265){int i=y*390+x;var a=before[i];var b=after[i];if(Math.Abs(a.a-b.a)>6)alphaChanged++;if(Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b)>30)changed++;}
                    if(alphaChanged<8||changed<30)throw new InvalidOperationException("Painted corner flowers remain still in world "+world+": "+changed+" colour / "+alphaChanged+" alpha pixels");
                    results.Add(new JObject {["world"]=world,["changed_corner_colour_pixels"]=changed,["changed_corner_alpha_pixels"]=alphaChanged});
                    if(world==5)File.WriteAllBytes(Path.Combine(folder,"flowers-frame-b.png"),pixels.EncodeToPNG());
                }
            }finally{RenderTexture.active=oldActive;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(canvasObject);UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(pixels);render.Release();UnityEngine.Object.DestroyImmediate(render);}
            return results;
        }
        static JObject Measure(CascadeGame game,string name,bool verify){
            var canvas=(RectTransform)game.GetComponentInChildren<Canvas>().transform;var corners=new Vector3[4];
            Rect Bounds(RectTransform rect){rect.GetWorldCorners(corners);var a=canvas.InverseTransformPoint(corners[0]);var b=canvas.InverseTransformPoint(corners[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);}
            var floor=game.GetComponentsInChildren<RawImage>().Single(r=>r.name=="Continuous menu floor");var ground=Bounds(floor.rectTransform);
            var buttons=game.GetComponentsInChildren<Button>();var travel=buttons.Single(b=>b.name=="Travel");var action=Bounds((RectTransform)travel.transform);
            var title=game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Reference shop map world heading");title.ForceMeshUpdate();
            var texts=new JArray();
            foreach(var text in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.isActiveAndEnabled&&t.gameObject.activeInHierarchy&&t.text.Length>0)){
                text.ForceMeshUpdate();texts.Add(new JObject {["name"]=text.name,["text"]=text.text,["overflow"]=text.isTextOverflowing||text.isTextTruncated,["font_size"]=text.fontSize});
                if(verify&&(text.isTextOverflowing||text.isTextTruncated))throw new InvalidOperationException(name+": text does not fit: "+text.name+" / "+text.text);
                if(verify){var ink=text.textBounds;var box=text.rectTransform.rect;
                    if(ink.min.x<box.xMin-1.5f||ink.max.x>box.xMax+1.5f||ink.min.y<box.yMin-1.5f||ink.max.y>box.yMax+1.5f)
                        throw new InvalidOperationException(name+": visible glyphs leave their label: "+text.name+" / "+text.text);
                }
            }
            if(verify){
                if(ground.height/canvas.rect.height>WorldFrameArt.MapLandingViewportFraction+.001f)throw new InvalidOperationException(name+": floor still covers perspective");
                // The latest approved composition rests the action on the short
                // landing; minimum touch padding may extend below its artwork.
                if(Mathf.Abs(action.yMin-ground.yMax)>canvas.rect.height*.018f)throw new InvalidOperationException(name+": continue button detached from landing");
                if(Mathf.Abs(ground.yMin-canvas.rect.yMin)>.05f)throw new InvalidOperationException(name+": ground detached from physical bottom");
                var home=Bounds((RectTransform)buttons.Single(b=>b.name=="Back").transform);var settings=Bounds((RectTransform)buttons.Single(b=>b.name=="Map settings").transform);
                var heading=Bounds(title.rectTransform);
                if(Mathf.Abs(home.center.y-settings.center.y)>.05f||home.center.y>=heading.center.y)throw new InvalidOperationException(name+": corner controls must align below the title");
                if(game.GetComponentsInChildren<MapRopeBridge>().Length!=0)throw new InvalidOperationException(name+": unwanted decorative bridges");
                foreach(var button in buttons.Where(b=>b.interactable&&new[]{"Back","Map settings","Travel","MapmodeShapes","MapmodeWords","MapmodeMixed"}.Contains(b.name))){
                    var p=RectTransformUtility.WorldToScreenPoint(Camera.main,button.transform.position);var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=p},hits);
                    if(hits.Count==0||hits[0].gameObject.GetComponentInParent<Button>()!=button)throw new InvalidOperationException(name+": button covered: "+button.name);
                }
            }
            return new JObject {["capture"]=name,["floor_fraction"]=ground.height/canvas.rect.height,["continue_clearance"]=action.yMin-ground.yMax,["title"]=title.text,["texts"]=texts};
        }
    }
}
