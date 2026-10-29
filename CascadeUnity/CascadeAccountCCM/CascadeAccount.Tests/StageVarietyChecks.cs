using System.Reflection;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using CascadeAccountModule;
public static class StageVarietyChecks
{
    static string Resource(string name){using var stream=typeof(PublishedCatalog).Assembly.GetManifestResourceStream("CascadeAccount.Published."+name+".json")!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
    static PuzzleCommand Parse(string value)=>value=="D"?PuzzleCommand.Drop:value.StartsWith("R")?PuzzleCommand.Reach(int.Parse(value[1..].Split(':')[0]),int.Parse(value.Split(':')[1])):PuzzleCommand.Swap(int.Parse(value));
    public static void Run()
    {
        int checks=0;void Check(bool value,string message){if(!value)throw new Exception(message);checks++;}
        var store=new StageVarietyStore(Resource("stage_words_v1"),Resource("stage_variety_templates_v1"),Resource("recipes"));
        foreach(string mode in new[]{"Shapes","Words","Mixed"})foreach(string locale in mode=="Shapes"?new[]{"en"}:new[]{"en","ru","uk"})foreach(string node in new[]{"C01","C05","C26","C30","C32","C540"}){
            var source=PublishedCatalog.Primary(node).Definition;var memory=new StageVarietyMemory{Seed="123456789abcdef0123456789abcdef0"};
            var run=store.Create(source,memory.Reserve(source,mode,locale,store.Words),PublishedCatalog.Run(source));AttemptService final=null;
            for(int i=0;i<3;i++){final=new AttemptService(run.Level(source,i),locale);final.AdvanceActiveTime(2);foreach(var command in run.Stages[i].Witness)Check(final.Execute(Parse(command)).Accepted,"A permitted stage witness must execute");if(i<2)run.AcceptCheckpoint(source,final);}
            var result=LevelCompletion.From(final,null,run.Performance(final));var profile=new PlayerProfile{Descent=run,PreferredMode=mode,Variety=memory};profile.RecordClear(final,result);var clear=profile.Clears.Single();
            Check(PublishedCatalog.ValidClear(clear,mode),"Server accepts actual personalised three-stage clear "+node+mode+locale);
            var tampered=AccountJson.Clone(clear);tampered.Replay.Variation.PoolHash=new string('f',64);Check(!PublishedCatalog.ValidClear(tampered,mode),"Unknown word pools cannot grant a clear");
            tampered=AccountJson.Clone(clear);tampered.Replay.Variation.Seed="broken";Check(!PublishedCatalog.ValidClear(tampered,mode),"Invalid shuffle metadata must reject safely");
            tampered=AccountJson.Clone(clear);tampered.Replay.Stages[0].Commands=Array.Empty<string>();Check(!PublishedCatalog.ValidClear(tampered,mode),"Vocabulary is not a substitute for replaying the puzzle");
            if(mode!="Shapes"){
                tampered=AccountJson.Clone(clear);tampered.Replay.Variation.WordDraws[0]++;Check(!PublishedCatalog.ValidClear(tampered,mode),"The word must match the exact allowed draw");
                tampered=AccountJson.Clone(clear);tampered.Replay.Variation.Symbols[0].Target[0]="g:"+locale+":X";Check(!PublishedCatalog.ValidClear(tampered,mode),"Substituted target symbols cannot grant a clear");
            }
            Check(!PublishedCatalog.ValidClear(clear,mode=="Shapes"?"Words":"Shapes"),"Modes cannot borrow each other's clears");
            profile.Validate();memory.ValidatePool(store.Words);
        }
        Console.WriteLine("Stage vocabulary server checks: "+checks+" PASS; actual reconstruction, no live service.");
    }
}
