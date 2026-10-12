using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using NUnit.Framework;

namespace Cascade.Tests
{
    public sealed class CampaignLogicTests
    {
        [Test,Timeout(900000)] public void EveryAuthoredPuzzleHasAnExecutableExactDelivery()
        {
            string Read(string name)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",name));
            var catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"),Read("campaign_extension.json"));
            var rows=new List<string>{"puzzle,node,locale,row,floors,minimum,witness"};
            var routes=new Dictionary<string,SearchResult>();
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels)){
                var level=content.Definition;
                Assert.That(LevelValidator.Validate(level,catalog.Words),Is.Empty,level.PuzzleId);
                Assert.That(level.Floors[0].EntryTransform,Is.EqualTo(EntryTransform.None),"Starting platforms must not hide a completed entry effect: "+level.PuzzleId);
                var attempt=new AttemptService(level,content.Locale);
                if(attempt.Current.Row.SequenceEqual(level.TargetSymbolIds))
                    Assert.That(level.Floors.Any(f=>f.EntryTransform!=EntryTransform.None||f.GustAfterSwaps>0),Is.True,
                        "An aligned start needs an actual row-changing device later: "+level.PuzzleId);
                // Localized bindings share slot topology. Replaying the exact
                // source witness proves them without repeating the same search.
                if(!routes.TryGetValue(level.CampaignNodeId,out var proof)){
                    proof=PuzzleSolver.FindShortest(attempt.Resolver);
                    routes.Add(level.CampaignNodeId,proof);
                }
                Assert.That(proof.Status,Is.EqualTo(SearchStatus.Solved),level.PuzzleId);
                foreach(var command in proof.Path){
                    var predicted=attempt.Predict(command);var actual=attempt.Execute(command);
                    Assert.That(actual.Accepted,Is.True,level.PuzzleId);
                    Assert.That(actual.State,Is.EqualTo(predicted.State),level.PuzzleId);
                    Assert.That(LevelValidator.SameMultiset(actual.State.Row,level.TargetSymbolIds),Is.True,level.PuzzleId);
                    Assert.That(actual.Events.Count(e=>e.Kind==PuzzleEventKind.EntryPermuted),Is.LessThanOrEqualTo(1),level.PuzzleId);
                }
                Assert.That(attempt.Current.Status,Is.EqualTo(PuzzleStatus.Won),level.PuzzleId);
                Assert.That(attempt.Current.Row,Is.EqualTo(level.TargetSymbolIds),level.PuzzleId);
                rows.Add(string.Join(",",level.PuzzleId,level.CampaignNodeId,content.Locale,level.InitialSymbolIds.Count,level.Floors.Count,
                    proof.OptimalActions,"\""+string.Join(" ",proof.Path.Select(c=>c.ToString()))+"\""));
            }
            Assert.That(rows.Count-1,Is.EqualTo(2157));
            string directory=Path.Combine(ReferenceFixtures.PackageRoot,"Reports","CampaignLogic-Oct08");Directory.CreateDirectory(directory);
            File.WriteAllLines(Path.Combine(directory,"authored-proofs.csv"),rows);
        }
    }
}
