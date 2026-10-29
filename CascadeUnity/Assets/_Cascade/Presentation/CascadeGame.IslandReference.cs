using System;
using System.Globalization;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        void DrawReferenceIsland(int[] stops)
        {
            LoadNavigationArt();machine.SetZone(mapPage);machine.LivingWorld.SetMapSky(null);machine.LivingWorld.SetWorkshop(true);
            var sky=Rect("Island illustrated background",menuRoot,Vector2.one*.5f,Vector2.one*.5f);
            var painting=sky.gameObject.AddComponent<Image>();painting.sprite=IslandLevelArt.Background(mapPage);painting.color=Color.white;painting.raycastTarget=false;
            var stage=Rect("Island reference composition",menuRoot,new Vector2(.5f,1),new Vector2(.5f,1));stage.pivot=new Vector2(.5f,1);
            var fit=stage.gameObject.AddComponent<IslandLevelLayout>();fit.Initialize(menuRoot,sky);
            var weather=Rect("Island weather behind controls",stage,Vector2.zero,Vector2.one).gameObject.AddComponent<MenuWorldParticles>();weather.Initialize(mapPage);
            stage.gameObject.AddComponent<IslandLevelAmbiance>().Initialize(this,weather);
            var title=ReferencePicture("Island title board",stage,ReferenceShopArt("HeaderBoard"),175,125,385,116,false);
            var titleInset=ReferenceRect("Island title readable inset",title.transform,51,37,283,60);
            var heading=ReferenceText("island world title",titleInset,JourneyStory.ZoneName(mapPage),0,0,283,60,37);heading.fontSizeMin=17;var titleFit=CentreContent(titleInset,heading.rectTransform);titleFit.Padding=0;titleFit.Refresh();
            title.gameObject.AddComponent<AttachedPlants>().Initialize(this,mapPage,true);
            var back=ReferenceButton("Back",stage,80,141,72,72,CloseJourneyIsland,"BackButton");fit.BindTouch((RectTransform)back.transform,new Vector2(72,72));
            var progress=ReferencePicture("Island earned parchment",stage,IslandLevelArt.Progress,234,243,253,84,false);progress.type=Image.Type.Sliced;progress.pixelsPerUnitMultiplier=8;
            double earned=stops.Select(n=>profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==Catalog.PrimaryLevels[n].Definition.CampaignNodeId)).Where(c=>c!=null).Sum(c=>c.BestStarRating);
            var star=ReferencePicture("Island progress star",progress.transform,CompletionArt(0),18,15,45,45);
            var count=ReferenceText("island progress",progress.transform,UiCopy.Raw(LevelCompletion.FormatStars(earned)+" / "+stops.Length*3),70,15,150,50,34);
            var total=CentreContent(progress.rectTransform,star.rectTransform,count.rectTransform);total.Padding=8;total.Refresh();
            var scrollRoot=ReferenceRect("Island level scroll",stage,73,390,572,stage.rect.height-396);
            var scroll=scrollRoot.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.inertia=true;scroll.decelerationRate=.12f;scroll.elasticity=.1f;scroll.movementType=ScrollRect.MovementType.Elastic;scroll.scrollSensitivity=30;
            var viewport=Rect("Island level viewport",scrollRoot,Vector2.zero,Vector2.one);Background(viewport,Color.white,true);viewport.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            var list=Rect("Island levels",viewport,new Vector2(0,1),Vector2.one);list.pivot=new Vector2(.5f,1);list.sizeDelta=new Vector2(0,stops.Length*123-5+IslandLevelLayout.EndOfListGap);
            var layout=list.gameObject.AddComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(12,12,0,IslandLevelLayout.EndOfListGap);layout.spacing=5;layout.childControlWidth=layout.childControlHeight=layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
            foreach(int stop in stops)DrawReferenceIslandLevel(stop,list,fit);
            scroll.viewport=viewport;scroll.content=list;
            var track=ReferenceRect("Island scroll track",stage,642,429,44,655);track.pivot=Vector2.one*.5f;track.anchoredPosition=new Vector2(664,-756.5f);
            var hit=track.gameObject.AddComponent<Image>();hit.color=Color.clear;
            var rail=Rect("Island scrollbar rail",track,new Vector2(.34f,0),new Vector2(.66f,1));var ink=rail.gameObject.AddComponent<Image>();ink.sprite=ReferenceShopArt("GreenAction");ink.type=Image.Type.Sliced;ink.pixelsPerUnitMultiplier=6;ink.color=new Color(.25f,.29f,.25f,.65f);ink.raycastTarget=false;
            var sliding=Rect("Island scrollbar sliding area",track,new Vector2(0,.01f),new Vector2(1,.99f));
            var handle=Rect("Island scrollbar thumb hit",sliding,Vector2.zero,Vector2.one);
            var thumbRect=Rect("Island scrollbar amber thumb",handle,new Vector2(.5f,0),new Vector2(.5f,1));thumbRect.sizeDelta=new Vector2(18,0);var thumb=thumbRect.gameObject.AddComponent<Image>();thumb.sprite=ParcelArt("AmberAction");thumb.type=Image.Type.Sliced;thumb.pixelsPerUnitMultiplier=8;thumb.color=Color.white;
            var bar=track.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle;bar.targetGraphic=thumb;bar.direction=Scrollbar.Direction.BottomToTop;bar.size=.14f;
            bar.onValueChanged.AddListener(value=>scroll.verticalNormalizedPosition=value);
            scroll.onValueChanged.AddListener(value=>bar.SetValueWithoutNotify(value.y));fit.BindScroll(scrollRoot,track);
            var cloud=Resources.Load<Sprite>("CascadeArt/Journey/Cloud");
            if(cloud!=null){
                var edge=Rect("Island foreground edge cloud",stage,new Vector2(0,0),new Vector2(0,0));edge.pivot=new Vector2(.5f,0);edge.anchoredPosition=new Vector2(360,-268);edge.sizeDelta=new Vector2(1100,390);var mist=edge.gameObject.AddComponent<Image>();mist.sprite=cloud;mist.color=mapPage==8?new Color(.53f,.64f,.86f,.94f):Color.white;mist.preserveAspect=true;mist.raycastTarget=false;
            }
            Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(list);
            int focus=Array.IndexOf(stops,selectedJourneyStop);float overflow=Mathf.Max(0,list.rect.height-viewport.rect.height);
            list.anchoredPosition=new Vector2(0,Mathf.Clamp((focus-1)*123,0,overflow));bar.SetValueWithoutNotify(scroll.verticalNormalizedPosition);
        }
        void DrawReferenceIslandLevel(int stop,RectTransform list,IslandLevelLayout fit)
        {
            var content=Catalog.PrimaryLevels[stop];var receipt=profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==content.Definition.CampaignNodeId);
            bool open=IsJourneyStopOpen(stop),resume=CanResumeSelectedJourney(stop),cleared=receipt!=null;
            var row=ReferenceButton("Island level "+(stop+1),list,0,0,548,118,()=>ReplayJourneyLevel(stop));row.interactable=open&&!saveBlocked;
            var size=row.gameObject.AddComponent<LayoutElement>();size.minHeight=size.preferredHeight=118;
            var body=ReferenceButtonBody(row);ReferencePicture("Island level timber card",body,IslandLevelArt.Row,0,0,548,118,false);
            var badge=ReferenceRect("Island level number badge",body,13,14,92,86);
            var number=ReferenceText("island level number",badge,UiCopy.Raw(DisplayLevelNumber(content.Definition.CampaignNodeId)),0,0,92,86,58);var numberFit=CentreContent(badge,number.rectTransform);numberFit.Padding=4;numberFit.Refresh();
            for(int s=0;s<3;s++){
                var socket=ReferenceRect("Island star socket "+s,body,121+s*50,30,48,48);
                var empty=Picture("Island earned star "+s,socket,Vector2.zero,Vector2.one,CompletionArt(0),true);empty.material=OwnedTrophyMaterial();empty.color=new Color(.68f,.68f,.68f);
                float amount=Mathf.Clamp01((float)(receipt?.BestStarRating??0)-s);
                if(amount>0){var gold=Picture("Island gold star "+s,socket,Vector2.zero,Vector2.one,CompletionArt(0),true);gold.type=Image.Type.Filled;gold.fillMethod=Image.FillMethod.Horizontal;gold.fillOrigin=0;gold.fillAmount=amount;}
            }
            var record=ReferenceRect("Island record column",body,298,22,90,74);
            ReferenceText("island record caption",record,ReferenceCopy("Best","Рекорд","Рекорд"),0,0,90,27,20);
            ReferenceText("island record value",record,UiCopy.Raw(receipt==null?"—":receipt.BestScore.ToString("N0",CultureInfo.InvariantCulture)),0,28,90,40,31);CentreInfoColumn(record);
            var action=ReferenceButton("Island action "+(stop+1),body,414,13,113,88,()=>ReplayJourneyLevel(stop));action.interactable=row.interactable;
            var actionBody=ReferenceButtonBody(action);var face=ReferencePicture("Island compact shared action skin",actionBody,ReferenceShopArt(cleared?"AmberAction":open?"GreenAction":"GrayAction"),0,0,113,88,false);face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=2.8f;
            action.targetGraphic=face;action.transition=Selectable.Transition.ColorTint;var colors=action.colors;colors.normalColor=colors.highlightedColor=Color.white;colors.pressedColor=new Color(.93f,.97f,.90f);colors.disabledColor=Color.white;action.colors=colors;
            var surface=ReferenceRect("Island action centred contents",actionBody,8,8,97,72);RectTransform glyph;
            if(open){
                glyph=ReferenceRect("Island action glyph",surface,38,8,37,37);glyph.gameObject.AddComponent<PauseGlyphGraphic>().Configure(cleared?2:3);
            }else glyph=ReferencePicture("Island action lock",surface,MapReferenceArt("Lock"),39,6,35,37).rectTransform;
            var label=ReferenceText("island action caption",surface,resume?(UiCopy)"Continue":cleared?ReferenceCopy("Replay","Переиграть","Переграти"):open?(UiCopy)"Play":ReferenceCopy("Locked","Закрыт","Закрито"),4,55,105,25,19,!cleared);
            CentreStack(surface,glyph,label.rectTransform);var group=surface.GetComponent<CenteredContentRow>();group.Padding=0;group.Refresh();fit.BindTouch((RectTransform)action.transform,new Vector2(113,88));
        }
    }
}
