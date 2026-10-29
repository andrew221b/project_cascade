using System;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        void DrawReferenceInventory()
        {
            var stage=ReferenceShopSurface();machine.SetZone(EnvironmentWorld);
            var farWindow=ReferenceRect("Inventory distant moving horizon",stage,53,735,627,355);farWindow.gameObject.AddComponent<RectMask2D>();
            var farRect=Rect("Inventory distant island drift",farWindow,Vector2.zero,Vector2.one);farRect.offsetMin=new Vector2(-6,-6);farRect.offsetMax=new Vector2(6,6);
            var far=farRect.gameObject.AddComponent<RawImage>();far.texture=ReferenceShopArt("Backdrop").texture;far.uvRect=new Rect(53/720f,190/1280f,627/720f,355/1280f);far.raycastTarget=false;
            farRect.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(1.2f,2.8f),1.1f);
            stage.GetComponent<ReferenceShopLayout>().BindContinuousGround();
            InventoryIsland(stage,33,415,105,135,0);
            InventoryIsland(stage,572,357,109,155,1);
            stage.Find("Natural menu foliage").SetAsLastSibling();
            ReferencePicture("Inventory Mika platform",stage,ReferenceShopArt("PreviewPlatform"),111,689,505,62);
            var hero=ReferencePicture("Inventory Mika",stage,ReferenceShopArt("InventoryMika")??HomeArt("Mika"),134,188,416,536);
            AttachMikaCosmetic(hero,null);
            hero.GetComponent<MikaCosmeticView>().SetBadgeAnchor(new Vector2(.705f,.55f),1.15f);
            var board=ReferencePicture("Inventory header board",stage,ReferenceShopArt("HeaderBoard"),165,78,406,123,false);
            board.type=Image.Type.Sliced;board.pixelsPerUnitMultiplier=4;
            var title=ReferenceText("inventory heading",stage,ReferenceCopy("My things","Мои вещи","Мої речі"),193,105,350,69,48);CentreHeading(board,title);
            ReferenceButton("Back",stage,51,116,84,84,BackFromBag,"BackButton");
            InventoryFact(stage,"Inventory lights",40,724,ToolArt(3),ReferenceCopy("My lights","Мои огоньки","Мої вогники"),JourneyClearCount.ToString(),OpenInventoryMap);
            long stock=Enumerable.Range(0,ShopCatalog.ToolCount).Sum(i=>(long)Stock((TravelTool)i));
            InventoryFact(stage,"Inventory items",366,724,ReferenceShopArt("InventoryItems"),ReferenceCopy("My items","Мои предметы","Мої предмети"),stock.ToString(),ShowSupplies);
            InventoryFact(stage,"Inventory balance",40,878,ReferenceShopCoin(),ReferenceCopy("My balance","Мой баланс","Мій баланс"),ShopCoins.ToString(),ShowCoinPacks);
            InventoryFact(stage,"Inventory trophies",366,878,TrophyArt(0),ReferenceCopy("My trophies","Мои трофеи","Мої трофеї"),ShopCatalog.TrophyIds.Count(OwnsGift)+" / "+ShopCatalog.TrophyIds.Length,ShowCollection);
            bool rewardAvailable=InventoryRewardAvailable;
            var reward=ReferenceButton("Inventory claim reward",stage,102,1036,516,147,WatchCoinVideo,rewardAvailable?"GreenAction":"GrayAction");
            var rewardIcon=ReferencePicture("Inventory reward gift and video",ReferenceButtonBody(reward),ReferenceShopArt("InventoryReward"),26,21,126,96);
            var rewardCaption=ReferenceText("inventory reward caption",ReferenceButtonBody(reward),ReferenceCopy("Claim reward","Забрать награду","Забрати нагороду"),178,26,306,94,34,true);CentreContent((RectTransform)ReferenceButtonBody(reward),rewardIcon.rectTransform,rewardCaption.rectTransform);
            reward.interactable=rewardAvailable;
        }
        void InventoryIsland(RectTransform stage,float x,float y,float width,float height,int index)
        {
            var island=ReferencePicture("Drifting home island "+index,stage,WorldIslandArt.Get(EnvironmentWorld,index),x,y,width,height);
            island.color=Color.white;
            island.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(index%2==0?3:-3,4+index%2),index*.83f);
        }
        void OpenInventoryMap()
        {
            // Looking at collected lights opens the map itself; stories retain their play entry.
            islandBrowseIndex=-1;selectedJourneyStop=HasResumableJourneyAttempt?Math.Max(0,Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node)):NextJourneyStop();
            mapPage=JourneyStory.Zone(selectedJourneyStop);SetScreen(GameScreen.JourneyMap);
        }
        bool InventoryRewardAvailable=>adProvider?.RewardReady==true&&monetization!=null&&!monetization.Ledger.HideVideoOffers
            &&RewardedOffersAllowed&&monetization.CanReceiveGrant&&!saveBlocked&&!HasSaveError&&!MechanicPopupOpen;
        void InventoryFact(RectTransform stage,string name,float x,float y,Sprite icon,UiCopy title,string count,Action action)
        {
            var button=ReferenceButton(name,stage,x,y,314,135,action);
            var body=ReferenceButtonBody(button);
            var face=ReferencePicture(name+" clean panel",body,ReferenceShopArt("InventoryStatCard"),-8,-10,330,155,false);
            face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=4;
            var emblem=ReferencePicture(name+" icon",body,icon,25,21,86,86);
            var column=ReferenceRect(name+" text column",body,134,20,164,96);
            ReferenceText(name+" title",column,title,0,0,164,34,24).alignment=TMPro.TextAlignmentOptions.Center;
            ReferenceText(name+" count",column,UiCopy.Raw(count),0,38,164,58,51).alignment=TMPro.TextAlignmentOptions.Center;
            CentreInfoColumn(column);CentreContent((RectTransform)body,emblem.rectTransform,column);
        }
        void AddMenuLeaves(RectTransform parent,bool reference=false)
        {
            RectTransform layer;
            if(reference)layer=ReferenceRect("Natural menu foliage",parent,53,42,627,1115);
            else layer=Rect("Natural menu foliage",parent,new Vector2(.055f,.10f),new Vector2(.945f,.93f));
            layer.gameObject.AddComponent<RectMask2D>();layer.gameObject.AddComponent<MenuLeafDrift>().Initialize(this,ReferenceShopArt("MenuGreenLeaf"),CurrentScreen==GameScreen.Supplies||CurrentScreen==GameScreen.Collection||CurrentScreen==GameScreen.Pause||ReferenceMapScreen,CurrentScreen==GameScreen.Pause);
        }
    }
}
