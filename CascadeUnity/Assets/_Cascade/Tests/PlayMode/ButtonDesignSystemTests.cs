#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Persistence;
using Cascade.Presentation;
using Cascade.Solver;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed partial class UiPolishReviewTests
    {
        void VerifyButtonPresets(string context)
        {
            var safe=(RectTransform)Field("safeRoot");
            foreach(var button in game.GetComponentsInChildren<Button>().Where(b=>b.name=="Travel"||b.name=="Map settings")){
                var design=button.GetComponent<GameButtonDesign>();Assert.That(design?.Bound,Is.True,context+" / "+button.name+" must use the shared design");
                Assert.That(design.Preset.Role,Is.EqualTo(button.name=="Travel"?GameButtonRole.PrimaryGreen:GameButtonRole.IconSecondary));
            }
            foreach(var design in game.GetComponentsInChildren<GameButtonDesign>()){
                if(!design.Bound)continue;
                var root=(RectTransform)design.transform;var size=design.DesiredSize(root.rect.size);
                Assert.That(Vector2.Distance(root.rect.size,size),Is.LessThan(.1f),context+" / "+root.name+" uses the shared dimensions");
                var corners=new Vector3[4];root.GetWorldCorners(corners);var low=camera.WorldToScreenPoint(corners[0]);var high=camera.WorldToScreenPoint(corners[2]);var screen=camera.pixelRect;
                Assert.That(low.x,Is.GreaterThanOrEqualTo(screen.xMin-.2f),context+" / "+root.name+" left screen bound");Assert.That(high.x,Is.LessThanOrEqualTo(screen.xMax+.2f),context+" / "+root.name+" right screen bound");
                Assert.That(low.y,Is.GreaterThanOrEqualTo(screen.yMin-.2f),context+" / "+root.name+" bottom screen bound");Assert.That(high.y,Is.LessThanOrEqualTo(screen.yMax+.2f),context+" / "+root.name+" top screen bound");
                foreach(var label in root.GetComponentsInChildren<TextMeshProUGUI>()){
                    if(label.GetComponentInParent<RoundHudBadge>()!=null||string.IsNullOrEmpty(label.text))continue;
                    Assert.That(label.font,Is.SameAs(design.Preset.Font),context+" / "+root.name+" shared font");
                }
                if(root.GetComponentInChildren<LocalizedCaption>()?.SourceIdentity=="Map"){
                    Assert.That(design.Preset.Role,Is.EqualTo(GameButtonRole.SecondaryYellow));
                    Assert.That(root.GetComponentsInChildren<Image>().Count(i=>i.name=="Shared map scroll icon"),Is.EqualTo(1),"Every Map uses the failure screen's scroll");
                }
            }
        }
        IEnumerator SettleButtons(string context,bool capture=false)
        {
            float until=Time.realtimeSinceStartup+10;while((bool)Field("applyingUiLanguage")&&Time.realtimeSinceStartup<until)yield return null;
            Assert.That((bool)Field("applyingUiLanguage"),Is.False,"The requested locale settles before measuring controls");
            yield return null;yield return null;game.RefreshLayout();Canvas.ForceUpdateCanvases();
            VerifyRows(context);VerifyButtonPresets(context);
            if(capture)yield return Shot(context);
        }
        [UnityTest,Timeout(600000)] public IEnumerator SharedActionDesignsFitMobileLocalesAndPreserveShopCards()
        {
            game.ReducedMotion=true;
            foreach(var size in new[]{new Vector2Int(320,568),new Vector2Int(390,844),new Vector2Int(430,932)})foreach(string locale in new[]{"en","ru","uk"}){
                Resize(size.x,size.y);game.SetUiLocale(locale);game.Preferences.LargeText=size.x==320;
                string key="shared-"+locale+"-"+size.x;bool capture=size.x==390;
                foreach(var screen in new[]{GameScreen.Home,GameScreen.AccountGate,GameScreen.Pause,GameScreen.Fail,GameScreen.JourneyMap,GameScreen.NoAds,GameScreen.Shop,GameScreen.TravelBag,GameScreen.Settings}){
                    Route(screen);yield return SettleButtons(key+"-"+screen,capture);
                }
                game.ShowShop();game.SelectShopTab(1);yield return SettleButtons(key+"-Trophies",capture);
                foreach(var card in game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Shop object ",StringComparison.Ordinal)))
                    Assert.That(card.GetComponent<GameButtonDesign>()?.Bound??false,Is.False,"Trophy cards keep their original skin, illustration and dimensions");
                game.ShowSupplies();yield return SettleButtons(key+"-Supplies",capture);game.ShowCollection();yield return SettleButtons(key+"-Collection",capture);
                Call("DrawReferenceCoinPacks");yield return null;yield return null;
                foreach(var cardAction in game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Purchase ",StringComparison.Ordinal))){
                    Assert.That(cardAction.GetComponent<GameButtonDesign>()?.Bound??false,Is.False,"Coin offer actions are part of the preserved product cards");
                    Assert.That(((RectTransform)cardAction.transform.Find("Reference shop button body")).sizeDelta,Is.EqualTo(new Vector2(267,63)));
                }
                game.ShowHome();game.OpenLegalDocument("privacy");yield return SettleButtons(key+"-Privacy",capture);game.CloseLegalDocument();
                var age=game.AudienceAgeBand;typeof(CascadeGame).GetField("audienceAgeBand",Private).SetValue(game,AudienceAgeBand.Unknown);
                Route(GameScreen.AccountGate);yield return SettleButtons(key+"-Age",capture);
                typeof(CascadeGame).GetField("audienceAgeBand",Private).SetValue(game,age);game.ShowHome();
            }
        }
        [UnityTest,Timeout(300000)] public IEnumerator SharedMapAndGameplayButtonsSurviveCheckpointWinAndNavigation()
        {
            game.ReducedMotion=true;
            foreach(string locale in new[]{"en","ru","uk"}){
                Resize(390,844);game.SetUiLocale(locale);
                yield return ReachCheckpoint();yield return SettleButtons("shared-"+locale+"-Checkpoint",true);
                var map=((RectTransform)Field("resultRoot")).Find("Resulthome").GetComponent<Button>();map.onClick.Invoke();
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap),"Map still opens the journey");
                game.LoadNode("C01");typeof(CascadeGame).GetField("journeyActive",Private).SetValue(game,true);game.ShowGame();
                float deadline=Time.realtimeSinceStartup+30;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<deadline)yield return null;Assert.That(game.Interaction.CanPlay,Is.True);
                yield return SettleButtons("shared-"+locale+"-Playing",true);
                Call("SetToolTray",true,true);yield return SettleButtons("shared-"+locale+"-FieldTools",true);Call("SetToolTray",false,true);
                foreach(var command in PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver).Path){game.Interaction.Execute(command);deadline=Time.realtimeSinceStartup+30;while(game.Interaction.IsPresenting&&Time.realtimeSinceStartup<deadline)yield return null;}
                deadline=Time.realtimeSinceStartup+30;while(game.CurrentScreen!=GameScreen.Win&&Time.realtimeSinceStartup<deadline)yield return null;Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.Win));
                yield return SettleButtons("shared-"+locale+"-Win",true);
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Completion map").onClick.Invoke();Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.JourneyMap));
            }
        }
        [UnityTest] public IEnumerator LiveMapAndShopCaptionsUseTheSharedIconAndSkinWithoutRebuildingTheButton()
        {
            game.ShowHome();var safe=(RectTransform)Field("safeRoot");var host=new GameObject("Dynamic action review",typeof(RectTransform)).GetComponent<RectTransform>();host.SetParent(safe,false);host.sizeDelta=new Vector2(720,1280);host.localScale=Vector3.one*Mathf.Min(safe.rect.width/720,safe.rect.height/1280);
            object Invoke(string key,params object[] args)=>typeof(CascadeGame).GetMethod(key,Private).Invoke(game,args);
            var button=(Button)Invoke("ReferenceButton","Dynamic primary action",host,120f,620f,480f,104f,(Action)(()=>{}),"GreenAction");var body=(Transform)Invoke("ReferenceButtonBody",button);
            var label=(TextMeshProUGUI)Invoke("ReferenceText","dynamic action copy",body,(UiCopy)"Continue",20f,12f,440f,80f,34f,true,false);yield return null;yield return null;
            var design=button.GetComponent<GameButtonDesign>();Assert.That(design.Bound,Is.True);
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);while((bool)Field("applyingUiLanguage"))yield return null;yield return SettleButtons("dynamic actions settled "+locale);
                foreach(string key in new[]{"Map","Back to map","Back to the map","To the map"}){
                    label.SetCopy(new UiCopy("Continue"));design.Refresh();Assert.That(design.Preset.Role,Is.EqualTo(GameButtonRole.PrimaryGreen));
                    label.SetCopy(new UiCopy(key));design.Refresh();Canvas.ForceUpdateCanvases();
                    Assert.That(design.Preset.Role,Is.EqualTo(GameButtonRole.SecondaryYellow));Assert.That(label.GetComponent<LocalizedCaption>().SourceIdentity,Is.EqualTo("Map"));Assert.That(label.text,Is.EqualTo(new UiCopy("Map").ToString()));
                    var icon=button.GetComponentsInChildren<Image>().Single(i=>i.name=="Shared map scroll icon");Assert.That(icon.sprite,Is.SameAs(FailureReferenceArt.Get(8)));Assert.That(Mathf.Max(icon.rectTransform.rect.width,icon.rectTransform.rect.height)/design.DesignUnit,Is.EqualTo(48).Within(.1f));
                    var face=button.GetComponentsInChildren<Image>().Single(i=>i.sprite==design.Preset.Skin);Assert.That(face.GetComponent<PrimaryButtonSkin>(),Is.Not.Null);
                    VerifyRows("live map "+locale);label.SetCopy(new UiCopy("Continue"));design.Refresh();Assert.That(button.GetComponentsInChildren<Image>().Any(i=>i.name=="Shared map scroll icon"),Is.False);
                }
            }
            label.SetCopy(new UiCopy("Find in shop"));design.Refresh();Canvas.ForceUpdateCanvases();Assert.That(label.GetComponent<LocalizedCaption>().SourceIdentity,Is.EqualTo("Shop"));Assert.That(button.GetComponentsInChildren<Image>().Count(i=>i.name=="Shared shop icon"),Is.EqualTo(1));VerifyRows("live shop");
            UnityEngine.Object.Destroy(host.gameObject);
        }
        [UnityTest] public IEnumerator FiveButtonPrefabsBindAsReusableControls()
        {
            var safe=(RectTransform)Field("safeRoot");
            var overlay=new GameObject("Button design system sheet",typeof(RectTransform),typeof(Image));overlay.transform.SetParent(safe,false);
            var rect=(RectTransform)overlay.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;overlay.GetComponent<Image>().color=new Color(.94f,.94f,.87f);
            var stage=new GameObject("720 reference units",typeof(RectTransform)).GetComponent<RectTransform>();stage.SetParent(rect,false);stage.sizeDelta=new Vector2(720,1280);stage.localScale=Vector3.one*Mathf.Min(rect.rect.width/720,rect.rect.height/1280);
            void Label(string copy,float y,int font=28){var go=new GameObject(copy,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(stage,false);var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.sizeDelta=new Vector2(690,58);r.anchoredPosition=new Vector2(0,-y);var text=go.GetComponent<TextMeshProUGUI>();text.font=GameButtonPreset.Load(GameButtonRole.PrimaryGreen).Font;text.fontSize=font;text.text=copy;text.alignment=TextAlignmentOptions.Center;text.color=new Color(.2f,.08f,.02f);}
            Label("FLYING LADDERS · BUTTONS",50,32);
            string folder="Assets/_Cascade/Content/Resources/CascadeArt/DesignSystem/Buttons/";
            int i=0;foreach(GameButtonRole role in Enum.GetValues(typeof(GameButtonRole))){
                var preset=GameButtonPreset.Load(role);Assert.That(preset,Is.Not.Null);Assert.That(preset.Role,Is.EqualTo(role));
                Assert.That(preset.Size,Is.EqualTo(role==GameButtonRole.IconPrimary||role==GameButtonRole.IconSecondary?new Vector2(96,96):new Vector2(480,104)));Assert.That(preset.FontSize,Is.EqualTo(34));Assert.That(preset.IconSize,Is.EqualTo(48));Assert.That(preset.Inset,Is.GreaterThanOrEqualTo(2));
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(folder+role+".prefab");Assert.That(prefab,Is.Not.Null);Assert.That(prefab.GetComponent<GameButtonDesign>().Preset,Is.SameAs(preset));
                if(role==GameButtonRole.TertiaryLabel){Assert.That(prefab.GetComponent<Image>().color.a,Is.Zero);Assert.That(prefab.GetComponentInChildren<TextMeshProUGUI>().fontSharedMaterial.IsKeywordEnabled("UNDERLAY_ON"),Is.True);}
                var control=UnityEngine.Object.Instantiate(prefab,stage);var r=(RectTransform)control.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-180-i*210);
                control.GetComponent<GameButtonDesign>().Initialize(safe,role);
                var text=control.GetComponentInChildren<TextMeshProUGUI>();if(text!=null)text.SetCopy(UiCopy.Raw(role==GameButtonRole.TertiaryLabel?"Условия":role==GameButtonRole.IconSecondary?"?":"Магазин"));
                Label(role.ToString(),110+i*210);i++;
            }
            yield return SettleButtons("shared-button-library",true);
            var controls=overlay.GetComponentsInChildren<GameButtonDesign>();for(int j=1;j<controls.Length;j++)Assert.That(Mathf.Abs(controls[j].transform.position.y-controls[j-1].transform.position.y),Is.GreaterThan(.1f),"Prefab binding must preserve each authored position");UnityEngine.Object.Destroy(overlay);
        }
    }
}
#endif
