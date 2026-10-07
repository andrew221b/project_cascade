using System;
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
        UnityEngine.UI.Button fieldTools;
        RectTransform landingPreview;
        int shopTab,shopItem;
        GameScreen shopReturnScreen=GameScreen.Home,toolsReturnScreen=GameScreen.TravelBag,noAdsReturnScreen=GameScreen.Home;
        UiCopy shopNotice;
        bool toolSearchPending;
        Image guideHand;
        Image displayedShopObject;
        float guideTime;
        string previewEquippedGift;
        public string EquippedGift=>previewEquippedGift??Rewards.EquippedGift;
        public int ShopCoins=>monetization?.CoinBalance(Rewards.EarnedCoins(OrderedNodes))??0;
        public int Stock(TravelTool tool)=>monetization?.ToolStock(tool)??0;
        public bool OwnsGift(string id)=>Rewards.Owns(id)||monetization?.Ledger.Cosmetics.Contains(id)==true;
        Sprite TravelArt(int index)
        {
            if(travelArt==null){
                travelArt=new Sprite[4];var texture=Resources.Load<Texture2D>("CascadeArt/TravelTools");
                if(texture!=null)for(int i=0;i<4;i++){
                    // Authored gap between the rows, rather than cutting the hourglass rim at the midpoint.
                    float split=texture.height*.53125f,y=i<2?split:0,h=i<2?texture.height-split:split;
                    travelArt[i]=Sprite.Create(texture,new Rect(i%2*texture.width*.5f,y,texture.width*.5f,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                }
            }
            return travelArt[index];
        }
        Sprite TreasureArt(int index)
        {
            if(treasureArt==null)treasureArt=NavigationSlices("TravelTreasures",2,3);
            return treasureArt[index];
        }
        Sprite ToolArt(int index)=>index<3?TravelArt(index):TreasureArt(index-3);
        Sprite TrophyArt(int index)=>index<4?Resources.Load<Sprite>("CascadeArt/Workshop/"+GiftAssets[index]):TreasureArt(index-2);
        bool HasFieldTools { get {for(int i=0;i<ShopCatalog.ToolCount;i++)if(Stock((TravelTool)i)>0)return true;return false;} }
        void BuildFieldTools()
        {
            LoadNavigationArt();
            fieldTools=Button("My tools",actions,new Vector2(.40f,0),new Vector2(.60f,1),ShowSupplies);fieldTools.name="Field tools";
            var face=fieldTools.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=Color.white;
            var label=fieldTools.GetComponentInChildren<TextMeshProUGUI>();label.SetCopy("");
            Picture("Tools bag icon",fieldTools.transform,new Vector2(.17f,.13f),new Vector2(.83f,.87f),navigationIcons[1],true);
            fieldTools.gameObject.SetActive(false);
        }
        void LayoutFieldTools(float unit)
        {
            if(fieldTools==null)return;
            var r=(RectTransform)fieldTools.transform;r.offsetMin=r.offsetMax=Vector2.zero;
            if(landingPreview!=null){var close=landingPreview.Find("Landing preview cloth/Close landing preview") as RectTransform;if(close!=null){close.sizeDelta=new Vector2(0,48*unit);close.anchoredPosition=new Vector2(0,12*unit);}}
        }
        void RefreshFieldTools()
        {
            if(fieldTools==null)return;
            bool show=CurrentScreen==GameScreen.Playing&&HasFieldTools;
            fieldTools.gameObject.SetActive(show);
            // Result controls own their lower row; a hidden bag must not stretch Undo again.
            if(undo.transform.parent!=actions)return;
            var u=(RectTransform)undo.transform;var d=(RectTransform)drop.transform;
            u.anchorMin=new Vector2(show?0:.12f,0);u.anchorMax=new Vector2(show?.36f:.47f,1);
            d.anchorMin=new Vector2(show?.64f:.53f,0);d.anchorMax=new Vector2(show?1:.88f,1);
            fieldTools.interactable=Interaction.CanPlay&&!MechanicPopupOpen;
        }
        public void SelectShopTab(int tab){shopTab=Mathf.Clamp(tab,0,1);shopItem=0;shopNotice=default;DrawMenu();}
        public void SelectShopItem(int index){shopItem=Mathf.Clamp(index,0,shopTab==0?ShopCatalog.ToolCount-1:ShopCatalog.TrophyIds.Length-1);if(CurrentScreen==GameScreen.Collection)giftPage=shopItem;shopNotice=default;DrawMenu();}
        public void ShowSupplies()
        {
            if(CurrentScreen!=GameScreen.Shop)toolsReturnScreen=CurrentScreen;
            else if(shopReturnScreen!=GameScreen.Supplies)toolsReturnScreen=shopReturnScreen;
            shopTab=0;shopItem=0;shopNotice=default;SetScreen(GameScreen.Supplies);
        }
        bool ToolsInLevel=>toolsReturnScreen==GameScreen.Pause||toolsReturnScreen==GameScreen.Playing||toolsReturnScreen==GameScreen.Fail;
        void BackFromTools(){if(ToolsInLevel)ShowGame();else SetScreen(toolsReturnScreen);}
        void BackFromShop(){if(shopReturnScreen==GameScreen.Shop)ShowHome();else SetScreen(shopReturnScreen);}
        static UiCopy ToolTitle(int i)=>new[]{"Wayfinder scroll","Undo spool","Return hourglass","Signal lantern","Landing lens"}[i];
        static UiCopy ToolDescription(int i)=>new[]{
            "Points out each next move on this board. Stops when the board breaks.",
            "One extra undo after your two free undos. Works only on the current board.",
            "Brings back the last broken board in this stage. Moves made since then are undone.",
            "Makes one safe move for you. Uses wood normally. Once per stage.",
            "Shows the exact next landing before you descend. No pieces move."}[i];
        Button CounterButton(string name,UiCopy copy,float left,float right,float bottom,float top,Action action,bool amber=false)
        {
            var b=Button(copy,menuRoot,new Vector2(left,bottom),new Vector2(right,top),action);b.name=name;
            var face=b.GetComponent<Image>();face.sprite=enamelControls[amber?1:0];face.type=Image.Type.Sliced;face.color=Color.white;
            var text=b.GetComponentInChildren<TextMeshProUGUI>();text.rectTransform.anchorMin=new Vector2(.06f,.06f);text.rectTransform.anchorMax=new Vector2(.94f,.94f);
            text.margin=new Vector4(5,2,5,2);text.textWrappingMode=TextWrappingModes.NoWrap;text.enableAutoSizing=true;text.fontSizeMin=16*MenuUnit;text.fontSizeMax=22*MenuUnit;
            if(!amber)navigationLightLabels.Add(text);RegisterMenuRow(b,top,0,48,0);return b;
        }
        void DrawSupplies(){DrawTravelCounter(true);}
        void QuietDisabledCounter(Button button)
        {
            if(button.interactable)return;
            var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;
            face.color=new Color(.70f,.76f,.72f);
            navigationLightLabels.Add(button.GetComponentInChildren<TextMeshProUGUI>());
        }
        void DrawTravelCounter(bool bag,bool collection=false)
        {
            if(collection)shopTab=1;
            LoadNavigationArt();NavigationHeader(collection?"Trophies":bag?"My tools":TestMonetization?"Test shop":"Shop",collection?()=>SetScreen(collectionReturn):bag?(Action)BackFromTools:BackFromShop);
            var headingPlate=(RectTransform)menuRoot.Find("Navigation heading");headingPlate.anchorMin=new Vector2(.06f,.906f);headingPlate.anchorMax=new Vector2(.94f,.985f);menuRows.Add(new MenuRow{rect=headingPlate,top=.985f,height=48});FitMenuRows();
            var backButton=headingPlate.GetComponentInChildren<Button>();var backRect=(RectTransform)backButton.transform;backRect.anchorMin=new Vector2(.01f,0);backRect.anchorMax=new Vector2(.20f,1);backRect.sizeDelta=Vector2.zero;
            // One continuous catalogue surface. The actual stock lives on two wooden shelves.
            MenuReadingSurface("Workshop catalogue",new Vector2(.055f,.035f),new Vector2(.945f,.815f));
            if(collection){
                MenuReadingSurface("Collection count cloth",new Vector2(.08f,.817f),new Vector2(.92f,.899f));
                Text("Trophy collection count",menuRoot,new Vector2(.11f,.826f),new Vector2(.89f,.891f),20,new UiCopy("Trophies collected: {0} / {1}",ShopCatalog.TrophyIds.Count(OwnsGift),ShopCatalog.TrophyIds.Length));
            }
            if(!collection){
                MenuReadingSurface("Coin wallet cloth",new Vector2(.08f,.817f),new Vector2(.92f,.899f));
                Picture("Earned coin pouch",menuRoot,new Vector2(.10f,.826f),new Vector2(.21f,.891f),bag?navigationIcons[1]:TravelArt(3),true);
                var coins=Text("Wallet coins",menuRoot,new Vector2(.23f,.826f),new Vector2(bag?.87f:.45f,.891f),22,bag?new UiCopy("Tools left: {0}",Enumerable.Range(0,ShopCatalog.ToolCount).Sum(i=>Stock((TravelTool)i))):UiCopy.Raw(ShopCoins.ToString("N0")));coins.alignment=TextAlignmentOptions.MidlineLeft;coins.font=art.titleFont;
                if(!bag)RewardedShopOffer();
            }
            if(!bag&&!collection){
                ShopCategoryButtons();
            }else {
                var note=Text("Catalogue guidance",menuRoot,new Vector2(.16f,.726f),new Vector2(.84f,.817f),18,collection?"Choose a trophy to take with Mika.":"Pick a tool. Each can be used once per stage.");
                note.enableAutoSizing=true;note.fontSizeMin=16*MenuUnit;note.fontSizeMax=18*MenuUnit;
            }
            int count=shopTab==0?ShopCatalog.ToolCount:ShopCatalog.TrophyIds.Length,columns=shopTab==0?3:4;
            shopItem=Mathf.Clamp(shopItem,0,count-1);float span=.83f/columns;
            for(int i=0;i<count;i++){
                int selected=i;float x=.085f+i%columns*span+(shopTab==0&&i>=3?span*.5f:0),top=.719f-(i/columns)*.137f;
                var button=Button("",menuRoot,new Vector2(x+.008f,top-.119f),new Vector2(x+span-.008f,top),()=>SelectShopItem(selected));button.name="Shop object "+i;button.interactable=!toolSearchPending;
                var face=button.GetComponent<Image>();face.sprite=art.panel;face.type=Image.Type.Sliced;face.color=i==shopItem?new Color(.99f,.79f,.36f):new Color(.86f,.91f,.85f,.55f);
                var picture=Picture("Object on shelf",button.transform,new Vector2(.09f,.14f),new Vector2(.91f,.96f),shopTab==0?ToolArt(i):TrophyArt(i),true);
                if(i==shopItem)displayedShopObject=picture;
                bool owned=shopTab==1&&OwnsGift(ShopCatalog.TrophyIds[i]);int stock=shopTab==0?Stock((TravelTool)i):0;
                if(owned||shopTab==0){var mark=Text("Owned stock",button.transform,new Vector2(.52f,.01f),new Vector2(.96f,.24f),14,owned?"Yours":UiCopy.Raw("×"+stock));mark.font=art.titleFont;mark.enableAutoSizing=true;mark.fontSizeMin=12*MenuUnit;mark.fontSizeMax=14*MenuUnit;}
            }
            Shelf(.582f);Shelf(.445f);
            string gift=ShopCatalog.TrophyIds[shopItem];var tool=(TravelTool)shopItem;
            var title=Text("Selected object title",menuRoot,new Vector2(.105f,.383f),new Vector2(.895f,.431f),25,shopTab==0?ToolTitle(shopItem):(UiCopy)GiftNames[shopItem]);title.font=art.titleFont;title.textWrappingMode=TextWrappingModes.NoWrap;title.enableAutoSizing=true;title.fontSizeMin=18*MenuUnit;title.fontSizeMax=25*MenuUnit;
            UiCopy benefit=shopTab==0?ToolDescription(shopItem):shopItem<4?"Mika wears it on the map. You can also earn it in parcels.":"A trophy for Mika's travels. Wear it on the map. No puzzle advantage.";
            var explanation=Text("Selected object benefit",menuRoot,new Vector2(.11f,.291f),new Vector2(.89f,.376f),19,benefit);explanation.enableAutoSizing=true;explanation.fontSizeMin=17*MenuUnit;explanation.fontSizeMax=19*MenuUnit*(Preferences.LargeText?1.12f:1);
            UiCopy state=shopTab==0?(UiCopy)"Tools: up to 2 stars. No clean record.":OwnsGift(gift)?(UiCopy)"Collected":collection&&shopItem<4?new UiCopy("Parts found: {0} / 3",Rewards.Fragments(gift)):(UiCopy)"Yours forever";
            if(!bag&&!string.IsNullOrEmpty(shopNotice.Key))state=shopNotice;
            var stockLabel=Text("Owned item count",menuRoot,new Vector2(.11f,.230f),new Vector2(.89f,.283f),18,state);stockLabel.textWrappingMode=shopTab==0?TextWrappingModes.Normal:TextWrappingModes.NoWrap;stockLabel.enableAutoSizing=true;stockLabel.fontSizeMin=16*MenuUnit;stockLabel.fontSizeMax=18*MenuUnit;
            int price=shopTab==0?ShopCatalog.Price(tool):ShopCatalog.KeepsakePrice;
            if(bag){
                var use=CounterButton("Use selected tool",toolSearchPending?"Checking the route…":"Use one",.095f,.905f,.138f,.217f,()=>UseShopTool(tool),true);
                use.interactable=!toolSearchPending&&ToolsInLevel&&Stock(tool)>0&&ToolEligible(tool);
                var why=ToolEligibilityText(tool);if(!string.IsNullOrEmpty(why.Key))use.GetComponentInChildren<TextMeshProUGUI>().SetCopy(why);QuietDisabledCounter(use);
                CounterButton("Open shop","Shop",.095f,.492f,.049f,.125f,ShowShop);
                CounterButton("Tools back",ToolsInLevel?"Continue":"Back",.508f,.905f,.049f,.125f,BackFromTools);
            }else if(collection){
                bool owned=OwnsGift(gift);
                UiCopy caption=owned?(EquippedGift==gift?(UiCopy)"With Mika":"Take along"):shopItem<4?(Rewards.EarnedFragments(gift)>=3?(UiCopy)"Open parcel":Rewards.SelectedGift==gift?(UiCopy)"Next trophy":"Choose this trophy"):(UiCopy)"Find in shop";
                var action=CounterButton("Trophy action",caption,.095f,.905f,.138f,.217f,()=>{if(owned)EquipShopGift(gift);else if(shopItem>=4){ShowShop();shopTab=1;DrawMenu();}else if(Rewards.EarnedFragments(gift)>=3)ShowParcel();else{Rewards.SelectGift(gift);PersistProgress();DrawMenu();}},true);
                action.interactable=owned?EquippedGift!=gift:shopItem>=4||Rewards.EarnedFragments(gift)>=3||Rewards.SelectedGift!=gift;QuietDisabledCounter(action);
                CounterButton("Collection back","Back",.095f,.905f,.049f,.125f,()=>SetScreen(collectionReturn));
            }else{
                bool owned=shopTab==1&&OwnsGift(gift);
                UiCopy caption=owned?(EquippedGift==gift?(UiCopy)"With Mika":"Take along"):TestMonetization?(UiCopy)"Try this item":new UiCopy("Buy · {0} coins",price);
                var buy=CounterButton("Buy selected item",caption,.095f,.905f,.138f,.217f,()=>BuySelectedShopItem(),true);
                buy.interactable=!saveBlocked&&monetization!=null&&!monetization.FullscreenActive&&(owned?EquippedGift!=gift:(TestMonetization||ShopCoins>=price)&&(shopTab!=0||Stock(tool)<ShopCatalog.StockLimit));
                if(!TestMonetization&&!owned&&ShopCoins<price)buy.GetComponentInChildren<TextMeshProUGUI>().SetCopy(new UiCopy("Need {0} more coins",price-ShopCoins));
                else if(shopTab==0&&Stock(tool)>=ShopCatalog.StockLimit)buy.GetComponentInChildren<TextMeshProUGUI>().SetCopy("Bag is full");QuietDisabledCounter(buy);
                bool fromTools=shopReturnScreen==GameScreen.Supplies||shopReturnScreen==GameScreen.Playing||shopReturnScreen==GameScreen.Pause||shopReturnScreen==GameScreen.Fail;
                CounterButton("My tools link",fromTools?"Tool bag":shopReturnScreen==GameScreen.Collection?"Trophies":"My things",.095f,.905f,.049f,.125f,
                    fromTools?(Action)ShowSupplies:shopReturnScreen==GameScreen.Collection?()=>SetScreen(GameScreen.Collection):ShowTravelBag);
            }
        }
        public bool BuySelectedShopItem()
        {
            if(saveBlocked||monetization==null)return false;
            string gift=ShopCatalog.TrophyIds[shopItem];if(shopTab==1&&OwnsGift(gift))return EquipShopGift(gift);
            bool bought=TestMonetization?monetization.TryPreviewItem((TravelTool)shopItem,shopTab==1?gift:null):shopTab==0?monetization.TryBuyTool((TravelTool)shopItem,Rewards.EarnedCoins(OrderedNodes)):monetization.TryBuyKeepsake(gift,Rewards.EarnedCoins(OrderedNodes),Rewards);
            shopNotice=bought?(shopTab==0?(UiCopy)"Added to your bag":"Added to your collection"):"Could not buy. No coins spent.";
            if(bought)feedback.Sound(ToyCue.Success);DrawMenu();RefreshFieldTools();return bought;
        }
        bool EquipShopGift(string id)
        {
            shopNotice=default;
            if(TestMonetization){if(!OwnsGift(id))return false;previewEquippedGift=id;feedback.Sound(ToyCue.Select);DrawMenu();return true;}
            string previous=Rewards.EquippedGift;if(!Rewards.Equip(id,monetization?.Ledger.Cosmetics))return false;
            PersistProgress();if(HasSaveError){Rewards.EquippedGift=previous;return false;}
            feedback.Sound(ToyCue.Select);DrawMenu();return true;
        }
        bool ToolEligible(TravelTool tool)
        {
            if(Interaction?.Attempt.ToolUsesRemaining==0)return false;
            var a=Interaction.Attempt;
            return !saveBlocked&&!Interaction.IsPreparing&&!Interaction.IsPresenting&&!Interaction.IsSuspended
                &&(tool==TravelTool.UndoSpool?a.CanUseSpool:tool==TravelTool.ReturnHourglass?a.CanUseHourglass:tool==TravelTool.LandingLens?a.CanUseLens:tool==TravelTool.SignalLantern?a.Current.Status==PuzzleStatus.Playing&&!a.LanternUsed&&!a.Current.Row.SequenceEqual(a.Level.TargetSymbolIds):a.Current.Status==PuzzleStatus.Playing&&!a.WayfinderUsed&&!a.Current.Row.SequenceEqual(a.Level.TargetSymbolIds));
        }
        UiCopy ToolEligibilityText(TravelTool tool)
        {
            var a=Interaction.Attempt;
            if(!ToolsInLevel)return "Open a level first";
            if(Stock(tool)==0)return "Your bag is empty";
            if(a.ToolUsesRemaining==0)return "Two tools used. Try the next step yourself.";
            if(tool==TravelTool.UndoSpool){if(a.SpoolUsed)return "Already used this stage";if(a.FreeUndoRemaining>0)return "Use your free undos first";if(!a.CanUndoHere)return "No move on this board to undo";}
            if(tool==TravelTool.ReturnHourglass&&!a.CanUseHourglass)return a.HourglassUsed?"Already used this stage":"No broken board to return to";
            if(tool==TravelTool.SignalLantern&&a.LanternUsed)return "Already used this stage";
            if(tool==TravelTool.LandingLens&&!a.CanUseLens)return a.LensUsed?"Already used this stage":"This is the last board";
            if(tool==TravelTool.Wayfinder&&a.WayfinderUsed)return "Already used this stage";
            if(tool==TravelTool.Wayfinder&&a.Current.Row.SequenceEqual(a.Level.TargetSymbolIds))return "The row is ready. Keep your scroll.";
            return default;
        }
        public async void UseShopTool(TravelTool tool)
        {
            if(MechanicPopupOpen||toolSearchPending||!ToolsInLevel||!ToolEligible(tool)||Stock(tool)==0)return;
            if(IntroduceTool(tool))return;
            var attempt=Interaction.Attempt;HintOffer guide=null;
            if(tool==TravelTool.Wayfinder||tool==TravelTool.SignalLantern){
                toolSearchPending=true;DrawMenu();var request=attempt.CaptureHintRequest();
                try {guide=await Task.Run(()=>AttemptService.CalculateHint(request));}
                catch {toolSearchPending=false;shopNotice="Could not check the route. Nothing used.";if(CanPresent)DrawMenu();return;}
                toolSearchPending=false;
                if(!CanPresent||CurrentScreen!=GameScreen.Supplies||shopItem!=(int)tool||attempt!=Interaction.Attempt||!attempt.IsCurrent(guide)||!ToolEligible(tool)){if(CanPresent&&CurrentScreen==GameScreen.Supplies)DrawMenu();return;}
            }
            var beforeToolState=attempt.Current;
            var prediction=tool==TravelTool.LandingLens?attempt.Predict(PuzzleCommand.Drop):null;
            if(!monetization.TryUseTool(tool,attempt,guide)){shopNotice="This tool cannot help here. Nothing used.";DrawMenu();return;}
            var activatedObject=displayedShopObject!=null?displayedShopObject.rectTransform:null;
            if(activatedObject!=null){var sway=activatedObject.GetComponent<WorldMotion>();if(sway!=null)sway.enabled=false;}
            Interaction.CancelPointer();EndTokenHold();CancelHint();CancelDeadEnd();RenderState();ScheduleDeadEnd();
            UiCopy reaction=tool==TravelTool.ReturnHourglass?"Back on the board":tool==TravelTool.UndoSpool?"One move undone":"Follow the hand. The scroll knows the way.";
            if(tool==TravelTool.SignalLantern){SetScreen(GameScreen.Playing);RenderFrame(new PresentationFrame(beforeToolState));Interaction.PresentToolTransition(attempt.LastToolTransition);feedback.Sound(ToyCue.Success);reactionBanner?.Show("One safe step","Wayfinder",activatedObject);RefreshFieldTools();return;}
            ShowGame();
            if(tool==TravelTool.LandingLens){OpenLandingPreview(prediction);RefreshFieldTools();return;}
            feedback.Sound(ToyCue.Success);status.SetCopy(reaction);reactionBanner?.Show(reaction,tool==TravelTool.ReturnHourglass?"Hourglass":tool==TravelTool.UndoSpool?"RibbonSpool":"Wayfinder",activatedObject);
            if(tool==TravelTool.Wayfinder&&guide!=null){attempt.RevealHint(guide);offeredHint=guide;PersistProgress();}
        }
        void TickToolGuide()
        {
            bool active=CurrentScreen==GameScreen.Playing&&Interaction.Attempt.GuideActive&&Interaction.CanPlay&&!IsHoldingToken&&!Interaction.SelectedSlot.HasValue;
            if(guideHand!=null)guideHand.gameObject.SetActive(false);
            if(!active)return;
            tutorialHand.gameObject.SetActive(false);
            if(offeredHint==null||!Interaction.Attempt.IsCurrent(offeredHint)){if(hintTask==null)RequestHint();return;}
            if(offeredHint.Result.Status!=HintStatus.Step)return;
            if(guideHand==null){
                var r=Rect("Wayfinder hand",safeRoot,Vector2.zero,Vector2.zero);r.pivot=new Vector2(.5f,.9f);r.sizeDelta=new Vector2(50,60);
                guideHand=r.gameObject.AddComponent<Image>();guideHand.sprite=tutorialHand.sprite;guideHand.material=tutorialOverlayMaterial;guideHand.raycastTarget=false;guideHand.preserveAspect=true;
                var canvas=r.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=29;
            }
            guideHand.gameObject.SetActive(true);var command=offeredHint.Result.Command.Value;guideTime+=Time.unscaledDeltaTime;
            float progress=ReducedMotion?0:Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Repeat(guideTime,2.8f)-.55f)/1));
            guideHand.rectTransform.position=command.Kind==CommandKind.EarlyDrop?drop.transform.position:Vector3.Lerp(machine.TokenPosition(command.Edge),machine.TokenPosition(command.Kind==CommandKind.LongReach?command.End:command.Edge+1),progress);
        }
    }
}
