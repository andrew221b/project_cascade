using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;

namespace Cascade.Persistence
{
    public sealed class ClearReceipt
    {
        public string CampaignNodeId, AttemptId, ContentHash;
        public int BestStars,BestScore;
    }

    public sealed class PlayerProfile
    {
        public GamePreferences Preferences=new GamePreferences();
        public string PreferredMode="Shapes";
        public List<ClearReceipt> Clears=new List<ClearReceipt>();
        public List<int> HeardStory=new List<int>();
        public List<MechanicLesson> SeenMechanics=new List<MechanicLesson>();
        public bool FirstGestureShown, JourneyAttempt;
        public bool? StoryRoute;
        public AttemptRecord Attempt;
        public DescentRun Descent;
        public LevelCompletion Completion;
        public JourneyRewards Rewards=new JourneyRewards();
        public DirectorMemory Director=new DirectorMemory();
        public MonetizationLedger Monetization=new MonetizationLedger();

        public bool RecordClear(AttemptService attempt,LevelCompletion completion=null)
        {
            if(Descent!=null&&(!Descent.IsFinal||attempt.PinnedContentHash!=Descent.StageHash(2)))return false;
            if(attempt.Current.Status!=PuzzleStatus.Won)return false;
            var receipt=Clears.FirstOrDefault(r=>r.CampaignNodeId==attempt.Level.CampaignNodeId);
            bool first=receipt==null;
            if(first){receipt=new ClearReceipt {CampaignNodeId=attempt.Level.CampaignNodeId,
                AttemptId=attempt.AttemptId.ToString("D"),ContentHash=Descent?.ContentHash??attempt.PinnedContentHash};Clears.Add(receipt);}
            if(completion!=null){receipt.BestStars=Math.Max(receipt.BestStars,completion.Stars);receipt.BestScore=Math.Max(receipt.BestScore,completion.Score);}
            if(!first)return false;
            return true;
        }
        public AttemptService RestoreAttempt(ContentCatalog catalog, IEnumerable<LevelContent> legacy = null)
        {
            if(Attempt==null)return null;
            var content=catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Concat(catalog.BonusLevels).Concat(legacy??Enumerable.Empty<LevelContent>()).FirstOrDefault(c=>
                c.Definition.PuzzleId==Attempt.PuzzleId && c.Definition.ContentHash==(Descent?.SourceHash??Attempt.Direction?.SourceHash??Attempt.ContentHash)
                && c.Definition.RulesVersion==Attempt.RulesVersion && c.Locale==Attempt.Locale);
            if(content==null)throw new InvalidDataException("Pinned puzzle content is unavailable.");
            if(Descent!=null)Descent.Validate(content.Definition,Attempt.Locale);
            var level=Descent!=null?Descent.Level(content.Definition,Descent.StageIndex):Attempt.Direction==null?content.Definition:Attempt.Direction.Apply(content.Definition);
            return AttemptService.Restore(level,Attempt);
        }
        public void Validate()
        {
            if(Preferences==null || !Locale(Preferences.UiLocale) || !Locale(Preferences.PuzzleLocale)
                || Preferences.EffectQuality<0 || Preferences.EffectQuality>2
                || !new[]{"Shapes","Words","Mixed"}.Contains(PreferredMode)
                || Clears==null || Clears.Count>10000 || HeardStory==null || HeardStory.Count>10000
                || HeardStory.Any(p=>p<0 || p>10000) || HeardStory.Distinct().Count()!=HeardStory.Count
                || !MechanicLessons.Valid(SeenMechanics))
                throw new InvalidDataException("Invalid profile fields.");
            var nodes=new HashSet<string>(StringComparer.Ordinal);
            foreach(var receipt in Clears)if(receipt==null || string.IsNullOrWhiteSpace(receipt.CampaignNodeId)
                || receipt.CampaignNodeId.Length>128 || !nodes.Add(receipt.CampaignNodeId)
                || !Guid.TryParse(receipt.AttemptId,out var id) || id==Guid.Empty || !Hash(receipt.ContentHash)
                ||receipt.BestStars<0||receipt.BestStars>5||receipt.BestScore<0||receipt.BestScore>2100)
                throw new InvalidDataException("Invalid first-clear receipt.");
            if(Completion!=null&&!Completion.IsValid())throw new InvalidDataException("Invalid completion receipt.");
            if(Rewards==null)throw new InvalidDataException("Missing journey rewards.");
            Rewards.Validate(Monetization?.Cosmetics);
            if(Director==null||!Director.IsValid())throw new InvalidDataException("Invalid delivery director memory.");
            if(Monetization==null)throw new InvalidDataException("Missing monetization ledger.");
            Monetization.Validate();
            if(Rewards.SeenClearNodes.Any(n=>!nodes.Contains(n)))throw new InvalidDataException("Reward progress has no first-clear receipt.");
            // An incompatible attempt is recoverable independently of valid meta-progress.
        }
        static bool Locale(string value)=>value=="en"||value=="ru"||value=="uk";
        static bool Hash(string value)=>value!=null && value.Length==64 && value.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
    }
}
