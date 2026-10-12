using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class TrophyVisualReview
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        const string Folder="TrophyWardrobe-Oct07";

        [MenuItem("CASCADE/QA/Capture trophy wardrobe")]
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeWorldAtmosphereReview"))
                throw new InvalidOperationException("Prepare the isolated world atmosphere profile before Play Mode.");
            game.StartCoroutine(Review(game));
        }
        static void PoseDiscovery(CascadeGame game,float phase)
        {
            var cue=game.GetComponentsInChildren<TrophyDiscoveryCue>().Last();
            typeof(TrophyDiscoveryCue).GetField("clock",Private).SetValue(cue,phase);
            typeof(TrophyDiscoveryCue).GetMethod("LateUpdate",Private).Invoke(cue,null);
        }
        static IEnumerator Review(CascadeGame game)
        {
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",Private).GetValue(game);
            typeof(CascadeGame).GetField("persistenceReady",Private).SetValue(game,false);
            profile.Rewards=new JourneyRewards();
            profile.Monetization.Cosmetics.Clear();profile.Monetization.Cosmetics.AddRange(new[]{"leaf","explorer_hat","star_knight"});
            profile.SeenCoachPrompts.Remove("trophy_preview_tap");
            var heard=(System.Collections.Generic.HashSet<int>)typeof(CascadeGame).GetField("heardStory",Private).GetValue(game);
            for(int world=0;world<JourneyStory.WorldCount;world++)heard.Add(WorldPrologues.ForWorld(world).Receipt);
            heard.Add(1000);
            heard.Add(StoryConversations.IslandOpening(0,0).Receipt);
            if(StoryConversations.HasLevelAside(0))heard.Add(StoryConversations.LevelAside(0).Receipt);
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Preferences.LargeText=false;game.Preferences.EffectQuality=2;game.ReducedMotion=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            game.SetUiLocale("en");game.ShowCollection();game.SelectShopItem(0);
            yield return PlayableCapture.CaptureSettled(Folder,"collection-page1-en.png",390,844);
            game.SetUiLocale("ru");game.SelectShopItem(12);
            yield return PlayableCapture.CaptureSettled(Folder,"collection-page2-ru.png",390,844);
            yield return PlayableCapture.CaptureSettled(Folder,"collection-finger-shelf.png",390,844,g=>PoseDiscovery(g,.35f));
            yield return PlayableCapture.CaptureSettled(Folder,"collection-finger-mika.png",390,844,g=>PoseDiscovery(g,2f));
            game.SetUiLocale("uk");game.Preferences.LargeText=true;game.SelectShopItem(15);
            yield return PlayableCapture.CaptureSettled(Folder,"collection-small-uk-large.png",320,693);
            game.SelectShopItem(12);
            yield return PlayableCapture.CaptureSettled(Folder,"collection-hat-uk-large.png",320,693);
            game.Preferences.LargeText=false;game.SetUiLocale("ru");game.ShowShop();game.SelectShopTab(1);
            foreach(int index in new[]{12,13,14,15}){
                game.SelectShopItem(index);
                yield return PlayableCapture.CaptureSettled(Folder,"shop-"+ShopCatalog.TrophyIds[index]+".png",390,844);
            }
            game.SelectShopItem(12);
            var preview=game.GetComponentsInChildren<MikaCosmeticView>().First(v=>v.name=="Trophy Mika preview");
            yield return PlayableCapture.CaptureSettled(Folder,"mika-tap-rise.png",390,844,g=>{
                typeof(MikaCosmeticView).GetField("pokeTime",Private).SetValue(preview,.40f);
                typeof(MikaCosmeticView).GetMethod("Apply",Private).Invoke(preview,null);
            });
            yield return new WaitForSecondsRealtime(.55f);
            yield return PlayableCapture.CaptureSettled(Folder,"mika-tap-rest.png",390,844);
            profile.Rewards.EquippedGift="star_knight";game.CreateGlobalCosmeticAmbiance();game.ShowJourney();
            yield return PlayableCapture.CaptureSettled(Folder,"equipped-map.png",390,844);
            profile.Rewards.EquippedGift="fox_mask";profile.Monetization.Cosmetics.Add("fox_mask");game.ShowJourney();
            var conversation=new StoryConversation(-1,new StoryLine("Mika","I am Mika. I am taking a light to Gran at the Great Beacon. Let us start here."));
            game.SkipConversation();
            typeof(CascadeGame).GetMethod("OpenConversation",Private).Invoke(game,new object[]{conversation,(UiCopy)"Back to the map",(Action)(()=>{}),null});
            yield return new WaitForSecondsRealtime(.4f);
            typeof(CascadeGame).GetField("conversationAge",Private).SetValue(game,1000f);
            yield return PlayableCapture.CaptureSettled(Folder,"equipped-dialogue.png",390,844);
            game.SkipConversation();
            profile.Rewards.Parcels.Add(new ParcelReceipt{Order=16,Kind="tool",GiftId="LandingLens",RecipeVersion=1,Claimed=true});
            typeof(CascadeGame).GetField("parcelOrder",Private).SetValue(game,16);
            typeof(CascadeGame).GetField("parcelOpened",Private).SetValue(game,true);
            typeof(CascadeGame).GetMethod("SetScreen",Private).Invoke(game,new object[]{GameScreen.Parcel});
            yield return new WaitForSecondsRealtime(2.5f);
            yield return PlayableCapture.CaptureSettled(Folder,"tool-chest.png",390,844);
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/"+Folder));
            File.WriteAllText(Path.Combine(folder,"capture-complete.txt"),"15 isolated native Unity captures; source art and resulting frames require visual review. No player profile was used.");
            Debug.Log("CASCADE trophy wardrobe capture complete.");
        }
    }
}
