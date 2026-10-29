using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        bool parcelBonusRound,parcelPreviewBonusClaimed;
        ParcelRevealView parcelView;
        TextMeshProUGUI parcelStatus,parcelClaimCaption,parcelBonusCaption;
        Button parcelClaimButton,parcelBonusButton;
        bool ParcelAwaitingVideo=>Rewards?.Parcels.FirstOrDefault(p=>p.Order==parcelOrder) is ParcelReceipt receipt&&!receipt.Claimed&&monetization?.HasParcelOpen(MonetizationLedger.ParcelSubject(parcelOrder))!=true;
        Sprite ParcelArt(string name)=>CleanMenuArt.Get(name)??Resources.Load<Sprite>("CascadeArt/ParcelReferenceOct13/"+name);
        void DrawReferenceParcel()
        {
            var receipt=Rewards.Parcels.First(p=>p.Order==parcelOrder);
            bool proof=monetization?.HasParcelBonus(MonetizationLedger.ParcelSubject(parcelOrder))==true;
            if(receipt.Claimed&&proof&&!receipt.BonusClaimed&&!parcelPreviewBonusClaimed)parcelBonusRound=true;
            bool collected=parcelBonusRound?(receipt.BonusClaimed||parcelPreviewBonusClaimed):receipt.Claimed;
            var stage=ReferenceShopSurface();
            var environment=menuRoot.Find("Reference shop environment").GetComponent<Image>();
            if(ParcelArt("Backdrop")!=null)environment.sprite=ParcelArt("Backdrop");
            environment.pixelsPerUnitMultiplier=environment.canvas.referencePixelsPerUnit/environment.sprite.pixelsPerUnit;
            InventoryIsland(stage,48,467,92,118,0);
            InventoryIsland(stage,587,384,82,118,1);
            stage.Find("Natural menu foliage").SetAsLastSibling();
            var header=ReferencePicture("Parcel header board",stage,ReferenceShopArt("HeaderBoard"),89,101,546,128,false);
            header.type=Image.Type.Sliced;header.pixelsPerUnitMultiplier=4;
            var parcelTitle=ReferenceText("parcel title",stage,ReferenceCopy("Rewards from your parcel!","Награды из посылки!","Нагороди з посилки!"),139,133,448,65,39);CentreHeading(header,parcelTitle);
            ReferencePicture("Parcel subtitle paper",stage,ParcelArt("SubtitlePaper"),135,216,450,83,false);
            parcelStatus=ReferenceText("parcel subtitle",stage,ReferenceCopy("Your parcel has arrived!\nHere are your rewards:","Твоя посылка прибыла!\nВот твои награды:","Твоя посилка прибула!\nОсь твої нагороди:"),155,226,410,62,22,false,true);
            parcelStatus.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceBody")??parcelStatus.font;
            if(ParcelAwaitingVideo)parcelStatus.SetCopy(ReferenceCopy("Watch a video to open your parcel.","Посмотри видео, чтобы открыть посылку.","Переглянь відео, щоб відкрити посилку."));
            if(parcelBonusRound)parcelStatus.SetCopy(ReferenceCopy("Thanks for watching!\nHere are your extra rewards:","Спасибо, что смотришь рекламу!\nВот твои награды:","Дякуємо за перегляд реклами!\nОсь твої нагороди:"));
            var lightWindow=ReferenceRect("Parcel light behind boards and frame",stage,53,47,614,1020);lightWindow.gameObject.AddComponent<RectMask2D>();
            var light=ReferenceRect("Parcel animated sunbeams",lightWindow,-183,143,980,755).gameObject.AddComponent<ParcelSunlight>();lightWindow.SetSiblingIndex(2);
            ReferencePicture("Parcel sandstone platform",stage,ParcelArt("StonePlatform"),32,652,656,155);
            var box=ReferencePicture("Parcel closed box",stage,ParcelArt("ClosedParcel"),117,305,486,479);
            var body=ReferencePicture("Parcel unwrapped body",stage,ParcelArt("UnwrappedParcel"),117,305,486,479);
            var trim=ReferencePicture("Parcel moving rope and tag",stage,ParcelArt("ParcelAdornments"),159,332,400,396);
            var opened=ReferencePicture("Parcel opened box and loose wrapping",stage,ParcelArt("OpenedParcel"),85,327,551,453);
            var envelope=ReferencePicture("Parcel bonus envelope",stage,ParcelArt("BonusEnvelope"),258,444,221,153);
            var emptyEnvelope=ReferencePicture("Parcel opened bonus envelope",stage,ParcelArt("OpenedEnvelope"),250,418,231,182);
            var flareWindow=ReferenceRect("Parcel flash clip",stage,76,315,568,470);flareWindow.gameObject.AddComponent<RectMask2D>();
            Rect("Parcel opening flash",flareWindow,Vector2.zero,Vector2.one).gameObject.AddComponent<ParcelOpeningFlash>();
            var drops=parcelBonusRound?receipt.BonusClaimed?receipt.BonusLoot:ParcelLoot.Resolve(ParcelLoot.Bonus(parcelOrder),Rewards,GameEconomy.Ledger)
                :ParcelLoot.IsLoot(receipt)?GameEconomy.PreviewParcelDrops(receipt,Rewards):LegacyParcelDrops(receipt);
            var cards=new List<RectTransform>();int count=drops.Count;float width=211,gap=8,left=(720-(width*count+gap*(count-1)))*.5f;
            for(int i=0;i<count;i++){
                var card=ReferenceRect("Parcel reward card "+i,stage,left+i*(width+gap),788,width,220);
                ReferencePicture("Parcel decorative card frame",card,ParcelArt("RewardCard")??ReferenceShopArt("SelectedCard"),-5,-6,width+10,232,false);
                var drop=drops[i];var icon=drop.Kind=="coins"?ReferenceShopCoin():drop.Kind=="tool"&&ShopCatalog.TryTool(drop.Id,out var tool)?ToolArt((int)tool):TrophyArt(Array.IndexOf(ShopCatalog.TrophyIds,drop.Id));
                ReferencePicture("Parcel reward icon "+i,card,icon,43.5f,30,124,124);
                ReferenceText("parcel reward count "+i,card,UiCopy.Raw(drop.Amount.ToString()),17,152,177,43,38);
                cards.Add(card);
            }
            parcelClaimButton=ReferenceButton("Parcel claim",stage,120,1140,480,104,ClaimReferenceParcel);
            var amber=ReferencePicture("Parcel claim amber artwork",ReferenceButtonBody(parcelClaimButton),ParcelArt("AmberAction"),0,0,220,119,false);
            amber.gameObject.AddComponent<PrimaryButtonSkin>().Configure(ParcelArt("AmberAction"));
            parcelClaimCaption=ReferenceText("parcel claim caption",ReferenceButtonBody(parcelClaimButton),ParcelAwaitingVideo?ReferenceCopy("Later","Позже","Пізніше"):collected?ReferenceCopy("Done","Готово","Готово"):ReferenceCopy("Claim","Забрать","Забрати"),21,28,177,67,34);
            parcelBonusButton=ReferenceButton("Parcel open more",stage,120,1020,480,104,OpenReferenceParcelBonus,"GreenAction");
            ReferencePicture("Parcel video gift",ReferenceButtonBody(parcelBonusButton),ReferenceShopArt("InventoryReward"),17,18,120,87);
            parcelBonusCaption=ReferenceText("parcel bonus caption",ReferenceButtonBody(parcelBonusButton),ParcelVideoCaption,137,29,237,66,32,true);
            bool available=!proof&&!receipt.BonusClaimed&&ParcelVideoOfferAllowed;
            parcelBonusButton.gameObject.SetActive(available);
            if(!available){var r=(RectTransform)parcelClaimButton.transform;r.anchoredPosition=new Vector2(360,-1080);}
            parcelView=stage.gameObject.AddComponent<ParcelRevealView>();
            bool waiting=ParcelAwaitingVideo;bool skip=!waiting&&(parcelAnimationShown||collected);
            parcelView.Initialize(this,box,body,trim,opened,envelope,emptyEnvelope,light,cards.ToArray(),collected,skip,parcelBonusRound,()=>{
                parcelOpened=true;parcelAnimationShown=true;SetParcelButtons(true);
            });
            if(waiting)parcelView.HoldClosedForVideo();
            SetParcelButtons(skip||waiting);
        }
        List<ParcelDrop> LegacyParcelDrops(ParcelReceipt receipt)
        {
            if(receipt.Kind=="coins"||receipt.DuplicateCoins)return new List<ParcelDrop>{new ParcelDrop{Kind="coins",Amount=JourneyRewards.ParcelCoins(receipt)}};
            return new List<ParcelDrop>{new ParcelDrop{Kind=receipt.Kind=="tool"?"tool":"cosmetic",Id=receipt.GiftId,Amount=1}};
        }
        // Loading/no-fill must not erase the optional offer or look like it was
        // already consumed. Readiness only controls its caption and enabled state.
        bool ParcelVideoOfferAllowed=>monetization!=null&&!monetization.Ledger.HideVideoOffers
            &&RewardedOffersAllowed&&monetization.CanReceiveGrant&&!saveBlocked&&!HasSaveError&&!MechanicPopupOpen;
        UiCopy ParcelVideoCaption=>adProvider?.RewardReady==true
            ?ParcelAwaitingVideo?ReferenceCopy("Open with video","Открыть за видео","Відкрити за відео"):ReferenceCopy("More with video","Ещё за видео","Ще за відео")
            :adProvider?.RewardLoading==true?ReferenceCopy("Loading video…","Загрузка видео…","Завантаження…")
            :ReferenceCopy("Video unavailable","Видео недоступно","Відео недоступне");
        void RefreshParcelAdAvailability()
        {
            if(parcelView==null||parcelBonusButton==null||parcelClaimButton==null)return;
            var receipt=Rewards.Parcels.FirstOrDefault(p=>p.Order==parcelOrder);if(receipt==null)return;
            bool available=!parcelBonusRound&&!receipt.BonusClaimed
                &&!monetization.HasParcelBonus(MonetizationLedger.ParcelSubject(parcelOrder))&&ParcelVideoOfferAllowed;
            parcelBonusButton.gameObject.SetActive(available);
            var r=(RectTransform)parcelClaimButton.transform;r.anchoredPosition=new Vector2(available?152:360,r.anchoredPosition.y);
            SetParcelButtons(ParcelAwaitingVideo||!parcelView.Busy);
        }
        void SetParcelButtons(bool ready)
        {
            ready=ready&&CelebrationMotionAllowed&&adProvider?.Showing!=true;
            if(parcelBonusCaption!=null)parcelBonusCaption.SetCopy(ParcelVideoCaption);
            if(parcelClaimCaption!=null)parcelClaimCaption.color=new Color(.24f,.095f,.045f);
            if(parcelClaimButton!=null)parcelClaimButton.interactable=ready&&!saveBlocked&&!HasSaveError;
            if(parcelBonusButton!=null)parcelBonusButton.interactable=ready&&InventoryRewardAvailable;
        }
        void ParcelSaveFailed()
        {
            parcelStatus.SetCopy(ReferenceCopy("Could not save. Please try again.","Не удалось сохранить. Попробуй ещё раз.","Не вдалося зберегти. Спробуй ще раз."));SetParcelButtons(true);
        }
        void ClaimReferenceParcel()
        {
            if(saveBlocked||HasSaveError)return;
            if(ParcelAwaitingVideo){LeaveReferenceParcel();return;}
            if(parcelView==null||parcelView.Busy)return;
            var receipt=Rewards.Parcels.First(p=>p.Order==parcelOrder);
            bool collected=parcelBonusRound?receipt.BonusClaimed||parcelPreviewBonusClaimed:receipt.Claimed;
            if(collected){LeaveReferenceParcel();return;}
            bool committed=parcelBonusRound?TestMonetization?monetization.HasParcelBonus(MonetizationLedger.ParcelSubject(parcelOrder)):GameEconomy.TryClaimParcelBonus(parcelOrder,Rewards)
                :GameEconomy.TryClaimParcel(parcelOrder,Rewards);
            if(!committed){ParcelSaveFailed();return;}
            if(parcelBonusRound&&TestMonetization)parcelPreviewBonusClaimed=true;
            RefreshFieldTools();SetParcelButtons(false);
            parcelView.Collect(()=>{
                if(parcelBonusRound){LeaveReferenceParcel();return;}
                parcelClaimCaption.SetCopy(ReferenceCopy("Done","Готово","Готово"));SetParcelButtons(true);
            });
        }
        void OpenReferenceParcelBonus()
        {
            if(ParcelAwaitingVideo){WatchParcelOpen(parcelOrder);return;}
            if(parcelView==null||parcelView.Busy||!InventoryRewardAvailable)return;
            var receipt=Rewards.Parcels.First(p=>p.Order==parcelOrder);
            if(receipt.BonusClaimed||monetization.HasParcelBonus(MonetizationLedger.ParcelSubject(parcelOrder)))return;
            if(!receipt.Claimed){
                if(!GameEconomy.TryClaimParcel(parcelOrder,Rewards)){ParcelSaveFailed();return;}
                RefreshFieldTools();SetParcelButtons(false);
                parcelView.Collect(()=>{parcelClaimCaption.SetCopy(ReferenceCopy("Done","Готово","Готово"));SetParcelButtons(true);WatchParcelBonus(parcelOrder);});
            }else WatchParcelBonus(parcelOrder);
        }
        void ParcelOpenConfirmed(AdTicket ticket)
        {
            if(CurrentScreen!=GameScreen.Parcel||ticket.RewardSubject!=MonetizationLedger.ParcelSubject(parcelOrder)||!monetization.HasParcelOpen(ticket.RewardSubject))return;
            parcelBonusRound=false;parcelOpened=false;parcelAnimationShown=false;DrawMenu();
        }
        void ParcelBonusConfirmed(AdTicket ticket)
        {
            if(CurrentScreen!=GameScreen.Parcel||ticket.RewardSubject!=MonetizationLedger.ParcelSubject(parcelOrder))return;
            parcelBonusRound=true;parcelAnimationShown=false;parcelOpened=false;DrawMenu();
        }
        void LeaveReferenceParcel(){if(parcelReturnsToMap)FinishCompletion();else SetScreen(parcelReturnScreen);}
    }
}
