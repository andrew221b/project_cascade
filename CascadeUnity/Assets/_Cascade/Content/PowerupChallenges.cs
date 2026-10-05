using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Domain;
namespace Cascade.Content
{
    public static class PowerupChallenges
    {
        public static LevelContent[] Create()
        {
            var result=new LevelContent[6];
            for(int i=0;i<6;i++){
                int n=i==2||i==4?5:4;
                var row=Enumerable.Range(0,n).Select(x=>SymbolId.Parse("s:"+(char)('A'+x))).ToArray();var open=Enumerable.Repeat(true,n-1).ToArray();
                FloorDefinition[] floors;PuzzleCommand[] route;
                PowerupDefinition Ribbon(int slot,int symbol)=>new PowerupDefinition(slot,row[symbol],PowerupKind.Ribbon);
                PowerupDefinition Clock(int slot,int symbol)=>new PowerupDefinition(slot,row[symbol],PowerupKind.Hourglass);
                if(i==0){floors=new[]{new FloorDefinition(2,open,powerup:Ribbon(0,1))};route=new[]{PuzzleCommand.Swap(0),PuzzleCommand.Reach(1,3)};}
                else if(i==1){floors=new[]{new FloorDefinition(3,open,powerup:Ribbon(1,3))};route=new[]{PuzzleCommand.Swap(2),PuzzleCommand.Swap(1),PuzzleCommand.Reach(0,3)};}
                else if(i==2){floors=new[]{new FloorDefinition(1,open,powerup:Ribbon(0,1)),new FloorDefinition(2,open,fragileEdges:new[]{2})};route=new[]{PuzzleCommand.Swap(0),PuzzleCommand.Reach(1,4)};}
                else if(i==3){floors=new[]{new FloorDefinition(4,open,bomb:new BombDefinition(3,row[0],2,4),powerup:Clock(0,1))};route=new[]{PuzzleCommand.Swap(0),PuzzleCommand.Swap(1),PuzzleCommand.Swap(2),PuzzleCommand.Drop};}
                else if(i==4){floors=new[]{new FloorDefinition(5,open,bomb:new BombDefinition(4,row[0],3,4),powerup:Clock(1,2))};route=new[]{PuzzleCommand.Swap(1),PuzzleCommand.Swap(0),PuzzleCommand.Swap(1),PuzzleCommand.Swap(2),PuzzleCommand.Swap(3)};}
                else {floors=new[]{new FloorDefinition(3,open,bomb:new BombDefinition(3,row[0],2,4),powerup:Clock(0,1)),new FloorDefinition(2,open,powerup:Ribbon(0,2))};route=new[]{PuzzleCommand.Swap(0),PuzzleCommand.Swap(1),PuzzleCommand.Swap(2),PuzzleCommand.Swap(0),PuzzleCommand.Reach(1,3)};}
                string id="B"+(i+1).ToString("00"),hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes("cascade-pickups-v1:"+id))).Replace("-","").ToLowerInvariant();
                var segments=new[]{new TargetSegment(SegmentKind.Shapes,0,n)};
                var draft=new LevelDefinition(id,id,"1.0","2.2-pickups",hash,row,row,segments,floors);
                var resolver=new PuzzleResolver(draft);var state=resolver.CreateInitialState();
                foreach(var command in route){var step=resolver.Resolve(state,command);if(!step.Accepted)throw new InvalidOperationException("Invalid authored power-up route "+id);state=step.State;}
                if(state.ActiveFloor!=floors.Length)throw new InvalidOperationException("Power-up route did not deliver "+id);
                result[i]=new LevelContent(new LevelDefinition(id,id,"1.0","2.2-pickups",hash,row,state.Row,segments,floors),"en","Workshop tricks",i<3?"ribbon_pickup":"hourglass",false);
            }
            return result;
        }
    }
}
