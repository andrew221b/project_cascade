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
            if(noAdsReferenceSprites.TryGetValue(name,out var cached)&&cached!=null)return cached;
            var texture=Resources.Load<Texture2D>("CascadeArt/NoAdsReferenceOct29/"+name);
            if(texture==null)throw new System.InvalidOperationException("Missing subscription illustration: "+name);
            Rect region=new Rect(0,0,texture.width,texture.height);
            if(noAdsArtBounds.TryGetValue(name,out var bounds))region=new Rect(bounds.x*texture.width,bounds.y*texture.height,bounds.width*texture.width,bounds.height*texture.height);
            var sprite=Sprite.Create(texture,new Rect(region.x,texture.height-region.y-region.height,region.width,region.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            sprite.name="Runtime subscription illustration "+name;noAdsReferenceSprites[name]=sprite;return sprite;
        }
        // Normalized top-left alpha bounds, recorded from the selected production artwork.
        static readonly System.Collections.Generic.Dictionary<string,Rect> noAdsArtBounds=new System.Collections.Generic.Dictionary<string,Rect>{
            {"Emblem",new Rect(0.11722488f,0.03508772f,0.79425837f,0.88995215f)},
            {"Scroll",new Rect(0.05110497f,0.07458564f,0.89779006f,0.81906077f)},
            {"Card",new Rect(0.05502183f,0.05458515f,0.89082969f,0.90465793f)},
            {"SelectedCard",new Rect(0.08921645f,0.01639344f,0.83397983f,0.96229508f)},
            {"DisabledCard",new Rect(0.05267630f,0.05613772f,0.89464741f,0.90943114f)},
            {"Calendar",new Rect(0.07867821f,0.08400646f,0.84107002f,0.85945073f)},
            {"Ribbon",new Rect(0.03176796f,0.08011050f,0.93646409f,0.86187845f)},
            {"OwnedBadge",new Rect(0.04541016f,0.13671875f,0.90917969f,0.73567708f)},
            {"Header",new Rect(0.05168986f,0.00000000f,0.90059642f,1.00000000f)},
            {"Green",new Rect(0.00092166f,0.15172414f,0.99815668f,0.67724138f)},
            {"Gold",new Rect(0.00000000f,0.18620690f,1.00000000f,0.66620690f)},
        };
        RectTransform NoAdsSurface()
        {
            LoadNavigationArt();catalogueInfo=null;catalogueStack.Clear();displayedShopObject=null;
            var frame=Rect("Subscription reference environment",menuRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            var image=frame.gameObject.AddComponent<Image>();image.sprite=NoAdsArt("Backdrop");image.raycastTarget=false;
            var stage=Rect("Subscription reference composition",menuRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            stage.sizeDelta=new Vector2(720,1280);stage.gameObject.AddComponent<ReferenceShopLayout>().Initialize(menuRoot,frame);return stage;
        }
        TextMeshProUGUI NoAdsLabel(string name,Transform parent,UiCopy copy,float x,float y,float width,float height,float size,bool light=false,bool wrap=false)
        {
            var label=ReferenceText(name,parent,copy,x,y,width,height,size,light,wrap);
            label.color=light?MenuPaper:new Color(.13f,.075f,.035f);label.alignment=TextAlignmentOptions.Center;return label;
        }
        void DrawReferenceNoAds()
        {
            var stage=NoAdsSurface();
            ReferencePicture("No ads hanging title",stage,NoAdsArt("Header"),126,60,480,190,false);
            var titleField=ReferenceRect("Subscription title writing field",stage,202,138,348,70);
            var title=NoAdsLabel("remove ads title",titleField,ReferenceCopy("Remove ads","Убрать рекламу","Прибрати рекламу"),0,0,348,70,38);
            CentreContent(titleField,title.rectTransform);
            ReferenceButton("Back",stage,30,154,100,95,()=>SetScreen(noAdsReturnScreen),"BackButton");
            ReferencePicture("No ads illustrated emblem",stage,NoAdsArt("Emblem"),239.5f,262,251,264,false);
            ReferencePicture("No ads parchment explanation",stage,NoAdsArt("Scroll"),80,532,560,200,false);
            var explanation=NoAdsLabel("no ads explanation",stage,ReferenceCopy(
                "Removes automatic ads after levels.\nRewarded videos remain optional.",
                "Убирает принудительную рекламу\nпосле окончания игры.\nНе убирает просмотр рекламы\nза вознаграждение.",
                "Прибирає примусову рекламу\nпісля завершення гри.\nНе прибирає рекламу\nза винагороду."),126,573,468,122,26,false,true);
            explanation.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceBody")??art.bodyFont;explanation.fontStyle=FontStyles.Bold;
            string owned=purchaseStore?.ActiveSubscription;bool active=purchaseStore?.Owned==true;
            if(active&&owned!=null)selectedNoAdsProduct=owned;
            string[] products=PurchaseCatalog.NoAdsSubscriptions;int[] days={7,30,365};
            string[] en={"Week","Month","Year"},ru={"Неделя","Месяц","Год"},uk={"Тиждень","Місяць","Рік"};
            for(int index=0;index<3;index++){
                string product=products[index];float left=90+index*180;bool selected=selectedNoAdsProduct==product,disabled=active&&!selected;
                ReferencePicture("Subscription card "+days[index],stage,NoAdsArt(selected?"SelectedCard":disabled?"DisabledCard":"Card"),left-(selected?15:0),selected?734:750,selected?204:168,selected?204:176,false);
                var choice=ReferenceButton("Subscription "+days[index],stage,left,750,168,176,()=>{if(!active){selectedNoAdsProduct=product;DrawMenu();}});choice.interactable=!active;
                var body=ReferenceButtonBody(choice);
                var calendar=ReferencePicture("Calendar "+days[index],body,NoAdsArt("Calendar"),52,10,64,72,false);if(disabled)calendar.material=OwnedTrophyMaterial();
                var dateField=ReferenceRect("Subscription calendar writing field",body,56,38,56,40);
                var date=NoAdsLabel("calendar day "+index,dateField,UiCopy.Raw(days[index].ToString()),0,0,56,40,28);
                var dateRow=CentreContent(dateField,date.rectTransform);dateRow.Padding=2;dateRow.Refresh();
                var durationField=ReferenceRect("Subscription duration writing field",body,12,96,144,36);
                var duration=NoAdsLabel("subscription duration "+index,durationField,UiCopy.Raw(UiLanguage=="ru"?ru[index]:UiLanguage=="uk"?uk[index]:en[index]),0,0,144,36,28);
                var durationRow=CentreContent(durationField,duration.rectTransform);durationRow.Padding=2;durationRow.Refresh();
                bool purchased=active&&selected;
                if(purchased)ReferencePicture("Subscription owned green badge",body,NoAdsArt("OwnedBadge"),13,134,143,38,false);
                var price=purchaseStore?.ProductPrice(product);
                var priceCopy=purchased?ReferenceCopy("Owned","Куплен","Придбано"):UiCopy.Raw(string.IsNullOrWhiteSpace(price)?"—":price);
                if(purchased){
                    var field=ReferenceRect("Subscription owned writing field",body,18,135,134,35);
                    var check=ReferenceRect("Subscription owned check",field,0,0,23,23).gameObject.AddComponent<SubscriptionGlyphGraphic>();check.Symbol=SubscriptionGlyphGraphic.Shape.Check;check.color=MenuPaper;check.raycastTarget=false;
                    var label=NoAdsLabel("subscription price "+index,field,priceCopy,0,0,106,35,25,true);var row=CentreContent(field,check.rectTransform,label.rectTransform);row.Padding=2;row.Refresh();
                }else{
                    var priceField=ReferenceRect("Subscription price writing field",body,18,131,134,40);
                    var label=NoAdsLabel("subscription price "+index,priceField,priceCopy,0,0,134,40,26);var row=CentreContent(priceField,label.rectTransform);row.Padding=2;row.Refresh();
                }
            }
            if(!active&&selectedNoAdsProduct==PurchaseCatalog.NoAdsYear&&purchaseStore?.AnnualSavingsPercent>0){
                var ribbon=ReferencePicture("Annual savings ribbon",stage,NoAdsArt("Ribbon"),476,730,178,60,false);ribbon.rectTransform.localRotation=Quaternion.Euler(0,0,-9);
                var text=NoAdsLabel("Annual savings",stage,ReferenceCopy("Save "+purchaseStore.AnnualSavingsPercent+"%","Экономия "+purchaseStore.AnnualSavingsPercent+"%","Економія "+purchaseStore.AnnualSavingsPercent+"%"),488,738,149,31,18,true);text.rectTransform.localRotation=Quaternion.Euler(0,0,-9);
            }
            bool ownedState=active||purchaseStore?.LifetimeOwned==true;
            if(ownedState){
                NoAdsFramedAction(stage,"Manage subscription",ReferenceCopy("Manage subscription","Управление подпиской","Керування підпискою"),85,962,550,100,false,()=>{
#if UNITY_ANDROID
                    UnityEngine.Application.OpenURL("https://play.google.com/store/account/subscriptions?package="+UnityEngine.Application.identifier);
#elif UNITY_IOS
                    UnityEngine.Application.OpenURL("https://apps.apple.com/account/subscriptions");
#endif
                },purchaseStore?.LifetimeOwned!=true,SubscriptionGlyphGraphic.Shape.Manage);

            }else{
                string period=selectedNoAdsProduct==PurchaseCatalog.NoAdsWeek?UiLanguage=="ru"?"на неделю":UiLanguage=="uk"?"на тиждень":"for a week":selectedNoAdsProduct==PurchaseCatalog.NoAdsMonth?UiLanguage=="ru"?"на месяц":UiLanguage=="uk"?"на місяць":"for a month":UiLanguage=="ru"?"на год":UiLanguage=="uk"?"на рік":"for a year";
                string price=purchaseStore?.ProductPrice(selectedNoAdsProduct);
                string copy=(UiLanguage=="ru"?"Убрать рекламу":UiLanguage=="uk"?"Прибрати рекламу":"Remove ads")+"\n"+period+(string.IsNullOrWhiteSpace(price)?"":" "+(UiLanguage=="ru"?"за ":UiLanguage=="uk"?"за ":"· ")+price);
                NoAdsFramedAction(stage,"Purchasequiet",UiCopy.Raw(copy),85,946,550,116,false,()=>RequestStorePurchase(selectedNoAdsProduct),purchaseStore?.CanBuyProduct(selectedNoAdsProduct)==true);
            }
            // Both entitlement states use the same full restore action and placement.
            NoAdsFramedAction(stage,"Restorepurchases",ReferenceCopy("Restore purchases","Восстановить покупки","Відновити покупки"),85,1070,550,100,true,()=>purchaseStore?.Restore(),purchaseStore?.CanRestore==true,SubscriptionGlyphGraphic.Shape.Restore);
            SubscriptionLegalLink(stage,"Subscription terms",ReferenceCopy("Terms","Условия","Умови"),ownedState?120:150,1174,ownedState?240:180);
            SubscriptionLegalLink(stage,"Subscription privacy",ReferenceCopy("Privacy","Политика","Приватність"),ownedState?360:390,1174,ownedState?240:180);
            if(!ownedState||active&&owned!=null){
                var details=ReferenceRect("Subscription billing details",stage,55,1238,610,42);
                var billing=selectedNoAdsProduct==PurchaseCatalog.NoAdsWeek?ReferenceCopy("Billed weekly until cancelled","Списания каждую неделю до отмены","Списання щотижня до скасування"):
                    selectedNoAdsProduct==PurchaseCatalog.NoAdsMonth?ReferenceCopy("Billed monthly until cancelled","Списания каждый месяц до отмены","Списання щомісяця до скасування"):ReferenceCopy("Billed yearly until cancelled","Списания каждый год до отмены","Списання щороку до скасування");
                var label=NoAdsLabel("subscription billing terms",details,billing,4,0,602,42,21,true);SubscriptionTextShadow(label);var fit=CentreContent(details,label.rectTransform);fit.Padding=2;fit.Refresh();
            }
        }
        Button NoAdsFramedAction(RectTransform stage,string name,UiCopy copy,float x,float y,float width,float height,bool gold,System.Action clicked,bool available,SubscriptionGlyphGraphic.Shape? icon=null)
        {
            var button=ReferenceButton(name,stage,x,y,width,height,clicked);var body=ReferenceButtonBody(button);
            var face=ReferencePicture("Subscription action artwork",body,ReferenceShopArt(gold?"AmberAction":"GreenAction"),0,0,width,height,false);
            button.targetGraphic=face;button.transition=Selectable.Transition.ColorTint;var colors=button.colors;colors.disabledColor=new Color(.75f,.78f,.74f);colors.pressedColor=new Color(.9f,.96f,.88f);button.colors=colors;button.interactable=available;
            face.gameObject.AddComponent<PrimaryButtonSkin>().Configure(face.sprite);
            face.gameObject.AddComponent<AttachedPlants>().Initialize(this,EnvironmentWorld,false);
            // The shared skin exposes its measured inner writing field.
            float inset=2*CenteredContentRow.Unit((RectTransform)body);var writing=CenteredContentRow.ReadingRect(face.rectTransform);
            var field=ReferenceRect("Subscription action writing field",body,writing.xMin-face.rectTransform.rect.xMin+inset,face.rectTransform.rect.yMax-writing.yMax+inset,writing.width-2*inset,writing.height-2*inset);
            if(!icon.HasValue){
                var lines=copy.English.Split(new[]{'\n'},2);var first=NoAdsLabel("no ads main action",field,UiCopy.Raw(lines[0]),0,0,field.rect.width,field.rect.height,33,true);
                if(lines.Length==2){
                    var second=NoAdsLabel("no ads purchase period",field,UiCopy.Raw(lines[1]),0,0,field.rect.width,field.rect.height,33,true);
                    CentreStack(field,first.rectTransform,second.rectTransform);var fit=field.GetComponent<CenteredContentRow>();fit.Padding=2;fit.Refresh();
                }else{var fit=CentreContent(field,first.rectTransform);fit.Padding=2;fit.Refresh();}
            }else{
                var label=NoAdsLabel("subscription "+name,field,copy,0,0,field.rect.width,field.rect.height,28,!gold);
                var picture=ReferenceRect("Subscription action icon",field,0,0,42,42).gameObject.AddComponent<SubscriptionGlyphGraphic>();picture.Symbol=icon.Value;picture.color=gold?new Color(.16f,.075f,.01f):MenuPaper;picture.raycastTarget=false;
                var fit=CentreContent(field,picture.rectTransform,label.rectTransform);fit.Padding=2;fit.Refresh();
            }
            return button;
        }
        void SubscriptionLegalLink(RectTransform stage,string name,UiCopy copy,float x,float y,float width=242)
        {
            // Keep the painted caption on its authored plank. Its larger invisible
            // touch area extends downwards, clear of Restore purchases above.
            const float touchHeight=88,captionOffset=25;
            var button=ReferenceButton(name,stage,x,y+captionOffset-(touchHeight-58)*.5f,width,touchHeight,()=>OpenLegalDocument(name=="Subscription terms"?"terms":"privacy"));
            var label=NoAdsLabel(name=="Subscription terms"?"subscription terms label":"subscription privacy label",ReferenceButtonBody(button),copy,8,0,width-16,58,30,true);
            label.fontStyle=FontStyles.Bold;SubscriptionTextShadow(label);CentreContent((RectTransform)ReferenceButtonBody(button),label.rectTransform);
            var design=RegisterButtonDesign(button,GameButtonRole.TertiaryLabel);design.PreserveSlot=true;design.ContentOffset=Vector2.up*captionOffset;
        }
        void SubscriptionTextShadow(TextMeshProUGUI label)
        {
            var lettering=label.fontMaterial;lettering.EnableKeyword("UNDERLAY_ON");lettering.SetColor(ShaderUtilities.ID_UnderlayColor,new Color(.035f,.018f,.01f,.95f));
            lettering.SetFloat(ShaderUtilities.ID_UnderlayOffsetX,.15f);lettering.SetFloat(ShaderUtilities.ID_UnderlayOffsetY,-.5f);lettering.SetFloat(ShaderUtilities.ID_UnderlayDilate,.10f);lettering.SetFloat(ShaderUtilities.ID_UnderlaySoftness,.15f);label.UpdateMeshPadding();
        }
    }
}
