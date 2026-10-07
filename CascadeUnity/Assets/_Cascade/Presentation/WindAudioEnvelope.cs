using UnityEngine;

namespace Cascade.Presentation
{
    // Cosmetic gain only. Input is the actual signed pressure from ToyMachineView.WindAt.
    public sealed class WindAudioEnvelope
    {
        public float Volume { get; private set; }
        public static float Target(float pressure, float attenuation = 1)
        {
            float strength = Mathf.Clamp01(Mathf.Abs(pressure));
            // A tiny breeze should not expose the recording's stronger internal gusts.
            return .15f * strength * strength * Mathf.Clamp01(attenuation);
        }
        public float Tick(float pressure, float attenuation, bool allowed, float seconds)
        {
            float dt = Mathf.Clamp(seconds, 0, .1f);
            if (!allowed) Volume = Mathf.MoveTowards(Volume, 0, dt * .9f);
            else {
                float target = Target(pressure, attenuation);
                float response = target > Volume ? .10f : .16f;
                Volume = Mathf.Lerp(Volume, target, 1 - Mathf.Exp(-dt / response));
                if (Volume < .0001f && target < .0001f) Volume = 0;
            }
            return Volume;
        }
        public void Reset() => Volume = 0;
    }
}
