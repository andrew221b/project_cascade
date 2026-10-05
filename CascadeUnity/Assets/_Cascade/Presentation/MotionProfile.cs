using UnityEngine;

namespace Cascade.Presentation
{
    [CreateAssetMenu(menuName = "CASCADE/Motion Profile")]
    public sealed class MotionProfile : ScriptableObject
    {
        [Min(.01f)] public float selection = .10f;
        [Min(.01f)] public float swap = .19f;
        [Min(.01f)] public float contact = .18f;
        [Min(.01f)] public float bomb = .24f;
        [Min(.01f)] public float fracture = .13f;
        [Min(.01f)] public float fall = .30f;
        [Min(.01f)] public float landing = .11f;
        [Min(.01f)] public float delivery = .34f;
        [Min(.01f)] public float reducedStep = .055f;
        [Min(0)] public float swapArc = .20f;
        [Range(0, .1f)] public float platformRecoil = .025f;
        [Range(0, .2f)] public float landingSquash = .08f;
        public int dustCapacity = 32;
        public int voiceCapacity = 8;
    }
}
