using System;

namespace Cascade.Content
{
    public static partial class StoryConversations
    {
        // One short exchange per island, before its sixth level and before its payoff.
        static readonly string[] IslandProgressLines={
            "The far anchor pulled loose, not the boards. My first repair missed it.",
            "Then we secure that end before anyone crosses. I'll hold the load here.",
            "The gate opens, but the latch won't hold. One gust could slam it shut.",
            "Let's test the latch before we send the lift through.",
            "The empty lift runs. I haven't trusted it with a passenger since the storm.",
            "Send the tools first. If they arrive safely, I'll take the next trip.",
            "The baker won't open after dark. She can't see the gap by her door.",
            "The next lamp goes there, then. Bread can wait; a safe doorway can't.",
            "Pip asked for my tools. I've packed them every morning, then stayed home.",
            "Bring them with us today. You can tell him yourself what needs fixing.",
            "The wheel turns, but that knock means a tooth is still out of place.",
            "Stop it while we fit the last pieces. Faster won't make them fit.",
            "Water reaches this joint, then leaks away. The village still gets nothing.",
            "I'll follow the pipe to the last house. We finish when their tap runs.",
            "My brother can lower the bucket. The handle slips when he pulls it back.",
            "Let's fix the handle for his paws, not yours. Then let him test it.",
            "The boat floats empty. With the mail aboard, water comes through this seam.",
            "Keep the letters ashore. We'll test it with the toolboxes first.",
            "Rue sent seeds when our mill failed. I still haven't taken her any water.",
            "Fill a can. Once this crossing is ready, we'll carry it to her together.",
            "That nest has chicks. Their parents won't leave while the branches are dark.",
            "We'll light their tree first. The harvest can wait until they can feed.",
            "The broken support rests on a living root. Pull it out and both may fall.",
            "We'll brace the bridge, then free the root. We don't have to cut it.",
            "These jars were promised to the mill. I've been rationing them here instead.",
            "Once the cart can pass, send the promised crate first. I'll help load it.",
            "The seed trays are dry, but the night air gets in beneath the door.",
            "Let's seal the gap. I want these seeds to survive more than this one storm.",
            "Sora used this path every week. The unopened mail is still on my table.",
            "Keep it for her. We'll reopen the path so she can collect it herself.",
            "Two envelopes lost their addresses. I can't just choose a house for them.",
            "Bring them to the post desk. Someone may recognise the handwriting.",
            "Your letter can leave with the next bag. Are you sure it's finished?",
            "I added everyone's names. Gran should know who is helping me reach her.",
            "I can fly over the gap. The full mail cart can't. I forgot who uses this bridge.",
            "Then test it with the cart. We'll leave a route everyone can use.",
            "I keep checking the stamp on my apology. It's easier than sending it.",
            "Put it in the basket yourself. I'll stay until it leaves.",
            "Neri is the friend I wrote to. I don't know if she'll want to see me.",
            "We can ask. Opening the path doesn't mean she owes us an answer.",
            "Sora's letter arrived. I haven't answered. We still need this pier repaired.",
            "We'll repair it. You can decide about the letter when you're ready.",
            "From shore, those lamps look alike. From a boat, one marks a dangerous turn.",
            "Show me from the boat, then. Hold it steady while I step in.",
            "The gate catches the smallest sail. Mother always lowered it before passing.",
            "We'll leave enough room for the sail. The next sailor may not know that trick.",
            "This lamp was Mother's. I'd rather it lit your road than stayed on my shelf.",
            "I'll bring it back. And I'll tell you where its light took us.",
            "Tell Sora there's a place at my table. I still want to hear her say it aloud.",
            "I'll tell her before we leave for the archive. This message shouldn't wait.",
            "The door swelled in the rain. Forcing it will tear the map caught underneath.",
            "Lift this corner with me. We can free the paper before opening the door.",
            "I tied the shelves together during the storm. Now I can't reach the roof ladder.",
            "Brace one before moving the other. Your books can stay where they are.",
            "My teacher left this turn blank. I've been afraid to admit the map is incomplete.",
            "Mark the gap on my copy. Lumi can check it; a guess could send us off the ridge.",
            "The leak is above the map cabinet. Saving one page won't keep the rest dry.",
            "I'll move the cabinet. You mend the roof before we uncover the maps.",
            "Gran left this note before the storm. It tells us how to approach the beacon.",
            "Then it's directions, not news. We'll still need to find out if she's safe.",
            "Your map leaves this fork blank. The old path ends at a fallen ledge now.",
            "Let's mark the safe fork for Toma. Nobody should follow the old line blindly.",
            "I said we could reach the beacon tonight. I can't feel my paws now.",
            "Then we stop here. I'll light the shelter; you take Pip's blanket.",
            "The near edge is sound. I can't see where the far one lands through this snow.",
            "Let's secure the far landing before we cross. I can wait for a clear view.",
            "Pip packed two blankets. I thought he'd miscounted. One is for the next traveller.",
            "We'll leave it in the shelter, wrapped dry. He thought further ahead than us.",
            "That window flashed twice. That's a keeper's signal: someone is there.",
            "Gran taught me the reply. Hold my lantern high enough for her to see.",
            "I saw your reply from the window. I nearly came out onto the broken bridge.",
            "Stay by the rail. I'll come to you as soon as this side holds.",
            "The lift jammed with the lens aboard. I couldn't bring it down alone.",
            "We won't pull against the jam. I'll unload it in the order you show me.",
            "I left the upper lamps dark to save oil. I was waiting for you to come home.",
            "I'm here now. Let's light the stairs for both of us.",
            "Two small flames won't make the beacon brighter unless the lens is aligned.",
            "You watch the beam. I'll adjust the frame until it reaches Bram's island.",
            "A light answered from the mill. They're waiting to see if we'll stay lit.",
            "Then I'll learn to tend it, too. You shouldn't have to keep watch alone.",
            "The dial agrees with the beacon. My chart was using the old starting mark.",
            "Let's correct that before checking the stars. The same error could shift every route.",
            "I watched from the doorway after the bridge broke. It hid half the sky.",
            "We'll reach the telescope. No wonder you couldn't check those bearings.",
            "This torn corner holds the turn after the ridge. Without it, a pilot must guess.",
            "We'll mark that route closed until it's checked. The others can still open.",
            "That glider follows the first lamps, then circles back. Our next turn is still dark.",
            "Light the turn before signalling him through. He needs a route, not a promise.",
            "Sora checked our route from the air. She found a lamp hidden behind the tower.",
            "We'll move it, then ask her to fly once more. This time we check together.",
        };
        public static bool HasIslandProgress(int stop)
        {
            if(stop<0||stop>=JourneyStory.TotalLevels)return false;
            return stop==JourneyStory.IslandFirst(JourneyStory.Zone(stop),JourneyStory.IslandFor(stop))+5;
        }
        public static StoryConversation IslandProgress(int stop)
        {
            if(!HasIslandProgress(stop))throw new ArgumentOutOfRangeException(nameof(stop));
            int world=JourneyStory.Zone(stop),offset=(world*5+JourneyStory.IslandFor(stop))*2;
            return new StoryConversation(7000+int.Parse(JourneyStory.NodeIds[stop].Substring(1))-1,
                new StoryLine(Resident(world),IslandProgressLines[offset]),new StoryLine("Mika",IslandProgressLines[offset+1]));
        }
    }
}
