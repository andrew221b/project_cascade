using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class EffectBudgetTests
    {
        ContentCatalog catalog;
        [OneTimeSetUp] public void Load(){
            string Read(string f)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",f));
            catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
        }
        [TestCase("C02")][TestCase("C04")][TestCase("C06")][TestCase("C07")]
        [TestCase("C65")][TestCase("C120")][TestCase("C360")][TestCase("C500")]
        public void EveryBenefitIsNeededInCampaignAndMissionStages(string node)
        {
            var source=catalog.GetPrimary(node).Definition;
            using var stop=new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var baseline=DescentRun.Build(source,version:6,cancellation:stop.Token);
            var mission=CampaignTriads.BuildPersonalMission(source,1,null,stop.Token,reference:baseline);
            Assert.That(mission,Is.Not.Null,node);
            var failures=new List<string>();
                foreach(var run in new[]{baseline,mission})for(int stage=0;stage<3;stage++){
                    var level=run.Level(source,stage);
                    for(int floor=0;floor<level.Floors.Count;floor++){
                        var f=level.Floors[floor];
                        var benefits=new[]{f.Repair!=null?(PuzzleEventKind?)PuzzleEventKind.Repaired:null,
                            f.Key!=null?(PuzzleEventKind?)PuzzleEventKind.KeyOpened:null,
                            f.Powerup==null?null:(PuzzleEventKind?)(f.Powerup.Kind==PowerupKind.Hourglass?PuzzleEventKind.HourglassCollected:PuzzleEventKind.RibbonCollected)};
                        foreach(var benefit in benefits.Where(b=>b.HasValue)){
                            var proof=CampaignTriads.WithoutBenefit(level,floor,benefit.Value,stop.Token);
                            if(proof.Status!=SearchStatus.Unsolvable)failures.Add($"{node} {(run==baseline?"campaign":"mission")} stage {stage+1} floor {floor+1} {benefit}: {proof.Status}");
                        }
                    }
                }
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
        }
    }
}
