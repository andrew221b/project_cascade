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
    }
}
