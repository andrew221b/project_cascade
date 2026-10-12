using Unity.Notifications;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Cascade.Editor.Playable
{
    // Package defaults otherwise ask for iOS permission before the player enables reminders.
    public sealed class LocalReminderProjectSettings : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;
        [InitializeOnLoadMethod]
        static void OnEditorLoaded() => EditorApplication.delayCall += Configure;
        public void OnPreprocessBuild(BuildReport report) => Configure();
        static void Configure()
        {
            if (NotificationSettings.iOSSettings.RequestAuthorizationOnAppLaunch)
                NotificationSettings.iOSSettings.RequestAuthorizationOnAppLaunch = false;
            if (NotificationSettings.iOSSettings.AddRemoteNotificationCapability)
                NotificationSettings.iOSSettings.AddRemoteNotificationCapability = false;
            if (NotificationSettings.iOSSettings.NotificationRequestAuthorizationForRemoteNotificationsOnAppLaunch)
                NotificationSettings.iOSSettings.NotificationRequestAuthorizationForRemoteNotificationsOnAppLaunch = false;
            if ((int)NotificationSettings.AndroidSettings.ExactSchedulingOption != 0)
                NotificationSettings.AndroidSettings.ExactSchedulingOption = (AndroidExactSchedulingOption)0;
            if (!NotificationSettings.AndroidSettings.RescheduleOnDeviceRestart)
                NotificationSettings.AndroidSettings.RescheduleOnDeviceRestart = true;
        }
    }
}
