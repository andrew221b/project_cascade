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
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class UiCenteringRegressionTests
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        CascadeGame game;Camera camera;RenderTexture target,oldTarget;string directory;bool background,audio;
        string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/UiCenteringOct16"));
        object Field(string key)=>typeof(CascadeGame).GetField(key,Flags).GetValue(game);
        object Call(string key,params object[] args)=>typeof(CascadeGame).GetMethod(key,Flags).Invoke(game,args);
        TextMeshProUGUI Label(string key)=>game.GetComponentsInChildren<TextMeshProUGUI>().Single(t=>t.name==key);
        [UnitySetUp] public IEnumerator Setup()
        {
            background=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;audio=AudioListener.pause;AudioListener.pause=true;
            directory=Path.Combine(Path.GetTempPath(),"CascadeNavigationReview",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=directory;
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            foreach(string locale in new[]{"en","ru","uk"})yield return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableAsync(UiCopy.Table,UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(locale));
            yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();camera=Camera.main;oldTarget=camera.targetTexture;Resize(390,844);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.ReducedMotion=true;game.Preferences.Tutorials=false;game.Coach.Enabled=false;
            ((PlayerProfile)Field("profile")).AccountIntroSeen=true;((PlayerProfile)Field("profile")).Missions.NextAfterClears=20000;game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            yield return null;
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
        Rect Ink(TextMeshProUGUI label,RectTransform area)
        {
            label.ForceMeshUpdate();var bounds=CenteredContentRow.GlyphBounds(label);var low=(Vector2)area.InverseTransformPoint(label.transform.TransformPoint(bounds.min));var high=(Vector2)area.InverseTransformPoint(label.transform.TransformPoint(bounds.max));
            return Rect.MinMaxRect(low.x,low.y,high.x,high.y);
        }
        void Centered(TextMeshProUGUI label,RectTransform area,string key,float padding=0)
        {
            var ink=Ink(label,area);float unit=CenteredContentRow.Unit(area);var reading=CenteredContentRow.ReadingRect(area);
            Assert.That(ink.center.x,Is.EqualTo(reading.center.x).Within(.8f*unit),key+" horizontal painted centre");
            Assert.That(ink.center.y,Is.EqualTo(reading.center.y).Within(.8f*unit),key+" vertical painted centre");
            Assert.That(ink.xMin,Is.GreaterThanOrEqualTo(reading.xMin+padding*unit-.8f*unit),key+" left padding");Assert.That(ink.xMax,Is.LessThanOrEqualTo(reading.xMax-padding*unit+.8f*unit),key+" right padding");
            Assert.That(ink.yMin,Is.GreaterThanOrEqualTo(reading.yMin-.8f*unit));Assert.That(ink.yMax,Is.LessThanOrEqualTo(reading.yMax+.8f*unit));
        }
        void StoryActionsFit(string primary,string tertiary)
        {
            var controls=new[]{primary,tertiary}.Select(name=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name)).ToArray();var boxes=new List<Rect>();
            foreach(var control in controls){
                var corners=new Vector3[4];((RectTransform)control.transform).GetWorldCorners(corners);var a=camera.WorldToScreenPoint(corners[0]);var b=camera.WorldToScreenPoint(corners[2]);var box=Rect.MinMaxRect(a.x,a.y,b.x,b.y);boxes.Add(box);
                Assert.That(box.xMin,Is.GreaterThanOrEqualTo(-.2f),control.name+" left screen bound");Assert.That(box.xMax,Is.LessThanOrEqualTo(target.width+.2f),control.name+" right screen bound");
                Assert.That(box.yMin,Is.GreaterThanOrEqualTo(-.2f),control.name+" bottom screen bound");Assert.That(box.yMax,Is.LessThanOrEqualTo(target.height+.2f),control.name+" top screen bound");
                Assert.That(box.width,Is.GreaterThanOrEqualTo(43.8f),control.name+" touch width");Assert.That(box.height,Is.GreaterThanOrEqualTo(43.8f),control.name+" touch height");
            }
            Assert.That(boxes[0].Overlaps(boxes[1]),Is.False,"Story controls have independent touch regions");
            foreach(var control in controls)foreach(var row in control.GetComponentsInChildren<CenteredContentRow>().Where(r=>r.enabled)){
                var inks=row.Elements.Select(element=>{
                    var text=element.GetComponent<TextMeshProUGUI>();if(text!=null)return Ink(text,row.Surface);
                    var a=(Vector2)row.Surface.InverseTransformPoint(element.TransformPoint(element.rect.min));var b=(Vector2)row.Surface.InverseTransformPoint(element.TransformPoint(element.rect.max));return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
                }).ToArray();
                if(inks.Length==0)continue;float unit=row.LocalUnit;var reading=CenteredContentRow.ReadingRect(row.Surface);
                Assert.That((inks.Min(r=>r.xMin)+inks.Max(r=>r.xMax))*.5f,Is.EqualTo(reading.center.x).Within(.8f*unit),control.name+" icon/caption horizontal centre");
                Assert.That((inks.Min(r=>r.yMin)+inks.Max(r=>r.yMax))*.5f,Is.EqualTo(reading.center.y).Within(.8f*unit),control.name+" icon/caption vertical centre");
                for(int i=1;i<inks.Length;i++)Assert.That(inks[i].xMin-inks[i-1].xMax,Is.EqualTo(4*unit).Within(.6f*unit),control.name+" icon/caption gap");
            }
            var design=controls[0].GetComponent<GameButtonDesign>();var caption=controls[0].GetComponentInChildren<LocalizedCaption>();
            if(caption?.SourceIdentity=="Map"){
                Assert.That(design.Preset.Role,Is.EqualTo(GameButtonRole.SecondaryYellow));Assert.That(controls[0].GetComponentsInChildren<Image>().Count(i=>i.name=="Shared map scroll icon"),Is.EqualTo(1),"Every story Map uses the failure-screen scroll");
            }
        }
        IEnumerator Shot(string name)
        {
            yield return null;yield return null;game.RefreshLayout();Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
            var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);pixels.Apply();Directory.CreateDirectory(Output);File.WriteAllBytes(Path.Combine(Output,name+".png"),pixels.EncodeToPNG());RenderTexture.active=previous;UnityEngine.Object.Destroy(pixels);
        }
        void Conversation(int world)
        {
            // ShowJourney selects the player's next stop, which resets a fresh
            // profile to world zero. Route this isolated visual fixture directly.
            typeof(CascadeGame).GetField("mapPage",Flags).SetValue(game,world);Call("SetScreen",GameScreen.JourneyMap);
            Call("OpenConversation",StoryConversations.IslandOpening(world,0),new UiCopy("Back to the map"),(Action)(()=>{}),null);
            Assert.That(game.EnvironmentWorld,Is.EqualTo(world),"Capture the requested world's actual skin");
        }
        Color32[] Pixels()
        {
            Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();
            var result=texture.GetPixels32();RenderTexture.active=previous;UnityEngine.Object.Destroy(texture);return result;
        }
        float PlaquePixelOffset(TextMeshProUGUI label,Image board)
        {
            var painted=Pixels();var hiding=label.gameObject.AddComponent<CanvasGroup>();hiding.alpha=0;var empty=Pixels();
            var corners=new Vector3[4];board.rectTransform.GetWorldCorners(corners);var low=camera.WorldToScreenPoint(corners[0]);var high=camera.WorldToScreenPoint(corners[2]);
            int x0=Mathf.Clamp(Mathf.CeilToInt(low.x),0,target.width-1),x1=Mathf.Clamp(Mathf.FloorToInt(high.x),0,target.width-1);
            int y0=Mathf.Clamp(Mathf.CeilToInt(low.y),0,target.height-1),y1=Mathf.Clamp(Mathf.FloorToInt(high.y),0,target.height-1);
            int inkLow=y1,inkHigh=y0;bool found=false;
            for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){
                var a=painted[y*target.width+x];var b=empty[y*target.width+x];
                if(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)<90)continue;
                found=true;inkLow=Mathf.Min(inkLow,y);inkHigh=Mathf.Max(inkHigh,y);
            }
            Assert.That(found,Is.True,label.name+" must actually paint glyphs");
            // Snow turns the wooden rim white. Classify the writing field from
            // the same rendered artwork without its seasonal colour shader;
            // otherwise white wood is falsely counted as more parchment.
            var seasonal=board.material;board.material=null;var original=Pixels();board.material=seasonal;hiding.alpha=1;UnityEngine.Object.Destroy(hiding);Canvas.ForceUpdateCanvases();
            int middle=(x0+x1)/2,start=-1,bestLow=0,bestHigh=-1;
            for(int y=y0;y<=y1+1;y++){
                var pixel=y<=y1?original[y*target.width+middle]:new Color32();bool paper=y<=y1&&pixel.r>180&&pixel.g>160&&pixel.b>90;
                if(paper&&start<0)start=y;
                if(!paper&&start>=0){if(y-start>bestHigh-bestLow+1){bestLow=start;bestHigh=y-1;}start=-1;}
            }
            Assert.That(bestHigh-bestLow,Is.GreaterThan(8),board.name+" must have a visible parchment field");
            return (inkLow+inkHigh-bestLow-bestHigh)*.5f;
        }
        [UnityTest,Timeout(300000)] public IEnumerator ComicPlaquesCentreRealRenderedLettersInTheParchmentField()
        {
            var measurements=new System.Text.StringBuilder();float worst=0;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932)})foreach(string locale in new[]{"en","ru","uk"}){
                Resize(size.x,size.y);game.SetUiLocale(locale);game.Preferences.LargeText=size.x!=390;
                for(int world=0;world<9;world++){
                    game.OpenWorldComic(world,game.ShowHome);yield return null;yield return null;
                    foreach(string key in new[]{"Comic title","World comic speaker"}){
                        var label=Label(key);float offset=PlaquePixelOffset(label,label.GetComponentInParent<Image>());worst=Mathf.Max(worst,Mathf.Abs(offset));measurements.AppendLine(locale+","+size.x+","+world+","+key+","+offset);
                    }
                    if(locale=="ru"&&size.x==390&&world==0)yield return Shot("plaque-comic-corrected");
                    game.FinishWorldComic();yield return null;Conversation(world);yield return null;yield return null;
                    var speaker=Label("Speaker");float nameOffset=PlaquePixelOffset(speaker,speaker.GetComponentInParent<Image>());worst=Mathf.Max(worst,Mathf.Abs(nameOffset));measurements.AppendLine(locale+","+size.x+","+world+",NPC name,"+nameOffset);
                    if(locale=="ru"&&size.x==390&&world==0)yield return Shot("plaque-conversation-corrected");
                    game.SkipConversation();yield return null;yield return null;
                    var map=Label("Reference shop map world heading");Assert.That(map.text,Is.EqualTo(new UiCopy(JourneyStory.ZoneName(world)).ToString()));float mapOffset=PlaquePixelOffset(map,map.GetComponentInParent<Image>());worst=Mathf.Max(worst,Mathf.Abs(mapOffset));measurements.AppendLine(locale+","+size.x+","+world+",Map title,"+mapOffset);
                    if(locale=="ru"&&size.x==390&&world==0)yield return Shot("plaque-map-corrected");
                    if(locale=="ru"&&size.x==320&&world==2)yield return Shot("plaque-map-copper-320");
                }
            }
            Directory.CreateDirectory(Output);File.WriteAllText(Path.Combine(Output,"plaque-pixel-metrics.csv"),"locale,width,world,label,vertical pixel offset\n"+measurements);
            Assert.That(worst,Is.LessThanOrEqualTo(1.25f),"Real rendered letters relative to visible parchment (independent colour/difference measurement)");
        }
        [UnityTest] public IEnumerator ComicAndConversationParagraphsUseThePaperCentreWithBalancedMargins()
        {
            game.SetUiLocale("ru");game.OpenWorldComic(0,game.ShowHome);yield return Shot("comic-ru-phone");
            Centered(Label("World comic spoken line"),(RectTransform)Label("World comic spoken line").GetComponentInParent<ScrollRect>().transform.parent,"Comic paper");
            game.FinishWorldComic();yield return null;Conversation(0);yield return Shot("conversation-ru-phone");
            Centered(Label("Conversation line"),(RectTransform)Field("conversationPlate"),"Conversation paper");
        }
        [UnityTest] public IEnumerator PlainReferenceActionGetsTheSharedPaintedCentreAutomatically()
        {
            var host=new GameObject("Reference fallback test",typeof(RectTransform));host.transform.SetParent((RectTransform)Field("menuRoot"),false);((RectTransform)host.transform).sizeDelta=new Vector2(720,1280);
            var button=(Button)Call("ReferenceButton","Unbound action",host.transform,100f,200f,400f,100f,(Action)(()=>{}),"GreenAction");
            var body=(Transform)Call("ReferenceButtonBody",button);
            var label=(TextMeshProUGUI)Call("ReferenceText","off-centre source",body,UiCopy.Raw("Continue"),23f,3f,340f,60f,38f,true,false);
            yield return null;yield return null;Canvas.ForceUpdateCanvases();
            Assert.That(body.GetComponent<CenteredContentRow>(),Is.Not.Null,"Every reference action has a common centring owner");Centered(label,(RectTransform)body,"Unbound action");
            var iconButton=(Button)Call("ReferenceButton","Unbound icon action",host.transform,100f,350f,400f,100f,(Action)(()=>{}),"GreenAction");
            var iconBody=(RectTransform)(Transform)Call("ReferenceButtonBody",iconButton);
            var emblem=(Image)Call("ReferencePicture","Action coin",iconBody,(Sprite)Call("ReferenceShopCoin"),10f,5f,44f,44f,true);
            var caption=(TextMeshProUGUI)Call("ReferenceText","icon action caption",iconBody,UiCopy.Raw("Continue"),73f,8f,290f,66f,38f,true,false);
            yield return null;yield return null;Canvas.ForceUpdateCanvases();
            var row=iconBody.GetComponent<CenteredContentRow>();Assert.That(row.Elements.Length,Is.EqualTo(2));
            var a=(Vector2)iconBody.InverseTransformPoint(emblem.transform.TransformPoint(emblem.rectTransform.rect.min));var b=(Vector2)iconBody.InverseTransformPoint(emblem.transform.TransformPoint(emblem.rectTransform.rect.max));var text=Ink(caption,iconBody);
            float unit=row.LocalUnit;var writing=CenteredContentRow.ReadingRect(iconBody);
            Assert.That((a.x+text.xMax)*.5f,Is.EqualTo(writing.center.x).Within(.8f*unit),"Combined icon and caption centre");
            Assert.That((a.y+b.y)*.5f,Is.EqualTo(writing.center.y).Within(.8f*unit));Assert.That(text.center.y,Is.EqualTo(writing.center.y).Within(.8f*unit));
            Assert.That(text.xMin-b.x,Is.EqualTo(4*unit).Within(.6f*unit),"Four logical pixels between icon and caption");
        }
        IEnumerator SelectSettledLocale(string locale)
        {
            game.SetUiLocale(locale);float deadline=Time.realtimeSinceStartup+10;
            while(((bool)Field("applyingUiLanguage")||game.UiLanguage!=locale)&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(game.UiLanguage,Is.EqualTo(locale),"Validate the requested active language");
            Assert.That((bool)Field("applyingUiLanguage"),Is.False,"Finish rebuilding raw reference captions before capture");
        }
        void PaddedInk(TextMeshProUGUI label,RectTransform surface,string context)
        {
            var ink=Ink(label,surface);if(ink.width<=0||ink.height<=0)return;
            float unit=CenteredContentRow.Unit(surface),inset=2*unit,tolerance=.15f*unit;
            var field=CenteredContentRow.ReadingRect(surface);
            Assert.That(ink.xMin,Is.GreaterThanOrEqualTo(field.xMin+inset-tolerance),context+" left clearance");
            Assert.That(ink.xMax,Is.LessThanOrEqualTo(field.xMax-inset+tolerance),context+" right clearance");
            Assert.That(ink.yMin,Is.GreaterThanOrEqualTo(field.yMin+inset-tolerance),context+" bottom clearance");
            Assert.That(ink.yMax,Is.LessThanOrEqualTo(field.yMax-inset+tolerance),context+" top clearance");
        }
        [UnityTest] public IEnumerator TightActionFitsBothAxesEvenWithAFixedCaptionSize()
        {
            var host=new GameObject("Tight framed action",typeof(RectTransform));host.transform.SetParent((RectTransform)Field("menuRoot"),false);((RectTransform)host.transform).sizeDelta=new Vector2(720,1280);
            var button=(Button)Call("ReferenceButton","Tight action",host.transform,100f,200f,400f,50f,(Action)(()=>{}),"GreenAction");
            var body=(RectTransform)(Transform)Call("ReferenceButtonBody",button);
            var label=(TextMeshProUGUI)Call("ReferenceText","tight fixed-size caption",body,UiCopy.Raw("Restore purchases · 1 499,00 ₴"),0f,0f,400f,50f,90f,true,false);
            label.fontSizeMin=label.fontSizeMax=90;
            yield return null;yield return null;Canvas.ForceUpdateCanvases();
            var row=body.GetComponent<CenteredContentRow>();Assert.That(row,Is.Not.Null);row.Refresh();PaddedInk(label,body,"Fixed-size action");
        }
        [UnityTest,Timeout(300000)] public IEnumerator LocalizedSubscriptionPricesKeepTwoPixelsInsideEveryFrame()
        {
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932)})foreach(string locale in new[]{"en","ru","uk"}){
                Resize(size.x,size.y);yield return SelectSettledLocale(locale);game.Preferences.LargeText=size.x!=390;
                foreach(string product in Cascade.Application.PurchaseCatalog.NoAdsSubscriptions){
                    typeof(CascadeGame).GetField("selectedNoAdsProduct",Flags).SetValue(game,product);game.ShowNoAds();Call("DrawMenu");
                    yield return null;yield return null;
                    foreach(var date in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop calendar day "))){
                        date.ForceMeshUpdate();Assert.That(date.textInfo.characterInfo.Take(date.textInfo.characterCount).Any(c=>c.isVisible),Is.True,date.name+" paints its digits instead of truncating the whole value");
                    }
                    var action=Label("Reference shop no ads main action");
                    action.SetCopy(UiCopy.Raw(locale=="ru"?"Убрать рекламу на неделю · 1 499,00 ₽":locale=="uk"?"Прибрати рекламу на тиждень · 1 499,00 ₴":"Remove ads for a week · $1.49"));
                    var billing=Label("Reference shop subscription billing terms");
                    billing.SetCopy(UiCopy.Raw(locale=="ru"?"1 499,00 ₽ каждую неделю до отмены":locale=="uk"?"1 499,00 ₴ щотижня до скасування":"$1.49 billed weekly until cancelled"));
                    foreach(var row in game.GetComponentsInChildren<CenteredContentRow>())row.Refresh();Canvas.ForceUpdateCanvases();
                    foreach(var row in game.GetComponentsInChildren<CenteredContentRow>())if(row.Elements!=null)foreach(var element in row.Elements){
                        var text=element.GetComponent<TextMeshProUGUI>();if(text!=null)PaddedInk(text,row.Surface,locale+size.x+product+text.name);
                    }
                    foreach(string key in new[]{"Reference shop subscription terms label","Reference shop subscription privacy label"}){
                        var label=Label(key);var corners=new Vector3[4];label.rectTransform.GetWorldCorners(corners);
                        var ink=CenteredContentRow.GlyphBounds(label);float bottom=camera.WorldToScreenPoint(label.transform.TransformPoint(ink.min)).y;
                        Assert.That(bottom,Is.GreaterThanOrEqualTo(2-.2f),key+" remains above the phone edge");
                    }
                    if(product==Cascade.Application.PurchaseCatalog.NoAdsWeek)yield return Shot("padding-noads-"+locale+"-"+size.x);
                }
            }
        }
        [UnityTest,Timeout(300000)] public IEnumerator SharedMenuRowsKeepTwoPixelsInsideTheirWritingFields()
        {
            var screens=new[]{GameScreen.Home,GameScreen.Shop,GameScreen.CoinPacks,GameScreen.Settings,GameScreen.AccountGate,GameScreen.TravelBag,GameScreen.Supplies,GameScreen.JourneyMap,GameScreen.Pause};
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932)})foreach(string locale in new[]{"en","ru","uk"}){
                Resize(size.x,size.y);yield return SelectSettledLocale(locale);game.Preferences.LargeText=size.x!=390;
                foreach(var screen in screens){
                    if(screen==GameScreen.Settings)typeof(CascadeGame).GetField("settingsPage",Flags).SetValue(game,-1);
                    Call("SetScreen",screen);yield return null;yield return null;Canvas.ForceUpdateCanvases();
                    foreach(var row in game.GetComponentsInChildren<CenteredContentRow>())if(row.Elements!=null){
                        row.Refresh();foreach(var element in row.Elements){var text=element.GetComponent<TextMeshProUGUI>();if(text!=null)PaddedInk(text,row.Surface,locale+size.x+screen+text.name);}
                    }
                    if(size.x==390&&locale=="ru")yield return Shot("padding-"+screen.ToString().ToLowerInvariant()+"-ru-phone");
                }
            }
        }
        [UnityTest,Timeout(300000)] public IEnumerator LocalizedComicsAndConversationsStayCentredAfterPageAndPhoneChanges()
        {
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932)})foreach(string locale in new[]{"en","ru","uk"}){
                Resize(size.x,size.y);game.SetUiLocale(locale);game.Preferences.LargeText=size.x!=390;
                foreach(int world in new[]{0,4,8}){
                    game.OpenWorldComic(world,game.ShowHome);yield return null;yield return null;
                    for(int turn=0;turn<8;turn++){
                        Canvas.ForceUpdateCanvases();StoryActionsFit("Next comic page","Skip comic");var body=Label("World comic spoken line");Centered(body,(RectTransform)body.GetComponentInParent<ScrollRect>().transform.parent,"Comic "+locale+size.x+world+turn);
                        Centered(Label("World comic speaker"),(RectTransform)Label("World comic speaker").GetComponentInParent<Image>().transform,"Comic name");
                        foreach(var group in game.GetComponentsInChildren<CenteredContentRow>())if(group.enabled&&group.Elements!=null&&group.Elements.Length==1&&group.Elements[0].parent==group.Surface&&group.Elements[0].GetComponent<TextMeshProUGUI>() is TextMeshProUGUI text)Centered(text,group.Surface,group.name);
                        if(turn<7){game.GetComponentsInChildren<Button>().Single(b=>b.name=="Next comic page").onClick.Invoke();yield return null;}
                    }
                    yield return Shot("comic-"+world+"-"+locale+"-"+size.x);game.FinishWorldComic();yield return null;
                    Conversation(world);yield return null;yield return null;
                    var scene=StoryConversations.IslandOpening(world,0);
                    for(int turn=0;turn<scene.Lines.Length;turn++){
                        Canvas.ForceUpdateCanvases();StoryActionsFit("Next line","Not now");Centered(Label("Conversation line"),(RectTransform)Field("conversationPlate"),"Conversation "+locale+size.x+world+turn);
                        Centered(Label("Speaker"),(RectTransform)Field("conversationName"),"Conversation name");
                        if(turn+1<scene.Lines.Length){game.AdvanceConversation();yield return null;}
                    }
                    yield return Shot("conversation-"+world+"-"+locale+"-"+size.x);game.SkipConversation();yield return null;
                }
            }
        }
    }
}
#endif
