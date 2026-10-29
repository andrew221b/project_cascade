#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Cascade.Tests
{
    public sealed class ComicReferenceTests
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,oldTarget;string directory;bool background,audio;
        object Field(string key)=>typeof(CascadeGame).GetField(key,Flags).GetValue(game);
        void Set(string key,object value)=>typeof(CascadeGame).GetField(key,Flags).SetValue(game,value);
        void Call(string key,params object[] values)=>typeof(CascadeGame).GetMethod(key,Flags).Invoke(game,values);
        TextMeshProUGUI Label(string name)=>game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name==name);
        Button Control(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/ComicReferenceOct16"));
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeComicReference",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;
            Resize(720,1280);game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=false;game.Coach.Enabled=false;game.Preferences.EffectQuality=2;game.ReducedMotion=false;
            ((PlayerProfile)Field("profile")).AccountIntroSeen=true;yield return null;
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(width,height,24);camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;UnityEngine.Application.runInBackground=background;AudioListener.pause=audio;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        void Open(int world,int page=0,int turn=0)
        {
            game.OpenWorldComic(world,game.ShowHome);
            if(page!=0||turn!=0){Set("worldComicPage",page);Set("worldComicLine",turn);Set("worldComicChunks",Array.Empty<string>());Call("DrawMenu");}
        }
        IEnumerator Shot(string name,bool verify=true)
        {
            yield return new WaitForSecondsRealtime(.35f);Canvas.ForceUpdateCanvases();
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"ComicReferenceOct16",name+".png",target.width,target.height,verify?(Action<CascadeGame>)(_=>Verify(name)):null});
        }
        Rect Ink(RectTransform r,RectTransform space)
        {
            var text=r.GetComponent<TextMeshProUGUI>();Bounds b;if(text!=null){text.ForceMeshUpdate();b=CenteredContentRow.GlyphBounds(text);}else b=new Bounds(r.rect.center,r.rect.size);
            var a=(Vector2)space.InverseTransformPoint(r.TransformPoint(b.min));var z=(Vector2)space.InverseTransformPoint(r.TransformPoint(b.max));return Rect.MinMaxRect(Mathf.Min(a.x,z.x),Mathf.Min(a.y,z.y),Mathf.Max(a.x,z.x),Mathf.Max(a.y,z.y));
        }
        void Verify(string name)
        {
            var layout=game.GetComponentInChildren<WorldComicLayout>();layout.RefreshLayout();Canvas.ForceUpdateCanvases();
            foreach(var label in new[]{Label("Comic title"),Label("World comic speaker"),Label("World comic spoken line"),Label("World comic progress")}){
                label.ForceMeshUpdate();Assert.That(label.isTextTruncated,Is.False,name+" / "+label.name);Assert.That(label.color.maxColorComponent,Is.LessThan(.5f),name+" dark ink on paper");
                var centred=label.GetComponentInParent<CenteredContentRow>();
                var area=label.name=="World comic spoken line"?(RectTransform)label.transform.parent:centred!=null?centred.Surface:label.rectTransform;
                var b=Ink(label.rectTransform,area);var writing=CenteredContentRow.ReadingRect(area);Assert.That(b.xMin,Is.GreaterThanOrEqualTo(writing.xMin-1),name+" / "+label.name+" left padding");Assert.That(b.xMax,Is.LessThanOrEqualTo(writing.xMax+1),name+" / "+label.name+" right padding");Assert.That(b.yMin,Is.GreaterThanOrEqualTo(writing.yMin-1),name+" / "+label.name+" bottom padding");Assert.That(b.yMax,Is.LessThanOrEqualTo(writing.yMax+1),name+" / "+label.name+" top padding");
            }
            var reader=Label("World comic spoken line").GetComponentInParent<ScrollRect>();Assert.That(reader.vertical,Is.False,name+" whole authored turn fits without scrolling");
            var scene=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World comic scene");var rect=(RectTransform)scene.transform.parent;
            Assert.That(rect.rect.width/rect.rect.height,Is.EqualTo(scene.sprite.rect.width/scene.sprite.rect.height).Within(.001f),name+" original comic aspect");
            foreach(var group in game.GetComponentsInChildren<CenteredContentRow>()){
                var b=group.Elements.Select(r=>Ink(r,group.Surface)).ToArray();float unit=group.LocalUnit;
                Assert.That((b.Min(r=>r.xMin)+b.Max(r=>r.xMax))*.5f,Is.EqualTo(CenteredContentRow.ReadingRect(group.Surface).center.x).Within(.8f*unit),name+" centred horizontal content");
                Assert.That((b.Min(r=>r.yMin)+b.Max(r=>r.yMax))*.5f,Is.EqualTo(CenteredContentRow.ReadingRect(group.Surface).center.y).Within(.8f*unit),name+" centred vertical content");
            }
            foreach(string key in new[]{"Next comic page","Skip comic"}){
                var button=Control(key);var safe=(RectTransform)Field("safeRoot");var b=Ink((RectTransform)button.transform,safe);float scale=game.GetComponentInChildren<Canvas>().scaleFactor;
                Assert.That(b.height*scale,Is.GreaterThanOrEqualTo(43.8f));Assert.That(b.width*scale,Is.GreaterThanOrEqualTo(43.8f));Assert.That(b.yMin,Is.GreaterThanOrEqualTo(safe.rect.yMin-.5f));
                var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(camera,button.transform.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                Assert.That(hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>(),Is.SameAs(button),key+" receives the actual pointer");
            }
        }
        readonly Dictionary<Texture2D,Color32[]> alphaPixels=new Dictionary<Texture2D,Color32[]>();
        Color32[] ReadSource(Texture2D source)
        {
            if(alphaPixels.TryGetValue(source,out var existing))return existing;
            var previous=RenderTexture.active;var buffer=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32);
            Graphics.Blit(source,buffer);RenderTexture.active=buffer;var copy=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);
            copy.ReadPixels(new Rect(0,0,source.width,source.height),0,0);copy.Apply();var pixels=copy.GetPixels32();UnityEngine.Object.Destroy(copy);RenderTexture.active=previous;RenderTexture.ReleaseTemporary(buffer);
            alphaPixels[source]=pixels;return pixels;
        }
        float PaintedBottom(Image image,RectTransform space)
        {
            var sprite=image.sprite;var pixels=ReadSource(sprite.texture);var source=sprite.rect;int row=Mathf.FloorToInt(source.yMin);bool found=false;
            for(;row<source.yMax;row++){
                for(int x=Mathf.CeilToInt(source.xMin);x<source.xMax;x++)if(pixels[row*sprite.texture.width+x].a>32){found=true;break;}
                if(found)break;
            }
            Assert.That(found,Is.True,image.name+" has a visible silhouette");
            var area=image.rectTransform.rect;float scale=Mathf.Min(area.width/source.width,area.height/source.height);var size=source.size*scale;
            var point=area.center-size*.5f+new Vector2(0,(Mathf.Max(source.yMin,row)-source.yMin)*scale);
            return space.InverseTransformPoint(image.transform.TransformPoint(point)).y;
        }
        Vector2 PointOnPaintedImage(Image image,Vector2 uv)
        {
            // Interpolate the actual rendered nine-slice mesh, independently of
            // the production reading-field calculations.
            var mesh=image.canvasRenderer.GetMesh();var vertices=mesh.vertices;var tex=mesh.uv;var triangles=mesh.triangles;
            for(int i=0;i<triangles.Length;i+=3){
                int a=triangles[i],b=triangles[i+1],c=triangles[i+2];var u=tex[b]-tex[a];var v=tex[c]-tex[a];var p=uv-tex[a];
                float determinant=u.x*v.y-u.y*v.x;if(Mathf.Abs(determinant)<.000001f)continue;
                float first=(p.x*v.y-p.y*v.x)/determinant,second=(u.x*p.y-u.y*p.x)/determinant;
                if(first<-.001f||second<-.001f||first+second>1.001f)continue;
                return vertices[a]+first*(vertices[b]-vertices[a])+second*(vertices[c]-vertices[a]);
            }
            Assert.Fail("Source support pixel must exist on the rendered board mesh");return Vector2.zero;
        }
        float PaintedSupportTop(Image image,RectTransform space)
        {
            var sprite=image.sprite;var pixels=ReadSource(sprite.texture);int x=Mathf.RoundToInt(sprite.rect.center.x),y=Mathf.FloorToInt(sprite.rect.yMax)-1;
            while(y>sprite.rect.yMin&&pixels[y*sprite.texture.width+x].a<240)y--;
            var local=PointOnPaintedImage(image,new Vector2((x+.5f)/sprite.texture.width,(y+.5f)/sprite.texture.height));
            return space.InverseTransformPoint(image.transform.TransformPoint(local)).y;
        }
        [UnityTest,Timeout(180000)] public IEnumerator PortraitBustsRemainAttachedToNameBoardsAcrossEverySpeakerAndPhone()
        {
            game.ReducedMotion=true;game.SetUiLocale("ru");var records=new System.Text.StringBuilder();
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932)}){
                Resize(size.x,size.y);game.Preferences.LargeText=size.x!=390;
                for(int world=0;world<9;world++){
                    Open(world);
                    for(int turn=0;turn<8;turn++){
                        yield return null;yield return null;Canvas.ForceUpdateCanvases();
                        var avatar=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World comic speaker portrait");var board=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World comic speaker name");
                        var space=(RectTransform)board.transform.parent;float bottom=PaintedBottom(avatar,space),top=PaintedSupportTop(board,space),unit=CenteredContentRow.Unit(space);
                        records.AppendLine(size.x+","+world+","+turn+","+Label("World comic speaker").text+","+(top-bottom)/unit);
                        if(world==0&&turn==0&&size.x==390)yield return Shot("portrait-contact-gran-390");
                        if(world==0&&turn==1&&size.x==390)yield return Shot("portrait-contact-mika-390");
                        Directory.CreateDirectory(Output);File.WriteAllText(Path.Combine(Output,"portrait-contact.csv"),"width,world,turn,speaker,overlap logical pixels\n"+records);
                        Assert.That(bottom,Is.LessThanOrEqualTo(top-2*unit),"Portrait's actual alpha edge must tuck behind its name board: "+size.x+" / "+Label("World comic speaker").text);
                        Control("Next comic page").onClick.Invoke();yield return null;
                    }
                }
            }
        }
        [UnityTest] public IEnumerator ReferencePreview()
        {
            game.SetEditorSafeInsets(Vector4.zero);game.SetUiLocale("ru");Open(0);yield return Shot("reference-wind-ru-720");Open(8);yield return Shot("reference-stars-ru-720");
        }
        [UnityTest,Timeout(300000)] public IEnumerator EveryPanelKeepsWholeLocalizedTurnsNamesAndPhoneButtons()
        {
            game.ReducedMotion=true;string hash=game.Interaction.Attempt.CurrentStateHash;
            foreach(string locale in new[]{"en","ru","uk"})foreach(int width in new[]{320,390,430})foreach(int world in Enumerable.Range(0,9)){
                Resize(width,width==320?693:width==430?932:844);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));game.Preferences.LargeText=width!=390;game.SetUiLocale(locale);Open(world);
                // Locale callbacks can redraw the page on the following frame.
                yield return new WaitForSecondsRealtime(.35f);Canvas.ForceUpdateCanvases();
                for(int page=0;page<4;page++)for(int turn=0;turn<2;turn++){
                    Canvas.ForceUpdateCanvases();yield return null;Verify(locale+"-"+width+"-"+world+"-"+page+"-"+turn);
                    Assert.That(Label("World comic spoken line").text,Is.EqualTo(new UiCopy(WorldPrologues.ForWorld(world).Pages[page].Lines[turn].Text).ToString()));
                    if(page==0&&turn==0)yield return Shot("world-"+world+"-"+locale+"-"+width);
                    Control("Next comic page").onClick.Invoke();yield return null;
                }
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));
            }
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator CompactPhonesAndTabletsKeepTheReferenceReadable()
        {
            game.SetUiLocale("uk");game.Preferences.LargeText=true;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(768,1024)}){Resize(size.x,size.y);Open(5,3);yield return Shot("compact-uk-"+size.x);}
        }
        [UnityTest] public IEnumerator SkipAcknowledgesOnlyOnClickAndNextKeepsTheActivePanel()
        {
            Open(0);yield return new WaitForSecondsRealtime(.4f);int receipt=WorldPrologues.ForWorld(0).Receipt;var before=new ProfileStore(game.ProfilePath).Load().Profile;
            Assert.That(before.HeardStory,Has.No.Member(receipt));var image=game.GetComponentsInChildren<Image>().Single(i=>i.name=="World comic scene");var frame=(RectTransform)image.transform.parent;var size=frame.rect.size;var position=frame.anchoredPosition;
            Control("Next comic page").onClick.Invoke();yield return null;game.GetComponentInChildren<WorldComicLayout>().RefreshLayout();var next=(RectTransform)game.GetComponentsInChildren<Image>().Single(i=>i.name=="World comic scene").transform.parent;
            Assert.That(game.ComicPage,Is.Zero);Assert.That(next.rect.size,Is.EqualTo(size));Assert.That(next.anchoredPosition,Is.EqualTo(position));
            Control("Skip comic").onClick.Invoke();yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Home));Assert.That(new ProfileStore(game.ProfilePath).Load().Profile.HeardStory,Does.Contain(receipt));
        }
        [UnityTest] public IEnumerator SpeakingPortraitsUseAnimatedFullFacesAndStopInTheBackground()
        {
            Resize(390,844);game.SetUiLocale("ru");Open(0);yield return new WaitForSecondsRealtime(.5f);string hash=game.Interaction.Attempt.CurrentStateHash;
            var folder=Path.Combine(Output,"frames");Directory.CreateDirectory(folder);var active=RenderTexture.active;var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            float peak=0,blink=0;CharacterPoseView view=null;
            for(int i=0;i<240;i++){
                if(i==120){Control("Next comic page").onClick.Invoke();yield return new WaitForSecondsRealtime(.35f);}
                if(i==0||i==120){view=game.GetComponentsInChildren<CharacterPoseView>().Single(v=>v.Hero.name=="World comic speaker portrait");view.ResetReview(197);}
                string words=Label("World comic spoken line").text;float speech=CharacterPoseView.SpeechShape(words,(i%120)/30f);view.AdvanceReview(1/30f,.35f,speech);peak=Mathf.Max(peak,view.Motion.MouthOpen);blink=Mathf.Max(blink,view.Motion.Blink);
                camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,i.ToString("D4")+".jpg"),pixels.EncodeToJPG(87));
            }
            RenderTexture.active=active;UnityEngine.Object.Destroy(pixels);Assert.That(peak,Is.GreaterThan(.8f));Assert.That(blink,Is.GreaterThan(.8f));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            view.EndReview();game.SendMessage("OnApplicationFocus",false);float age=view.AnimationAge;yield return new WaitForSecondsRealtime(.2f);Assert.That(view.AnimationAge,Is.EqualTo(age));
        }
    }
}
#endif
