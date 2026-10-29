using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Solver;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        Sprite[] travelArt,treasureArt;
        UnityEngine.UI.Button fieldTools;Image fieldToolsMeaning;
        RectTransform landingPreview;
        int shopTab,shopItem;
        bool shopDetailOpen;
        GameScreen shopReturnScreen=GameScreen.Home,toolsReturnScreen=GameScreen.TravelBag,noAdsReturnScreen=GameScreen.Home;
        ShopLocation toolsShopLocation;
        int toolsItem;
        UiCopy shopNotice;
        bool toolSearchPending;
        Coroutine toolAppearance;
        float toolCueLeadIn;
        Image guideHand;
        Image displayedShopObject;
        Material unavailableTrophyMaterial,ownedTrophyMaterial;
        float guideTime;
        public string EquippedGift=>Rewards.EquippedGift;
        public int ShopCoins=>GameEconomy?.CoinBalance(Rewards.EarnedCoins(OrderedNodes))??0;
        public int Stock(TravelTool tool)=>GameEconomy?.ToolStock(tool)??0;
        public bool OwnsGift(string id)=>ShopCatalog.TrophyIds.Contains(id)&&(Rewards.Owns(id)||GameEconomy?.Ledger.Cosmetics.Contains(id)==true);
        Sprite TravelArt(int index)
        {
            if(travelArt==null){
                travelArt=new Sprite[4];var texture=Resources.Load<Texture2D>("CascadeArt/TravelTools");
                var cells=new[]{new Rect(72,61,571,519),new Rect(690,70,495,489),new Rect(81,625,455,556),new Rect(661,637,560,540)};
                if(texture!=null)for(int i=0;i<4;i++)travelArt[i]=AuthoredIconRegion(texture,cells[i],1254,1254);

            }
            return travelArt[index];
        }
        Sprite TreasureArt(int index)
        {
            if(treasureArt==null){
                var texture=Resources.Load<Texture2D>("CascadeArt/TravelTreasures");treasureArt=new Sprite[6];
                var cells=new[]{new Rect(112,23,309,472),new Rect(548,62,433,426),new Rect(66,537,426,444),new Rect(570,549,402,419),new Rect(91,1005,386,476),new Rect(589,1006,363,505)};
                if(texture!=null)for(int i=0;i<6;i++)treasureArt[i]=AuthoredIconRegion(texture,cells[i],1024,1536);
            }
            return treasureArt[index];
        }
        static Sprite AuthoredIconRegion(Texture2D texture,Rect source,float width,float height)
        {
            var rect=new Rect(source.x*texture.width/width,(height-source.yMax)*texture.height/height,source.width*texture.width/width,source.height*texture.height/height);
            return Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        }
        Sprite ToolArt(int index)=>index==ShopCatalog.ToolCount?navigationIcons[4]:index==(int)TravelTool.RepairKit?ToolBody(4):index<3?TravelArt(index):TreasureArt(index-3);
        Sprite TrophyArt(int index)
        {
            // The authored bright icons are shared by catalogue, collection and worn pins.
            string bright=index==0?"TrophyLeaf":index==1?"TrophySun":index==2?"TrophyRibbon":index==3?"TrophyMoon":index==5?"TrophyRain":null;
            if(bright!=null){var sprite=ReferenceShopArt(bright);if(sprite!=null)return sprite;}
            return ShopCatalog.IsParcelExclusive(ShopCatalog.TrophyIds[index])?ParcelBrooch(ShopCatalog.TrophyIds[index]):index>=ShopCatalog.MikaTrophyIds.Length?ScreenTrophyArt(ShopCatalog.TrophyIds[index]):index<4?Resources.Load<Sprite>("CascadeArt/Workshop/"+GiftAssets[index]):index<8?TreasureArt(index-2):MikaCostume(ShopCatalog.TrophyIds[index],false,true);
        }
        Material TrophyUnavailableMaterial()
        {
            if(unavailableTrophyMaterial==null){
                var shader=Resources.Load<Shader>("CascadeArt/TrophyAvailability");
                if(shader!=null)unavailableTrophyMaterial=new Material(shader){name="Unavailable trophy grayscale"};
            }
            return unavailableTrophyMaterial;
        }
        Material OwnedTrophyMaterial()
        {
            if(ownedTrophyMaterial==null&&TrophyUnavailableMaterial()!=null){
                ownedTrophyMaterial=new Material(TrophyUnavailableMaterial()){name="Owned trophy grayscale"};
                ownedTrophyMaterial.SetFloat("_Opacity",1);
            }
            return ownedTrophyMaterial;
        }
        bool HasFieldTools { get {for(int i=0;i<ShopCatalog.ToolCount;i++)if(Stock((TravelTool)i)>0)return true;return false;} }
        void BuildFieldTools()
        {
            LoadNavigationArt();
            fieldTools=Button("My tools",actions,new Vector2(.40f,0),new Vector2(.60f,1),ToggleFieldToolTray);fieldTools.name="Field tools";
            StyleGameplayControl(fieldTools,true);
            var label=fieldTools.GetComponentInChildren<TextMeshProUGUI>();label.SetCopy("");
            fieldToolsMeaning=Picture("Tools bag icon",fieldTools.transform,new Vector2(.12f,.12f),new Vector2(.88f,.88f),HomeArt("BagIcon")??navigationIcons[1],true);
            var plus=Rect("Field bag plus badge",fieldTools.transform,new Vector2(.68f,-.03f),new Vector2(1.04f,.33f));plus.gameObject.AddComponent<RoundHudBadge>().raycastTarget=false;var plusLabel=Text("Bag plus",plus,Vector2.zero,Vector2.one,22,UiCopy.Raw("+"));plusLabel.color=Color.white;plusLabel.fontSizeMin=plusLabel.fontSizeMax=22;FitRoundHudBadge(plus,plusLabel);
            RegisterButtonDesign(fieldTools,GameButtonRole.IconPrimary);fieldTools.gameObject.SetActive(false);
            BuildToolTray();
        }
        void LayoutFieldTools(float unit)
        {
            if(fieldTools==null)return;
            LayoutToolTray(unit);LayoutPlayActions();
            if(landingPreview!=null){var close=landingPreview.Find("Landing preview cloth/Close landing preview") as RectTransform;if(close!=null){close.sizeDelta=new Vector2(0,48*unit);close.anchoredPosition=new Vector2(0,12*unit);}}
        }
        void RefreshFieldTools()
        {
            if(fieldTools==null)return;
            bool show=CurrentScreen==GameScreen.Playing;
            fieldTools.gameObject.SetActive(show);
            // Result controls own their lower row; a hidden bag must not stretch Undo again.
            if(undo.transform.parent!=actions)return;
            LayoutPlayActions();
            var u=(RectTransform)undo.transform;var d=(RectTransform)drop.transform;

            fieldTools.interactable=Interaction.CanPlay&&!MechanicPopupOpen&&Interaction.AllowsControl(PracticeControl.Toolbag);
            RefreshToolTray();
        }
        public void SelectShopTab(int tab){shopTab=Mathf.Clamp(tab,0,1);shopDetailOpen=false;shopItem=0;shopNotice=default;DrawMenu();}
        void MarkTrophyDiscovery()
        {
            if(profile.SeenCoachPrompts.Contains("trophy_preview_tap"))return;
            profile.SeenCoachPrompts.Add("trophy_preview_tap");PersistProgress();
        }
        public void SelectShopItem(int index)
        {
            if(CurrentScreen==GameScreen.Supplies&&PendingLesson>=MechanicLesson.Wayfinder&&PendingLesson<=MechanicLesson.LandingLens&&index!=(int)PendingLesson.Value-(int)MechanicLesson.Wayfinder)return;
            shopItem=Mathf.Clamp(index,0,shopTab==0?ShopCatalog.ToolCount-1:ShopCatalog.TrophyIds.Length-1);
            if(CurrentScreen==GameScreen.Collection)giftPage=shopItem;
            if(CurrentScreen==GameScreen.Supplies)toolsItem=shopItem;
            shortfallUntil=0;shopNotice=default;DrawMenu();
        }
        void OpenSelectedShopItem(){if(CurrentScreen!=GameScreen.Collection)return;shopDetailOpen=true;shopNotice=default;DrawMenu();}
        public void ShowSupplies()
        {
            if(!Interaction.AllowsControl(PracticeControl.Toolbag))return;
            if(CurrentScreen==GameScreen.Playing&&!PendingLesson.HasValue&&RequireActionLesson(MechanicLesson.Toolbag))return;
            CompleteLessonPractice(MechanicLesson.Toolbag);
            if(CurrentScreen==GameScreen.Shop&&shopReturnScreen==GameScreen.Supplies){BackFromShop();return;}
            if(CurrentScreen!=GameScreen.Supplies){
                toolsReturnScreen=CurrentScreen==GameScreen.Shop&&(shopReturnScreen==GameScreen.Playing||shopReturnScreen==GameScreen.Pause||shopReturnScreen==GameScreen.Fail)?shopReturnScreen:CurrentScreen;toolsItem=0;
                if(CurrentScreen==GameScreen.Shop)toolsShopLocation=CaptureShop();
            }
            shopTab=0;shopItem=toolsItem;shopNotice=default;shopDetailOpen=false;SetScreen(GameScreen.Supplies);
        }
        bool ToolsInLevel=>toolsReturnScreen==GameScreen.Pause||toolsReturnScreen==GameScreen.Playing||toolsReturnScreen==GameScreen.Fail;
        void BackFromTools(){if(ToolsInLevel)ShowGame();else if(toolsReturnScreen==GameScreen.Shop)RestoreShop(toolsShopLocation);else SetScreen(toolsReturnScreen);}
        void BackFromShop(){if(shopReturnScreen==GameScreen.Shop)ShowHome();else SetScreen(shopReturnScreen);}
        static UiCopy ToolTitle(int i)=>i==ShopCatalog.ToolCount?(UiCopy)"Field guide":new[]{"Wayfinder scroll","Undo spool","Return hourglass","Signal lantern","Landing lens","Repair kit"}[i];
        static UiCopy ToolDescription(int i)=>i==ShopCatalog.ToolCount?(UiCopy)"Watch, then follow the hand.":new[]{
            "Shows moves until this board breaks.",
            "One extra undo on this board.",
            "Returns the last board and its row.",
            "Makes one safe move. Uses wood.",
            "Shows the next landing before you drop.",
            "Board durability +2. Bombs and wind keep running."}[i];
        Button CounterButton(string name,UiCopy copy,float left,float right,float bottom,float top,Action action,bool amber=false)
        {
            if(left==.095f&&right==.905f){left=MenuColumnInset;right=1-MenuColumnInset;}
            var b=Button(copy,menuRoot,new Vector2(left,bottom),new Vector2(right,top),action);b.name=name;
            var face=b.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;if(amber)StylePrimaryFace(face);
            var text=b.GetComponentInChildren<TextMeshProUGUI>();text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;
            text.margin=new Vector4(5,2,5,2);text.textWrappingMode=TextWrappingModes.NoWrap;text.enableAutoSizing=true;text.fontSizeMin=16*MenuUnit;text.fontSizeMax=22*MenuUnit;
            navigationLightLabels.Add(text);RegisterMenuRow(b,top,0,48,0);return b;
        }
        void DrawSupplies()=>DrawReferenceSupplies();
        void QuietDisabledCounter(Button button)
        {
            if(button.interactable)return;
            var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;
            face.color=new Color(.70f,.76f,.72f);
            navigationLightLabels.Add(button.GetComponentInChildren<TextMeshProUGUI>());
        }
        static void DimUnavailableArrow(Button button)
        {
            if(button.interactable)return;
            var arrow=button.transform.Find("Navigation arrow")?.GetComponent<ChevronGraphic>();
            if(arrow!=null)arrow.color=new Color(1,1,1,.42f);
        }
        void DrawTravelCounter(bool bag,bool collection=false)
        {
            if(bag)DrawReferenceSupplies();else DrawReferenceShop(collection);
        }
        void CatalogueSelectionCopy(UiCopy title)
        {
            ObjectNameStrip("Selected object title",new Vector2(.055f,.19f),new Vector2(.945f,.265f),title);
        }
        void CompactCoinPrice(Button button,int amount)
        {
            var caption=button.GetComponentInChildren<TextMeshProUGUI>();caption.SetCopy("Buy");
            caption.rectTransform.anchorMin=new Vector2(.07f,0);caption.rectTransform.anchorMax=new Vector2(.42f,1);
            caption.margin=Vector4.one*(2*MenuUnit);caption.alignment=TextAlignmentOptions.Midline;
            Picture("Coin price icon",button.transform,new Vector2(.45f,.15f),new Vector2(.55f,.85f),RewardEmblem(0),true);
            var price=Text("Coin price",button.transform,new Vector2(.57f,.08f),new Vector2(.93f,.92f),18,UiCopy.Raw("× "+amount.ToString("N0")));
            price.fontSize=18*MenuUnit;price.textWrappingMode=TextWrappingModes.NoWrap;price.margin=Vector4.one*(2*MenuUnit);price.alignment=TextAlignmentOptions.MidlineLeft;
            StyleCelebrationType(price,CelebrationType.EnamelLabel);navigationLightLabels.Add(price);
        }
        public bool BuySelectedShopItem()
        {
            if(saveBlocked||GameEconomy==null||monetization?.FullscreenActive==true||shopTab==0&&shopItem==ShopCatalog.ToolCount)return false;
            string gift=ShopCatalog.TrophyIds[shopItem];if(shopTab==1&&OwnsGift(gift))return false;
            int price=shopTab==0?ShopCatalog.Price((TravelTool)shopItem):ShopCatalog.TrophyPrice(gift);
            if(ShopCoins<price){shopNotice=new UiCopy("You need {0} more coins.",price-ShopCoins);DrawMenu();ShowInsufficientCoins();return false;}
            shortfallUntil=0;
            bool bought=shopTab==0?GameEconomy.TryBuyTool((TravelTool)shopItem,Rewards.EarnedCoins(OrderedNodes)):GameEconomy.TryBuyKeepsake(gift,Rewards.EarnedCoins(OrderedNodes),Rewards);
            shopNotice=bought?(shopTab==0?(UiCopy)"Added to your bag":default):"Could not buy. No coins spent.";
            if(bought){feedback.Sound(ToyCue.Success);feedback.Haptic(.12f);}
            DrawMenu();RefreshFieldTools();return bought;
        }
        bool EquipShopGift(string id)
        {
            shopNotice=default;
            if(saveBlocked||!OwnsGift(id))return false;
            if(ShopCatalog.IsScreenCosmetic(id))return EquipScreenTrophy(id);
            if(GameEconomy?.TryEquipMika(EquippedGift==id?null:id,Rewards)!=true){shopNotice="Could not save. Nothing changed.";return false;}
            CreateGlobalCosmeticAmbiance();feedback.Sound(ToyCue.Select);DrawMenu();return true;
        }
        bool ToolEligible(TravelTool tool)
        {
            
            var a=Interaction.Attempt;
            return !saveBlocked&&!Interaction.IsPreparing&&!Interaction.IsPresenting&&!Interaction.IsSuspended
                &&(tool==TravelTool.UndoSpool?a.CanUseSpool:tool==TravelTool.ReturnHourglass?a.CanUseHourglass:tool==TravelTool.RepairKit?a.CanUseRepairKit:tool==TravelTool.LandingLens?a.CanUseLens:tool==TravelTool.SignalLantern?a.Current.Status==PuzzleStatus.Playing&&!a.Current.Row.SequenceEqual(a.Level.TargetSymbolIds):a.Current.Status==PuzzleStatus.Playing&&!a.Current.Row.SequenceEqual(a.Level.TargetSymbolIds));
        }
        UiCopy ToolEligibilityText(TravelTool tool)
        {
            var a=Interaction.Attempt;
            if(!ToolsInLevel)return "Open a level first";
            if(Stock(tool)==0)return "Your bag is empty";
            
            if(tool==TravelTool.UndoSpool){if(a.FreeUndoRemaining>0)return "Use your free undos first";if(!a.CanUndoHere)return "No move on this board to undo";}
            if(tool==TravelTool.ReturnHourglass&&!a.CanUseHourglass)return "No broken board to return to";
            if(tool==TravelTool.RepairKit&&!a.CanUseRepairKit)return "This board cannot be repaired now.";
            
            if(tool==TravelTool.LandingLens&&!a.CanUseLens)return "No active board";
            
            if(tool==TravelTool.Wayfinder&&a.Current.Row.SequenceEqual(a.Level.TargetSymbolIds))return "The row is ready. Keep your scroll.";
            return default;
        }
        public void UseShopTool(TravelTool tool,RectTransform fieldSource=null)=>UseShopToolCore(tool,fieldSource,false);
        async void UseShopToolCore(TravelTool tool,RectTransform fieldSource,bool afterIntroduction)
        {
            if(CurrentScreen!=GameScreen.Supplies&&(CurrentScreen!=GameScreen.Playing||!fieldTrayOpen&&!afterIntroduction))return;
            if(MechanicPopupOpen||toolSearchPending||!ToolsInLevel||!ToolEligible(tool)||Stock(tool)==0||!Interaction.AllowsTool(tool))return;
            var requestedAttempt=Interaction.Attempt;
            if(!afterIntroduction&&IntroduceTool(tool,()=>{
                if(CurrentScreen==GameScreen.Playing&&ReferenceEquals(Interaction.Attempt,requestedAttempt))UseShopToolCore(tool,fieldSource,true);
            }))return;
            var attempt=Interaction.Attempt;HintOffer guide=null;var screen=CurrentScreen;
            if(tool==TravelTool.Wayfinder||tool==TravelTool.SignalLantern){
                toolSearchPending=true;if(screen==GameScreen.Supplies)DrawMenu();else RefreshToolTray();var request=attempt.CaptureHintRequest();
                try {guide=await Task.Run(()=>AttemptService.CalculateHint(request));}
                catch {toolSearchPending=false;shopNotice="Could not check the route. Nothing used.";if(CanPresent&&CurrentScreen==GameScreen.Supplies)DrawMenu();else if(CanPresent){status.SetCopy(shopNotice);RefreshToolTray();}return;}
                toolSearchPending=false;
                if(!CanPresent||CurrentScreen!=screen||screen==GameScreen.Playing&&!fieldTrayOpen||shopItem!=(int)tool||attempt!=Interaction.Attempt||!attempt.IsCurrent(guide)||!ToolEligible(tool)){if(CanPresent&&CurrentScreen==GameScreen.Supplies)DrawMenu();else if(CanPresent)RefreshToolTray();return;}
            }
            var beforeToolState=attempt.Current;
            var prediction=tool==TravelTool.LandingLens?attempt.Predict(PuzzleCommand.Drop):null;
            if(!Interaction.AllowsTool(tool))return;
            if(!GameEconomy.TryUseTool(tool,attempt,guide)){shopNotice="This tool cannot help here. Nothing used.";if(CurrentScreen==GameScreen.Supplies)DrawMenu();else status.SetCopy(shopNotice);return;}
            CompleteLessonPractice(MechanicLessons.Tool(tool));
            var activatedObject=fieldSource!=null?fieldSource:displayedShopObject!=null?displayedShopObject.rectTransform:null;
            if(activatedObject!=null){var sway=activatedObject.GetComponent<WorldMotion>();if(sway!=null)sway.enabled=false;}
            Interaction.CancelPointer();EndTokenHold();CancelHint();CancelDeadEnd();
            UiCopy reaction=tool==TravelTool.ReturnHourglass?"Back on the board":tool==TravelTool.UndoSpool?"One move undone":tool==TravelTool.RepairKit?"Board reinforced: +2 durability":tool==TravelTool.LandingLens?"Next landing":tool==TravelTool.SignalLantern?"One safe move":"Follow the hand. The scroll knows the way.";
            if(tool==TravelTool.Wayfinder&&guide!=null){attempt.RevealHint(guide);offeredHint=guide;hintVisible=true;PersistProgress();}
            SetScreen(GameScreen.Playing);RenderFrame(new PresentationFrame(beforeToolState));
            feedback.Sound(ToyCue.Success);status.SetCopy(reaction);reactionBanner?.Show(reaction,tool==TravelTool.ReturnHourglass?"Hourglass":tool==TravelTool.UndoSpool?"RibbonSpool":tool==TravelTool.RepairKit?"Repair":tool==TravelTool.LandingLens?"LandingLens":tool==TravelTool.SignalLantern?"SignalLantern":"Wayfinder",activatedObject);
            if(fieldTrayOpen)SetToolTray(false);
            float lead=ReducedMotion?0:.48f;
            if(tool==TravelTool.SignalLantern){
                toolCueLeadIn=lead;
                if(!Interaction.PresentToolTransition(attempt.LastToolTransition)){toolCueLeadIn=0;RenderState();}
            }else if(tool==TravelTool.UndoSpool||tool==TravelTool.ReturnHourglass)eventPlayer.PlayUndo(lead);
            else if(tool==TravelTool.LandingLens){RenderState();OpenLandingPreview(prediction);}
            else {Interaction.BeginRestorePresentation();toolAppearance=StartCoroutine(FinishToolAppearance(attempt,null,lead));}
            RefreshFieldTools();RenderControls();
        }
        IEnumerator FinishToolAppearance(AttemptService attempt,TransitionResult landing,float lead)
        {
            yield return null;
            float elapsed=0;
            while(elapsed<lead){
                if(!CanPresent||CurrentScreen!=GameScreen.Playing||Interaction.Attempt!=attempt)yield break;
                if(!Interaction.IsPaused&&!Interaction.IsSuspended)elapsed+=Mathf.Min(.05f,Time.unscaledDeltaTime);
                yield return null;
            }
            toolAppearance=null;Interaction.FinishPresentation();RenderState();RenderControls();ScheduleDeadEnd();
            if(landing!=null)OpenLandingPreview(landing);
        }
        void StopToolAppearance()
        {
            toolCueLeadIn=0;if(toolAppearance==null)return;
            StopCoroutine(toolAppearance);toolAppearance=null;Interaction?.FinishPresentation();
            if(CanPresent&&Interaction!=null&&CurrentScreen==GameScreen.Playing)RenderState();
        }
        void TickToolGuide()
        {
            bool active=!PendingLesson.HasValue&&CurrentScreen==GameScreen.Playing&&(Interaction.Attempt.GuideActive||HintVisible)&&Interaction.CanPlay&&!IsHoldingToken&&!Interaction.SelectedSlot.HasValue;
            if(guideHand!=null)guideHand.gameObject.SetActive(false);
            if(!active)return;
            tutorialHand.gameObject.SetActive(false);
            if(offeredHint==null||!Interaction.Attempt.IsCurrent(offeredHint)){if(Interaction.Attempt.GuideActive&&hintTask==null)RequestHint();return;}
            if(!hintVisible||offeredHint.Result.Status!=HintStatus.Step)return;
            if(guideHand==null){
                var r=Rect("Wayfinder hand",safeRoot,Vector2.zero,Vector2.zero);r.pivot=new Vector2(.60f,.865f);r.sizeDelta=new Vector2(50,60);
                guideHand=r.gameObject.AddComponent<Image>();guideHand.sprite=tutorialHand.sprite;guideHand.material=tutorialOverlayMaterial;guideHand.raycastTarget=false;guideHand.preserveAspect=true;
                var canvas=r.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=29;
            }
            guideHand.gameObject.SetActive(true);var command=offeredHint.Result.Command.Value;guideTime+=Time.unscaledDeltaTime;
            float progress=ReducedMotion?0:Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Repeat(guideTime,2.8f)-.55f)/1));
            guideHand.rectTransform.position=command.Kind==CommandKind.EarlyDrop?drop.transform.position:Vector3.Lerp(
                machine.TokenVisualPosition(command.Edge)+machine.TokenBodyCentre(command.Edge),
                machine.TokenVisualPosition(command.Kind==CommandKind.LongReach?command.End:command.Edge+1)+machine.TokenBodyCentre(command.Kind==CommandKind.LongReach?command.End:command.Edge+1),progress);
        }
    }
}
