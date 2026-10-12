using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Editor.Playable;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Release
{
    // Isolated native-camera art review; never builds a player or changes the device profile.
    [InitializeOnLoad]
    public static class SawDiscReview
    {
        const string Prefix="Cascade.SawDiscReview.",Phase="SawDisc-Oct10";
        const System.Reflection.BindingFlags Private=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
        static SawDiscReview(){if(SessionState.GetBool(Prefix+"Finishing",false))EditorApplication.update+=Finish;else if(SessionState.GetBool(Prefix+"Auto",false)){SessionState.SetBool(Prefix+"Started",false);EditorApplication.update+=Tick;UnityEngine.Application.logMessageReceived+=RecordError;}}
        public static void BeginBatch()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Cascade/Scenes/CascadePlayable.unity");
            const string path="Assets/_Cascade/Content/Resources/CascadeArt/Devices/CircularSawDisc.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=512;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
            importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            LocalizationAssets.Build();NavigationCapture.Prepare();Directory.CreateDirectory(Output);
            SessionState.SetBool(Prefix+"Auto",true);SessionState.SetBool(Prefix+"Started",false);SessionState.SetInt(Prefix+"Errors",0);
            SessionState.SetFloat(Prefix+"Began",(float)EditorApplication.timeSinceStartup);
            UnityEngine.Application.logMessageReceived+=RecordError;EditorApplication.update+=Tick;UnityEngine.Application.runInBackground=true;EditorApplication.isPlaying=true;
        }
        static void RecordError(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)SessionState.SetInt(Prefix+"Errors",SessionState.GetInt(Prefix+"Errors",0)+1);}
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup-SessionState.GetFloat(Prefix+"Began",0)>160){Debug.LogError("Saw disc review timed out");EditorApplication.Exit(2);return;}
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
        static void Check(bool ok,string reason){if(!ok)throw new InvalidOperationException(reason);}
        static IEnumerator Ready(CascadeGame g){float deadline=Time.realtimeSinceStartup+18;while(!g.Interaction.CanPlay){Check(Time.realtimeSinceStartup<deadline,"Fixture blocked");yield return null;}}
        static LevelDefinition Fixture(SymbolId[] row,bool dense=false)
        {
            var open=Enumerable.Repeat(true,row.Length-1).ToArray();open[1]=false;
            var floor=new FloorDefinition(7,open,key:new KeyDefinition(1,row[0],1),bomb:dense?new BombDefinition(1,row[0],2,1):null);
            return new LevelDefinition("disc-review","C02","2.0","2.0","review",row,row.Reverse(),new[]{new TargetSegment(row[0].Kind==SymbolKind.Glyph?SegmentKind.Word:SegmentKind.Shapes,0,row.Length,row[0].Kind==SymbolKind.Glyph?"disc-review-word":null)},
                new[]{floor}.Concat(Enumerable.Range(0,dense?3:1).Select(i=>new FloorDefinition(4,Enumerable.Repeat(true,row.Length-1)))));
        }
        static void CheckFace(CascadeGame g,bool film=false)
        {
            Canvas.ForceUpdateCanvases();
            var rects=film?UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None):g.GetComponentsInChildren<RectTransform>();
            var face=rects.Single(t=>t.name=="Saw disc face layout"&&(!film||t.parent.name=="Film contact body"));
            var image=face.parent.GetComponent<Image>();Check(image.sprite.name=="CircularSawDisc","Old saw used");
            Check(Mathf.Abs(face.rect.width-face.rect.height)<.1f,"Disc symbol space stretched");
            var mark=(RectTransform)face.GetChild(0);var c=new Vector3[4];mark.GetWorldCorners(c);var camera=Camera.main;
            float px=Vector2.Distance(camera.WorldToScreenPoint(c[0]),camera.WorldToScreenPoint(c[1]));
            Check(px>=20,"Required contact is too small: "+px);
            var glyph=mark.GetComponentInChildren<TMPro.TMP_Text>();if(glyph!=null){glyph.ForceMeshUpdate();Check(!glyph.isTextOverflowing,"Contact letter clipped");Check(glyph.textInfo.characterCount==1&&glyph.textInfo.characterInfo[0].isVisible,"Missing contact letter");}
            Debug.Log("CASCADE disc face: "+(glyph==null?"shape":glyph.text)+"; "+px.ToString("F1")+" pixels");
        }
        static IEnumerator Run(CascadeGame g)
        {
            var camera=Camera.main;var target=camera.targetTexture;var portrait=new RenderTexture(390,844,24);camera.targetTexture=portrait;
            try{
                yield return null;g.RefreshLayout();yield return null;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
                g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.NotificationsEnabled=false;g.Preferences.Tutorials=false;g.ReducedMotion=true;
                var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(g);
                foreach(MechanicLesson lesson in Enum.GetValues(typeof(MechanicLesson)))if(!profile.SeenMechanics.Contains(lesson))profile.SeenMechanics.Add(lesson);
                profile.SeenCoachPrompts.Add("controls-two-ways-v1");profile.SeenCoachPrompts.Add("controls-undo-v1");profile.SeenCoachPrompts.Add("closed-path-v2");
                typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(g,false);
                var shapes=new[]{"s:C","s:B","s:A","s:D"}.Select(x=>SymbolId.Parse(x)).ToArray();
                g.LoadLevel(Fixture(shapes));g.ShowGame();yield return Ready(g);
                yield return PlayableCapture.CaptureSettled(Phase,"disc-shape.png",390,844,owner=>CheckFace(owner));
                Check(g.Interaction.Execute(PuzzleCommand.Swap(0)).Accepted,"Saw activation exchange rejected");yield return Ready(g);
                Check(g.Interaction.Attempt.Current.KeyUsed,"Saw did not clear its branch");
                Check(g.Interaction.Execute(PuzzleCommand.Swap(1)).Accepted,"Saw left its branch exchange blocked");yield return Ready(g);
                foreach(string locale in new[]{"en","ru","uk"}){
                    string letters=locale=="en"?"WARD":locale=="ru"?"ЖАРК":"ЇЖАК";
                    var row=letters.Select(c=>SymbolId.Parse("g:"+locale+":"+c)).ToArray();g.SetUiLocale(locale);g.Preferences.LargeText=true;
                    g.LoadLevel(Fixture(row),locale);g.ShowGame();yield return Ready(g);
                    yield return PlayableCapture.CaptureSettled(Phase,"disc-letter-"+locale+".png",320,693,owner=>CheckFace(owner));
                }
                g.SetUiLocale("en");g.Preferences.LargeText=false;
                var dense=new[]{"s:F","s:B","s:A","s:E","s:D","s:C"}.Select(x=>SymbolId.Parse(x)).ToArray();g.LoadLevel(Fixture(dense,true));g.ShowGame();yield return Ready(g);
                string hash=g.Interaction.Attempt.CurrentStateHash;g.ReducedMotion=false;g.Machine.CueWind(true);g.CueDeviceWind(true);yield return new WaitForSecondsRealtime(.5f);
                yield return PlayableCapture.CaptureSettled(Phase,"disc-dense.png",320,693,owner=>CheckFace(owner));Check(hash==g.Interaction.Attempt.CurrentStateHash,"Swing changed puzzle state");
                g.LoadLevel(Fixture(shapes));g.ShowGame();yield return Ready(g);
                g.Preferences.Tutorials=true;
                typeof(CascadeGame).GetMethod("OpenMechanicPopup",Private).Invoke(g,new object[]{MechanicLesson.Key,null,default(UiCopy),null});
                yield return new WaitForSecondsRealtime(.35f);
                yield return PlayableCapture.CaptureSettled(Phase,"disc-lesson.png",390,844,owner=>CheckFace(owner,true));
                Check(SessionState.GetInt(Prefix+"Errors",0)==0,"Unity recorded review errors");
                File.WriteAllText(Path.Combine(Output,"native-review.txt"),"PASS: native circular blade, large shape/EN/RU/UK letter contact faces, compact 320px Large Text, dense saw+bomb wind, real branch activation and lesson film. No phone build/install or physical-device validation.");
                Debug.Log("CASCADE saw disc review passed.");SessionState.SetBool(Prefix+"Finishing",true);EditorApplication.update+=Finish;EditorApplication.isPlaying=false;
            }finally{camera.targetTexture=target;portrait.Release();UnityEngine.Object.Destroy(portrait);}
        }
    }
}
