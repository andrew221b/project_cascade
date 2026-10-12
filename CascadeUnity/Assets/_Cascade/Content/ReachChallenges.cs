using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Domain;

namespace Cascade.Content
{
    // Authored workshop detours, versioned separately from the campaign.
    public static class ReachChallenges
    {
        public static LevelContent[] Create()
        {
            var result=new LevelContent[6];
            for(int i=0;i<result.Length;i++) {
                int count=i==0?3:i<4?4:5;
                var row=Enumerable.Range(0,count).Select(n=>SymbolId.Parse("s:"+((char)('A'+n)))).ToArray();
                var open=Enumerable.Repeat(true,count-1).ToArray();
                FloorDefinition[] floors;
                PuzzleCommand[] route;
                if(i<2){floors=new[]{new FloorDefinition(1,open)};route=new[]{PuzzleCommand.Reach(0,count-1)};}
                else if(i==2){floors=new[]{new FloorDefinition(2,open)};route=new[]{PuzzleCommand.Reach(0,3),PuzzleCommand.Swap(0)};}
                else if(i==3){floors=new[]{new FloorDefinition(1,open),new FloorDefinition(1,open,entryTransform:EntryTransform.ShiftLeftOne)};route=new[]{PuzzleCommand.Reach(0,3),PuzzleCommand.Swap(1)};}
                else if(i==4){floors=new[]{new FloorDefinition(3,open,fragileEdges:new[]{1},oneUseBridgeEdge:2)};route=new[]{PuzzleCommand.Reach(0,3),PuzzleCommand.Swap(3)};}
                else {floors=new[]{new FloorDefinition(2,open,optionalStamp:new ContactDefinition(0,row[3])),new FloorDefinition(2,open,entryTransform:EntryTransform.MirrorNext)};route=new[]{PuzzleCommand.Reach(0,3),PuzzleCommand.Swap(1),PuzzleCommand.Swap(0),PuzzleCommand.Swap(3)};}
                string id="R"+(i+1).ToString("00"), hash;
                using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes("cascade-longreach-v1:"+id))).Replace("-","").ToLowerInvariant();
                var segments=new[]{new TargetSegment(SegmentKind.Shapes,0,count)};
                var draft=new LevelDefinition(id,id,"1.0","2.1-longreach",hash,row,row,segments,floors,1);
                var resolver=new PuzzleResolver(draft);var state=resolver.CreateInitialState();
                foreach(var command in route){var step=resolver.Resolve(state,command);if(!step.Accepted)throw new InvalidOperationException("Invalid authored reach route "+id);state=step.State;}
                var level=new LevelDefinition(id,id,"1.0","2.1-longreach",hash,row,state.Row,segments,floors,1);
                result[i]=new LevelContent(level,"en","Ribbon reach","reach",false);
            }
            return result;
        }
    }
}
