using System;
using UnityEngine;

namespace Cascade.Presentation
{
    // A silent, isolated demonstration. Its clock never advances the player's attempt.
    public sealed class LessonFilm : MonoBehaviour
    {
        CascadeGame owner;
        Action<float, bool> paint;
        CanvasGroup opacity;
        Sprite ownedMotif;
        float elapsed;
        public float Elapsed => elapsed;
        public int CycleCount { get; private set; }
        public const float Duration = 8;
        public void OwnMotif(Sprite sprite) { ownedMotif = sprite; }

        public void Initialize(CascadeGame game, CanvasGroup group, Action<float, bool> draw)
        {
            owner = game; opacity = group; paint = draw; elapsed = 0; CycleCount = 0;
            Paint();
        }

        void LateUpdate()
        {
            if (owner == null || paint == null || !owner.IntroductionAnimationAllowed) return;
            float next = elapsed + Time.unscaledDeltaTime;
            if (next >= Duration) CycleCount += Mathf.FloorToInt(next / Duration);
            elapsed = Mathf.Repeat(next, Duration);
            Paint();
        }

        void Paint()
        {
            bool still = owner.ReducedMotion;
            // With reduced motion, change only between held before / after poses.
            // The brief crossfade passes through zero before the pose changes.
            float alpha = elapsed < .35f ? elapsed / .35f : elapsed > 7.55f ? (8 - elapsed) / .45f : 1;
            if (still && elapsed >= 3.55f && elapsed <= 4.05f)
                alpha *= Mathf.Abs(elapsed - 3.8f) / .25f;
            opacity.alpha = Mathf.Clamp01(alpha);
            paint(still ? (elapsed < 3.8f ? 1.7f : 6.4f) : elapsed, still);
        }

        void OnDestroy() { paint = null; owner = null; if (ownedMotif != null) Destroy(ownedMotif); }
    }
}
