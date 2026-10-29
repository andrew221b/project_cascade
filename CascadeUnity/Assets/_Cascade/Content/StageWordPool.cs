using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Domain;
using Newtonsoft.Json.Linq;
namespace Cascade.Content
{
    public sealed class StageWordPool
    {
        public const string Revision="stage-words-v1";
        readonly Dictionary<string,string[]> buckets=new Dictionary<string,string[]>();
        readonly Dictionary<string,int[]> familiar=new Dictionary<string,int[]>();
        public string Hash { get; }
        public int Count { get; }
        public StageWordPool(string json)
        {
            var root=JObject.Parse(json);if((string)root["revision"]!=Revision||!(root["words"] is JObject words))throw new ArgumentException("Unknown stage word pool.");
            Hash=ContentImporter.Hash(json);int total=0;
            foreach(string locale in new[]{"en","ru","uk"}){
                var list=(words[locale] as JArray)?.Select(t=>(string)t).ToArray();
                if(list==null||list.Length<1500||list.Length>100000||list.Distinct(StringComparer.Ordinal).Count()!=list.Length)throw new ArgumentException("Invalid stage word pool.");
                foreach(var word in list){
                    if(string.IsNullOrEmpty(word)||word.Length<3||word.Length>8||word!=word.Normalize(NormalizationForm.FormC)||word!=word.ToUpperInvariant()||word.Distinct().Count()!=word.Length)throw new ArgumentException("Invalid stage word: "+word);
                    foreach(char c in word)if(locale=="en"?c<'A'||c>'Z':locale=="ru"?!(c>='А'&&c<='Я'||c=='Ё'):!(c>='А'&&c<='Щ'||c=='Ь'||c=='Ю'||c=='Я'||c=='Є'||c=='І'||c=='Ї'||c=='Ґ'))throw new ArgumentException("Wrong stage alphabet.");
                }
                for(int length=3;length<=8;length++){
                    string key=locale+":"+length;var bucket=list.Where(w=>w.Length==length).OrderBy(w=>w,StringComparer.Ordinal).ToArray();buckets.Add(key,bucket);
                    var preferred=(root["preferred"]?[locale] as JArray)?.Select(t=>(string)t).Where(w=>w!=null&&w.Length==length).ToArray()??Array.Empty<string>();
                    familiar.Add(key,preferred.Select(w=>Array.IndexOf(bucket,w)).Where(i=>i>=0).Distinct().OrderBy(i=>i).ToArray());
                }
                total+=list.Length;
            }
            Count=total;
        }
        public int Size(string locale,int length)=>buckets.TryGetValue(locale+":"+length,out var words)?words.Length:0;
        public WordDefinition At(string locale,int length,int index)
        {
            if(!buckets.TryGetValue(locale+":"+length,out var words)||index<0||index>=words.Length)throw new ArgumentException("Unknown stage word index.");
            return new WordDefinition("vw1_"+locale+"_"+length+"_"+index,locale,words[index]);
        }
        public int[] Permutation(string locale,int length,string seed,int cycle)
        {
            string key=locale+":"+length;int count=Size(locale,length);if(count<3)throw new ArgumentException("Unknown word deck.");
            if(cycle==0&&familiar[key].Length>0){
                var first=familiar[key];var used=new HashSet<int>(first);var rest=Enumerable.Range(0,count).Where(i=>!used.Contains(i)).ToArray();
                int[] Shuffle(int[] values,string part)=>values.Length<3?values:StageShuffle.Permutation(values.Length,seed,"word:"+key+part,0).Select(i=>values[i]).ToArray();
                return Shuffle(first,":familiar").Concat(Shuffle(rest,":rest")).ToArray();
            }
            var deck=StageShuffle.Permutation(count,seed,"word:"+key,cycle);
            if(cycle==1&&deck[0]==Permutation(locale,length,seed,0)[count-1]){int value=deck[0];deck[0]=deck[1];deck[1]=value;}return deck;
        }
        public WordDefinition Draw(string locale,int length,string seed,int draw)
        {int count=Size(locale,length);if(count<3||draw<0||draw>1000000000)throw new ArgumentException("Invalid word draw.");return At(locale,length,Permutation(locale,length,seed,draw/count)[draw%count]);}
        public WordDefinition Find(string id)
        {
            var parts=id?.Split('_');if(parts?.Length!=4||parts[0]!="vw1"||!int.TryParse(parts[2],out int length)||!int.TryParse(parts[3],out int index))throw new ArgumentException("Unknown stage WordId.");
            var word=At(parts[1],length,index);if(word.WordId!=id)throw new ArgumentException("Noncanonical stage WordId.");return word;
        }
    }
    // Stable shuffled decks, independent of Unity/.NET Random implementation and of account services.
    public static class StageShuffle
    {
        static int[] Deck(int count,string seed,string key,int cycle)
        {
            uint state;using(var hash=SHA256.Create()){var bytes=hash.ComputeHash(Encoding.UTF8.GetBytes(seed+"|"+key+"|"+cycle));state=(uint)(bytes[0]|bytes[1]<<8|bytes[2]<<16|bytes[3]<<24);}
            if(state==0)state=0x9e3779b9;uint Next(){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
            var deck=Enumerable.Range(0,count).ToArray();for(int i=count-1;i>0;i--){int j=(int)(Next()%(uint)(i+1));int value=deck[i];deck[i]=deck[j];deck[j]=value;}return deck;
        }
        public static int[] Permutation(int count,string seed,string key,int cycle)
        {
            if(count<3||seed==null||seed.Length!=32||cycle<0)throw new ArgumentException("Invalid stage draw.");
            var deck=Deck(count,seed,key,cycle);
            if(cycle>0&&deck[0]==Deck(count,seed,key,cycle-1)[count-1]){int value=deck[0];deck[0]=deck[1];deck[1]=value;}
            return deck;
        }
        public static int Index(int count,string seed,string key,int draw){if(count<3||draw<0||draw>1000000000)throw new ArgumentException("Invalid stage draw.");return Permutation(count,seed,key,draw/count)[draw%count];}
    }
}
