using System;
using System.Collections;
using System.Collections.Generic;
using Cascade.Application;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace Cascade.Presentation
{
    // The SDK supplies ad events; the existing policy/ledger owns eligibility and grants.
    public sealed class NativeAdProvider:MonoBehaviour
    {
        CascadeGame game;AdService policy;MonetizationSettings settings;
        LevelPlayRewardedAd rewarded;LevelPlayInterstitialAd interstitial;
        readonly Dictionary<string,AdTicket> displayedRewards=new Dictionary<string,AdTicket>();
        AdTicket pending;float deadline;bool privacyReady,initialized,dead;
        public bool RewardReady=>initialized&&rewarded!=null&&rewarded.IsAdReady();
        public bool AutomaticReady=>initialized&&interstitial!=null&&interstitial.IsAdReady();
        public bool PrivacyReady=>privacyReady&&initialized;
        public void Initialize(CascadeGame owner,AdService ledger,MonetizationSettings configuration){game=owner;policy=ledger;settings=configuration;}
        // Called by the publisher's age/consent bridge AFTER CMP/ATT flow, never with invented consent.
        public void InitializeAfterPrivacy(bool consent,bool optOut,bool isChild)
        {
            if(settings==null||!settings.adsReleaseReady||initialized||privacyReady)return;
            string key=UnityEngine.Application.platform==RuntimePlatform.IPhonePlayer?settings.iosAppKey:settings.androidAppKey;
            if(string.IsNullOrWhiteSpace(key)||string.IsNullOrWhiteSpace(settings.rewardedUnitId)||string.IsNullOrWhiteSpace(settings.interstitialUnitId))return;
            privacyReady=true;
            LevelPlayPrivacySettings.SetGDPRConsent(consent);LevelPlayPrivacySettings.SetCCPA(optOut);LevelPlayPrivacySettings.SetCOPPA(isChild);
            LevelPlay.OnInitSuccess-=Initialized;LevelPlay.OnInitFailed-=InitFailed;
            LevelPlay.OnInitSuccess+=Initialized;LevelPlay.OnInitFailed+=InitFailed;LevelPlay.Init(key);
        }
        void Initialized(LevelPlayConfiguration configuration)
        {
            if(dead)return;initialized=true;
            rewarded=new LevelPlayRewardedAd(settings.rewardedUnitId);interstitial=new LevelPlayInterstitialAd(settings.interstitialUnitId);
            rewarded.OnAdDisplayed+=RewardDisplayed;rewarded.OnAdDisplayFailed+=RewardFailed;rewarded.OnAdRewarded+=Reward;
            rewarded.OnAdClosed+=RewardClosed;rewarded.OnAdLoadFailed+=LoadFailed;rewarded.OnAdImpressionDataReady+=Impression;
            interstitial.OnAdDisplayed+=AutomaticDisplayed;interstitial.OnAdDisplayFailed+=AutomaticFailed;
            interstitial.OnAdClosed+=AutomaticClosed;interstitial.OnAdLoadFailed+=LoadFailed;interstitial.OnAdImpressionDataReady+=Impression;
            rewarded.LoadAd();interstitial.LoadAd();
        }
        void InitFailed(LevelPlayInitError error){privacyReady=false;initialized=false;}
        void Impression(LevelPlayImpressionData data){/* Publisher analytics may subscribe here; no receipt or user data is logged. */}
        void LoadFailed(LevelPlayAdError error){/* Not ready means skip, never hold gameplay or queue a debt. */}
        public bool TryAutomatic(AdContext context)
        {
            var ticket=policy.TryAutomatic(context,AutomaticReady);if(ticket==null)return false;
            Show(ticket,false);return true;
        }
        public void OfferCoins(AdContext context)
        {
            var ticket=policy.TryRewarded(context,"coins",RewardReady);if(ticket!=null)Show(ticket,true);
        }
        void Show(AdTicket ticket,bool video)
        {
            pending=ticket;deadline=Time.realtimeSinceStartup+12;game.SetNativeAdSuspended(true);
            try{if(video)rewarded.ShowAd();else interstitial.ShowAd();}catch(Exception){Fail();}
        }
        void RewardDisplayed(LevelPlayAdInfo info)
        {
            if(pending==null||!pending.Rewarded)return;policy.Opened(pending.Id);deadline=0;
            string key=Key(info);if(key!=null)displayedRewards[key]=pending;
        }
        static string Key(LevelPlayAdInfo info)=>!string.IsNullOrEmpty(info.AuctionId)?info.AuctionId:info.AdId;
        void Reward(LevelPlayAdInfo info,LevelPlayReward reward)
        {
            string key=Key(info);if(key==null||!displayedRewards.TryGetValue(key,out var ticket))return;
            if(policy.RewardConfirmed(ticket.Id,"ad_"+ticket.Id,true)){displayedRewards.Remove(key);game.NotifyStoreChanged();}
        }
        void AutomaticDisplayed(LevelPlayAdInfo info){if(pending!=null){policy.Opened(pending.Id);deadline=0;}}
        void RewardClosed(LevelPlayAdInfo info)
        {
            string key=Key(info);if(key!=null&&displayedRewards.TryGetValue(key,out var ticket))StartCoroutine(ExpireReward(key,ticket));
            Close();rewarded.LoadAd();
        }
        IEnumerator ExpireReward(string key,AdTicket ticket){yield return new WaitForSecondsRealtime(30);if(displayedRewards.Remove(key))policy.Failed(ticket.Id);}
        void AutomaticClosed(LevelPlayAdInfo info){Close();interstitial.LoadAd();}
        void RewardFailed(LevelPlayAdInfo info,LevelPlayAdError error)=>Fail();
        void AutomaticFailed(LevelPlayAdInfo info,LevelPlayAdError error)=>Fail();
        void Close(){if(pending!=null)policy.Closed(pending.Id);pending=null;deadline=0;game.SetNativeAdSuspended(false);}
        void Fail(){if(pending!=null)policy.Failed(pending.Id);Close();}
        void Update(){if(pending!=null&&deadline>0&&Time.realtimeSinceStartup>=deadline)Fail();}
        void OnDestroy()
        {
            dead=true;StopAllCoroutines();if(policy!=null&&pending!=null)policy.Failed(pending.Id);
            LevelPlay.OnInitSuccess-=Initialized;LevelPlay.OnInitFailed-=InitFailed;
            if(rewarded!=null){rewarded.OnAdDisplayed-=RewardDisplayed;rewarded.OnAdDisplayFailed-=RewardFailed;rewarded.OnAdRewarded-=Reward;rewarded.OnAdClosed-=RewardClosed;rewarded.OnAdLoadFailed-=LoadFailed;rewarded.OnAdImpressionDataReady-=Impression;rewarded.DestroyAd();}
            if(interstitial!=null){interstitial.OnAdDisplayed-=AutomaticDisplayed;interstitial.OnAdDisplayFailed-=AutomaticFailed;interstitial.OnAdClosed-=AutomaticClosed;interstitial.OnAdLoadFailed-=LoadFailed;interstitial.OnAdImpressionDataReady-=Impression;interstitial.DestroyAd();}
        }
    }
}
