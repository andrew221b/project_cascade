using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cascade.Domain;

namespace Cascade.Content
{
    public sealed class WordEntry
    {
        public WordDefinition Definition { get; }
        public string EditorialStatus { get; }
        public string Origin { get; }
        public string LicenseReview { get; }
        internal WordEntry(WordDefinition definition, string editorialStatus, string origin, string licenseReview)
        { Definition = definition; EditorialStatus = editorialStatus; Origin = origin; LicenseReview = licenseReview; }
    }

    public sealed class LevelContent
    {
        public LevelDefinition Definition { get; }
        public string Locale { get; }
        public PuzzleMode Mode { get; }
        public string Title { get; }
        public string Lesson { get; }
        public bool IsPrimary { get; }
        public ReadOnlyCollection<string> WordIds { get; }
        internal LevelContent(LevelDefinition definition, string locale, string title, string lesson, bool isPrimary)
        {
            Definition = definition; Locale = locale; Title = title; Lesson = lesson; IsPrimary = isPrimary;
            bool shapes = definition.TargetSymbolIds.Any(s => s.Kind == SymbolKind.Shape);
            bool glyphs = definition.TargetSymbolIds.Any(s => s.Kind == SymbolKind.Glyph);
            Mode = shapes && glyphs ? PuzzleMode.Mixed : glyphs ? PuzzleMode.Words : PuzzleMode.Shapes;
            WordIds = Array.AsReadOnly(definition.TargetSegments.Where(s => s.Kind == SegmentKind.Word).Select(s => s.WordId).ToArray());
        }
    }

    public sealed class ContentCatalog
    {
        public const string DefaultLocale = "en";
        public ReadOnlyCollection<LevelContent> PrimaryLevels { get; }
        public ReadOnlyCollection<LevelContent> LocalizedLevels { get; }
        public ReadOnlyDictionary<string, WordEntry> WordEntries { get; }
        public ReadOnlyDictionary<string, WordDefinition> Words { get; }
        public string ContentHash { get; }
        readonly Dictionary<string, LevelContent> primary;
        readonly Dictionary<string, LevelContent> localized;

        internal ContentCatalog(IEnumerable<LevelContent> primaryLevels, IEnumerable<LevelContent> localizedLevels,
            IDictionary<string, WordEntry> words, string contentHash)
        {
            PrimaryLevels = Array.AsReadOnly(primaryLevels.ToArray()); LocalizedLevels = Array.AsReadOnly(localizedLevels.ToArray());
            WordEntries = new ReadOnlyDictionary<string, WordEntry>(new Dictionary<string, WordEntry>(words));
            Words = new ReadOnlyDictionary<string, WordDefinition>(words.ToDictionary(p => p.Key, p => p.Value.Definition));
            primary = PrimaryLevels.ToDictionary(l => l.Definition.CampaignNodeId, StringComparer.Ordinal);
            localized = LocalizedLevels.ToDictionary(l => l.Definition.CampaignNodeId + ":" + l.Locale, StringComparer.Ordinal);
            ContentHash = contentHash;
        }
        public LevelContent GetPrimary(string nodeId) => primary.TryGetValue(nodeId, out var level) ? level
            : throw new KeyNotFoundException("Unknown campaign node: " + nodeId);
        public bool TryGetLocalizedBinding(string nodeId, string locale, out LevelContent level) => localized.TryGetValue(nodeId + ":" + locale, out level);
        public bool IsShapeOnlyLesson(string nodeId) => primary.ContainsKey(nodeId) && !LocalizedLevels.Any(l => l.Definition.CampaignNodeId == nodeId);
    }
}
