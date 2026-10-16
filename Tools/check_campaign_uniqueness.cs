using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal static class CheckCampaignUniqueness
{
    static int assertions;
    static void Check(bool condition,string label)
    {if(!condition)throw new InvalidDataException("Uniqueness regression: "+label);assertions++;}

    static LevelDefinition Fixture(string id="fixture",string[] symbols=null,bool mirror=false,int hp=6,
        int ticks=2,int reach=1,bool repeated=false,int gustAfter=2)
    {
        var target=(symbols??new[]{"s:A","s:B","s:C","s:D"}).Select(s=>SymbolId.Parse(s)).ToArray();
        if(repeated)target[3]=target[1];
        var initial=(mirror?new[]{0,2,3,1}:new[]{2,0,1,3}).Select(i=>target[i]).ToArray();
        var rich=mirror?new FloorDefinition(hp,new[]{true,true,false},new[]{2},new LinkedPair(2,0),EntryTransform.ShiftRightOne,
            new BombDefinition(0,target[0],ticks,1),new KeyDefinition(2,target[2],2),new RepairDefinition(3,target[1],2),1,
            new ContactDefinition(1,target[3]),new PowerupDefinition(2,target[1],PowerupKind.Ribbon),gustAfter,EntryTransform.ShiftRightOne,1):
            new FloorDefinition(hp,new[]{false,true,true},new[]{0},new LinkedPair(0,2),EntryTransform.ShiftLeftOne,
            new BombDefinition(3,target[3],ticks,1),new KeyDefinition(1,target[1],0),new RepairDefinition(0,target[2],2),1,
            new ContactDefinition(2,target[0]),new PowerupDefinition(1,target[2],PowerupKind.Ribbon),gustAfter,EntryTransform.ShiftLeftOne,1);
        return new LevelDefinition(id,id,"2.0","2.0",id,initial,target,
            new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{new FloorDefinition(2,new[]{true,true,true}),rich},reach);
    }

    static void SelfTest()
    {
        var source=Fixture();string key=CampaignUniqueness.CanonicalStage(source);
        Check(key==CampaignUniqueness.CanonicalStage(Fixture(id:"different-label-and-hash")),"IDs and hashes are not gameplay");
        Check(key==CampaignUniqueness.CanonicalStage(Fixture(symbols:new[]{"g:ru:К","g:ru:О","g:ru:Т","g:ru:Ы"})),"renamed symbols/alphabets");
        Check(key==CampaignUniqueness.CanonicalStage(Fixture(mirror:true)),"independently authored reflected contacts, edges, linked pair and shifts");
        Check(key!=CampaignUniqueness.CanonicalStage(Fixture(hp:7)),"real durability difference");
        Check(key!=CampaignUniqueness.CanonicalStage(Fixture(ticks:3)),"bomb deadline difference");
        Check(key!=CampaignUniqueness.CanonicalStage(Fixture(reach:0)),"reach charge difference");
        Check(key!=CampaignUniqueness.CanonicalStage(Fixture(repeated:true)),"repeated symbols must retain their equality pattern");
        Check(key!=CampaignUniqueness.CanonicalStage(Fixture(gustAfter:3)),"gust trigger difference");
        Check(CampaignUniqueness.Reflected(EntryTransform.MirrorNext)==EntryTransform.MirrorNext,"mirror remains a mirror");
        Check(CampaignUniqueness.Reflected(EntryTransform.None)==EntryTransform.None,"no transform remains absent");
        var inactiveA=new FloorDefinition(2,new[]{true,true,true},gustTransform:EntryTransform.ShiftLeftOne,gustDamage:8);
        var inactiveB=new FloorDefinition(2,new[]{true,true,true},gustTransform:EntryTransform.ShiftRightOne,gustDamage:0);
        LevelDefinition Single(FloorDefinition floor)=>new LevelDefinition("x","x","2.0","2.0","x",source.InitialSymbolIds,
            source.TargetSymbolIds,source.TargetSegments,new[]{floor});
        Check(CampaignUniqueness.CanonicalStage(Single(inactiveA))==CampaignUniqueness.CanonicalStage(Single(inactiveB)),"disabled gust defaults do not create uniqueness");
        var normal=new[]{key,"other","third"};var reordered=new[]{"other","third",key};
        Check(CampaignUniqueness.Run(normal)!=CampaignUniqueness.Run(reordered),"ordered stage report");
        Check(CampaignUniqueness.Run(normal,true)==CampaignUniqueness.Run(reordered,true),"reordering three old stages cannot create a new level");
        Check(CampaignUniqueness.Run(new[]{"ab","c"})!=CampaignUniqueness.Run(new[]{"a","bc"}),"unambiguous stage boundaries");
        Check(CampaignUniqueness.Stage(Fixture(mirror:true))!=CampaignUniqueness.Stage(source),"fixture is actually reflected rather than identical");
        Check(CampaignUniqueness.Stage(Fixture(mirror:true))==CampaignUniqueness.Stage(source,true),"reflection normalizes every active field");
        var failed=new JObject { ["passed"]=false,["complete_duplicates"]=new JArray(new JArray("C01","C02")) };
        bool rejected=false;try{CampaignUniqueness.ThrowIfRepeated(failed);}catch(InvalidDataException){rejected=true;}
        Check(rejected,"publication rejects repeated levels");
        failed=new JObject { ["passed"]=false,["complete_duplicates"]=new JArray(),["stage_duplicates"]=new JArray(new JArray("C01:1","C02:2")) };
        rejected=false;try{CampaignUniqueness.ThrowIfRepeated(failed);}catch(InvalidDataException){rejected=true;}
        Check(rejected,"publication rejects an individual stage repeated inside different triads");
    }

    static void Main(string[] args)
    {
        SelfTest();int regressionAssertions=assertions;string root=args[0],folder=args[1];
        string Read(string name)=>File.ReadAllText(Path.Combine(root,"Data",name+".json"));
        var resourcePins=new JObject();
        foreach(string name in new[]{"levels","localized_levels","words_en","words_ru","words_uk","campaign_extension","production_triad_recipes","production_triad_manifest"}){
            string source=Read(name),packaged=File.ReadAllText(Path.Combine(root,"CascadeUnity","Assets","_Cascade","Content","Resources","CascadeContent",name+".json"));
            Check(source==packaged,"source and packaged content match: "+name);resourcePins[name]=PublishedTriadStore.Hash(source);
        }
        var catalog=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
        var store=new PublishedTriadStore(Read("production_triad_recipes"),Read("production_triad_manifest"));
        var report=CampaignUniqueness.Verify(catalog,store,out var entries);
        report["checker_regression_assertions"]=regressionAssertions;
        report["matching_source_resource_pairs"]=resourcePins;
        if(args.Length>2)report["revision_compatibility"]=CheckPrevious(catalog,root,args[2]);
        report["total_validation_assertions"]=assertions;
        report["checked_utc"]=DateTime.UtcNow.ToString("o");
        report["scope"]="Actual bound three-stage campaign levels, separately for every selectable category and puzzle locale. Symbol names/kinds, IDs, word segmentation and recipe labels are ignored; symbol equality and active mechanics are retained. Every stage is canonicalized with its horizontal reflection. Whole triads are also compared without stage order.";
        report["limits"]="Every individual bound stage must be structurally unique within each category/locale, including reflections and symbol renaming. This is not graph-isomorphism of every possible play path or a claim of human enjoyment.";
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder,"validation.json"),report.ToString(Formatting.Indented)+"\n");
        File.WriteAllText(Path.Combine(folder,"levels.json"),entries.ToString(Formatting.Indented)+"\n");
        foreach(var category in report["categories"])Console.WriteLine(category["mode"]+"/"+category["locale"]+": "+category["levels"]+
            " levels; "+category["unique_complete_levels"]+" unique; "+category["unique_even_without_stage_order"]+" unique even ignoring stage order.");
        CampaignUniqueness.ThrowIfRepeated(report);
        Console.WriteLine("PASS: "+assertions+" checker/package/save assertions; "+report["replayed_stage_certificates"]+" actual primary/localized stage certificates replayed; no repeated stage or complete level in any category.");
    }

    static PuzzleCommand Parse(string command)=>command=="D"?PuzzleCommand.Drop:command.StartsWith("R",StringComparison.Ordinal)
        ?PuzzleCommand.Reach(int.Parse(command.Substring(1).Split(':')[0]),int.Parse(command.Split(':')[1])):PuzzleCommand.Swap(int.Parse(command));

    static JObject CheckPrevious(ContentCatalog catalog,string root,string previous)
    {
        var old=CloudJson.Decode<Dictionary<string,DescentStage[]>>(File.ReadAllText(Path.Combine(previous,"production_triad_recipes.json")));
        var current=CloudJson.Decode<Dictionary<string,DescentStage[]>>(File.ReadAllText(Path.Combine(root,"Data","production_triad_recipes.json")));
        var archive=CloudJson.Decode<Dictionary<string,DescentStage[]>>(File.ReadAllText(Path.Combine(root,"Data","production_triad_recipes_legacy_oct09.json")));
        var changed=old.Keys.Where(n=>CloudJson.Encode(old[n])!=CloudJson.Encode(current[n])).ToArray();
        var rows=new JArray();int restored=0;
        foreach(string node in changed){
            Check(old[node][0].Triad.Lesson==current[node][0].Triad.Lesson,"retained principle: "+node);
            Check(current[node].Select((s,i)=>s.Minimum>=old[node][i].Minimum).All(x=>x),"no easier stage: "+node);
            Check(current[node].Select((s,i)=>s.Minimum<=old[node][i].Minimum+CampaignUniqueness.StageIncreaseAllowance(old[node][i].Minimum)).All(x=>x),"bounded individual-stage difficulty: "+node);
            Check(current[node].Sum(s=>s.Minimum)<=old[node].Sum(s=>s.Minimum)+CampaignUniqueness.LevelIncreaseAllowance(old[node].Sum(s=>s.Minimum)),"bounded whole-level difficulty: "+node);
            Check(archive.Any(p=>(p.Key==node||p.Key.StartsWith(node+"@",StringComparison.Ordinal))&&CloudJson.Encode(p.Value)==CloudJson.Encode(old[node])),"exact old recipe archived: "+node);
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Where(c=>c.Definition.CampaignNodeId==node)){
                var source=content.Definition;
                var run=new DescentRun {Version=6,SourceHash=source.ContentHash,Stages=CloudJson.Decode<DescentStage[]>(CloudJson.Encode(old[node]))};
                run.Validate(source,content.Locale);
                var first=new AttemptService(run.Level(source,0),content.Locale);
                foreach(string command in run.Stages[0].Witness)first.Execute(Parse(command));
                run.AcceptCheckpoint(source,first);
                var second=new AttemptService(run.Level(source,1),content.Locale);second.Execute(Parse(run.Stages[1].Witness[0]));
                string directory=Path.Combine(Path.GetTempPath(),"CascadeUniquenessSave-"+Guid.NewGuid().ToString("N"));
                try{
                    Directory.CreateDirectory(directory);var store=new ProfileStore(Path.Combine(directory,"profile.json"));
                    store.Save(new PlayerProfile {Descent=run,Attempt=AttemptRecord.Capture(second)});
                    var loaded=store.Load().Profile;var restoredAttempt=loaded.RestoreAttempt(catalog);
                    Check(loaded.Descent.ContentHash==run.ContentHash&&restoredAttempt.CurrentStateHash==second.CurrentStateHash,"old checkpoint restores after publication: "+content.Definition.PuzzleId);
                    bool expectedUndo=second.Undo();
                    Check(restoredAttempt.Undo()==expectedUndo&&restoredAttempt.CurrentStateHash==second.CurrentStateHash,"old checkpoint retains its actual undo availability and state: "+content.Definition.PuzzleId);restored++;
                }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
            }
            rows.Add(new JObject {["node"]=node,["old_stage_minima"]=new JArray(old[node].Select(s=>s.Minimum)),["new_stage_minima"]=new JArray(current[node].Select(s=>s.Minimum)),["principle"]=old[node][0].Triad.Lesson});
        }
        return new JObject { ["changed_nodes"]=rows,["unchanged_nodes"]=old.Count-changed.Length,["restored_old_primary_localized_checkpoints"]=restored };
    }
}
