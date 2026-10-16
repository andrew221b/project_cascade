namespace Cascade.Application
{
    // Stored in the versioned profile; active UI locale remains LocalizationSettings.SelectedLocale.
    public sealed class GamePreferences
    {
        public bool Music = true, Sound = true, Haptics = true;
        public bool ReducedMotion, ScreenShake, HighContrast, LargeText, PreciseInput;
        public bool Tutorials = true, DeadEndWarnings = true;
        public bool NotificationsEnabled;
        public bool NotificationPermissionRequested;
        public int EffectQuality = 2;
        public string UiLocale = "en", PuzzleLocale = "en";
        // Null identifies saves made before automatic device-language selection.
        [Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? FollowSystemLanguage;
    }
}
