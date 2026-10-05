using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cascade.Domain
{
    public sealed class ValidationIssue
    {
        public string Path { get; }
        public string Message { get; }
        public ValidationIssue(string path, string message) { Path = path; Message = message; }
        public override string ToString() => Path + ": " + Message;
    }

    public static class LevelValidator
    {
        public static ReadOnlyCollection<ValidationIssue> Validate(LevelDefinition level,
            IReadOnlyDictionary<string, WordDefinition> words = null)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            var issues = new List<ValidationIssue>();
            void Check(bool valid, string path, string message)
            { if (!valid) issues.Add(new ValidationIssue(path, message)); }
            Check(!string.IsNullOrWhiteSpace(level.PuzzleId), "PuzzleId", "Required.");
            Check(!string.IsNullOrWhiteSpace(level.CampaignNodeId), "CampaignNodeId", "Required.");
            Check(!string.IsNullOrWhiteSpace(level.SchemaVersion), "SchemaVersion", "Required.");
            Check(!string.IsNullOrWhiteSpace(level.RulesVersion), "RulesVersion", "Required.");
            Check(!string.IsNullOrWhiteSpace(level.ContentHash), "ContentHash", "Required.");
            Check(level.ReachCharges >= 0 && level.ReachCharges <= 1, "ReachCharges", "Expected zero or one.");
            int n = level.InitialSymbolIds.Count;
            Check(n >= 3 && n <= 8, "InitialSymbolIds", "Row length must be 3–8.");
            Check(level.TargetSymbolIds.Count == n, "TargetSymbolIds", "Row lengths differ.");
            Check(level.InitialSymbolIds.All(s => s.IsValid) && level.TargetSymbolIds.All(s => s.IsValid),
                "SymbolIds", "Invalid symbol.");
            Check(SameMultiset(level.InitialSymbolIds, level.TargetSymbolIds), "TargetSymbolIds", "Multiset differs.");
            Check(level.Floors.Count >= 1 && level.Floors.Count <= 4, "Floors", "Floor count must be 1–4.");

            for (int i = 0; i < level.Floors.Count; i++)
            {
                var floor = level.Floors[i]; string path = "Floors[" + i + "]";
                if (floor == null) { Check(false, path, "Missing floor."); continue; }
                bool Edge(int e) => e >= 0 && e < n - 1;
                Check(floor.BaseDurability >= 1 && floor.BaseDurability <= 12, path + ".BaseDurability", "Expected 1–12.");
                Check(floor.OpenEdges.Count == n - 1, path + ".OpenEdges", "Expected N−1 edges.");
                Check(Enum.IsDefined(typeof(EntryTransform), floor.EntryTransform), path + ".EntryTransform", "Unknown transform.");
                Check(floor.FragileEdges.Distinct().Count() == floor.FragileEdges.Count && floor.FragileEdges.All(Edge),
                    path + ".FragileEdges", "Expected distinct valid edges.");
                if (floor.LinkedPair.HasValue)
                {
                    var pair = floor.LinkedPair.Value;
                    Check(Edge(pair.FirstEdge) && Edge(pair.SecondEdge) && Math.Abs((long)pair.FirstEdge - pair.SecondEdge) > 1,
                        path + ".LinkedPair", "Linked pairs must be disjoint adjacent edges.");
                }
                if (floor.OneUseBridgeEdge.HasValue)
                {
                    int edge = floor.OneUseBridgeEdge.Value;
                    Check(Edge(edge) && edge < floor.OpenEdges.Count && floor.OpenEdges[edge], path + ".OneUseBridgeEdge", "Bridge must start open.");
                    Check(floor.Key == null || floor.Key.EdgeToOpen != edge, path + ".OneUseBridgeEdge", "Bridge and key cannot share an edge.");
                }
                void Contact(ContactDefinition device, string name)
                {
                    if (device == null) return;
                    Check(device.Slot >= 0 && device.Slot < n, path + "." + name, "Invalid sensor slot.");
                    Check(device.RequiredSymbol.IsValid && level.TargetSymbolIds.Contains(device.RequiredSymbol),
                        path + "." + name, "Required symbol is absent from target.");
                }
                Contact(floor.Bomb, "Bomb"); Contact(floor.Key, "Key");
                Contact(floor.Repair, "Repair"); Contact(floor.OptionalStamp, "OptionalStamp");Contact(floor.Powerup,"Powerup");
                if(floor.Powerup!=null)Check(Enum.IsDefined(typeof(PowerupKind),floor.Powerup.Kind),path+".Powerup.Kind","Unknown pickup.");
                if (floor.Key != null)
                {
                    int edge = floor.Key.EdgeToOpen;
                    Check(Edge(edge) && edge < floor.OpenEdges.Count && !floor.OpenEdges[edge], path + ".Key", "Key must open a closed edge.");
                }
                if (floor.Repair != null)
                    Check(floor.Repair.Amount >= 1 && floor.Repair.Amount <= 3, path + ".Repair", "Amount must be 1–3.");
                if (floor.Bomb != null)
                    Check(floor.Bomb.Ticks >= 1 && floor.Bomb.Ticks <= 6 && floor.Bomb.Damage >= 1 && floor.Bomb.Damage <= 4,
                        path + ".Bomb", "Ticks must be 1–6; damage 1–4.");
            }

            var covered = new bool[level.TargetSymbolIds.Count];
            for (int i = 0; i < level.TargetSegments.Count; i++)
            {
                var segment = level.TargetSegments[i]; string path = "TargetSegments[" + i + "]";
                if (segment == null) { Check(false, path, "Missing segment."); continue; }
                bool range = segment.Start >= 0 && segment.Length > 0 && (long)segment.Start + segment.Length <= covered.Length;
                Check(range, path, "Segment must fit target.");
                Check(Enum.IsDefined(typeof(SegmentKind), segment.Kind), path, "Unknown segment kind.");
                Check(segment.Kind != SegmentKind.Word || !string.IsNullOrWhiteSpace(segment.WordId), path, "WordId required.");
                Check(segment.Kind != SegmentKind.Shapes || string.IsNullOrEmpty(segment.WordId), path, "Shape segment cannot bind a word.");
                if (!range) continue;
                string locale = null;
                for (int j = segment.Start; j < segment.Start + segment.Length; j++)
                {
                    Check(!covered[j], path, "Segments overlap."); covered[j] = true;
                    var symbol = level.TargetSymbolIds[j];
                    Check(symbol.Kind == (segment.Kind == SegmentKind.Shapes ? SymbolKind.Shape : SymbolKind.Glyph), path, "Segment namespace differs.");
                    if (segment.Kind == SegmentKind.Word)
                    {
                        if (locale == null) locale = symbol.Locale;
                        Check(symbol.Locale == locale, path, "A word cannot mix puzzle locales.");
                    }
                }
                if (segment.Kind == SegmentKind.Word && words != null)
                {
                    WordDefinition word = null;
                    Check(segment.WordId != null && words.TryGetValue(segment.WordId, out word) && word != null, path, "Unknown WordId.");
                    if (word != null)
                    {
                        Check(word.Locale == locale && word.Graphemes.Count == segment.Length, path, "Word locale/length differs.");
                        if (word.Graphemes.Count == segment.Length)
                            for (int j = 0; j < segment.Length; j++)
                                Check(word.Graphemes[j] == level.TargetSymbolIds[segment.Start + j].Value, path, "Word graphemes differ from target.");
                    }
                }
            }
            Check(covered.All(v => v), "TargetSegments", "Segments must cover the entire target.");
            return issues.AsReadOnly();
        }

        public static bool SameMultiset(IEnumerable<SymbolId> first, IEnumerable<SymbolId> second)
        {
            var counts = new Dictionary<SymbolId, int>();
            foreach (var symbol in first) { counts.TryGetValue(symbol, out int count); counts[symbol] = count + 1; }
            foreach (var symbol in second)
            {
                if (!counts.TryGetValue(symbol, out int count) || count == 0) return false;
                counts[symbol] = count - 1;
            }
            return counts.Values.All(v => v == 0);
        }

        public static void ThrowIfInvalid(LevelDefinition level, IReadOnlyDictionary<string, WordDefinition> words = null)
        {
            var issues = Validate(level, words);
            if (issues.Count != 0) throw new ArgumentException(string.Join("\n", issues), nameof(level));
        }
    }
}
