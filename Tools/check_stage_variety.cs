using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
class StageVarietyAudit
{
    static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    static void Main(string[] args)
    {
        string root=args[0];string Data(string name)=>File.ReadAllText(Path.Combine(root,"Data",name+".json"));
        var catalog=ContentImporter.Import(Data("levels"),Data("localized_levels"),Data("words_en"),Data("words_ru"),Data("words_uk"),Data("campaign_extension"));
        string wordJson=Data("stage_words_v1");var pool=new StageWordPool(wordJson);var manifest=new PublishedTriadStore(Data("production_triad_recipes"),Data("production_triad_manifest"));
        var extras=new Dictionary<string,DescentStage[]>();
        if(args.Contains("--bake")){
            foreach(var content in catalog.PrimaryLevels)foreach(string mode in new[]{"Shapes","Words","Mixed"}){
                var source=content.Definition;string key=StageContentBasis.Key(source,mode);if(StageContentBasis.IsRenaming(source,mode)||extras.ContainsKey(key))continue;
                Console.WriteLine("Certifying "+key);var basis=StageContentBasis.Canonical(source,mode);var run=CampaignTriads.Build(basis,CancellationToken.None,60000);Require(run!=null,"Missing canonical alternative "+key);run.Validate(basis,"en");extras.Add(key,run.Stages);
            }
            string json=CloudJson.Encode(extras);string target=Path.Combine(root,"Data/stage_variety_templates_v1.json");File.WriteAllText(target,json);File.WriteAllText(Path.Combine(root,"CascadeUnity/Assets/_Cascade/Content/Resources/CascadeContent/stage_variety_templates_v1.json"),json);Console.WriteLine("Baked "+extras.Count+" canonical alternative triads.");return;
        }
        var store=new StageVarietyStore(wordJson,Data("stage_variety_templates_v1"),Data("production_triad_recipes"));
        int stages=0,minimums=0;var duplicates=new List<string>();var reports=new List<object>();
        foreach(string locale in new[]{"en","ru","uk"}){
            var memory=new StageVarietyMemory{Seed="0123456789abcdef0123456789abcdef"};var words=new HashSet<string>();var shapes=new HashSet<string>();
            foreach(string mode in new[]{"Words","Mixed","Shapes"})foreach(var content in catalog.PrimaryLevels){
                var source=content.Definition;string activeLocale=mode=="Shapes"?"en":locale;var baseline=manifest.Bind(source,content.Locale,catalog.ContentHash);Require(baseline!=null,"Published source missing: "+source.PuzzleId);
                var plan=memory.Reserve(source,mode,activeLocale,pool);var run=store.Create(source,plan,baseline);
                Require(run.Version==7&&run.Variation.IsBounded(),"Missing personalised stage proof.");
                for(int stage=0;stage<3;stage++){
                    var level=run.Level(source,stage);var symbols=run.Variation.Symbols[stage];Require(level.TargetSymbolIds.Count>=3&&level.TargetSymbolIds.Count<=8,"Invalid width.");
                    if(mode!="Shapes")Require(words.Add(symbols.WordId),"Repeated word before the full campaign was traversed: "+locale+" "+mode+" "+source.CampaignNodeId+" "+stage+" "+symbols.Word);
                    else Require(shapes.Add(string.Join(",",symbols.Target)),"Repeated figure target in campaign: "+source.CampaignNodeId+" "+stage);
                    if(mode=="Mixed")Require(level.TargetSymbolIds.Any(s=>s.Kind==SymbolKind.Shape)&&level.TargetSymbolIds.Count(s=>s.Kind==SymbolKind.Glyph)>=3,"Mixed must always contain a real word and figures.");
                    var attempt=new AttemptService(level,activeLocale);foreach(string command in run.Stages[stage].Witness){var parts=command.StartsWith("R",StringComparison.Ordinal)?command.Substring(1).Split(':'):null;var cmd=command=="D"?PuzzleCommand.Drop:int.TryParse(command,out int edge)?PuzzleCommand.Swap(edge):parts?.Length==2?PuzzleCommand.Reach(int.Parse(parts[0]),int.Parse(parts[1])):throw new Exception("Unsupported witness command");Require(attempt.Execute(cmd).Accepted,"Broken stage witness.");}
                    Require(attempt.Current.Status==PuzzleStatus.Won,"Stage witness did not win.");
                    if(args.Contains("--solve")){var proof=PuzzleSolver.FindShortest(new PuzzleResolver(level),stateBudget:60000);Require(proof.Status==SearchStatus.Solved&&proof.OptimalActions==run.Stages[stage].Minimum,"Stage minimum differs: "+source.PuzzleId);minimums++;}
                    stages++;
                }
                reports.Add(new{Locale=activeLocale,Mode=mode,Node=source.CampaignNodeId,Words=run.Variation.Symbols.Select(s=>s.Word).ToArray(),Targets=run.Variation.Symbols.Select(s=>s.Target).ToArray()});
            }
            memory.Validate();Require(CloudJson.Encode(memory).Length<20000,"Word history must remain compact.");
        }
        string folder=Path.Combine(root,"Reports/StageVarietyOct29");Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"targets.json"),CloudJson.Encode(reports));
        Console.WriteLine("PASS: "+pool.Count+" words; "+stages+" stage witness replays; "+minimums+" optimal solver checks; no repeated campaign words or figure targets; all Mixed stages contain words and figures.");
    }
}
