using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Content;
using Cascade.Domain;
namespace Cascade.Persistence
{
    // Global to the profile, so Words and Mixed share their used-word decks.
    // Compact bitmaps merge independently of the device's shuffle seed.
    public sealed class StageVarietyMemory
    {
        public string Seed,PoolHash;
        public Dictionary<string,int> Draws=new Dictionary<string,int>();
        public Dictionary<string,string> UsedWords=new Dictionary<string,string>();
        public Dictionary<string,int> LastWords=new Dictionary<string,int>();
        public Dictionary<string,string> LastShapes=new Dictionary<string,string>();
        public void Validate()
        {
            bool Hex(string s,int length)=>s!=null&&s.Length==length&&s.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
            if(Draws==null||UsedWords==null||LastWords==null||LastShapes==null||Draws.Count>64||UsedWords.Count>18||LastWords.Count>18||LastShapes.Count>64
                ||Seed!=null&&!Hex(Seed,32)||PoolHash!=null&&!Hex(PoolHash,64)||Draws.Any(p=>p.Key==null||p.Key.Length>100||p.Value<0||p.Value>1000000000)
                ||LastShapes.Any(p=>p.Key==null||p.Key.Length>100||string.IsNullOrEmpty(p.Value)||p.Value.Length>8||p.Value.Any(c=>c<'A'||c>'F'))
                ||LastWords.Any(p=>!WordKey(p.Key)||p.Value<0||p.Value>100000)||UsedWords.Any(p=>!WordKey(p.Key)||p.Value==null||p.Value.Length>20000))throw new InvalidDataException("Invalid stage variety memory.");
            foreach(var bits in UsedWords.Values)try{Convert.FromBase64String(bits);}catch(FormatException){throw new InvalidDataException("Invalid used-word bitmap.");}
            if((Draws.Count>0||UsedWords.Count>0)&&(Seed==null||PoolHash==null))throw new InvalidDataException("Unpinned stage variety memory.");
        }
        public void ValidatePool(StageWordPool pool)
        {
            Validate();if(PoolHash!=null&&PoolHash!=pool.Hash)throw new InvalidDataException("Unknown stage vocabulary memory.");
            foreach(var pair in UsedWords){var size=pool.Size(pair.Key.Substring(0,2),pair.Key[3]-'0');var bytes=Convert.FromBase64String(pair.Value);
                if(bytes.Length!=(size+7)/8||Convert.ToBase64String(bytes)!=pair.Value||size%8!=0&&bytes[bytes.Length-1]>>(size%8)!=0)throw new InvalidDataException("Invalid stage vocabulary bitmap.");}
            foreach(var pair in LastWords)if(pair.Value>=pool.Size(pair.Key.Substring(0,2),pair.Key[3]-'0'))throw new InvalidDataException("Invalid last stage word.");
        }
        static bool WordKey(string key)=>key!=null&&key.Length==4&&(key.StartsWith("en:",StringComparison.Ordinal)||key.StartsWith("ru:",StringComparison.Ordinal)||key.StartsWith("uk:",StringComparison.Ordinal))&&key[3]>='3'&&key[3]<='8';
        int Next(string key){Draws.TryGetValue(key,out int value);if(value>=1000000000)throw new InvalidOperationException("Stage draw budget exhausted.");Draws[key]=value+1;return value;}
        int WordDraw(StageWordPool pool,string locale,int length)
        {
            string key=locale+":"+length,counter="word:"+key;int count=pool.Size(locale,length),bytes=(count+7)/8;
            if(count<3)throw new InvalidOperationException("Stage word deck unavailable.");
            var bitmap=UsedWords.TryGetValue(key,out var encoded)?Convert.FromBase64String(encoded):new byte[bytes];if(bitmap.Length!=bytes)throw new InvalidDataException("Stage vocabulary size changed.");
            bool Used(int index)=>(bitmap[index/8]&(1<<(index%8)))!=0;
            if(Enumerable.Range(0,count).All(Used))Array.Clear(bitmap,0,bitmap.Length);
            Draws.TryGetValue(counter,out int draw);int previous=LastWords.TryGetValue(key,out int last)?last:-1;
            var decks=new Dictionary<int,int[]>();
            for(int tries=0;tries<count*3;tries++,draw++){
                int cycle=draw/count;if(!decks.TryGetValue(cycle,out var deck)){deck=pool.Permutation(locale,length,Seed,cycle);decks.Add(cycle,deck);}int index=deck[draw%count];
                if(Used(index)||index==previous)continue;
                bitmap[index/8]|=(byte)(1<<(index%8));UsedWords[key]=Convert.ToBase64String(bitmap);LastWords[key]=index;
                if(draw>=1000000000)throw new InvalidOperationException("Stage draw budget exhausted.");Draws[counter]=draw+1;return draw;
            }
            throw new InvalidOperationException("Could not select an unused stage word.");
        }
        public StageVariation Reserve(LevelDefinition source,string mode,string locale,StageWordPool pool)
        {
            ValidatePool(pool);if(Seed==null)Seed=Guid.NewGuid().ToString("N");if(PoolHash==null)PoolHash=pool.Hash;if(PoolHash!=pool.Hash)throw new InvalidDataException("Stage vocabulary revision unavailable.");
            var plan=new StageVariation{Seed=Seed,PoolHash=pool.Hash,Mode=mode,Locale=locale,WordDraws=new int[3],ShapeDraws=new int[3]};
            int length=StageContentBasis.WordLength(source,mode),shapes=StageContentBasis.ShapeCount(source,mode);
            for(int i=0;i<3;i++){
                plan.WordDraws[i]=length>0?WordDraw(pool,locale,length):-1;plan.ShapeDraws[i]=-1;
                if(shapes>0){
                    string key=StageContentBasis.ShapeBucket(source,mode);LastShapes.TryGetValue(key,out string previous);
                    do{plan.ShapeDraws[i]=Next(key);}while(ShapeSignature(StageContentBasis.Symbols(source,mode,locale,pool,Seed,plan.WordDraws[i],plan.ShapeDraws[i]))==previous);
                    LastShapes[key]=ShapeSignature(StageContentBasis.Symbols(source,mode,locale,pool,Seed,plan.WordDraws[i],plan.ShapeDraws[i]));
                }
            }
            return plan;
        }
        static string ShapeSignature(StageSymbols symbols)=>string.Concat(symbols.Target.Where(id=>id.StartsWith("s:",StringComparison.Ordinal)).Select(id=>id.Substring(2)));
        public StageVariation Renew(LevelDefinition source,StageVariation previous,int stage,StageWordPool pool)
        {
            ValidatePool(pool);
            if(previous==null||!previous.IsBounded()||stage<0||stage>2||previous.PoolHash!=pool.Hash)throw new ArgumentException("Invalid stage retry.");
            var plan=CloudJson.Decode<StageVariation>(CloudJson.Encode(previous));string activeSeed=Seed;Seed=previous.Seed;
            try{
                int length=StageContentBasis.WordLength(source,plan.Mode),shapes=StageContentBasis.ShapeCount(source,plan.Mode);
                plan.WordDraws[stage]=length>0?WordDraw(pool,plan.Locale,length):-1;
                if(shapes>0){
                    string key=StageContentBasis.ShapeBucket(source,plan.Mode);
                    var neighbours=previous.Symbols.Select(ShapeSignature).ToHashSet(StringComparer.Ordinal);
                    StageSymbols candidate;
                    do{
                        plan.ShapeDraws[stage]=Next(key);
                        candidate=StageContentBasis.Symbols(source,plan.Mode,plan.Locale,pool,plan.Seed,plan.WordDraws[stage],plan.ShapeDraws[stage]);
                        }while(neighbours.Contains(ShapeSignature(candidate)));
                    // The last reserved stage is still the next round's neighbour.
                    LastShapes[key]=stage==2?ShapeSignature(candidate):ShapeSignature(previous.Symbols[2]);
                }
                plan.Symbols=null;return plan;
            }finally{Seed=activeSeed;}
        }
        public static StageVarietyMemory Merge(StageVarietyMemory remote,StageVarietyMemory local)
        {
            if(local==null)return remote==null?new StageVarietyMemory():CloudJson.Decode<StageVarietyMemory>(CloudJson.Encode(remote));
            var result=CloudJson.Decode<StageVarietyMemory>(CloudJson.Encode(local));if(remote==null||remote.PoolHash==null)return result;
            if(result.PoolHash==null)return CloudJson.Decode<StageVarietyMemory>(CloudJson.Encode(remote));
            if(result.PoolHash!=remote.PoolHash)throw new InvalidDataException("Different stage vocabulary revisions.");
            if(result.Seed==remote.Seed)foreach(var draw in remote.Draws){result.Draws.TryGetValue(draw.Key,out int current);if(draw.Value>current&&remote.LastShapes.TryGetValue(draw.Key,out string last))result.LastShapes[draw.Key]=last;result.Draws[draw.Key]=Math.Max(current,draw.Value);}
            foreach(var pair in remote.UsedWords){
                if(!result.UsedWords.TryGetValue(pair.Key,out var existing)){result.UsedWords[pair.Key]=pair.Value;continue;}
                var left=Convert.FromBase64String(existing);var right=Convert.FromBase64String(pair.Value);if(left.Length!=right.Length)throw new InvalidDataException("Different stage vocabulary bitmap sizes.");
                for(int i=0;i<left.Length;i++)left[i]|=right[i];result.UsedWords[pair.Key]=Convert.ToBase64String(left);
            }
            result.Validate();return result;
        }
    }
}
