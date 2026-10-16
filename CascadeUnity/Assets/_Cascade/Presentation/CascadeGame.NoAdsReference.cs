using Cascade.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        string selectedNoAdsProduct=PurchaseCatalog.NoAdsYear;
        readonly System.Collections.Generic.Dictionary<string,Sprite> noAdsReferenceSprites=new System.Collections.Generic.Dictionary<string,Sprite>();
        Sprite NoAdsArt(string name)
        {
            if(!noAdsReferenceSprites.TryGetValue(name,out var sprite)||sprite==null){
                string path=name=="ExplanationPaper"?"CascadeArt/Navigation/StoryPaperStrip":"CascadeArt/NoAdsOct14/"+name;
                sprite=Resources.Load<Sprite>(path);if(sprite==null){var texture=Resources.Load<Texture2D>(path);if(texture==null)throw new System.InvalidOperationException("Missing subscription illustration: "+path);var region=name=="ExplanationPaper"?new Rect(41,169,2090,403):name=="CalendarBlank"?new Rect(74,64,1106,1164):new Rect(40,54,1178,1170);var sourceSize=name=="ExplanationPaper"?new Vector2(2172,724):new Vector2(1254,1254);region=new Rect(region.x*texture.width/sourceSize.x,region.y*texture.height/sourceSize.y,region.width*texture.width/sourceSize.x,region.height*texture.height/sourceSize.y);sprite=Sprite.Create(texture,region,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name="Runtime subscription illustration "+name;}
                noAdsReferenceSprites[name]=sprite;
            }
            return sprite;
        }
        void DrawReferenceNoAds()
        {
            var stage=ReferenceShopSurface();
            // The same 720x1280 composition as the supplied portrait reference.
            var title=ReferencePicture("No ads hanging title",stage,ReferenceShopArt("HeaderBoard"),128,79,488,158,false);
            title.type=Image.Type.Sliced;title.pixelsPerUnitMultiplier=4;
            var heading=ReferenceText("remove ads title",stage,ReferenceCopy("Remove ads","Убрать рекламу","Прибрати рекламу"),194,124,338,70,43);CentreHeading(title,heading);
            ReferenceButton("Back",stage,30,128,100,95,()=>SetScreen(noAdsReturnScreen),"BackButton");
            ReferencePicture("No ads illustrated emblem",stage,NoAdsArt("NoAdsEmblem"),222,259,276,285);
            ReferencePicture("No ads parchment explanation",stage,NoAdsArt("ExplanationPaper"),78,553,564,202,false);
            var explanation=ReferenceText("no ads explanation",stage,ReferenceCopy(
                "Removes automatic ads after levels. Rewarded videos remain optional.",
                "Убирает принудительную рекламу после окончания игры.\nНе убирает просмотр рекламы за вознаграждение.",
                "Прибирає примусову рекламу після завершення гри.\nРеклама за винагороду залишається."),
                128,587,464,125,27,false,true);
            explanation.alignment=TextAlignmentOptions.Center;
            string owned=purchaseStore?.ActiveSubscription;
            bool active=purchaseStore?.Owned==true;
            if(active&&owned!=null)selectedNoAdsProduct=owned;
            string[] products=PurchaseCatalog.NoAdsSubscriptions;
            int[] days={7,30,365};
            string[] english={"Week","Month","Year"},russian={"Неделя","Месяц","Год"},ukrainian={"Тиждень","Місяць","Рік"};
            for(int index=0;index<3;index++){
                int chosen=index;string product=products[index];float left=88+index*184;
                bool selected=selectedNoAdsProduct==product,disabled=active&&!selected;
                if(selected)ReferencePicture("Selected subscription border",stage,ReferenceShopArt("GreenAction"),left-9,743,188,193,false);
                var card=ReferencePicture("Subscription card "+days[index],stage,ReferenceShopArt("CoinCard"),left,754,168,176,false);
                if(disabled)card.color=new Color(.64f,.64f,.64f,1);
                var button=ReferenceButton("Subscription "+days[index],stage,left,754,168,176,()=>{if(!active){selectedNoAdsProduct=product;DrawMenu();}});
                button.interactable=!active;
                var body=ReferenceButtonBody(button);
                var calendar=ReferencePicture("Calendar "+days[index],body,NoAdsArt("CalendarBlank"),50,13,68,76);
                if(disabled)calendar.color=new Color(.63f,.63f,.63f,1);
                var date=ReferenceText("calendar day "+index,body,UiCopy.Raw(days[index].ToString()),63,47,43,35,28);
                date.alignment=TextAlignmentOptions.Center;
                ReferenceText("subscription duration "+index,body,UiCopy.Raw(UiLanguage=="ru"?russian[index]:UiLanguage=="uk"?ukrainian[index]:english[index]),12,88,144,29,25).alignment=TextAlignmentOptions.Center;
                var price=purchaseStore?.ProductPrice(product);
                var pricePanel=ReferencePicture("Subscription price plate",body,ReferenceShopArt(disabled?"GrayAction":"AmberAction"),11,117,146,52,false);
                pricePanel.type=Image.Type.Sliced;pricePanel.pixelsPerUnitMultiplier=4;
                var caption=active&&selected?ReferenceCopy("Owned","Куплено","Куплено"):UiCopy.Raw(TestMonetization?"—":string.IsNullOrWhiteSpace(price)?"—":price);
                ReferenceText("subscription price "+index,body,caption,19,123,130,39,24).alignment=TextAlignmentOptions.Center;
                if(disabled){var shade=body.gameObject.AddComponent<CanvasGroup>();shade.alpha=.58f;}
            }
            if(!active&&selectedNoAdsProduct==PurchaseCatalog.NoAdsYear&&purchaseStore?.AnnualSavingsPercent>0){
                var sale=ReferencePicture("Annual savings ribbon",stage,ReferenceShopArt("RedAction"),479,734,168,48,false);
                sale.type=Image.Type.Sliced;sale.pixelsPerUnitMultiplier=4;
                ReferenceText("Annual savings",stage,ReferenceCopy("Save "+purchaseStore.AnnualSavingsPercent+"%","Экономия "+purchaseStore.AnnualSavingsPercent+"%","Економія "+purchaseStore.AnnualSavingsPercent+"%"),486,744,154,29,20,true).alignment=TextAlignmentOptions.Center;
            }
            bool restore=active||purchaseStore?.LifetimeOwned==true;
            string selectedPrice=purchaseStore?.ProductPrice(selectedNoAdsProduct);
            var action=ReferenceButton(restore?"Restorepurchases":"Purchasequiet",stage,104,955,512,116,()=>{
                if(restore)purchaseStore?.Restore();
                else if(purchaseStore?.State==PurchaseState.Retry)purchaseStore.RetryConnection();
                else purchaseStore?.Buy(selectedNoAdsProduct);
            },restore?"AmberAction":"GreenAction");
            action.interactable=restore?purchaseStore?.CanRestore==true:purchaseStore?.CanBuyProduct(selectedNoAdsProduct)==true;
            string verb=selectedNoAdsProduct==PurchaseCatalog.NoAdsWeek?UiLanguage=="ru"?"на неделю":UiLanguage=="uk"?"на тиждень":"for a week":selectedNoAdsProduct==PurchaseCatalog.NoAdsMonth?UiLanguage=="ru"?"на месяц":UiLanguage=="uk"?"на місяць":"for a month":UiLanguage=="ru"?"на год":UiLanguage=="uk"?"на рік":"for a year";
            UiCopy actionCopy=restore?ReferenceCopy("Restore purchases","Восстановить покупки","Відновити покупки"):
                purchaseStore?.State==PurchaseState.Retry?ReferenceCopy("Try again","Повторить","Повторити"):
                UiCopy.Raw((UiLanguage=="ru"?"Убрать рекламу ":UiLanguage=="uk"?"Прибрати рекламу ":"Remove ads ")+verb+(string.IsNullOrWhiteSpace(selectedPrice)?"":" · "+selectedPrice));
            var actionLabel=ReferenceText("no ads main action",ReferenceButtonBody(action),actionCopy,24,17,464,82,35,true);actionLabel.fontSizeMin=19;CentreContent((RectTransform)ReferenceButtonBody(action),actionLabel.rectTransform);
            if(!restore){
                var plate=ReferencePicture("Subscription renewal board",stage,ReferenceShopArt("CoinCard"),104,1078,512,46,false);
                plate.gameObject.AddComponent<PrimaryButtonSkin>().Configure(ReferenceShopArt("CoinCard"));
                var legal=ReferenceText("subscription renewal terms",plate.transform,UiCopy.Raw((UiLanguage=="ru"?"Автопродление ":UiLanguage=="uk"?"Автоподовження ":"Auto-renews ")+verb+(string.IsNullOrWhiteSpace(selectedPrice)?"":" · "+selectedPrice)),14,4,484,38,22);
                legal.fontSizeMin=18;CentreHeading(plate,legal);
            }
            var secondary=ReferenceButton(restore?"Manage subscription":"Restorepurchases",stage,184,1130,352,64,()=>{
                if(restore){
#if UNITY_ANDROID
                    UnityEngine.Application.OpenURL("https://play.google.com/store/account/subscriptions?package="+UnityEngine.Application.identifier);
#elif UNITY_IOS
                    UnityEngine.Application.OpenURL("https://apps.apple.com/account/subscriptions");
#endif
                }else purchaseStore?.Restore();
            },"GreenAction");
            secondary.interactable=restore?purchaseStore?.LifetimeOwned!=true:purchaseStore?.CanRestore==true;
            var secondaryLabel=ReferenceText("subscription secondary action",ReferenceButtonBody(secondary),restore?ReferenceCopy("Manage subscription","Управлять подпиской","Керувати підпискою"):ReferenceCopy("Restore purchases","Восстановить покупки","Відновити покупки"),12,5,328,54,24,true);
            CentreContent((RectTransform)ReferenceButtonBody(secondary),secondaryLabel.rectTransform);
            // Enlarge the gap only when the phone's 44px touch targets need it.
            // Shorter 16:9 viewports retain the original in-bounds footer position.
            float pixelScale=stage.localScale.x*stage.GetComponentInParent<Canvas>().scaleFactor;
            float minimum=44/Mathf.Max(.001f,pixelScale);
            float footerTop=1214+Mathf.Max(0,(Mathf.Max(64,minimum)+Mathf.Max(62,minimum))*.5f-83+2/Mathf.Max(.001f,pixelScale));
            var terms=ReferenceButton("Subscription terms",stage,104,footerTop,242,62,()=>OpenLegalDocument("terms"),"AmberAction");
            var termsLabel=ReferenceText("subscription terms label",ReferenceButtonBody(terms),ReferenceCopy("Terms","Условия","Умови"),12,6,218,50,24);
            CentreContent((RectTransform)ReferenceButtonBody(terms),termsLabel.rectTransform);
            var privacy=ReferenceButton("Subscription privacy",stage,374,footerTop,242,62,()=>OpenLegalDocument("privacy"),"AmberAction");
            var privacyLabel=ReferenceText("subscription privacy label",ReferenceButtonBody(privacy),ReferenceCopy("Privacy","Политика","Приватність"),12,6,218,50,24);
            CentreContent((RectTransform)ReferenceButtonBody(privacy),privacyLabel.rectTransform);
        }
    }
}
