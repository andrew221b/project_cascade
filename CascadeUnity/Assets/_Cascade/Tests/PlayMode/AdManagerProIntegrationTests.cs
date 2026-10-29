using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using AdManagerPro;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed class AdManagerProIntegrationTests
    {
        [Test] public void PluginUsesAllSixFlyingLaddersIdsAndLiveMode()
        {
            var p=Resources.Load<MobileAdsSettings>("MobileAdsSettings");Assert.That(p,Is.Not.Null);Assert.That(p.testMode,Is.False);
            Assert.That(p.androidAppID,Is.EqualTo("ca-app-pub-2724223438862289~3999988514"));
            Assert.That(p.androidInterstitialID,Is.EqualTo("ca-app-pub-2724223438862289/8493942302"));
            Assert.That(p.androidRewardedVideoID,Is.EqualTo("ca-app-pub-2724223438862289/8065944779"));
            Assert.That(p.iosAppID,Is.EqualTo("ca-app-pub-2724223438862289~7300939270"));
            Assert.That(p.iosInterstitialID,Is.EqualTo("ca-app-pub-2724223438862289/3241615625"));
            Assert.That(p.iosRewardedVideoID,Is.EqualTo("ca-app-pub-2724223438862289/1665469215"));
            var sdk=UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            const BindingFlags fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            Assert.That(sdk.GetType().GetField("adMobIOSAppId",fields).GetValue(sdk),Is.EqualTo(p.iosAppID));
            Assert.That(sdk.GetType().GetField("adMobAndroidAppId",fields).GetValue(sdk),Is.EqualTo(p.androidAppID));
            var m=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            Assert.That(p.androidRewardedVideoID,Is.EqualTo(m.androidRewardedUnitId));Assert.That(p.iosRewardedVideoID,Is.EqualTo(m.iosRewardedUnitId));
#if UNITY_IOS
            Assert.That(p.GetAppID(),Is.EqualTo(p.iosAppID));Assert.That(p.GetRewardedVideoID(),Is.EqualTo(p.iosRewardedVideoID));Assert.That(p.GetInterstitialID(),Is.EqualTo(p.iosInterstitialID));
#else
            Assert.That(p.GetAppID(),Is.EqualTo(p.androidAppID));Assert.That(p.GetRewardedVideoID(),Is.EqualTo(p.androidRewardedVideoID));Assert.That(p.GetInterstitialID(),Is.EqualTo(p.androidInterstitialID));
#endif
        }
        [UnityTest] public IEnumerator ResetDuringPendingLoadCannotPublishOldReadiness()
        {
            RewardedAdManager.Reset();RewardedAdManager.RequestRewardedAd();RewardedAdManager.Reset();
            for(int frame=0;frame<5;frame++)yield return null;
            Assert.That(RewardedAdManager.IsReady,Is.False,"Canceled load must not republish its old ad");
            Assert.That(RewardedAdManager.IsLoading,Is.False);
        }
        [UnityTest] public IEnumerator PluginLoadsAndShowsSdkEditorRewardAndStopsReloadAfterOwnerReset()
        {
            RewardedAdManager.Reset();int opened=0,earned=0,closed=0;bool watched=false;
            try{
                RewardedAdManager.RequestRewardedAd();RewardedAdManager.RequestRewardedAd();
                float deadline=Time.realtimeSinceStartup+15;while(!RewardedAdManager.IsReady&&Time.realtimeSinceStartup<deadline)yield return null;
                Assert.That(RewardedAdManager.IsReady,Is.True,"The actual Google SDK Editor transport must deliver an ad through AdManagerPro");
                var show=typeof(RewardedAdManager).GetMethods().Single(m=>m.Name=="ShowRewardedAd"&&m.GetParameters().Length==4);
                bool shown=(bool)show.Invoke(null,new object[]{(Action<bool>)(value=>{closed++;watched=value;RewardedAdManager.Reset();}),
                    (Action)(()=>opened++),(Action)(()=>earned++),null});Assert.That(shown,Is.True);
                deadline=Time.realtimeSinceStartup+15;while(earned==0&&Time.realtimeSinceStartup<deadline)yield return null;
                Assert.That(opened,Is.EqualTo(1));Assert.That(earned,Is.EqualTo(1));
                var candidates=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
                    .Where(b=>b.GetComponentsInChildren<Text>().Any(t=>t.text=="Close Ad"&&t.GetComponentInParent<Button>()==b)).ToArray();
                string PathOf(Transform t)=>t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;
                Assert.That(candidates.Length,Is.EqualTo(1),string.Join("; ",candidates.Select(b=>PathOf(b.transform)+" scene="+b.gameObject.scene.name)));
                candidates[0].onClick.Invoke();yield return null;yield return null;
                Assert.That(closed,Is.EqualTo(1));Assert.That(watched,Is.True);Assert.That(earned,Is.EqualTo(1));
                Assert.That(RewardedAdManager.IsLoading,Is.False,"A disposed owner must not start the plugin's next automatic load");
                Assert.That(RewardedAdManager.IsReady,Is.False);
            }finally{RewardedAdManager.Reset();InterstitialAdManager.Reset();Time.timeScale=1;}
        }
    }
}
