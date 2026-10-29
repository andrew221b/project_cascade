using System;
using Cascade.Domain;
namespace Cascade.Content
{
    public static partial class StoryConversations
    {
        public static StoryConversation ParallelSky(PuzzleMode mode,int world,bool returning)
        {
            if(!Enum.IsDefined(typeof(PuzzleMode),mode))throw new ArgumentOutOfRangeException(nameof(mode));
            if(world<0||world>=JourneyStory.WorldCount)throw new ArgumentOutOfRangeException(nameof(world));
            int at=(int)mode*(returning?2:3);var lines=returning?ParallelReturn:ParallelFirst;
            int receipt=(returning?9010:9000)+(int)mode;
            return returning?new StoryConversation(receipt,new StoryLine("Mika",lines[at]),new StoryLine(Resident(world),lines[at+1])):
                new StoryConversation(receipt,new StoryLine("Mika",lines[at]),new StoryLine(Resident(world),lines[at+1]),new StoryLine("Mika",lines[at+2]));
        }
        static readonly string[] ParallelFirst={
            "Shapes on this receiver. Ada taught me those before I could read her notes.",
            "This route is still dark. I'll check the anchors while you check the receiver.",
            "A different order here. Let's start with the nearest lamp.",
            "Letters on the light pieces. Same row to carry, another way to read the receiver.",
            "There's a separate chain of lamps here. We still need to light it.",
            "I'll read the whole order before moving. Ada would ask me to do that twice.",
            "Letters and shapes together. I'll need to watch the order, not just the familiar colours.",
            "The far rope is secured. Look at the receiver while I check this anchor.",
            "I see it. I'll start with the lower lamp, then we'll check the whole chain."
        };
        static readonly string[] ParallelReturn={
            "Our shape route. I'll check the last receiver before continuing.",
            "The rope is where you left it. We kept the notes too.",
            "Back to the lettered pieces. Our notes show where we stopped.",
            "The rope is ready. We'll start at that anchor. I'll show you.",
            "Here's the mixed receiver again. I'll read every mark before lowering the row.",
            "I kept the far anchor clear. Your notes are beside the rope."
        };
    }
}
