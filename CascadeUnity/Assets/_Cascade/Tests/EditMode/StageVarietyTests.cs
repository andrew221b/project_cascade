using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using NUnit.Framework;
using UnityEngine;
namespace Cascade.Tests
{
    public sealed class StageVarietyTests
    {
        ContentCatalog catalog;StageVarietyStore store;
        static string Read(string name)=>Resources.Load<TextAsset>("CascadeContent/"+name).text;
        [OneTimeSetUp] public void Load(){catalog=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));store=new StageVarietyStore(Read("stage_words_v1"),Read("stage_variety_templates_v1"),Read("production_triad_recipes"));}
        static PuzzleCommand Parse(string value)=>value=="D"?PuzzleCommand.Drop:value.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(value.Substring(1).Split(':')[0]),int.Parse(value.Split(':')[1])):PuzzleCommand.Swap(int.Parse(value));
        static void Win(AttemptService attempt,DescentStage stage){foreach(string encoded in stage.Witness)Assert.That(attempt.Execute(Parse(encoded)).Accepted,Is.True);Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won));}
        StageVarietyMemory Memory(string seed="0123456789abcdef0123456789abcdef")=>new StageVarietyMemory{Seed=seed};
        DescentRun New(string node,string mode,string locale,StageVarietyMemory memory){var source=catalog.GetPrimary(node).Definition;return store.Create(source,memory.Reserve(source,mode,locale,store.Words),store.Published(source));}
        [TestCase("en"),TestCase("ru"),TestCase("uk")] public void MixedStartsWithARealWordAndShapesAndChangesEveryStage(string locale)
        {
            var run=New("C01","Mixed",locale,Memory());var words=new HashSet<string>();
            for(int stage=0;stage<3;stage++){var level=run.Level(catalog.GetPrimary("C01").Definition,stage);Assert.That(level.TargetSymbolIds.Count,Is.EqualTo(4));Assert.That(level.TargetSymbolIds.Count(s=>s.Kind==SymbolKind.Glyph),Is.EqualTo(3));Assert.That(level.TargetSymbolIds.Any(s=>s.Kind==SymbolKind.Shape),Is.True);Assert.That(words.Add(run.Variation.Symbols[stage].WordId),Is.True);}
        }
        [TestCase(PuzzleMode.Shapes),TestCase(PuzzleMode.Words),TestCase(PuzzleMode.Mixed)] public void EveryCategoryOffersAll540StableNodes(PuzzleMode mode)
        {var levels=StageJourney.Sequence(catalog,mode,"uk");Assert.That(levels.Length,Is.EqualTo(540));Assert.That(levels.Select(l=>l.Definition.CampaignNodeId).Distinct().Count(),Is.EqualTo(540));Assert.That(levels.All(l=>l.Mode==mode),Is.True);}
        [Test] public void IndividualOrderSurvivesSerializationAndDiffersBetweenPlayers()
        {
            var source=catalog.GetPrimary("C01").Definition;var a=Memory();var b=Memory("fedcba9876543210fedcba9876543210");var first=New("C01","Words","ru",a);var other=New("C01","Words","ru",b);
            Assert.That(first.Variation.Symbols.Select(s=>s.Word).SequenceEqual(other.Variation.Symbols.Select(s=>s.Word)),Is.False);
            var restored=CloudJson.Decode<StageVarietyMemory>(CloudJson.Encode(a));var one=store.Expand(source,a.Reserve(source,"Words","ru",store.Words));var two=store.Expand(source,restored.Reserve(source,"Words","ru",store.Words));Assert.That(one.Fingerprint,Is.EqualTo(two.Fingerprint));
            Assert.That(one.Symbols.Select(s=>s.WordId).Intersect(first.Variation.Symbols.Select(s=>s.WordId)),Is.Empty);
        }
        [TestCase("en"),TestCase("ru"),TestCase("uk")] public void CompleteWordDecksHaveNoRepetitionAndNoBoundaryDuplicate(string locale)
        {
            for(int length=3;length<=8;length++){int n=store.Words.Size(locale,length);int previous=-1;for(int cycle=0;cycle<3;cycle++){var deck=store.Words.Permutation(locale,length,Memory().Seed,cycle);Assert.That(deck.Distinct().Count(),Is.EqualTo(n));Assert.That(deck[0],Is.Not.EqualTo(previous));previous=deck[n-1];}}
        }
        [Test] public void RetryingTheNextStageChangesItsWordWithoutRewritingTheCompletedProof()
        {
            var source=catalog.GetPrimary("C01").Definition;var memory=Memory();var run=New("C01","Mixed","uk",memory);var first=new AttemptService(run.Level(source,0),"uk");Win(first,run.Stages[0]);run.AcceptCheckpoint(source,first);string originalHash=run.StageHash(0);
            var plan=memory.Renew(source,run.Variation,1,store.Words);var fresh=store.Personalize(source,plan,run.Stages);fresh.Completed=run.Completed.ToList();fresh.CompletedExplosions=run.CompletedExplosions;fresh.CompletedToolFlags=run.CompletedToolFlags;fresh.Validate(source,"uk");
            Assert.That(fresh.StageHash(0),Is.EqualTo(originalHash));Assert.That(fresh.Variation.Symbols[0].Word,Is.EqualTo(run.Variation.Symbols[0].Word));Assert.That(fresh.Variation.Symbols[1].WordId,Is.Not.EqualTo(run.Variation.Symbols[1].WordId));
            var attempt=new AttemptService(fresh.Level(source,1),"uk");var p=new PlayerProfile{Descent=fresh,Attempt=AttemptRecord.Capture(attempt),PreferredMode="Mixed",Variety=memory};var copy=CloudJson.Decode<PlayerProfile>(CloudJson.Encode(p));Assert.That(copy.RestoreAttempt(catalog,variety:store).CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
        }
        [Test] public void MergingDevicesRetainsTheirUsedWordsEvenWhenTheirShuffleSeedsDiffer()
        {
            var source=catalog.GetPrimary("C01").Definition;var a=Memory();var b=Memory("fedcba9876543210fedcba9876543210");var used=new HashSet<string>();
            for(int i=0;i<12;i++)foreach(var memory in new[]{a,b})foreach(var symbol in store.Expand(source,memory.Reserve(source,"Words","ru",store.Words)).Symbols)used.Add(symbol.WordId);
            var merged=StageVarietyMemory.Merge(b,a);foreach(var symbol in store.Expand(source,merged.Reserve(source,"Words","ru",store.Words)).Symbols)Assert.That(used.Contains(symbol.WordId),Is.False);merged.ValidatePool(store.Words);Assert.That(CloudJson.Encode(merged).Length,Is.LessThan(20000));
        }
        [TestCase("Shapes"),TestCase("Words"),TestCase("Mixed")] public void PersonalMissionsUseFreshVocabularyAndRestoreTheirOwnCertifiedBoards(string mode)
        {
            var source=catalog.GetPrimary("C05").Definition;var basis=StageContentBasis.Canonical(source,mode);var reference=store.CanonicalReference(source,mode);
            using var stop=new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(8));
            var mission=CampaignTriads.BuildPersonalMission(basis,1,null,stop.Token,reference:reference);Assert.That(mission,Is.Not.Null);
            var memory=Memory();var locale=mode=="Shapes"?"en":"uk";var plan=memory.Reserve(source,mode,locale,store.Words);var run=store.Personalize(source,plan,mission.Stages);var attempt=new AttemptService(run.Level(source,0),locale);
            var campaign=store.Create(source,memory.Reserve(source,mode,locale,store.Words),store.Published(source));var old=new AttemptService(campaign.Level(source,0),locale);
            var p=new PlayerProfile{Attempt=AttemptRecord.Capture(attempt),Descent=run,PreferredMode=mode,Variety=memory};p.Missions.Present(new PersonalMissionOffer{Serial=1,Hero=0,SourceHash=source.ContentHash,Locale=locale,Run=run},3);p.Missions.Active=true;p.Missions.CampaignAttempt=AttemptRecord.Capture(old);p.Missions.CampaignDescent=campaign;
            p.Validate();var copy=CloudJson.Decode<PlayerProfile>(CloudJson.Encode(p));Assert.That(copy.RestoreAttempt(catalog,variety:store).CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
            for(int stage=0;stage<3;stage++)Win(new AttemptService(run.Level(source,stage),locale),run.Stages[stage]);
        }
        [TestCase("Shapes"),TestCase("Mixed")] public void RetryShapesDifferFromBothNeighboursAndTheFollowingRound(string mode)
        {
            var source=catalog.GetPrimary("C01").Definition;var memory=Memory();string Shape(StageSymbols symbols)=>string.Concat(symbols.Target.Where(id=>id.StartsWith("s:",StringComparison.Ordinal)));
            string last=null;
            for(int round=0;round<24;round++){
                var run=New("C01",mode,"en",memory);Assert.That(Shape(run.Variation.Symbols[0]),Is.Not.EqualTo(last));
                for(int stage=0;stage<3;stage++){
                    var prior=run.Variation.Symbols.Select(Shape).ToArray();run=store.Personalize(source,memory.Renew(source,run.Variation,stage,store.Words),run.Stages);
                    Assert.That(prior.Contains(Shape(run.Variation.Symbols[stage])),Is.False);
                }
                last=Shape(run.Variation.Symbols[2]);
            }
        }
        [Test] public void CorruptedMetadataAndWordHistoryRejectBeforeReconstruction()
        {
            var source=catalog.GetPrimary("C01").Definition;var memory=Memory();var run=New("C01","Words","en",memory);
            var bad=CloudJson.Decode<StageVariation>(CloudJson.Encode(run.Variation));bad.Locale="xx";Assert.Throws<ArgumentException>(()=>store.Create(source,bad,store.Published(source)));
            bad=CloudJson.Decode<StageVariation>(CloudJson.Encode(run.Variation));bad.WordDraws[0]++;Assert.Throws<ArgumentException>(()=>store.Create(source,bad,store.Published(source)));
            memory.UsedWords["en:3"]="AA==";Assert.Throws<InvalidDataException>(()=>memory.ValidatePool(store.Words));
        }
        [Test] public void HistoricalCatalogPinsAndVersionSixCheckpointsRemainExact()
        {
            Assert.That(catalog.ContentHash,Is.EqualTo("3512889ac0cc3d8e8f943d996d44c6452e621b36237db40ab7583510a4813d1d"));var source=catalog.GetPrimary("C01").Definition;var run=store.Published(source);var first=new AttemptService(run.Level(source,0));Win(first,run.Stages[0]);run.AcceptCheckpoint(source,first);var current=new AttemptService(run.Level(source,1));var p=new PlayerProfile{Descent=run,Attempt=AttemptRecord.Capture(current)};var restored=CloudJson.Decode<PlayerProfile>(CloudJson.Encode(p));Assert.That(restored.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(current.CurrentStateHash));
        }
    }
}
