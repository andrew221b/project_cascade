using System;

namespace Cascade.Content
{
    // Story and map pages bind published concrete puzzles; never send gameplay commands.
    public static class JourneyStory
    {
        public static int Zone(int stop)=>stop<15?0:stop<30?1:stop<40?2:stop<64?3:stop<88?4:stop<112?5:stop<136?6:stop<160?7:8;
        static readonly int[] PageStarts={0,5,10,15,20,25,30,35,40,45,50,55,60,64,69,74,79,84,88,93,98,103,108,112,117,122,127,132,136,141,146,151,156,160,165,170,175,180};
        static readonly int[] WorldStarts={0,15,30,40,64,88,112,136,160};
        public const int WorldCount=9;
        public const int TotalLevels=184;
        public static int WorldFirst(int world)=>WorldStarts[Math.Max(0,Math.Min(WorldCount-1,world))];
        public static int WorldLevels(int world)=>(world<WorldCount-1?WorldFirst(world+1):TotalLevels)-WorldFirst(world);
        public static int IslandFirst(int world,int island)=>WorldFirst(world)+(WorldLevels(world)*Math.Max(0,Math.Min(5,island))+4)/5;
        public static int IslandLevels(int world,int island)=>IslandFirst(world,island+1)-IslandFirst(world,island);
        public static int IslandFor(int stop){int world=Zone(stop);for(int i=4;i>=0;i--)if(stop>=IslandFirst(world,i))return i;return 0;}
        public static int PageCount=>PageStarts.Length;
        public static int FirstStop(int page)=>PageStarts[Math.Max(0,Math.Min(PageStarts.Length-1,page))];
        public static int PageFor(int stop){for(int p=PageStarts.Length-1;p>=0;p--)if(stop>=PageStarts[p])return p;return 0;}
        public static int StopsOnPage(int page)=>page+1<PageCount?FirstStop(page+1)-FirstStop(page):TotalLevels-FirstStop(page);
        public static string MapAsset(int zone)=>new[]{"WindGardenMap","RainMillMap","CopperOrchardMap","CloudPostMap","GlassLagoonMap","MossArchiveMap","SnowSpireMap","GreatBeaconMap","StarObservatoryMap"}[Math.Max(0,Math.Min(WorldCount-1,zone))];
        public static string ZoneName(int zone) => new[]{"Wind Gardens","Rain Mill","Copper Orchard","Cloud Post","Glass Lagoon","Moss Archive","Snow Spire","Great Beacon","Star Observatory"}[Math.Max(0,Math.Min(WorldCount-1,zone))];
        public static string Portrait(int zone) => new[]{"Mika","Pip","Rue","Pip","Rue","Rue","Mika","Mika","Aster"}[Math.Max(0,Math.Min(WorldCount-1,zone))];
        public static string Speaker(int zone) => new[]{"Mika","Pip","Rue","Pip","Rue","Rue","Mika","Mika","Aster"}[Math.Max(0,Math.Min(WorldCount-1,zone))];
        public static string Task(int page) => new[]{"Light the first bridge","Open the wind gate","Mend the sky lift","Help Pip turn the mill",
            "Bring water to the village","Repair the post boat","Wake the orchard lights","Open the road to the beacon","Tie the mail bridge","Catch the lost letters","Open the post gate","Lift the mail basket","Send Gran's letter","Mend the little pier","Turn the water gate","Wake the glass lamps","Carry the pond light","Help Rue's garden","Open the leaf door","Untie the old shelves","Find the storm map","Keep the page dry","Read Gran's note","Light the snow steps","Fix the warm shelter","Cross the frozen arch","Carry Pip's blanket","Find the beacon road","Open the last bridge","Mend the lantern lift","Light the beacon stairs","Lift the sky lantern","Bring the light home","Wake the moon dial","Mend the telescope bridge","Gather the scattered star charts","Relight the constellations","Open the night travellers' route"}[Math.Max(0,Math.Min(PageCount-1,page))];
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
                case 7:return new[]{"Look, Mika. The beacon is beyond those clouds.","You've helped us find each other. We'll help you reach it."};
                case 8:return new[]{"Pip brought a bag of letters.","One is for Gran. Let's fix the mail bridge."};
                case 9:return new[]{"The wind scattered our letters.","I'll hold the bag. You fix the pattern."};
                case 10:return new[]{"This gate is tied shut.","The little key can help us."};
                case 11:return new[]{"The basket is too heavy for me.","We can lift it together."};
                case 12:return new[]{"There goes Gran's letter!","Pip drew a heart on the bag."};
                case 13:return new[]{"Rue is waiting by the water.","Her little pier has a crack."};
                case 14:return new[]{"The pond has gone quiet.","Open the gate and let the water flow."};
                case 15:return new[]{"These lamps look like tiny stars.","They need their patterns back."};
                case 16:return new[]{"A frog came out to watch us.","Easy, little light. We are almost there."};
                case 17:return new[]{"The flowers have water again!","Rue gave me a leaf for my pocket."};
                case 18:return new[]{"This old library smells of leaves.","A small door hides the storm map."};
                case 19:return new[]{"These shelves move in pairs.","I will watch both ends this time."};
                case 20:return new[]{"Here is the road through the snow.","The beacon is still far, but we know the way."};
                case 21:return new[]{"A drop of rain fell on the page.","My scarf will keep it dry."};
                case 22:return new[]{"Gran left a note here for me.","One small step is enough to begin."};
                case 23:return new[]{"My paws are cold.","I can see a warm lamp up ahead."};
                case 24:return new[]{"Someone left us a little shelter.","We can mend its steps before we rest."};
                case 25:return new[]{"The icy arch turns the whole row around.","I will think about the landing first."};
                case 26:return new[]{"Pip sent a blanket after us.","He remembered that I get cold."};
                case 27:return new[]{"The clouds are opening.","I can see the beacon at last!"};
                case 28:return new[]{"The last bridge is very high.","I am scared. But my friends are with me."};
                case 29:return new[]{"The lantern lift needs a patch.","I have mended so many boards. I can do this."};
                case 30:return new[]{"A light for every step.","Now no one has to walk alone in the dark."};
                case 31:return new[]{"I kept our little light safe.","Just one more landing."};
                case 32:return new[]{"Gran, I am bringing the light home.","We found the way together."};
                case 33:return new[]{"The beacon is bright, but the sky paths are still dark.","Aster has been waiting above the clouds. Tonight we will help him."};
                case 34:return new[]{"The moon dial is turning. Its light reaches a tiny telescope.","Aster says he used to watch alone. I will watch with him."};
                case 35:return new[]{"These star charts blew out of Aster's notebook.","Some corners are missing. There is still enough to find the way."};
                case 36:return new[]{"One constellation is awake. I can see the next in its light.","The stars are far apart. Their pattern brings them together."};
                case 37:return new[]{"A little glider is following our new sky path.","Gran's beacon and Aster's stars are shining together."};
                default:return new[]{"We found the way together.","Our little light is home."};
            }
        }
    }
}
