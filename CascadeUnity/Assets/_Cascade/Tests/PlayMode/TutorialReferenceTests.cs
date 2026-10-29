#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
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
    public sealed class TutorialReferenceTests
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,oldTarget;string directory;bool background,audio;
        object Field(string name)=>typeof(CascadeGame).GetField(name,Flags).GetValue(game);
        void Set(string name,object value)=>typeof(CascadeGame).GetField(name,Flags).SetValue(game,value);
        void Call(string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Flags).Invoke(game,args);
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeTutorialReference",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;Resize(720,1280);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Preferences.Tutorials=true;game.Coach.Enabled=false;game.Preferences.EffectQuality=2;
            var p=(PlayerProfile)Field("profile");p.AccountIntroSeen=true;p.Missions.NextAfterClears=20000;p.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            p.SeenCoachPrompts.AddRange(new[]{"controls-two-ways-v1","controls-undo-v1","closed-path-v2"});foreach(var lesson in p.SeenMechanics)p.SeenCoachPrompts.Add("triad-guide-v6-"+(int)lesson);
            ((HashSet<int>)Field("heardStory")).UnionWith(Enumerable.Range(0,10000));game.Preferences.ReducedMotion=false;game.ReducedMotion=false;game.ShowGame();
            float deadline=Time.realtimeSinceStartup+30;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<deadline)yield return null;Assert.That(game.Interaction.CanPlay,Is.True);
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}target=new RenderTexture(width,height,24);camera.targetTexture=target;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}CascadeGame.EditorProfileDirectory=null;AudioListener.pause=audio;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        void Open(MechanicLesson lesson,bool pages=false)
        {
            if(game.MechanicPopupOpen)Call("CloseMechanicPopup");Set("lessonReplay",pages);
            var list=(List<MechanicLesson>)Field("replayLessons");list.Clear();if(pages)list.AddRange(new[]{lesson,MechanicLesson.Repair,MechanicLesson.Mirror});Set("replayLessonIndex",0);
            Call("OpenMechanicPopup",lesson,null,default(UiCopy),null);Assert.That(game.MechanicPopupOpen,Is.True);
        }
        void Pose(float age=1.7f)
        {
            foreach(var film in game.GetComponentsInChildren<LessonFilm>()){
                film.enabled=false;typeof(LessonFilm).GetField("elapsed",Flags).SetValue(film,age);typeof(LessonFilm).GetMethod("Paint",Flags).Invoke(film,null);
            }
        }
        Rect Ink(RectTransform target,RectTransform area)
        {
            var label=target.GetComponent<TextMeshProUGUI>();Bounds b;if(label!=null){label.ForceMeshUpdate();b=CenteredContentRow.GlyphBounds(label);}
            else if(target.GetComponent<PauseGlyphGraphic>() is PauseGlyphGraphic glyph&&glyph.canvasRenderer.GetMesh() is Mesh mesh&&mesh.vertexCount>0)b=mesh.bounds;
            else b=new Bounds(target.rect.center,target.rect.size);
            var a=(Vector2)area.InverseTransformPoint(target.TransformPoint(b.min));var z=(Vector2)area.InverseTransformPoint(target.TransformPoint(b.max));return Rect.MinMaxRect(Mathf.Min(a.x,z.x),Mathf.Min(a.y,z.y),Mathf.Max(a.x,z.x),Mathf.Max(a.y,z.y));
        }
        void Verify(string key)
        {
            foreach(var group in game.GetComponentsInChildren<CenteredContentRow>()){
                if(group.Surface.GetComponentInParent<TutorialReferenceLayout>()==null)continue;
                var bounds=group.Elements.Select(e=>Ink(e,group.Surface)).ToArray();float unit=group.LocalUnit;
                Assert.That((bounds.Min(b=>b.xMin)+bounds.Max(b=>b.xMax))*.5f,Is.EqualTo(CenteredContentRow.ReadingRect(group.Surface).center.x).Within(.8f*unit),key+" "+group.Surface.name+" centre x");
                Assert.That((bounds.Min(b=>b.yMin)+bounds.Max(b=>b.yMax))*.5f,Is.EqualTo(CenteredContentRow.ReadingRect(group.Surface).center.y).Within(.8f*unit),key+" "+group.Surface.name+" centre y");
                Assert.That(bounds.Min(b=>b.xMin),Is.GreaterThanOrEqualTo(group.Surface.rect.xMin-1));Assert.That(bounds.Max(b=>b.xMax),Is.LessThanOrEqualTo(group.Surface.rect.xMax+1));
                for(int i=1;i<bounds.Length;i++){float gap=group.Vertical?bounds[i-1].yMin-bounds[i].yMax:bounds[i].xMin-bounds[i-1].xMax;Assert.That(gap,Is.EqualTo(4*unit).Within(.65f*unit),key+" content gap");}
            }
            var body=game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="What this mechanic changes");body.ForceMeshUpdate();
            Assert.That(body.isTextOverflowing||body.isTextTruncated,Is.False,key+" body fits");
            var safe=(RectTransform)Field("mechanicSafe");var canvas=game.GetComponentInChildren<Canvas>();
            foreach(var button in game.GetComponentsInChildren<Button>().Where(b=>b.name=="Acknowledge mechanic")){
                var r=Ink((RectTransform)button.transform,safe);Assert.That(r.width*canvas.scaleFactor,Is.GreaterThanOrEqualTo(43.7f));Assert.That(r.height*canvas.scaleFactor,Is.GreaterThanOrEqualTo(43.7f));
                Assert.That(r.yMin,Is.GreaterThanOrEqualTo(safe.rect.yMin-.5f));Assert.That(r.yMax,Is.LessThanOrEqualTo(safe.rect.yMax+.5f));
                var hits=new List<RaycastResult>();var rect=(RectTransform)button.transform;EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center))},hits);
                Assert.That(hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>(),Is.SameAs(button),key+" "+button.name+" receives a real pointer raycast");
            }
        }
        IEnumerator Shot(string name,float age=1.7f)
        {
            yield return new WaitForSecondsRealtime(.42f);Pose(age);game.RefreshLayout();Canvas.ForceUpdateCanvases();
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"TutorialReferenceOct16",name+".png",target.width,target.height,(Action<CascadeGame>)(_=>{Pose(age);foreach(var row in game.GetComponentsInChildren<CenteredContentRow>())row.Refresh();Verify(name);})});
            var texture=new Texture2D(2,2,TextureFormat.RGB24,false);
            try{
                texture.LoadImage(File.ReadAllBytes(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/TutorialReferenceOct16",name+".png"))));
                var stage=game.GetComponentsInChildren<TutorialReferenceLayout>().Single().transform as RectTransform;
                var point=camera.WorldToScreenPoint(stage.TransformPoint(new Vector3(stage.rect.xMin+360,stage.rect.yMax-480)));
                var colour=texture.GetPixel(Mathf.RoundToInt(point.x),Mathf.RoundToInt(point.y));Assert.That(colour.r-colour.b,Is.GreaterThan(.1f),name+" cream reading field actually renders");
            }finally{UnityEngine.Object.Destroy(texture);}
        }
        [UnityTest] public IEnumerator EveryMechanicShowsOnlyTryItAndKeepsThePuzzleUnchanged()
        {
            game.ReducedMotion=true;
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);
                foreach(var lesson in Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>()){
                    Open(lesson);yield return null;
                    string hash=game.Interaction.Attempt.CurrentStateHash;
                    var root=(RectTransform)Field("mechanicOverlay");var actions=root.GetComponentsInChildren<Button>();
                    Assert.That(actions.Length,Is.EqualTo(1),lesson+" has exactly one action");Assert.That(actions[0].name,Is.EqualTo("Acknowledge mechanic"));
                    var caption=actions[0].GetComponentInChildren<TextMeshProUGUI>();Assert.That(caption.GetComponent<LocalizedCaption>().SourceIdentity,Is.EqualTo("Try it"));
                    Assert.That(root.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Film action button"),Is.False,"No extra button-shaped controls in the demonstration");
                    foreach(var dots in root.GetComponentsInChildren<TutorialPageControl>())Assert.That(dots.raycastTarget,Is.False);
                    Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
                }
            }
        }
        [UnityTest] public IEnumerator ReferencePreview()
        {
            game.SetEditorSafeInsets(Vector4.zero);game.SetUiLocale("ru");Open(MechanicLesson.Undo);yield return Shot("undo-single-ru-720");
            Open(MechanicLesson.Undo,true);yield return Shot("undo-pages-ru-720");
        }
        [UnityTest,Timeout(900000)] public IEnumerator EveryLessonFitsTheReferenceOnPhonesAndInAllLanguages()
        {
            foreach(string locale in new[]{"en","ru","uk"})foreach(int width in new[]{320,390,430}){
                Resize(width,width==320?693:width==430?932:844);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));game.SetUiLocale(locale);game.Preferences.LargeText=true;
                foreach(var lesson in Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>()){
                    Open(lesson);string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;
                    yield return Shot(locale+"-"+width+"-"+lesson);
                    Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.ShopCoins,Is.EqualTo(coins));
                    Assert.That(game.GetComponentsInChildren<TutorialPageControl>().Length,Is.EqualTo(lesson==MechanicLesson.Swap?1:0));
                }
            }
        }
        [UnityTest] public IEnumerator PagesSwipeAndSingleTryActionDoNotMoveTheLivePuzzle()
        {
            Resize(390,844);game.SetEditorSafeInsets(new Vector4(0,.04f,0,.035f));Open(MechanicLesson.Undo,true);yield return Shot("pager-start-390");
            string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;
            var dots=game.GetComponentsInChildren<TutorialPageControl>().Single();var centre=RectTransformUtility.WorldToScreenPoint(camera,dots.rectTransform.TransformPoint(dots.rectTransform.rect.center+Vector2.right*35));
            var pointer=new PointerEventData(EventSystem.current){position=centre,button=PointerEventData.InputButton.Left};
            var raycaster=dots.GetComponentInParent<GraphicRaycaster>();pointer.pointerPressRaycast=new RaycastResult{module=raycaster};dots.OnPointerClick(pointer);yield return null;
            Assert.That(dots.raycastTarget,Is.False,"Pagination is an indicator, not another action");
            Assert.That(game.CurrentMechanic,Is.EqualTo(MechanicLesson.Undo));Assert.That(game.GetComponentsInChildren<TutorialPageControl>().Single().Selected,Is.EqualTo(0));
            var swipe=game.GetComponentsInChildren<MenuSwipe>().Single(s=>s.name=="Tutorial page swipe area");var rect=(RectTransform)swipe.transform;var point=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center));
            pointer=new PointerEventData(EventSystem.current){position=point,pressPosition=point,button=PointerEventData.InputButton.Left,eligibleForClick=true};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.That(hits.Count,Is.GreaterThan(0));Assert.That(ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject),Is.SameAs(swipe.gameObject));
            swipe.OnBeginDrag(pointer);pointer.position-=Vector2.right*110;swipe.OnDrag(pointer);swipe.OnEndDrag(pointer);yield return null;
            Assert.That(game.CurrentMechanic,Is.EqualTo(MechanicLesson.Repair));Assert.That(pointer.eligibleForClick,Is.False);yield return Shot("pager-swipe-middle-390");
            Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));Assert.That(game.ShopCoins,Is.EqualTo(coins));Assert.That(game.Interaction.IsPaused,Is.True);
            Assert.That(game.Interaction.Execute(PuzzleCommand.Swap(0)),Is.Null,"The tutorial blocks live puzzle commands before they reach the attempt");
            var backdrop=game.GetComponentsInChildren<TutorialBackdrop>().Single();var snapshot=backdrop.GetComponent<RawImage>().texture;
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Acknowledge mechanic").onClick.Invoke();yield return null;
            Assert.That(game.MechanicPopupOpen,Is.False);Assert.That(game.Interaction.IsPaused,Is.False);Assert.That(snapshot==null,Is.True,"The single captured texture is released on close");
        }
        [UnityTest] public IEnumerator FilmMotionSuspensionAndReducedMotionKeepTheAttemptUnchanged()
        {
            Open(MechanicLesson.Repair);yield return new WaitForSecondsRealtime(.45f);var film=game.GetComponentsInChildren<LessonFilm>().Single();float age=film.Elapsed;string hash=game.Interaction.Attempt.CurrentStateHash;
            yield return new WaitForSecondsRealtime(.2f);Assert.That(film.Elapsed,Is.GreaterThan(age));game.SendMessage("OnApplicationFocus",false);age=film.Elapsed;yield return new WaitForSecondsRealtime(.2f);Assert.That(film.Elapsed,Is.EqualTo(age));
            game.SendMessage("OnApplicationFocus",true);game.ReducedMotion=true;yield return Shot("repair-reduced-390",6.4f);Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            game.DismissMechanic();yield return null;game.Preferences.EffectQuality=0;Open(MechanicLesson.Undo);yield return null;Assert.That(game.GetComponentsInChildren<TutorialBackdrop>().Length,Is.Zero,"Low effects needs no blur capture");
            game.Preferences.Tutorials=false;yield return null;Assert.That(game.MechanicPopupOpen,Is.False);Assert.That(game.Interaction.IsPaused,Is.False);
        }
        [UnityTest] public IEnumerator WordsAndMixedUseTheirOwnSymbolsWithoutChangingTheAttempt()
        {
            Resize(390,844);game.SetEditorSafeInsets(new Vector4(0,.04f,0,.035f));
            foreach(string locale in new[]{"en","ru","uk"})foreach(bool mixed in new[]{false,true}){
                if(game.MechanicPopupOpen)Call("CloseMechanicPopup");
                string word=locale=="en"?"STAR":locale=="ru"?"МОСТ":"МІСТ";
                var row=word.Select(c=>SymbolId.Parse("g:"+locale+":"+c)).ToArray();
                var segments=new List<TargetSegment>();
                if(mixed){row[0]=SymbolId.Parse("s:circle");row[1]=SymbolId.Parse("s:triangle");segments.Add(new TargetSegment(SegmentKind.Shapes,0,2));}
                segments.Add(new TargetSegment(SegmentKind.Word,mixed?2:0,mixed?2:4,"tutorial-letters"));
                var goal=row.ToArray();var first=goal[0];goal[0]=goal[1];goal[1]=first;
                game.LoadLevel(new LevelDefinition("tutorial-symbol-fixture","tutorial-symbol-fixture","1","2.0","tutorial-symbol-fixture",row,goal,segments,new[]{new FloorDefinition(8,new[]{true,true,true})}),locale);
                game.SetUiLocale(locale);game.ShowGame();float until=Time.realtimeSinceStartup+30;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<until)yield return null;Assert.That(game.Interaction.CanPlay,Is.True);
                Assert.That(game.Interaction.Attempt.Mode,Is.EqualTo(mixed?PuzzleMode.Mixed:PuzzleMode.Words));string hash=game.Interaction.Attempt.CurrentStateHash;
                Open(MechanicLesson.Undo);yield return Shot((mixed?"mixed-":"words-")+locale+"-390");
                var letters=game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name=="Raised lesson letter").ToArray();Assert.That(letters.Length,Is.EqualTo(mixed?2:4));
                foreach(var letter in letters){letter.ForceMeshUpdate();Assert.That(letter.isTextOverflowing||letter.isTextTruncated,Is.False);}
                Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
            }
        }
        [UnityTest] public IEnumerator ResultPosesKeepTwoBoardExamplesAndGesturePagesReadable()
        {
            Resize(390,844);game.SetEditorSafeInsets(new Vector4(0,.04f,0,.035f));game.SetUiLocale("ru");
            foreach(var lesson in new[]{MechanicLesson.ReturnHourglass,MechanicLesson.LandingLens,MechanicLesson.Mirror,MechanicLesson.Repair}){
                Open(lesson);yield return Shot("result-"+lesson+"-ru-390",6.4f);
            }
            Open(MechanicLesson.Swap);Call("SelectTutorialPage",1);yield return Shot("swap-tap-page-ru-390");
            Assert.That(game.GetComponentsInChildren<TutorialPageControl>().Single().Selected,Is.EqualTo(1));
        }
    }
}
#endif
