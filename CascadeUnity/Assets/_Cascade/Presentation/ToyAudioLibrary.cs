using UnityEngine;

namespace Cascade.Presentation
{
    public enum ToyCue { Select, Swap, Land, Fragile, Unlock, Repair, Bomb, Defuse, Bridge, Stamp, Success, Reject }
    public sealed class ToyAudioLibrary : ScriptableObject
    {
        public AudioClip[] cues;
        public AudioClip[] alternates;
        public AudioClip music;
    }
}
