using System.Linq;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Pure presentation of the presenter's number. Lifetime follows the source label.
    public sealed class MenuSpriteCounter : MonoBehaviour
    {
        TMP_Text source;VictoryScoreDigits digits;string previous;Vector2 previousArea;
        public void Initialize(TMP_Text label,Sprite[] numbers,Sprite[] symbols,TMP_FontAsset font)
        {
            source=label;
            var child=new GameObject("Menu illustrated counter",typeof(RectTransform));child.transform.SetParent(transform,false);
            var rect=(RectTransform)child.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            digits=child.AddComponent<VictoryScoreDigits>();
            if(!digits.Initialize(numbers,font,source.fontSize,symbols)){Destroy(child);Destroy(this);return;}
            if(source.name=="Navigation page count")digits.SetDigitScale('7',new Vector2(.90f,.94f));
            if(source.name=="Wallet coins")digits.SetDigitScale('8',source.text.Trim()=="× 8"?new Vector2(1.10f,1.08f):Vector2.one);
            foreach(var outline in child.GetComponentsInChildren<Outline>(true))outline.effectDistance=new Vector2(.65f,-.65f);
            Refresh();
        }
        void LateUpdate(){Refresh();}
        internal void Refresh()
        {
            if(source==null||digits==null)return;
            // Localized labels can arrive after the initial numeric copy. If a
            // price caption becomes ordinary text (for example, Buy), restore
            // the TMP label and remove its stale illustrated-number overlay.
            if(string.IsNullOrEmpty(source.text)||!source.text.Any(char.IsDigit)){
                source.canvasRenderer.SetAlpha(1);
                digits.gameObject.SetActive(false);
                Destroy(this);
                return;
            }
            digits.SetCapHeight(source.fontSize);
            // Keep grouping separators as the authored period glyph and omit only spacing.
            Vector2 area=source.rectTransform.rect.size;
            if(source.name=="Wallet coins")digits.SetDigitScale('8',source.text.Trim()=="× 8"?new Vector2(1.10f,1.08f):Vector2.one);
            if(previous!=source.text||previousArea!=area){previous=source.text;previousArea=area;digits.SetFormatted(new string(previous.Select(c=>c==','?'.':c).Where(c=>!char.IsWhiteSpace(c)).ToArray()));}
            source.canvasRenderer.SetAlpha(0);
        }
        void OnDisable(){if(source!=null)source.canvasRenderer.SetAlpha(1);}
    }

    // Replaces digits inside a localized TMP line with the same authored glyphs
    // used by the result counters. The source remains the accessible/localized
    // value; a matching TMP copy preserves its original advances and wrapping.
    public sealed class InlineIllustratedDigits : MonoBehaviour
    {
        static readonly Dictionary<(int Font,int Texture,int Bounds),TMP_SpriteAsset> digitAssets=new Dictionary<(int Font,int Texture,int Bounds),TMP_SpriteAsset>();
        static readonly Dictionary<(int Font,int Texture,int Bounds),TMP_SpriteAsset> symbolAssets=new Dictionary<(int Font,int Texture,int Bounds),TMP_SpriteAsset>();
        static readonly Dictionary<(int Font,int Texture,int Bounds),Rect[]> digitSources=new Dictionary<(int Font,int Texture,int Bounds),Rect[]>();
        static readonly Dictionary<(int Font,int Texture,int Bounds),Rect[]> symbolSources=new Dictionary<(int Font,int Texture,int Bounds),Rect[]>();
        TextMeshProUGUI source,copy;Sprite[] digits,symbols;string lastRaw,lastRendered;TMP_FontAsset lastFont;bool syncing;

        public bool Initialize(TextMeshProUGUI label,Sprite[] numberSprites,Sprite[] symbolSprites)
        {
            if(label==null||numberSprites==null||numberSprites.Length<10||symbolSprites==null||symbolSprites.Length<3)return false;
            source=label;digits=numberSprites;symbols=symbolSprites;
            var font=source.font;
            var digitAsset=Asset(font,digits,"CASCADE_DIGITS",new[]{'0','1','2','3','4','5','6','7','8','9'},digitAssets,digitSources);
            var symbolAsset=Asset(font,symbols,"CASCADE_NUMBER_SIGNS",new[]{'×',':','.'},symbolAssets,symbolSources);
            if(digitAsset==null||symbolAsset==null)return false;
            var copyObject=new GameObject("Illustrated numeral copy",typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));
            copyObject.transform.SetParent(transform,false);
            var rect=(RectTransform)copyObject.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            copy=copyObject.GetComponent<TextMeshProUGUI>();copy.raycastTarget=false;copy.maskable=source.maskable;
            source.OnPreRenderText+=SourcePreRender;
            source.canvasRenderer.SetAlpha(0);
            Synchronize(digitAsset,symbolAsset);
            source.ForceMeshUpdate();
            return true;
        }
        static TMP_SpriteAsset Asset(TMP_FontAsset font,Sprite[] sprites,string prefix,char[] metricCharacters,Dictionary<(int Font,int Texture,int Bounds),TMP_SpriteAsset> cache,Dictionary<(int Font,int Texture,int Bounds),Rect[]> sources)
        {
            if(font==null||sprites==null||sprites.Length!=metricCharacters.Length||sprites[0]==null||sprites.Any(s=>s==null||s.texture!=sprites[0].texture))return null;
            int id=font.GetInstanceID(),boundsHash=17;
            unchecked{foreach(var sprite in sprites)boundsHash=boundsHash*31+sprite.rect.GetHashCode();}
            var key=(Font:id,Texture:sprites[0].texture.GetInstanceID(),Bounds:boundsHash);
            // Concurrent views may create distinct Sprite objects over the same artwork.
            // Assets live with their texture/geometry, never with the last view to render.
            while(cache.TryGetValue(key,out var existing)&&existing!=null){
                if(sources.TryGetValue(key,out var known)&&SameGeometry(known,sprites))return existing;
                key.Bounds++;
            }
            var fallback=font.characterLookupTable.TryGetValue((uint)'0',out var zero)?zero:null;
            if(fallback?.glyph==null)return null;
            var asset=ScriptableObject.CreateInstance<TMP_SpriteAsset>();asset.name=prefix+"_"+id+"_"+asset.GetInstanceID();asset.hideFlags=HideFlags.HideAndDontSave;
            asset.spriteSheet=sprites[0].texture;asset.faceInfo=font.faceInfo;asset.fallbackSpriteAssets=new List<TMP_SpriteAsset>();
            for(int i=0;i<sprites.Length;i++){
                TMP_Character character=font.characterLookupTable.TryGetValue(metricCharacters[i],out var sourceCharacter)&&sourceCharacter?.glyph!=null?sourceCharacter:fallback;
                GlyphMetrics sourceMetrics=character.glyph.metrics;var bounds=sprites[i].rect;
                var rect=new GlyphRect(Mathf.RoundToInt(bounds.x),Mathf.RoundToInt(bounds.y),Mathf.RoundToInt(bounds.width),Mathf.RoundToInt(bounds.height));
                var metrics=new GlyphMetrics(sourceMetrics.width,sourceMetrics.height,sourceMetrics.horizontalBearingX,sourceMetrics.horizontalBearingY,sourceMetrics.horizontalAdvance);
                var glyph=new TMP_SpriteGlyph((uint)i,metrics,rect,1,0,sprites[i]);
                var spriteCharacter=new TMP_SpriteCharacter(0xFFFE,glyph){name=prefix=="CASCADE_DIGITS"?i.ToString():i==0?"multiply":i==1?"colon":"decimal"};
                asset.spriteGlyphTable.Add(glyph);asset.spriteCharacterTable.Add(spriteCharacter);
            }
            // Build name lookup before attaching a material; this avoids an editor-only
            // legacy upgrade path intended for serialized, pre-1.1 sprite assets.
            asset.UpdateLookupTables();
            var shader=Shader.Find("TextMeshPro/Sprite");if(shader==null){Destroy(asset);return null;}
            var material=new Material(shader){name=asset.name+" material",hideFlags=HideFlags.HideAndDontSave};material.mainTexture=asset.spriteSheet;asset.material=material;
            MaterialReferenceManager.AddSpriteAsset(asset);cache[key]=asset;sources[key]=sprites.Select(s=>s.rect).ToArray();return asset;
        }
        static bool SameGeometry(Rect[] a,Sprite[] b)
        {
            if(a==null||b==null||a.Length!=b.Length)return false;
            for(int i=0;i<a.Length;i++)if(b[i]==null||a[i]!=b[i].rect)return false;
            return true;
        }
        void OnEnable()
        {
            if(source==null)return;source.OnPreRenderText-=SourcePreRender;source.OnPreRenderText+=SourcePreRender;source.canvasRenderer.SetAlpha(0);
            Synchronize(Asset(source.font,digits,"CASCADE_DIGITS",new[]{'0','1','2','3','4','5','6','7','8','9'},digitAssets,digitSources),
                Asset(source.font,symbols,"CASCADE_NUMBER_SIGNS",new[]{'×',':','.'},symbolAssets,symbolSources));
        }
        void SourcePreRender(TMP_TextInfo _) { Synchronize(
            Asset(source?.font,digits,"CASCADE_DIGITS",new[]{'0','1','2','3','4','5','6','7','8','9'},digitAssets,digitSources),
            Asset(source?.font,symbols,"CASCADE_NUMBER_SIGNS",new[]{'×',':','.'},symbolAssets,symbolSources)); }
        void Synchronize(TMP_SpriteAsset digitAsset,TMP_SpriteAsset symbolAsset)
        {
            if(syncing||source==null||copy==null||digitAsset==null||symbolAsset==null)return;
            syncing=true;
            copy.font=source.font;copy.fontSharedMaterial=source.fontSharedMaterial;copy.spriteAsset=source.spriteAsset;
            copy.fontSize=source.fontSize;copy.fontSizeMin=source.fontSizeMin;copy.fontSizeMax=source.fontSizeMax;copy.enableAutoSizing=source.enableAutoSizing;
            copy.fontStyle=source.fontStyle;copy.fontWeight=source.fontWeight;copy.alignment=source.alignment;copy.verticalAlignment=source.verticalAlignment;
            copy.textWrappingMode=source.textWrappingMode;copy.overflowMode=source.overflowMode;copy.margin=source.margin;
            copy.lineSpacing=source.lineSpacing;copy.characterSpacing=source.characterSpacing;copy.wordSpacing=source.wordSpacing;copy.paragraphSpacing=source.paragraphSpacing;
            copy.extraPadding=source.extraPadding;copy.isRightToLeftText=source.isRightToLeftText;copy.enableVertexGradient=source.enableVertexGradient;copy.colorGradient=source.colorGradient;
            copy.color=source.color;copy.richText=true;copy.raycastTarget=false;copy.maskable=source.maskable;
            string raw=source.text??string.Empty;
            string rendered=Rewrite(raw,source.richText,digitAsset.name,symbolAsset.name,source.color.a,source.name=="ReceiverLabel"?1.28f:source.name=="Outcome heading"?1.12f:1);
            if(raw!=lastRaw||rendered!=lastRendered||lastFont!=source.font){
                lastRaw=raw;lastRendered=rendered;lastFont=source.font;copy.text=rendered;
                // The transparent source can rebuild after its visible copy in the
                // same Canvas pass. Commit the new glyph mesh now, not one frame later.
                copy.ForceMeshUpdate();
            }
            if(source.canvasRenderer.GetAlpha()!=0)source.canvasRenderer.SetAlpha(0);
            syncing=false;
        }
        static string Rewrite(string value,bool parseMarkup,string numberAsset,string symbolAsset,float alpha,float numeralScale=1)
        {
            if(string.IsNullOrEmpty(value))return value;
            var result=new StringBuilder(value.Length+48);bool inTag=false;
            string alphaHex=Mathf.RoundToInt(Mathf.Clamp01(alpha)*255).ToString("X2",System.Globalization.CultureInfo.InvariantCulture);
            for(int i=0;i<value.Length;i++){
                char c=value[i];
                if(parseMarkup){
                    if(c=='<'){inTag=true;result.Append(c);continue;}
                    if(c=='>'){inTag=false;result.Append(c);continue;}
                    if(inTag){result.Append(c);continue;}
                }else{
                    if(c=='<'){result.Append("&lt;");continue;}
                    if(c=='>'){result.Append("&gt;");continue;}
                }
                if(c>='0'&&c<='9'){if(numeralScale!=1)result.Append("<size=").Append((numeralScale*100).ToString("0",System.Globalization.CultureInfo.InvariantCulture)).Append("%>");AppendSprite(result,numberAsset,c.ToString(),alphaHex);if(numeralScale!=1)result.Append("</size>");}
                else if(c=='×')AppendSprite(result,symbolAsset,"multiply",alphaHex);
                else if(c==':'&&i>0&&i+1<value.Length&&char.IsDigit(value[i-1])&&char.IsDigit(value[i+1]))AppendSprite(result,symbolAsset,"colon",alphaHex);
                else if((c==','||c=='.')&&i>0&&i+1<value.Length&&char.IsDigit(value[i-1])&&char.IsDigit(value[i+1]))AppendSprite(result,symbolAsset,"decimal",alphaHex);
                else if(c=='/'&&HasDigitsAround(value,i))result.Append("<space=1.5px><size=112%>∕</size><space=1.5px>");
                else if(c=='-'&&i>0&&i+1<value.Length&&char.IsDigit(value[i-1])&&char.IsDigit(value[i+1]))result.Append("<size=120%>−</size>");
                else result.Append(c);
            }
            return result.ToString();
        }
        static void AppendSprite(StringBuilder result,string asset,string name,string alphaHex)
        {
            // A neutral vertex color keeps the authored gold/black sprite art intact
            // even when the surrounding localized copy is dark, white, or gradient-tinted.
            result.Append("<color=#FFFFFF").Append(alphaHex).Append("><sprite=\"").Append(asset)
                .Append("\" name=\"").Append(name).Append("\"></color>");
        }
        static bool HasDigitsAround(string value,int index)
        {
            int left=index-1,right=index+1;
            while(left>=0&&char.IsWhiteSpace(value[left]))left--;
            while(right<value.Length&&char.IsWhiteSpace(value[right]))right++;
            return left>=0&&right<value.Length&&char.IsDigit(value[left])&&char.IsDigit(value[right]);
        }
        void OnDisable(){if(source!=null){source.OnPreRenderText-=SourcePreRender;source.canvasRenderer.SetAlpha(1);}}
        void OnDestroy(){if(source!=null)source.OnPreRenderText-=SourcePreRender;}
    }
}
