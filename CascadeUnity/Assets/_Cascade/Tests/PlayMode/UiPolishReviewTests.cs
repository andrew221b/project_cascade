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
using Cascade.Solver;
using UnityEngine.EventSystems;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class UiPolishReviewTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture portrait,oldTarget;string directory;bool background,audio;
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/UiPolishOct16"));
        bool Smoke=>File.Exists(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../ui-polish-smoke.flag")));
        bool Before=>File.Exists(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../ui-polish-before.flag")));
        object Field(string key)=>typeof(CascadeGame).GetField(key,Private).GetValue(game);
        void Call(string key,params object[] values)=>typeof(CascadeGame).GetMethod(key,Private).Invoke(game,values);
        void Route(GameScreen screen)=>Call("SetScreen",screen);
        static Type EditorType(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Playable."+name)).First(t=>t!=null);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeUiPolish",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;Resize(390,844);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.SetEditorSafeInsets(new Vector4(.01f,.04f,.01f,.035f));
            game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;game.Coach.Enabled=false;game.ReducedMotion=false;
            var profile=(PlayerProfile)Field("profile");profile.AccountIntroSeen=true;profile.Missions.NextAfterClears=20000;
            profile.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();((HashSet<int>)Field("heardStory")).UnionWith(Enumerable.Range(0,10000));profile.Monetization.Cosmetics.AddRange(new[]{"leaf","sun","wind_chime"});game.ShowHome();yield return null;
        }
        void Resize(int width,int height)
        {
            var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}portrait=new RenderTexture(width,height,24);camera.targetTexture=portrait;scaler.enabled=true;game.RefreshLayout();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(camera!=null)camera.targetTexture=oldTarget;if(game!=null){UnityEngine.Object.Destroy(game.gameObject);yield return null;}
            if(portrait!=null){portrait.Release();UnityEngine.Object.Destroy(portrait);}CascadeGame.EditorProfileDirectory=null;AudioListener.pause=audio;UnityEngine.Application.runInBackground=background;if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Shot(string key)
        {
            if(game.ConversationOpen){typeof(CascadeGame).GetField("conversationAge",Private).SetValue(game,1000f);}
            yield return new WaitForSecondsRealtime(.55f);Canvas.ForceUpdateCanvases();
            if(!Before){
                VerifyRows(key);Directory.CreateDirectory(Output);var rows=new System.Text.StringBuilder();
                foreach(var row in game.GetComponentsInChildren<CenteredContentRow>()){
                    rows.Append(key).Append(',').Append(row.Surface.name).Append(',').Append(row.LocalUnit).Append(',').Append(row.Surface.rect.size).Append(',').Append(row.Vertical).AppendLine();
                    foreach(var element in row.Elements){var copy=element.GetComponent<TextMeshProUGUI>();rows.Append("  ").Append(element.name).Append(',').Append(element.anchoredPosition).Append(',').Append(element.rect.size).Append(',').Append(copy==null?"image":copy.text).Append(',').Append(copy==null?"":copy.textBounds.ToString()).AppendLine();}
                }
                File.WriteAllText(Path.Combine(Output,key+"-metrics.txt"),rows.ToString());
            }
            yield return (IEnumerator)EditorType("PlayableCapture").GetMethod("CaptureSettled").Invoke(null,new object[]{"UiPolishOct16"+(Before?"Before":"After"),key+".png",portrait.width,portrait.height,Before?null:(Action<CascadeGame>)(_=>VerifyRows(key+" rendered"))});
        }
        Rect Ink(RectTransform target,RectTransform space)
        {
            var label=target.GetComponent<TextMeshProUGUI>();Bounds bounds;
            if(label!=null){label.ForceMeshUpdate();Assert.That(label.textInfo.lineCount,Is.LessThanOrEqualTo(1),label.name+" stays on one line after layout and render");bounds=label.textBounds;}else bounds=new Bounds(target.rect.center,target.rect.size);
            Assert.That(bounds.size.x,Is.GreaterThanOrEqualTo(0),target.name+" has a real painted bound");
            var a=(Vector2)space.InverseTransformPoint(target.TransformPoint(bounds.min));var b=(Vector2)space.InverseTransformPoint(target.TransformPoint(bounds.max));
            return Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));
        }
        void VerifyRows(string screen)
        {
            foreach(var row in game.GetComponentsInChildren<CenteredContentRow>()){
                var bounds=row.Elements.Select(r=>Ink(r,row.Surface)).ToArray();if(bounds.Length==0)continue;
                float left=bounds.Min(r=>r.xMin),right=bounds.Max(r=>r.xMax),low=bounds.Min(r=>r.yMin),high=bounds.Max(r=>r.yMax),unit=row.LocalUnit;
                Assert.That((left+right)*.5f,Is.EqualTo(row.Surface.rect.center.x).Within(.8f*unit),screen+" / "+row.Surface.name+" horizontal centre");
                Assert.That((low+high)*.5f,Is.EqualTo(row.Surface.rect.center.y).Within(.8f*unit),screen+" / "+row.Surface.name+" vertical centre");
                Assert.That(left,Is.GreaterThanOrEqualTo(row.Surface.rect.xMin-1*unit),screen+" / "+row.Surface.name+" left fit");
                Assert.That(right,Is.LessThanOrEqualTo(row.Surface.rect.xMax+1*unit),screen+" / "+row.Surface.name+" right fit");
                for(int i=1;i<bounds.Length;i++){
                    float gap=row.Vertical?bounds[i-1].yMin-bounds[i].yMax:bounds[i].xMin-bounds[i-1].xMax;
                    Assert.That(gap,Is.EqualTo(4*unit).Within(.6f*unit),screen+" / "+row.Surface.name+" icon/label gap "+i);
                }
            }
        }
        IEnumerator ReachCheckpoint()
        {
            game.StartCampaignLevel(game.Catalog.GetPrimary("C02").Definition);game.ShowGame();
            float deadline=Time.realtimeSinceStartup+30;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<deadline){if(game.MechanicPopupOpen)game.AcknowledgeMechanic();yield return null;}
            Assert.That(game.Interaction.CanPlay,Is.True);
            var attempt=game.Interaction.Attempt;var solution=PuzzleSolver.FindShortest(attempt.Resolver,attempt.Current);Assert.That(solution.Status,Is.EqualTo(SearchStatus.Solved));
            foreach(var command in solution.Path){
                Assert.That(game.Interaction.Execute(command).Accepted,Is.True);deadline=Time.realtimeSinceStartup+30;
                while(game.Interaction.IsPresenting&&Time.realtimeSinceStartup<deadline)yield return null;
            }
            deadline=Time.realtimeSinceStartup+30;while(game.CurrentScreen!=GameScreen.Checkpoint&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Checkpoint));Assert.That(game.Interaction.Attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));
        }
        int Selected=>(int)Field("shopItem");
        IEnumerator Drag(MenuSwipe swipe,float x,float y=0,bool capture=false)
        {
            var rect=(RectTransform)swipe.transform;var point=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center));
            var pointer=new PointerEventData(EventSystem.current){position=point,pressPosition=point,eligibleForClick=true,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.That(hits.Count,Is.GreaterThan(0));
            var handler=ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject);Assert.That(handler,Is.Not.Null,"The visible card/paper receives a real drag");
            Assert.That(handler.GetComponent<MenuSwipe>(),Is.SameAs(swipe));
            ExecuteEvents.Execute(handler,pointer,ExecuteEvents.beginDragHandler);
            for(int i=1;i<=5;i++){
                pointer.position=point+new Vector2(x,y)*i/5;ExecuteEvents.Execute(handler,pointer,ExecuteEvents.dragHandler);yield return null;
            }
            Assert.That(pointer.eligibleForClick,Is.False,"Dragging a card cannot also click it");
            if(capture)yield return Shot("gesture-dragged");
            ExecuteEvents.Execute(handler,pointer,ExecuteEvents.endDragHandler);yield return null;
        }
        [UnityTest] public IEnumerator ShopAndInventoryGesturesUseSpringPagesWithoutSpendingOrMovingThePuzzle()
        {
            game.ShowShop();game.SelectShopTab(0);yield return new WaitForSecondsRealtime(.6f);
            string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;
            var action=game.GetComponentsInChildren<Button>().Single(b=>b.name=="Buy selected item");var fixedPoint=action.transform.position;
            var swipe=game.GetComponentsInChildren<MenuSwipe>().Single();Assert.That(Selected,Is.Zero);
            yield return Drag(swipe,-110,0,true);Assert.That(Selected,Is.EqualTo(1));
            var incoming=game.GetComponentsInChildren<MenuSwipe>().Single();Assert.That(incoming.IsSettling,Is.True);
            Assert.That(game.GetComponentsInChildren<Button>().Single(b=>b.name=="Buy selected item").transform.position,Is.EqualTo(fixedPoint));
            yield return Shot("gesture-arrived");Assert.That(incoming.IsSettling,Is.False);
            yield return Drag(incoming,-20,110);Assert.That(Selected,Is.EqualTo(1),"Vertical scrolling is not a horizontal page turn");
            game.SelectShopItem(0);yield return new WaitForSecondsRealtime(.45f);yield return Drag(game.GetComponentsInChildren<MenuSwipe>().Single(),110);Assert.That(Selected,Is.Zero,"The first tool has an elastic boundary");
            game.SelectShopTab(1);yield return new WaitForSecondsRealtime(.5f);
            var group=ShopCatalog.TrophyIds.Where(id=>!ShopCatalog.IsParcelExclusive(id)).ToArray();
            yield return Drag(game.GetComponentsInChildren<MenuSwipe>().Single(),-110);Assert.That(Selected,Is.EqualTo(Array.IndexOf(ShopCatalog.TrophyIds,group[6])));
            yield return new WaitForSecondsRealtime(.45f);VerifyRows("trophy second page");
            game.ShowSupplies();yield return new WaitForSecondsRealtime(.5f);int previous=Selected;
            var tools=new[]{1,2,3,0,4,5};int current=Array.IndexOf(tools,previous),direction=current==tools.Length-1?-1:1;
            yield return Drag(game.GetComponentsInChildren<MenuSwipe>().Single(),direction>0?-110:110);Assert.That(Selected,Is.EqualTo(tools[current+direction]));
            game.ShowCollection();yield return new WaitForSecondsRealtime(.5f);previous=Selected;
            yield return Drag(game.GetComponentsInChildren<MenuSwipe>().Single(),-110);Assert.That(Selected,Is.Not.EqualTo(previous));
            Assert.That(game.ShopCoins,Is.EqualTo(coins));Assert.That(game.Interaction.Attempt.CurrentStateHash,Is.EqualTo(hash));
        }
        [UnityTest] public IEnumerator InterruptedGesturesAndReducedMotionKeepHitTargetsAndVisiblePagesStable()
        {
            game.ShowShop();game.SelectShopTab(0);yield return new WaitForSecondsRealtime(.55f);
            yield return Drag(game.GetComponentsInChildren<MenuSwipe>().Single(),-110);var page=game.GetComponentsInChildren<MenuSwipe>().Single();
            game.ReducedMotion=true;yield return null;Assert.That(page.IsSettling,Is.False);
            var content=page.transform.Find("Interactive shelf contents") as RectTransform;Assert.That(content.anchoredPosition,Is.EqualTo(Vector2.zero));
            yield return Drag(page,-110);Assert.That(Selected,Is.EqualTo(2));yield return null;Assert.That(game.GetComponentsInChildren<MenuSwipe>().Single().IsSettling,Is.False);
            game.ReducedMotion=false;yield return Drag(game.GetComponentsInChildren<MenuSwipe>().Single(),-110);game.ShowHome();yield return new WaitForSecondsRealtime(.5f);
            Assert.That(game.GetComponentsInChildren<MenuSwipe>().Length,Is.Zero);VerifyRows("rapid navigation home");
        }
        [UnityTest] public IEnumerator AttachedDecorationsStayRootedAndAnimateInEveryWorld()
        {
            foreach(int world in Enumerable.Range(0,9)){
                EditorType("WorldMenusReview").GetMethod("ConfigureWorld").Invoke(null,new object[]{game,world});game.ShowShop();yield return new WaitForSecondsRealtime(.5f);
                var plants=game.GetComponentsInChildren<AttachedPlants>();Assert.That(plants.Length,Is.GreaterThan(0));
                var stem=plants.First().GetComponentsInChildren<Image>().First(i=>i.name=="Rooted decorative sprig 0").rectTransform;
                var root=stem.anchoredPosition;var turn=stem.localRotation;yield return new WaitForSecondsRealtime(.9f);
                Assert.That(stem.anchoredPosition,Is.EqualTo(root),"The stem remains attached in world "+world);Assert.That(Quaternion.Angle(turn,stem.localRotation),Is.GreaterThan(.2f),"Visible movement in world "+world);
                game.ReducedMotion=true;yield return null;turn=stem.localRotation;yield return new WaitForSecondsRealtime(.1f);Assert.That(stem.localRotation,Is.EqualTo(turn));game.ReducedMotion=false;
                yield return Shot("world-"+world+"-rooted-plants");
            }
        }
        [UnityTest] public IEnumerator LocalizedNumericMeshesUseTheCurrentCopyDuringThatSameRender()
        {
            var digits=(Sprite[])typeof(CascadeGame).GetMethod("VictoryDigits",Private).Invoke(game,null);var symbols=(Sprite[])typeof(CascadeGame).GetMethod("VictorySymbols",Private).Invoke(game,null);
            var element=new GameObject("Localized numeric render fixture",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));element.transform.SetParent(game.GetComponentInChildren<Canvas>().transform,false);
            try{
                var rect=(RectTransform)element.transform;rect.sizeDelta=new Vector2(300,60);var source=element.GetComponent<TextMeshProUGUI>();source.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceTitle");source.fontSize=18;source.text="Moves: 3";
                Assert.That(element.AddComponent<InlineIllustratedDigits>().Initialize(source,digits,symbols),Is.True);Canvas.ForceUpdateCanvases();yield return null;
                var visible=element.transform.Find("Illustrated numeral copy").GetComponent<TextMeshProUGUI>();
                source.text="Ходы: 4";source.ForceMeshUpdate();
                Assert.That(visible.textInfo.characterInfo[0].character,Is.EqualTo('Х'),"The same render must contain the new localized glyphs, even when the invisible source updates after its visible copy");
            }finally{UnityEngine.Object.Destroy(element);}
        }
        [UnityTest,Timeout(300000)] public IEnumerator ActualCheckpointCountersKeepTheirDigitsSlashAndPaddingInAllLocales()
        {
            yield return ReachCheckpoint();
            foreach(int width in new[]{320,390,430})foreach(string locale in new[]{"en","ru","uk"})foreach(bool large in new[]{false,true}){
                Resize(width,width==320?693:width==430?932:844);game.SetUiLocale(locale);game.Preferences.LargeText=large;game.ShowGame();
                yield return Shot(locale+"-"+width+(large?"-large":"")+"-Checkpoint");
                var heading=game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="Outcome heading");
                Assert.That(heading.text,Does.Contain("1/3"),"A real completed first stage retains its stage heading");
                var written=heading.transform.Find("Illustrated numeral copy").GetComponent<TextMeshProUGUI>();written.ForceMeshUpdate();
                Assert.That(written.text,Does.Contain("<space=1.5px><size=112%>∕</size><space=1.5px>"));
                Assert.That(written.textInfo.characterInfo.Take(written.textInfo.characterCount).Count(c=>c.elementType==TMP_TextElementType.Sprite),Is.EqualTo(2));
                var receiver=game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name=="ReceiverLabel");
                var numberCopy=receiver.transform.Find("Illustrated numeral copy").GetComponent<TextMeshProUGUI>();
                Assert.That(receiver.text,Is.EqualTo(new UiCopy("Moves to deliver: {0}",game.Interaction.Attempt.RouteLength).ToString()),"The accessible source follows the UI locale");
                Assert.That(numberCopy.text,Does.StartWith(receiver.text.Split(' ')[0]),"The visible numeral copy must use the same language as its source: "+receiver.text+" / "+numberCopy.text);
                Assert.That(numberCopy.text,Does.Contain("<size=128%>"));
            }
        }
        [UnityTest,Timeout(300000)] public IEnumerator CompactFootersAndTabsHaveSeparateTouchAreasAcrossPhoneLocales()
        {
            foreach(int width in new[]{320,390,430,720})foreach(string locale in new[]{"en","ru","uk"})foreach(bool large in new[]{false,true}){
                Resize(width,width==720?1280:width==320?693:width==430?932:844);game.SetUiLocale(locale);game.Preferences.LargeText=large;
                string prefix=locale+"-"+width+(large?"-large":"");
                foreach(var screen in new[]{GameScreen.NoAds,GameScreen.Shop}){
                    Route(screen);yield return Shot(prefix+"-"+screen);var buttons=game.GetComponentsInChildren<Button>();
                    if(screen==GameScreen.NoAds){var safe=(RectTransform)Field("safeRoot");foreach(var control in buttons){var box=Ink((RectTransform)control.transform,safe);Assert.That(box.yMin,Is.GreaterThanOrEqualTo(safe.rect.yMin-.5f),control.name+" clears the safe bottom");}}
                    for(int i=0;i<buttons.Length;i++)for(int j=i+1;j<buttons.Length;j++){
                        var a=Ink((RectTransform)buttons[i].transform,(RectTransform)game.GetComponentInChildren<Canvas>().transform);
                        var b=Ink((RectTransform)buttons[j].transform,(RectTransform)game.GetComponentInChildren<Canvas>().transform);
                        float x=Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin),y=Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin);
                        Assert.That(x>.2f&&y>.2f,Is.False,screen+" touch overlap: "+buttons[i].name+" / "+buttons[j].name);
                    }
                }
                game.SelectShopTab(1);yield return Shot(prefix+"-Trophies");
            }
        }
        [UnityTest,Timeout(900000)] public IEnumerator MarkedScreensStayCentredOnPhonesInAllLocales()
        {
            foreach(int width in (Before||Smoke?new[]{390}:new[]{320,390,430}))foreach(string locale in (Before||Smoke?new[]{"en"}:new[]{"en","ru","uk"}))foreach(bool large in (Before||Smoke?new[]{false}:new[]{false,true})){
                Resize(width,width==320?693:width==430?932:844);game.SetUiLocale(locale);game.Preferences.LargeText=large;
                string prefix=locale+"-"+width+(large?"-large":"");
                foreach(var screen in new[]{GameScreen.Home,GameScreen.AccountGate,GameScreen.Pause,GameScreen.Fail,GameScreen.JourneyMap,GameScreen.NoAds,GameScreen.Shop,GameScreen.TravelBag}){
                    Route(screen);yield return Shot(prefix+"-"+screen);
                }
                game.ShowShop();game.SelectShopTab(1);yield return Shot(prefix+"-Trophies");
                game.ShowSupplies();yield return Shot(prefix+"-Supplies");game.ShowCollection();yield return Shot(prefix+"-Collection");
                Route(GameScreen.JourneyMap);Call("OpenConversation",StoryConversations.Visit(0),new UiCopy("Back to the map"),(Action)game.ShowJourney,null);game.AdvanceConversation();game.AdvanceConversation();yield return Shot(prefix+"-Dialogue");game.SkipConversation();
                game.OpenLegalDocument("privacy");yield return Shot(prefix+"-Privacy");game.CloseLegalDocument();
                game.OpenLegalDocument("terms");yield return Shot(prefix+"-Terms");game.CloseLegalDocument();
                yield return ReachCheckpoint();yield return Shot(prefix+"-Checkpoint");
            }
        }
    }
}
#endif
