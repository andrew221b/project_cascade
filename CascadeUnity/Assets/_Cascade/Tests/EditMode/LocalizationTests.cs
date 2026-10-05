using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Presentation;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor.Localization;

namespace Cascade.Tests
{
    public sealed class LocalizationTests
    {
        [Test] public void AllThreeUiTablesAreCompleteAndKeepFormatArguments()
        {
            var collection=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table);
            Assert.That(collection,Is.Not.Null);Assert.That(collection.SharedData.Entries.Count,Is.GreaterThan(200));
            Assert.That(collection.StringTables.Select(t=>t.LocaleIdentifier.Code),Is.EquivalentTo(new[]{"en","ru","uk"}));
            int checks=0;
            foreach(var key in collection.SharedData.Entries)foreach(var table in collection.StringTables) {
                var entry=table.GetEntry(key.Id);Assert.That(entry,Is.Not.Null,key.Key);Assert.That(entry.Value,Is.Not.Empty,key.Key);
                var source=Regex.Matches(key.Key,@"\{\d+\}").Cast<Match>().Select(m=>m.Value).OrderBy(v=>v).ToArray();
                var target=Regex.Matches(entry.Value,@"\{\d+\}").Cast<Match>().Select(m=>m.Value).OrderBy(v=>v).ToArray();
                Assert.That(target,Is.EqualTo(source),table.LocaleIdentifier.Code+": "+key.Key);checks++;
            }
            var folder=Path.Combine(ReferenceFixtures.PackageRoot,"Reports",System.Environment.GetEnvironmentVariable("CASCADE_VALIDATION_PHASE")??"P15");Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"table-completeness.txt"),checks+" non-empty entries; matching format arguments; en/ru/uk\n");
        }
        [Test] public void BothFontsCoverEveryUiAndPuzzleCharacterWithoutFallback()
        {
            var catalog=ContentTests.LoadCatalog();var collection=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table);
            string all=string.Concat(collection.StringTables.SelectMany(t=>t.Values).Select(e=>e.Value))+string.Concat(catalog.Words.Values.Select(w=>w.DisplayText));
            var characters=all.Where(c=>!char.IsControl(c)).Distinct().ToArray();
            foreach(var font in new[]{ToyArt.Load().bodyFont,ToyArt.Load().titleFont}) {
                var missing=characters.Where(c=>!font.HasCharacter(c,false,false)).ToArray();
                Assert.That(missing,Is.Empty,font.name+": "+new string(missing));
            }
        }
        [Test] public void SupportedNfcGraphemesAndDistinctCyrillicLettersStayExact()
        {
            Assert.That(GraphemeText.Split("Е\u0308ІЇҐЄ"),Is.EqualTo(new[]{"Ё","І","Ї","Ґ","Є"}));
            foreach(var pair in new[]{new[]{"ru","Е","Ё"},new[]{"uk","И","І"},new[]{"uk","І","Ї"},new[]{"uk","Г","Ґ"},new[]{"uk","Е","Є"}})
                Assert.That(SymbolId.Parse("glyph:"+pair[0]+":"+pair[1]),Is.Not.EqualTo(SymbolId.Parse("glyph:"+pair[0]+":"+pair[2])));
            Assert.That(SymbolId.Parse("glyph:ru:Е\u0308"),Is.EqualTo(SymbolId.Parse("glyph:ru:Ё")));
            Assert.That(SymbolId.Parse("glyph:en:O"),Is.Not.EqualTo(SymbolId.Parse("shape:O")));
            Assert.That(SymbolId.Parse("glyph:en:A"),Is.Not.EqualTo(SymbolId.Parse("glyph:en:a")));
            Assert.That(GraphemeText.Split("😀").Count,Is.EqualTo(1));
        }
        [Test] public void EveryWordIsUppercaseNfcAndLowercasePackIsRejected()
        {
            var catalog=ContentTests.LoadCatalog();foreach(var word in catalog.Words.Values) {
                Assert.That(word.DisplayText,Is.EqualTo(word.DisplayText.ToUpperInvariant()));Assert.That(word.DisplayText.IsNormalized(NormalizationForm.FormC),Is.True);
            }
            string Read(string file)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",file+".json"));
            var words=JArray.Parse(Read("words_en"));words[0]["text"]=((string)words[0]["text"]).ToLowerInvariant();
            words[0]["tokens"]=new JArray(GraphemeText.Split((string)words[0]["text"]));
            Assert.Throws<InvalidDataException>(()=>ContentImporter.Import(Read("levels"),Read("localized_levels"),words.ToString(),Read("words_ru"),Read("words_uk")));
        }
    }
}
