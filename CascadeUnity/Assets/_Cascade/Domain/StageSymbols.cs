using System;
using System.Linq;

namespace Cascade.Domain
{
    // Source rows remain pinned. A stage owns its actual visible/logical symbols.
    public sealed class StageSymbols
    {
        public string[] Target;
        public string Locale,WordId,Word;
        public int WordStart=-1;
        public string Fingerprint=>string.Join(",",Target??Array.Empty<string>())+"|"+Locale+"|"+WordStart+"|"+WordId+"|"+Word;
        public void Validate(string mode,string locale)
        {
            if(Target==null||Target.Length<3||Target.Length>8||Target.Any(s=>s==null||s.Length>24)||Locale!=locale)throw new ArgumentException("Invalid stage symbols.");
            var target=Target.Select(s=>SymbolId.Parse(s,locale)).ToArray();
            if(target.Any(s=>s.Kind==SymbolKind.Shape&&(s.Value.Length!=1||s.Value[0]<'A'||s.Value[0]>'F')||s.Kind==SymbolKind.Glyph&&s.Locale!=locale))throw new ArgumentException("Invalid stage alphabet.");
            if(mode=="Shapes"){
                if(target.Any(s=>s.Kind!=SymbolKind.Shape)||Word!=null||WordId!=null||WordStart!=-1)throw new ArgumentException("Shape stage contains letters.");return;
            }
            if(mode!="Words"&&mode!="Mixed"||string.IsNullOrWhiteSpace(WordId)||WordId.Length>80||Word==null||Word.Length>32)throw new ArgumentException("Invalid stage vocabulary.");
            var letters=GraphemeText.Split(Word);
            if(letters.Count<3||letters.Count>8||WordStart<0||WordStart+letters.Count>target.Length||Word!=Word.ToUpperInvariant())throw new ArgumentException("Invalid stage word.");
            for(int i=0;i<target.Length;i++){
                bool word=i>=WordStart&&i<WordStart+letters.Count;
                if(word?(target[i].Kind!=SymbolKind.Glyph||target[i].Value!=letters[i-WordStart]):target[i].Kind!=SymbolKind.Shape)throw new ArgumentException("Stage word does not match its target.");
            }
            if(mode=="Words"&&(letters.Count!=target.Length||WordStart!=0)||mode=="Mixed"&&!target.Any(s=>s.Kind==SymbolKind.Shape))throw new ArgumentException("Wrong stage mode.");
        }
        public LevelDefinition Apply(LevelDefinition source)
        {
            var target=Target.Select(s=>SymbolId.Parse(s,Locale)).ToArray();var segments=new System.Collections.Generic.List<TargetSegment>();
            int length=Word==null?0:GraphemeText.Split(Word).Count;
            for(int i=0;i<target.Length;){
                if(i==WordStart){segments.Add(new TargetSegment(SegmentKind.Word,i,length,WordId));i+=length;}
                else{int end=i+1;while(end<target.Length&&end!=WordStart)end++;segments.Add(new TargetSegment(SegmentKind.Shapes,i,end-i));i=end;}
            }
            // TriadRecipe binds its own boards; these source boards are never played.
            return new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,source.ContentHash,
                target,target,segments,source.Floors,source.ReachCharges,source.TeachingNode);
        }
    }
    public sealed class StageVariation
    {
        public const string Protocol="stage-variety-v1";
        public string Revision=Protocol,PoolHash,Seed,Mode,Locale;
        public int[] WordDraws,ShapeDraws;
        public StageSymbols[] Symbols;
        public string Fingerprint=>Revision+"|"+PoolHash+"|"+Seed+"|"+Mode+"|"+Locale+"|"+string.Join(",",WordDraws??Array.Empty<int>())+"|"+string.Join(",",ShapeDraws??Array.Empty<int>())+"|"+string.Join("/",(Symbols??Array.Empty<StageSymbols>()).Select(s=>s?.Fingerprint));
        public bool HasValidMetadata()
        {
            bool Hex(string value,int length)=>value!=null&&value.Length==length&&value.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
            if(Revision!=Protocol||!Hex(PoolHash,64)||!Hex(Seed,32)||!(Locale=="en"||Locale=="ru"||Locale=="uk")||!(Mode=="Shapes"||Mode=="Words"||Mode=="Mixed")
                ||WordDraws?.Length!=3||ShapeDraws?.Length!=3||WordDraws.Any(n=>n< -1||n>1000000000)||ShapeDraws.Any(n=>n< -1||n>1000000000))return false;
            return true;
        }
        public bool IsBounded()
        {
            if(!HasValidMetadata()||Symbols?.Length!=3||Symbols.Any(s=>s==null))return false;
            try{foreach(var symbols in Symbols)symbols.Validate(Mode,Locale);}catch(ArgumentException){return false;}catch(FormatException){return false;}
            return true;
        }
    }
}
