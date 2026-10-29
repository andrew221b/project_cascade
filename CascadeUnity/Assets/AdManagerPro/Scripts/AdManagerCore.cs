#if ADS || NATIVE_ADS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

namespace AdManagerPro
{
    public static class AdManagerCore
    {
        public static MobileAdsSettings Settings { get; private set; }
        public static bool IsInitialized { get; private set; }
        public static event Action<string,string,int,string> AdFailed;
        static bool initStarted;
        static readonly List<Action> pendingCallbacks=new List<Action>();
        static readonly List<Action> pendingFailures=new List<Action>();
        static AdManagerCore(){Settings=Resources.Load<MobileAdsSettings>("MobileAdsSettings");}
        public static void EnsureInitialized(Action onReady)=>EnsureInitialized(onReady,null);
        public static void EnsureInitialized(Action onReady,Action onFailed)
        {
            if(Settings==null){ReportFailure("initialize","configuration",-1,"MobileAdsSettings not found");onFailed?.Invoke();return;}
            if(IsInitialized){onReady?.Invoke();return;}
            if(onReady!=null)pendingCallbacks.Add(onReady);
            if(onFailed!=null)pendingFailures.Add(onFailed);
            if(initStarted)return;initStarted=true;
            MobileAds.RaiseAdEventsOnUnityMainThread=true;
            try{MobileAds.Initialize(status=>{
                initStarted=false;IsInitialized=status!=null;
                var callbacks=(IsInitialized?pendingCallbacks:pendingFailures).ToArray();
                pendingCallbacks.Clear();pendingFailures.Clear();
                if(!IsInitialized)ReportFailure("initialize","gma",-1,"No initialization result");
                foreach(var action in callbacks)action();
            });}catch(Exception e){
                initStarted=false;var callbacks=pendingFailures.ToArray();pendingCallbacks.Clear();pendingFailures.Clear();
                ReportFailure("initialize","gma",-1,e.Message);foreach(var action in callbacks)action();
            }
        }
        public static void ReportFailure(string stage,string domain,int code,string message)
        {
            if(AdFailed!=null)AdFailed(stage,domain,code,message);
            else Debug.LogWarning("[AdManagerPro] "+stage+" ["+domain+":"+code+"] "+message);
        }
        public static void RetryAfterBackoff(int attempt,Action retry)
        {AdManagerRunner.Instance.DelayedCall(Mathf.Pow(2f,Mathf.Clamp(attempt,1,6)),retry);}
    }
    internal class AdManagerRunner:MonoBehaviour
    {
        static AdManagerRunner instance;
        public static AdManagerRunner Instance{
            get{if(instance==null){var go=new GameObject("AdManagerPro Runner"){hideFlags=HideFlags.HideAndDontSave};DontDestroyOnLoad(go);instance=go.AddComponent<AdManagerRunner>();}return instance;}
        }
        public void DelayedCall(float seconds,Action action){StartCoroutine(Delay(seconds,action));}
        IEnumerator Delay(float seconds,Action action){yield return new WaitForSecondsRealtime(seconds);action?.Invoke();}
    }
}
#endif
