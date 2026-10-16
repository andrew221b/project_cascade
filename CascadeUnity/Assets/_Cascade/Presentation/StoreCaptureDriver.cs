#if CASCADE_STORE_CAPTURE && UNITY_IOS && DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using UnityEngine;

namespace Cascade.Presentation
{
    // Included only in the dedicated Simulator screenshot build. No capture code in release players.
    public sealed class StoreCaptureDriver:MonoBehaviour
    {
        const string CommandFile="store-capture-command.txt",StatusFile="store-capture-status.txt";
        bool busy;string last="";float nextPoll;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            var p=new PlayerProfile();p.Preferences.UiLocale="en";p.Preferences.PuzzleLocale="en";
            p.Preferences.Tutorials=false;p.Preferences.NotificationsEnabled=false;p.Preferences.NotificationPermissionRequested=true;
            p.FirstGestureShown=true;p.SeenMechanics=Enum.GetValues(typeof(MechanicLesson)).Cast<MechanicLesson>().ToList();
            p.Missions.NextAfterClears=10000;
            p.Monetization.BonusCoins=220;p.Monetization.WayfinderScrolls=2;p.Monetization.UndoSpools=1;
            p.Monetization.ReturnHourglasses=1;p.Monetization.SignalLanterns=1;p.Monetization.LandingLenses=1;
            var catalog=ContentImporter.Import(Source("levels"),Source("localized_levels"),Source("words_en"),Source("words_ru"),Source("words_uk"),Source("campaign_extension"));
            foreach(var level in catalog.PrimaryLevels.Take(5))p.Clears.Add(new ClearReceipt{CampaignNodeId=level.Definition.CampaignNodeId,
                ContentHash=level.Definition.ContentHash,AttemptId=Guid.NewGuid().ToString("D"),BestStars=3,BestScore=1200});
            p.Rewards.Synchronize(p.Clears.Select(l=>l.CampaignNodeId));
            for(int world=0;world<JourneyStory.WorldCount;world++)p.HeardStory.Add(WorldPrologues.ForWorld(world).Receipt);
            new ProfileStore(Path.Combine(UnityEngine.Application.persistentDataPath,"cascade-profile.json")).Save(p);
            var owner=new GameObject("Simulator store screenshot controller");DontDestroyOnLoad(owner);owner.AddComponent<StoreCaptureDriver>();
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
            float deadline=Time.realtimeSinceStartup+60;
            while((g==null||!g.PreparationReady)&&Time.realtimeSinceStartup<deadline){yield return null;g=FindFirstObjectByType<CascadeGame>();}
            if(g==null){Write("error: no game");busy=false;yield break;}
            g.SkipConversation();g.Preferences.Tutorials=false;
            try{
                if(command=="map"){g.ShowJourney();g.SkipConversation();}
                else if(command=="home")g.ShowHome();
                else if(command=="collection")g.ShowCollection();
                else{
                    string node=command=="night"?"C161":command=="words"?"C37":command=="advanced"?"C65":"C03";
                    var level=g.Catalog.GetPrimary(node);
                    if(command=="words"&&g.Catalog.TryGetLocalizedBinding(node,"en",out var localized))level=localized;
                    g.StartCampaignLevel(level.Definition);g.ShowGame();
                }
            }catch(Exception e){Write("error: "+e.GetType().Name+" "+e.Message);busy=false;yield break;}
            deadline=Time.realtimeSinceStartup+60;
            while(!g.PreparationReady&&Time.realtimeSinceStartup<deadline)yield return null;
            g.Interaction.SetSuspended(false);
            if(command=="bag"&&!g.FieldToolTrayOpen)g.ToggleFieldToolTray();
            yield return new WaitForSecondsRealtime(2);
            g.RefreshLayout();Canvas.ForceUpdateCanvases();
            Write(command+" ready; "+g.CurrentScreen+"; "+Screen.width+"x"+Screen.height+"; "+DateTime.UtcNow.ToString("O"));busy=false;
        }
        static void Write(string value)=>File.WriteAllText(Path.Combine(UnityEngine.Application.persistentDataPath,StatusFile),value);
    }
}
#endif
