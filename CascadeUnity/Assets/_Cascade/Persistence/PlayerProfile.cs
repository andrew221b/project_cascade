using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Newtonsoft.Json;

namespace Cascade.Persistence
{
    public sealed class ClearReceipt
    {
        public string CampaignNodeId, AttemptId, ContentHash;
        public int BestStars,BestScore;
        [JsonProperty(DefaultValueHandling=DefaultValueHandling.Ignore)] public bool BestHalfStar,ReplayRewardHalfStar;
        [JsonIgnore] public int BestStarHalfUnits=>BestStars*2+(BestHalfStar?1:0);
        [JsonIgnore] public double BestStarRating=>BestStarHalfUnits/2.0;
        [JsonIgnore] public int ReplayRewardHalfUnits=>ReplayRewardStars*2+(ReplayRewardHalfStar?1:0);
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public ClearReplay Replay;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public ClearReplay ScoreReplay;
        [JsonProperty(DefaultValueHandling=DefaultValueHandling.Ignore)] public int ReplayRewardStars;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)] public ClearReplay ReplayRewardProof,ReplayRewardPriorProof;
        [JsonIgnore] public ClearReplay RewardProof=>ReplayRewardProof??Replay;
        [JsonIgnore] public ClearReplay PriorRewardProof=>ReplayRewardPriorProof??Replay;
        public void CompactRewardProofs()
        {
            string current=Replay?.Stages[2].AttemptId;
            if(current!=null&&ReplayRewardProof?.Stages[2].AttemptId==current)ReplayRewardProof=null;
            if(current!=null&&ReplayRewardPriorProof?.Stages[2].AttemptId==current)ReplayRewardPriorProof=null;
        }
    }

    public sealed class PlayerProfile
    {
        public GamePreferences Preferences=new GamePreferences();
        public StageVarietyMemory Variety=new StageVarietyMemory();
        public string PreferredMode="Shapes";
        // Revision 1 maps the retired five-star presentation scale to the three-star scale.
        [JsonProperty(DefaultValueHandling=DefaultValueHandling.Ignore)] public int RatingRevision;
        // Device-local reply receipt, committed atomically with its applied profile.
        public string CloudAppliedRequestId;
        public bool CloudAppliedEconomyConflict;
        public List<ClearReceipt> Clears=new List<ClearReceipt>();
        public List<int> HeardStory=new List<int>();
        public List<string> SeenCoachPrompts=new List<string>();
        public List<MechanicLesson> SeenMechanics=new List<MechanicLesson>();
        public bool FirstGestureShown, JourneyAttempt, AccountIntroSeen;
        public bool? StoryRoute;
        public AttemptRecord Attempt;
        public DescentRun Descent;
        public LevelCompletion Completion;
        public JourneyRewards Rewards=new JourneyRewards();
        public DirectorMemory Director=new DirectorMemory();
        public PersonalMissionMemory Missions=new PersonalMissionMemory();
        public MonetizationLedger Monetization=new MonetizationLedger();
        // Only inactive journeys live here. Existing root fields are the active journey.
        public List<ModeJourney> ModeJourneys=new List<ModeJourney>();
        public List<MasteryRecord> ModeRecords=new List<MasteryRecord>();
        public IEnumerable<ClearReceipt> AllClears=>Clears.Concat(ModeJourneys.SelectMany(j=>j.Clears));
        // Story is remembered across parallel journeys; gameplay progress stays local.
        public bool RemembersStory(int receipt)=>HeardStory.Contains(receipt)||ModeJourneys.Any(j=>j.HeardStory.Contains(receipt));
        public bool HasStoryMemory=>HeardStory.Count>0||ModeJourneys.Any(j=>j.HeardStory.Count>0);
        public void MigrateCoinEconomy()
        {
            MigrateRatings();
            if(Rewards==null||Monetization==null||Rewards.CoinEconomyRevision>=1)return;
            int clears=Rewards.SeenClearNodes?.Distinct(StringComparer.Ordinal).Count()??0;
            long incomeCarry=(long)clears*40+(clears/10)*100-((long)clears*20+(clears/10)*50);
            long priceCarry=0;var paid=Monetization.Grants?.Where(g=>g.Kind=="cosmetic").Select(g=>g.Product).ToHashSet(StringComparer.Ordinal)
                ??new HashSet<string>(StringComparer.Ordinal);
            foreach(TravelTool tool in Enum.GetValues(typeof(TravelTool))){
                int stock=tool==TravelTool.Wayfinder?Monetization.WayfinderScrolls:tool==TravelTool.UndoSpool?Monetization.UndoSpools:
                    tool==TravelTool.ReturnHourglass?Monetization.ReturnHourglasses:tool==TravelTool.SignalLantern?Monetization.SignalLanterns:
                    tool==TravelTool.LandingLens?Monetization.LandingLenses:Monetization.RepairKits;
                int parcelStock=Monetization.Grants?.Count(g=>g.Kind=="parcel_tool"&&g.Product==tool.ToString())??0;
                priceCarry+=(long)Math.Max(0,stock-parcelStock)*(ShopCatalog.Price(tool)-ShopCatalog.HistoricalPrice(tool));
            }
            foreach(string cosmetic in Monetization.Cosmetics??new List<string>())if(!paid.Contains(cosmetic))
                priceCarry+=ShopCatalog.TrophyPrice(cosmetic)-ShopCatalog.HistoricalTrophyPrice(cosmetic);
            Rewards.CoinIncomeCarryover=checked((int)(incomeCarry+priceCarry));
            Rewards.CoinEconomyRevision=1;
            Monetization.SpentCoins=checked(Monetization.SpentCoins+(int)priceCarry);
        }
        public void MigrateRatings()
        {
            if(RatingRevision>=1)return;
            foreach(var receipt in Clears)MigrateReceipt(receipt);
            foreach(var journey in ModeJourneys??new List<ModeJourney>()){
                foreach(var receipt in journey.Clears??new List<ClearReceipt>())MigrateReceipt(receipt);
                MigrateCompletion(journey.Completion);
            }
            MigrateCompletion(Completion);RatingRevision=1;
        }
        static void MigrateReceipt(ClearReceipt receipt)
        {
            if(receipt==null)return;
            if(!receipt.BestHalfStar)receipt.BestStars=ThreeStarValue(receipt.BestStars);
            if(!receipt.ReplayRewardHalfStar)receipt.ReplayRewardStars=ThreeStarValue(receipt.ReplayRewardStars);
        }
        static void MigrateCompletion(LevelCompletion completion)
        {
            if(completion==null||completion.RatingVersion>=2)return;
            completion.Stars=ThreeStarValue(completion.Stars);completion.RatingVersion=2;
        }
        static int ThreeStarValue(int stars)=>stars<=0?0:stars<=2?1:stars<=4?2:3;
        public PlayerProfile ProgressSnapshot()
        {
            var snapshot=CloudJson.Clone(this);snapshot.DiscardSessionAttempts();return snapshot;
        }
        public void DiscardSessionAttempts()
        {
            Attempt=null;Descent=null;Completion=null;JourneyAttempt=false;StoryRoute=null;
            if(Missions.Active)Missions.ClearActive();
            foreach(var journey in ModeJourneys){
                journey.Attempt=null;journey.Descent=null;journey.Completion=null;journey.JourneyAttempt=false;journey.StoryRoute=null;
                if(journey.Missions.Active)journey.Missions.ClearActive();
            }
        }
        public void SelectJourney(string mode)
        {
            if(!new[]{"Shapes","Words","Mixed"}.Contains(mode))throw new ArgumentException("Unknown journey mode.");
            if(mode==PreferredMode)return;
            var next=ModeJourneys.FirstOrDefault(j=>j.Mode==mode)??new ModeJourney{Mode=mode};
            ModeJourneys.Remove(next);ModeJourneys.Add(ModeJourney.Capture(this));
            next.Apply(this);PreferredMode=mode;
        }

        public bool RecordClear(AttemptService attempt,LevelCompletion completion=null)
        {
            if(completion?.RatingVersion>=3)MigrateRatings();
            if(Descent!=null&&(!Descent.IsFinal||attempt.PinnedContentHash!=Descent.StageHash(2)))return false;
            if(attempt.Current.Status!=PuzzleStatus.Won)return false;
            var receipt=Clears.FirstOrDefault(r=>r.CampaignNodeId==attempt.Level.CampaignNodeId);
            bool first=receipt==null;
            if(first){receipt=new ClearReceipt {CampaignNodeId=attempt.Level.CampaignNodeId,
                AttemptId=attempt.AttemptId.ToString("D"),ContentHash=Descent?.ContentHash??attempt.PinnedContentHash};Clears.Add(receipt);}
            var replay=ClearReplay.Capture(Descent,attempt);
            if(!first&&completion!=null&&replay!=null&&receipt.Replay!=null&&completion.StarHalfUnits>receipt.ReplayRewardHalfUnits
                &&receipt.Replay.Stages[2].AttemptId!=attempt.AttemptId.ToString("D")){
                receipt.ReplayRewardPriorProof=receipt.PriorRewardProof;
                receipt.ReplayRewardProof=replay;receipt.ReplayRewardStars=completion.Stars;receipt.ReplayRewardHalfStar=completion.HalfStar;
                Rewards.RecordReplayCredit(receipt.CampaignNodeId,completion.Stars,completion.HalfStar);
            }
            if(replay!=null) {
                if(receipt.Replay==null||completion!=null&&(completion.StarHalfUnits>receipt.BestStarHalfUnits||completion.StarHalfUnits==receipt.BestStarHalfUnits&&completion.Score>receipt.BestScore)){
                    if(completion!=null&&completion.Score<receipt.BestScore&&receipt.ScoreReplay==null)receipt.ScoreReplay=receipt.Replay;
                    receipt.Replay=replay;receipt.AttemptId=attempt.AttemptId.ToString("D");receipt.ContentHash=Descent.ContentHash;
                    if(completion!=null&&completion.Score>=receipt.BestScore)receipt.ScoreReplay=null;
                }else if(completion!=null&&completion.Score>=receipt.BestScore)receipt.ScoreReplay=replay;
            }
            if(completion!=null){if(completion.StarHalfUnits>receipt.BestStarHalfUnits){receipt.BestStars=completion.Stars;receipt.BestHalfStar=completion.HalfStar;}receipt.BestScore=Math.Max(receipt.BestScore,completion.Score);}
            receipt.CompactRewardProofs();
            if(!first)return false;
            return true;
        }
        public AttemptService RestoreAttempt(ContentCatalog catalog, IEnumerable<LevelContent> legacy = null,StageVarietyStore variety=null)
        {
            if(Attempt==null)return null;
            var content=catalog.AllLevels.Concat(catalog.BonusLevels).Concat(legacy??Enumerable.Empty<LevelContent>()).FirstOrDefault(c=>
                c.Definition.PuzzleId==Attempt.PuzzleId && c.Definition.ContentHash==(Descent?.SourceHash??Attempt.Direction?.SourceHash??Attempt.ContentHash)
                && c.Definition.RulesVersion==Attempt.RulesVersion && (Descent?.Version==7||c.Locale==Attempt.Locale));
            if(content==null)throw new InvalidDataException("Pinned puzzle content is unavailable.");
            if(Descent?.Version==7){
                if(variety==null)throw new InvalidDataException("Stage vocabulary is required to restore this attempt.");
                if(Missions.Active&&Missions.Offer?.Run?.Version==7&&Missions.Offer.Run.ContentHash==Descent.ContentHash){var expected=variety.Expand(content.Definition,Descent.Variation);if(expected.Fingerprint!=Descent.Variation.Fingerprint)throw new InvalidDataException("Invalid personal stage vocabulary.");Descent.Validate(content.Definition,Attempt.Locale);}
                else variety.ValidateSaved(Descent,content.Definition,variety.Published(content.Definition));
            }else if(Descent!=null)Descent.Validate(content.Definition,Attempt.Locale);
            var level=Descent!=null?Descent.Level(content.Definition,Descent.StageIndex):Attempt.Direction==null?content.Definition:Attempt.Direction.Apply(content.Definition);
            return AttemptService.Restore(level,Attempt);
        }
        public void Validate()
        {
            Variety?.Validate();
            if(Preferences==null || !Locale(Preferences.UiLocale) || !Locale(Preferences.PuzzleLocale)
                || Preferences.EffectQuality<0 || Preferences.EffectQuality>2
                || !new[]{"Shapes","Words","Mixed"}.Contains(PreferredMode)
                || CloudAppliedRequestId?.Length>128
                || Clears==null || Clears.Count>10000 || HeardStory==null || HeardStory.Count>10000
                || HeardStory.Any(p=>p<0 || p>10000) || HeardStory.Distinct().Count()!=HeardStory.Count
                || SeenCoachPrompts==null||SeenCoachPrompts.Count>10000||SeenCoachPrompts.Any(c=>string.IsNullOrWhiteSpace(c)||c.Length>256)
                ||SeenCoachPrompts.Distinct(StringComparer.Ordinal).Count()!=SeenCoachPrompts.Count
                || !MechanicLessons.Valid(SeenMechanics))
                throw new InvalidDataException("Invalid profile fields.");
            if(ModeJourneys==null||ModeJourneys.Count>2||ModeJourneys.Any(j=>j==null||j.Mode==PreferredMode||!new[]{"Shapes","Words","Mixed"}.Contains(j.Mode))
                ||ModeJourneys.Select(j=>j.Mode).Distinct().Count()!=ModeJourneys.Count)throw new InvalidDataException("Invalid mode journeys.");
            foreach(var journey in ModeJourneys){
                var check=new PlayerProfile();journey.Apply(check);check.PreferredMode=journey.Mode;
                // The account reward ledger is shared and validated against all journeys below.
                check.Validate();
            }
            var nodes=new HashSet<string>(StringComparer.Ordinal);
            foreach(var receipt in Clears)if(receipt==null || string.IsNullOrWhiteSpace(receipt.CampaignNodeId)
                || receipt.CampaignNodeId.Length>128 || !nodes.Add(receipt.CampaignNodeId)
                || !Guid.TryParse(receipt.AttemptId,out var id) || id==Guid.Empty || !Hash(receipt.ContentHash)
                ||receipt.Replay!=null&&!receipt.Replay.IsBounded()
                ||receipt.ScoreReplay!=null&&!receipt.ScoreReplay.IsBounded()
                ||receipt.BestHalfStar&&(receipt.BestStars<1||receipt.BestStars>2)
                ||receipt.ReplayRewardHalfStar&&(receipt.ReplayRewardStars<1||receipt.ReplayRewardStars>2)
                ||receipt.ReplayRewardStars<0||receipt.ReplayRewardStars>(RatingRevision>=1?3:5)
                ||receipt.ReplayRewardProof!=null&&!receipt.ReplayRewardProof.IsBounded()
                ||receipt.ReplayRewardPriorProof!=null&&!receipt.ReplayRewardPriorProof.IsBounded()
                ||receipt.BestStars<0||receipt.BestStars>(RatingRevision>=1?3:5)||receipt.BestScore<0||receipt.BestScore>2100)
                throw new InvalidDataException("Invalid first-clear receipt.");
            if(Completion!=null&&!Completion.IsValid())throw new InvalidDataException("Invalid completion receipt.");
            new JourneyRewards{Records=ModeRecords}.Validate();
            if(Rewards==null)throw new InvalidDataException("Missing journey rewards.");
            Rewards.Validate(Monetization?.Cosmetics);
            if(Rewards.ReplayCredits!=null&&Rewards.ReplayCredits.Any(c=>!AllClears.Any(r=>r.CampaignNodeId==c.NodeId&&r.ReplayRewardHalfUnits>=c.HalfUnits)))
                throw new InvalidDataException("Replay credit requires its clear evidence.");
            if(Director==null||!Director.IsValid())throw new InvalidDataException("Invalid delivery director memory.");
            if(Missions==null||!Missions.IsValid())throw new InvalidDataException("Invalid personal mission memory.");
            if(Monetization==null)throw new InvalidDataException("Missing monetization ledger.");
            Monetization.Validate();
            Rewards.ValidateToolClaims(Monetization);
            var accountNodes=AllClears.Select(c=>c.CampaignNodeId).ToHashSet();
            if(Rewards.SeenClearNodes.Any(n=>!accountNodes.Contains(n)))throw new InvalidDataException("Reward progress has no first-clear receipt.");
            // An incompatible attempt is recoverable independently of valid meta-progress.
        }
        static bool Locale(string value)=>value=="en"||value=="ru"||value=="uk";
        static bool Hash(string value)=>value!=null && value.Length==64 && value.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
    }
    public sealed class ModeJourney
    {
        public string Mode;
        public List<ClearReceipt> Clears=new List<ClearReceipt>();
        public List<int> HeardStory=new List<int>();
        public List<MechanicLesson> SeenMechanics=new List<MechanicLesson>();
        public bool FirstGestureShown,JourneyAttempt;
        public bool? StoryRoute;
        public AttemptRecord Attempt;
        public DescentRun Descent;
        public LevelCompletion Completion;
        public DirectorMemory Director=new DirectorMemory();
        public PersonalMissionMemory Missions=new PersonalMissionMemory();
        public List<MasteryRecord> Records=new List<MasteryRecord>();
        public static ModeJourney Capture(PlayerProfile p)=>new ModeJourney{Mode=p.PreferredMode,Clears=p.Clears,HeardStory=p.HeardStory,
            SeenMechanics=p.SeenMechanics,FirstGestureShown=p.FirstGestureShown,JourneyAttempt=p.JourneyAttempt,StoryRoute=p.StoryRoute,
            Attempt=p.Attempt,Descent=p.Descent,Completion=p.Completion,Director=p.Director,Missions=p.Missions,Records=p.ModeRecords};
        public void Apply(PlayerProfile p)
        {
            p.Clears=Clears;p.HeardStory=HeardStory;p.SeenMechanics=SeenMechanics;p.FirstGestureShown=FirstGestureShown;
            p.JourneyAttempt=JourneyAttempt;p.StoryRoute=StoryRoute;p.Attempt=Attempt;p.Descent=Descent;p.Completion=Completion;p.Director=Director;p.Missions=Missions;p.ModeRecords=Records;
        }
    }
}
