using System;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Application
{
    // Versioned storage lives outside Domain. This DTO carries a replay proof, never mutable state.
    public sealed class AttemptRecord
    {
        public string AttemptId, PuzzleId, CampaignNodeId, ContentHash, RulesVersion, Locale, StateHash;
        public int RequiredStampBits, UndoCount;
        public int? MovesMade;
        public double? ActiveSeconds;
        public bool HintUsed;
        public string FreeHintStateHash;
        [Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
        public string[] FreeHintStates;
        [Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
        public string BonusHintStateHash;
        public int FreeUndoUsed = -1, GuideFloor = -1;
        public bool SpoolUsed, HourglassUsed, WayfinderUsed, LanternUsed, LensUsed;
        [Newtonsoft.Json.JsonProperty(DefaultValueHandling=Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public bool RepairKitUsed;
        [Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
        public int? RepairAtRoute;
        [Newtonsoft.Json.JsonProperty(NullValueHandling=Newtonsoft.Json.NullValueHandling.Ignore)]
        public int[] RepairRoutes;
        public string[] Commands;
        public DirectionRecipe Direction;

        public static AttemptRecord Capture(AttemptService attempt) => new AttemptRecord {
            AttemptId=attempt.AttemptId.ToString("D"), PuzzleId=attempt.Level.PuzzleId,
            CampaignNodeId=attempt.Level.CampaignNodeId, ContentHash=attempt.PinnedContentHash,
            RulesVersion=attempt.PinnedRulesVersion, Locale=attempt.ActiveLocale,
            RequiredStampBits=attempt.Goal.RequiredStampBits, UndoCount=attempt.UndoCount,
            MovesMade=attempt.MovesMade,ActiveSeconds=attempt.ActiveSeconds,
            HintUsed=attempt.HintUsed, FreeHintStateHash=attempt.FreeHintStateHash, FreeHintStates=attempt.FreeHintStates.Count==0?null:attempt.FreeHintStates.ToArray(), BonusHintStateHash=attempt.BonusHintStateHash, StateHash=attempt.CurrentStateHash,
            FreeUndoUsed=attempt.FreeUndoUsed,GuideFloor=attempt.GuideFloor,SpoolUsed=attempt.SpoolUsed,HourglassUsed=attempt.HourglassUsed,WayfinderUsed=attempt.WayfinderUsed,LanternUsed=attempt.LanternUsed,LensUsed=attempt.LensUsed,
            RepairKitUsed=attempt.RepairKitUsed,RepairAtRoute=attempt.RepairAtRoute,RepairRoutes=attempt.RepairRoutes.Count>1?attempt.RepairRoutes.ToArray():null,
            Commands=attempt.History.Select(h=>h.Command.ToString()).ToArray(),Direction=attempt.Direction?.Copy()
        };
    }
}
