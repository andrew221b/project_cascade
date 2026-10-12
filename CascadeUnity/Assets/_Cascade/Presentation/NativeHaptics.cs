using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Cascade.Presentation
{
    public static class NativeHaptics
    {
        static bool nativeUnavailable;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { nativeUnavailable = false; }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void CascadePulseHaptic(float strength);
#endif

        // Preferences, focus and the feedback cooldown belong to ToyFeedbackPool.
        public static void Pulse(float strength)
        {
            if (nativeUnavailable || float.IsNaN(strength) || strength <= 0) return;
            strength = Mathf.Clamp01(strength);
#if UNITY_IOS && !UNITY_EDITOR
            try { CascadePulseHaptic(strength); }
            catch (System.DllNotFoundException error) { DisableAfterFailure(error); }
            catch (System.EntryPointNotFoundException error) { DisableAfterFailure(error); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    if (activity == null) return;
                    using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                    using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        if (vibrator == null || !vibrator.Call<bool>("hasVibrator")) return;
                        if (version.GetStatic<int>("SDK_INT") >= 26)
                        {
                            bool adjustable = vibrator.Call<bool>("hasAmplitudeControl");
                            long duration = adjustable ? Mathf.RoundToInt(Mathf.Lerp(12, 38, strength)) : 12;
                            int amplitude = adjustable ? Mathf.RoundToInt(Mathf.Lerp(28, 220, strength)) : -1;
                            using (var effects = new AndroidJavaClass("android.os.VibrationEffect"))
                            using (var effect = effects.CallStatic<AndroidJavaObject>("createOneShot", duration, amplitude))
                                vibrator.Call("vibrate", effect);
                        }
                        else
                        {
                            // Old motors have no amplitude control; limit the full-strength fallback.
                            vibrator.Call("vibrate", 10L);
                        }
                    }
                }
            }
            catch (AndroidJavaException error) { DisableAfterFailure(error); }
#endif
        }

        static void DisableAfterFailure(System.Exception error)
        {
            nativeUnavailable = true;
            string reason = error.Message;
            int newline = reason.IndexOf('\n');
            if (newline >= 0) reason = reason.Substring(0, newline);
            Debug.LogWarning("CASCADE native haptics disabled for this session: " + reason);
        }
    }
}
