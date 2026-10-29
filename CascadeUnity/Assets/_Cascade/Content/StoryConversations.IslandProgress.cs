using System;

namespace Cascade.Content
{
    public static partial class StoryConversations
    {
        // One short exchange per island, before its sixth level and before its payoff.
        static readonly string[] IslandProgressLines={
            "This anchor holds. I can see the far one from here; the rope has slipped out of its guide.",
            "Keep watching it. I'll stop above the guide so you can put the rope back.",
            "A gust changes the row on this landing. The receiver stays where it was.",
            "Right. Turn first, then descend. Give me a moment to work it out.",
            "The new contact fits. But this old one burnt out before any rain could reach it.",
            "So the storm broke the bridge, but something else stopped the lift?",
            "The baker won't cross until I test the loaded cart. An empty one proves little.",
            "With the flour on it? I'll stay where I can see both anchors.",
            "The gate mark is narrow. My missing chisel is twice that width.",
            "Yours is steel, too. Where did the brass on the gate come from?",
            "That knock is the last bad tooth. I'm stopping the wheel. Yes, stopping it.",
            "No hurry. I've got the row. Find the tooth before it starts turning again.",
            "Water reaches us, then leaks away. My brother still has a dry tap.",
            "We'll follow the pipe. He can come and show us where it stops working.",
            "My brother remembers that knock too. It drove him indoors before the rain. So the crate hit first.",
            "And you pulled the lever afterwards. The pipe was your accident; the stopped wheel wasn't.",
            "The boat floats empty. The mail makes this seam dip under the water.",
            "Unload the mail. I'll keep the landing lit while you patch the seam.",
            "My brother tied red thread round the tap. Says he'll take it off after a whole day without a leak.",
            "Fair enough. Tell him where the new valve is, in case it needs tightening.",
            "The roots block the short descent. I won't cut a living root just to save a few minutes.",
            "Outer rope it is. Can you watch the branch above my head?",
            "Walter's agreed to come. The chisel is wrapped and waiting for him beside the bridge.",
            "Then we'll light the last joint before he gets here. He'll need to see the crack from both sides.",
            "Ben's jars are packed. Too heavy to carry across that broken section; we'll need the cart.",
            "I'll light the rest of the track. Keep the jars here until Walter has checked the joint.",
            "The packing entry says Lens padding. Deliver empty crate to beacon. It doesn't list what went inside.",
            "Nora should have the list for the loaded crate. I'll copy this entry for her.",
            "The birds found the lit tree. I was about to harvest every fruit for the market.",
            "That branch has enough for the nest and us. Leave the high ones.",
            "An envelope with no address. I could guess; I recognise the paper.",
            "Put it on the collection shelf. We can ask at the next landing.",
            "Move the source. Leave the relays connected. Odd wording for Ada.",
            "Ada taught me to disconnect them before moving a source. Is this the original, or your copy?",
            "The cart catches at the far end. I never saw that part from the air.",
            "Stand beside the cart. I'll light that edge so you can see where the wheel catches.",
            "I keep checking the stamp. It's easier than letting Clara read the letter.",
            "The stamp's straight. Shall I keep you company until they collect the basket?",
            "The receipt time and Ben's clock agree. The crate flew before the storm arrived.",
            "Clara keeps a record of boats. Let's ask if she saw that flight beside the north channel.",
            "Nora offered to hold the boat. I said yes. That's enough talking for now.",
            "I'll help with the boat. You can both stay on the pier.",
            "My log: the lamps went out, then a glowing crate passed. The source was still lit.",
            "Mark that entry for Ada. If it was the High Lantern, the flame survived the move.",
            "The smallest sail catches here. My mother always ducked it under the beam.",
            "Where did she lower it? We can leave enough clearance for the next boat.",
            "Felix asked about the last north lamp every spring. I thought he'd given up.",
            "Did he leave his measurements here? I'd like to see where he meant to put the lamps.",
            "Nora can come for supper. I asked her to leave the mailbag at home.",
            "Shall I tell her now? She's still on the pier.",
            "There's the trapped page. Lift the door, don't pull the paper.",
            "The edge is free now. Is the writing legible?",
            "The key was in my pocket. Exceptionally safe. Especially from me.",
            "Tie it to the register? That's where you looked first.",
            "Here it is: north lens trial. Ada's permit number, Felix's name. The work itself was approved.",
            "But Nora carried a burning source. Does it say he could move that too?",
            "Clara asked for lamps. Felix supplied the lens plan. Ada wrote Deferred on both.",
            "Take the dates down too. I want to ask Ada what happened between the two petitions.",
            "I haven't surveyed the pass since the storm. That blank marks a turn I can't vouch for.",
            "We'll check the turn with Lumi. If the old path has gone, that's what we'll put on the map.",
            "The old fork ends at a fallen ledge. My memory would have taken us straight over it.",
            "Mark it closed on Oscar's map. We'll use the new fork.",
            "I can't feel this buckle. I should have said so before touching your rope.",
            "Give me the rope and get the blanket. We're secure here. We'll wait.",
            "The far anchor is sound. I checked it from below this time, not from the ridge.",
            "Keep watching it. I'll allow for the turn and bring the row down.",
            "Spare blanket above the fuel. Dry up here. It's snowing outside, but the stove's warm now.",
            "I'll mark the shelter. Ben will want to know where his parcel ended up.",
            "A two-flash reply. Ada saw you. She's staying beside the window.",
            "Hold the hand lamp higher. One more connection, then we cross.",
            "I nearly stepped onto the broken bridge when I saw your light. Old habit.",
            "Stay by the rail, Gran. I'll give the signal when both ends are safe.",
            "North lens trial, key issued to Felix. He signed for two sheets: the permit and its conditions.",
            "Nora only saw the delivery copy. Did those two sheets go with the equipment?",
            "No. Disconnecting the relays is the first safety check. Whoever wrote this bypassed it.",
            "Walter found brass at the gate. Could it have come from the lens equipment?",
            "A thin beam from the observatory. The signal lens can catch it if we align this row.",
            "Set the lens there. I'll bring the row to that receiver.",
            "I feared opening the route. Clara sailed it unlit anyway. I should have asked what she needed.",
            "Ask her about the channel too. There's a low beam her mother's sail used to catch.",
            "The signal's steady. Ben described the same failure at the mill. His lever didn't cause it.",
            "Write that on his chart. He still thinks he broke the whole chain.",
            "Nora sees the dead patch. My north beam stops one turn too soon.",
            "Lower relay there. Nora checks it again before we move the source.",
            "I knew after his letter. I sent enough to fix the wheel. I should have sent this too.",
            "We'll need an explanation. Lay the permit beside the chart; I can read it while you hold the source.",
            "The pilot's at the turn. I kept the relays on for the north beam. It burnt out the other branches.",
            "Then we check every branch before moving it again. This receiver first. Watch Nora for the signal.",
            "Nora reports a clear route. Clara confirms the water lamps. We can lower the lantern.",
            "On my signal. Ada's at the receiver. Nora, check the far light once more.",
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
