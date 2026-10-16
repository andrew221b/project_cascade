using UnityEngine;
namespace Cascade.Presentation
{
    // Body lengths share one scale. Depth never makes a bee the size of a bird
    // on the same plane; wings and anatomical pivots remain the rig's concern.
    public static class WildlifePerspective
    {
        public static float Length(WildlifeSpecies species)
        {
            switch(species){
                case WildlifeSpecies.BarnOwl:return 2.3f;
                case WildlifeSpecies.Gull:return 2;
                case WildlifeSpecies.Tern:return 1.7f;
                case WildlifeSpecies.Bullfinch:return 1.1f;
                case WildlifeSpecies.WinterWren:return .76f;
                case WildlifeSpecies.SnowBunting:return .9f;
                case WildlifeSpecies.Bumblebee:return .18f;
                case WildlifeSpecies.Dragonfly:case WildlifeSpecies.Damselfly:return .29f;
                case WildlifeSpecies.Butterfly:return .25f;
                case WildlifeSpecies.LunaMoth:case WildlifeSpecies.MoonMoth:return .30f;
                case WildlifeSpecies.BookMoth:return .19f;
                case WildlifeSpecies.Ladybird:return .08f;
                case WildlifeSpecies.LanternBeetle:case WildlifeSpecies.JewelBeetle:return .12f;
                case WildlifeSpecies.FieldCricket:return .22f;
                default:return 1;
            }
        }
        public static float Proximity(float progress,float seed,int lane=0)
        {
            int band=(Mathf.FloorToInt(seed*2.999f)+lane)%3;
            return Mathf.Clamp01(.16f+band*.31f+Mathf.Sin(Mathf.Clamp01(progress)*Mathf.PI)*.10f+Mathf.Sin(progress*Mathf.PI*2+seed*6)*.025f);
        }
        public static float Scale(float proximity)=>Mathf.Lerp(.45f,1.8f,Mathf.SmoothStep(0,1,proximity));
        public static float BodySize(WildlifeSpecies species,float viewportWidth,float proximity)=>viewportWidth*.031f*Length(species)*Scale(proximity);
        public static Color Tint(Color light,float proximity,float alpha)
        {
            var colour=Color.Lerp(light,new Color(.76f,.86f,.94f),Mathf.Lerp(.46f,.04f,proximity));colour.a=alpha*Mathf.Lerp(.66f,.96f,proximity);return colour;
        }
    }
}
