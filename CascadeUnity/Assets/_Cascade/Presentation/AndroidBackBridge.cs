using UnityEngine;
namespace Cascade.Presentation
{
    // UI-thread callbacks only enqueue a bit. Never call Unity or wait for its
    // render thread from Android's Back dispatcher.
    internal static class AndroidBackBridge
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        static AndroidJavaClass bridge;
        static bool failed;
        public static void Register()
        {
            if(failed)return;
            try{
                if(bridge==null)bridge=new AndroidJavaClass("com.vickand.flyingladders.platform.CascadeAndroidBack");
                using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=unity.GetStatic<AndroidJavaObject>("currentActivity"))bridge.CallStatic("install",activity);
            }catch(AndroidJavaException e){failed=true;Debug.LogWarning("Android Back bridge unavailable: "+e.GetType().Name);}
        }
        public static bool Consume()
        {
            if(bridge==null||failed)return false;
            try{return bridge.CallStatic<bool>("consume");}
            catch(AndroidJavaException){failed=true;return false;}
        }
        public static void Clear(){Consume();}
        public static void Background()
        {
            try{
                using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=unity.GetStatic<AndroidJavaObject>("currentActivity")){
                    if(activity==null)return;
                    if(bridge!=null&&!failed)bridge.CallStatic("background",activity);
                    else activity.Call("runOnUiThread",new AndroidJavaRunnable(BackgroundOnUiThread));
                }
            }catch(AndroidJavaException){failed=true;}
        }
        static void BackgroundOnUiThread()
        {
            // Resolve on dispatch, never capture a disposed or obsolete JNI handle.
            try{
                using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=unity.GetStatic<AndroidJavaObject>("currentActivity"))
                    if(activity!=null&&!activity.Call<bool>("isDestroyed")&&!activity.Call<bool>("isFinishing"))activity.Call<bool>("moveTaskToBack",true);
            }catch(AndroidJavaException){}
        }
        public static void Shutdown()
        {
            if(bridge!=null){
                try{if(!failed)bridge.CallStatic("uninstall");}catch(AndroidJavaException){}
                finally{bridge.Dispose();bridge=null;}
            }
            failed=false;
        }
#else
        public static void Register(){}
        public static bool Consume()=>false;
        public static void Clear(){}
        public static void Background(){}
        public static void Shutdown(){}
#endif
    }
}
