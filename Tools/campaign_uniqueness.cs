using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Authoring/validation only: never adds work to the player or changes saved recipes.
internal static class CampaignUniqueness
{
    internal static int StageIncreaseAllowance(int minimum)=>Math.Max(4,minimum/3);
    internal static int LevelIncreaseAllowance(int minimum)=>Math.Max(9,minimum/3);
    internal sealed class Category
    {
        public string Mode, Locale;
        public LevelContent[] Levels;
    }

    // Keep aligned with CascadeGame.PreferredLevels, including the alternating Mixed route.
    internal static IEnumerable<Category> Categories(ContentCatalog catalog)
    {
        yield return new Category { Mode="Shapes", Locale="all",
            Levels=catalog.PrimaryLevels.Where(c=>c.Mode==PuzzleMode.Shapes).ToArray() };
        foreach(string locale in new[]{"en","ru","uk"}) {
            yield return new Category { Mode="Words", Locale=locale,
                Levels=catalog.LocalizedLevels.Where(c=>c.Locale==locale&&c.Mode==PuzzleMode.Words).ToArray() };
            yield return new Category { Mode="Mixed", Locale=locale,
                Levels=MixedJourney.Sequence(catalog,locale) };
        }
    }

    internal static EntryTransform Reflected(EntryTransform transform)
        =>transform==EntryTransform.ShiftLeftOne?EntryTransform.ShiftRightOne:
          transform==EntryTransform.ShiftRightOne?EntryTransform.ShiftLeftOne:transform;

    // Equality of symbols matters; their names, alphabet, art and word IDs do not.
    // Only bound, active gameplay fields are compared. Recipe labels/proofs and absent
    // contacts' unused integer defaults cannot disguise a duplicate.
    internal static string Stage(LevelDefinition level,bool reflected=false)
    {
        int width=level.TargetSymbolIds.Count;
        var names=new Dictionary<SymbolId,int>();
        int Symbol(SymbolId symbol) {
            if(!names.TryGetValue(symbol,out int value)){value=names.Count;names.Add(symbol,value);}
            return value;
        }
        IEnumerable<SymbolId> Row(IEnumerable<SymbolId> row)=>reflected?row.Reverse():row;
        int Edge(int edge)=>reflected?width-2-edge:edge;
        int Slot(int slot)=>reflected?width-1-slot:slot;
        int Transform(EntryTransform value)=>(int)(reflected?Reflected(value):value);
        var target=new JArray(Row(level.TargetSymbolIds).Select(Symbol));
        var initial=new JArray(Row(level.InitialSymbolIds).Select(Symbol));
        var floors=new JArray();
        foreach(var floor in level.Floors) {
            JArray Contact(ContactDefinition contact)=>contact==null?null:
                new JArray(Slot(contact.Slot),Symbol(contact.RequiredSymbol));
            var board=new JObject {
                ["hp"]=floor.BaseDurability,
                ["open"]=new JArray(reflected?floor.OpenEdges.Reverse():floor.OpenEdges),
                ["fragile"]=new JArray(floor.FragileEdges.Select(Edge).OrderBy(e=>e)),
                ["entry"]=Transform(floor.EntryTransform)
            };
            if(floor.LinkedPair.HasValue)board["linked"]=new JArray(new[]{
                Edge(floor.LinkedPair.Value.FirstEdge),Edge(floor.LinkedPair.Value.SecondEdge)}.OrderBy(e=>e));
            if(floor.OneUseBridgeEdge.HasValue)board["bridge"]=Edge(floor.OneUseBridgeEdge.Value);
            if(floor.Bomb!=null){var value=Contact(floor.Bomb);value.Add(floor.Bomb.Ticks);value.Add(floor.Bomb.Damage);board["bomb"]=value;}
            if(floor.Key!=null){var value=Contact(floor.Key);value.Add(Edge(floor.Key.EdgeToOpen));board["key"]=value;}
            if(floor.Repair!=null){var value=Contact(floor.Repair);value.Add(floor.Repair.Amount);board["repair"]=value;}
            if(floor.OptionalStamp!=null)board["stamp"]=Contact(floor.OptionalStamp);
            if(floor.Powerup!=null){var value=Contact(floor.Powerup);value.Add((int)floor.Powerup.Kind);board["power"]=value;}
            if(floor.GustAfterSwaps>0)board["gust"]=new JArray(floor.GustAfterSwaps,Transform(floor.GustTransform),floor.GustDamage);
            floors.Add(board);
        }
        return new JObject { ["target"]=target,["start"]=initial,["reach"]=level.ReachCharges,["floors"]=floors }.ToString(Formatting.None);
    }

    internal static string CanonicalStage(LevelDefinition level)
    {
        string direct=Stage(level),reflected=Stage(level,true);
        return string.CompareOrdinal(direct,reflected)<=0?direct:reflected;
    }

    internal static string Run(IEnumerable<string> stages,bool unordered=false)
        =>new JArray(unordered?stages.OrderBy(s=>s,StringComparer.Ordinal):stages).ToString(Formatting.None);

    internal static string[] Keys(LevelDefinition source,DescentStage[] stages)
    {
        var run=new DescentRun { Version=6,SourceHash=source.ContentHash,Stages=stages };
        return Enumerable.Range(0,3).Select(i=>CanonicalStage(run.Level(source,i))).ToArray();
    }

    internal static JObject Verify(ContentCatalog catalog,PublishedTriadStore store,out JArray entries)
    {
        var stageKeys=new Dictionary<string,string[]>(StringComparer.Ordinal);
        foreach(var content in catalog.AllLevels) {
            var run=store.Bind(content.Definition,content.Locale,catalog.ContentHash);
            if(run==null)throw new InvalidDataException("Unbound campaign level: "+content.Definition.PuzzleId);
            stageKeys.Add(content.Definition.PuzzleId,Keys(content.Definition,run.Stages));
        }
        entries=new JArray();var categories=new JArray();var duplicates=new JArray();var stageDuplicates=new JArray();
        foreach(var category in Categories(catalog)) {
            var ordered=new Dictionary<string,List<string>>(StringComparer.Ordinal);
            var unordered=new Dictionary<string,List<string>>(StringComparer.Ordinal);
            var stages=new Dictionary<string,List<string>>(StringComparer.Ordinal);
            void Add(Dictionary<string,List<string>> groups,string key,string label) {
                if(!groups.TryGetValue(key,out var group)){group=new List<string>();groups.Add(key,group);}group.Add(label);
            }
            int position=0;
            foreach(var content in category.Levels) {
                string id=content.Definition.PuzzleId;var keys=stageKeys[id];string key=Run(keys);
                Add(ordered,key,id);Add(unordered,Run(keys,true),id);
                for(int stage=0;stage<3;stage++)Add(stages,keys[stage],id+":"+(stage+1));
                entries.Add(new JObject { ["mode"]=category.Mode,["locale"]=category.Locale,
                    ["position"]=++position,["node"]=content.Definition.CampaignNodeId,["puzzle"]=id,
                    ["canonical_sha256"]=PublishedTriadStore.Hash(key),
                    ["stage_sha256"]=new JArray(keys.Select(PublishedTriadStore.Hash)) });
            }
            var repeated=stages.Values.Where(g=>g.Count>1).ToArray();
            foreach(var group in repeated)stageDuplicates.Add(new JObject {
                ["mode"]=category.Mode,["locale"]=category.Locale,["stages"]=new JArray(group) });
            foreach(var group in unordered.Values.Where(g=>g.Count>1))duplicates.Add(new JObject {
                ["mode"]=category.Mode,["locale"]=category.Locale,["levels"]=new JArray(group) });
            categories.Add(new JObject { ["mode"]=category.Mode,["locale"]=category.Locale,
                ["levels"]=category.Levels.Length,["unique_complete_levels"]=ordered.Count,
                ["unique_even_without_stage_order"]=unordered.Count,
                ["complete_duplicate_groups"]=ordered.Values.Count(g=>g.Count>1),
                ["repeated_stage_groups"]=repeated.Length,
                ["repeated_stage_occurrences_beyond_first"]=repeated.Sum(g=>g.Count-1),
                ["repeated_stages"]=new JArray(repeated.Select(g=>new JArray(g))) });
        }
        return new JObject { ["catalog_hash"]=catalog.ContentHash,["primary_nodes"]=catalog.PrimaryLevels.Count,
            ["bound_puzzles"]=stageKeys.Count,["replayed_stage_certificates"]=stageKeys.Count*3,
            ["categories"]=categories,["complete_duplicates"]=duplicates,["stage_duplicates"]=stageDuplicates,
            ["passed"]=duplicates.Count==0&&stageDuplicates.Count==0 };
    }

    internal static void ThrowIfRepeated(JObject report)
    {
        if(!(bool)report["passed"])throw new InvalidDataException(
            "Repeated campaign stages or levels (renaming, reflection and stage reordering ignored): "+
            (report["stage_duplicates"]??report["complete_duplicates"]).ToString(Formatting.None));
    }
}
