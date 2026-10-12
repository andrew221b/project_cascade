using TMPro;
using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        float illustratedNumberScan;
        internal void ApplyControlCaption(TextMeshProUGUI label,float unit=1){
            var button=label.GetComponentInParent<UnityEngine.UI.Button>();
            bool tab=button!=null&&(button.name.StartsWith("Mapmode",System.StringComparison.Ordinal)
                ||button.name.StartsWith("Trophy category ",System.StringComparison.Ordinal)||button.name.EndsWith(" tab",System.StringComparison.Ordinal));
            label.enableAutoSizing=false;label.fontSize=(tab?(Preferences.LargeText?18:16):(Preferences.LargeText?22:18))*unit;
            label.fontSizeMin=label.fontSizeMax=label.fontSize;
            label.margin=new Vector4(4,4,4,4)*unit;
            label.textWrappingMode=tab?TextWrappingModes.NoWrap:TextWrappingModes.Normal;label.lineSpacing=0;
        }
        void ApplyMenuTypography(){
            if(menuRoot==null)return;
            float unit=MenuUnit;
            foreach(var label in menuRoot.GetComponentsInChildren<TextMeshProUGUI>()){
                if(label.name=="Illustrated numeral copy")continue;
                if(IsSymbolFace(label.name)||label.name=="Mini glyph")continue;
                var button=label.GetComponentInParent<UnityEngine.UI.Button>();
                if(label.name=="Label"&&button!=null){
                    label.font=Preferences.LargeText?Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceTitle")??art.titleFont:art.titleFont;
                    if(CurrentScreen==GameScreen.Settings){
                        // Settings has a narrow scroll column on phones. Keep each action on
                        // one line and let longer localized captions shrink to fit instead of
                        // inheriting the generic large-text wrapping that clips at 320 px.
                        label.enableAutoSizing=true;label.fontSizeMin=14*unit;
                        label.fontSizeMax=(Preferences.LargeText?18:17)*unit;
                        label.margin=new Vector4(18,6,42,6)*unit;
                        label.textWrappingMode=TextWrappingModes.NoWrap;label.lineSpacing=0;
                    }else if(CurrentScreen==GameScreen.Win&&(button.name=="Claim trophy"||button.name=="Continue completion")){
                        label.enableAutoSizing=true;label.fontSizeMin=16*unit;
                        label.fontSizeMax=(Preferences.LargeText?24:22)*unit;
                        label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;
                        label.margin=new Vector4(20,8,20,8)*unit;
                        label.alignment=TextAlignmentOptions.Midline;label.textWrappingMode=TextWrappingModes.NoWrap;label.lineSpacing=0;
                    }else if(button.name=="My journal navigation"){
                        // Ukrainian is wider than the other bag tabs. Keep this
                        // action on one line and shrink the caption within its button.
                        label.enableAutoSizing=true;label.fontSizeMin=14*unit;
                        label.fontSizeMax=(Preferences.LargeText?19:18)*unit;
                        label.margin=new Vector4(1,3,1,3)*unit;
                        label.textWrappingMode=TextWrappingModes.NoWrap;label.lineSpacing=0;
                        label.rectTransform.anchorMin=new Vector2(.23f,0);label.rectTransform.anchorMax=new Vector2(.87f,1);
                    }else ApplyControlCaption(label,unit);
                    if(int.TryParse(label.text,out _))SetMenuType(label,Preferences.LargeText?28:26,unit);
                }else if(label.name=="Page heading"||label.name=="Menu title"||label.name=="Region"||label.name=="Comic title"){
                    label.enableAutoSizing=false;label.fontSize=((label.name=="Region"||label.name=="Comic title")?(Preferences.LargeText?24:22):(Preferences.LargeText?28:26))*unit;label.font=art.titleFont;
                    label.textWrappingMode=TextWrappingModes.Normal;
                    if(label.name=="Page heading"){
                        label.enableAutoSizing=true;label.fontSizeMin=14*unit;label.fontSizeMax=(Preferences.LargeText?28:26)*unit;
                        label.textWrappingMode=TextWrappingModes.NoWrap;
                    }
                }else if(label.name=="Wallet coins"||label.name=="Coin balance"||label.name=="Trophy collection count"){
                    SetMenuType(label,label.name=="Wallet coins"&&CurrentScreen==GameScreen.Shop?(Preferences.LargeText?17:15):(Preferences.LargeText?21:19),unit);
                    FitWalletCounter(label,unit);
                }else if(label.name=="Navigation page count"||label.name=="Level shelf page"||label.name=="Request progress"||label.name=="Request speaker"){
                    SetMenuType(label,Preferences.LargeText?24:22,unit);
                }
            }
            ApplyMenuSpriteCounters();
        }
        void ApplyMenuSpriteCounters()
        {
            if(safeRoot==null)return;
            foreach(var label in safeRoot.GetComponentsInChildren<TextMeshProUGUI>()){
                if(label==null||label.name=="Illustrated numeral copy"||label.GetComponent<InlineIllustratedDigits>()!=null
                    ||label.GetComponentInParent<InlineIllustratedDigits>()!=null||label.GetComponent<MenuSpriteCounter>()!=null
                    ||IsSymbolFace(label.name)||label.name=="Mini glyph")continue;
                // Preserve the island's compact completed/available level badge as requested.
                if(label.name.StartsWith("Island completed levels ",System.StringComparison.Ordinal))continue;
                string value=label.text;
                if(string.IsNullOrWhiteSpace(value)||!System.Linq.Enumerable.Any(value,char.IsDigit))continue;
                // The shop's short limits explain costs and gameplay consequences; keep them as plain copy.
                if(label.name=="Purchase limits")continue;
                bool counterOnly=System.Linq.Enumerable.All(value,c=>char.IsDigit(c)||char.IsWhiteSpace(c)||c=='×'||c=='/'||c==':'||c==','||c=='.'||c=='-');
                if(counterOnly){
                    // Keep the original value for localization/accessibility and replace only its rendering.
                    var counter=label.gameObject.AddComponent<MenuSpriteCounter>();
                    counter.Initialize(label,VictoryDigits(),VictorySymbols(),art.displayFont);
                }else{
                    // Inline labels keep their original TMP advances, wrapping and text value.
                    var inline=label.gameObject.AddComponent<InlineIllustratedDigits>();
                    if(!inline.Initialize(label,VictoryDigits(),VictorySymbols()))Destroy(inline);
                }
            }
        }
        void SetMenuType(TextMeshProUGUI label,float size,float unit)
        {
            label.enableAutoSizing=false;label.fontSize=label.fontSizeMin=label.fontSizeMax=size*unit;
            label.font=art.titleFont;label.margin=Vector4.zero;
        }
        void FitWalletCounter(TextMeshProUGUI label,float unit)
        {
            label.textWrappingMode=TextWrappingModes.NoWrap;
            float width=menuRoot.rect.width,right=(label.name=="Wallet coins"&&CurrentScreen==GameScreen.Shop ? .91f : .945f)*width;
            float textWidth=Mathf.Min(right-.055f*width-43*unit,
                Mathf.Ceil(label.GetPreferredValues(label.text,Mathf.Infinity,Mathf.Infinity).x)+2*unit);
            if(label.name=="Wallet coins"&&CurrentScreen==GameScreen.Shop&&label.text.Trim()=="× 8")textWidth=Mathf.Min(right-.055f*width-43*unit,textWidth+5*unit);
            var rect=label.rectTransform;
            rect.anchorMin=new Vector2((right-textWidth)/width,rect.anchorMin.y);
            rect.anchorMax=new Vector2(.945f,rect.anchorMax.y);rect.offsetMin=rect.offsetMax=Vector2.zero;
            string iconName=label.name=="Coin balance"?"Coin wallet":label.name=="Trophy collection count"?"Collection badge":"Earned coin pouch";
            var icon=menuRoot.Find(iconName) as RectTransform;if(icon==null)return;
            icon.anchorMin=new Vector2((right-textWidth-43*unit)/width,rect.anchorMin.y);
            icon.anchorMax=new Vector2((right-textWidth-3*unit)/width,rect.anchorMax.y);icon.offsetMin=icon.offsetMax=Vector2.zero;
            if(label.name=="Trophy collection count")foreach(var heading in menuRoot.GetComponentsInChildren<TextMeshProUGUI>())if(heading.name=="Page heading"){
                // Keep the page title centered, with a symmetric gap before the trophy counter.
                float titleWidth=Mathf.Max(40*unit,2*(right-textWidth-43*unit-width*.5f-6*unit));
                var title=heading.rectTransform;title.anchorMin=new Vector2(.5f,title.anchorMin.y);title.anchorMax=new Vector2(.5f,title.anchorMax.y);
                title.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,titleWidth);title.anchoredPosition=new Vector2(0,title.anchoredPosition.y);
                heading.enableAutoSizing=true;heading.fontSizeMin=14*unit;heading.fontSizeMax=(Preferences.LargeText?28:26)*unit;heading.textWrappingMode=TextWrappingModes.NoWrap;
            }
        }
    }
}
