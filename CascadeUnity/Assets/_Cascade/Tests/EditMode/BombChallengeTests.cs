using System;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class BombChallengeTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp] public void Load(){
            string Read(string f)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",f));
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
        }
        public static LevelDefinition Example(int hp,int ticks,bool lower=false){
            var row="BADC".Select(c=>SymbolId.Parse("s:"+c)).ToArray();
            var target="ABCD".Select(c=>SymbolId.Parse("s:"+c)).ToArray();
            var first=new FloorDefinition(hp,new[]{true,true,true},bomb:new BombDefinition(0,target[0],ticks,1));
            return new LevelDefinition("bomb-window","C99","1","2.0","bomb-window",row,target,
                new[]{new TargetSegment(SegmentKind.Shapes,0,4)},lower?new[]{first,new FloorDefinition(1,new[]{true,true,true})}:new[]{first});
        }
        [Test] public void EqualFuseAndWoodAddsNoDeadlineToTheTwoExchangeSolution(){
            var level=Example(2,2);var resolver=new PuzzleResolver(level);
            foreach(var order in new[]{new[]{0,2},new[]{2,0}}){
                var state=resolver.CreateInitialState();
                foreach(int edge in order)state=resolver.Resolve(state,PuzzleCommand.Swap(edge)).State;
                Assert.That(state.Status,Is.EqualTo(PuzzleStatus.Won));
                Assert.That(state.ExplodedCount,Is.Zero);
            }
            Assert.That(CampaignTriads.HasLiveBombWindows(level),Is.False);
        }
        [Test] public void MissingAnEarlierDeadlineLosesWhileCorrectPriorityStillWins(){
            var level=Example(2,1);var resolver=new PuzzleResolver(level);
            var wrong=resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Swap(2));
            Assert.That(wrong.State.Status,Is.EqualTo(PuzzleStatus.Lost));
            Assert.That(wrong.Events.Any(e=>e.Kind==PuzzleEventKind.BombExploded),Is.True);
            Assert.That(PuzzleSolver.FindShortest(resolver).Status,Is.EqualTo(SearchStatus.Solved));
            Assert.That(CampaignTriads.BombsRequired(level),Is.True);
        }
        [Test] public void LaterPlatformsCanExposeAWinningExplosionBypass(){
            var level=Example(2,1,true);var bypass=CampaignTriads.WithMissedBomb(level);
            Assert.That(bypass.Status,Is.EqualTo(SearchStatus.Solved));
            var resolver=new PuzzleResolver(level);var state=resolver.CreateInitialState();
            foreach(var command in bypass.Path){var result=resolver.Resolve(state,command);Assert.That(result.Accepted,Is.True);state=result.State;}
            Assert.That(state.Status,Is.EqualTo(PuzzleStatus.Won));Assert.That(state.ExplodedCount,Is.GreaterThan(0));
            Assert.That(CampaignTriads.BombsRequired(level),Is.False);
        }
        [Test] public void AbandonmentAndUnknownCannotPassTheBombChallengeGuard(){
            var level=Example(4,3,true);
            var abandoned=PuzzleSolver.FindShortestOutcome(new PuzzleResolver(level),s=>s.Status==PuzzleStatus.Won&&s.AbandonedCount>0);
            Assert.That(abandoned.Status,Is.EqualTo(SearchStatus.Solved));Assert.That(CampaignTriads.BombsRequired(level),Is.False);
            Assert.That(CampaignTriads.WithMissedBomb(Example(2,1),budget:1).Status,Is.EqualTo(SearchStatus.Unknown));
            Assert.That(CampaignTriads.BombsRequired(Example(2,1),budget:1),Is.False);
            using var cancelled=new CancellationTokenSource();cancelled.Cancel();
            Assert.That(CampaignTriads.WithMissedBomb(Example(2,1),cancelled.Token).Status,Is.EqualTo(SearchStatus.Unknown));
        }
        [Test] public void PinnedEqualFuseRecipeRetainsItsReplayAndAttemptAfterSave(){
            var source=catalog.GetPrimary("C03").Definition;var run=DescentRun.Build(source,version:6);
            run.Stages[0].Triad.Boards[0].BombTicks=run.Stages[0].Triad.Boards[0].Hp;
            run.Validate(source,"en");string recipeHash=run.ContentHash;
            var attempt=new AttemptService(run.Level(source,0));
            int edge=int.Parse(run.Stages[0].Witness[0]);attempt.Execute(PuzzleCommand.Swap(edge));
            string folder=Path.Combine(Path.GetTempPath(),"CascadePinnedBomb",Guid.NewGuid().ToString("N"));
            try{
                Directory.CreateDirectory(folder);var store=new ProfileStore(Path.Combine(folder,"profile.json"));
                store.Save(new PlayerProfile{Descent=run,Attempt=AttemptRecord.Capture(attempt)});
                var loaded=store.Load().Profile;var restored=loaded.RestoreAttempt(catalog);
                Assert.That(loaded.Descent.ContentHash,Is.EqualTo(recipeHash));
                Assert.That(loaded.Descent.Stages[0].Triad.Boards[0].BombTicks,Is.EqualTo(run.Stages[0].Triad.Boards[0].Hp));
                Assert.That(restored.CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
            }finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
        }
        [TestCase("C03")][TestCase("C07")][TestCase("C37")][TestCase("C65")]
        [TestCase("C120")][TestCase("C360")][TestCase("C500")]
        public void CampaignAndMissionBombsHaveMeaningfulWindowsAndNoIgnoredWin(string node){
            var source=catalog.GetPrimary(node).Definition;
            using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var campaign=DescentRun.Build(source,version:6,cancellation:stop.Token);
            var mission=CampaignTriads.BuildPersonalMission(source,1,null,stop.Token,reference:campaign);
            Assert.That(mission,Is.Not.Null,node);
            foreach(var run in new[]{campaign,mission})for(int stage=0;stage<3;stage++){
                var level=run.Level(source,stage);
                Assert.That(CampaignTriads.HasLiveBombWindows(level),Is.True,node+" "+stage);
                Assert.That(CampaignTriads.BombsRequired(level,stop.Token),Is.True,node+" "+stage);
                Assert.That(CampaignTriads.BenefitsRequired(level,stop.Token),Is.True,node+" "+stage);
            }
        }
    }
}
