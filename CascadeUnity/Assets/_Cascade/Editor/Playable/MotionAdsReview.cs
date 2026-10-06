using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Solver;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class MotionAdsReview
    {
        static string Folder=>Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/MotionAds");
        public static void Run()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Compiled Play Mode required.");
            Directory.CreateDirectory(Folder);File.WriteAllText(Path.Combine(Folder,"runtime.txt"),"Native MCP Editor review; isolated profile. Local preview ad lifecycle below. Official SDK rewarded callbacks were checked separately.\n");
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Review());
        }
        public static void FinishVisualCorrections(){UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(Corrections());}
        static void Check(bool valid,string description)
        {
            File.AppendAllText(Path.Combine(Folder,"runtime.txt"),(valid?"PASS ":"FAIL ")+description+"\n");
            if(!valid)throw new InvalidOperationException(description);
        }
        static Button Named(CascadeGame g,string name)=>g.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        static IEnumerator Review()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();UnityEngine.Application.runInBackground=true;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");g.Preferences.LargeText=true;g.ShowShop();yield return Wait(.4f);
            Check(g.TestMonetization&&g.PurchaseStore.CanBuy&&g.AdProvider.RewardReady,"Test store and both ad formats ready");
            Check(!g.GetComponentsInChildren<TMP_Text>().Any(t=>t.name=="Total item price"),"Item prices absent");
            yield return Shot("shop-ru-small",320,693);g.Preferences.LargeText=false;
            g.AdProvider.UseLocalPreviewForReview(true);g.ShowShop();yield return Wait(.4f);
            string hash=g.Interaction.Attempt.CurrentStateHash;int bonus=g.Monetization.Ledger.BonusCoins;
            var oldBuy=Named(g,"Buy selected item");g.WatchCoinVideo();
            Check(g.AdProvider.Showing&&g.Interaction.IsSuspended&&!oldBuy.IsInteractable(),"Ad blocks underlying controls and puzzle input");
            yield return Wait(.35f);yield return Shot("video-preview-ru",390,844);
            Named(g,"Close test advertisement").onClick.Invoke();
            Check(!g.AdProvider.Showing&&!g.Interaction.IsSuspended&&g.Monetization.Ledger.BonusCoins==bonus,"Cancelled video gives nothing and releases input");
            g.WatchCoinVideo();yield return Wait(5.2f);Named(g,"Collectcoins").onClick.Invoke();
            Check(g.Monetization.Ledger.BonusCoins==bonus+40&&g.Interaction.Attempt.CurrentStateHash==hash,"Finished video gives exactly 40 coins; row unchanged");
            yield return Shot("coins-received-ru",390,844);yield return Wait(2.1f);
            g.Monetization.AdvanceForeground(180);
            var context=new AdContext{Surface=AdSurface.Win,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true,UniqueClears=3,NodeId="C03",SavedWin=true,BaseRewardGiven=true,FirstSession=true};
            Check(g.AdProvider.TryAutomatic(context),"Interstitial provider exercised with synthetic third-completion context");
            yield return Wait(2.2f);Named(g,"Continue").onClick.Invoke();
            Check(g.Monetization.AutomaticShows==1&&!g.AdProvider.Showing&&g.Monetization.Ledger.BonusCoins==bonus+40,"Interstitial ends without a coin grant or puzzle change");
            g.ShowShop();for(int i=0;i<5;i++)Check(g.BuySelectedShopItem(),"Test tool grant "+(i+1));
            Check(!g.BuySelectedShopItem()&&g.Stock(TravelTool.Wayfinder)==5&&g.Monetization.Ledger.SpentCoins==0,"Test stock cap and zero spend");
            g.SelectShopTab(1);Check(g.BuySelectedShopItem()&&g.BuySelectedShopItem()&&g.EquippedGift==JourneyRewards.Gifts[0],"Test keepsake can be taken along");
            g.SetUiLocale("uk");g.Preferences.LargeText=true;g.ShowNoAds();yield return Wait(.4f);
            Check(!g.GetComponentsInChildren<Button>().Any(b=>b.name=="Restorepurchases")&&g.PurchaseStore.Price==null,"Test no-ads page hides price and real restore");
            yield return Shot("no-ads-uk-small",320,693);g.PurchaseStore.Buy();Check(g.PurchaseStore.Owned&&!g.PurchaseStore.CanBuy,"Test no-ads purchase succeeds once");
            context.NodeId="C06";context.UniqueClears=6;g.Monetization.AdvanceForeground(180);Check(!g.AdProvider.TryAutomatic(context),"Test entitlement blocks automatic ads");
            g.Preferences.LargeText=false;g.SetUiLocale("en");g.ShowHome();g.ShowShop();g.ShowNoAds();g.ShowHome();yield return Wait(.4f);
            var page=g.GetComponentInChildren<MenuTransition>();var rect=(RectTransform)page.transform;var opacity=page.GetComponent<CanvasGroup>();
            Check(Mathf.Approximately(opacity.alpha,1)&&rect.anchoredPosition.sqrMagnitude<.01f,"Rapid navigation settles without accumulated offset");
            var button=Named(g,"Startgame");var body=button.GetComponentInChildren<TMP_Text>().transform;var feel=button.GetComponent<ButtonFeel>();
            feel.OnPointerDown(new PointerEventData(EventSystem.current));yield return Wait(.12f);
            Check(body.localScale.x<.97f&&button.transform.localScale==Vector3.one,"Press compresses artwork; hit rectangle stays fixed");
            feel.OnPointerUp(new PointerEventData(EventSystem.current));yield return Wait(.35f);Check(Mathf.Abs(body.localScale.x-1)<.001f,"Release settles to original size");
            g.ShowShop();yield return Wait(.04f);g.SendMessage("OnApplicationFocus",false);yield return null;float stopped=opacity.alpha;yield return Wait(.2f);
            Check(Mathf.Abs(stopped-opacity.alpha)<.001f,"Screen arrival pauses when unfocused");g.SendMessage("OnApplicationFocus",true);yield return Wait(.4f);
            g.ReducedMotion=true;g.ShowHome();yield return null;Check(opacity.alpha==1&&rect.anchoredPosition.sqrMagnitude<.01f,"Reduced motion settles instantly");g.ReducedMotion=false;
            g.StartDescentLevel(g.Catalog.GetPrimary("C01").Definition);yield return Ready(g);
            int stages=0,commands=0;
            while(g.CurrentScreen!=GameScreen.Win&&commands<30){
                if(g.CurrentScreen==GameScreen.Checkpoint){stages++;g.GoLower();yield return Ready(g);continue;}
                var offer=AttemptService.CalculateHint(g.Interaction.Attempt.CaptureHintRequest());
                Check(offer.Result.Status==HintStatus.Step&&offer.Result.Command.HasValue,"Witness command available");
                Check(g.Interaction.Execute(offer.Result.Command.Value)?.Accepted==true,"Witness command accepted");commands++;yield return Ready(g);
            }
            Check(g.CurrentScreen==GameScreen.Win&&stages==2&&g.LastCompletion.Stages==3,"Actual level reaches statistics only after all three stages");
            var receipt=g.LastCompletion;yield return Wait(.3f);var ceremony=g.GetComponentInChildren<RewardCelebration>();float progress=ceremony.Progress;
            g.SendMessage("OnApplicationPause",true);yield return null;progress=ceremony.Progress;yield return Wait(.2f);Check(Mathf.Abs(ceremony.Progress-progress)<.001f,"Reward ceremony pauses with application");
            g.SendMessage("OnApplicationPause",false);yield return Wait(1.6f);
            Check(g.LastCompletion==receipt&&g.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Earned score").text==receipt.Score.ToString("N0"),"Score count-up finishes without changing receipt");
            yield return Shot("level-complete-en",390,844);g.ContinueCompletion();yield return Wait(.4f);Check(g.CurrentScreen==GameScreen.JourneyMap,"Statistics continue returns to map");
            yield return Introduction(g);
            g.ShowHome();yield return Wait(.4f);Check(g.Interaction.Attempt.CurrentStateHash!=null,"Lifecycle completed");
            File.AppendAllText(Path.Combine(Folder,"runtime.txt"),"COMPLETE. Physical-device ads, store transactions and player builds NOT RUN.\n");Debug.Log("CASCADE motion and ad review complete.");
        }
        static IEnumerator Introduction(CascadeGame g)
        {
            g.ShowHome();g.SetUiLocale("ru");g.Preferences.LargeText=true;
            var fixture=g.Catalog.PrimaryLevels.Concat(g.Catalog.BonusLevels).First(c=>MechanicLessons.Discover(c.Definition,0,new int[0]).Any(m=>!g.SeenMechanics.Contains(m)));
            g.LoadLevel(fixture.Definition);g.ShowGame();double until=Time.realtimeSinceStartupAsDouble+10;
            while(!g.MechanicPopupOpen&&Time.realtimeSinceStartupAsDouble<until)yield return null;
            Check(g.MechanicPopupOpen&&g.Interaction.IsPaused,"First encounter still pauses input after DOTween integration");
            yield return Wait(.4f);yield return Shot("introduction-ru-small",320,693);
        }
        static IEnumerator Corrections()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();UnityEngine.Application.runInBackground=true;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");g.ShowShop();yield return Wait(.4f);
            g.AdProvider.UseLocalPreviewForReview(true);g.WatchCoinVideo();yield return Wait(.4f);yield return Shot("video-preview-ru",390,844);
            Check(Named(g,"Close test advertisement").GetComponentInChildren<TMP_Text>().text=="Закрыть","Ad close button localized");
            Named(g,"Close test advertisement").onClick.Invoke();g.SetUiLocale("uk");g.Preferences.LargeText=true;g.ShowNoAds();yield return Wait(.4f);
            yield return Shot("no-ads-uk-small",320,693);yield return Introduction(g);
            foreach(var text in g.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing&&!text.text.Contains("No translation found"),"Final text fits: "+text.name);}
            g.ShowHome();g.Preferences.LargeText=false;yield return Wait(.4f);
            File.AppendAllText(Path.Combine(Folder,"runtime.txt"),"CORRECTIONS COMPLETE. Earlier final tutorial probe used an arbitrary level and a fixed half-second delay; corrected to a new-mechanic fixture and readiness wait. Earlier lifecycle checks passed.\n");
            Debug.Log("CASCADE motion and ad corrections complete.");
        }
        static IEnumerator Ready(CascadeGame g)
        {
            double until=Time.realtimeSinceStartupAsDouble+30;
            while(Time.realtimeSinceStartupAsDouble<until){
                if(g.MechanicPopupOpen){g.AcknowledgeMechanic();yield return null;continue;}
                if(g.PreparationReady&&!g.IsPlanningDescent&&!g.IsDescending&&!g.Interaction.IsPresenting&&!g.Interaction.IsPreparing){yield return null;if(!g.MechanicPopupOpen)yield break;}
                yield return null;
            }
            Check(false,"Readiness timeout");
        }
        static IEnumerator Wait(float seconds){yield return new WaitForSecondsRealtime(seconds);}
        static IEnumerator Shot(string name,int width,int height)=>PlayableCapture.CaptureSettled("MotionAds",name+".png",width,height);
    }
}
