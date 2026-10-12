using System;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class MechanicLessonTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp] public void Setup()
        {
            string Read(string name)=>System.IO.File.ReadAllText(System.IO.Path.Combine(ReferenceFixtures.PackageRoot,"Data",name+".json"));
            catalog=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
        }
        [Test] public void VisibleCombinationFindsEachEffectOnceWithoutChangingTheLevel()
        {
            var all=catalog.PrimaryLevels.Concat(catalog.BonusLevels).SelectMany(c=>MechanicLessons.Discover(c.Definition,0,Array.Empty<int>())).Distinct().ToArray();
            foreach(var lesson in new[]{MechanicLesson.Bomb,MechanicLesson.Key,MechanicLesson.Repair,MechanicLesson.Ribbon,MechanicLesson.Stamp,MechanicLesson.BombHourglass,MechanicLesson.Bridge,MechanicLesson.LinkedPairs,MechanicLesson.Fragile,MechanicLesson.ClosedPath,MechanicLesson.Rollers,MechanicLesson.Mirror})Assert.That(all,Does.Contain(lesson));
            var level=catalog.GetPrimary("C03").Definition;var initial=new PuzzleResolver(level).CreateInitialState();string hash=StateHash.Compute(level,initial);
            var discovered=MechanicLessons.Discover(level,0,Array.Empty<int>());
            Assert.That(discovered.Count,Is.EqualTo(discovered.Distinct().Count()));Assert.That(StateHash.Compute(level,initial),Is.EqualTo(hash));
        }
        [Test] public void WindOnLandingIsDifferentFromRollersAndPassingFloorsRetiresTheirLessons()
        {
            var level=LevelDefinitionTests.Level(LevelDefinitionTests.Row("ABCD"),LevelDefinitionTests.Row("DABC"),new[]{
                new FloorDefinition(2,new[]{true,true,true},bomb:new BombDefinition(0,LevelDefinitionTests.S("A"),2,1)),
                new FloorDefinition(2,new[]{true,true,true},entryTransform:EntryTransform.ShiftRightOne)});
            Assert.That(MechanicLessons.Discover(level,0,new[]{1}),Does.Contain(MechanicLesson.LandingWind));
            Assert.That(MechanicLessons.Discover(level,0,new[]{1}),Has.No.Member(MechanicLesson.Rollers));
            Assert.That(MechanicLessons.Discover(level,1,Array.Empty<int>()),Is.Empty);
        }
        [Test] public void InitialMirrorHasAlreadyRunAndNeverPromisesAReceiverEffect()
        {
            var level=LevelDefinitionTests.Level(LevelDefinitionTests.Row("ABCD"),LevelDefinitionTests.Row("DCBA"),
                new[]{new FloorDefinition(3,new[]{true,true,true},entryTransform:EntryTransform.MirrorNext)});
            var resolver=new PuzzleResolver(level);var state=resolver.CreateInitialState();
            Assert.That(state.Row,Is.EqualTo(level.TargetSymbolIds));
            Assert.That(MechanicLessons.Discover(level,0,Array.Empty<int>()),Is.Empty);
            var delivered=resolver.Resolve(state,PuzzleCommand.Drop);
            Assert.That(delivered.State.Status,Is.EqualTo(PuzzleStatus.Won));
            Assert.That(delivered.Events.Any(e=>e.Kind==PuzzleEventKind.EntryPermuted),Is.False);
        }
        [Test] public void FutureMirrorReallyReversesOnTheNextPlatform()
        {
            var level=LevelDefinitionTests.Level(LevelDefinitionTests.Row("ABCD"),LevelDefinitionTests.Row("DCBA"),
                new[]{new FloorDefinition(3,new[]{true,true,true}),new FloorDefinition(3,new[]{true,true,true},entryTransform:EntryTransform.MirrorNext)});
            Assert.That(MechanicLessons.Discover(level,0,Array.Empty<int>()),Does.Contain(MechanicLesson.Mirror));
            var resolver=new PuzzleResolver(level);var landed=resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Drop);
            Assert.That(landed.Events.Any(e=>e.Kind==PuzzleEventKind.EntryPermuted&&e.Floor==1),Is.True);
            Assert.That(landed.State.Row,Is.EqualTo(level.TargetSymbolIds));
            Assert.That(MechanicLessons.Discover(level,1,Array.Empty<int>()),Is.Empty);
        }
        [Test] public void LegacyProfileStartsUnseenAndAcknowledgementSurvivesSerialization()
        {
            var old=JsonConvert.DeserializeObject<PlayerProfile>("{}");Assert.That(old.SeenMechanics,Is.Empty);old.Validate();
            old.SeenMechanics.Add(MechanicLesson.Bomb);old.SeenMechanics.Add(MechanicLesson.Gust);
            var restored=JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(old));restored.Validate();
            Assert.That(restored.SeenMechanics,Is.EqualTo(old.SeenMechanics));
        }
        [Test] public void InvalidOrRepeatedAcknowledgementsAreRejected()
        {
            var profile=new PlayerProfile();profile.SeenMechanics=null;Assert.Throws<System.IO.InvalidDataException>(()=>profile.Validate());
            profile.SeenMechanics=new System.Collections.Generic.List<MechanicLesson>{(MechanicLesson)999};Assert.Throws<System.IO.InvalidDataException>(()=>profile.Validate());
            profile.SeenMechanics=new System.Collections.Generic.List<MechanicLesson>{MechanicLesson.Gust,MechanicLesson.Gust};Assert.Throws<System.IO.InvalidDataException>(()=>profile.Validate());
        }
        [Test] public void NewLessonIdsAppendWithoutRenumberingExistingSaveValues()
        {
            Assert.That((int)MechanicLesson.Bomb,Is.Zero);Assert.That((int)MechanicLesson.LiveGust,Is.EqualTo(19));
            Assert.That(new[]{MechanicLesson.Swap,MechanicLesson.Descent,MechanicLesson.Undo,MechanicLesson.Hint,MechanicLesson.FarSwap,MechanicLesson.Toolbag}.Select(x=>(int)x),Is.EqualTo(Enumerable.Range(20,6)));
            var profile=new PlayerProfile();profile.SeenMechanics.Add(MechanicLesson.FarSwap);
            var restored=JsonConvert.DeserializeObject<PlayerProfile>(JsonConvert.SerializeObject(profile));restored.Validate();
            Assert.That(restored.SeenMechanics,Does.Contain(MechanicLesson.FarSwap));
        }
        [Test] public void VisibleLessonsExplainTheNextEntryBeforeAnyMoveAndNeverRepeatAnAppliedEntry()
        {
            var level=LevelDefinitionTests.Level(floors:new[]{
                new FloorDefinition(4,new[]{true,true}),
                new FloorDefinition(4,new[]{true,true},entryTransform:EntryTransform.MirrorNext,bomb:new BombDefinition(0,LevelDefinitionTests.S("B"),2,1))});
            var resolver=new PuzzleResolver(level);var initial=resolver.CreateInitialState();string hash=StateHash.Compute(level,initial);
            Assert.That(MechanicLessons.DiscoverVisible(level,initial),Is.EqualTo(new[]{MechanicLesson.Mirror}));
            Assert.That(StateHash.Compute(level,initial),Is.EqualTo(hash));
            var landing=resolver.Resolve(initial,PuzzleCommand.Drop);
            Assert.That(MechanicLessons.DiscoverVisible(level,landing.State),Is.EqualTo(new[]{MechanicLesson.Bomb}));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Mirror,0,landing,PuzzleCommand.Drop),Is.True);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Mirror,1,landing,PuzzleCommand.Drop),Is.False);
        }
        [TestCase(EntryTransform.ShiftLeftOne,false,MechanicLesson.Rollers)]
        [TestCase(EntryTransform.ShiftRightOne,true,MechanicLesson.LandingWind)]
        [TestCase(EntryTransform.MirrorNext,false,MechanicLesson.Mirror)]
        public void UpcomingEntryIsDiscoveredAtMountWithoutTeachingHiddenBoards(EntryTransform entry,bool wind,MechanicLesson expected)
        {
            var level=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(4,new[]{true,true}),
                new FloorDefinition(4,new[]{true,true},entryTransform:entry),
                new FloorDefinition(4,new[]{true,true},fragileEdges:new[]{0})});
            var resolver=new PuzzleResolver(level);var state=resolver.CreateInitialState();
            string hash=StateHash.Compute(level,state);
            Assert.That(MechanicLessons.DiscoverVisible(level,state,wind),Is.EqualTo(new[]{expected}));
            Assert.That(StateHash.Compute(level,state),Is.EqualTo(hash));
            Assert.That(LessonPractice.CompletedBy(expected,0,resolver.Resolve(state,PuzzleCommand.Drop),PuzzleCommand.Drop),Is.True);
        }
        [Test] public void ContactsResolvedOnArrivalDoNotAskThePlayerToRepeatThem()
        {
            var a=LevelDefinitionTests.S("A");
            var level=LevelDefinitionTests.Level(LevelDefinitionTests.Row("ABC"),LevelDefinitionTests.Row("ABC"),new[]{
                new FloorDefinition(4,new[]{false,true},bomb:new BombDefinition(0,a,2,1),key:new KeyDefinition(0,a,0),repair:new RepairDefinition(0,a,1),
                    optionalStamp:new ContactDefinition(0,a),powerup:new PowerupDefinition(0,a,PowerupKind.Ribbon))});
            var state=new PuzzleResolver(level).CreateInitialState();
            Assert.That(state.KeyUsed&&state.RepairUsed&&state.PowerupUsed,Is.True);
            Assert.That(MechanicLessons.DiscoverVisible(level,state),Is.EqualTo(new[]{MechanicLesson.FarSwap}));
        }
        [Test] public void UnusedContactsAreCompletedOnlyByTheirActualAcceptedEvents()
        {
            var a=LevelDefinitionTests.S("A");
            var level=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(8,new[]{true,false},
                bomb:new BombDefinition(0,a,3,1),key:new KeyDefinition(0,a,1),repair:new RepairDefinition(0,a,1),
                optionalStamp:new ContactDefinition(0,a),powerup:new PowerupDefinition(0,a,PowerupKind.Ribbon))});
            var resolver=new PuzzleResolver(level);var initial=resolver.CreateInitialState();
            var lessons=new[]{MechanicLesson.Bomb,MechanicLesson.Key,MechanicLesson.Repair,MechanicLesson.Ribbon,MechanicLesson.Stamp};
            Assert.That(MechanicLessons.DiscoverVisible(level,initial),Is.EqualTo(lessons));
            var rejected=resolver.Resolve(initial,PuzzleCommand.Swap(1));
            foreach(var lesson in lessons)Assert.That(LessonPractice.CompletedBy(lesson,0,rejected,PuzzleCommand.Swap(1)),Is.False,lesson.ToString());
            var accepted=resolver.Resolve(initial,PuzzleCommand.Swap(0));
            foreach(var lesson in lessons)Assert.That(LessonPractice.CompletedBy(lesson,0,accepted,PuzzleCommand.Swap(0)),Is.True,lesson.ToString());
            Assert.That(MechanicLessons.DiscoverVisible(level,accepted.State),Is.EqualTo(new[]{MechanicLesson.FarSwap}));
        }
        [Test] public void ActualGustIsTaughtUntilItMovesTheRowAndDamagesTheWood()
        {
            var level=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(6,new[]{true,true},gustAfterSwaps:1,gustTransform:EntryTransform.ShiftRightOne,gustDamage:1),new FloorDefinition(4,new[]{true,true})});
            var resolver=new PuzzleResolver(level);var initial=resolver.CreateInitialState();
            Assert.That(MechanicLessons.DiscoverVisible(level,initial),Does.Contain(MechanicLesson.LiveGust));
            var gust=resolver.Resolve(initial,PuzzleCommand.Swap(0));
            Assert.That(gust.Events.Any(e=>e.Kind==PuzzleEventKind.WindPermuted),Is.True);
            Assert.That(gust.State.Row,Is.EqualTo(LevelDefinitionTests.Row("CAB")));Assert.That(gust.State.Durability,Is.EqualTo(4));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.LiveGust,0,gust,PuzzleCommand.Swap(0)),Is.True);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Gust,0,gust,PuzzleCommand.Swap(0)),Is.False,"ambient rocking is a separate watch-only lesson");
            Assert.That(MechanicLessons.DiscoverVisible(level,gust.State),Has.No.Member(MechanicLesson.LiveGust));
        }
        [Test] public void LandingContactsCompleteOnArrivalWithoutACommandOnTheNewBoard()
        {
            var a=LevelDefinitionTests.S("A");
            var level=LevelDefinitionTests.Level(LevelDefinitionTests.Row("ABC"),LevelDefinitionTests.Row("ABC"),new[]{
                new FloorDefinition(4,new[]{true,true}),
                new FloorDefinition(4,new[]{false,true},bomb:new BombDefinition(0,a,2,1),key:new KeyDefinition(0,a,0),
                    repair:new RepairDefinition(0,a,1),optionalStamp:new ContactDefinition(0,a),powerup:new PowerupDefinition(0,a,PowerupKind.Ribbon))});
            var resolver=new PuzzleResolver(level);var initial=resolver.CreateInitialState();
            var landing=resolver.Resolve(initial,PuzzleCommand.Drop);string hash=StateHash.Compute(level,landing.State);
            Assert.That(landing.Events.Any(e=>e.Kind==PuzzleEventKind.CommandAccepted&&e.Floor==1),Is.False);
            foreach(var lesson in new[]{MechanicLesson.Bomb,MechanicLesson.Key,MechanicLesson.Repair,MechanicLesson.Ribbon,MechanicLesson.Stamp})
                Assert.That(LessonPractice.CompletedBy(lesson,1,landing,PuzzleCommand.Drop),Is.True,lesson.ToString());
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Descent,1,landing,PuzzleCommand.Drop),Is.False,"the player descended from board zero, not board one");
            Assert.That(StateHash.Compute(level,landing.State),Is.EqualTo(hash));
        }
        [Test] public void AcceptedSwapDropAndFarSwapHaveDistinctPracticeCompletions()
        {
            var level=LevelDefinitionTests.Level(floors:new[]{
                new FloorDefinition(8,new[]{true,true},powerup:new PowerupDefinition(0,LevelDefinitionTests.S("B"),PowerupKind.Ribbon)),
                new FloorDefinition(8,new[]{true,true})});
            var resolver=new PuzzleResolver(level);var initial=resolver.CreateInitialState();
            var swap=resolver.Resolve(initial,PuzzleCommand.Swap(0));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Swap,0,swap,PuzzleCommand.Swap(0)),Is.True);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Descent,0,swap,PuzzleCommand.Swap(0)),Is.False);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.FarSwap,0,swap,PuzzleCommand.Swap(0)),Is.False);
            var far=resolver.Resolve(initial,PuzzleCommand.Reach(0,2));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.FarSwap,0,far,PuzzleCommand.Reach(0,2)),Is.True);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Swap,0,far,PuzzleCommand.Reach(0,2)),Is.False);
            var descent=resolver.Resolve(initial,PuzzleCommand.Drop);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Descent,0,descent,PuzzleCommand.Drop),Is.True);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Swap,0,descent,PuzzleCommand.Drop),Is.False,"leaving a board does not practice a neighbour swap");
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.FarSwap,0,descent,PuzzleCommand.Drop),Is.False,"leaving a board does not use its far-swap charge");
            var next=resolver.Resolve(descent.State,PuzzleCommand.Swap(0));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Swap,0,next,PuzzleCommand.Swap(0)),Is.False,"a later board cannot complete an earlier cue");
        }
        [Test] public void LeavingTheTeachingBoardDoesNotCountAsUsingItsMechanic()
        {
            var level=LevelDefinitionTests.Level(floors:new[]{
                new FloorDefinition(4,new[]{true,true},bomb:new BombDefinition(0,LevelDefinitionTests.S("A"),2,1),
                    repair:new RepairDefinition(2,LevelDefinitionTests.S("A"),1)),
                new FloorDefinition(4,new[]{true,true})});
            var resolver=new PuzzleResolver(level);var drop=resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Drop);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Bomb,0,drop,PuzzleCommand.Drop),Is.False,"skipping a bomb does not rehearse defusing it");
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Repair,0,drop,PuzzleCommand.Drop),Is.False,"an unused patch must be taught on a later board");
            foreach(var entry in new[]{MechanicLesson.Mirror,MechanicLesson.Rollers,MechanicLesson.LandingWind})
                Assert.That(LessonPractice.CompletedBy(entry,0,drop,PuzzleCommand.Drop),Is.False,entry.ToString());
            var terminal=resolver.Resolve(drop.State,PuzzleCommand.Drop);
            Assert.That(MechanicLessons.DiscoverVisible(level,terminal.State),Is.Empty);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Swap,0,null,PuzzleCommand.Swap(0)),Is.False);
        }
        [Test] public void DescendingCannotPretendThatAControlOrPurchasedToolWasUsed()
        {
            var level=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(4,new[]{true,true}),new FloorDefinition(4,new[]{true,true})});
            var resolver=new PuzzleResolver(level);var drop=resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Drop);
            foreach(var lesson in new[]{MechanicLesson.Undo,MechanicLesson.Hint,MechanicLesson.Toolbag,MechanicLesson.Wayfinder,
                MechanicLesson.UndoSpool,MechanicLesson.ReturnHourglass,MechanicLesson.SignalLantern,MechanicLesson.LandingLens,MechanicLesson.Gust})
                Assert.That(LessonPractice.CompletedBy(lesson,0,drop,PuzzleCommand.Drop),Is.False,lesson.ToString());
        }
        [TestCase(EntryTransform.ShiftLeftOne)] [TestCase(EntryTransform.ShiftRightOne)]
        public void EntryLessonsObserveTheNextBoardPermutation(EntryTransform transform)
        {
            var level=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(4,new[]{true,true}),new FloorDefinition(4,new[]{true,true},entryTransform:transform)});
            var resolver=new PuzzleResolver(level);var landing=resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Drop);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Rollers,0,landing,PuzzleCommand.Drop),Is.True);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.LandingWind,0,landing,PuzzleCommand.Drop),Is.True);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Mirror,0,landing,PuzzleCommand.Drop),Is.False);
        }
        [Test] public void SurfaceExamplesFollowBridgeLinkedAndFragileConsequences()
        {
            var bridge=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(8,new[]{true,true},oneUseBridgeEdge:0)});
            var resolver=new PuzzleResolver(bridge);var crossing=resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Swap(0));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Bridge,0,crossing,PuzzleCommand.Swap(0)),Is.True);
            Assert.That(MechanicLessons.DiscoverVisible(bridge,crossing.State),Has.No.Member(MechanicLesson.Bridge));
            var linked=LevelDefinitionTests.Level(LevelDefinitionTests.Row("BADC"),LevelDefinitionTests.Row("ABCD"),new[]{new FloorDefinition(8,new[]{true,true,true},linkedPair:new LinkedPair(0,2))});
            resolver=new PuzzleResolver(linked);var pair=resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Swap(0));
            Assert.That(pair.State.Row,Is.EqualTo(linked.TargetSymbolIds));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.LinkedPairs,0,pair,PuzzleCommand.Swap(0)),Is.True);
            var fragile=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(8,new[]{true,true},fragileEdges:new[]{0})});
            resolver=new PuzzleResolver(fragile);var initial=resolver.CreateInitialState();
            var normal=resolver.Resolve(initial,PuzzleCommand.Swap(1));var cracked=resolver.Resolve(initial,PuzzleCommand.Swap(0));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Fragile,0,normal,PuzzleCommand.Swap(1)),Is.False);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.Fragile,0,cracked,PuzzleCommand.Swap(0)),Is.True);
        }
        [Test] public void FrostAndBlockedPathsNeedTheirRealConsequences()
        {
            var flask=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(8,new[]{true,true},powerup:new PowerupDefinition(0,LevelDefinitionTests.S("A"),PowerupKind.Hourglass))});
            var resolver=new PuzzleResolver(flask);var freeze=resolver.Resolve(resolver.CreateInitialState(),PuzzleCommand.Swap(0));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.BombHourglass,0,freeze,PuzzleCommand.Swap(0)),Is.True);
            var branch=LevelDefinitionTests.Level(floors:new[]{new FloorDefinition(8,new[]{false,true})});
            resolver=new PuzzleResolver(branch);var initial=resolver.CreateInitialState();
            Assert.That(MechanicLessons.DiscoverVisible(branch,initial),Does.Contain(MechanicLesson.ClosedPath));
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.ClosedPath,0,resolver.Resolve(initial,PuzzleCommand.Swap(0)),PuzzleCommand.Swap(0)),Is.False);
            Assert.That(LessonPractice.CompletedBy(MechanicLesson.ClosedPath,0,resolver.Resolve(initial,PuzzleCommand.Swap(1)),PuzzleCommand.Swap(1)),Is.True);
            foreach(var control in new[]{MechanicLesson.Undo,MechanicLesson.Hint,MechanicLesson.Toolbag,MechanicLesson.Wayfinder})
                Assert.That(LessonPractice.CompletedBy(control,0,freeze,PuzzleCommand.Swap(0)),Is.False,"Application action must not complete on an unrelated swap");
        }
    }
}
