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
            "Walter left a triangle mark on the crate. That must be the timber he promised.",
            "He sends the straightest boards away and keeps the crooked ones for his own door.",
            "Let's save him one straight board. Gran says his door only opens in dry weather.",
            "There's a letter on each parcel. This one spells HOME. Is it for the beacon?",
            "For a family at the mill. They've been sleeping in the storehouse since the storm.",
            "Then their parcel goes first. I'll put Gran's letter somewhere dry while we carry it.",
            "A crate with a square, and a parcel with a letter. Who packed this lot?",
            "The whole village. Someone even tied a biscuit to the handle. For the journey, they said.",
            "I'll deliver everything. Except perhaps the biscuit. It might not survive the journey."
        };
        static readonly string[] ParallelReturn={
            "There's Walter's triangle again. We left this crate by the bridge.",
            "And it's still dry. I covered it when the rain came. Shall we carry on?",
            "The lettered parcels! We left the last one here. Did anyone collect it?",
            "Not yet. They asked me to thank you. I'll tell them when the next load arrives.",
            "Our mixed delivery is still waiting. I hope nobody ate the biscuit.",
            "I put it in the tin. The crumbs were tempting the birds. Here, take it before they return."
        };
    }
}
