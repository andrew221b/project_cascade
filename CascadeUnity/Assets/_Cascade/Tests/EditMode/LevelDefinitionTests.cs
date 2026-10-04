using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Domain;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class LevelDefinitionTests
    {
        internal static SymbolId S(string value) => SymbolId.Parse("s:" + value);
        internal static SymbolId[] Row(string values) => values.Select(c => S(c.ToString())).ToArray();
        internal static LevelDefinition Level(SymbolId[] start = null, SymbolId[] target = null,
            FloorDefinition[] floors = null, TargetSegment[] segments = null)
        {
            start = start ?? Row("BAC"); target = target ?? Row("ABC");
            return new LevelDefinition("test", "test", "2.0", "2.0", "fixture",
                start, target, segments ?? new[] { new TargetSegment(SegmentKind.Shapes, 0, target.Length) },
                floors ?? new[] { new FloorDefinition(2, Enumerable.Repeat(true, start.Length - 1)) });
        }
        static void Invalid(LevelDefinition level, string path)
        { Assert.That(LevelValidator.Validate(level).Any(i => i.Path.Contains(path)), Is.True, path); }

        [Test] public void CopiesInputCollectionsAndExposesReadOnlyViews()
        {
            var row = Row("BAC"); var edges = new[] { true, true }; var fragile = new[] { 0 };
            var floor = new FloorDefinition(2, edges, fragile);
            var floors = new[] { floor }; var segments = new[] { new TargetSegment(SegmentKind.Shapes, 0, 3) };
            var level = Level(start: row, floors: floors, segments: segments);
            row[0] = S("X"); edges[0] = false; fragile[0] = 1; floors[0] = null; segments[0] = null;
            Assert.That(level.InitialSymbolIds[0], Is.EqualTo(S("B")));
            Assert.That(level.Floors[0].OpenEdges[0], Is.True);
            Assert.That(level.Floors[0].FragileEdges[0], Is.Zero);
            Assert.That(LevelValidator.Validate(level), Is.Empty);
            Assert.Throws<NotSupportedException>(() => ((IList<SymbolId>)level.InitialSymbolIds)[0] = S("X"));
        }

        [Test] public void RepeatedSymbolsUseEqualityRatherThanPosition()
        { Assert.That(LevelValidator.Validate(Level(Row("BAA"), Row("AAB"))), Is.Empty); }
        [Test] public void RejectsChangedMultiset()
        { Invalid(Level(target: Row("ABD")), "TargetSymbolIds"); }

        [TestCase(2, false)] [TestCase(3, true)] [TestCase(8, true)] [TestCase(9, false)]
        public void RowBoundaries(int n, bool valid)
        {
            var row = Enumerable.Repeat(S("A"), n).ToArray();
            Assert.That(LevelValidator.Validate(Level(row, row)).Count == 0, Is.EqualTo(valid));
        }
        [TestCase(0, false)] [TestCase(1, true)] [TestCase(4, true)] [TestCase(5, false)]
        public void FloorBoundaries(int n, bool valid)
        {
            var floors = Enumerable.Range(0, n).Select(_ => new FloorDefinition(1, new[] { true, true })).ToArray();
            Assert.That(LevelValidator.Validate(Level(floors: floors)).Count == 0, Is.EqualTo(valid));
        }
        [TestCase(0, false)] [TestCase(1, true)] [TestCase(12, true)] [TestCase(13, false)]
        public void DurabilityBoundaries(int hp, bool valid)
        { Assert.That(LevelValidator.Validate(Level(floors: new[] { new FloorDefinition(hp, new[] { true, true }) })).Count == 0, Is.EqualTo(valid)); }

        [Test] public void RejectsWrongEdgeCountAndDuplicateFragileEdges()
        {
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true }) }), "OpenEdges");
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, new[] { 0, 0 }) }), "FragileEdges");
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, new[] { 2 }) }), "FragileEdges");
        }
        [Test] public void LinkedPairsMustBeDisjointAndInRange()
        {
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, linkedPair: new LinkedPair(0, 1)) }), "LinkedPair");
            var row = Row("ABCD");
            Assert.That(LevelValidator.Validate(Level(row, row, new[] { new FloorDefinition(1, new[] { true, true, true }, linkedPair: new LinkedPair(0, 2)) })), Is.Empty);
        }
        [Test] public void RejectsInvalidBridgeAndKeyConfiguration()
        {
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { false, true }, oneUseBridgeEdge: 0) }), "OneUseBridgeEdge");
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, key: new KeyDefinition(0, S("A"), 0)) }), ".Key");
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, key: new KeyDefinition(0, S("A"), 0), oneUseBridgeEdge: 0) }), "OneUseBridgeEdge");
        }
        [Test] public void SensorMustFitRowAndReferenceItsTarget()
        {
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, repair: new RepairDefinition(3, S("A"), 1)) }), "Repair");
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, bomb: new BombDefinition(0, S("X"), 2, 2)) }), "Bomb");
        }
        [TestCase(0, 1, false)] [TestCase(1, 1, true)] [TestCase(6, 4, true)] [TestCase(7, 4, false)] [TestCase(2, 5, false)]
        public void BombBoundaries(int ticks, int damage, bool valid)
        { Assert.That(LevelValidator.Validate(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, bomb: new BombDefinition(0, S("A"), ticks, damage)) })).Count == 0, Is.EqualTo(valid)); }
        [TestCase(0, false)] [TestCase(1, true)] [TestCase(3, true)] [TestCase(4, false)]
        public void RepairBoundaries(int amount, bool valid)
        { Assert.That(LevelValidator.Validate(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, repair: new RepairDefinition(0, S("A"), amount)) })).Count == 0, Is.EqualTo(valid)); }

        [Test] public void ShapeAndGlyphRemainDifferent()
        { Assert.That(SymbolId.Parse("shape:O"), Is.Not.EqualTo(SymbolId.Parse("glyph:en:O"))); }
        [Test] public void ShortAndLongNamespacesAgreeAndPreserveOriginalId()
        {
            var symbol = SymbolId.Parse("g:en:A");
            Assert.That(symbol, Is.EqualTo(SymbolId.Parse("glyph:en:A")));
            Assert.That(symbol.PersistedId, Is.EqualTo("g:en:A"));
            Assert.That(SymbolId.Parse("s:T"), Is.EqualTo(SymbolId.Parse("shape:T")));
        }
        [TestCase("g:ua:І")] [TestCase("glyph:en:AB")] [TestCase("s:")] [TestCase("other:A")]
        public void RejectsInvalidIds(string id) { Assert.Throws<FormatException>(() => SymbolId.Parse(id)); }
        [Test] public void NormalizesCombiningTextAndDoesNotSplitSurrogatePairs()
        {
            Assert.That(SymbolId.Parse("g:en:e\u0301"), Is.EqualTo(SymbolId.Parse("g:en:é")));
            Assert.That(GraphemeText.Split("😀Їe\u0301"), Is.EqualTo(new[] { "😀", "Ї", "é" }));
        }
        [Test] public void UkrainianAndRussianDistinctLettersRemainDistinct()
        {
            foreach (var pair in new[] { new[] { "Е", "Ё" }, new[] { "И", "І" }, new[] { "І", "Ї" }, new[] { "Г", "Ґ" }, new[] { "Е", "Є" } })
                Assert.That(SymbolId.Parse("g:uk:" + pair[0]), Is.Not.EqualTo(SymbolId.Parse("g:uk:" + pair[1])));
        }
        [Test] public void PositionalWordSignaturesDiffer()
        {
            Assert.That(new WordDefinition("one", "en", "ABAB").EqualitySignature, Is.EqualTo(new[] { 0, 1, 0, 1 }));
            Assert.That(new WordDefinition("two", "en", "AABB").EqualitySignature, Is.EqualTo(new[] { 0, 0, 1, 1 }));
        }
        [Test] public void SevenSymbolMixedTargetIsAccepted()
        {
            var target = new[] { S("T"), S("T"), SymbolId.Parse("g:en:C"), SymbolId.Parse("g:en:A"), SymbolId.Parse("g:en:T"), S("O"), S("O") };
            var segments = new[] { new TargetSegment(SegmentKind.Shapes, 0, 2), new TargetSegment(SegmentKind.Word, 2, 3, "en_CAT"), new TargetSegment(SegmentKind.Shapes, 5, 2) };
            var words = new Dictionary<string, WordDefinition> { ["en_CAT"] = new WordDefinition("en_CAT", "en", "CAT") };
            Assert.That(LevelValidator.Validate(Level(target.Reverse().ToArray(), target, segments: segments), words), Is.Empty);
            words["en_CAT"] = new WordDefinition("en_CAT", "en", "COT");
            Assert.That(LevelValidator.Validate(Level(target, target, segments: segments), words), Is.Not.Empty);
        }
        [Test] public void SegmentsMustCoverTargetWithoutOverlapAndMatchNamespace()
        {
            Invalid(Level(segments: new[] { new TargetSegment(SegmentKind.Shapes, 0, 2) }), "TargetSegments");
            Invalid(Level(segments: new[] { new TargetSegment(SegmentKind.Shapes, 0, 3), new TargetSegment(SegmentKind.Shapes, 1, 1) }), "TargetSegments");
            Invalid(Level(segments: new[] { new TargetSegment(SegmentKind.Word, 0, 3, "en_CAT") }), "TargetSegments");
            Invalid(Level(segments: new[] { new TargetSegment(SegmentKind.Shapes, int.MaxValue, int.MaxValue) }), "TargetSegments");
        }
        [Test] public void DefaultSymbolAndUnknownTransformAreRejected()
        {
            var row = Row("ABC"); row[0] = default;
            Invalid(Level(row, row), "SymbolIds");
            Invalid(Level(floors: new[] { new FloorDefinition(1, new[] { true, true }, entryTransform: (EntryTransform)999) }), "EntryTransform");
        }
    }
}
