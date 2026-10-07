using System;

namespace Cascade.Content
{
    public sealed class WorldComicPage
    {
        public string Caption { get; }
        public StoryLine[] Lines { get; }
        public WorldComicPage(string caption,params StoryLine[] lines){Caption=caption;Lines=lines;}
    }
    public sealed class WorldPrologue
    {
        public int World { get; }
        public int Receipt { get; }
        public WorldComicPage[] Pages { get; }
        public WorldPrologue(int world,params WorldComicPage[] pages){World=world;Receipt=6000+world;Pages=pages;}
    }
    // Three short comic pages before each world. Art and reading never change puzzle state.
    public static class WorldPrologues
    {
        public static WorldPrologue ForWorld(int world)
        {
            switch(world) {
                case 0: return new WorldPrologue(world,
                        new WorldComicPage("A light to carry", new StoryLine("Gran","The storm took our island lights. I will be waiting at the Great Beacon."), new StoryLine("Mika","I will bring our little light to you. I just wish the bridges were lower.")),
                        new WorldComicPage("The broken way", new StoryLine("Bram","I built these bridges. Seeing them broken feels like losing old friends."), new StoryLine("Mika","Then we will give them new boards. Will you stay while I cross?")),
                        new WorldComicPage("A builder's promise", new StoryLine("Bram","Bram. Builder, bread lover, and your first travelling friend. I will stay."), new StoryLine("Mika","One bridge first. The beacon can wait for one careful step.")));
                case 1: return new WorldPrologue(world,
                        new WorldComicPage("The quiet wheel", new StoryLine("Bram","Pip's mill used to sing all day. Now the buckets come back empty."), new StoryLine("Mika","I can hear the rain. I cannot hear the water reaching the village.")),
                        new WorldComicPage("Too fast to listen", new StoryLine("Pip","I tried turning everything at once. My little brother got the biggest splash."), new StoryLine("Mika","He is still here with his bucket. He believes you will mend it.")),
                        new WorldComicPage("Water for everyone", new StoryLine("Pip","I want to deserve that. Help me start with one wheel, Mika."), new StoryLine("Mika","One wheel, then one well. We will listen as we go.")));
                case 2: return new WorldPrologue(world,
                        new WorldComicPage("An orchard without lanterns", new StoryLine("Pip","Without Rue's lantern fruit, the birds circle past their own nests."), new StoryLine("Mika","A home should be easy to find. Let us give them a light.")),
                        new WorldComicPage("Seeds for tomorrow", new StoryLine("Rue","I saved the seeds before the fruit. Tomorrow needs a little care too."), new StoryLine("Mika","Can I carry a few? My pocket has room beside Gran's letter.")),
                        new WorldComicPage("The first branch", new StoryLine("Rue","Keep them warm. We will wake one tree and let the birds guide us."), new StoryLine("Mika","Small things can show a very long way.")));
                case 3: return new WorldPrologue(world,
                        new WorldComicPage("Letters in the clouds", new StoryLine("Rue","Sora knows every sky route. The storm scattered the letters across them."), new StoryLine("Mika","There is a letter for Gran somewhere up there. We will find it.")),
                        new WorldComicPage("The letter not sent", new StoryLine("Sora","I deliver everybody's letters. My own has waited in my pocket for months."), new StoryLine("Mika","Maybe you can write it while I mend the post bridge.")),
                        new WorldComicPage("A route between friends", new StoryLine("Sora","First we get these letters home. Then I will find my first sentence."), new StoryLine("Mika","You could start with what you miss. That is how I started Gran's.")));
                case 4: return new WorldPrologue(world,
                        new WorldComicPage("No light on the shore", new StoryLine("Sora","The lagoon still shines like glass, but Neri's boats cannot find the shore."), new StoryLine("Mika","Deep water makes my paws shake. I can still help from the pier.")),
                        new WorldComicPage("The first little boat", new StoryLine("Neri","My mother helped me fold this boat. I promised to keep its route open."), new StoryLine("Mika","Let us start with the nearest lamp. I can reach that one.")),
                        new WorldComicPage("A safe place to land", new StoryLine("Neri","You do not have to become brave all at once. I will hold the boat."), new StoryLine("Mika","And I will make the first landing safe for both of us.")));
                case 5: return new WorldPrologue(world,
                        new WorldComicPage("Rain between the shelves", new StoryLine("Neri","Toma keeps every island map in the Moss Archive. Rain has found a way in."), new StoryLine("Mika","We need the road through the snow. We can mend the roof on our way.")),
                        new WorldComicPage("A map kept safe", new StoryLine("Toma","That map is the last copy. If you take it, what will stay on my shelf?"), new StoryLine("Mika","A space for the story we bring back.")),
                        new WorldComicPage("A map put to use", new StoryLine("Toma","A road belongs under someone's paws. I will help you read it, Mika."), new StoryLine("Mika","Then the archive is coming with us, one page at a time.")));
                case 6: return new WorldPrologue(world,
                        new WorldComicPage("The road disappears", new StoryLine("Toma","Snow covered the warm trail lamps. Even Lumi cannot see the next ridge."), new StoryLine("Mika","I thought guides always knew the way.")),
                        new WorldComicPage("A guide needs company", new StoryLine("Lumi","I know this mountain. I do not know every snowflake. Stay close today."), new StoryLine("Mika","We can share my lantern. Gran's scarf is long enough for two.")),
                        new WorldComicPage("One warm footprint", new StoryLine("Lumi","First we wake the steps. Then the mountain will show us its shape again."), new StoryLine("Mika","And if it does not, we will wait together.")));
                case 7: return new WorldPrologue(world,
                        new WorldComicPage("A window still glowing", new StoryLine("Lumi","Gran kept a lamp in her window while the Great Beacon was dark."), new StoryLine("Mika","I saw it from the snow road. You knew I would come.")),
                        new WorldComicPage("Stories at the door", new StoryLine("Gran","I hoped you would. Come warm your paws and tell me who helped you."), new StoryLine("Mika","So many friends. I brought a little piece of every island home.")),
                        new WorldComicPage("Two keepers, one light", new StoryLine("Gran","My knees dislike these stairs now. Will you carry the light beside me?"), new StoryLine("Mika","I will, Gran. Let us make a light our friends can see.")));
                case 8: return new WorldPrologue(world,
                        new WorldComicPage("Above the beacon", new StoryLine("Mika","The beacon is shining. Above its clouds, the star routes are still dark."), new StoryLine("Gran","Aster watches there. After breakfast, you could take him a light.")),
                        new WorldComicPage("An imperfect chart", new StoryLine("Aster","I am Aster. My charts lost some stars. I have been trying to fix them alone."), new StoryLine("Mika","There is room for two by your telescope. May I look with you?")),
                        new WorldComicPage("A path for night travellers", new StoryLine("Aster","The chart is not perfect yet. But perhaps we can start with the moon dial."), new StoryLine("Mika","We do not need every star to take the first step.")));
                default: throw new ArgumentOutOfRangeException(nameof(world));
            }
        }
    }
}
