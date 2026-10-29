using System;
namespace Cascade.Content
{
    public readonly struct StoryLine
    {
        public readonly string Speaker, Text, Goal;
        public StoryLine(string speaker,string text,string goal=null){Speaker=speaker;Text=text;Goal=goal;}
    }
    public sealed class StoryConversation
    {
        public int Receipt { get; }
        public StoryLine[] Lines { get; }
        public StoryConversation(int receipt,params StoryLine[] lines){Receipt=receipt;Lines=lines;}
    }
    // Authored story beats. Reading, skipping and their animations never send puzzle commands.
    public static partial class StoryConversations
    {
        static readonly string[] Residents={
            "Bram",
            "Pip",
            "Rue",
            "Sora",
            "Neri",
            "Toma",
            "Lumi",
            "Gran",
            "Aster"
        };
        static readonly string[] Goals={
            "Light the first lower lamp",
            "Restore the wind-gate lamp",
            "Restore the sky-lift relay",
            "Light the market landings",
            "Open the lit route to Ben",
            "Restore the mill's lower lamp",
            "Restore water to the far tap",
            "Light the wells; record the time",
            "Light the boat repair landing",
            "Take the crate evidence to Rose",
            "Light the lamp by the old tree",
            "Light and check the orchard bridge",
            "Open the harvest-cart path",
            "Warm the seed house; copy the loan",
            "Open the route to Nora",
            "Recover the mail at a lit landing",
            "Light the sorting room and check the order",
            "Make the mail-cart route safe",
            "Send Nora's letter to Clara",
            "Take the receipt to the lagoon",
            "Light the pier below the tide",
            "Light the lagoon; check the log",
            "Light the landing and clear the low beam",
            "Prepare the spare travelling lamp",
            "Take the tide record to Oscar",
            "Free the trapped key-register page",
            "Restore the archive shelf lift",
            "Recover the service permit record",
            "Save the north-route petitions",
            "Prepare the snow-route copy",
            "Light the safe snow fork",
            "Light the shelter and take a rest",
            "Check the frozen arch's far anchor",
            "Light the shelter supply path",
            "Signal Ada from the beacon road",
            "Restore Ada's last bridge lamp",
            "Find the receipt for Felix's permit",
            "Check the delivery order with Ada",
            "Trace the observatory beam",
            "Prepare the beacon to receive its light",
            "Restore a steady relay signal",
            "Light the blind turn below the lens",
            "Restore the complete chart's relays",
            "Guide the glider through the dark turn",
            "Return the High Lantern; open the north route"
        };
        static readonly string[] IslandLines={
            "First lamp lit. I'll sign the anchor. I've checked it often enough to know it.",
            "The first lamp... Could we stay a moment? I want to see who notices.",
            "The gate holds against the wind. You checked the far latch before I asked.",
            "I'm still scared. But now I know what to look at.",
            "The lift works. Ben's wheel stopped at the same time as this relay. He may have seen more than I did.",
            "We'll ask him when we get the market route open.",
            "The market lamp is steady. The baker sent bread. He says I'm to eat it warm, without measuring it first.",
            "Save Ben a piece. You eat yours now. I'll hold the rope.",
            "The gate mark is measured. A thin brass edge made it, not my broad steel chisel.",
            "That clears your chisel. Ben heard the crate at the mill; let's ask him about that night.",
            "The wheel turns without a knock. My brother asked who broke the pipe. I told him.",
            "And? Is he coming to test it with us?",
            "His tap runs. He made me wait there while he filled the bucket himself.",
            "Is it full? Good. I've been listening to that bucket scrape the bottom all morning.",
            "The wells are lit. This slip fell off the crate. Its time matches the clock; my brother saw it before the rain.",
            "So the crate came through while the sky was clear. The storm came after.",
            "The loaded post boat stays dry. Nora can take the records, and my letter to my brother.",
            "Across the yard? I think you can manage that delivery. I'll watch the boat.",
            "The orchard gate is open. Here are the dust and Rose's seal, wrapped as you asked.",
            "Thanks. I'll take the loading slip too. Nora can check it against her flight book.",
            "The old tree has light. We missed the roots. Tomorrow I'll check the soil beside the rope hasn't slipped.",
            "Look, something's moving by the nest. The birds noticed before we did.",
            "The bridge is lit. I'll return Walter's chisel myself; he shouldn't have to come looking for it.",
            "Ask him to check the bridge while he's here. He knows which joint the storm damaged.",
            "Walter checked the track. Ben sent his cart for the jars, with water for the seedlings on the return trip.",
            "Then leave room for the water cans. I'll help load the jars before we go.",
            "Here: felt for Felix's new lens. Nora took the empty crate to Ada first, then collected a sealed load there.",
            "So you supplied the packing, not the load. I'll ask Nora what the delivery order said.",
            "The post gate is open. Three seeds for Ada, and a signed copy of the crate loan for you.",
            "Seeds in this pocket, papers in the other. I'll give Ada your planting instructions with them.",
            "Every letter recovered. One had no address; its owner came to ask before we could guess.",
            "Then it can go in the right bag. Let's get the next load ready before the wind picks up.",
            "My delivery copy. The signed work permit stayed with the load. I checked Ada's seal and never asked why the relays had to stay on.",
            "Keep your copy. I'll take a tracing to Ada; she can tell us what she approved.",
            "The cart crossed. I walked beside it. It's a much longer bridge when you can't flap over the gap.",
            "Will you try it once more with the mail aboard? I'll watch the far wheel.",
            "It's gone. I wrote that I was scared to visit after her mother's boat vanished. Not another page about the weather.",
            "I'll go with you. We can help with the boats while Clara reads it.",
            "The lagoon gate is ready. My receipt records the northbound crate before the storm.",
            "Keep the flight book with you. We'll ask Clara to check that entry at the pier.",
            "The pier holds. I asked Nora to stay. She put the bag down before I finished asking.",
            "Then I'll light the bench lamp too. You can read Nora's letter without holding mine.",
            "The channel lamps are steady. I've copied the sighting for Ada; the original stays in my log.",
            "You saw light through the felt. If that was the High Lantern, someone moved it while it was burning.",
            "The little sail went straight through. I kept waiting for it to catch where Mother's always did.",
            "Mark that clearance on your chart. The next boat may have a taller sail.",
            "Full tank, clean lens. And take these surveys for Ada. Felix measured the north channel from my boat.",
            "I'll pack them with Nora's order. I'd like to hear why Ada kept postponing this route.",
            "The archive path is lit. Tell Oscar to look under the north petitions. He'll know the shelf.",
            "I'll ask him for those too. Nora knows about supper. She's still here.",
            "The page is safe. Felix signed for the key before the storm. There's no return entry yet.",
            "Thank you. The original can stay here. I'll take the copy and Nora's order to Ada.",
            "The shelves move again. Spare key tied to the register. No exceptionally safe hiding places.",
            "I'll borrow that second pencil for the map. This one has nearly had it.",
            "No equipment list here. Ada has the receipt for the permit; it will tell us where to find the conditions.",
            "Take the date too. Then she can check the right permit, not rely on her memory.",
            "The petitions are dry. Clara asked first. Felix drew the route. Both replies say Deferred.",
            "Ada needs to read these again. Clara's boats are still taking that route.",
            "The snow map is ready. I signed beside the blank turn. Lumi can replace my question with a survey.",
            "I'll wrap the copies to keep them dry. Lumi will check the turn, and we'll bring you the map with her correction.",
            "The new path is lit. I've crossed the old one out and marked the way round for Oscar.",
            "Sign the correction. I'll bring it back to Oscar with the map.",
            "Warm paws. I can fasten that buckle now. We'll stop here on the next trip too.",
            "How long to the beacon from here? Including a stop on the way.",
            "The arch holds. You planned the turned landing without asking me to name the order.",
            "I still need you on that far anchor. Ready for the next one?",
            "The spare blanket is dry in the shelter. Its lamp can be seen from the lower path.",
            "I'll write Ben where it is. And that we're using his first blanket.",
            "Ada's answering. The bridge lights reach her window now; she can guide us across.",
            "Come with me. You know the pass; she'll want to hear which paths need closing.",
            "The bridge is safe. Come here. You waited for both anchors, just as I taught you.",
            "Your hands are still on the rail. Good. Now I can come and put this bag down.",
            "Yes. Felix collected both; he signed here. He should still have them at the observatory.",
            "Then we'll compare his copy with Nora's delivery order. Someone changed the instructions between them.",
            "Yes, a lens sleeve could leave that mark. It tells us where the load passed, not who wrote the order.",
            "The crate went to the observatory. Let's check whether its beam is really our missing light.",
            "The signal catches the observatory beam. The High Lantern is still burning up there.",
            "Signal the others before I go. Nora needs to know where that crate ended up.",
            "The beacon receivers are ready. Take my keeper's cord, Rin. I will keep watch here, not climb.",
            "Keep the other end. If I stop halfway, wait for my signal. I'll start again.",
            "The first relays are steady. Nora says the pilot can see the landing now, but the turn is still dark.",
            "Keep the beam on the landing. We'll open the rest of the route before they try to cross.",
            "Nora sees the new lamp. The pilot can move to the next landing; the final turn still needs the lower chain.",
            "Tell them to wait there. I'll connect the last relays and check that permit.",
            "The next relay is ready. Here's the permit. Both sheets, as you asked.",
            "Ada allowed a spare lens. It says the High Lantern must stay at the beacon. Who wrote Nora's order?",
            "The glider cleared the turn. Nora signalled; Clara saw it from the water. The lower chain holds.",
            "Now we can lower the High Lantern. Nora, stay with the pilot. Felix, on my count.",
            "Ada has the High Lantern home. And my key. Walter's waiting at the first anchor. We start the repairs at dawn.",
            "I'll give Gran her hand lamp back. I'll keep Clara's. I'll need it on the morning rounds."
        };
        static readonly string[] BanterLines={
            "Don't watch the drop. Watch the joint. If I tap twice, stop lowering.",
            "Two taps. I can remember that. Will you keep talking until I reach the lamp?",
            "I'm reaching for the fast lever again. Red thread. Right. Hands off.",
            "Keep one paw on the brake. We can afford another minute; not another broken pipe.",
            "You haven't asked why I keep the crooked tree. Most people ask that first.",
            "You showed me the roots holding up the terrace. We can take the rope around that bent branch.",
            "I could fly to the next desk and be back before this basket lands.",
            "I can't see your signal from there. Stay until the basket lands.",
            "The reflection is shaking. The real lamp isn't. Look at the bracket, not the water.",
            "Like Walter's joint. I know what to watch now.",
            "That shelf holds last winter's maps. The pass changed in the storm; go with Lumi, our guide.",
            "Mark the uncertain turn now. I'll ask her to check it before we set foot there.",
            "I can do this descent with my eyes closed. That's what I tell nervous travellers.",
            "Keep them open for me. I want to know what the far anchor's doing.",
            "I used to mend that knot for you before you'd finished trying.",
            "Let me finish this one. Then check it. I do want you to check.",
            "My drawing shows this ledge lit. I never came down to check it from below.",
            "Nora, can you see the far turn? Tell him where the light stops.",
        };
        static readonly string[] OpeningLines={
            "First anchor. I'll hold the line until you tell me to lower it. Not a moment sooner.",
            "Wait. Let me check the receiver once more. You can see the knot from there, can't you?",
            "The wind gate feeds a lamp below its hinges. Mind the changing row when you descend.",
            "Then I'll check the order after the gust. Is the far anchor secure?",
            "The lift relay has burnt out. Here's the workshop diagram; it shows where the spare contact goes.",
            "Can you replace it while I light the service landing?",
            "The market lamp won't catch until all its lower receivers are ready.",
            "Does the baker have a hand lamp? We might be a while down here.",
            "There's a score on the gate, like a chisel slipped. One of mine is missing.",
            "Could it be from a tool? Let's keep the mark clear until we find yours.",
            "Slow lever this time. I put a red thread on the fast one so I can't pretend I forgot.",
            "I'll take the lower receiver. Tell me if the wheel starts knocking again.",
            "The pipe reaches the houses, but its lamp is dark. My brother's tap is at the far end.",
            "Then that's where we test the repair. Not just here, where it's easy to see.",
            "The well clock stopped when the crate hit. I haven't touched its hands.",
            "Leave them. We'll light the well and compare that time with Nora's book.",
            "The mail boat has a split seam. I can reach it from the lower landing, but its lamp is out.",
            "I'll light the landing. Bring your tools; the letters can wait somewhere dry.",
            "Copper dust in the wheel. Rose's seal on the lid. I nearly threw both away as rubbish.",
            "Wrap them separately. Once this lamp is lit, we can ask Rose about the crate.",
            "The oldest tree gets the first lamp. Its roots hold the whole terrace.",
            "There's room by the outer rope. More lowering, less arguing with roots.",
            "Walter's missing chisel is here. I borrowed it for a graft and forgot to tell him.",
            "He can collect it when he checks the bridge. Which joint needs lighting first?",
            "Ben's jars are ready. The big ones are still on the top shelf. Couldn't find room for them down here.",
            "We can open the cart path today. Let Ben's brother choose the first jar.",
            "The seed-house lamp warms the young roots. These won't last another cold night.",
            "Then the book can wait. Show me where to lower the pieces.",
            "I know Nora well enough to lend her a crate. I didn't think to ask what Ada was sending.",
            "She may have thought you knew. Let's take the packing entry so we can check it together.",
            "I can catch the envelopes. The heavy bag needs a lit landing below.",
            "I'll take the lower rope. Anything that falls, I'll catch from here.",
            "The delivery order is in the sorting room. Its lamp went out; I nearly fell over a parcel going in.",
            "I'll light it from below. Wait by the door; we can look for the order together.",
            "I flew over the broken mail bridge. It took me a week to notice the carts couldn't follow.",
            "You can watch the far anchor from the air. I'll test the lower relay with the load.",
            "My letter to Clara is on top of this basket. If I leave it here, I'll take it home again.",
            "I'll stay with you. Light the basket landing first, then we'll send it together.",
            "The northbound receipt has a tide mark. Clara keeps those times better than any clock.",
            "We'll open the lagoon gate, then take both records to her.",
            "The tide is rising towards the lower landing. I can hold the boat here while you light the pier lamp.",
            "Read the tide to me. I'll read the receiver to you. We stop if either changes.",
            "The reflected lamps look close. Their real receivers are on separate ledges.",
            "We'll follow the ledges, not the reflections. Keep Nora's receipt dry for me.",
            "The storm knocked that beam down. Boats can't pass with their sails up. I'll lift it once the landing is lit.",
            "I'll light both ends. Ask Nora to keep the boats back until you've secured it.",
            "Your hand lamp is almost empty. I've a spare upstairs. Light the stair lamp and I'll fetch it.",
            "Thanks. I'll keep Ada's to return to her. Is there enough oil in the spare for the pass?",
            "Oscar keeps the maintenance records. Tell him the tide and the clock agree.",
            "I'll bring him the copies. The originals stay with you and Nora.",
            "The storm jammed the lift with the register inside. Light its relay; I'll free the door.",
            "Is the page caught in the hinge? Don't start the lift until you've freed it.",
            "The shelf lift has its own little key. I put it somewhere exceptionally safe.",
            "Your waistcoat pocket, perhaps? That's where you kept checking when we came in.",
            "The permit number is on this shelf. Its lamp is out; I won't take a candle near the papers.",
            "I'll reach the lamp from below. Find the entry for the day Nora collected the crate.",
            "Water is reaching the north-route petitions. I shelved them under Unfinished Matters.",
            "Let's dry them. Then put them where Ada will have to read them again.",
            "Lumi's route note is behind the last cabinet. I'll copy the blank turn as blank.",
            "Does Lumi know that turn? We'll check it with her and bring you the correction.",
            "Count the buried lamps, not my footprints. Snow can fill a footprint in a breath.",
            "You count. I'll relight them. Snow won't cover that signal so easily.",
            "The shelter is below us. I usually take travellers straight past it.",
            "Today we'll stop. You need warm paws to secure my rope.",
            "The arch turns the row before the landing. I used to know its order by heart.",
            "The ice has moved it. We'll read this receiver as it is now.",
            "Ben's second blanket is on the supply landing. Its lamp is buried in snow; no one can see the steps.",
            "Clear the lamp while I reach its receiver. Then we can carry the blanket in without falling down the steps.",
            "The beacon is on the left fork. The right one follows the beam to the observatory.",
            "Ada first. We have her permit number, but we still haven't heard her side.",
            "My last bridge lamp. I used to check this rope every dawn. Now I can't reach the first rung.",
            "Tell me what to look for. I can reach the lower anchor from here.",
            "The service file is in the lift, beside the oil bills. Pull the green ribbon; the other bundle is a year's worth of accounts.",
            "The green one. I'll steady the lift before you reach in.",
            "Put Nora's order here. The number and seal are mine. I never saw this delivery copy.",
            "It says to move a source with the relays connected. Is that part of your trial?",
            "We can test the north beam with these relays. But we cannot light the empty beacon yet.",
            "Then ready every receiver. Returning the lantern won't help if its route is broken.",
            "I kept saying the north route was too risky. Clara kept sending boats. I knew that.",
            "We'll show her the new plan before using it. She knows that water better than either of us.",
            "The timing wheel stopped when the High Lantern was moved. The lamps below can't hold a steady signal until I reset it.",
            "I'll light the service landing. Reset the wheel, then we'll test the next relay.",
            "The far lens is across this bridge. I measured the beam from my desk and missed the dead patch.",
            "We need Nora above that turn. I'll ask her to signal when she can see the landing.",
            "The full chart shows a direct feed from the beacon to this lens. That's why the lower chain failed when the source moved.",
            "You left that connection out of Ben's copy. He couldn't have found the cause from what you sent.",
            "I did. I took it. Used Ada's permit number and changed the load. I meant to light the north for one night, then bring it back.",
            "And kept quiet when the lamps failed. We'll come back to that. Hold the source; Nora's bringing the pilot through.",
            "The north chain is holding. I was afraid Ada would shut the trial down if I told her. So I let Ben blame himself.",
            "Tell him yourself. Send the full chart, then take Walter through every damaged relay. Ada's waiting for the Lantern."
        };
        static readonly string[] AsideLines={
            "I'll hold this end. You tell me if the rope starts rubbing against the frame.",
            "And you tell me if I start going too fast. I'm watching the receiver so hard I forget to look up.",
            "My brother put red thread on his bucket handle. Says he's checking before he pulls too.",
            "Has he put the bucket down yet? I wanted to ask him to help with the boat.",
            "Those three seeds are still in your pocket. Ada used to plant something after every hard winter.",
            "I'll find her three pots. There's bound to be room on that windowsill.",
            "I left the weather out of Clara's letter. Now there's rather a lot of blank paper.",
            "What did you want to tell her before you started writing about the weather?",
            "Nora asked what time supper is. Not which delivery could fit before it. Just what time.",
            "I'll check the bench lamp before we leave. You'll want it when supper runs late.",
            "Two north petitions, two postponements. I filed them so neatly I forgot they were unanswered.",
            "I'll take copies with her replies. I want to ask what she was waiting for all that time.",
            "Put a sign to the shelter beside the path. You can't see the door from that side in the snow.",
            "Hang it inside, away from the snow. I'll clear the bottom step so the door opens all the way.",
            "Clara's requests are on my table. This time I'll answer with a route and a date for checking it.",
            "Leave room for Clara's notes. There's a turn on that plan you can't see from the beacon.",
            "Clara sent her channel marks. I drew the trial beam above them, but never checked the turn from a boat.",
            "Ask her to check with Nora now. They need the same turn lit, one from the water, one from the air."
        };
        public static string Resident(int world)=>Residents[Math.Max(0,Math.Min(JourneyStory.WorldCount-1,world))];
        public static string Task(int stop)=>Goals[JourneyStory.Zone(stop)*5+JourneyStory.IslandFor(stop)];
        public static StoryConversation BeforeLevel(int stop)=>Visit(stop,JourneyStory.PageFor(JourneyStory.WorldFirst(JourneyStory.Zone(stop))));
        public static StoryConversation Visit(int stop,int receipt=-1,bool alreadyMet=false)
        {
            int world=JourneyStory.Zone(stop),offset=(world*5+JourneyStory.IslandFor(stop))*2;
            return new StoryConversation(receipt,new StoryLine(Resident(world),OpeningLines[offset]),
                new StoryLine("Mika",OpeningLines[offset+1],Task(stop)));
        }

        public static StoryConversation Island(int world,int island)
        {
            if(world<0||world>=JourneyStory.WorldCount||island<0||island>=5)throw new ArgumentOutOfRangeException();
            int offset=(world*5+island)*2;
            return new StoryConversation(2000+world*10+island,
                new StoryLine(Resident(world),IslandLines[offset]),new StoryLine("Mika",IslandLines[offset+1]));
        }

        public static StoryConversation IslandOpening(int world,int island)
        {
            if(world<0||world>=JourneyStory.WorldCount||island<0||island>=5)throw new ArgumentOutOfRangeException();
            int offset=(world*5+island)*2;
            return new StoryConversation(8000+world*10+island,
                new StoryLine(Resident(world),OpeningLines[offset]),new StoryLine("Mika",OpeningLines[offset+1],Goals[world*5+island]));
        }
        public static bool HasLevelAside(int stop)
        {
            if(stop<0||stop>=JourneyStory.TotalLevels)return false;
            return HasIslandProgress(stop)||stop==JourneyStory.IslandFirst(JourneyStory.Zone(stop),3)+1;
        }
        public static StoryConversation LevelAside(int stop)
        {
            if(!HasLevelAside(stop))throw new ArgumentOutOfRangeException(nameof(stop));
            if(HasIslandProgress(stop))return IslandProgress(stop);
            int world=JourneyStory.Zone(stop),offset=world*2;
            return new StoryConversation(7000+int.Parse(JourneyStory.NodeIds[stop].Substring(1))-1,new StoryLine(Resident(world),AsideLines[offset]),new StoryLine("Mika",AsideLines[offset+1]));
        }
        public static bool HasCheckpoint(int stop,int completedStage)=>completedStage==1&&stop>=1&&stop<JourneyStory.TotalLevels
            &&stop==JourneyStory.WorldFirst(JourneyStory.Zone(stop))+1;
        public static StoryConversation Checkpoint(int stop)
        {
            int world=JourneyStory.Zone(stop),offset=world*2;
            return new StoryConversation(4000+int.Parse(JourneyStory.NodeIds[stop].Substring(1))-1,new StoryLine(Resident(world),BanterLines[offset]),new StoryLine("Mika",BanterLines[offset+1]));
        }
    }
}
