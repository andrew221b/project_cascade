using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class DirectorTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp] public void Setup()
        {
            string Read(string file)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file));
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
        }
        AttemptService Win(LevelDefinition source,DeliveryMood mood=DeliveryMood.Steady)
        {
            var recipe=DeliveryDirector.Plan(source,mood);var attempt=new AttemptService(recipe?.Apply(source)??source,direction:recipe);
            var proof=PuzzleSolver.FindShortest(attempt.Resolver);Assert.That(proof.Status,Is.EqualTo(SearchStatus.Solved));
            foreach(var command in proof.Path)attempt.Execute(command);return attempt;
        }
        LevelDefinition Fixture()
        {
            var a=SymbolId.Parse("s:A");var b=SymbolId.Parse("s:B");var c=SymbolId.Parse("s:C");
            return new LevelDefinition("director-fixture","C81","1","2.0",new string('a',64),new[]{b,a,c},new[]{a,b,c},
                new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(3,new[]{true,true}),new FloorDefinition(3,new[]{true,true})});
        }
        [Test] public void DirectorAlternatesConfidencePeakAndRecoveryAndDoesNotCountAResultTwice()
        {
            var memory=new DirectorMemory();var source=Fixture();
            Assert.That(memory.Choose(source),Is.EqualTo(DeliveryMood.Steady));
            var first=Win(source);memory.ObserveTerminal(first,first.RouteLength);memory.ObserveTerminal(first,first.RouteLength);
            Assert.That(memory.Deliveries,Is.EqualTo(1));
            var second=Win(source);memory.ObserveTerminal(second,second.RouteLength);
            Assert.That(memory.Choose(source),Is.EqualTo(DeliveryMood.Wind));
            var peak=Win(source,DeliveryMood.Wind);memory.ObserveTerminal(peak,peak.RouteLength);
            Assert.That(memory.Choose(source),Is.EqualTo(DeliveryMood.Recovery));
            var rest=Win(source,DeliveryMood.Recovery);memory.ObserveTerminal(rest,rest.RouteLength);
            Assert.That(memory.Choose(source),Is.EqualTo(DeliveryMood.Steady));
            Assert.That(memory.IsValid(),Is.True);
        }
        [Test] public void FailedAndAbandonedAttemptsEaseTheNextDeliveryButRetryKeepsItsPin()
        {
            var source=Fixture();var memory=new DirectorMemory();var attempt=new AttemptService(source);
            attempt.Execute(PuzzleCommand.Swap(1));memory.ObserveRetry(attempt);memory.ObserveRetry(attempt);
            Assert.That(memory.Struggle,Is.EqualTo(1));string pin=attempt.PinnedContentHash;attempt.Retry();
            Assert.That(attempt.PinnedContentHash,Is.EqualTo(pin));
            attempt.Execute(PuzzleCommand.Drop);attempt.Execute(PuzzleCommand.Drop);memory.ObserveTerminal(attempt,null);
            Assert.That(memory.Choose(source),Is.EqualTo(DeliveryMood.Recovery));
            Assert.That(memory.Choose(catalog.GetPrimary("C13").Definition),Is.EqualTo(DeliveryMood.Recovery));
            Assert.That(memory.Choose(catalog.GetPrimary("C04").Definition),Is.EqualTo(DeliveryMood.Steady));
        }
        [Test] public void AllLateRecipesPreserveIdentitiesDevicesMultisetsAndBoundWood()
        {
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Where(c=>DeliveryDirector.CanDirect(c.Definition)))
            foreach(var mood in new[]{DeliveryMood.Tight,DeliveryMood.Recovery,DeliveryMood.Wind}) {
                var source=content.Definition;var recipe=DeliveryDirector.Plan(source,mood);if(recipe==null)continue;
                var level=recipe.Apply(source);LevelValidator.ThrowIfInvalid(level);
                Assert.That(level.PuzzleId,Is.EqualTo(source.PuzzleId));Assert.That(level.RulesVersion,Is.EqualTo(source.RulesVersion));
                Assert.That(level.InitialSymbolIds,Is.EqualTo(source.InitialSymbolIds));Assert.That(level.TargetSymbolIds,Is.EqualTo(source.TargetSymbolIds));
                for(int i=0;i<level.Floors.Count;i++) {
                    Assert.That(Math.Abs(level.Floors[i].BaseDurability-source.Floors[i].BaseDurability),Is.LessThanOrEqualTo(1));
                    Assert.That(level.Floors[i].Bomb,Is.SameAs(source.Floors[i].Bomb));Assert.That(level.Floors[i].Powerup,Is.SameAs(source.Floors[i].Powerup));
                    Assert.That(level.Floors[i].EntryTransform,i==recipe.WindFloor?Is.Not.EqualTo(EntryTransform.None):Is.EqualTo(source.Floors[i].EntryTransform));
                }
                Assert.That(recipe.Apply(source).ContentHash,Is.EqualTo(level.ContentHash));
            }
        }
        [Test] public void WindUsesTheExactResolverAndKeepsContactsOnTheFinalRow()
        {
            int solved=0;string directory=Path.Combine(ReferenceFixtures.PackageRoot,"Reports","Director");Directory.CreateDirectory(directory);
            var lines=new System.Collections.Generic.List<string>();
            foreach(var source in catalog.PrimaryLevels.Where(c=>new[]{"C81","C82","C83","C84","C85","C86","C87","C88"}.Contains(c.Definition.CampaignNodeId)).Select(c=>c.Definition)) {
                var recipe=DeliveryDirector.Plan(source,DeliveryMood.Wind);if(recipe==null)continue;
                var attempt=new AttemptService(recipe.Apply(source),direction:recipe);var proof=PuzzleSolver.FindShortest(attempt.Resolver);
                lines.Add(source.CampaignNodeId+" "+proof.Status+" "+proof.OptimalActions);
                if(proof.Status!=SearchStatus.Solved)continue;solved++;
                bool gust=false;foreach(var command in proof.Path) {
                    var prediction=attempt.Predict(command);var result=attempt.Execute(command);Assert.That(result.State,Is.EqualTo(prediction.State));
                    Assert.That(LevelValidator.SameMultiset(result.State.Row,source.TargetSymbolIds),Is.True);
                    foreach(var e in result.Events.Where(e=>e.Kind==PuzzleEventKind.EntryPermuted)) {
                        if(e.Floor==recipe.WindFloor)gust=true;
                        int index=result.Events.ToList().IndexOf(e);
                        Assert.That(result.Events.Take(index).Any(p=>p.Kind==PuzzleEventKind.FloorEntered&&p.Floor==e.Floor),Is.True);
                    }
                }
                Assert.That(gust,Is.True);Assert.That(attempt.Goal.IsSatisfied(attempt.Current),Is.True);
            }
            File.WriteAllLines(Path.Combine(directory,"wind-proofs.txt"),lines);Assert.That(solved,Is.GreaterThan(0));
            var row=Fixture().InitialSymbolIds;var open=new[]{true,true};
            var level=new LevelDefinition("wind-contact","C82","1","2.0",new string('b',64),row,row,
                new[]{new TargetSegment(SegmentKind.Shapes,0,3)},new[]{new FloorDefinition(1,open),
                new FloorDefinition(2,open,entryTransform:EntryTransform.ShiftRightOne,bomb:new BombDefinition(0,row[2],2,2),repair:new RepairDefinition(0,row[2],1))});
            var transition=new PuzzleResolver(level).Resolve(new PuzzleResolver(level).CreateInitialState(),PuzzleCommand.Drop);
            Assert.That(transition.State.BombPhase,Is.EqualTo(BombPhase.Defused));Assert.That(transition.State.RepairUsed,Is.True);
            Assert.That(transition.State.Durability,Is.EqualTo(3));
        }
        [Test] public void DirectedSaveReconstructsExactCommandsAndRejectsTamperedRecipes()
        {
            var source=catalog.GetPrimary("C81").Definition;var recipe=DeliveryDirector.Plan(source,DeliveryMood.Recovery);
            var attempt=new AttemptService(recipe.Apply(source),direction:recipe);attempt.Execute(PuzzleCommand.Swap(0));
            var profile=new PlayerProfile {Attempt=AttemptRecord.Capture(attempt)};
            var restored=profile.RestoreAttempt(catalog);Assert.That(restored.CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
            restored.Retry();Assert.That(restored.PinnedContentHash,Is.EqualTo(attempt.PinnedContentHash));
            Assert.That(DeliveryDirector.MatchesSource(restored,source),Is.True);
            profile.Attempt.Direction.Durability[0]+=4;Assert.Throws<ArgumentException>(()=>profile.RestoreAttempt(catalog));
        }
        [Test] public void LegacyProfilesGetCalmDefaultsWithoutChangingTheirAttempt()
        {
            string folder=Path.Combine(Path.GetTempPath(),"CascadeDirector-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
            try {
                var attempt=new AttemptService(catalog.GetPrimary("C13").Definition);var profile=new PlayerProfile{Attempt=AttemptRecord.Capture(attempt)};
                var envelope=JObject.Parse(ProfileStore.Encode(profile));var payload=JObject.Parse((string)envelope["payload"]);
                payload.Remove("Director");((JObject)payload["Attempt"]).Remove("Direction");
                string body=payload.ToString(Newtonsoft.Json.Formatting.None);envelope["payload"]=body;envelope["sha256"]=ContentImporter.Hash(body);
                string file=Path.Combine(folder,"profile.json");File.WriteAllText(file,envelope.ToString());var loaded=new ProfileStore(file).Load();
                Assert.That(loaded.Recovery,Is.EqualTo(SaveRecovery.None));Assert.That(loaded.Profile.Director.CleanWins,Is.Zero);
                Assert.That(loaded.Profile.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
            }finally{Directory.Delete(folder,true);}
        }
        [Test] public void IrregularWindIsSmoothBoundedAndNotAShortLoop()
        {
            for(float t=0;t<200;t+=.03f){float a=WorkshopWind.Sample(t),b=WorkshopWind.Sample(t+.016f);Assert.That(Math.Abs(a),Is.LessThanOrEqualTo(1));Assert.That(Math.Abs(a-b),Is.LessThan(.025f));}
            Assert.That(Math.Abs(WorkshopWind.Sample(4)-WorkshopWind.Sample(18)),Is.GreaterThan(.01f));
        }
    }
}
