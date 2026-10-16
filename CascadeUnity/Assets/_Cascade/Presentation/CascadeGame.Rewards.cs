using System;
using System.Collections;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using Cascade.Solver;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int giftPage,workshopPage,recordPage,contractPage,parcelOrder;
        bool parcelOpened,parcelAnimationShown;
        GameScreen collectionReturn=GameScreen.TravelBag,workshopReturn=GameScreen.TravelBag;
        LevelContent[] contracts;
        static readonly string[] GiftNames={"Leaf pin","Sun button","Sky ribbon","Moon patch","Wind chime","Rain badge","Copper acorn","Beacon brooch","Sky pilot","Moth mask","Garden festival","Cloud gardener","Explorer hat","Frog suit","Star knight","Fox mask","Mushroom hat","Bee costume","Winter hood","Sky pirate","Cloud jester","Star mage","Paper crane","Little comet","Dew bell","Night flower","Cloud kite","Fern compass","Amber moth","Moon boat"};
        static readonly string[] GiftAssets={"LeafPin","SunButton","SkyRibbon","MoonPatch"};
        static readonly string[] WorkshopNames={"Bridge lantern","Rain pump","Post glider","Beacon model"};
        static readonly string[] WorkshopAssets={"BridgeLantern","RainPump","PostGlider","BeaconModel"};
        static readonly string[] RankNames={"Apprentice","Garden friend","Mill keeper","Pathfinder","Beacon keeper"};
        ParcelReceipt WaitingParcel=>Rewards.Parcels.FirstOrDefault(p=>!p.Claimed||!p.BonusClaimed&&GameEconomy?.HasParcelBonus(MonetizationLedger.ParcelSubject(p.Order))==true);
        string[] OrderedNodes=>Catalog.PrimaryLevels.Select(c=>c.Definition.CampaignNodeId).ToArray();
        GameScreen bagReturnScreen=GameScreen.Home;
        ShopLocation bagShopLocation,collectionShopLocation,workshopShopLocation;
        GameScreen parcelReturnScreen=GameScreen.Journal;
        public void ShowTravelBag()
        {
            if(CurrentScreen==GameScreen.Shop&&shopReturnScreen==GameScreen.TravelBag){BackFromShop();return;}
            if(CurrentScreen==GameScreen.Home||CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.JourneyMap||CurrentScreen==GameScreen.JourneyEnd){
                bagReturnScreen=CurrentScreen==GameScreen.Shop?GameScreen.Home:CurrentScreen;
                if(CurrentScreen==GameScreen.Shop)bagShopLocation=CaptureShop();
            }
            SetScreen(GameScreen.TravelBag);
        }
        void BackFromBag()
        {
            if(bagReturnScreen==GameScreen.Shop){RestoreShop(bagShopLocation);return;}
            SetScreen(bagReturnScreen==GameScreen.TravelBag?GameScreen.Home:bagReturnScreen);
        }
        public void ShowJournal()=>ShowTravelBag();
        public void ShowCollection()
        {
            if(CurrentScreen==GameScreen.Shop&&shopReturnScreen==GameScreen.Collection){BackFromShop();return;}
            if(CurrentScreen!=GameScreen.Collection){
                collectionReturn=CurrentScreen;
                if(CurrentScreen==GameScreen.Shop)collectionShopLocation=CaptureShop();
                giftPage=Math.Max(0,Array.IndexOf(JourneyRewards.Gifts,Rewards.SelectedGift));
            }
            shopDetailOpen=false;SetScreen(GameScreen.Collection);
        }
        void BackFromCollection()
        {
            if(collectionReturn==GameScreen.Shop){RestoreShop(collectionShopLocation);return;}
            SetScreen(collectionReturn);
        }
        public void ShowWorkshop()
        {
            ShowShop();
        }
        void BackFromWorkshop()
        {
            if(workshopReturn==GameScreen.Shop){RestoreShop(workshopShopLocation);return;}
            SetScreen(workshopReturn);
        }
        public void ShowRequests(){ShowJourney();}
        public void ShowRecords()=>ShowTravelBag();
        public void ShowContracts()=>ShowJourney();
        public void ShowParcel()
            =>OpenParcel(false);
        void OpenParcel(bool returnToMap)
        {
            var receipt=returnToMap&&profile.Completion?.ParcelOrder>0?Rewards.Parcels.FirstOrDefault(p=>p.Order==profile.Completion.ParcelOrder):WaitingParcel;
            if(receipt==null){if(returnToMap)FinishCompletion();return;}
            parcelReturnsToMap=returnToMap;
            if(!returnToMap&&CurrentScreen!=GameScreen.Parcel)parcelReturnScreen=CurrentScreen;
            parcelOrder=receipt.Order;parcelOpened=false;parcelAnimationShown=false;parcelBonusRound=false;parcelPreviewBonusClaimed=false;SetScreen(GameScreen.Parcel);
        }
        void DrawTravelBag()
        {
            DrawCraftBag();
        }
        void MetaArrows(Action previousAction,Action nextAction)
        {
            var previous=DirectionButton("Previous",menuRoot,new Vector2(.08f,0),new Vector2(.26f,0),false,previousAction);
            var next=DirectionButton("Next",menuRoot,new Vector2(.74f,0),new Vector2(.92f,0),true,nextAction);
            foreach(var button in new[]{previous,next}){var rect=(RectTransform)button.transform;menuActions.Add(rect);menuBottoms.Add(new MenuBottom{rect=rect,bottom=16});button.GetComponent<UnityEngine.UI.Image>().color=Color.clear;}
        }
        void RecordArrows(Action previousAction,Action nextAction)
        {
            var ribbon=menuRoot.Find("Menu description papyrus") as RectTransform;
            if(ribbon!=null){
                ribbon.anchorMin=new Vector2(.15f,ribbon.anchorMin.y);
                ribbon.anchorMax=new Vector2(.85f,ribbon.anchorMax.y);
            }
            void Add(string name,float x,bool right,Action action){
                var button=DirectionButton(name,menuRoot,new Vector2(x,.855f),new Vector2(x,.855f),right,action);
                var rect=(RectTransform)button.transform;rect.sizeDelta=Vector2.one*48*MenuUnit;
                menuSquares.Add(new System.Collections.Generic.KeyValuePair<RectTransform,float>(rect,48));
                var shadow=Rect(name+" outline",rect,new Vector2(.14f,.22f),new Vector2(.86f,.78f)).gameObject.AddComponent<ChevronGraphic>();
                shadow.PointsRight=right;shadow.Weight=0;shadow.color=new Color(.07f,.16f,.16f,.95f);
                shadow.raycastTarget=false;shadow.transform.SetAsFirstSibling();
                var arrow=rect.Find("Navigation arrow") as RectTransform;
                if(arrow!=null){arrow.anchorMin=new Vector2(.19f,.27f);arrow.anchorMax=new Vector2(.81f,.73f);}
            }
            Add("Previous record",.245f,false,previousAction);
            Add("Next record",.755f,true,nextAction);
        }
        UnityEngine.UI.Image RewardPicture(string asset,Vector2 min,Vector2 max)
            =>Picture(asset,menuRoot,min,max,Resources.Load<Sprite>("CascadeArt/Workshop/"+asset),true);
        void DrawCollection(){shopItem=giftPage;DrawTravelCounter(false,true);}
        void DrawParcel()=>DrawReferenceParcel();
        void ShowCollectedTrophy(string id)
        {
            if(!OwnsGift(id))return;
            ShowCollection();shopItem=giftPage=Array.IndexOf(ShopCatalog.TrophyIds,id);shopDetailOpen=true;DrawMenu();
        }
        IEnumerator GiftArrival(RectTransform picture,UnityEngine.UI.Image parcel)
        {
            var rest=picture.anchoredPosition;
            var opened=parcel.sprite;parcel.sprite=Resources.Load<Sprite>("CascadeArt/Workshop/Parcel");var gift=picture.GetComponent<UnityEngine.UI.Image>();gift.color=new Color(1,1,1,0);
            bool openedSound=false,revealSound=false;
            // A parcel should have a visible breath before it opens. This only
            // presents an already-earned receipt; opening state remains owned by
            // the profile before this coroutine begins.
            const float anticipateEnd=.86f,openAt=.94f,revealStart=1.10f,revealDuration=1.34f,duration=3.25f;
            for(float t=0;t<duration;){if(picture==null||parcel==null)yield break;
                if(ReducedMotion)break;
                if(!CelebrationMotionAllowed){yield return null;continue;}
                float prepare=Mathf.Clamp01(t/anticipateEnd),pulse=Mathf.Sin(prepare*Mathf.PI*3f)*(1-prepare*.35f);
                parcel.rectTransform.localScale=new Vector3(1+.018f*pulse,1-.014f*pulse,1);
                parcel.rectTransform.localRotation=Quaternion.Euler(0,0,t<openAt?Mathf.Sin(prepare*Mathf.PI*3f)*(1.2f+prepare*2.5f):0);
                float opening=Mathf.Clamp01((t-openAt)/.32f);
                if(t>=openAt){parcel.sprite=opened;parcel.rectTransform.localScale=new Vector3(1+.085f*Mathf.Sin(opening*Mathf.PI),1-.052f*Mathf.Sin(opening*Mathf.PI),1);if(!openedSound){openedSound=true;feedback.Sound(ToyCue.Stamp);}}
                float p=Mathf.Clamp01((t-revealStart)/revealDuration),back=1+2.70158f*Mathf.Pow(p-1,3)+1.70158f*Mathf.Pow(p-1,2);
                float alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01(p/.26f));gift.color=new Color(1,1,1,alpha);
                picture.localScale=Vector3.one*(Mathf.Lerp(.16f,1,back)+.11f*Mathf.Sin(p*Mathf.PI));
                picture.anchoredPosition=rest+Vector2.down*(1-back)*118+Vector2.up*Mathf.Sin(p*Mathf.PI)*28;
                picture.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(-14,0,back)+Mathf.Sin(p*Mathf.PI*2)*(1-p)*4);
                if(p>.30f&&!revealSound){revealSound=true;feedback.Sound(ToyCue.Success);feedback.Haptic(ToyHapticMoment.Parcel);}
                t+=Mathf.Min(.05f,Time.unscaledDeltaTime);yield return null;}
            if(picture!=null){picture.anchoredPosition=rest;picture.localRotation=Quaternion.identity;}
            if(parcel!=null){parcel.sprite=opened;parcel.rectTransform.localScale=Vector3.one;parcel.rectTransform.localRotation=Quaternion.identity;}
            if(picture!=null){picture.localScale=Vector3.one;gift.color=Color.white;}
        }
        static int StampGoal(LevelDefinition level)=>level.Floors.Select((f,i)=>f.OptionalStamp==null?0:1<<i).Aggregate(0,(a,b)=>a|b);
        LevelContent[] StampTrails()
        {
            if(contracts==null)contracts=Catalog.PrimaryLevels.Where(c=>StampGoal(c.Definition)!=0&&
                PuzzleSolver.FindShortest(new PuzzleResolver(c.Definition),goal:new PuzzleGoal(StampGoal(c.Definition))).Status==SearchStatus.Solved).ToArray();
            return contracts;
        }
        public void StartStampTrail(string campaignNode)
        {
            var content=StampTrails().Single(c=>c.Definition.CampaignNodeId==campaignNode);
            if(PreferredMode!=PuzzleMode.Shapes&&Catalog.TryGetLocalizedBinding(campaignNode,Preferences.PuzzleLocale,out var localized))content=localized;
            StartExactTrail(content,StampGoal(content.Definition));
        }
        void StartExactTrail(LevelContent content,int stampGoal)
        {
            if(HasResumableJourneyAttempt)return;
            var goal=new PuzzleGoal(stampGoal);
            if(PuzzleSolver.FindShortest(new PuzzleResolver(content.Definition),goal:goal).Status!=SearchStatus.Solved)return;
            MountAttempt(new AttemptService(content.Definition,content.Locale,goal));
            levelLabel.SetCopy(new UiCopy("Stamp trail {0}",DisplayLevelNumber(content.Definition.CampaignNodeId)));PersistProgress();ShowGame();
        }
        void RecordMetric(string name,RectTransform parent,Vector2 min,Vector2 max,Sprite icon,string value)
        {
            var group=Rect(name,parent,min,max);
            Picture(name+" icon",group,new Vector2(.02f,.07f),new Vector2(.40f,.93f),icon,true);
            var number=Text(name+" value",group,new Vector2(.43f,.07f),new Vector2(.99f,.93f),26,UiCopy.Raw("× "+value));
            number.alignment=TextAlignmentOptions.MidlineLeft;number.margin=Vector4.zero;
            number.textWrappingMode=TextWrappingModes.NoWrap;StyleCelebrationType(number,CelebrationType.SkyNumber);
        }
    }
}
