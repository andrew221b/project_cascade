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
        public bool HintUsed;
        public string[] Commands;

        public static AttemptRecord Capture(AttemptService attempt) => new AttemptRecord {
            AttemptId=attempt.AttemptId.ToString("D"), PuzzleId=attempt.Level.PuzzleId,
            CampaignNodeId=attempt.Level.CampaignNodeId, ContentHash=attempt.PinnedContentHash,
            RulesVersion=attempt.PinnedRulesVersion, Locale=attempt.ActiveLocale,
            RequiredStampBits=attempt.Goal.RequiredStampBits, UndoCount=attempt.UndoCount,
            HintUsed=attempt.HintUsed, StateHash=attempt.CurrentStateHash,
            Commands=attempt.History.Select(h=>h.Command.ToString()).ToArray()
        };
    }
}
