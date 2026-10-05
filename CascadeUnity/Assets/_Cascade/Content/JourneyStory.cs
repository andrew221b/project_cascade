using System;

namespace Cascade.Content
{
    // Presentation story binds existing concrete puzzles; no new gameplay IDs or rules.
    public static class JourneyStory
    {
        public static int Zone(int zeroBasedStop) => zeroBasedStop < 15 ? 0 : zeroBasedStop < 30 ? 1 : 2;
        public static string ZoneName(int zone) => new[]{"Wind Gardens","Rain Mill","Copper Orchard"}[Math.Max(0,Math.Min(2,zone))];
        public static string Portrait(int zone) => new[]{"Mika","Pip","Rue"}[Math.Max(0,Math.Min(2,zone))];
        public static string Speaker(int zone) => new[]{"Mika","Pip","Rue"}[Math.Max(0,Math.Min(2,zone))];
        public static string Task(int page) => new[]{"Light the first bridge","Open the wind gate","Mend the sky lift","Help Pip turn the mill",
            "Bring water to the village","Repair the post boat","Wake the orchard lights","Open the road to the beacon"}[Math.Max(0,Math.Min(7,page))];
        public static string[] Lines(int page)
        {
            switch(page) {
                case 0:return new[]{"The storm took our lights. I will carry a new one to the Great Beacon.","This bridge is high... One small step, Mika. Let's fix its pattern."};
                case 1:return new[]{"One light is back! Now I can see the wind gate.","These boards move when we land. I'll look before I leap."};
                case 2:return new[]{"My scarf got caught. A little patch will hold it.","The sky lift needs a patch too. Then we can reach the mill."};
                case 3:return new[]{"I'm Pip. My mill stopped, and the village needs water.","Will you help me mend it? We can work it out together."};
                case 4:return new[]{"The wheel is turning! I can hear the water.","Some boards are cracked. Use the strong ones when you can."};
                case 5:return new[]{"The post boat can sail again. I kept a letter for your gran.","Take it with you. She'll be glad to know we're all right."};
                case 6:return new[]{"I'm Rue. I grow lantern fruit. They glow when the night comes.","Help me light the trees. Try to find a stamp along the way."};
                default:return new[]{"Look, Mika. The beacon is beyond those clouds.","You've helped us find each other. We'll help you reach it."};
            }
        }
    }
}
