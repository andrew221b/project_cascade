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
        Sprite[] travelArt;
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
        public void SelectShopTab(int tab){shopTab=Mathf.Clamp(tab,0,1);shopItem=0;shopNotice=default;DrawMenu();}
        public void SelectShopItem(int index){shopItem=Mathf.Clamp(index,0,shopTab==0?2:3);shopNotice=default;DrawMenu();}
        public void ShowSupplies()
        {
            if(CurrentScreen!=GameScreen.Shop)toolsReturnScreen=CurrentScreen;
            else if(shopReturnScreen!=GameScreen.Supplies)toolsReturnScreen=shopReturnScreen;
            shopTab=0;shopItem=0;shopNotice=default;SetScreen(GameScreen.Supplies);
        }
        bool ToolsInLevel=>toolsReturnScreen==GameScreen.Pause||toolsReturnScreen==GameScreen.Playing||toolsReturnScreen==GameScreen.Fail;
        void BackFromTools(){if(ToolsInLevel)ShowGame();else SetScreen(toolsReturnScreen);}
        void BackFromShop(){if(shopReturnScreen==GameScreen.Shop)ShowHome();else SetScreen(shopReturnScreen);}
        static UiCopy ToolTitle(int i)=>new[]{"Wayfinder scroll","Undo spool","Return hourglass"}[i];
        static UiCopy ToolDescription(int i)=>new[]{
            "Points out each next move on this board. Stops when the board breaks.",
            "One extra undo after your two free undos. Works only on the current board.",
            "Brings back the last broken board in this stage. Moves made since then are undone."}[i];
        Button CounterButton(string name,UiCopy copy,float left,float right,float bottom,float top,Action action,bool amber=false)
        {
            var b=Button(copy,menuRoot,new Vector2(left,bottom),new Vector2(right,top),action);b.name=name;
            var face=b.GetComponent<Image>();face.sprite=enamelControls[amber?1:0];face.type=Image.Type.Sliced;face.color=Color.white;
            var text=b.GetComponentInChildren<TextMeshProUGUI>();text.rectTransform.anchorMin=new Vector2(.06f,.06f);text.rectTransform.anchorMax=new Vector2(.94f,.94f);
            text.margin=new Vector4(5,2,5,2);text.textWrappingMode=TextWrappingModes.NoWrap;text.enableAutoSizing=true;text.fontSizeMin=16*MenuUnit;text.fontSizeMax=22*MenuUnit;
            if(!amber)navigationLightLabels.Add(text);return b;
        }
        void DrawSupplies(){DrawTravelCounter(true);}
        void QuietDisabledCounter(Button button)
        {
            if(button.interactable)return;
            var face=button.GetComponent<Image>();face.sprite=art.panel;face.type=Image.Type.Sliced;
            navigationLightLabels.Remove(button.GetComponentInChildren<TextMeshProUGUI>());
        }
        void DrawTravelCounter(bool bag)
        {
            LoadNavigationArt();NavigationHeader(bag?"My tools":TestMonetization?"Test shop":"Shop",bag?(Action)BackFromTools:BackFromShop);
            MenuReadingSurface("Coin wallet cloth",new Vector2(.09f,.80f),new Vector2(.91f,.867f));
            Picture("Earned coin pouch",menuRoot,new Vector2(.11f,.806f),new Vector2(.23f,.862f),TravelArt(3),true);
            var coins=Text("Wallet coins",menuRoot,new Vector2(.25f,.805f),new Vector2(bag?.88f:.45f,.862f),22,bag?new UiCopy("{0} coins",ShopCoins):UiCopy.Raw(ShopCoins.ToString("N0")));coins.alignment=TextAlignmentOptions.MidlineLeft;coins.font=art.titleFont;
            if(!bag)RewardedShopOffer();
            if(!bag){
                CounterButton("Tools tab","Tools",.08f,.48f,.700f,.782f,()=>SelectShopTab(0),shopTab==0);
                CounterButton("Keepsakes tab","Keepsakes",.52f,.92f,.700f,.782f,()=>SelectShopTab(1),shopTab==1);
            }else NavigationNote("Tools guidance","Pick a tool. Each can be used once per stage.",.700f,.782f);
            int count=shopTab==0?3:4;float span=.84f/count;
            for(int i=0;i<count;i++){
                int selected=i;float x=.08f+i*span;
                var b=Button("",menuRoot,new Vector2(x+.012f,.575f),new Vector2(x+span-.012f,.695f),()=>SelectShopItem(selected));b.name="Shop object "+i;b.interactable=!toolSearchPending;
                var face=b.GetComponent<Image>();face.color=i==shopItem?new Color(.97f,.75f,.31f):new Color(.06f,.29f,.29f);face.sprite=art.panel;face.type=Image.Type.Sliced;
                var sprite=shopTab==0?TravelArt(i):Resources.Load<Sprite>("CascadeArt/Workshop/"+GiftAssets[i]);
                Picture("Tool on shelf",b.transform,new Vector2(.08f,.09f),new Vector2(.92f,.94f),sprite,true);
            }
            Shelf(.557f);
            string gift=JourneyRewards.Gifts[shopItem];var tool=(TravelTool)shopItem;
            var objectArt=shopTab==0?TravelArt(shopItem):Resources.Load<Sprite>("CascadeArt/Workshop/"+GiftAssets[shopItem]);
            var display=Picture("Selected shop object",menuRoot,new Vector2(.31f,.453f),new Vector2(.69f,.557f),objectArt,true);
            displayedShopObject=display;
            display.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.8f),.65f,.55f);
            MenuReadingSurface("Shop explanation cloth",new Vector2(.08f,.195f),new Vector2(.92f,.448f));
            var title=Text("Selected object title",menuRoot,new Vector2(.12f,.392f),new Vector2(.88f,.441f),26,shopTab==0?ToolTitle(shopItem):(UiCopy)GiftNames[shopItem]);title.font=art.titleFont;title.textWrappingMode=TextWrappingModes.NoWrap;title.enableAutoSizing=true;title.fontSizeMin=19*MenuUnit;title.fontSizeMax=26*MenuUnit;
            UiCopy benefit=shopTab==0?ToolDescription(shopItem):"Mika wears it on the map. You can also earn it in parcels.";
            bool hasNotice=!string.IsNullOrEmpty(shopNotice.Key);
            var explanation=Text("Selected object benefit",menuRoot,new Vector2(.13f,hasNotice?.272f:.247f),new Vector2(.87f,.385f),20,benefit);explanation.enableAutoSizing=true;explanation.fontSizeMin=(Preferences.LargeText?20:18)*MenuUnit;explanation.fontSizeMax=20*MenuUnit*(Preferences.LargeText?1.12f:1);
            if(hasNotice){var notice=Text("Shop confirmation",menuRoot,new Vector2(.13f,.238f),new Vector2(.87f,.272f),18,shopNotice);notice.textWrappingMode=TextWrappingModes.NoWrap;notice.enableAutoSizing=true;notice.fontSizeMin=18*MenuUnit;notice.fontSizeMax=20*MenuUnit;}
            UiCopy stock=shopTab==0?new UiCopy("In your bag: {0} / {1}",Stock(tool),ShopCatalog.StockLimit):OwnsGift(gift)?(EquippedGift==gift?(UiCopy)"With Mika":"Already yours"):(UiCopy)"Yours forever";
            var owned=Text("Owned item count",menuRoot,new Vector2(.13f,.200f),new Vector2(bag||TestMonetization?.87f:.55f,.236f),20,stock);owned.alignment=bag||TestMonetization?TextAlignmentOptions.Center:TextAlignmentOptions.MidlineLeft;owned.textWrappingMode=TextWrappingModes.NoWrap;owned.enableAutoSizing=true;owned.fontSizeMin=(Preferences.LargeText?20:18)*MenuUnit;owned.fontSizeMax=20*MenuUnit*(Preferences.LargeText?1.12f:1);
            int price=shopTab==0?ShopCatalog.Price(tool):ShopCatalog.KeepsakePrice;
            if(!bag&&!TestMonetization){var cost=Text("Total item price",menuRoot,new Vector2(.57f,.200f),new Vector2(.87f,.236f),20,new UiCopy("{0} coins",price));cost.alignment=TextAlignmentOptions.MidlineRight;cost.textWrappingMode=TextWrappingModes.NoWrap;cost.enableAutoSizing=true;cost.fontSizeMin=(Preferences.LargeText?20:18)*MenuUnit;cost.fontSizeMax=20*MenuUnit*(Preferences.LargeText?1.12f:1);}
            if(bag){
                var use=CounterButton("Use selected tool",toolSearchPending?"Checking the route…":"Use one",.08f,.92f,.108f,.190f,()=>UseShopTool(tool),true);
                use.interactable=!toolSearchPending&&ToolsInLevel&&Stock(tool)>0&&ToolEligible(tool);
                UiCopy why=ToolEligibilityText(tool);
                if(!string.IsNullOrEmpty(why.Key))use.GetComponentInChildren<TextMeshProUGUI>().SetCopy(why);
                QuietDisabledCounter(use);
                CounterButton("Open shop","Shop",.08f,.48f,.008f,.090f,ShowShop);
                CounterButton("Tools back","Back",.52f,.92f,.008f,.090f,BackFromTools);
            }else{
                bool alreadyOwned=shopTab==1&&OwnsGift(gift);
                UiCopy label=alreadyOwned?(EquippedGift==gift?(UiCopy)"With Mika":"Take along"):TestMonetization?(UiCopy)"Try this item":new UiCopy("Buy · {0} coins",price);
                var buy=CounterButton("Buy selected item",label,.08f,.92f,.108f,.190f,()=>BuySelectedShopItem(),true);
                buy.interactable=!saveBlocked&&monetization!=null&&!monetization.FullscreenActive&&(alreadyOwned?EquippedGift!=gift:(TestMonetization||ShopCoins>=price)&&(shopTab!=0||Stock(tool)<ShopCatalog.StockLimit));
                if(!TestMonetization&&!alreadyOwned&&ShopCoins<price)buy.GetComponentInChildren<TextMeshProUGUI>().SetCopy(new UiCopy("Need {0} more coins",price-ShopCoins));
                else if(shopTab==0&&Stock(tool)>=ShopCatalog.StockLimit)buy.GetComponentInChildren<TextMeshProUGUI>().SetCopy("Bag is full");
                QuietDisabledCounter(buy);
                CounterButton("My tools link","Tool bag",.08f,.48f,.008f,.090f,ShowSupplies);
                CounterButton("Remove ads link","No ads",.52f,.92f,.008f,.090f,ShowNoAds);
            }
        }
        public bool BuySelectedShopItem()
        {
            if(saveBlocked||monetization==null)return false;
            string gift=JourneyRewards.Gifts[shopItem];if(shopTab==1&&OwnsGift(gift))return EquipShopGift(gift);
            bool bought=TestMonetization?monetization.TryPreviewItem((TravelTool)shopItem,shopTab==1?gift:null):shopTab==0?monetization.TryBuyTool((TravelTool)shopItem,Rewards.EarnedCoins(OrderedNodes)):monetization.TryBuyKeepsake(gift,Rewards.EarnedCoins(OrderedNodes),Rewards);
            shopNotice=bought?(UiCopy)"Added to your bag":"Could not buy. No coins spent.";
            if(bought)feedback.Sound(ToyCue.Success);DrawMenu();return bought;
        }
        bool EquipShopGift(string id)
        {
            if(TestMonetization){if(!OwnsGift(id))return false;previewEquippedGift=id;feedback.Sound(ToyCue.Select);DrawMenu();return true;}
            string previous=Rewards.EquippedGift;if(!Rewards.Equip(id,monetization?.Ledger.Cosmetics))return false;
            PersistProgress();if(HasSaveError){Rewards.EquippedGift=previous;return false;}
            feedback.Sound(ToyCue.Select);DrawMenu();return true;
        }
        bool ToolEligible(TravelTool tool)
        {
            var a=Interaction.Attempt;
            return !saveBlocked&&!Interaction.IsPreparing&&!Interaction.IsPresenting&&!Interaction.IsSuspended
                &&(tool==TravelTool.UndoSpool?a.CanUseSpool:tool==TravelTool.ReturnHourglass?a.CanUseHourglass:a.Current.Status==PuzzleStatus.Playing&&!a.WayfinderUsed&&!a.Current.Row.SequenceEqual(a.Level.TargetSymbolIds));
        }
        UiCopy ToolEligibilityText(TravelTool tool)
        {
            var a=Interaction.Attempt;
            if(!ToolsInLevel)return "Open a level first";
            if(Stock(tool)==0)return "Your bag is empty";
            if(tool==TravelTool.UndoSpool){if(a.SpoolUsed)return "Already used this stage";if(a.FreeUndoRemaining>0)return "Use your free undos first";if(!a.CanUndoHere)return "No move on this board to undo";}
            if(tool==TravelTool.ReturnHourglass&&!a.CanUseHourglass)return a.HourglassUsed?"Already used this stage":"No broken board to return to";
            if(tool==TravelTool.Wayfinder&&a.WayfinderUsed)return "Already used this stage";
            if(tool==TravelTool.Wayfinder&&a.Current.Row.SequenceEqual(a.Level.TargetSymbolIds))return "The row is ready. Keep your scroll.";
            return default;
        }
        public async void UseShopTool(TravelTool tool)
        {
            if(MechanicPopupOpen||toolSearchPending||!ToolsInLevel||!ToolEligible(tool)||Stock(tool)==0)return;
            if(IntroduceTool(tool))return;
            var attempt=Interaction.Attempt;HintOffer guide=null;
            if(tool==TravelTool.Wayfinder){
                toolSearchPending=true;DrawMenu();var request=attempt.CaptureHintRequest();
                try {guide=await Task.Run(()=>AttemptService.CalculateHint(request));}
                catch {toolSearchPending=false;shopNotice="Could not check the route. Nothing used.";if(CanPresent)DrawMenu();return;}
                toolSearchPending=false;
                if(!CanPresent||CurrentScreen!=GameScreen.Supplies||shopItem!=(int)tool||attempt!=Interaction.Attempt||!attempt.IsCurrent(guide)||!ToolEligible(tool))return;
            }
            if(!monetization.TryUseTool(tool,attempt,guide)){shopNotice="This tool cannot help here. Nothing used.";DrawMenu();return;}
            var activatedObject=displayedShopObject!=null?displayedShopObject.rectTransform:null;
            if(activatedObject!=null){var sway=activatedObject.GetComponent<WorldMotion>();if(sway!=null)sway.enabled=false;}
            Interaction.CancelPointer();EndTokenHold();CancelHint();CancelDeadEnd();RenderState();ScheduleDeadEnd();ShowGame();
            UiCopy reaction=tool==TravelTool.ReturnHourglass?"Back on the board":tool==TravelTool.UndoSpool?"One move undone":"Follow the hand. The scroll knows the way.";
            feedback.Sound(ToyCue.Success);status.SetCopy(reaction);reactionBanner?.Show(reaction,tool==TravelTool.ReturnHourglass?"Hourglass":tool==TravelTool.UndoSpool?"RibbonSpool":"Wayfinder",activatedObject);
            if(guide!=null){attempt.RevealHint(guide);offeredHint=guide;PersistProgress();}
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
