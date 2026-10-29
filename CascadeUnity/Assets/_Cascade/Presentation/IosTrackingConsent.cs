using System;
using System.Runtime.InteropServices;
namespace Cascade.Presentation
{
    // One native prompt at a time; the SDK still reads the actual OS permission
    // on every request, including when the player later changes it in Settings.
    public static class IosTrackingConsent
    {
#if UNITY_IOS && !UNITY_EDITOR
        delegate void Completion(int status);
        static Action finished;
        static readonly Completion callback=Completed;
        [DllImport("__Internal")] static extern void CascadeRequestTrackingAuthorization(Completion completion);
        [AOT.MonoPInvokeCallback(typeof(Completion))]
        static void Completed(int status){var action=finished;finished=null;action?.Invoke();}
#endif
        public static void Request(Action completion)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if(finished!=null){finished+=completion;return;}
            finished=completion;CascadeRequestTrackingAuthorization(callback);
#else
            completion?.Invoke();
#endif
        }
    }
}
