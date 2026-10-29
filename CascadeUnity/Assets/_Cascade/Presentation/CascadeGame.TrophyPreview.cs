using System;
using System.Linq;
using Cascade.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        static readonly string[] TrophyBenefits={
            "Leaf pin, drifting leaves","Sun button, warm motes","Sky outfit, falling feathers","Moon outfit, twinkling stars",
            "Chime pin, floating petals","Raincoat, soft raindrops","Acorn pin, amber leaves","Brooch, glowing fireflies",
            "Pilot cap, falling feathers","Moth mask, twinkling stars","Festival coat, floating petals","Gardener hat, drifting leaves",
            "Explorer hat, drifting leaves","Frog hood, soft raindrops","Knight cape, twinkling stars","Fox mask, floating petals",
            "Mushroom hat, drifting leaves","Bee costume, warm motes","Winter hood, soft snowfall",
            "Pirate hat, falling feathers","Jester suit, floating petals","Mage cloak, twinkling stars",
            "Exclusive crane brooch","Exclusive comet brooch","Exclusive dew bell brooch","Exclusive flower brooch",
            "Exclusive kite brooch","Exclusive compass brooch","Exclusive amber moth brooch","Exclusive moon boat brooch"
        };

        bool DurableTrophyOwned(string id)=>Rewards.Owns(id)||profile.Monetization?.Cosmetics.Contains(id)==true;

        UiCopy TrophyAcquisition(string id)
        {
            if(DurableTrophyOwned(id))return "Collected";
            if(ShopCatalog.IsParcelExclusive(id))return "Only from parcels. Cannot be bought.";
            if(ShopCatalog.IsScreenCosmetic(id)&&!ShopCatalog.ExpansionTrophyIds.Contains(id))return "Coins only";
            return JourneyRewards.Gifts.Contains(id)&&Rewards.EarnedFragments(id)>0?new UiCopy("Parcel parts: {0}/3",Rewards.Fragments(id)):(UiCopy)"Rare parcel or coins";
        }

        TextMeshProUGUI TrophyDetailText(string name,Vector2 min,Vector2 max,int size,UiCopy copy,bool title=false)
        {
            Transform parent=menuRoot;
            if(title){
                float centre=(min.y+max.y)*.5f;
                var strip=Rect("Trophy name papyrus",menuRoot,new Vector2(min.x,centre-.031f),new Vector2(max.x,centre+.031f));
                PaintStoryPaper(strip,false);parent=strip;min=Vector2.zero;max=Vector2.one;
            }
            var label=Text(name,parent,min,max,size,copy);
            label.alignment=TextAlignmentOptions.MidlineLeft;label.enableAutoSizing=true;
            label.enableAutoSizing=false;label.fontSize=(Preferences.LargeText?22:20)*MenuUnit;
            label.margin=new Vector4(4,4,4,4)*MenuUnit;label.textWrappingMode=TextWrappingModes.Normal;
            if(title){
                label.margin=new Vector4(12,6,12,6)*MenuUnit;
                label.alignment=TextAlignmentOptions.Midline;label.textWrappingMode=TextWrappingModes.NoWrap;
                label.enableAutoSizing=true;label.fontSizeMax=(Preferences.LargeText?22:20)*MenuUnit;label.fontSizeMin=14*MenuUnit;
                StyleCelebrationType(label,CelebrationType.PaperTitle);
            }
            return label;
        }

        void DrawTrophySelection(bool collection)
        {
            string id=ShopCatalog.TrophyIds[shopItem];bool owned=OwnsGift(id);
            bool frameStyle=ShopCatalog.IsFrame(id);
            bool screenStyle=ShopCatalog.IsScreenCosmetic(id);
            int groupIndex=Array.IndexOf(CurrentTrophyGroup,id);
            bool widePreview=frameStyle||screenStyle&&CurrentTrophyGroup.Length-groupIndex/6*6<=3;
            var preview=Rect("Trophy appearance preview",menuRoot,new Vector2(.055f,.32f),new Vector2(.945f,.81f));
            Image hero;
            bool showEffectPreview=!collection||owned&&TrophyEquipped(id);
            if(screenStyle)hero=DrawScreenTrophyPreview(preview,id,showEffectPreview);
            else{
                if(showEffectPreview)CosmeticAmbiance(preview,id,true);
                hero=Picture("Trophy Mika preview",preview,new Vector2(.07f,.02f),new Vector2(.93f,.98f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
                AttachMikaCosmetic(hero,id);
            }
            // Locked shop items stay colourful in the full-size appearance preview.
            hero.raycastTarget=true;
            var touch=hero.gameObject.AddComponent<Button>();touch.transition=Selectable.Transition.None;
            touch.onClick.AddListener(()=>{hero.GetComponent<MikaCosmeticView>()?.Poke();hero.GetComponent<ScreenVisitorPreview>()?.Poke();MarkTrophyDiscovery();feedback.Sound(ToyCue.Select);feedback.Haptic(.12f);});
            ObjectNameStrip("Selected object title",new Vector2(.055f,.235f),new Vector2(.945f,.307f),TrophyTitle(id));
            int gate=ShopCatalog.PurchaseClearMilestone(id);
            bool lockedByJourney=gate>Rewards.CosmeticClearCount&&!owned;
            int trophyParts=Rewards.Fragments(id);
            if(!owned&&JourneyRewards.Gifts.Contains(id)&&trophyParts>0){
                var parts=Text("Trophy parts badge",preview,new Vector2(.72f,.81f),new Vector2(.96f,.91f),18,UiCopy.Raw(trophyParts+" / 3"));
                parts.alignment=TextAlignmentOptions.MidlineRight;parts.enableAutoSizing=true;parts.fontSizeMin=14*MenuUnit;parts.fontSizeMax=18*MenuUnit;parts.textWrappingMode=TextWrappingModes.NoWrap;
                StyleCelebrationType(parts,CelebrationType.SkyNumber);
            }

            bool pending=Rewards.Parcels.Any(p=>p.GiftId==id&&!p.Claimed);
            bool parcelTrophy=JourneyRewards.Gifts.Contains(id);
            UiCopy caption;Action action;bool enabled;
            if(owned){
                caption=TrophyEquipped(id)?(UiCopy)"Remove style":"Use this style";
                action=()=>EquipShopGift(id);enabled=!saveBlocked;
            }else if(pending){
                caption="Open parcel";action=()=>OpenSelectedTrophyParcel(id);enabled=!saveBlocked;
            }else if(collection){
                caption=ShopCatalog.IsParcelExclusive(id)?(UiCopy)"Only from parcels":"Shop";
                action=()=>{ShowShop();shopTab=1;shopItem=Array.IndexOf(ShopCatalog.TrophyIds,id);DrawMenu();};
                enabled=!ShopCatalog.IsParcelExclusive(id)&&!saveBlocked;
            }else if(ShopCatalog.IsParcelExclusive(id)){caption="Only from parcels";action=()=>{};enabled=false;
            }else{
                caption=UiCopy.Raw("× "+ShopCatalog.TrophyPrice(id).ToString("N0"));
                action=()=>BuySelectedShopItem();
                enabled=!saveBlocked&&GameEconomy!=null&&monetization?.FullscreenActive!=true&&
                    ShopCoins>=ShopCatalog.TrophyPrice(id)&&!lockedByJourney;
            }
            var primary=CounterButton(collection?"Trophy action":"Buy selected item",caption,.095f,.905f,.138f,.217f,action,true);
            if(!collection&&!owned&&!pending&&!ShopCatalog.IsParcelExclusive(id)){
                CompactCoinPrice(primary,ShopCatalog.TrophyPrice(id));
            }
            primary.interactable=enabled;QuietDisabledCounter(primary);
            menuRows.RemoveAll(row=>row.rect==(RectTransform)primary.transform);
            menuBottoms.Add(new MenuBottom{rect=(RectTransform)primary.transform,bottom=collection?16:64});
            if(collection&&!profile.SeenCoachPrompts.Contains("trophy_preview_tap")){
                var finger=Picture("Touch a trophy or Mika",menuRoot,new Vector2(.47f,.46f),new Vector2(.59f,.54f),tutorialHand.sprite,true);
                finger.rectTransform.pivot=new Vector2(.60f,.865f);
                finger.raycastTarget=false;finger.material=tutorialOverlayMaterial;finger.transform.SetAsLastSibling();
                finger.gameObject.AddComponent<TrophyDiscoveryCue>().Initialize(this,finger.rectTransform,displayedShopObject?.rectTransform,hero.rectTransform);
            }
        }

        void OpenSelectedTrophyParcel(string id)
        {
            var receipt=Rewards.Parcels.FirstOrDefault(p=>p.GiftId==id&&!p.Claimed);if(receipt==null)return;
            parcelReturnsToMap=false;parcelReturnScreen=CurrentScreen;parcelOrder=receipt.Order;parcelOpened=false;parcelAnimationShown=false;parcelBonusRound=false;parcelPreviewBonusClaimed=false;SetScreen(GameScreen.Parcel);
        }
    }
}
