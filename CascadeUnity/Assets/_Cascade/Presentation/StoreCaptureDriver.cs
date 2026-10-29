#if CASCADE_STORE_CAPTURE
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using UnityEngine;

namespace Cascade.Presentation
{
    // A separate, offline native player renders the shipping UI and authored puzzles.
    // This controller and its fixture are absent from ordinary and release builds.
    public sealed class StoreCaptureDriver:MonoBehaviour
    {
        [Serializable] sealed class Request {public string id,locale,scene;}
        const string CommandFile="store-capture-command.txt",StatusFile="store-capture-status.txt";
        bool busy;string last="";float nextPoll;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            if(UnityEngine.Application.identifier!="com.vickand.flyingladders.capture")
                throw new InvalidOperationException("Capture fixture requires its dedicated application identity.");
            PlayerPrefs.SetInt(AudiencePolicy.PreferenceKey,(int)AudienceAgeBand.SixteenPlus);PlayerPrefs.Save();
            var p=new PlayerProfile();p.AccountIntroSeen=true;
            p.Preferences.UiLocale="en";p.Preferences.PuzzleLocale="en";p.Preferences.FollowSystemLanguage=false;
            p.Preferences.Tutorials=false;p.Preferences.NotificationsEnabled=false;p.Preferences.NotificationPermissionRequested=true;
            p.FirstGestureShown=true;p.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            p.Missions.NextAfterClears=10000;
            p.Monetization.BonusCoins=2200;p.Monetization.WayfinderScrolls=3;p.Monetization.UndoSpools=2;
            p.Monetization.ReturnHourglasses=1;p.Monetization.SignalLanterns=2;p.Monetization.LandingLenses=1;
            p.Monetization.Cosmetics.Add("sky_pilot");p.Monetization.SpentCoins=ShopCatalog.TrophyPrice("sky_pilot");
            var catalog=ContentImporter.Import(Source("levels"),Source("localized_levels"),Source("words_en"),Source("words_ru"),Source("words_uk"),Source("campaign_extension"));
            foreach(var level in catalog.PrimaryLevels.Take(420))p.Clears.Add(new ClearReceipt{CampaignNodeId=level.Definition.CampaignNodeId,
                ContentHash=level.Definition.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=3,BestScore=1200});
            p.Rewards.Synchronize(p.Clears.Select(l=>l.CampaignNodeId));
            p.HeardStory=Enumerable.Range(0,10000).ToList();
            new ProfileStore(Path.Combine(UnityEngine.Application.persistentDataPath,"cascade-profile.json")).Save(p);
            var owner=new GameObject("Offline store screenshot controller");DontDestroyOnLoad(owner);owner.AddComponent<StoreCaptureDriver>();
        }
        static string Source(string name)=>Resources.Load<TextAsset>("CascadeContent/"+name).text;
        void Update()
        {
            if(busy||Time.realtimeSinceStartup<nextPoll)return;nextPoll=Time.realtimeSinceStartup+.25f;
            string path=Path.Combine(UnityEngine.Application.persistentDataPath,CommandFile);
            if(!File.Exists(path))return;string command=File.ReadAllText(path).Trim();
            if(command==last||command.Length==0)return;last=command;StartCoroutine(Select(command));
        }
        IEnumerator Select(string command)
        {
            busy=true;var g=FindFirstObjectByType<CascadeGame>();
            float deadline=Time.realtimeSinceStartup+180;
            while((g==null||!g.PreparationReady)&&Time.realtimeSinceStartup<deadline){yield return null;g=FindFirstObjectByType<CascadeGame>();}
            Request request=null;Exception failure=null;
            try{request=JsonUtility.FromJson<Request>(command);if(g==null||request==null||string.IsNullOrEmpty(request.id)||!(new[]{"en","ru","uk"}).Contains(request.locale))throw new InvalidOperationException("Invalid capture request.");g.SetGameLanguage(request.locale);}
            catch(Exception e){failure=e;}
            if(failure!=null){Write("error: "+failure.Message);busy=false;yield break;}
            yield return new WaitForSecondsRealtime(2);
            try{g.CaptureStoreScene(request.scene,request.locale);}catch(Exception e){failure=e;}
            if(failure!=null){Write("error: "+failure.GetType().Name+" "+failure.Message);busy=false;yield break;}
            deadline=Time.realtimeSinceStartup+180;
            while(!g.PreparationReady&&Time.realtimeSinceStartup<deadline)yield return null;
            g.Interaction.SetSuspended(false);
            yield return new WaitForSecondsRealtime(3);
            g.RefreshLayout();Canvas.ForceUpdateCanvases();
            Write(request.id+" ready; "+g.CurrentScreen+"; "+Screen.width+"x"+Screen.height+"; "+request.locale+"; "+DateTime.UtcNow.ToString("O"));busy=false;
        }
        static void Write(string value)=>File.WriteAllText(Path.Combine(UnityEngine.Application.persistentDataPath,StatusFile),value);
    }
    public sealed partial class CascadeGame
    {
        internal void CaptureStoreScene(string scene,string locale)
        {
            SkipConversation();Preferences.Tutorials=false;firstGestureShown=true;
            if(scene=="trophies"){ShowCollection();SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,"sky_pilot"));return;}
            if(scene=="map"){mapPage=4;selectedJourneyStop=JourneyStory.WorldFirst(4)+24;islandBrowseIndex=-1;SetScreen(GameScreen.JourneyMap);return;}
            if(scene=="account"){ShowAccountGate();return;}
            if(scene=="home"){ShowHome();return;}
            string node=scene=="words-rain"?"C19":scene=="mix-lagoon"?"C70":scene=="shapes-snow"?"C145":"C03";
            var mode=scene=="words-rain"?PuzzleMode.Words:scene=="mix-lagoon"?PuzzleMode.Mixed:PuzzleMode.Shapes;
            PreferredMode=mode;profile.PreferredMode=mode.ToString();
            var level=mode==PuzzleMode.Mixed?MixedJourney.Sequence(Catalog,locale).First(c=>c.Definition.CampaignNodeId==node):Catalog.GetPrimary(node);
            if(mode==PuzzleMode.Words&&Catalog.TryGetLocalizedBinding(node,locale,out var localized))level=localized;
            if(mode==PuzzleMode.Mixed&&level.Mode!=PuzzleMode.Mixed)throw new InvalidOperationException("Requested scene must show simultaneous letters and shapes.");
            StartCampaignLevel(level.Definition,locale);ShowGame();
        }
    }
}
#endif
