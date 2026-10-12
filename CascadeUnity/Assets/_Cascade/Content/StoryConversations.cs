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
            "Light the first bridge",
            "Open the wind gate",
            "Mend the sky lift",
            "Light the cloud market",
            "Reach Pip's mill",
            "Start the mill wheel",
            "Bring water to the village",
            "Fill the little wells",
            "Repair the post boat",
            "Reach Rue's orchard",
            "Light the lantern trees",
            "Mend the branch bridge",
            "Open the orchard path",
            "Light the seed house",
            "Reach the sky post",
            "Find the lost letters",
            "Send Gran's letter",
            "Mend the mail bridge",
            "Lift the mail basket",
            "Reach the glass lagoon",
            "Mend the little pier",
            "Wake the pond lamps",
            "Open the water gate",
            "Carry the pond light",
            "Find Toma's archive",
            "Open the leaf door",
            "Untie the old shelves",
            "Find the storm map",
            "Keep the page dry",
            "Read Gran's note",
            "Light the snow steps",
            "Fix the warm shelter",
            "Cross the frozen arch",
            "Carry Pip's blanket",
            "Find the beacon road",
            "Open the last bridge",
            "Mend the lantern lift",
            "Light the beacon stairs",
            "Lift the sky lantern",
            "Bring the light home",
            "Wake the moon dial",
            "Mend the telescope bridge",
            "Gather the scattered star charts",
            "Relight the constellations",
            "Open the night travellers' route"
        };
        static readonly string[] IslandLines={
            "The first bridge is lit. I can see my workshop again!",
            "You left your name on it! I'll tell Gran her bridge builder is coming for tea.",
            "The wind gate is open. Hold your scarf, Mika.",
            "I am holding it! Now we can reach the sky lift.",
            "That lift has not moved since the storm. You did it.",
            "We did it. Your new boards held me up.",
            "The market lamps are back. I missed the smell of warm bread.",
            "Let us save a loaf for Pip. He has been waiting.",
            "I crossed the whole bridge. First time since the storm. The bread got here warm.",
            "Thank you, Bram. When the beacon shines, you will see it from here.",
            "Listen! The wheel is turning again.",
            "Your brother heard it first. He's run off with his bucket. Let's see if the water reaches him.",
            "Water is on its way to every little house.",
            "Your brother put his bucket down. He can use both paws to wave now.",
            "The wells are full. Even my smallest bucket can reach.",
            "He filled it without asking you. Look at him trying not to spill a drop!",
            "The loaded boat stays dry. Sora can collect mail from the village again.",
            "Then I'll take my letter to her at the sky post.",
            "Rue is waiting beyond the orchard gate. Tell her the mill is all right.",
            "I will. Keep a cup of tea for our way back!",
            "The lantern trees are glowing. Look, the birds found their nests.",
            "That chick has stopped calling. Its mother found the crooked branch after all.",
            "The branch bridge is steady now. No more wobbling baskets.",
            "Good. I nearly dropped Gran's letter!",
            "The path is open. I can send jam to the village again.",
            "Gran will ask for the recipe. She always does.",
            "The seeds are safe and warm. Soon there will be new trees.",
            "My three seeds are still dry. One day their fruit will light this road too.",
            "Follow that red scarf. Sora will take you to the sky post.",
            "I'll take her the waiting letters. Yours can go on top, so she sees your handwriting first.",
            "The letters are all together. Not one left in a cloud!",
            "Mine is still in my pocket. Now we can send it with the recovered mail.",
            "Your letter has left for the beacon. The next keeper will carry it up.",
            "Now she will know I am coming.",
            "This bridge connects the whole post route again.",
            "So everyone can hear from their friends.",
            "My letter went with the basket. I wrote the apology, not the weather.",
            "You sent it. Now she gets to choose her first sentence.",
            "Neri's pier is beyond the lagoon gate. I sent the apology; now I need to listen.",
            "I'll go with you. She can tell us if she wants company.",
            "The pier is safe. Tell Sora she may come ashore. We have things to talk about.",
            "I'll tell her. And I'll stay by the lamp while you talk.",
            "Our lamps have little twins in the water. See?",
            "I'll draw them for Gran. The real lamp and the one that won't sit still in the water.",
            "The water gate is open. Mother's boat can follow its old lamps again.",
            "I stayed for the whole launch. Next time I'll help with the sail.",
            "Your pond light is steady. Even the fish have come to look.",
            "I'll bring you a drawing of the beacon. Perhaps the fish would like a taller lamp.",
            "Toma keeps a map of the snow road. His archive is just ahead.",
            "Keep a place for Sora at supper. I'll take the snow road after you two have talked.",
            "The leaf door is open. Please wipe your paws. The maps are old.",
            "I will be careful. We need just one road.",
            "The shelves are free. I can reach my favourite book again.",
            "Did your teacher write in its margins too? You could show me while my scarf dries.",
            "The copy marks the known turns and the gap. Lumi will check that part on the ridge.",
            "I'll add a mark when we stop there. You'll know we found it.",
            "The page is dry. Your scarf saved the ink.",
            "Gran knitted it. I think she knew I would need it.",
            "Gran's note marks the sheltered approach to the beacon. Lumi knows that ridge.",
            "Let's find Lumi. We'll take your copy and bring back the missing turns.",
            "The snow steps are bright. Stay close to me.",
            "I can follow them now, even in all this white.",
            "The shelter is warm. I kept saying I was fine; my paws say otherwise.",
            "Sit with me. I'll mark this stop on Toma's map.",
            "The frozen arch is behind us. You crossed it without looking down.",
            "I looked at your scarf instead. It helped.",
            "Pip's blanket is still dry. He told me to make sure you stayed warm.",
            "He remembered. I will bring it back with a thank-you note.",
            "The beacon road is clear. Gran answered your signal; she knows we're coming.",
            "She saw us! Let's keep the lamp raised until we reach the bridge.",
            "The last bridge is lit. Come here, let me see you.",
            "I brought stories, Gran. More than I can fit in one letter.",
            "The lantern lift is ready. Who helped you get this far?",
            "Bram, Pip, Rue... so many friends. I wish they could see us.",
            "These stairs used to look so tall to you.",
            "Then let's stop halfway. You can tell me which stair squeaks, like you used to.",
            "The sky lantern is up. I held the rail this time, and you held the light.",
            "I nearly turned back at Bram's bridge. I'm glad I told him.",
            "Look, Mika. The beacon is shining. Every island can see us now.",
            "Bram's bridge, Pip's mill... they're answering. Our way home is their road, too.",
            "The moon dial is keeping time again. I can hear its quiet tick.",
            "It sounds like Pip's mill, but sleepy.",
            "The telescope bridge is steady. There is room for two observers now.",
            "Now we can check the part of the route the tower was hiding.",
            "I marked the gap instead of hiding it. The gliders can use the route we checked.",
            "Sora can carry the corrected chart. She'll want a copy for the post.",
            "The constellations are bright. That glider knows where to turn.",
            "Look, Aster. Someone is waving from its window!",
            "The sky gate is open. Tonight I will leave a second stool by the telescope.",
            "Save it for me. I will bring Gran and a flask of warm tea."
        };
        static readonly string[] BanterLines={
            "I used to whistle on these bridges. Today I can't remember a single tune.",
            "Gran whistles when her soup boils over. I know that one. Shall I start?",
            "That board held. My hammer and your patience make a good team.",
            "Your hammer is much louder than my patience.",
            "I used to rush the wheel. It only splashed me.",
            "Let us look before we move this time.",
            "I can hear the water. Once the pipe is clear, my brother can put his bucket down.",
            "He can carry his tea cup instead. Bram saved him a slice of bread.",
            "I tried tying this branch straight. The tree broke every ribbon I used.",
            "Perhaps it wants a bow instead. That's what I ask for when Gran brushes my fur.",
            "I think a bird is following your lantern.",
            "It can come. There is room for one more friend.",
            "I can fly fast. Landing neatly is the hard part.",
            "Then we both have something to practise.",
            "This letter only says: Come for tea.",
            "That is plenty. I would go.",
            "Mother called this boat Leaf. I called every boat Leaf for years.",
            "Then this one is Little Leaf. It deserves a lamp of its own.",
            "My smallest boat always takes the longest way.",
            "Maybe it likes the view. I do too.",
            "There are biscuit crumbs in this map. My teacher used it as a tray.",
            "Then a little rain won't be its strangest journey.",
            "I marked our place. We will not lose the road.",
            "Good. I sometimes forget how far we have come.",
            "The snow is deep, but this board is clear.",
            "One dry place for my paws. Thank you.",
            "That gust lifts the snow sideways. Even the path needs a rest.",
            "I'll wait with you. The beacon won't run away.",
            "I used to count these steps with you.",
            "Now we can count the lights instead.",
            "You've tucked Rue's seeds beside the wick. Mind they don't get toasted.",
            "They're for your window. First we'll find them a pot.",
            "I mistook a firefly for a star once. I wrote it down very carefully.",
            "Maybe it was your nearest star that night.",
            "I left blank spaces on the chart. Blank isn't the same as wrong.",
            "I'll draw the clouds lightly. We can rub them out tomorrow."
        };
        static readonly string[] OpeningLines={
            "My first bridge has lost its boards. I kept the good timber.",
            "You bring the boards, Bram. I will bring the light.",
            "The wind gate creaks like my old knees. It needs a careful hand.",
            "I will keep my scarf clear of the hinges this time.",
            "I checked the sky lift twice. I still worry when someone climbs aboard.",
            "Let us mend it, then check it together.",
            "The market baker saved us a loaf. Her lamps need saving too.",
            "Warm bread and a bright doorway. That sounds like a good job.",
            "This bridge leads to Pip. I left my lunch on this side every day since the storm.",
            "When it is safe, you could walk the first part with me.",
            "One gear first. Last time I spun the wheel before my brother moved his bucket.",
            "We can go slowly enough to hear it catch.",
            "The village pipe is blocked. My brother put his bucket at the far end.",
            "Let us bring the water all the way to him.",
            "These wells serve the smallest houses. They should not have to wait.",
            "Small houses need full buckets too.",
            "I promised Sora a post boat. A promise is heavier than a bucket.",
            "Good thing we have two pairs of paws to carry it.",
            "Rue is waiting beyond this gate. She traded us seeds for a cup of water.",
            "Then this path is our way of saying thank you.",
            "The birds keep circling this tree. They remember its light.",
            "We will help the tree remember too.",
            "I made the branch bridge narrow to spare the roots. Now it needs mending.",
            "We can look after the roots and the travellers.",
            "The village needs this jam. I would rather share it than keep it safe here.",
            "First the path, then a jar for every table.",
            "The seed house is cold. I planted my first tree from a tray just like this.",
            "Let us make a warm place for things we cannot see yet.",
            "Sora used to bring letters through this gate. I miss her noisy landings.",
            "We will open it. You can put the kettle on.",
            "Every scattered letter belongs to someone waiting. I keep thinking of them.",
            "Let us gather them one safe landing at a time.",
            "Gran's letter is small, but you have carried it such a long way.",
            "I want her to know I am coming. Can we send it from here?",
            "Without this bridge, I can reach the clouds but not the post house.",
            "Even a flyer needs somewhere steady to land.",
            "The mail basket is heavy. My apology is on top; I promised not to hide it.",
            "I'll hold the basket. You can check the address one last time.",
            "Neri's shore is below this last gate. I owe her a visit, not just a letter.",
            "Come with me after we open it. She might have tea.",
            "The pier is cracked. I promised you a dry place to stand.",
            "We can make one. I will start close to the shore.",
            "Each lamp marks a turn. Mother taught me the route; I still count them aloud.",
            "Tell me the turns as we bring their lights back.",
            "The water gate holds my smallest boats outside. I hear their sails tapping.",
            "We will open their way home, carefully.",
            "This pond light has travelled for years. Now it needs someone to carry it.",
            "I can carry it beside mine. They will keep each other company.",
            "Toma keeps the snow map beyond this path. He dislikes lending precious things.",
            "I will tell him what a careful boat maker taught me.",
            "The leaf door jammed in the rain. Please mind the oldest maps inside.",
            "I will wipe my paws before I touch a page.",
            "These shelves move together. I tied them to keep the books from falling.",
            "We can untie the road without losing a book.",
            "This is my teacher's last map. I won't risk it under an open sky.",
            "Then let me trace the route. Your map can stay dry here.",
            "Rain is reaching the ink. I cannot hold the roof and the page together.",
            "You hold the roof. My scarf will look after the page.",
            "Gran's old route note is in that cabinet. We need to stop the leak to reach it.",
            "Let us open the way to it. I think I am ready now.",
            "The snow steps are hidden. I remember where their lamps belong.",
            "You remember the places. I will bring the light.",
            "I built this shelter after guiding Pip home. I'd forgotten how low its door is.",
            "Then we mend it for both of us, and have a rest.",
            "The frozen arch hides its far side. I do not like that either.",
            "We can plan the landing together before we cross.",
            "Pip sent a blanket for you. He asked me to carry it carefully.",
            "There is room for your paws under it too.",
            "The beacon road starts here. I can see one window through the snow.",
            "That is Gran's lamp. Let us give her a whole road of lights.",
            "This last bridge used to feel enormous to you. I left its lamp burning.",
            "It still feels enormous. But I know how to mend a bridge now.",
            "The lantern lift jammed in the storm. I kept its parts in my soup pot.",
            "Let's put them back. Then you can use the pot for supper.",
            "I used to carry you up these stairs. Today we will climb side by side.",
            "I can hold the lantern so you can hold the rail.",
            "The sky lantern needs both our lights. Mine is small, but I kept it safe.",
            "Mine came through every island. Let us lift them together.",
            "The beacon lens is ready. Would you like to light it, keeper Mika?",
            "Yes. So everyone who helped us can find the way home.",
            "The moon dial lost its rhythm. I counted alone until sunrise.",
            "We can count together. You can close one tired eye.",
            "My telescope bridge is missing a board. I kept observing from the doorstep.",
            "Let us mend it. The stars deserve a closer look.",
            "The wind took one corner of this chart. I stopped sharing the rest.",
            "We can start with the pieces we have, Aster.",
            "These constellation lamps once guided the night gliders. I miss their wings.",
            "Let us give the first glider a pattern to follow.",
            "The last sky gate is ahead. I would like someone else to check my chart.",
            "I will check it with you. Then we can wave to the travellers."
        };
        static readonly string[] AsideLines={
            "I checked that joint again. No creak. I might walk over for lunch tomorrow.",
            "I'll wait on the far side. With bread, if the baker has any left.",
            "My brother filled his bucket himself. He asked when you are coming for tea.",
            "After the post boat is ready. We have a promise to carry.",
            "You still have those seeds. I thought you might have forgotten them.",
            "They are warm beside the letter. I want to show Gran tomorrow's orchard.",
            "I wrote: I was wrong to leave without listening. Now the page can dry.",
            "I'll keep the bag open for it. No hiding it underneath this time.",
            "The little boat reached its lamp. My mother would have liked your careful landing.",
            "I was still scared. You made room for that too.",
            "The original is back on its shelf. Your copy already has a new shelter.",
            "I'll pencil in each place we stop. The next traveller can use it.",
            "I usually tell travellers to stay close. Today I am glad you told me too.",
            "Guides can share blankets. Pip sent a very big one.",
            "I burnt the soup twice watching that bridge. You can stir while I watch the wick.",
            "Gladly. And I've brought enough stories to last until it's ready.",
            "I put two pencils by the chart. Someone sharpened the spare while we worked.",
            "Leave the second stool there. They may want to finish their drawing."
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
            int world=JourneyStory.Zone(stop),offset=world*4+(JourneyStory.IslandFor(stop)%2)*2;
            return new StoryConversation(4000+int.Parse(JourneyStory.NodeIds[stop].Substring(1))-1,new StoryLine(Resident(world),BanterLines[offset]),new StoryLine("Mika",BanterLines[offset+1]));
        }
    }
}
