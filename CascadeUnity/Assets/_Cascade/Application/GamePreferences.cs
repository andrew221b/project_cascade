namespace Cascade.Application
{
    // In-memory preferences; versioned durable storage is introduced in P16.
    public sealed class GamePreferences
    {
        public bool Music = true, Sound = true, Haptics = true;
        public bool ReducedMotion, ScreenShake, HighContrast, LargeText, PreciseInput;
        public bool Tutorials = true, DeadEndWarnings = true;
        public int EffectQuality = 2;
        public string UiLocale = "en", PuzzleLocale = "en";
    }
}
