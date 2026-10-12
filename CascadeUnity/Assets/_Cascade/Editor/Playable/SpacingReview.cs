using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Content;
using Cascade.Presentation;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class SpacingReview
    {
        public static void Capture(){var g=Object.FindFirstObjectByType<CascadeGame>();if(g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new System.InvalidOperationException("Review profile required");g.StartCoroutine(Run(g));}
        static IEnumerator Shot(string name,int w=390,int h=844){yield return new WaitForSecondsRealtime(.18f);yield return PlayableCapture.CaptureSettled("Spacing-Oct08",name+".png",w,h);}
        static IEnumerator Run(CascadeGame g){
            int fps=UnityEngine.Application.targetFrameRate;UnityEngine.Application.targetFrameRate=30;UnityEngine.Application.runInBackground=true;
            try{
                g.SendMessage("OnApplicationFocus",true);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.SetUiLocale("ru");g.ReducedMotion=true;
                g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
                g.ShowHome();yield return Shot("home");g.ShowSettings();yield return Shot("settings");
                g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(12);yield return Shot("shop");
                g.Preferences.LargeText=true;g.ShowShop();g.SelectShopTab(1);g.SelectShopItem(19);yield return Shot("shop-small",320,693);g.Preferences.LargeText=false;
                g.ShowCollection();yield return Shot("collection");g.ShowCoinPacks();yield return Shot("coins");
                g.ShowJourney();if(g.ComicWorld>=0)g.FinishWorldComic();if(g.ConversationOpen)g.SkipConversation();
                g.TurnJourneyMap(-1);if(g.ComicWorld>=0)g.FinishWorldComic();if(g.ConversationOpen)g.SkipConversation();yield return Shot("map-two-arrows");
                g.ReviewConversation(StoryConversations.IslandProgress(JourneyStory.IslandFirst(g.MapWorld,0)+5));yield return Shot("dialogue");
                g.SkipConversation();g.OpenJourneyIsland(0);yield return Shot("island-list");
                g.OpenWorldComic(0);yield return Shot("comic");g.FinishWorldComic();if(g.ConversationOpen)g.SkipConversation();
                g.ShowTravelBag();yield return Shot("bag");g.ShowSupplies();yield return Shot("tools");g.ShowHowToPlay();yield return Shot("guide");
                g.SetUiLocale("en");g.ShowShop();g.SelectShopTab(1);yield return Shot("font-en");
                File.WriteAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/Spacing-Oct08/complete.txt")),"13 native Editor screen captures. Isolated profile; no phone-build claim.");
            }finally{UnityEngine.Application.targetFrameRate=fps;UnityEngine.Application.runInBackground=false;}
        }
    }
}
