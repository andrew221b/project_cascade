using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class EarlyMechanicsTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp]public void Setup(){string Read(string f)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",f));catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));}
        static PuzzleCommand Decode(string c)=>c=="D"?PuzzleCommand.Drop:c.StartsWith("R",StringComparison.Ordinal)?PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):PuzzleCommand.Swap(int.Parse(c));
        [Test]public void EveryChangedEarlyDefinitionAndRepresentativeLatePlansHaveReplayableThreeStageProofs()
        {
            var failures=new List<string>();var lines=new List<string>{"puzzle,locale,first,second,final"};
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Where(c=>int.Parse(c.Definition.CampaignNodeId.Substring(1))<=40||new[]{"C41","C81","C160"}.Contains(c.Definition.CampaignNodeId))){
                var source=content.Definition;var run=DescentRun.Build(source,version:4);if(run==null){failures.Add(source.PuzzleId);continue;}
                run.Validate(source,content.Locale);lines.Add(source.PuzzleId+","+content.Locale+","+string.Join(",",run.Stages.Select(s=>s.Minimum)));
                for(int stage=0;stage<3;stage++){
                    var level=run.Level(source,stage);LevelValidator.ThrowIfInvalid(level);Assert.That(LevelValidator.SameMultiset(level.InitialSymbolIds,source.TargetSymbolIds),Is.True);
                    var a=new AttemptService(level,content.Locale,direction:run.Direction);foreach(var c in run.Stages[stage].Witness)Assert.That(a.Execute(Decode(c)).Accepted,Is.True,source.PuzzleId);
                    Assert.That(a.Current.Status,Is.EqualTo(PuzzleStatus.Won));if(stage<2)run.AcceptCheckpoint(source,a);
                }
            }
            var dir=Path.Combine(ReferenceFixtures.PackageRoot,"Reports","EarlyMechanics");Directory.CreateDirectory(dir);File.WriteAllLines(Path.Combine(dir,"curves.csv"),lines);
            Assert.That(failures,Is.Empty,string.Join(",",failures));
        }
        [Test]public void FirstIslandIntroducesRealContactsAndThenHourglassBridgeAndLinkedSwaps()
        {
            foreach(var pair in new[]{("C02",0,PuzzleEventKind.Repaired),("C03",1,PuzzleEventKind.BombDefused),("C04",0,PuzzleEventKind.KeyOpened),("C05",1,PuzzleEventKind.RibbonCollected),("C06",0,PuzzleEventKind.StampCollected),("C07",1,PuzzleEventKind.HourglassCollected),("C08",0,PuzzleEventKind.BridgeConsumed)}){
                var source=catalog.GetPrimary(pair.Item1).Definition;var run=DescentRun.Build(source,version:4);Assert.That(run,Is.Not.Null,pair.Item1);
                var a=new AttemptService(run.Level(source,pair.Item2));bool found=false;
                for(int edge=0;edge<a.Level.InitialSymbolIds.Count-1;edge++){var result=a.Resolver.Resolve(a.Current,PuzzleCommand.Swap(edge));found|=result.Accepted&&result.Events.Any(e=>e.Kind==pair.Item3);}
                Assert.That(found,Is.True,"A contact should be legible and reachable in one exchange: "+pair.Item1);
            }
            var linked=DescentRun.Build(catalog.GetPrimary("C09").Definition,version:4).Level(catalog.GetPrimary("C09").Definition,0);
            Assert.That(linked.Floors[0].LinkedPair.HasValue,Is.True);
            var wind=DescentRun.Build(catalog.GetPrimary("C02").Definition,version:4).Level(catalog.GetPrimary("C02").Definition,1);
            Assert.That(wind.Floors[1].EntryTransform,Is.Not.EqualTo(EntryTransform.None));
        }
        [Test]public void WideEarlyRowsPreferTheProvenSingleShiftWithoutExhaustingAReverseSearch()
        {
            var path=Path.Combine(ReferenceFixtures.PackageRoot,"Reports","EarlyMechanics","curves.csv");var lines=File.ReadAllLines(path).Skip(1).ToDictionary(l=>l.Split(',')[0]);
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Where(c=>int.Parse(c.Definition.CampaignNodeId.Substring(1))<=40&&c.Definition.TargetSymbolIds.Count>=7)){
                var source=content.Definition;var run=DescentRun.Build(source,version:4);Assert.That(run,Is.Not.Null,source.PuzzleId);run.Validate(source,content.Locale);
                for(int stage=0;stage<3;stage++){var a=new AttemptService(run.Level(source,stage),content.Locale);foreach(var c in run.Stages[stage].Witness)Assert.That(a.Execute(Decode(c)).Accepted,Is.True);Assert.That(a.Current.Status,Is.EqualTo(PuzzleStatus.Won));}
                Assert.That(run.Stages[2].Arrival,Is.Not.EqualTo(EntryTransform.MirrorNext));
                lines[source.PuzzleId]=source.PuzzleId+","+content.Locale+","+string.Join(",",run.Stages.Select(s=>s.Minimum));
            }
            File.WriteAllLines(path,new[]{"puzzle,locale,first,second,final"}.Concat(lines.Values));
        }
        [Test]public void IntroductoryBombAndHourglassAppearOnTheProvenUsefulRoute()
        {
            var path=Path.Combine(ReferenceFixtures.PackageRoot,"Reports","EarlyMechanics","curves.csv");var lines=File.ReadAllLines(path).Skip(1).ToDictionary(l=>l.Split(',')[0]);
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels).Where(c=>c.Definition.CampaignNodeId=="C03"||c.Definition.CampaignNodeId=="C07")){
                var source=content.Definition;var run=DescentRun.Build(source,version:4);Assert.That(run,Is.Not.Null);run.Validate(source,content.Locale);
                var a=new AttemptService(run.Level(source,1),content.Locale);var events=new List<PuzzleEventKind>();foreach(var command in run.Stages[1].Witness)events.AddRange(a.Execute(Decode(command)).Events.Select(e=>e.Kind));
                Assert.That(events,Does.Contain(PuzzleEventKind.BombDefused),source.PuzzleId);
                if(source.CampaignNodeId=="C07")Assert.That(events,Does.Contain(PuzzleEventKind.HourglassCollected),source.PuzzleId);
                lines[source.PuzzleId]=source.PuzzleId+","+content.Locale+","+string.Join(",",run.Stages.Select(s=>s.Minimum));
            }
            File.WriteAllLines(path,new[]{"puzzle,locale,first,second,final"}.Concat(lines.Values));
        }
        [Test]public void OldVersionThreeCheckpointKeepsItsHashAndV4DoesNotReuseItsCache()
        {
            var source=catalog.GetPrimary("C02").Definition;var old=DescentRun.Build(source,version:3);var a=new AttemptService(old.Level(source,0));foreach(var c in old.Stages[0].Witness)a.Execute(Decode(c));old.AcceptCheckpoint(source,a);
            a=new AttemptService(old.Level(source,1));var profile=new PlayerProfile{Descent=old,Attempt=AttemptRecord.Capture(a)};
            Assert.That(profile.RestoreAttempt(catalog).CurrentStateHash,Is.EqualTo(a.CurrentStateHash));
            Assert.That(DescentRun.Build(source,version:4).ContentHash,Is.Not.EqualTo(old.ContentHash));
        }
        [Test]public void DirectorAdaptsAfterFirstIslandAndEveryEarlyMoodRetainsAProof()
        {
            Assert.That(new DirectorMemory{CleanWins=6,Deliveries=5}.Choose(catalog.GetPrimary("C04").Definition),Is.EqualTo(DeliveryMood.Steady));
            var memory=new DirectorMemory{CleanWins=3,Deliveries=5};var source=catalog.PrimaryLevels.First(c=>DeliveryDirector.CanDirect(c.Definition)&&int.Parse(c.Definition.CampaignNodeId.Substring(1))<=40&&DeliveryDirector.WindFloor(c.Definition)>=1).Definition;
            Assert.That(memory.Choose(source),Is.EqualTo(DeliveryMood.Wind));
            memory.Struggle=2;Assert.That(memory.Choose(source),Is.EqualTo(DeliveryMood.Recovery));
            foreach(var node in new[]{"C06","C11","C20","C27"})foreach(DeliveryMood mood in Enum.GetValues(typeof(DeliveryMood))){
                var s=catalog.GetPrimary(node).Definition;var run=DescentRun.Build(s,mood,version:4);Assert.That(run,Is.Not.Null,node+" / "+mood);run.Validate(s,"en");
            }
        }
    }
}
