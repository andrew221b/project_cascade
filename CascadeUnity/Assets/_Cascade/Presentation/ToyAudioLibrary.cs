using UnityEngine;

namespace Cascade.Presentation
{
    public enum ToyCue { Select, Swap, Land, Fragile, Unlock, Repair, Bomb, Defuse, Bridge, Stamp, Success, Reject }
    public sealed class ToyAudioLibrary : ScriptableObject
    {
        public AudioClip[] cues;
        public AudioClip[] alternates;
        public AudioClip music;
        public AudioClip menuMusic;
        public AudioClip[] worldMusic;
        public AudioClip naturalWind;

        public AudioClip ScoreFor(GameScreen screen, int zone)
        {
            bool inWorld = screen == GameScreen.Playing || screen == GameScreen.Pause || screen == GameScreen.Win
                || screen == GameScreen.Fail || screen == GameScreen.Checkpoint || screen == GameScreen.JourneyMap
                || screen == GameScreen.Dialogue || screen == GameScreen.JourneyEnd;
            if (inWorld && worldMusic != null && zone >= 0 && zone < worldMusic.Length && worldMusic[zone] != null)
                return worldMusic[zone];
            return menuMusic != null ? menuMusic : music;
        }
    }
}
