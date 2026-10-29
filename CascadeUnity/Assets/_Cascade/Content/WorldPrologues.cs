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
                        new WorldComicPage("A note from the dark beacon", new StoryLine("Gran","Rin, the High Lantern is missing. Take my hand lamp. The lower lights can still be lit, but I can't reach them now."), new StoryLine("Mika","I know the knots. It's easier to remember them with solid ground under both paws.")),
                        new WorldComicPage("Below the islands", new StoryLine("Gran","The lamps hang under the islands; no bridge reaches them. If you doubt a knot, stop and tie it again. I'll wait."), new StoryLine("Mika","All right. Walter first. I'll ask him to check my knots before I go over the edge.")),
                        new WorldComicPage("The receiver's order", new StoryLine("Bram","The same row goes all the way down. Match the receiver to light a lamp. Its relay powers the nearby machinery too."), new StoryLine("Mika","The same row all the way down? Then I can change the order on the way. Keep hold until I say.")),
                        new WorldComicPage("Lights before the storm", new StoryLine("Bram","One thing about that storm. I was measuring this bridge when the lights failed. The sky was still clear."), new StoryLine("Mika","Before the storm? Then someone may have seen what happened. We can ask once the path is lit.")));
                case 1: return new WorldPrologue(world,
                        new WorldComicPage("A witness at the mill", new StoryLine("Bram","Ben heard a crate strike the mill that night. The lights went out about then. Ask him what he saw."), new StoryLine("Mika","We'll ask where the crate went. If the marks are still there, we'll compare them with the scratch on the gates.")),
                        new WorldComicPage("The fast lever", new StoryLine("Pip","The crate hit the wheel. I pulled the fast lever. A crack, then water everywhere. My brother's tap is still dry."), new StoryLine("Mika","Is that him with the bucket? Let's tell him before he makes another trip.")),
                        new WorldComicPage("A fault beyond the mill", new StoryLine("Pip","The repair drawing arrived after I sent word about the wheel. It shows a burnt relay. I thought my lever had ruined that too."), new StoryLine("Mika","Walter found the same damage at the lift. Your lever couldn't have reached that. What happened first?")),
                        new WorldComicPage("What happened first?", new StoryLine("Pip","A crate hit the wheel. The clock stopped with it. Then I pulled the lever. My brother was still outside; the rain hadn't started."), new StoryLine("Mika","So the wheel had already stopped. We'll keep the clock as it is and check its time against Nora's flight book.")));
                case 2: return new WorldPrologue(world,
                        new WorldComicPage("A seal on a crate", new StoryLine("Pip","Copper dust on the broken lid. And Rose's stamp. Whatever was in that crate, it didn't smell of apples."), new StoryLine("Mika","Let's take her the lid. Easier to ask about something she can see.")),
                        new WorldComicPage("Whose seal is on the crate?", new StoryLine("Rue","My stamp, yes. I lend the empty crates out. They come back smelling of all sorts. That one's overdue."), new StoryLine("Mika","Was it lined with felt when it left? Some caught in the hinge.")),
                        new WorldComicPage("Felt for a lens", new StoryLine("Rue","Felix ordered the felt for a lens. The crate went to Ada's for loading; Nora collected it there."), new StoryLine("Mika","Please. Nora carried the crate, didn't she? I want to know where it went after the mill.")),
                        new WorldComicPage("The seedlings can't wait", new StoryLine("Rue","Nora collected it for the lens workshop. I'll show you the entry. We can read it while the seedling beds warm up."), new StoryLine("Mika","Good. And show me which lamps need lighting; Ada asked me to check the beds too.")));
                case 3: return new WorldPrologue(world,
                        new WorldComicPage("Tea at the post office", new StoryLine("Rue","Nora'll perch on a chimney while it's smoking. Getting her to stay for tea is harder."), new StoryLine("Mika","I'll ask for her flight book first. Tea can come with it.")),
                        new WorldComicPage("Ada's alleged order", new StoryLine("Sora","Northbound crate? Delivered. Ada's seal, fee paid. If there's damage, I need to see the receipt."), new StoryLine("Mika","It may have held the High Lantern. Who gave you the order? Ada herself?")),
                        new WorldComicPage("The unattended desk", new StoryLine("Sora","The night desk. I didn't see the sender. I was late for Clara... Let me get the book."), new StoryLine("Mika","And the order, please. We have the crate loan. Something on your copy might explain the rest.")),
                        new WorldComicPage("A letter without weather", new StoryLine("Sora","Clara's letter is in there too. I've rewritten it three times. Somehow the weather gets a whole page every time."), new StoryLine("Mika","Are you coming with us? You could give it to her at the lagoon.")));
                case 4: return new WorldPrologue(world,
                        new WorldComicPage("Clara at the dock", new StoryLine("Sora","Clara notes every tide and passing boat. She'll know whether my flight time fits. If she'll see me."), new StoryLine("Mika","There's a second rope on the quay. Let's tie up the boat first. We can ask about the journal once she's free.")),
                        new WorldComicPage("The letter beneath the cup", new StoryLine("Neri","Nora's letter is under the cup. I'll answer it. That boat's drifting; pass me the line."), new StoryLine("Mika","Here. Once she's secure, could you show me the north channel in your log?")),
                        new WorldComicPage("Her mother's boat", new StoryLine("Neri","Light came through the felt. I thought Ada had finally sent a lamp north. My mother's boat vanished beyond the last channel light."), new StoryLine("Mika","Did Ada tell you she was opening that route? Nora's order says the load was authorised.")),
                        new WorldComicPage("The north channel", new StoryLine("Neri","No. Every reply was another delay. Felix surveyed the route for me, but Ada never gave us a date."), new StoryLine("Mika","Then we need to ask her. First let's open the channel; the archive path is on the other bank.")));
                case 5: return new WorldPrologue(world,
                        new WorldComicPage("The archive door", new StoryLine("Neri","Oscar keeps the key register. He'll ask you to wipe your paws. There's a mat behind the door as well."), new StoryLine("Mika","I'll use both. Will you keep Nora here? She keeps checking the time.")),
                        new WorldComicPage("The service key", new StoryLine("Toma","The beacon key log. Ada keeps one key; Felix signs for the service key when he repairs lenses. Ada keeps the permit receipts too."), new StoryLine("Mika","Then his key alone proves nothing. We need to see what Ada allowed him to take.")),
                        new WorldComicPage("The equipment list", new StoryLine("Toma","The key log records access, not what was carried out. For that we need the equipment list on the permit."), new StoryLine("Mika","Will you help me find its number? I don't want Ada searching every file at the beacon.")),
                        new WorldComicPage("Two deferred petitions", new StoryLine("Toma","Ada has it. Here are the north-route petitions. Clara's first, then Felix's plan. Same reply: Deferred. For years."), new StoryLine("Mika","Copy those too. Ada's waiting for us, and I have more questions than will fit on her note.")));
                case 6: return new WorldPrologue(world,
                        new WorldComicPage("A question on the map", new StoryLine("Lumi","Oscar left this turn blank. Good. The old ledge fell away in the storm."), new StoryLine("Mika","Show me the new route. We'll mark it before anyone else comes this way.")),
                        new WorldComicPage("The guide's cold paws", new StoryLine("Lumi","I know the ridge. Keep close. We can reach the beacon without a stop."), new StoryLine("Mika","Lumi, that buckle's still open. This is the third time you've tried to fasten it.")),
                        new WorldComicPage("The shelter first", new StoryLine("Lumi","Can't feel the clasp. There's a shelter below. I keep meaning to stop there on the way back."), new StoryLine("Mika","We'll stop now. Nora left blankets from Ben in the shelter. Show me where to lower the light.")),
                        new WorldComicPage("A beam above the pass", new StoryLine("Lumi","The observatory beam came on before the snow. Nora's crate went that way. I thought Ada had opened the north route."), new StoryLine("Mika","Clara thought so too, but nobody told her. We'll ask Ada why that delivery never reached her.")));
                case 7: return new WorldPrologue(world,
                        new WorldComicPage("Two answering flashes", new StoryLine("Lumi","Two flashes. Ada's window. She sees us. I'll hold the bridge; answer her."), new StoryLine("Mika","Two back. Gran, stay there! Let me finish this crossing.")),
                        new WorldComicPage("The question at the door", new StoryLine("Gran","Rin… let me look at you. I've watched for your light all the way. I wanted to come and meet you, but this bridge…"), new StoryLine("Mika","I'm here, Grandma. Look at Nora's order. It has your seal. Did you tell her to take the High Lantern north?")),
                        new WorldComicPage("Ada's own delay", new StoryLine("Gran","I allowed a lens trial, not the High Lantern. After Clara's mother vanished, I was afraid to open that route."), new StoryLine("Mika","But the boats kept going. Clara needs lights, not another delay. Can we check that permit number?")),
                        new WorldComicPage("The service file", new StoryLine("Gran","The receipt is in my service file. The lift's jammed. Help me get it down, then we can check what Felix collected."), new StoryLine("Mika","I'll light the lift relay. Stay at the rail and watch the far end for me.")));
                case 8: return new WorldPrologue(world,
                        new WorldComicPage("The light behind the lens", new StoryLine("Mika","The High Lantern's behind your lens. Ada says she never authorised moving it. Show me the permit that came with the crate."), new StoryLine("Aster","The seal was hers. I was testing the north beam. Wait—the pilot's signal. Three flashes.")),
                        new WorldComicPage("Three flashes", new StoryLine("Mika","What does that mean? Can they reach the next landing?"), new StoryLine("Aster","No. The glider's circling beyond the lens. My chart shows a lamp at the turn, but Nora says it's dark.")),
                        new WorldComicPage("A gap in the chart", new StoryLine("Mika","Then leave the High Lantern where it is. Which relays can light that turn?"), new StoryLine("Aster","The lower chain. I have its full chart. The copy I sent Ben doesn't show it.")),
                        new WorldComicPage("Keep the beam steady", new StoryLine("Aster","I'll bring it. And the permit. Just don't move the source while the pilot's following its beam."), new StoryLine("Mika","Put both beside the receiver. You watch the source; I'll light the lower chain. Nora, stay with the pilot.")));
                default: throw new ArgumentOutOfRangeException(nameof(world));
            }
        }
    }
}
