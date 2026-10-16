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
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    // Native screenshot catalogue. Fabricated progress belongs only to the temporary QA profile.
    public static class ScreenGalleryCapture
    {
        static string Phase="AllScreens-Oct08";
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        [Serializable] sealed class Entry { public string file,title;public int width,height;public string screen,capturedUtc; }
        static readonly List<Entry> entries=new List<Entry>();
        static readonly List<string> failures=new List<string>();
        public static bool Running { get; private set; }
        static string Folder=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Phase));
        public static void Prepare()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play before creating the QA profile.");
            FiveStarReview.Prepare();
            var store=new ProfileStore(Path.Combine(CascadeGame.EditorProfileDirectory,"cascade-profile.json"));
            var p=store.Load().Profile;
            string Read(string n)=>Resources.Load<TextAsset>("CascadeContent/"+n).text;
            var catalog=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
            foreach(var level in catalog.PrimaryLevels.Take(5))if(!p.Clears.Any(c=>c.CampaignNodeId==level.Definition.CampaignNodeId))
                p.Clears.Add(new ClearReceipt{CampaignNodeId=level.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=level.Definition.ContentHash,BestStars=3,BestScore=1200});
            p.Rewards.Synchronize(p.Clears.Select(c=>c.CampaignNodeId));
            p.Missions.NextAfterClears=10000;p.Missions.Offer=null;
            p.FirstGestureShown=true;p.SeenMechanics.Clear();p.SeenMechanics.AddRange((MechanicLesson[])Enum.GetValues(typeof(MechanicLesson)));
            foreach(var id in new[]{"bee_suit","explorer_hat","origami_cranes"})if(!p.Monetization.Cosmetics.Contains(id))p.Monetization.Cosmetics.Add(id);
            p.Monetization.WayfinderScrolls=p.Monetization.UndoSpools=p.Monetization.ReturnHourglasses=p.Monetization.SignalLanterns=p.Monetization.LandingLenses=1;
            p.SeenCoachPrompts.Add("trophy_preview_tap");p.SeenCoachPrompts.Add("map_island_tap");p.Preferences.UiLocale="ru";
            store.Save(p);
        }
        public static void Capture(string folder="AllScreens-Oct08")
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(Running||!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Compiled Play with isolated QA profile required.");
            if(Path.GetFileName(folder)!=folder)throw new ArgumentException("Use a single report folder name.");
            Phase=folder;Running=true;entries.Clear();failures.Clear();Directory.CreateDirectory(Folder);
            string complete=Path.Combine(Folder,"complete.txt");if(File.Exists(complete))File.Delete(complete);
            game.StartCoroutine(Run(game));
        }
        static object Field(CascadeGame g,string name)=>typeof(CascadeGame).GetField(name,Private).GetValue(g);
        public static void CaptureResponsiveCorrections()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated review required.");
            g.StartCoroutine(ResponsiveCorrections(g));
        }
        static IEnumerator ResponsiveCorrections(CascadeGame g)
        {
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.Preferences.LargeText=true;g.ReducedMotion=true;g.SetUiLocale("uk");
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            g.ShowSettings();Field(g,"settingsPage",0);Draw(g);yield return new WaitForSecondsRealtime(.2f);
            yield return PlayableCapture.CaptureSettled("ClarityFinal-Oct08","46-settings-large.png",320,693);
            Field(g,"mapPage",0);Field(g,"selectedJourneyStop",0);Screen(g,GameScreen.JourneyMap);
            g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(0,0)+5));yield return new WaitForSecondsRealtime(.2f);
            yield return PlayableCapture.CaptureSettled("ClarityFinal-Oct08","48-dialogue-large.png",320,693,owner=>{
                foreach(var t in owner.GetComponentsInChildren<TMPro.TextMeshProUGUI>())if(t.name=="Conversation line"||t.name=="Label"&&t.transform.parent.name=="Not now"){
                    t.ForceMeshUpdate();if(t.isTextOverflowing)throw new InvalidOperationException("Reader/action text overflow: "+t.name);
                }
            });
            g.SkipConversation();File.WriteAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/ClarityFinal-Oct08/responsive-fixes.txt")),
                "PASS: narrow UK Large Text settings and dialogue captures; dialogue chunk and Not now caption fit.");
        }
        static void Field(CascadeGame g,string name,object value)=>typeof(CascadeGame).GetField(name,Private).SetValue(g,value);
        static object Call(CascadeGame g,string name,params object[] args)=>typeof(CascadeGame).GetMethod(name,Private).Invoke(g,args);
        static void Screen(CascadeGame g,GameScreen screen)=>Call(g,"SetScreen",screen);
        static void Draw(CascadeGame g)=>Call(g,"DrawMenu");
        static IEnumerator Run(CascadeGame g)
        {
            int fps=UnityEngine.Application.targetFrameRate;bool persistence=(bool)Field(g,"persistenceReady");
            UnityEngine.Application.targetFrameRate=30;UnityEngine.Application.runInBackground=true;
            try {
                Field(g,"persistenceReady",false);g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
                g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=true;g.Preferences.LargeText=false;
                g.Preferences.Tutorials=false;
                g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");
                var p=(PlayerProfile)Field(g,"profile");
                p.Rewards.RecordDelivery(g.Interaction.Attempt,g.Descent?.Performance(g.Interaction.Attempt),p.ModeRecords);
                var checkpoint=g.Descent==null?null:JsonConvert.DeserializeObject<DescentRun>(JsonConvert.SerializeObject(g.Descent));
                var heard=(HashSet<int>)Field(g,"heardStory");for(int w=0;w<JourneyStory.WorldCount;w++)heard.Add(WorldPrologues.ForWorld(w).Receipt);
                yield return Shot("25-victory","25. Победа и статистика",()=>Screen(g,GameScreen.Win),settle:1);
                yield return Shot("27-parcel","27. Посылка",g.ShowParcel);
                yield return Shot("28-parcel-open","28. Полученная награда",()=>{int order=(int)Field(g,"parcelOrder");g.Rewards.ClaimParcel(order);Field(g,"parcelOpened",true);Screen(g,GameScreen.Parcel);},settle:1);
                if(p.Completion!=null)p.Completion.Acknowledged=true;
                Load("C03");
                yield return Shot("01-home","01. Главное меню",g.ShowHome);
                yield return Shot("02-settings","02. Настройки",g.ShowSettings);
                string[] settingNames={"Звук и движение","Удобство и помощь","Языки","Напоминания","Облачное сохранение"};
                for(int i=0;i<5;i++){int page=i;yield return Shot((3+i).ToString("00")+"-settings-"+i,(3+i).ToString("00")+". "+settingNames[i],()=>{g.ShowSettings();Field(g,"settingsPage",page);Draw(g);});}
                yield return Shot("08-map","08. Карта и переходы между мирами",()=>Map(0));
                yield return Shot("09-map-night","09. Ночной мир",()=>Map(8));
                yield return Shot("10-island","10. Уровни открытого острова",()=>{Map(0);g.OpenJourneyIsland(0);});
                yield return Shot("11a-shop-shelf","Полки инструментов",()=>{g.ShowShop();g.SelectShopTab(0);});
                yield return Shot("12a-trophy-shelf","Полки костюмов",()=>{g.ShowShop();g.SelectShopTab(1);});
                yield return Shot("11-shop-tools","11. Магазин — помощь",()=>{g.ShowShop();g.SelectShopTab(0);g.SelectShopItem(4);Call(g,"OpenSelectedShopItem");});
                yield return Shot("12-shop-mika","12. Магазин — костюмы Мики",()=>{g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(17);Call(g,"OpenSelectedShopItem");});
                yield return Shot("13-shop-background","13. Магазин — анимации фона",()=>{g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(ShopCatalog.MikaTrophyIds.Length);Call(g,"OpenSelectedShopItem");});
                yield return Shot("14-shop-frames","14. Магазин — рамки",()=>{g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(Array.FindIndex(ShopCatalog.TrophyIds,ShopCatalog.IsFrame));Call(g,"OpenSelectedShopItem");});
                yield return Shot("15-coins","15. Пакеты монет",g.ShowCoinPacks);
                yield return Shot("16-no-ads","16. Отключение рекламы",g.ShowNoAds);
                yield return Shot("17-bag","17. Мои вещи",g.ShowTravelBag);
                yield return Shot("18-trophies","18. Коллекция трофеев",()=>{g.ShowCollection();g.SelectShopItem(17);Call(g,"OpenSelectedShopItem");});
                yield return Shot("19-supplies","19. Сумка с инструментами",()=>{Load("C03");g.ShowSupplies();});
                yield return Shot("20-game","20. Игра — фигуры",()=>Load("C03"));
                yield return Shot("20a-inline-bag","Сумка — иконки над игровыми кнопками",()=>{},settle:.5f);
                yield return Shot("11b-tool-preview","Инструмент — описание и покупка",()=>{g.ShowShop();g.SelectShopTab(0);g.SelectShopItem(3);Call(g,"OpenSelectedShopItem");});
                yield return Shot("12b-costume-preview","Костюм — предпросмотр",()=>{g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(17);Call(g,"OpenSelectedShopItem");});
                yield return Shot("21-game-words","21. Игра — слова",()=>Load("C37","ru"));
                yield return Shot("22-game-mixed","22. Игра — микс",()=>Load("C32"));
                yield return Shot("23-pause","23. Пауза",()=>Screen(g,GameScreen.Pause));
                yield return Shot("24-checkpoint","24. Завершённый этап",()=>{
                    checkpoint.Completed.Clear();
                    var source=g.Catalog.PrimaryLevels.First(c=>c.Definition.ContentHash==checkpoint.SourceHash).Definition;
                    var a=new AttemptService(checkpoint.Level(source,0));
                    foreach(string token in checkpoint.Stages[0].Witness){var parts=token.TrimStart('R').Split(':');var command=token=="D"?PuzzleCommand.Drop:token.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(parts[0]),int.Parse(parts[1])):PuzzleCommand.Swap(int.Parse(token));if(!a.Execute(command).Accepted)throw new InvalidOperationException("Checkpoint witness rejected.");}
                    Call(g,"MountAttempt",a,checkpoint);g.ShowGame();
                });
                yield return Shot("26-failure","26. Проигрыш",()=>{var a=new AttemptService(g.Catalog.GetPrimary("C01").Definition);for(int i=0;i<8&&a.Current.Status==PuzzleStatus.Playing;i++)a.Execute(PuzzleCommand.Drop);if(a.Current.Status!=PuzzleStatus.Lost)throw new InvalidOperationException("Expected lost fixture.");Call(g,"MountAttempt",a,null);g.ShowGame();});
                yield return Shot("29-comic","29. Сюжетный комикс",()=>g.OpenWorldComic(0,()=>Map(0)));
                yield return Shot("30-dialogue","30. Диалог на карте",()=>{Map(0);g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(0,0)+5));},settle:.6f);
                yield return Shot("mission-offer","Личная миссия — принять или отказаться",()=>{
                    Map(0);Call(g,"CancelMissionPlan");var content=g.Catalog.GetPrimary("C03");
                    var store=new PublishedTriadStore(Resources.Load<TextAsset>("CascadeContent/production_triad_recipes").text,Resources.Load<TextAsset>("CascadeContent/production_triad_manifest").text);
                    var reference=store.Bind(content.Definition,content.Locale,g.Catalog.ContentHash);
                    var mission=CampaignTriads.BuildPersonalMission(content.Definition,p.Missions.Serial+1,null,default,reference:reference);
                    if(mission==null)throw new InvalidOperationException("No certified mission fixture.");
                    p.Missions.Offer=null;p.Missions.Present(new PersonalMissionOffer{Serial=p.Missions.Serial+1,Hero=0,SourceHash=content.Definition.ContentHash,Locale=content.Locale,Run=mission},p.Clears.Count);
                    Call(g,"ShowPersonalMissionOffer");
                });
                g.DeclinePersonalMission();
                g.SkipConversation();Load("C03");
                yield return Shot("31-guide","31. Как играть",g.ShowHowToPlay);
                yield return Shot("32-effect-guide","32. Объяснение эффекта",()=>{g.ShowDevices();Field(g,"deviceIndex",2);Draw(g);});
                yield return Shot("33-tutorial","33. Анимированное обучение",()=>{Load("C02");Call(g,"OpenMechanicPopup",MechanicLesson.Gust,null,default(UiCopy),null);},settle:1.3f);
                Call(g,"CloseMechanicPopup");
                yield return Shot("34-journal","34. Журнал",g.ShowJournal);
                yield return Shot("35-workshop","35. Мастерская",g.ShowWorkshop);
                yield return Shot("36-requests","36. Просьбы жителей",g.ShowRequests);
                yield return Shot("37-records","37. Рекорды",g.ShowRecords);
                yield return Shot("38-contracts","38. Задания",g.ShowContracts);
                yield return Shot("39-story-ending","39. Итог путешествия",()=>Screen(g,GameScreen.JourneyEnd));
                yield return Shot("40-word-language","40. Выбор языка слов",()=>g.ProposePuzzleLocale(g.Preferences.PuzzleLocale=="uk"?"ru":"uk"));
                yield return Shot("41-target","41. Просмотр цели",()=>{Load("C03");Call(g,"ToggleTargetZoom");});
                Call(g,"ToggleTargetZoom");
                yield return Shot("42-test-ad","42. Тестовая реклама с наградой",()=>g.ShowPreviewAd(true,_=>{}));
                Call(g,"FinishPreviewAd",false);
                yield return Shot("43-save-recovery","43. Восстановление сохранения",()=>{typeof(CascadeGame).GetProperty("Recovery").SetValue(g,SaveRecovery.AttemptUnavailable);Screen(g,GameScreen.SaveRecovery);});
                typeof(CascadeGame).GetProperty("Recovery").SetValue(g,SaveRecovery.None);
                yield return Shot("44-small-shop","44. Магазин — узкий экран",()=>{g.SetUiLocale("uk");g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(19);},320,693);
                yield return Shot("45-large-text","45. Меню — крупный текст",()=>{g.SetUiLocale("ru");g.Preferences.LargeText=true;g.ShowHome();},320,693);
                yield return Shot("46-settings-large","Настройки — крупный текст, UK",()=>{g.Preferences.LargeText=true;g.SetUiLocale("uk");g.ShowSettings();Field(g,"settingsPage",0);Draw(g);},320,693);
                yield return Shot("47-records-large","Рекорды — крупный текст, UK",g.ShowRecords,320,693);
                yield return Shot("48-dialogue-large","Диалог — крупный текст, UK",()=>{Map(0);g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(0,0)+5));},320,693);
                g.SkipConversation();
                yield return Shot("49-comic-large","Комикс — крупный текст, UK",()=>g.OpenWorldComic(8,()=>Map(8)),320,693);
                yield return Shot("50-tool-large","Предмет — крупный текст, UK",()=>{Field(g,"worldComic",-1);g.ShowShop();g.SelectShopTab(0);g.SelectShopItem(4);},320,693);
                yield return Shot("51-shop-english","Каталог — EN",()=>{g.Preferences.LargeText=false;g.SetUiLocale("en");g.ShowShop();g.SelectShopTab(0);},390,844);
                Write();File.WriteAllText(Path.Combine(Folder,"complete.txt"),entries.Count+" native Unity screenshots; "+failures.Count+" capture failures. Isolated QA progress; simulated portrait safe insets. No device/Simulator/build/audio validation.");
                void Map(int world){
                    g.SkipConversation();Call(g,"CancelMissionPlan");p.Missions.Offer=null;p.Missions.NextAfterClears=10000;
                    int count=JourneyStory.WorldFirst(world)+5;
                    var current=p.Clears.ToDictionary(c=>c.CampaignNodeId);
                    p.Clears=g.Catalog.PrimaryLevels.Take(count).Select(c=>current.TryGetValue(c.Definition.CampaignNodeId,out var receipt)?receipt:
                        new ClearReceipt{CampaignNodeId=c.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=c.Definition.ContentHash,BestStars=3,BestScore=1200}).ToList();
                    var session=(HashSet<string>)Field(g,"sessionClears");session.Clear();session.UnionWith(p.Clears.Select(c=>c.CampaignNodeId));
                    Field(g,"worldComic",-1);Field(g,"islandBrowseIndex",-1);Field(g,"mapPage",world);Field(g,"selectedJourneyStop",count);Screen(g,GameScreen.JourneyMap);
                }
                void Load(string id,string locale="en"){g.SkipConversation();if(locale!="en"&&g.Catalog.TryGetLocalizedBinding(id,locale,out var words))g.StartCampaignLevel(words.Definition,locale);else g.StartCampaignLevel(g.Catalog.GetPrimary(id).Definition,locale);g.ShowGame();}
            }
            finally {Field(g,"persistenceReady",persistence);UnityEngine.Application.targetFrameRate=fps;UnityEngine.Application.runInBackground=false;Running=false;Write();}
            IEnumerator Shot(string file,string title,Action setup,int width=390,int height=844,float settle=.20f){
                yield return Guard(CaptureOne(),file);
                IEnumerator CaptureOne(){setup();yield return null;yield return new WaitForSecondsRealtime(settle);if(g.CurrentScreen==GameScreen.Playing){float until=Time.realtimeSinceStartup+20;while(!g.PreparationReady&&Time.realtimeSinceStartup<until)yield return null;if(!g.PreparationReady)throw new InvalidOperationException("Game preparation did not settle.");}yield return PlayableCapture.CaptureSettled(Phase,file+".png",width,height,file=="20a-inline-bag"?(Action<CascadeGame>)(owner=>{
                    if(!owner.FieldToolTrayOpen)owner.ToggleFieldToolTray();
                    if(!owner.FieldToolTrayOpen)throw new InvalidOperationException("Inline bag did not open for the capture.");
                    owner.RefreshLayout();Canvas.ForceUpdateCanvases();
                }):null);entries.Add(new Entry{file=file+".png",title=title,width=width,height=height,screen=g.CurrentScreen.ToString(),capturedUtc=DateTime.UtcNow.ToString("O")});Write();}
            }
        }
        static void Write(){Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"screens.json"),JsonConvert.SerializeObject(entries.OrderBy(e=>e.file),Formatting.Indented));File.WriteAllLines(Path.Combine(Folder,"failures.txt"),failures);}
        static IEnumerator Guard(IEnumerator routine,string name)
        {
            var stack=new Stack<IEnumerator>();stack.Push(routine);
            while(stack.Count>0){object next=null;bool moved=false;Exception failure=null;
                try {moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
                if(failure!=null){failures.Add(name+": "+failure.GetBaseException());while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();yield break;}
                if(!moved){(stack.Pop() as IDisposable)?.Dispose();continue;}
                if(next is IEnumerator nested)stack.Push(nested);else yield return next;
            }
        }
    }
}
