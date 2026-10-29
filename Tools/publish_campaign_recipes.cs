using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using System.Threading;
class PublishCampaignRecipes
{
    static void Main(string[] args)
    {
        string root=args[0];bool check=args.Contains("--check");
        string Read(string file)=>File.ReadAllText(Path.Combine(root,"Data",file+".json"));
        var catalog=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
        string recipeJson,manifestJson,legacyJson=null;
        if(check){recipeJson=Read("production_triad_recipes");manifestJson=Read("production_triad_manifest");}
        else {
            bool repin=args.Contains("--repin-sources"),opening=args.Contains("--refresh-opening"),deduplicate=args.Contains("--deduplicate"),pacing=args.Contains("--refresh-pacing"),distinctStages=args.Contains("--deduplicate-stages");
            var recipes=repin||opening||deduplicate||pacing||distinctStages?CloudJson.Decode<Dictionary<string,DescentStage[]>>(Read("production_triad_recipes")):new Dictionary<string,DescentStage[]>();
            foreach(var content in repin||deduplicate||pacing||distinctStages?Enumerable.Empty<LevelContent>():opening?catalog.PrimaryLevels.Take(14):catalog.PrimaryLevels){
                var run=DescentRun.Build(content.Definition,version:6);
                if(run==null)throw new InvalidDataException("Missing recipe for "+content.Definition.CampaignNodeId);
                run.Validate(content.Definition,content.Locale);recipes[content.Definition.CampaignNodeId]=run.Stages;
            }
            if(pacing){
                var legacy=CloudJson.Decode<Dictionary<string,DescentStage[]>>(Read("production_triad_recipes_legacy_oct09"));
                string Fingerprint(DescentStage[] stages)=>string.Join("|",stages.Select(s=>s.Triad.Fingerprint));
                var used=new HashSet<string>(recipes.Values.Select(Fingerprint));int previous=-1,streak=0,position=0,changed=0;
                foreach(var content in catalog.PrimaryLevels){
                    var source=content.Definition;string node=source.CampaignNodeId;var old=recipes[node];
                    int lead=old[0].Triad.Lesson,total=old.Sum(s=>s.Minimum),world=position++/60;
                    int target=world==3?Math.Max(total,14):total;
                    var choices=CampaignTriads.AuthoredPrinciples(source).OrderBy(l=>(int)l==lead?1:0).ToArray();
                    bool repeat=streak>=2&&previous==lead&&choices.Any(l=>(int)l!=lead);
                    if(!repeat&&target==total){streak=previous==lead?streak+1:1;previous=lead;continue;}
                    DescentRun chosen=null,fallback=null;
                    foreach(var family in choices){
                        if(repeat&&(int)family==previous)continue;
                        for(int variant=1;variant<=16&&chosen==null;variant++){
                            using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(12));DescentRun candidate;
                            try{candidate=CampaignTriads.BuildCampaignRevision(source,family,variant,new DescentRun{Version=6,SourceHash=source.ContentHash,Stages=old},stop.Token);}
                            catch(Exception error)when(error is InvalidOperationException||error is OperationCanceledException){continue;}
                            int sum=candidate.Stages.Sum(s=>s.Minimum);
                            if(sum<total||sum>Math.Max(target+2,total+Math.Max(4,total/3))||used.Contains(Fingerprint(candidate.Stages)))continue;
                            candidate.Validate(source,content.Locale);
                            if(fallback==null||sum>fallback.Stages.Sum(s=>s.Minimum))fallback=candidate;
                            if(sum>=target)chosen=candidate;
                        }
                        if(chosen!=null)break;
                    }
                    chosen=chosen??fallback;
                    if(chosen==null){Console.WriteLine("Retained certified recipe: "+node+"; no harder authored alternative.");streak=previous==lead?streak+1:1;previous=lead;continue;}
                    string archiveKey=legacy.ContainsKey(node)?node+"@"+PublishedTriadStore.Hash(CloudJson.Encode(old)):node;
                    legacy[archiveKey]=old;used.Remove(Fingerprint(old));used.Add(Fingerprint(chosen.Stages));recipes[node]=chosen.Stages;changed++;
                    lead=chosen.Stages[0].Triad.Lesson;streak=previous==lead?streak+1:1;previous=lead;
                    Console.WriteLine("Pacing: "+node+" lesson "+old[0].Triad.Lesson+" -> "+lead+", actions "+total+" -> "+chosen.Stages.Sum(s=>s.Minimum));
                }
                legacyJson=CloudJson.Encode(legacy);Console.WriteLine("Pacing replacements: "+changed);
            }
            if(deduplicate){
                var legacy=CloudJson.Decode<Dictionary<string,DescentStage[]>>(Read("production_triad_recipes_legacy_oct09"));
                string[] Keys(LevelContent content,DescentStage[] stages)=>catalog.AllLevels
                    .Where(c=>c.Definition.CampaignNodeId==content.Definition.CampaignNodeId)
                    .Select(c=>CampaignUniqueness.Run(CampaignUniqueness.Keys(c.Definition,stages),true)).Distinct(StringComparer.Ordinal).ToArray();
                // Reserve every old layout, even later ones: repairing a duplicate must
                // not force an otherwise unique future level to change.
                var reserved=new HashSet<string>(catalog.PrimaryLevels.SelectMany(c=>Keys(c,recipes[c.Definition.CampaignNodeId])),StringComparer.Ordinal);
                var used=new HashSet<string>(StringComparer.Ordinal);int changed=0;
                foreach(var content in catalog.PrimaryLevels){
                    string node=content.Definition.CampaignNodeId;var old=recipes[node];var keys=Keys(content,old);
                    if(!keys.Any(used.Contains)){foreach(string key in keys)used.Add(key);continue;}
                    bool found=false;
                    var baseline=new DescentRun { Version=6,SourceHash=content.Definition.ContentHash,Stages=old };
                    for(int variant=1;variant<=128&&!found;variant++){
                        using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(12));DescentRun run;
                        try{run=CampaignTriads.Primary(content.Definition)==(MechanicLesson)old[0].Triad.Lesson
                            ?CampaignTriads.BuildAlternative(content.Definition,variant,stop.Token,60000)
                            :CampaignTriads.BuildCampaignRevision(content.Definition,(MechanicLesson)old[0].Triad.Lesson,variant,baseline,stop.Token);}
                        catch(Exception error)when(error is InvalidOperationException||error is OperationCanceledException){continue;}
                        int total=old.Sum(s=>s.Minimum);
                        if(run==null||run.Stages.Select((s,i)=>s.Minimum<old[i].Minimum||s.Minimum>old[i].Minimum+Math.Max(2,old[i].Minimum/3)).Any(x=>x)
                            ||run.Stages.Sum(s=>s.Minimum)>total+Math.Max(3,total/3))continue;
                        keys=Keys(content,run.Stages);if(keys.Any(reserved.Contains))continue;
                        run.Validate(content.Definition,content.Locale);
                        string archiveKey=legacy.ContainsKey(node)?node+"@"+PublishedTriadStore.Hash(CloudJson.Encode(old)):node;
                        legacy[archiveKey]=old;recipes[node]=run.Stages;
                        foreach(string key in keys){used.Add(key);reserved.Add(key);}
                        found=true;changed++;Console.WriteLine("Distinct gameplay: "+node+" variant "+variant+"; actions "+total+" -> "+run.Stages.Sum(s=>s.Minimum));
                    }
                    if(!found)throw new InvalidDataException("Could not separate repeated recipe: "+node);
                }
                legacyJson=CloudJson.Encode(legacy);Console.WriteLine("Gameplay duplicate replacements: "+changed);
            }
            if(distinctStages)legacyJson=RepairCampaignStages.Apply(catalog,recipes,root);
            recipeJson=CloudJson.Encode(recipes);
            manifestJson=CloudJson.Encode(new PublishedTriadStore.Manifest{CatalogHash=catalog.ContentHash,RecipesHash=PublishedTriadStore.Hash(recipeJson),
                SourceHashes=catalog.AllLevels.Select(c=>c.Definition.ContentHash).Distinct().ToArray()});
        }
        var store=new PublishedTriadStore(recipeJson,manifestJson);int stages=0;
        var published=CloudJson.Decode<Dictionary<string,DescentStage[]>>(recipeJson);
        if(published.Values.Select(s=>string.Join("|",s.Select(v=>v.Triad.Fingerprint))).Distinct().Count()!=catalog.PrimaryLevels.Count)
            throw new InvalidDataException("The published campaign still contains identical three-stage recipes.");
        foreach(var content in catalog.AllLevels){
            var run=store.Bind(content.Definition,content.Locale,catalog.ContentHash);
            if(run==null)throw new InvalidDataException("Unbound recipe for "+content.Definition.PuzzleId);stages+=run.Stages.Length;
        }
        // A different label, unused default, alphabet or reflection is not a new puzzle.
        CampaignUniqueness.ThrowIfRepeated(CampaignUniqueness.Verify(catalog,store,out _));
        string resources=Path.Combine(root,"CascadeUnity","Assets","_Cascade","Content","Resources","CascadeContent");
        var outputs=new Dictionary<string,string>{
            {Path.Combine(root,"Data","production_triad_recipes.json"),recipeJson},
            {Path.Combine(root,"Data","production_triad_manifest.json"),manifestJson},
            {Path.Combine(resources,"production_triad_recipes.json"),recipeJson},
            {Path.Combine(resources,"production_triad_manifest.json"),manifestJson}};
        if(legacyJson!=null)outputs.Add(Path.Combine(root,"Data","production_triad_recipes_legacy_oct09.json"),legacyJson);
        if(check){foreach(var entry in outputs)if(File.ReadAllText(entry.Key)!=entry.Value)throw new InvalidDataException("Published resource differs: "+entry.Key);}
        else {
            // Publish only after every locale has replayed; each file replacement is atomic.
            string suffix=".publish-"+Guid.NewGuid().ToString("N");
            try {
                foreach(var entry in outputs)File.WriteAllText(entry.Key+suffix,entry.Value);
                foreach(var entry in outputs){if(File.Exists(entry.Key))File.Replace(entry.Key+suffix,entry.Key,null);else File.Move(entry.Key+suffix,entry.Key);}
            }finally{foreach(string path in outputs.Keys)if(File.Exists(path+suffix))File.Delete(path+suffix);}
        }
        Console.WriteLine("PASS: "+catalog.PrimaryLevels.Count+" campaign recipes, "+stages+" primary/localized witness replays; "+(check?"published resources match.":"resources published; existing metadata preserved."));
    }
}
