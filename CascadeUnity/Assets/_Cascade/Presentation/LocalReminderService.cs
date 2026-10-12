using System;
using System.Collections;
using System.Globalization;
using Cascade.Application;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#elif UNITY_IOS && !UNITY_EDITOR
using Unity.Notifications.iOS;
#endif

namespace Cascade.Presentation
{
    // Owns one local OS notification. No remote registration, gameplay or reward callbacks.
    public sealed class LocalReminderService
    {
        const string ChannelId = "cascade_local_reminders";
        const string MorningReminderIdPrefix = "cascade.local.reminder.morning.";
        const string EveningReminderIdPrefix = "cascade.local.reminder.evening.";
        const int AndroidReminderIdBase = 170200;
        const int ScheduledDays = 14;
        bool background;

        public static bool IsSupported
        {
            get {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
        public bool PermissionGranted
        {
            get {
                try {
#if UNITY_ANDROID && !UNITY_EDITOR
                    return AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.Allowed;
#elif UNITY_IOS && !UNITY_EDITOR
                    var status = iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus;
                    return status == AuthorizationStatus.Authorized || status == AuthorizationStatus.Provisional;
#else
                    return false;
#endif
                } catch (Exception) { return false; }
            }
        }

        public void Initialize(string channelName)
        {
            try {
#if UNITY_ANDROID && !UNITY_EDITOR
                AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel {
                    Id = ChannelId, Name = channelName, Description = channelName,
                    Importance = Importance.Default
                });
#endif
                Cancel();
            } catch (Exception) { Debug.LogWarning("CASCADE local reminders unavailable."); }
        }

        public IEnumerator RequestPermission(Action<bool> completed)
        {
            if (PermissionGranted) { completed(true); yield break; }
#if UNITY_ANDROID && !UNITY_EDITOR
            PermissionRequest request;
            try { request = new PermissionRequest(); }
            catch (Exception) { completed(false); yield break; }
            while (request.Status == PermissionStatus.RequestPending) yield return null;
            completed(request.Status == PermissionStatus.Allowed);
#elif UNITY_IOS && !UNITY_EDITOR
            AuthorizationRequest request;
            try { request = new AuthorizationRequest(AuthorizationOption.Alert, false); }
            catch (Exception) { completed(false); yield break; }
            try {
                while (!request.IsFinished) yield return null;
                completed(request.Granted);
            } finally { request.Dispose(); }
#else
            completed(false);
            yield break;
#endif
        }

        public void SetBackground(bool value, bool enabled, string morningTitle, Func<DateTime, string> morningBody, string eveningTitle, Func<DateTime, string> eveningBody)
        {
            bool wasBackground = background;
            background = value;
            if (!value || !enabled) { Cancel(); return; }
            if (wasBackground) return;
            var now = DateTime.Now;
            if (!LocalReminderPlanner.ForMorning(now, enabled, PermissionGranted).HasValue) { Cancel(); return; }
            try {
                Cancel();
#if UNITY_ANDROID && !UNITY_EDITOR
                for (int day = 1; day <= ScheduledDays; day++) {
                    var morning = LocalReminderPlanner.ForMorning(now, enabled, true, day).Value;
                    var evening = LocalReminderPlanner.ForBackground(now, enabled, true, day).Value;
                    var morningNotification = new AndroidNotification { Title = morningTitle, Text = morningBody(morning), FireTime = morning, ShouldAutoCancel = true };
                    AndroidNotificationCenter.SendNotificationWithExplicitID(morningNotification, ChannelId, AndroidId(day, false));
                    var eveningNotification = new AndroidNotification { Title = eveningTitle, Text = eveningBody(evening), FireTime = evening, ShouldAutoCancel = true };
                    AndroidNotificationCenter.SendNotificationWithExplicitID(eveningNotification, ChannelId, AndroidId(day, true));
                }
#elif UNITY_IOS && !UNITY_EDITOR
                for (int day = 1; day <= ScheduledDays; day++) {
                    var morning = LocalReminderPlanner.ForMorning(now, enabled, true, day).Value;
                    var evening = LocalReminderPlanner.ForBackground(now, enabled, true, day).Value;
                    ScheduleIos(IosId(day, false), morningTitle, morningBody(morning), morning);
                    ScheduleIos(IosId(day, true), eveningTitle, eveningBody(evening), evening);
                }
#endif
            } catch (Exception) { Debug.LogWarning("CASCADE could not schedule a local reminder."); }
        }

#if UNITY_IOS && !UNITY_EDITOR
        static void ScheduleIos(string identifier, string title, string body, DateTime time)
        {
            iOSNotificationCenter.ScheduleNotification(new iOSNotification {
                Identifier = identifier, Title = title, Body = body, ShowInForeground = false,
                Trigger = new iOSNotificationCalendarTrigger {
                    Year = time.Year, Month = time.Month, Day = time.Day,
                    Hour = time.Hour, Minute = time.Minute, Second = 0, Repeats = false
                }
            });
        }
#endif

        static int AndroidId(int dayAhead, bool evening) => AndroidReminderIdBase + (dayAhead - 1) * 2 + (evening ? 1 : 0);
        static string IosId(int dayAhead, bool evening) => (evening ? EveningReminderIdPrefix : MorningReminderIdPrefix)
            + (dayAhead - 1).ToString("D2", CultureInfo.InvariantCulture);

        public void Cancel()
        {
            try {
#if UNITY_ANDROID && !UNITY_EDITOR
                // Clear identifiers from the previous one-day implementation as well as the rolling window.
                AndroidNotificationCenter.CancelNotification(170101);
                AndroidNotificationCenter.CancelNotification(170102);
                AndroidNotificationCenter.CancelDisplayedNotification(170101);
                AndroidNotificationCenter.CancelDisplayedNotification(170102);
                for (int day = 1; day <= ScheduledDays; day++) {
                    int morning = AndroidId(day, false), evening = AndroidId(day, true);
                    AndroidNotificationCenter.CancelNotification(morning);
                    AndroidNotificationCenter.CancelNotification(evening);
                    AndroidNotificationCenter.CancelDisplayedNotification(morning);
                    AndroidNotificationCenter.CancelDisplayedNotification(evening);
                }
#elif UNITY_IOS && !UNITY_EDITOR
                iOSNotificationCenter.RemoveScheduledNotification("cascade.local.reminder.morning");
                iOSNotificationCenter.RemoveDeliveredNotification("cascade.local.reminder.morning");
                iOSNotificationCenter.RemoveScheduledNotification("cascade.local.reminder");
                iOSNotificationCenter.RemoveDeliveredNotification("cascade.local.reminder");
                for (int day = 1; day <= ScheduledDays; day++) {
                    string morning = IosId(day, false), evening = IosId(day, true);
                    iOSNotificationCenter.RemoveScheduledNotification(morning);
                    iOSNotificationCenter.RemoveDeliveredNotification(morning);
                    iOSNotificationCenter.RemoveScheduledNotification(evening);
                    iOSNotificationCenter.RemoveDeliveredNotification(evening);
                }
#endif
            } catch (Exception) { Debug.LogWarning("CASCADE could not cancel a local reminder."); }
        }
    }
}
