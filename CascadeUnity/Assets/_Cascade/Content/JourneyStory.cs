using System;

namespace Cascade.Content
{
    // Story and map pages bind published concrete puzzles; never send gameplay commands.
    public static class JourneyStory
    {
        public static int Zone(int stop)=>Math.Max(0,Math.Min(WorldCount-1,stop/60));
        public static readonly System.Collections.ObjectModel.ReadOnlyCollection<string> NodeIds=Array.AsReadOnly(BuildRoute());
        static readonly int[] PageStarts=Array.ConvertAll(new[]{0,5,10,15,20,25,30,35,40,45,50,55,60,64,69,74,79,84,88,93,98,103,108,112,117,122,127,132,136,141,146,151,156,160,165,170,175,180},
            stop=>NodeIds.IndexOf("C"+(stop+1).ToString("D2")));
        public const int WorldCount=9;
        public const int TotalLevels=540;
        static string[] BuildRoute()
        {
            var route=new System.Collections.Generic.List<string>();var legacy=new[]{0,15,30,40,64,88,112,136,160,184};int next=185;
            for(int world=0;world<WorldCount;world++)for(int island=0;island<5;island++){
                int count=legacy[world+1]-legacy[world],first=legacy[world]+(count*island+4)/5,last=legacy[world]+(count*(island+1)+4)/5;
                for(int stop=first;stop<last;stop++)route.Add("C"+(stop+1).ToString("D2"));
                for(int extra=last-first;extra<12;extra++)route.Add("C"+(next++).ToString("D2"));
            }
            return route.ToArray();
        }
        public static bool IsWorldFinal(string node){int index=NodeIds.IndexOf(node);return index>=0&&(index+1)%60==0;}
        public static int WorldFirst(int world)=>60*Math.Max(0,Math.Min(WorldCount-1,world));
        public static int WorldLevels(int world)=>60;
        public static int IslandFirst(int world,int island)=>WorldFirst(world)+12*Math.Max(0,Math.Min(5,island));
        public static int IslandLevels(int world,int island)=>IslandFirst(world,island+1)-IslandFirst(world,island);
        public static int IslandFor(int stop){int world=Zone(stop);for(int i=4;i>=0;i--)if(stop>=IslandFirst(world,i))return i;return 0;}
        public static int PageCount=>PageStarts.Length;
        public static int FirstStop(int page)=>PageStarts[Math.Max(0,Math.Min(PageStarts.Length-1,page))];
        public static int PageFor(int stop){for(int p=PageStarts.Length-1;p>=0;p--)if(stop>=PageStarts[p])return p;return 0;}
        public static int StopsOnPage(int page)=>page+1<PageCount?FirstStop(page+1)-FirstStop(page):TotalLevels-FirstStop(page);
        public static string MapAsset(int zone)=>new[]{"WindGardenMap","RainMillMap","CopperOrchardMap","CloudPostMap","GlassLagoonMap","MossArchiveMap","SnowSpireMap","GreatBeaconMap","StarObservatoryMap"}[Math.Max(0,Math.Min(WorldCount-1,zone))];
        public static string ZoneName(int zone) => new[]{"Wind Gardens","Rain Mill","Copper Orchard","Cloud Post","Glass Lagoon","Moss Archive","Snow Spire","Great Beacon","Star Observatory"}[Math.Max(0,Math.Min(WorldCount-1,zone))];
        public static string Portrait(int zone)=>StoryConversations.Resident(zone);
        public static string Speaker(int zone)=>StoryConversations.Resident(zone);
        public static string Task(int page)=>StoryConversations.Task(FirstStop(page));
        public static string[] Lines(int page)
        {
            var scene=StoryConversations.IslandOpening(Zone(FirstStop(page)),IslandFor(FirstStop(page)));
            return Array.ConvertAll(scene.Lines,line=>line.Text);
        }
    }
}
