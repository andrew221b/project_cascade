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
    public static class StoryConversations
    {
        static readonly string[] Residents={
            "Bram",
            "Pip",
            "Rue",
            "Sora",
            "Neri",
            "Toma",
            "Lumi",
            "Gran"
        };
        static readonly string[] Introductions={
            "Bram here. The storm broke our bridges. I can mend the wood if you bring back the lights.",
            "I am Pip. Our mill has stopped. Without it, the village has no water.",
            "I am Rue. My lantern trees went dark. Even the birds cannot find their nests.",
            "Sora, sky post! Well... ground post today. The wind scattered all our letters.",
            "I am Neri. The pond lamps went out. My little boats cannot find the shore.",
            "Toma. I keep the maps. The storm hid the road to the beacon between these shelves.",
            "I am Lumi. I know the snow road. We will follow the warm lights together.",
            "Mika! There you are. The beacon is dark, but I kept a little light for you."
        };
        static readonly string[] Replies={
            "I am Mika. I am taking a light to Gran at the Great Beacon. Let us start here.",
            "We will get it turning. I would like to hear the water again.",
            "Let us light one tree first. Then the birds can show us the way.",
            "One of those letters is for Gran. Let us find it together.",
            "I can help with the lamps. I will stay close to the shore!",
            "We have come so far. Let us find the last part of the road.",
            "My paws are cold, but my lantern is still warm. Lead the way.",
            "I kept mine too, Gran. Let us bring them home."
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
            "Bring the light home"
        };
        static readonly string[] IslandLines={
            "The first bridge is lit. I can see my workshop again!",
            "And I can see the next step. That feels less scary.",
            "The wind gate is open. Hold your scarf, Mika.",
            "I am holding it! Now we can reach the sky lift.",
            "That lift has not moved since the storm. You did it.",
            "We did it. Your new boards held me up.",
            "The market lamps are back. I missed the smell of warm bread.",
            "Let us save a loaf for Pip. He has been waiting.",
            "The road to the mill is safe. I will keep these bridges lit.",
            "Thank you, Bram. When the beacon shines, you will see it from here.",
            "Listen! The wheel is turning again.",
            "It sounds like the village waking up.",
            "Water is on its way to every little house.",
            "Now nobody has to carry it all that way.",
            "The wells are full. Even my smallest bucket can reach.",
            "Then your little brother can help too.",
            "The post boat floats again. There is a letter for your gran aboard.",
            "I will make sure it gets there.",
            "Rue is waiting beyond the orchard gate. Tell her the mill is all right.",
            "I will. Keep a cup of tea for our way back!",
            "The lantern trees are glowing. Look, the birds found their nests.",
            "They are singing louder than my lantern can shine.",
            "The branch bridge is steady now. No more wobbling baskets.",
            "Good. I nearly dropped Gran's letter!",
            "The path is open. I can send jam to the village again.",
            "Gran will ask for the recipe. She always does.",
            "The seeds are safe and warm. Soon there will be new trees.",
            "Little lights for the next travellers.",
            "Follow that red scarf. Sora will take you to the sky post.",
            "Another friend on the road. I like that.",
            "The letters are all together. Not one left in a cloud!",
            "That small one is Gran's. Please keep it dry.",
            "Your letter is on its way. I put it in my safest bag.",
            "Now she will know I am coming.",
            "This bridge connects the whole post route again.",
            "So everyone can hear from their friends.",
            "The basket is up! I should write a letter too. I just never know how to start.",
            "Try: I missed you. That is a good start.",
            "Neri is waiting by the lagoon. She makes boats out of leaves.",
            "I hope mine will have a very small sail.",
            "The pier is whole again. You can come closer, Mika.",
            "Only a little. Deep water still makes my paws shake.",
            "Our lamps have little twins in the water. See?",
            "Two lights from one. Gran would love this place.",
            "The water gate is open. The little boats can go home.",
            "The beacon is still dark. I hope Gran can see my letter.",
            "Your pond light is steady. Even the fish have come to look.",
            "They look like tiny stars swimming.",
            "Toma keeps a map of the snow road. His archive is just ahead.",
            "Thank you, Neri. I will come back when the lamps are all lit.",
            "The leaf door is open. Please wipe your paws. The maps are old.",
            "I will be careful. We need just one road.",
            "The shelves are free. I can reach my favourite book again.",
            "What is it about? I would like to read it with Gran.",
            "Here is the storm map. The snow road leads straight to the beacon.",
            "It looks far on paper. We will take it one step at a time.",
            "The page is dry. Your scarf saved the ink.",
            "Gran knitted it. I think she knew I would need it.",
            "This note is from Gran: I am waiting. There is soup on the stove.",
            "Then we had better not let it get cold. Where is Lumi?",
            "The snow steps are bright. Stay close to me.",
            "I can follow them now, even in all this white.",
            "The shelter is warm again. We can rest our paws.",
            "Just a moment. I want to tell Gran about everyone we met.",
            "The frozen arch is behind us. You crossed it without looking down.",
            "I looked at your scarf instead. It helped.",
            "Pip's blanket is still dry. He told me to make sure you stayed warm.",
            "He remembered. I will bring it back with a thank-you note.",
            "There is the beacon road. You can see Gran's window from here.",
            "A light in her window! She really is waiting.",
            "The last bridge is lit. Come here, let me see you.",
            "I brought stories, Gran. More than I can fit in one letter.",
            "The lantern lift is ready. Who helped you get this far?",
            "Bram, Pip, Rue... so many friends. I wish they could see us.",
            "These stairs used to look so tall to you.",
            "They still do. But now I know how to keep going.",
            "The sky lantern is up. Your little light made it all the way.",
            "I was scared sometimes. The others stayed with me.",
            "Look, Mika. The beacon is shining. Every island can see us now.",
            "Then let us wave to everyone. We made it home together."
        };
        static readonly string[] BanterLines={
            "Your paws are shaking. We can take a breath before the next board.",
            "A breath, then a step. I can do that.",
            "That board held. My hammer and your patience make a good team.",
            "Your hammer is much louder than my patience.",
            "I used to rush the wheel. It only splashed me.",
            "Let us look before we move this time.",
            "Can you hear the mill from here?",
            "Yes. I like knowing it is still working.",
            "One branch at a time. That is how a tree grows.",
            "And one board at a time for us.",
            "I think a bird is following your lantern.",
            "It can come. There is room for one more friend.",
            "I can fly fast. Landing neatly is the hard part.",
            "Then we both have something to practise.",
            "This letter only says: Come for tea.",
            "That is plenty. I would go.",
            "The pond is quiet today. We can hear ourselves think.",
            "I am thinking about that next board.",
            "My smallest boat always takes the longest way.",
            "Maybe it likes the view. I do too.",
            "We need the right order, just like books on a shelf.",
            "Then I will leave the silly book next to the scary one.",
            "I marked our place. We will not lose the road.",
            "Good. I sometimes forget how far we have come.",
            "The snow is deep, but this board is clear.",
            "One dry place for my paws. Thank you.",
            "My ears caught a breeze. We will wait until you are ready.",
            "Almost ready. I am checking the row.",
            "I used to count these steps with you.",
            "Now we can count the lights instead.",
            "No need to hurry, little one. I am right here.",
            "Then I will do this last part carefully."
        };
        public static string Resident(int world)=>Residents[Math.Max(0,Math.Min(7,world))];
        public static string Task(int stop)=>Goals[JourneyStory.Zone(stop)*5+JourneyStory.IslandFor(stop)];
        public static StoryConversation BeforeLevel(int stop)=>Visit(stop,JourneyStory.PageFor(JourneyStory.WorldFirst(JourneyStory.Zone(stop))));
        public static StoryConversation Visit(int stop,int receipt=-1,bool alreadyMet=false)
        {
            int world=JourneyStory.Zone(stop);
            if(alreadyMet)return new StoryConversation(receipt,new StoryLine(Resident(world),"Our next job: {0}. Ready when you are.",Task(stop)),new StoryLine("Mika","Let us do this together. Then we can carry on."));
            return new StoryConversation(receipt,new StoryLine(Resident(world),Introductions[world]),
                new StoryLine("Mika",Replies[world]),new StoryLine(Resident(world),"Our next job: {0}. Ready when you are.",Task(stop)));
        }
        public static StoryConversation Island(int world,int island)
        {
            if(world<0||world>=8||island<0||island>=5)throw new ArgumentOutOfRangeException();
            int offset=(world*5+island)*2;
            return new StoryConversation(2000+world*10+island,new StoryLine(Resident(world),IslandLines[offset]),new StoryLine("Mika",IslandLines[offset+1]));
        }
        public static bool HasCheckpoint(int stop,int completedStage)=>completedStage==1&&stop>=1&&stop<160&&stop%3==1;
        public static StoryConversation Checkpoint(int stop)
        {
            int world=JourneyStory.Zone(stop),offset=world*4+(JourneyStory.IslandFor(stop)%2)*2;
            return new StoryConversation(4000+stop,new StoryLine(Resident(world),BanterLines[offset]),new StoryLine("Mika",BanterLines[offset+1]));
        }
    }
}
