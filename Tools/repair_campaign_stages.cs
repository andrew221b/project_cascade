using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using Newtonsoft.Json.Linq;

// Offline only. Keep unaffected stages; reuse the production constructor and solver.
internal static class RepairCampaignStages
{
    internal static string Apply(ContentCatalog catalog,Dictionary<string,DescentStage[]> recipes,string root)
    {
        var bound=catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).GroupBy(c=>c.Definition.CampaignNodeId)
            .ToDictionary(g=>g.Key,g=>g.ToArray(),StringComparer.Ordinal);
        string[] Keys(string node,DescentStage[] stages,int index)=>bound[node]
            .Select(c=>CampaignUniqueness.Keys(c.Definition,stages)[index]).Distinct(StringComparer.Ordinal).ToArray();
        var reserved=new HashSet<string>(catalog.PrimaryLevels.SelectMany(c=>Enumerable.Range(0,3)
            .SelectMany(i=>Keys(c.Definition.CampaignNodeId,recipes[c.Definition.CampaignNodeId],i))),StringComparer.Ordinal);
        var used=new HashSet<string>(StringComparer.Ordinal);
        var legacy=CloudJson.Decode<Dictionary<string,DescentStage[]>>(File.ReadAllText(Path.Combine(root,"Data","production_triad_recipes_legacy_oct09.json")));
        var changes=new JArray();int changedStages=0;
        string folder=Path.Combine(root,"Reports","StageUniquenessOct15");Directory.CreateDirectory(folder);
        foreach(var content in catalog.PrimaryLevels){
            string node=content.Definition.CampaignNodeId;var old=recipes[node];var replacement=old.ToArray();
            int oldTotal=old.Sum(s=>s.Minimum);var edited=new JArray();
            for(int index=0;index<3;index++){
                var keys=Keys(node,replacement,index);
                if(keys.Any(used.Contains)){
                    bool found=false;var rejected=new Dictionary<string,int>();
                    void Reject(string reason){rejected.TryGetValue(reason,out int count);rejected[reason]=count+1;}
                    for(int variant=1;variant<=1536&&!found;variant++){
                        using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(5));DescentStage stage;
                        try{stage=CampaignTriads.AuthorStageAlternative(content.Definition,(MechanicLesson)old[index].Triad.Lesson,index,variant,old[index],stop.Token);}
                        catch(Exception error)when(error is InvalidOperationException||error is OperationCanceledException){Reject("timeout-or-no-proof");continue;}
                        if(stage==null){Reject("null");continue;}
                        if(stage.Minimum<old[index].Minimum||stage.Minimum>old[index].Minimum+CampaignUniqueness.StageIncreaseAllowance(old[index].Minimum)){Reject("minimum-"+stage.Minimum);continue;}
                        int total=replacement.Sum(s=>s.Minimum)-replacement[index].Minimum+stage.Minimum;
                        if(total>oldTotal+CampaignUniqueness.LevelIncreaseAllowance(oldTotal)){Reject("total");continue;}
                        replacement[index]=stage;var proposed=Keys(node,replacement,index);
                        if(proposed.Any(reserved.Contains)){replacement[index]=old[index];Reject("reserved-minimum-"+stage.Minimum);continue;}
                        keys=proposed;found=true;changedStages++;edited.Add(new JObject{["stage"]=index+1,["variant"]=variant,["old_minimum"]=old[index].Minimum,["new_minimum"]=stage.Minimum});
                    }
                    if(!found)throw new InvalidDataException("No distinct certified stage: "+node+":"+(index+1)+"; target="+string.Join(",",content.Definition.TargetSymbolIds)+"; old="+old[index].Minimum+"; "+CloudJson.Encode(rejected));
                }
                foreach(string key in keys){used.Add(key);reserved.Add(key);}
            }
            if(edited.Count==0)continue;
            string archiveKey=legacy.ContainsKey(node)?node+"@"+PublishedTriadStore.Hash(CloudJson.Encode(old)):node;
            legacy[archiveKey]=old;recipes[node]=replacement;
            changes.Add(new JObject{["node"]=node,["stages"]=edited,["old_total"]=oldTotal,["new_total"]=replacement.Sum(s=>s.Minimum),["archive_key"]=archiveKey});
            File.WriteAllText(Path.Combine(folder,"changes.json"),changes.ToString()+"\n");
            Console.WriteLine("Distinct stages: "+node+" / "+edited.Count+" replaced; "+changedStages+" stages total");
        }
        Console.WriteLine("Stage replacements: "+changedStages+" in "+changes.Count+" nodes");
        return CloudJson.Encode(legacy);
    }
}

namespace Cascade.Application
{
    public static partial class CampaignTriads
    {
        // This half of the partial class is compiled only by the offline publisher.
        // Early certified lessons can introduce a mechanic absent from the old source
        // floors. Retain that certified lesson instead of consulting the old vocabulary.
        internal static DescentStage AuthorStageAlternative(Cascade.Domain.LevelDefinition source,MechanicLesson lesson,int index,int variant,DescentStage baseline,CancellationToken cancellation)
        {
            if(variant>512){
                var recipe=CloudJson.Decode<TriadRecipe>(CloudJson.Encode(baseline.Triad));
                var random=new Random(unchecked(int.Parse(source.CampaignNodeId.Substring(1))*7919+index*137+variant*104729));
                for(int i=recipe.Start.Length-1;i>0;i--){int j=random.Next(i+1),value=recipe.Start[i];recipe.Start[i]=recipe.Start[j];recipe.Start[j]=value;}
                foreach(var board in recipe.Boards)for(int edge=0;edge<board.Open.Length;edge++)if(random.Next(5)==0)board.Open[edge]=!board.Open[edge];
                int floor=random.Next(recipe.Boards.Length),kind=variant%4;var selected=recipe.Boards[floor];
                if(kind==0&&floor>0)selected.Entry=(int)Cascade.Domain.EntryTransform.MirrorNext;
                else if(kind==1){selected.Fragile=new[]{random.Next(selected.Open.Length)};selected.Hp++;}
                else if(kind==2&&selected.StampSlot>=0){selected.StampSlot=random.Next(source.TargetSymbolIds.Count);selected.StampSymbol=random.Next(source.TargetSymbolIds.Count);}
                else if(kind==3&&recipe.Boards.Length>1){int other=random.Next(recipe.Boards.Length);var value=recipe.Boards[floor];recipe.Boards[floor]=recipe.Boards[other];recipe.Boards[other]=value;if(recipe.Boards[0].Entry!=0)return null;}
                return ProveRecipe(source,lesson,recipe,cancellation,60000);
            }
            int pattern=(index+variant/32)%3,boost=variant<96?0:1;
            var floors=source.Floors.AsEnumerable();
            if(variant>=192){
                var open=Enumerable.Repeat(true,source.TargetSymbolIds.Count-1).ToArray();var symbol=source.TargetSymbolIds[0];
                int kind=(variant-192)/40%5;
                var extra=new Cascade.Domain.FloorDefinition(2,open,
                    fragileEdges:kind==4?new[]{0}:null,
                    bomb:kind==2?new Cascade.Domain.BombDefinition(0,symbol,2,1):null,
                    key:kind==1?new Cascade.Domain.KeyDefinition(0,symbol,0):null,
                    repair:kind==0?new Cascade.Domain.RepairDefinition(0,symbol,1):null,
                    oneUseBridgeEdge:kind==3?(int?)0:null);
                floors=floors.Concat(new[]{extra});
            }
            var construction=variant<160?source:new Cascade.Domain.LevelDefinition(source.PuzzleId,source.CampaignNodeId,
                source.SchemaVersion,source.RulesVersion,source.ContentHash,source.InitialSymbolIds,source.TargetSymbolIds,
                source.TargetSegments,floors,source.ReachCharges);
            for(int trial=0;trial<48;trial++){
                cancellation.ThrowIfCancellationRequested();
                var recipe=Construct(construction,lesson,pattern,unchecked(int.Parse(source.CampaignNodeId.Substring(1))*7919+index*137+trial*43+variant*104729),boost);
                if(recipe==null)continue;var stage=ProveRecipe(source,lesson,recipe,cancellation,60000);
                if(stage!=null)return stage;
            }
            return null;
        }
    }
}
