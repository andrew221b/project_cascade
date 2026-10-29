using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Content
{
    // A symbol bijection preserves every puzzle transition and published witness.
    public static class MixedJourney
    {
        public const string Revision="mixed-rows-v1";
        internal static void AddPronouns(IDictionary<string,WordEntry> entries)
        {
            foreach(string locale in new[]{"en","ru","uk"}){
                string id="mix_"+locale+"_pronoun",text=locale=="en"?"I":"Я";
                entries.Add(id,new WordEntry(new WordDefinition(id,locale,text),"approved","Basic pronoun; authored for repeated-symbol Mix lessons","Common word; no third-party text"));
            }
        }
        public static LevelContent[] Sequence(ContentCatalog catalog,string locale)
        {
            if(catalog==null)throw new ArgumentNullException(nameof(catalog));
            if(locale!="en"&&locale!="ru"&&locale!="uk")throw new ArgumentOutOfRangeException(nameof(locale));
            var bindings=catalog.MixedLevels.Where(c=>c.Locale==locale).ToDictionary(c=>c.Definition.CampaignNodeId);
            int shortLevel=0;
            return catalog.PrimaryLevels.Select(primary=>{
                catalog.TryGetLocalizedBinding(primary.Definition.CampaignNodeId,locale,out var localized);
                if(primary.Definition.TargetSymbolIds.Count==3)return shortLevel++%2==0?primary:localized??primary;
                if(localized?.Mode==PuzzleMode.Mixed)return localized;
                return bindings.TryGetValue(primary.Definition.CampaignNodeId,out var mixed)?mixed:localized??primary;
            }).ToArray();
        }
        internal static LevelContent[] CreateBindings(ContentCatalog catalog)
        {
            var result=new List<LevelContent>();int position=0;
            foreach(var primary in catalog.PrimaryLevels){
                int width=primary.Definition.TargetSymbolIds.Count;
                if(width<4){position++;continue;}
                foreach(string locale in new[]{"en","ru","uk"}){
                    if(catalog.TryGetLocalizedBinding(primary.Definition.CampaignNodeId,locale,out var existing)&&existing.Mode==PuzzleMode.Mixed)continue;
                    result.Add(Bind(catalog,primary,locale,position));
                }
                position++;
            }
            return result.ToArray();
        }
        static LevelContent Bind(ContentCatalog catalog,LevelContent content,string locale,int position)
        {
            var source=content.Definition;int width=source.TargetSymbolIds.Count;
            var mapping=source.TargetSymbolIds.Distinct().Select((symbol,index)=>new{symbol,shape=SymbolId.Parse("s:"+(char)('A'+index))}).ToDictionary(p=>p.symbol,p=>p.shape);
            WordDefinition word;
            if(mapping.Count==width){
                int length=width==4?3:width-1-position%(width-3);
                var choices=catalog.Words.Values.Where(w=>w.Locale==locale&&w.Graphemes.Count==length&&w.Graphemes.Distinct().Count()==length).OrderBy(w=>w.WordId,StringComparer.Ordinal).ToArray();
                if(choices.Length==0)throw new InvalidOperationException("No distinct-letter Mix word: "+locale+" / "+length);
                word=choices[position%choices.Length];int first=position%2==0?0:width-length;
                for(int i=0;i<length;i++)mapping[source.TargetSymbolIds[first+i]]=SymbolId.Parse("g:"+locale+":"+word.Graphemes[i]);
            }else{
                word=catalog.Words["mix_"+locale+"_pronoun"];
                // Repeated-shape lessons retain their exact equality pattern and difficulty.
                mapping[source.TargetSymbolIds.Last()]=SymbolId.Parse("g:"+locale+":"+word.Graphemes[0]);
            }
            SymbolId Map(SymbolId symbol)=>mapping[symbol];
            var target=source.TargetSymbolIds.Select(Map).ToArray();var segments=new List<TargetSegment>();
            for(int i=0;i<target.Length;){
                if(target[i].Kind==SymbolKind.Shape){int end=i+1;while(end<target.Length&&target[end].Kind==SymbolKind.Shape)end++;segments.Add(new TargetSegment(SegmentKind.Shapes,i,end-i));i=end;}
                else{segments.Add(new TargetSegment(SegmentKind.Word,i,word.Graphemes.Count,word.WordId));i+=word.Graphemes.Count;}
            }
            ContactDefinition Contact(ContactDefinition c)=>c==null?null:new ContactDefinition(c.Slot,Map(c.RequiredSymbol));
            var floors=source.Floors.Select(f=>new FloorDefinition(f.BaseDurability,f.OpenEdges,f.FragileEdges,f.LinkedPair,f.EntryTransform,
                f.Bomb==null?null:new BombDefinition(f.Bomb.Slot,Map(f.Bomb.RequiredSymbol),f.Bomb.Ticks,f.Bomb.Damage),
                f.Key==null?null:new KeyDefinition(f.Key.Slot,Map(f.Key.RequiredSymbol),f.Key.EdgeToOpen),
                f.Repair==null?null:new RepairDefinition(f.Repair.Slot,Map(f.Repair.RequiredSymbol),f.Repair.Amount),f.OneUseBridgeEdge,Contact(f.OptionalStamp),
                f.Powerup==null?null:new PowerupDefinition(f.Powerup.Slot,Map(f.Powerup.RequiredSymbol),f.Powerup.Kind),f.GustAfterSwaps,f.GustTransform,f.GustDamage));
            string payload=Revision+"|"+source.ContentHash+"|"+locale+"|"+string.Join(",",target.Select(s=>s.PersistedId))+"|"+word.WordId+"|"+word.DisplayText;
            var level=new LevelDefinition(source.CampaignNodeId+"_mix_"+locale,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,ContentImporter.Hash(payload),
                source.InitialSymbolIds.Select(Map),target,segments,floors,source.ReachCharges,source.TeachingNode);
            LevelValidator.ThrowIfInvalid(level,catalog.Words);
            return new LevelContent(level,locale,content.Title,content.Lesson,false);
        }
    }
}
