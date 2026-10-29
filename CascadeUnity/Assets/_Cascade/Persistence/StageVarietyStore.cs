using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
namespace Cascade.Persistence
{
    // Client and Cloud Code use this same reconstruction. Uploaded symbols/boards
    // never replace the permitted pool or the published mechanical certificates.
    public sealed class StageVarietyStore
    {
        readonly Dictionary<string,string> alternatives;
        public StageWordPool Words { get; }
        readonly Dictionary<string,string> published;
        public DescentRun Published(LevelDefinition source)=>new DescentRun{Version=6,SourceHash=source.ContentHash,Stages=CloudJson.Decode<DescentStage[]>(published[source.CampaignNodeId])};
        public StageVarietyStore(string wordsJson,string templatesJson,string publishedJson)
        {
            Words=new StageWordPool(wordsJson);
            published=CloudJson.Decode<Dictionary<string,DescentStage[]>>(publishedJson).ToDictionary(p=>p.Key,p=>CloudJson.Encode(p.Value));
            var recipes=CloudJson.Decode<Dictionary<string,DescentStage[]>>(templatesJson);
            if(recipes==null||recipes.Count>100)throw new ArgumentException("Invalid stage alternatives.");
            alternatives=recipes.ToDictionary(p=>p.Key,p=>CloudJson.Encode(p.Value));
        }
        public StageVariation Expand(LevelDefinition source,StageVariation plan)
        {
            if(source==null||plan==null||!plan.HasValidMetadata()||plan.PoolHash!=Words.Hash)throw new ArgumentException("Unavailable stage vocabulary.");
            var expanded=CloudJson.Decode<StageVariation>(CloudJson.Encode(plan));expanded.Symbols=new StageSymbols[3];
            for(int i=0;i<3;i++){
                bool hasWord=StageContentBasis.WordLength(source,plan.Mode)>0,hasShape=StageContentBasis.ShapeCount(source,plan.Mode)>0;
                if(hasWord?(plan.WordDraws[i]<0):plan.WordDraws[i]!=-1)throw new ArgumentException("Invalid word draw.");
                if(hasShape?(plan.ShapeDraws[i]<0):plan.ShapeDraws[i]!=-1)throw new ArgumentException("Invalid shape draw.");
                expanded.Symbols[i]=StageContentBasis.Symbols(source,plan.Mode,plan.Locale,Words,plan.Seed,plan.WordDraws[i],plan.ShapeDraws[i]);
            }
            if(!expanded.IsBounded())throw new ArgumentException("Invalid stage variation.");return expanded;
        }
        public DescentRun Create(LevelDefinition source,StageVariation plan,DescentRun published)
        {
            if(source==null||published?.Version!=6||published.SourceHash!=source.ContentHash)throw new ArgumentException("Published stage source required.");
            var expected=Expand(source,plan);
            if(plan.Symbols!=null&&plan.Fingerprint!=expected.Fingerprint)throw new ArgumentException("Stage vocabulary differs from its draw.");
            DescentStage[] stages;
            if(StageContentBasis.IsRenaming(source,plan.Mode))stages=CloudJson.Decode<DescentStage[]>(CloudJson.Encode(published.Stages));
            else if(alternatives.TryGetValue(StageContentBasis.Key(source,plan.Mode),out string recipe))stages=CloudJson.Decode<DescentStage[]>(recipe);
            else throw new ArgumentException("A certified stage alternative is missing: "+StageContentBasis.Key(source,plan.Mode));
            return Personalize(source,expected,stages);
        }
        public DescentRun Personalize(LevelDefinition source,StageVariation plan,DescentStage[] stages)
        {
            var expected=Expand(source,plan);if(plan.Symbols!=null&&plan.Fingerprint!=expected.Fingerprint)throw new ArgumentException("Wrong stage vocabulary.");
            var run=new DescentRun{Version=7,SourceHash=source.ContentHash,Stages=CloudJson.Decode<DescentStage[]>(CloudJson.Encode(stages)),Variation=expected};
            run.Validate(source,expected.Locale);
            for(int i=0;i<3;i++){
                var symbol=expected.Symbols[i];var words=new Dictionary<string,WordDefinition>();if(symbol.WordId!=null){var word=Words.Find(symbol.WordId);if(word.DisplayText!=symbol.Word)throw new ArgumentException("Wrong stage word.");words.Add(word.WordId,word);}
                LevelValidator.ThrowIfInvalid(run.Level(source,i),words);
            }
            return run;
        }
        public DescentRun CanonicalReference(LevelDefinition source,string mode)
        {
            var basis=StageContentBasis.Canonical(source,mode);DescentStage[] stages;
            if(StageContentBasis.IsRenaming(source,mode))stages=Published(source).Stages;
            else if(alternatives.TryGetValue(StageContentBasis.Key(source,mode),out string encoded))stages=CloudJson.Decode<DescentStage[]>(encoded);
            else throw new ArgumentException("A canonical stage certificate is missing.");
            var run=new DescentRun{Version=6,SourceHash=basis.ContentHash,Stages=stages};run.Validate(basis,"en");return run;
        }
        public void ValidateSaved(DescentRun saved,LevelDefinition source,DescentRun published)
        {
            if(saved?.Version!=7)throw new ArgumentException("Wrong stage variation version.");
            var expected=Create(source,saved.Variation,published);
            if(expected.ContentHash!=saved.ContentHash)throw new ArgumentException("Saved stage certificate differs.");
            saved.Validate(source,saved.Variation.Locale);
        }
    }
}
