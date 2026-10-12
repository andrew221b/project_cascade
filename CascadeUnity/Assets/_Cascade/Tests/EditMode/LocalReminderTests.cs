using System;
using Cascade.Application;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class LocalReminderTests
    {
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void NoReminderWithoutPreferenceAndOsConsent(bool enabled, bool consent)
        {
            var local = new DateTime(2026, 10, 7, 12, 0, 0);
            Assert.That(LocalReminderPlanner.ForMorning(local, enabled, consent), Is.Null);
            Assert.That(LocalReminderPlanner.ForBackground(local, enabled, consent), Is.Null);
        }
        [TestCase(0)]
        [TestCase(17)]
        [TestCase(23)]
        public void MorningReminderIsTomorrowAtNineEvenWhenSessionEndsAfterNine(int hour)
        {
            var local = new DateTime(2026, 10, 7, hour, 37, 0, DateTimeKind.Local);
            Assert.That(LocalReminderPlanner.ForMorning(local, true, true), Is.EqualTo(new DateTime(2026, 10, 8, 9, 0, 0, DateTimeKind.Local)));
        }
        [TestCase(0)]
        [TestCase(17)]
        [TestCase(23)]
        public void EveningReminderIsTomorrowAtSeven(int hour)
        {
            var local = new DateTime(2026, 10, 7, hour, 37, 0, DateTimeKind.Local);
            Assert.That(LocalReminderPlanner.ForBackground(local, true, true), Is.EqualTo(new DateTime(2026, 10, 8, 19, 0, 0, DateTimeKind.Local)));
        }
        [Test]
        public void RollingWindowSchedulesBothLocalTimesForEachFutureDay()
        {
            var local = new DateTime(2026, 10, 7, 23, 59, 0, DateTimeKind.Local);
            Assert.That(LocalReminderPlanner.ForMorning(local, true, true, 14), Is.EqualTo(new DateTime(2026, 10, 21, 9, 0, 0, DateTimeKind.Local)));
            Assert.That(LocalReminderPlanner.ForBackground(local, true, true, 14), Is.EqualTo(new DateTime(2026, 10, 21, 19, 0, 0, DateTimeKind.Local)));
        }
        [Test]
        public void ReminderCannotBeScheduledForTodayOrThePast()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LocalReminderPlanner.ForMorning(DateTime.Now, true, true, 0));
        }
        [TestCase(2026, 12, 31, 2027, 1, 1)]
        [TestCase(2028, 2, 28, 2028, 2, 29)]
        [TestCase(2028, 2, 29, 2028, 3, 1)]
        public void NextLocalDayHandlesYearAndLeapDayBoundaries(int year, int month, int day, int nextYear, int nextMonth, int nextDay)
        {
            Assert.That(LocalReminderPlanner.ForBackground(new DateTime(year, month, day, 22, 0, 0), true, true),
                Is.EqualTo(new DateTime(nextYear, nextMonth, nextDay, 19, 0, 0)));
        }
        [Test] public void UtcIsNotMistakenForLocalCalendarTime()
        {
            Assert.Throws<ArgumentException>(() => LocalReminderPlanner.ForBackground(DateTime.SpecifyKind(new DateTime(2026, 10, 7), DateTimeKind.Utc), true, true));
        }
        [Test] public void NewProfilesKeepRemindersOffUntilThePlayerEnablesThem()
        {
            var preferences = new GamePreferences();
            Assert.That(preferences.NotificationsEnabled, Is.False);
            Assert.That(preferences.NotificationPermissionRequested, Is.False);
            Assert.That(LocalReminderPlanner.ForMorning(DateTime.Now, preferences.NotificationsEnabled, true), Is.Null);
            Assert.That(LocalReminderPlanner.ForBackground(DateTime.Now, true, false), Is.Null);
        }
    }
}
