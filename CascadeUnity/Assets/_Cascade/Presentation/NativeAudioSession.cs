using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Cascade.Presentation
{
    // System media volume and the game's music/sound preferences own loudness.
    // Configure on foreground/output changes; never poll native audio every frame.
    public static class NativeAudioSession
    {
        static bool warned;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>warned=false;

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern int CascadeConfigurePlaybackAudio();
#endif

        public static void ConfigurePlayback()
        {
#if UNITY_IOS && !UNITY_EDITOR
            try {
                if(CascadeConfigurePlaybackAudio()==0)Warn("iOS rejected the playback category");
            }
            catch(System.DllNotFoundException error){Warn(error.GetType().Name);}
            catch(System.EntryPointNotFoundException error){Warn(error.GetType().Name);}
#elif UNITY_ANDROID && !UNITY_EDITOR
            try {
                using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
                    activity?.Call("runOnUiThread",new AndroidJavaRunnable(ConfigureAndroidMediaControls));
            }
            catch(AndroidJavaException error){Warn(error.GetType().Name);}
#endif
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        static void ConfigureAndroidMediaControls()
        {
            // Resolve the activity on the UI thread, after dispatch, so no disposed
            // Java wrapper is captured and no obsolete activity is retained.
            try {
                using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
                using(var audio=new AndroidJavaClass("android.media.AudioManager"))
                    activity?.Call("setVolumeControlStream",audio.GetStatic<int>("STREAM_MUSIC"));
            }
            catch(AndroidJavaException error){Warn(error.GetType().Name);}
        }
#endif
        static void Warn(string reason)
        {
            if(warned)return;warned=true;
            Debug.LogWarning("CASCADE media audio configuration failed: "+reason);
        }
    }
}
