using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        LocalReminderService localReminders;
        bool reminderPermissionPending, reminderPermissionDenied;
        public bool LocalRemindersAvailable => LocalReminderService.IsSupported;

        void InitializeLocalReminders()
        {
            localReminders = new LocalReminderService();
            localReminders.Initialize(new UiCopy("Local reminders").ToString());
        }
        void SetLocalReminderBackground(bool background)
        {
            localReminders?.SetBackground(background, Preferences.NotificationsEnabled,
                new UiCopy("Good morning").ToString(), ReminderMorningBody,
                new UiCopy("Good evening").ToString(), ReminderEveningBody);
        }
        string ReminderMorningBody(System.DateTime scheduledFor)
        {
            int variation = (scheduledFor.DayOfYear - 1) % 3;
            UiCopy body = variation == 0 ? "Good morning! Mika left a lantern glowing for your next little adventure."
                : variation == 1 ? "Good morning! May your day begin softly and surprise you in a lovely way."
                : "Good morning! A fresh path is waiting whenever you have a quiet moment.";
            return body.ToString();
        }
        string ReminderEveningBody(System.DateTime scheduledFor)
        {
            int variation = (scheduledFor.DayOfYear - 1) % 3;
            int clearedPaths = profile?.AllClears?.Select(receipt => receipt.CampaignNodeId).Distinct().Count() ?? 0;
            UiCopy body = variation == 0 ? "Good evening! Let the day rest; Mika will keep your place on the map."
                : variation == 1 ? "Good evening! A quiet little puzzle is here whenever you feel like unwinding."
                : clearedPaths > 0 ? new UiCopy("You’ve lit {0} paths so far. Mika saved a little light for the next.", clearedPaths)
                : "Good evening! Wishing you a peaceful night and a lighter tomorrow.";
            return body.ToString();
        }
        public void SetNotificationsEnabled(bool enabled)
        {
            if (reminderPermissionPending) return;
            if (!enabled || !LocalRemindersAvailable) {
                Preferences.NotificationsEnabled = false;
                reminderPermissionDenied = false;
                localReminders?.Cancel();
                ApplyPreferences();
                return;
            }
            if (localReminders == null) InitializeLocalReminders();
            Preferences.NotificationPermissionRequested = true;
            PersistProgress(true);
            if (HasSaveError) return;
            reminderPermissionPending = true;
            StartCoroutine(EnableLocalReminders());
        }
        IEnumerator EnableLocalReminders()
        {
            yield return localReminders.RequestPermission(granted => {
                reminderPermissionPending = false;
                reminderPermissionDenied = !granted;
                Preferences.NotificationsEnabled = granted;
                if (!granted) localReminders.Cancel();
                ApplyPreferences();
                if (CurrentScreen == GameScreen.Settings && settingsPage == 3) DrawMenu();
            });
        }
        void DrawNotificationSettings()
        {
            if (!LocalRemindersAvailable) {
                SettingsNote("Reminder detail", "Available on your phone");
                return;
            }
            bool active = Preferences.NotificationsEnabled && localReminders != null && localReminders.PermissionGranted;
            var button=Setting("Local reminders", active, 0, () => SetNotificationsEnabled(!active));
            button.name = "Local reminders";
            button.interactable = LocalRemindersAvailable && !reminderPermissionPending;
            UiCopy note = !LocalRemindersAvailable ? "Available on your phone"
                : reminderPermissionPending ? "Waiting for device permission..."
                : reminderPermissionDenied || Preferences.NotificationPermissionRequested && Preferences.NotificationsEnabled && !active ? "Allow reminders in device settings."
                : "Daily reminders at 9:00 and 19:00.";
            SettingsNote("Reminder detail",note);
        }
    }
}
