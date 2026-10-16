using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;

namespace Cascade.Editor.Playable
{
    public static class CampaignTriadAudit
    {
        static Task audit;
        static CancellationTokenSource cancel;
        public static void Start(bool full=false)
        {
            if(audit!=null&&!audit.IsCompleted)throw new InvalidOperationException("An audit is already running.");
            string root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            string Read(string file)=>File.ReadAllText(Path.Combine(root,"Data",file));
            var catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
            string folder=Path.Combine(root,"Reports","LevelTriads-Oct08");Directory.CreateDirectory(folder);
            foreach(string file in new[]{"audit-failure.txt","audit-progress.txt",full?"audit-full.txt":"audit-intro.txt"}){string path=Path.Combine(folder,file);if(File.Exists(path))File.Delete(path);}
            cancel=new CancellationTokenSource();var token=cancel.Token;
            audit=Task.Run(()=>{
                var rows=new List<string>{"node,stage,lesson,locale,row,boards,minimum,witness"};int count=0;
                try{
                    var sources=full?catalog.PrimaryLevels:catalog.PrimaryLevels.Where(c=>int.Parse(c.Definition.CampaignNodeId.Substring(1))<=14);
                    foreach(var source in sources){
                        var clock=System.Diagnostics.Stopwatch.StartNew();
                        var run=DescentRun.Build(source.Definition,cancellation:token,version:6);
                        if(run==null)throw new InvalidOperationException("No proved triad for "+source.Definition.CampaignNodeId+" / "+CampaignTriads.Primary(source.Definition));
                        run.Validate(source.Definition,source.Locale);
                        Record(source,run);count++;
                        if(full)foreach(var translated in catalog.LocalizedLevels.Where(c=>c.Definition.CampaignNodeId==source.Definition.CampaignNodeId)){
                            var localized=new DescentRun{Version=6,SourceHash=translated.Definition.ContentHash,Stages=run.Stages};
                            localized.Validate(translated.Definition,translated.Locale);Record(translated,localized);
                        }
                        File.WriteAllText(Path.Combine(folder,"audit-progress.txt"),count+" primary levels; latest "+source.Definition.CampaignNodeId+" "+clock.ElapsedMilliseconds+" ms\n");
                    }
                    File.WriteAllLines(Path.Combine(folder,full?"all-triad-proofs.csv":"intro-triad-proofs.csv"),rows);
                    File.WriteAllText(Path.Combine(folder,full?"audit-full.txt":"audit-intro.txt"),"PASS: "+count+" primary triads; "+(rows.Count-1)+" stages replayed through the production resolver, including localized bindings.\n");
                }catch(Exception e){File.WriteAllLines(Path.Combine(folder,"partial-proofs.csv"),rows);File.WriteAllText(Path.Combine(folder,"audit-failure.txt"),e.ToString());throw;}
                void Record(LevelContent content,DescentRun run){
                    for(int stage=0;stage<3;stage++){
                        var recipe=run.Stages[stage];var level=run.Level(content.Definition,stage);
                        var commands=recipe.Witness.Select(Parse).ToArray();
                        if(!CampaignTriads.UsesPrinciple(level,commands,(MechanicLesson)recipe.Triad.Lesson))throw new InvalidOperationException("Missing causal mechanic: "+level.PuzzleId);
                        rows.Add(string.Join(",",level.CampaignNodeId,stage+1,(MechanicLesson)recipe.Triad.Lesson,content.Locale,level.TargetSymbolIds.Count,level.Floors.Count,recipe.Minimum,"\""+string.Join(" ",recipe.Witness)+"\""));
                    }
                }
            },token);
        }
        public static string Status=>audit==null?"Idle":audit.IsFaulted?audit.Exception.InnerException.Message:audit.IsCompleted?"Complete":"Running";
        public static void Stop(){cancel?.Cancel();}
        public static PuzzleCommand Parse(string command)=>command=="D"?PuzzleCommand.Drop:command.StartsWith("R",StringComparison.Ordinal)
            ?PuzzleCommand.Reach(int.Parse(command.Substring(1).Split(':')[0]),int.Parse(command.Split(':')[1])):PuzzleCommand.Swap(int.Parse(command));
    }
}
