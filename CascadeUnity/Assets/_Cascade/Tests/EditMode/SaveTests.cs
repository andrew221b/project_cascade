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
    public sealed class SaveTests
    {
        ContentCatalog catalog;
        string directory;
        ProfileStore store;
        [OneTimeSetUp] public void Import(){catalog=ContentTests.LoadCatalog();}
        [SetUp] public void SetUp(){directory=Path.Combine(Path.GetTempPath(),"CascadeSaveTests",Guid.NewGuid().ToString("N"));store=new ProfileStore(Path.Combine(directory,"profile.json"));}
        [TearDown] public void Clean(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        AttemptService Attempt(string id="C02")=>new AttemptService(catalog.GetPrimary(id).Definition);
        static void Win(AttemptService a){foreach(var command in PuzzleSolver.FindShortest(a.Resolver).Path)a.Execute(command);}

        [Test] public void EveryBuiltInPrefixRestoresExactStateAndUndoHistory()
        {
            int count=0;
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels)) {
                var attempt=new AttemptService(content.Definition,content.Locale);
                Check(attempt);
                foreach(var command in PuzzleSolver.FindShortest(attempt.Resolver).Path){attempt.Execute(command);Check(attempt);count++;}
            }
            string report=Path.Combine(ReferenceFixtures.PackageRoot,"Reports",Environment.GetEnvironmentVariable("CASCADE_VALIDATION_PHASE")??"P16");Directory.CreateDirectory(report);
            File.WriteAllText(Path.Combine(report,"replay-count.txt"),count+" committed prefixes plus "+(catalog.PrimaryLevels.Count+catalog.LocalizedLevels.Count)+" initial states verified\n");
        }
        static void Check(AttemptService attempt)
        {
            var restored=AttemptService.Restore(attempt.Level,AttemptRecord.Capture(attempt));
            Assert.That(restored.CurrentStateHash,Is.EqualTo(attempt.CurrentStateHash));
            Assert.That(restored.AttemptId,Is.EqualTo(attempt.AttemptId));
            Assert.That(restored.RouteLength,Is.EqualTo(attempt.RouteLength));
            Assert.That(restored.History.Select(h=>h.Before),Is.EqualTo(attempt.History.Select(h=>h.Before)));
            if(restored.CanUndo){var expected=attempt.History.Last().Before;restored.Undo();Assert.That(restored.Current,Is.EqualTo(expected));}
        }
        [Test] public void UndoCountAndHintUsedRemainIndependentOfShortenedRoute()
        {
            var a=new AttemptService(LevelDefinitionTests.Level(LevelDefinitionTests.Row("DCBA"),LevelDefinitionTests.Row("ABCD"),new[]{new FloorDefinition(12,new[]{true,true,true})}));a.RevealHint(AttemptService.CalculateHint(a.CaptureHintRequest()));
            a.Execute(PuzzleCommand.Swap(1));a.Execute(PuzzleCommand.Swap(0));a.Undo();
            var restored=AttemptService.Restore(a.Level,AttemptRecord.Capture(a));
            Assert.That(restored.HintUsed,Is.True);Assert.That(restored.UndoCount,Is.EqualTo(1));Assert.That(restored.RouteLength,Is.EqualTo(1));
            restored.Undo();Assert.That(restored.HintUsed,Is.True);Assert.That(restored.UndoCount,Is.EqualTo(2));
            restored.Retry();Assert.That(restored.HintUsed,Is.False);Assert.That(restored.UndoCount,Is.Zero);
        }
        [Test] public void TamperedRouteAndPinsCannotBecomeAnotherPuzzle()
        {
            var a=Attempt();var record=AttemptRecord.Capture(a);
            record.ContentHash=new string('0',64);Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,record));
            record=AttemptRecord.Capture(a);record.Commands=new[]{"-1"};Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,record));
            record=AttemptRecord.Capture(a);record.StateHash=new string('0',64);Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,record));
            record=AttemptRecord.Capture(a);record.Commands=new[]{"D"};Assert.Throws<ArgumentException>(()=>AttemptService.Restore(a.Level,record));
        }
        [Test] public void FirstClearReceiptIsStableAcrossLanguagesBindingsAndUndo()
        {
            var p=new PlayerProfile();var primary=Attempt("C01");Win(primary);Assert.That(p.RecordClear(primary),Is.True);
            primary.Undo();Assert.That(p.RecordClear(primary),Is.False);Win(primary);Assert.That(p.RecordClear(primary),Is.False);
            foreach(var content in catalog.LocalizedLevels.Where(c=>c.Definition.CampaignNodeId=="C01")) {
                var a=new AttemptService(content.Definition,content.Locale);Win(a);Assert.That(p.RecordClear(a),Is.False);
            }
            p.Attempt=AttemptRecord.Capture(primary);store.Save(p);
            var restored=store.Load().Profile;Assert.That(restored.Clears.Count,Is.EqualTo(1));Assert.That(restored.RecordClear(primary),Is.False);
        }
        [Test] public void OptionalObjectiveAndPinnedWordLocaleRestore()
        {
            catalog.TryGetLocalizedBinding("C35","uk",out var content);
            var a=new AttemptService(content.Definition,"uk",new PuzzleGoal(1));
            foreach(var command in PuzzleSolver.FindShortest(a.Resolver,goal:a.Goal).Path)a.Execute(command);
            var p=new PlayerProfile{Attempt=AttemptRecord.Capture(a)};store.Save(p);
            var restored=store.Load().Profile.RestoreAttempt(catalog);
            Assert.That(restored.Goal.RequiredStampBits,Is.EqualTo(1));Assert.That(restored.Goal.IsSatisfied(restored.Current),Is.True);
            Assert.That(restored.ActiveLocale,Is.EqualTo("uk"));Assert.That(restored.Current.Row,Is.EqualTo(a.Current.Row));
        }
        [Test] public void CorruptPrimaryUsesBackupWithoutLosingTheLastGoodBackupOnWrite()
        {
            var p=new PlayerProfile();p.Preferences.UiLocale="ru";store.Save(p);
            p.Preferences.UiLocale="uk";store.Save(p);File.WriteAllText(store.Path,"{truncated");
            var loaded=store.Load();Assert.That(loaded.Recovery,Is.EqualTo(SaveRecovery.Backup));Assert.That(loaded.Profile.Preferences.UiLocale,Is.EqualTo("ru"));
            string backup=File.ReadAllText(store.BackupPath);store.Save(loaded.Profile);
            Assert.That(File.ReadAllText(store.BackupPath),Is.EqualTo(backup));Assert.That(Directory.GetFiles(directory,"*.rejected-*").Length,Is.EqualTo(1));
            Assert.That(store.Load().Recovery,Is.EqualTo(SaveRecovery.None));
        }
        [Test] public void InterruptedTemporaryWriteIsNotACommit()
        {
            var p=new PlayerProfile();p.HeardStory.Add(2);store.Save(p);File.WriteAllText(store.Path+".tmp","{half");
            Assert.That(store.Load().Profile.HeardStory,Is.EqualTo(new[]{2}));
            File.Delete(store.Path);Assert.That(store.Load().Recovery,Is.EqualTo(SaveRecovery.None));Assert.That(store.Load().Profile.HeardStory,Is.Empty);
        }
        [Test] public void UnreadableFilesAndFutureSchemaAreNotSilentlyDiscarded()
        {
            Directory.CreateDirectory(directory);File.WriteAllText(store.Path,"broken");File.WriteAllText(store.BackupPath,"also broken");
            Assert.That(store.Load().Recovery,Is.EqualTo(SaveRecovery.Unreadable));
            Assert.That(File.ReadAllText(store.Path),Is.EqualTo("broken"));
            string future="{\"schema\":2}";File.WriteAllText(store.Path,future);
            Assert.That(store.Load().Recovery,Is.EqualTo(SaveRecovery.NewerVersion));
            Assert.Throws<InvalidOperationException>(()=>store.Save(new PlayerProfile()));Assert.That(File.ReadAllText(store.Path),Is.EqualTo(future));
        }
        [Test] public void IncompatibleContentKeepsValidMetaProgressAndRequiresExplicitRestart()
        {
            var a=Attempt("C01");Win(a);var p=new PlayerProfile();p.RecordClear(a);p.Attempt=AttemptRecord.Capture(a);
            p.Attempt.ContentHash=new string('0',64);p.Preferences.UiLocale="uk";store.Save(p);
            var loaded=store.Load();Assert.That(loaded.Recovery,Is.EqualTo(SaveRecovery.None));
            Assert.That(loaded.Profile.Clears.Count,Is.EqualTo(1));Assert.That(loaded.Profile.Preferences.UiLocale,Is.EqualTo("uk"));
            Assert.Throws<InvalidDataException>(()=>loaded.Profile.RestoreAttempt(catalog));
        }
        [Test] public void ChecksumFailureAndDuplicateReceiptsAreRejected()
        {
            var a=Attempt("C01");Win(a);var p=new PlayerProfile();p.RecordClear(a);store.Save(p);
            var json=JObject.Parse(File.ReadAllText(store.Path));json["sha256"]=new string('0',64);File.WriteAllText(store.Path,json.ToString());
            Assert.That(store.Load().Recovery,Is.EqualTo(SaveRecovery.Unreadable));
            p.Clears.Add(p.Clears[0]);Assert.Throws<InvalidDataException>(()=>store.Save(p));
        }
    }
}
