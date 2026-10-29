using Newtonsoft.Json.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using System.Reflection;

namespace CascadeAccountModule;

public static class PublishedCatalog
{
    public static readonly IReadOnlyList<string> AllowedNodes = Load();
    static string Resource(string name)
    {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("CascadeAccount.Published."+name+".json")??throw new InvalidDataException("Published resource missing: "+name);
        using var reader=new StreamReader(stream);return reader.ReadToEnd();
    }
    static readonly ContentCatalog catalog=ContentImporter.Import(Resource("levels"),Resource("localized_levels"),Resource("words_en"),Resource("words_ru"),Resource("words_uk"),Resource("campaign_extension"));
    static readonly IReadOnlyDictionary<string,DescentStage[]> recipes=AccountJson.Decode<Dictionary<string,DescentStage[]>>(Resource("recipes"));
    static readonly IReadOnlyDictionary<string,DescentStage[]> legacyRecipes=AccountJson.Decode<Dictionary<string,DescentStage[]>>(Resource("recipes_legacy"));
    static readonly StageVarietyStore variety=new(Resource("stage_words_v1"),Resource("stage_variety_templates_v1"),Resource("recipes"));
    public static LevelContent Primary(string node)=>catalog.GetPrimary(node);
    public static LevelContent Source(string hash)=>catalog.AllLevels.FirstOrDefault(c=>c.Definition.ContentHash==hash);
    public static DescentRun Run(LevelDefinition source)=>new(){Version=6,SourceHash=source.ContentHash,Stages=recipes[source.CampaignNodeId]};
    public static bool ValidClear(ClearReceipt receipt,string mode,double knownStars=0,int knownScore=0)
    {
        var proof=receipt?.Replay;if(proof==null||!proof.IsBounded())return false;
        if(!Verify(proof,receipt.CampaignNodeId,mode,out var run,out var final,out var result)
            ||run.ContentHash!=receipt.ContentHash||final.AttemptId.ToString("D")!=receipt.AttemptId||receipt.BestStarRating>Math.Max(knownStars,result.StarRating))return false;
        if(receipt.BestScore<=Math.Max(knownScore,result.Score))return true;
        return Verify(receipt.ScoreReplay,receipt.CampaignNodeId,mode,out _,out _,out var scoreResult)&&receipt.BestScore<=scoreResult.Score;
    }
    static bool Verify(ClearReplay proof,string node,string mode,out DescentRun run,out AttemptService final,out LevelCompletion result)
    {
        run=null;final=null;result=null;if(proof==null||!proof.IsBounded())return false;
        var source=Source(proof.SourceHash);
        // Mix includes pure short rows and historical alternating rows; all still need a published replay.
        if(source==null||source.Definition.CampaignNodeId!=node||(proof.Version==7?proof.Variation?.Mode!=mode:(mode!="Mixed"&&new AttemptService(source.Definition,source.Locale).Mode.ToString()!=mode)))return false;
        run=Run(source.Definition);
        try {
            if(proof.Version==7)run=variety.Create(source.Definition,proof.Variation,run);
            if(proof.Version==6&&proof.Stages[0].ContentHash!=run.Level(source.Definition,0).ContentHash){
                run=legacyRecipes.Where(pair=>pair.Key==node||pair.Key.StartsWith(node+"@",StringComparison.Ordinal))
                    .Take(32).Select(pair=>new DescentRun{Version=6,SourceHash=source.Definition.ContentHash,Stages=AccountJson.Decode<DescentStage[]>(AccountJson.Encode(pair.Value))})
                    .FirstOrDefault(candidate=>candidate.Level(source.Definition,0).ContentHash==proof.Stages[0].ContentHash);
                if(run==null)return false;
            }
            for(int stage=0;stage<3;stage++) {
                var record=proof.Stages[stage];
                if(record.Locale!=(proof.Version==7?proof.Variation.Locale:source.Locale)||record.Direction!=null||record.RequiredStampBits!=0)return false;
                final=AttemptService.Restore(run.Level(source.Definition,stage),record);
                if(final.Current.Status!=PuzzleStatus.Won)return false;
                if(stage<2)run.AcceptCheckpoint(source.Definition,final);
            }
            result=LevelCompletion.From(final,null,run.Performance(final));return true;
        }catch(Exception e)when(e is ArgumentException or InvalidOperationException or OverflowException){return false;}
    }
    public static bool ValidProgress(PlayerProfile uploaded,PlayerProfile trusted)
    {
        try{uploaded.Variety?.ValidatePool(variety.Words);}catch(Exception e)when(e is ArgumentException or InvalidDataException or FormatException){return false;}
        foreach(string mode in new[]{"Shapes","Words","Mixed"}) {
            var incoming=mode==uploaded.PreferredMode?uploaded.Clears:uploaded.ModeJourneys.FirstOrDefault(j=>j.Mode==mode)?.Clears;
            var existing=mode==trusted.PreferredMode?trusted.Clears:trusted.ModeJourneys.FirstOrDefault(j=>j.Mode==mode)?.Clears;
            foreach(var clear in incoming??new List<ClearReceipt>()) {
                var prior=existing?.FirstOrDefault(c=>c.CampaignNodeId==clear.CampaignNodeId);
                if(clear.ReplayRewardHalfUnits>(prior?.ReplayRewardHalfUnits??0)){
                    if(!Verify(clear.RewardProof,clear.CampaignNodeId,mode,out _,out var repeat,out var rating)
                        ||!Verify(clear.PriorRewardProof,clear.CampaignNodeId,mode,out _,out var first,out _)
                        ||rating.StarHalfUnits<clear.ReplayRewardHalfUnits||repeat.AttemptId==first.AttemptId)return false;
                }
                if(prior!=null&&prior.ContentHash==clear.ContentHash&&prior.AttemptId==clear.AttemptId&&prior.BestStarHalfUnits>=clear.BestStarHalfUnits&&prior.BestScore>=clear.BestScore)continue;
                if(!ValidClear(clear,mode,prior?.BestStarRating??0,prior?.BestScore??0))return false;
            }
        }
        foreach(var credit in uploaded.Rewards.ReplayCredits??new List<ReplayCredit>())
            if(!uploaded.AllClears.Any(c=>c.CampaignNodeId==credit.NodeId&&c.ReplayRewardHalfUnits>=credit.HalfUnits))return false;
        return true;
    }
    static IReadOnlyList<string> Load()
    {
        var nodes = new List<string>();
        foreach (var name in new[] { "levels", "campaign_extension" }) {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CascadeAccount.Published." + name + ".json")
                ?? throw new InvalidDataException("Published campaign resource missing.");
            using var reader = new StreamReader(stream);
            var data = JToken.Parse(reader.ReadToEnd());
            var levels = data as JArray ?? (JArray)data["levels"];
            foreach (var level in levels) {
                string node = (string)level["id"];
                if (node == null || !node.StartsWith("C", StringComparison.Ordinal) || !int.TryParse(node.AsSpan(1), out _))
                    throw new InvalidDataException("Invalid published campaign node.");
                if (!nodes.Contains(node, StringComparer.Ordinal)) nodes.Add(node);
            }
        }
        if (nodes.Count == 0) throw new InvalidDataException("Published campaign empty.");
        return nodes.AsReadOnly();
    }
}
