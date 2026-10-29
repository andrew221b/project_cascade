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
        void DrawReferenceCollection()
        {
            shopTab=1;
            var owned=CurrentTrophyGroup;
            var stage=ReferenceShopSurface();DrawCollectionHeader(stage);
            stage.GetComponent<ReferenceShopLayout>().BindContinuousGround();
            if(owned.Length==0){
                ReferencePicture("Reference shop empty collection paper",stage,ReferenceShopArt("DescriptionPaper"),70,781,582,190);
                ReferencePicture("Reference shop Mika stone platform",stage,ReferenceShopArt("PreviewPlatform"),110,749,500,68);
                ReferencePicture("Trophy Mika preview",stage,HomeArt("Mika"),164,263,394,542);
                ReferenceText("empty collection",stage,"No trophies yet",117,860,484,70,34);
                var open=ReferenceButton("Find trophies in shop",stage,127,976,466,112,()=>{ShowShop();SelectShopTab(1);},"GreenAction");
                ReferenceText("empty collection action",ReferenceButtonBody(open),"Shop",25,18,416,75,38,true);
                return;
            }
            int selected=Array.IndexOf(owned,ShopCatalog.TrophyIds[Mathf.Clamp(shopItem,0,ShopCatalog.TrophyIds.Length-1)]);
            if(selected<0){selected=0;shopItem=Array.IndexOf(ShopCatalog.TrophyIds,owned[0]);}
            giftPage=shopItem;
            DrawCollectionFocus(stage,owned[selected]);DrawCollectionStrip(stage,owned,selected);
            int current=selected;
            SwipeReferencePage(stage,246,728,t=>t.name.StartsWith("Reference shop collection description",StringComparison.Ordinal)||t.name=="Reference shop preview trophy title"||t.name=="Reference shop preview benefit",direction=>SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,owned[current+direction])),direction=>current+direction>=0&&current+direction<owned.Length,true);
        }
        void DrawCollectionHeader(RectTransform stage)
        {
            ReferencePicture("Reference shop awning",stage,ReferenceShopArt("Awning"),121,161,508,85);
            var board=ReferencePicture("Reference shop title board",stage,ReferenceShopArt("HeaderBoard"),156,82,333,126,false);
            board.type=Image.Type.Sliced;board.pixelsPerUnitMultiplier=4;
            var heading=ReferenceText("heading",stage,ReferenceCopy("My trophies","Мои трофеи","Мої трофеї"),180,112,285,66,35);CentreHeading(board,heading);
            ReferenceButton("Back",stage,51,119,86,85,BackFromCollection,"BackButton");
            var wallet=ReferencePicture("Reference shop wallet board",stage,ReferenceShopArt("WalletBoard"),493,110,214,94,false);
            wallet.type=Image.Type.Sliced;wallet.pixelsPerUnitMultiplier=4;
            var emblem=ReferencePicture("Reference shop trophy count icon",wallet.transform,TrophyArt(0),17,13,60,60);
            var count=ReferenceText("wallet count",wallet.transform,UiCopy.Raw(ShopCatalog.TrophyIds.Count(OwnsGift)+" / "+ShopCatalog.TrophyIds.Length),84,23,113,55,38);CentreContent(wallet.rectTransform,emblem.rectTransform,count.rectTransform);
        }
        void DrawCollectionFocus(RectTransform stage,string id)
        {
            bool applied=TrophyEquipped(id),costume=MikaCostume(id,false,false)!=null;
            bool pin=!costume&&MikaBadge(id)!=null;
            var effects=ReferenceRect("Miniature game",stage,47,246,626,574);
            // This local display previews the selected effect without changing equipment.
            CosmeticAmbiance(effects,id,true);
            if(ShopCatalog.IsFrame(id))effects.gameObject.AddComponent<ScreenCosmeticFrame>().Configure(this,id,true);
            var paper=ReferencePicture("Reference shop collection description paper",stage,ReferenceShopArt("DescriptionPaper"),70,781,582,192,false);
            paper.type=Image.Type.Sliced;paper.pixelsPerUnitMultiplier=1.5f;
            ReferencePicture("Reference shop Mika stone platform",stage,ReferenceShopArt("PreviewPlatform"),pin?75:110,749,pin?480:500,68);
            var hero=ReferencePicture("Trophy Mika preview",stage,HomeArt("Mika"),pin?87:163,263,394,542);
            AttachMikaCosmetic(hero,id);var view=hero.GetComponent<MikaCosmeticView>();view.SetPreviewBadgeScale(1.35f);
            hero.raycastTarget=true;var touch=hero.gameObject.AddComponent<Button>();touch.transition=Selectable.Transition.None;
            touch.onClick.AddListener(()=>{view.Poke();MarkTrophyDiscovery();feedback.Sound(ToyCue.Select);});
            if(pin)DrawCollectionMagnifier(stage,hero,view,applied);
            ReferenceText("preview trophy title",stage,ReferenceTrophyTitle(id),117,812,484,50,34);
            ReferenceText("preview benefit",stage,ReferenceTrophyDescription(id),121,881,478,79,26,false,true);
            var primary=ReferenceButton("Trophy action",stage,127,977,466,110,()=>EquipShopGift(id),applied?"RedAction":"GreenAction");
            primary.interactable=!saveBlocked&&!HasSaveError&&monetization?.FullscreenActive!=true;
            ReferenceText("preview action",ReferenceButtonBody(primary),applied?ReferenceCopy("Remove","Отменить","Скасувати"):ReferenceCopy("Apply","Применить","Застосувати"),25,16,416,78,40,true);
        }
        void DrawCollectionStrip(RectTransform stage,string[] owned,int selected)
        {
            int first=selected/4*4,last=Math.Min(first+4,owned.Length),count=last-first;
            ReferenceCollectionArrow(stage,"Previous owned trophies",56,true,owned.Length>4,()=>{
                int index=first==0?(owned.Length-1)/4*4:first-4;SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,owned[index]));
            });
            ReferenceCollectionArrow(stage,"Next owned trophies",598,false,owned.Length>4,()=>{
                int index=last>=owned.Length?0:last;SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,owned[index]));
            });
            float start=(720-(count*112+(count-1)*2))*.5f;
            for(int i=first;i<last;i++){
                string id=owned[i];int index=Array.IndexOf(ShopCatalog.TrophyIds,id);float x=start+(i-first)*114;
                bool current=i==selected;
                var tile=ReferencePicture("Reference shop collection tile "+id,stage,ReferenceShopArt(current?"SelectedCard":"Card"),x-(current?9:0),1105-(current?9:0),112+(current?18:0),112+(current?18:0),false);
                tile.type=Image.Type.Sliced;tile.pixelsPerUnitMultiplier=current?9:7;
                var button=ReferenceButton("Shop object "+index,stage,x,1105,112,112,()=>SelectShopItem(index));
                CataloguePicture(ReferenceButtonBody(button),TrophyArt(index),13,13,86,86,id);
                if(TrophyEquipped(id)){
                    var tag=ReferenceRect("Reference shop applied ribbon",ReferenceButtonBody(button),5,76,102,31);tag.pivot=new Vector2(.5f,.5f);tag.anchoredPosition=new Vector2(56,-91.5f);tag.localRotation=Quaternion.Euler(0,0,13);
                    var label=ReferencePicture("Reference shop applied ribbon face",tag,ReferenceShopArt("PurchasedRibbon"),0,0,102,31,false);label.type=Image.Type.Sliced;label.pixelsPerUnitMultiplier=8;
                    label.color=new Color(.14f,.66f,.21f);
                    ReferenceText("applied card "+id,tag,ReferenceCopy("Applied","Применено","Застосовано"),12,4,78,24,17,true);
                }
            }
        }
        void ReferenceCollectionArrow(RectTransform stage,string name,float x,bool left,bool active,Action action)
        {
            var button=ReferenceButton(name,stage,x,1134,66,66,action,"BackButton");button.interactable=active;
            var drawing=ReferenceButtonBody(button).GetComponentInChildren<Image>();
            drawing.rectTransform.pivot=new Vector2(.5f,.5f);drawing.rectTransform.anchoredPosition=new Vector2(33,-33);
            if(!left)drawing.rectTransform.localScale=new Vector3(-1,1,1);
            drawing.color=active?Color.white:new Color(.76f,.76f,.76f);
        }
        void DrawCollectionMagnifier(RectTransform stage,Image hero,MikaCosmeticView view,bool applied)
        {
            var line=ReferenceRect("Reference shop magnifier connector",stage,0,0,1,2.5f);line.pivot=new Vector2(0,.5f);
            var lineImage=line.gameObject.AddComponent<Image>();lineImage.color=Color.white;lineImage.raycastTarget=false;
            var marker=ReferenceRect("Reference shop worn pin outline",stage,0,0,60,60);marker.pivot=new Vector2(.5f,.5f);
            marker.gameObject.AddComponent<TrophyLensGraphic>().Configure(true,2.5f);marker.gameObject.SetActive(applied);
            var mask=ReferenceRect("Reference shop magnifier window",stage,450,273,250,250);
            mask.gameObject.AddComponent<TrophyLensGraphic>().Configure(false,0);mask.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var detail=ReferencePicture("Magnified actual Mika sprite",mask,hero.sprite,0,0,394,542);detail.rectTransform.anchorMin=detail.rectTransform.anchorMax=new Vector2(.5f,.5f);detail.rectTransform.pivot=new Vector2(.5f,.5f);
            var badge=ReferencePicture("Magnified actual trophy sprite",detail.transform,null,0,0,1,1);badge.rectTransform.anchorMin=badge.rectTransform.anchorMax=new Vector2(.5f,.5f);badge.rectTransform.pivot=new Vector2(.5f,.5f);
            ReferencePicture("Reference shop magnifier frame",stage,ReferenceShopArt("MagnifierFrame"),422.0124f,250.1635f,393.8874f,378.0907f);
            mask.gameObject.AddComponent<TrophyMagnifierView>().Initialize(hero,view,detail,badge,stage,line,marker);
        }
    }
}
