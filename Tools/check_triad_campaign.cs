using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
class Audit
{
    static void Main(string[] args)
    {
        string root=args[0],output=Path.Combine(root,"Reports","LevelTriads-Oct08");Directory.CreateDirectory(output);
        string Read(string file)=>File.ReadAllText(Path.Combine(root,"Data",file));
        var catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
        var rows=new List<string>{"node,stage,lesson,locale,row,boards,minimum,witness"};int count=0;long maximum=0;
        foreach(string file in new[]{"audit-failure.txt","audit-full.txt"}){var path=Path.Combine(output,file);if(File.Exists(path))File.Delete(path);}
        foreach(var content in catalog.PrimaryLevels){
            var time=System.Diagnostics.Stopwatch.StartNew();
            try{
                var run=DescentRun.Build(content.Definition,version:6);run.Validate(content.Definition,content.Locale);Record(content,run);
                foreach(var binding in catalog.LocalizedLevels.Where(c=>c.Definition.CampaignNodeId==content.Definition.CampaignNodeId)){
                    var clone=new DescentRun{Version=6,SourceHash=binding.Definition.ContentHash,Stages=run.Stages};clone.Validate(binding.Definition,binding.Locale);Record(binding,clone);
                }
                count++;maximum=Math.Max(maximum,time.ElapsedMilliseconds);
                if(count%25==0)Console.WriteLine(count+" primary triads / "+(rows.Count-1)+" stages; slowest "+maximum+" ms");
                File.WriteAllText(Path.Combine(output,"audit-progress.txt"),count+" primary triads; "+(rows.Count-1)+" stages; latest "+content.Definition.CampaignNodeId+" "+time.ElapsedMilliseconds+" ms; slowest "+maximum+" ms\n");
            }catch(Exception e){File.WriteAllLines(Path.Combine(output,"partial-proofs.csv"),rows);File.WriteAllText(Path.Combine(output,"audit-failure.txt"),e.ToString());Console.WriteLine(e);Environment.ExitCode=1;return;}
        }
        File.WriteAllLines(Path.Combine(output,"all-triad-proofs.csv"),rows);File.WriteAllText(Path.Combine(output,"audit-full.txt"),"PASS: "+count+" primary triads, "+(rows.Count-1)+" primary/localized stages, exact production resolver replay. Slowest primary plus its localized replays: "+maximum+" ms.\n");
        Console.WriteLine("PASS "+count+" primary / "+(rows.Count-1)+" stages");
        void Record(LevelContent content,DescentRun run){
            for(int stage=0;stage<3;stage++){
                var recipe=run.Stages[stage];var level=run.Level(content.Definition,stage);
                if(!CampaignTriads.UsesPrinciple(level,recipe.Witness.Select(Parse),(MechanicLesson)recipe.Triad.Lesson))throw new Exception("Missing principle "+level.PuzzleId+" "+stage);
                rows.Add(string.Join(",",level.CampaignNodeId,stage+1,(MechanicLesson)recipe.Triad.Lesson,content.Locale,level.TargetSymbolIds.Count,level.Floors.Count,recipe.Minimum,"\""+string.Join(" ",recipe.Witness)+"\""));
            }
        }
    }
    static PuzzleCommand Parse(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R")?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
}
