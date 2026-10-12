using System;

namespace Cascade.Application
{
    public static class LocalReminderPlanner
    {
        public static DateTime? ForMorning(DateTime localNow, bool enabled, bool permissionGranted, int daysAhead = 1)
        {
            return AtLocalHour(localNow, enabled, permissionGranted, daysAhead, 9);
        }

        public static DateTime? ForBackground(DateTime localNow, bool enabled, bool permissionGranted, int daysAhead = 1)
        {
            return AtLocalHour(localNow, enabled, permissionGranted, daysAhead, 19);
        }

        static DateTime? AtLocalHour(DateTime localNow, bool enabled, bool permissionGranted, int daysAhead, int hour)
        {
            if (!enabled || !permissionGranted) return null;
            if (localNow.Kind == DateTimeKind.Utc) throw new ArgumentException("A reminder needs local calendar time.", nameof(localNow));
            if (daysAhead < 1) throw new ArgumentOutOfRangeException(nameof(daysAhead), "Local reminders must be scheduled for a future day.");
            return localNow.Date.AddDays(daysAhead).AddHours(hour);
        }
    }
}
