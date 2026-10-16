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
    // Four short comic pages before each world. Art and reading never change puzzle state.
    public static class WorldPrologues
    {
        public static WorldPrologue ForWorld(int world)
        {
            switch(world) {
                case 0: return new WorldPrologue(world,
                        new WorldComicPage("A light to carry", new StoryLine("Gran","If you see my window light, I'm still here. Don't try the broken bridges alone."), new StoryLine("Mika","She wrote that before the storm. The beacon is dark, but I can't see her window from home.")),
                        new WorldComicPage("The broken way", new StoryLine("Bram","I built that bridge. Twenty winters it stood. Now I can't even put my name on it."), new StoryLine("Mika","Don't scrape it off yet. How will I know whom to thank when I get across?")),
                        new WorldComicPage("A builder's promise", new StoryLine("Bram","You think an old builder can mend this? I've spent three days sorting nails."), new StoryLine("Mika","Then you know where every nail is. That's a start. I'll hold the lantern for you.")),
                        new WorldComicPage("The first crossing", new StoryLine("Bram","All right. Pass me the long ones. And tell your gran her bridge builder hasn't retired."), new StoryLine("Mika","Tell her yourself when the road is open. She always sets one extra cup.")));
                case 1: return new WorldPrologue(world,
                        new WorldComicPage("The quiet wheel", new StoryLine("Bram","Hear that? No mill wheel. I used to complain it woke me before breakfast."), new StoryLine("Mika","There are buckets at every door. Even the little houses have big ones.")),
                        new WorldComicPage("Too fast to listen", new StoryLine("Pip","My brother asked if rain could fill them. I said I'd have the mill running by lunch."), new StoryLine("Mika","Which lunch? Today's or yesterday's?")),
                        new WorldComicPage("Water for everyone", new StoryLine("Pip","Yesterday's. I turned the wheel too fast and snapped another tooth. He saw everything."), new StoryLine("Mika","Then let him see you fix it too. My gran says hiding a broken cup never mends it.")),
                        new WorldComicPage("One gear, one bucket", new StoryLine("Pip","I'll ask him to bring the spare gear. And I'll tell him why we need it."), new StoryLine("Mika","I'll wait by his bucket. This time let's promise water, not a lunchtime.")));
                case 2: return new WorldPrologue(world,
                        new WorldComicPage("An orchard without lanterns", new StoryLine("Pip","Rue sent us fruit through the storm. I haven't taken her water in weeks."), new StoryLine("Mika","Bring a watering can. A thank-you might be thirsty.")),
                        new WorldComicPage("Seeds for tomorrow", new StoryLine("Rue","Mind that crooked tree. I planted it the year I learnt to tie my apron."), new StoryLine("Mika","It's taller than your house now. Did you plant it crooked on purpose?")),
                        new WorldComicPage("The first branch", new StoryLine("Rue","No. I nearly pulled it out. Now the birds nest in the bend I wanted to straighten."), new StoryLine("Mika","Then let's leave their bend alone. Gran has room for a crooked tree too.")),
                        new WorldComicPage("A pocket of seeds", new StoryLine("Rue","Take these three seeds. Plant one by her window, and keep two in case the first sleeps in."), new StoryLine("Mika","I'll write to you when it wakes up. Even if it grows sideways.")));
                case 3: return new WorldPrologue(world,
                        new WorldComicPage("Letters in the clouds", new StoryLine("Rue","Sora can find a house from one bent chimney. But the storm has mixed up all her letters."), new StoryLine("Mika","Perhaps there's a reply from Gran. I'll help look.")),
                        new WorldComicPage("The letter not sent", new StoryLine("Sora","That's mine. Please leave it. It hasn't got a stamp yet."), new StoryLine("Mika","It's worn thin at the folds. Have you been waiting for a stamp for long?")),
                        new WorldComicPage("A route between friends", new StoryLine("Sora","Months. Clara asked me to stay when her mother was ill. I said I had deliveries."), new StoryLine("Mika","Is that what you wrote in the letter?")),
                        new WorldComicPage("The first honest sentence", new StoryLine("Sora","No. Three pages about the weather. Not one word about leaving her alone."), new StoryLine("Mika","Start with that word. I'll find you a stamp while you write.")));
                case 4: return new WorldPrologue(world,
                        new WorldComicPage("No light on the shore", new StoryLine("Sora","Clara lives across the lagoon. Will you take this? I've finally written the first sentence."), new StoryLine("Mika","Come and give it to her. I'll walk beside you.")),
                        new WorldComicPage("The first little boat", new StoryLine("Neri","I saw the post bag on the pier. Is Sora waiting outside?"), new StoryLine("Mika","Yes. She wants to talk. I can wait here if you'd rather be alone.")),
                        new WorldComicPage("A safe place to land", new StoryLine("Neri","After we mend the pier. Mother built it so visitors could step ashore with dry paws."), new StoryLine("Mika","I'm glad she thought of that. Mine shake when I look at deep water.")),
                        new WorldComicPage("The nearest lamp", new StoryLine("Neri","Keep them on this board. I'll hold the boat. You needn't prove anything to the pond."), new StoryLine("Mika","Good. I was afraid it might ask me to swim.")));
                case 5: return new WorldPrologue(world,
                        new WorldComicPage("Rain between the shelves", new StoryLine("Neri","Oscar has the snow-road map. Tell him you'll bring it back. He'll ask twice."), new StoryLine("Mika","I'll answer twice. I need that road to reach Gran.")),
                        new WorldComicPage("A map kept safe", new StoryLine("Toma","My teacher drew this map. It's the last one. If it gets wet, I can't ask him for another."), new StoryLine("Mika","Then leave it here. May I copy the road onto the back of my letter?")),
                        new WorldComicPage("A map put to use", new StoryLine("Toma","There's a blank turn by the ridge. I always meant to ask him about it."), new StoryLine("Mika","Let's leave it blank on mine too. Alice can show us when we get there.")),
                        new WorldComicPage("A map for the road", new StoryLine("Toma","Bring back what you find. He left room for notes. I thought I wasn't allowed to add any."), new StoryLine("Mika","He left you a pencil too, didn't he? Keep it sharp for us.")));
                case 6: return new WorldPrologue(world,
                        new WorldComicPage("The road disappears", new StoryLine("Toma","Alice knows the ridge. Follow her, but don't make her hurry. She never asks for a rest."), new StoryLine("Mika","I'll keep the blanket where I can reach it.")),
                        new WorldComicPage("A guide needs company", new StoryLine("Lumi","The old turn has fallen away. Your map is wrong here. I've led travellers along it for years."), new StoryLine("Mika","Then I'm glad we have you, not just the map.")),
                        new WorldComicPage("One warm footprint", new StoryLine("Lumi","We'll go around. I said we'd reach the beacon tonight. I shouldn't have said tonight."), new StoryLine("Mika","Gran would rather open her door tomorrow than have us freeze outside it today.")),
                        new WorldComicPage("Half a scarf each", new StoryLine("Lumi","There's a shelter below. Just until the gust passes. And perhaps a little longer."), new StoryLine("Mika","Long enough for tea. I'll pour it while you unfold Ben's blanket.")));
                case 7: return new WorldPrologue(world,
                        new WorldComicPage("A window still glowing", new StoryLine("Lumi","Two flashes in the window. Someone saw our light."), new StoryLine("Mika","That's her signal! Gran, don't come onto the bridge. We're nearly there!")),
                        new WorldComicPage("Stories at the door", new StoryLine("Gran","Your scarf has a new patch. And my letter has more folds than a concertina."), new StoryLine("Mika","I kept opening it. I knew the words, but I wanted to hear your voice.")),
                        new WorldComicPage("Two keepers, one light", new StoryLine("Gran","Here I am. Come closer. You can put that bag down for a minute."), new StoryLine("Mika","Carefully! There are seeds in it. Rose sent you a tree that hasn't happened yet.")),
                        new WorldComicPage("The keeper's lamp", new StoryLine("Gran","We'll find it a pot. Then help me with the beacon. My knees won't carry the lamp up anymore."), new StoryLine("Mika","I'll carry it. You tell me where to shine. Everyone down there is waiting.")));
                case 8: return new WorldPrologue(world,
                        new WorldComicPage("Above the beacon", new StoryLine("Mika","The mill answered our beacon. But those gliders keep turning back above the clouds."), new StoryLine("Gran","Felix's route lamps are dark. Take him breakfast. He forgets to eat when he worries.")),
                        new WorldComicPage("An imperfect chart", new StoryLine("Aster","That glider followed my chart into a cloud bank. I closed the route before anyone else tried."), new StoryLine("Mika","Did the pilot get home?")),
                        new WorldComicPage("A path for night travellers", new StoryLine("Aster","Yes. He came back to show me the wrong turn. I was too ashamed to open the door."), new StoryLine("Mika","Could we ask him again? We could have breakfast at the telescope while we wait.")),
                        new WorldComicPage("A second reading", new StoryLine("Aster","I'll send a signal. But we'll check the route from his glider, not just from my desk."), new StoryLine("Mika","And leave the door open. He's bringing the bit of sky we couldn't see.")));
                default: throw new ArgumentOutOfRangeException(nameof(world));
            }
        }
    }
}
