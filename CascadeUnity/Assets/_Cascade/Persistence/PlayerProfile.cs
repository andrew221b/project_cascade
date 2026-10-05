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
    }

    public sealed class PlayerProfile
    {
        public GamePreferences Preferences=new GamePreferences();
        public string PreferredMode="Shapes";
        public List<ClearReceipt> Clears=new List<ClearReceipt>();
        public List<int> HeardStory=new List<int>();
        public bool FirstGestureShown, JourneyAttempt;
        public AttemptRecord Attempt;

        public bool RecordClear(AttemptService attempt)
        {
            if(attempt.Current.Status!=PuzzleStatus.Won || Clears.Any(r=>r.CampaignNodeId==attempt.Level.CampaignNodeId))return false;
            Clears.Add(new ClearReceipt {CampaignNodeId=attempt.Level.CampaignNodeId,
                AttemptId=attempt.AttemptId.ToString("D"),ContentHash=attempt.PinnedContentHash});
            return true;
        }
        public AttemptService RestoreAttempt(ContentCatalog catalog)
        {
            if(Attempt==null)return null;
            var content=catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).FirstOrDefault(c=>
                c.Definition.PuzzleId==Attempt.PuzzleId && c.Definition.ContentHash==Attempt.ContentHash
                && c.Definition.RulesVersion==Attempt.RulesVersion && c.Locale==Attempt.Locale);
            if(content==null)throw new InvalidDataException("Pinned puzzle content is unavailable.");
            return AttemptService.Restore(content.Definition,Attempt);
        }
        public void Validate()
        {
            if(Preferences==null || !Locale(Preferences.UiLocale) || !Locale(Preferences.PuzzleLocale)
                || Preferences.EffectQuality<0 || Preferences.EffectQuality>2
                || !new[]{"Shapes","Words","Mixed"}.Contains(PreferredMode)
                || Clears==null || Clears.Count>10000 || HeardStory==null || HeardStory.Count>10000
                || HeardStory.Any(p=>p<0 || p>10000) || HeardStory.Distinct().Count()!=HeardStory.Count)
                throw new InvalidDataException("Invalid profile fields.");
            var nodes=new HashSet<string>(StringComparer.Ordinal);
            foreach(var receipt in Clears)if(receipt==null || string.IsNullOrWhiteSpace(receipt.CampaignNodeId)
                || receipt.CampaignNodeId.Length>128 || !nodes.Add(receipt.CampaignNodeId)
                || !Guid.TryParse(receipt.AttemptId,out var id) || id==Guid.Empty || !Hash(receipt.ContentHash))
                throw new InvalidDataException("Invalid first-clear receipt.");
            // An incompatible attempt is recoverable independently of valid meta-progress.
        }
        static bool Locale(string value)=>value=="en"||value=="ru"||value=="uk";
        static bool Hash(string value)=>value!=null && value.Length==64 && value.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
    }
}
