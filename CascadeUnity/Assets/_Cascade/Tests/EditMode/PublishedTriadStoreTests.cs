using System.IO;
using System.Linq;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Persistence;
using NUnit.Framework;
namespace Cascade.Tests
{
    public sealed class PublishedTriadStoreTests
    {
        static string Read(string name)=>File.ReadAllText(Path.Combine(ReferenceFixtures.PackageRoot,"Data",name+".json"));
        static ContentCatalog Catalog()=>ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
        static PublishedTriadStore Store()=>new PublishedTriadStore(Read("production_triad_recipes"),Read("production_triad_manifest"));
        [Test]public void PartialOrCorruptRecipePublicationIsRejected()
        {Assert.Throws<System.ArgumentException>(()=>new PublishedTriadStore(Read("production_triad_recipes")+" ",Read("production_triad_manifest")));}
        [Test]public void EveryPublishedAndLocalizedCertificateReplaysWithoutRegeneration()
        {
            var store=Store();var catalog=Catalog();int stages=0;
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels)){
                var run=store.Bind(content.Definition,content.Locale,catalog.ContentHash);
                Assert.That(run,Is.Not.Null,content.Definition.PuzzleId);Assert.That(run.StageIndex,Is.Zero);stages+=run.Stages.Length;
            }
            Assert.That(stages,Is.EqualTo(6471));
        }
        [Test]public void CachedStagesNeverShareMutableRunState()
        {
            var store=Store();var catalog=Catalog();var source=catalog.GetPrimary("C03");
            var first=store.Bind(source.Definition,source.Locale,catalog.ContentHash);string hash=first.ContentHash;
            first.Stages[0].Triad.Boards[0].Hp+=4;
            Assert.That(store.Bind(source.Definition,source.Locale,catalog.ContentHash).ContentHash,Is.EqualTo(hash));
        }
        [Test]public void StaleCatalogOrCustomDraftCannotUsePublishedCertificates()
        {
            var store=Store();var catalog=Catalog();var source=catalog.GetPrimary("C01").Definition;
            Assert.That(store.Bind(source,"en",new string('b',64)),Is.Null);
            var draft=new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,new string('a',64),source.InitialSymbolIds,source.TargetSymbolIds,source.TargetSegments,source.Floors);
            Assert.That(store.Bind(draft,"en",catalog.ContentHash),Is.Null);
        }
    }
}
