using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Cascade.Domain
{
    internal static class Frozen
    {
        internal static ReadOnlyCollection<T> Copy<T>(IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return Array.AsReadOnly(source.ToArray());
        }
    }

    public enum SegmentKind { Shapes, Word }
    public enum EntryTransform { None, ShiftLeftOne, ShiftRightOne, MirrorNext }

    public sealed class TargetSegment
    {
        public SegmentKind Kind { get; }
        public int Start { get; }
        public int Length { get; }
        public string WordId { get; }
        public TargetSegment(SegmentKind kind, int start, int length, string wordId = null)
        { Kind = kind; Start = start; Length = length; WordId = wordId; }
    }

    public readonly struct LinkedPair
    {
        public int FirstEdge { get; }
        public int SecondEdge { get; }
        public LinkedPair(int firstEdge, int secondEdge)
        { FirstEdge = firstEdge; SecondEdge = secondEdge; }
        public bool Contains(int edge) => edge == FirstEdge || edge == SecondEdge;
    }

    public class ContactDefinition
    {
        public int Slot { get; }
        public SymbolId RequiredSymbol { get; }
        public ContactDefinition(int slot, SymbolId requiredSymbol)
        { Slot = slot; RequiredSymbol = requiredSymbol; }
    }

    public sealed class KeyDefinition : ContactDefinition
    {
        public int EdgeToOpen { get; }
        public KeyDefinition(int slot, SymbolId requiredSymbol, int edgeToOpen) : base(slot, requiredSymbol)
        { EdgeToOpen = edgeToOpen; }
    }

    public sealed class RepairDefinition : ContactDefinition
    {
        public int Amount { get; }
        public RepairDefinition(int slot, SymbolId requiredSymbol, int amount) : base(slot, requiredSymbol)
        { Amount = amount; }
    }

    public sealed class BombDefinition : ContactDefinition
    {
        public int Ticks { get; }
        public int Damage { get; }
        public BombDefinition(int slot, SymbolId requiredSymbol, int ticks, int damage) : base(slot, requiredSymbol)
        { Ticks = ticks; Damage = damage; }
    }

    public sealed class FloorDefinition
    {
        public int BaseDurability { get; }
        public ReadOnlyCollection<bool> OpenEdges { get; }
        public ReadOnlyCollection<int> FragileEdges { get; }
        public LinkedPair? LinkedPair { get; }
        public EntryTransform EntryTransform { get; }
        public BombDefinition Bomb { get; }
        public KeyDefinition Key { get; }
        public RepairDefinition Repair { get; }
        public int? OneUseBridgeEdge { get; }
        public ContactDefinition OptionalStamp { get; }

        public FloorDefinition(int baseDurability, IEnumerable<bool> openEdges,
            IEnumerable<int> fragileEdges = null, LinkedPair? linkedPair = null,
            EntryTransform entryTransform = EntryTransform.None, BombDefinition bomb = null,
            KeyDefinition key = null, RepairDefinition repair = null,
            int? oneUseBridgeEdge = null, ContactDefinition optionalStamp = null)
        {
            BaseDurability = baseDurability;
            OpenEdges = Frozen.Copy(openEdges);
            FragileEdges = Frozen.Copy(fragileEdges ?? Array.Empty<int>());
            LinkedPair = linkedPair;
            EntryTransform = entryTransform;
            Bomb = bomb; Key = key; Repair = repair;
            OneUseBridgeEdge = oneUseBridgeEdge; OptionalStamp = optionalStamp;
        }
    }

    public sealed class WordDefinition
    {
        public string WordId { get; }
        public string Locale { get; }
        public string DisplayText { get; }
        public ReadOnlyCollection<string> Graphemes { get; }
        public ReadOnlyCollection<int> EqualitySignature { get; }
        public WordDefinition(string wordId, string locale, string displayText)
        {
            WordId = wordId; Locale = locale;
            Graphemes = GraphemeText.Split(displayText);
            DisplayText = string.Concat(Graphemes);
            var seen = new Dictionary<string, int>(StringComparer.Ordinal);
            EqualitySignature = Frozen.Copy(Graphemes.Select(g =>
            {
                if (!seen.TryGetValue(g, out var index)) { index = seen.Count; seen.Add(g, index); }
                return index;
            }));
        }
    }

    public sealed class LevelDefinition
    {
        public string PuzzleId { get; }
        public string CampaignNodeId { get; }
        public string SchemaVersion { get; }
        public string RulesVersion { get; }
        public string ContentHash { get; }
        public ReadOnlyCollection<SymbolId> InitialSymbolIds { get; }
        public ReadOnlyCollection<SymbolId> TargetSymbolIds { get; }
        public ReadOnlyCollection<TargetSegment> TargetSegments { get; }
        public ReadOnlyCollection<FloorDefinition> Floors { get; }

        public LevelDefinition(string puzzleId, string campaignNodeId, string schemaVersion,
            string rulesVersion, string contentHash, IEnumerable<SymbolId> initialSymbolIds,
            IEnumerable<SymbolId> targetSymbolIds, IEnumerable<TargetSegment> targetSegments,
            IEnumerable<FloorDefinition> floors)
        {
            PuzzleId = puzzleId; CampaignNodeId = campaignNodeId; SchemaVersion = schemaVersion;
            RulesVersion = rulesVersion; ContentHash = contentHash;
            InitialSymbolIds = Frozen.Copy(initialSymbolIds); TargetSymbolIds = Frozen.Copy(targetSymbolIds);
            TargetSegments = Frozen.Copy(targetSegments); Floors = Frozen.Copy(floors);
        }
    }
}
