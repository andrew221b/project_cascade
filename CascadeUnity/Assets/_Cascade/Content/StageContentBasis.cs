using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Domain;
namespace Cascade.Content
{
    public static class StageContentBasis
    {
        public static int[] Pattern(LevelDefinition source,string mode)
        {
            int n=source.TargetSymbolIds.Count;
            if(mode=="Words")return Enumerable.Range(0,n).ToArray();
            if(mode=="Mixed"){
                if(n==7&&source.TargetSymbolIds.Distinct().Count()<n)return new[]{0,0,1,2,3,4,4};
                return Enumerable.Range(0,Math.Max(4,n)).ToArray();
            }
            if(mode!="Shapes")throw new ArgumentException("Unknown stage mode.");
            var order=source.TargetSymbolIds.Distinct().ToArray();
            return order.Length<=6?source.TargetSymbolIds.Select(s=>Array.IndexOf(order,s)).ToArray():new[]{0,1,2,3,4,5,0,1};
        }
        public static string Key(LevelDefinition source,string mode)=>source.CampaignNodeId+":"+string.Join(",",Pattern(source,mode));
        public static bool IsRenaming(LevelDefinition source,string mode)
        {
            var order=source.TargetSymbolIds.Distinct().ToArray();return source.TargetSymbolIds.Select(s=>Array.IndexOf(order,s)).SequenceEqual(Pattern(source,mode));
        }
        public static LevelDefinition Canonical(LevelDefinition source,string mode)
        {
            var pattern=Pattern(source,mode);int n=pattern.Length;var target=pattern.Select(i=>SymbolId.Parse("s:"+(char)('A'+i))).ToArray();
            SymbolId Map(SymbolId value){int first=source.TargetSymbolIds.ToList().IndexOf(value);if(first<0)throw new ArgumentException("Unknown contact symbol.");return target[first];}
            ContactDefinition Contact(ContactDefinition c)=>c==null?null:new ContactDefinition(c.Slot,Map(c.RequiredSymbol));
            var floors=source.Floors.Select(f=>new FloorDefinition(f.BaseDurability,f.OpenEdges.Concat(Enumerable.Repeat(true,n-source.TargetSymbolIds.Count)),f.FragileEdges,f.LinkedPair,f.EntryTransform,
                f.Bomb==null?null:new BombDefinition(f.Bomb.Slot,Map(f.Bomb.RequiredSymbol),f.Bomb.Ticks,f.Bomb.Damage),f.Key==null?null:new KeyDefinition(f.Key.Slot,Map(f.Key.RequiredSymbol),f.Key.EdgeToOpen),
                f.Repair==null?null:new RepairDefinition(f.Repair.Slot,Map(f.Repair.RequiredSymbol),f.Repair.Amount),f.OneUseBridgeEdge,Contact(f.OptionalStamp),
                f.Powerup==null?null:new PowerupDefinition(f.Powerup.Slot,Map(f.Powerup.RequiredSymbol),f.Powerup.Kind),f.GustAfterSwaps,f.GustTransform,f.GustDamage)).ToArray();
            var start=target.Reverse().ToArray();
            var level=new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,ContentImporter.Hash("stage-basis-v1|"+source.ContentHash+"|"+string.Join(",",pattern)),start,target,new[]{new TargetSegment(SegmentKind.Shapes,0,n)},floors,source.ReachCharges,source.TeachingNode);
            LevelValidator.ThrowIfInvalid(level);return level;
        }
        public static int WordStart(LevelDefinition source,string mode)=>mode=="Shapes"?-1:mode=="Mixed"&&Pattern(source,mode).Length==7&&Pattern(source,mode).Distinct().Count()<7?2:0;
        public static int WordLength(LevelDefinition source,string mode)=>mode=="Shapes"?0:mode=="Words"?Pattern(source,mode).Length:WordStart(source,mode)==2?3:Pattern(source,mode).Length-1;
        public static string ShapeBucket(LevelDefinition source,string mode)=>mode=="Mixed"?"mixed-shape:"+ShapeCount(source,mode):"shape:"+string.Join(",",Pattern(source,mode));
        public static int ShapeCount(LevelDefinition source,string mode)
        {
            int start=WordStart(source,mode),length=WordLength(source,mode);return Pattern(source,mode).Where((value,index)=>index<start||index>=start+length).Distinct().Count();
        }
        static readonly Dictionary<int,int[][]> choices=new Dictionary<int,int[][]>();
        static int[][] ShapeChoices(int length)
        {
            lock(choices){if(choices.TryGetValue(length,out var existing))return existing;var rows=new List<int[]>();
                void Add(List<int> row){if(row.Count==length){rows.Add(row.ToArray());return;}for(int i=0;i<6;i++)if(!row.Contains(i)){row.Add(i);Add(row);row.RemoveAt(row.Count-1);}}
                Add(new List<int>());return choices[length]=rows.ToArray();}
        }
        public static StageSymbols Symbols(LevelDefinition source,string mode,string locale,StageWordPool pool,string seed,int wordDraw,int shapeDraw)
        {
            int[] pattern=Pattern(source,mode);int start=WordStart(source,mode),length=WordLength(source,mode);var mapping=new Dictionary<int,string>();WordDefinition word=null;
            if(length>0){word=pool.Draw(locale,length,seed,wordDraw);for(int i=0;i<length;i++)mapping.Add(pattern[start+i],"g:"+locale+":"+word.Graphemes[i]);}
            var shapeKeys=pattern.Where(p=>!mapping.ContainsKey(p)).Distinct().ToArray();
            if(shapeKeys.Length>0){var permutations=ShapeChoices(shapeKeys.Length);var shapes=permutations[StageShuffle.Index(permutations.Length,seed,ShapeBucket(source,mode),shapeDraw)];for(int i=0;i<shapeKeys.Length;i++)mapping.Add(shapeKeys[i],"s:"+(char)('A'+shapes[i]));}
            return new StageSymbols{Target=pattern.Select(p=>mapping[p]).ToArray(),Locale=locale,WordStart=start,WordId=word?.WordId,Word=word?.DisplayText};
        }
    }
}
