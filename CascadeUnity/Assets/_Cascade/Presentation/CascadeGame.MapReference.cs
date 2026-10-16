using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        const string MapReferencePath="CascadeArt/MapReferenceOct14/";
        bool ReferenceMapScreen=>CurrentScreen==GameScreen.JourneyMap&&islandBrowseIndex<0;
        Sprite MapReferenceArt(string name)=>Resources.Load<Sprite>(MapReferencePath+name);
        void DrawReferenceMap()
        {
            journeyFooter=null;journeyFooterButtons=null;journeyModeTabs=null;
            machine.SetZone(mapPage);machine.LivingWorld.SetMapSky(null);
            var stage=ReferenceShopSurface(MapReferenceArt("Backdrop"));stage.name="Reference map composition";
            stage.GetComponent<ReferenceShopLayout>().BindCompactGround(WorldFrameArt.MapLandingViewportFraction);
            Background(stage,Color.clear,true);

            stage.gameObject.AddComponent<MenuSwipe>().Initialize(TurnJourneyMap);
            var leaves=stage.GetComponentInChildren<MenuLeafDrift>();
            // Bring ambient leaves into view by advancing the same offscreen trajectories.
            leaves.WarmStart();
            var view=stage.gameObject.AddComponent<MapReferenceAmbiance>();view.Initialize(this,stage.GetComponent<ReferenceShopLayout>().Backdrop);
            int stop=IsPinnedJourneyStop(selectedJourneyStop)?selectedJourneyStop:NextJourneyStop();
            int focused=JourneyStory.Zone(stop)==mapPage?JourneyStory.IslandFor(stop):-1;
            var islands=new RectTransform[5];var overlays=new RectTransform[5];
            var places=new[]{new Vector4(-5,741,302,260),new Vector4(330,576,264,271),new Vector4(145,470,234,220),new Vector4(486,245,216,270),new Vector4(222,240,220,211)};

            WorldMapComposition.Apply(places,mapPage);
            // Keep the original island sizes; the wider alternating route leaves air
            // between their actual painted silhouettes, not just their centres.
            for(int i=0;i<5;i++){
                var p=places[i];var root=ReferenceRect("Floating island "+i,stage,p.x,p.y,p.z,p.w);
                root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=new Vector2(p.x+p.z*.5f,-p.y-p.w*.5f);
                islands[i]=root;view.BindIsland(root,i);
            }
            // The user approved omitting map bridges when their landings do not match
            // the nine painted island sets. Gameplay bridge mechanics are independent.
            for(int i=0;i<5;i++){
                var p=places[i];var overlay=ReferenceRect("Reference map island labels "+i,stage,p.x,p.y,p.z,p.w);
                overlay.pivot=new Vector2(.5f,.5f);overlay.anchoredPosition=new Vector2(p.x+p.z*.5f,-p.y-p.w*.5f);view.BindIsland(overlay,i);overlays[i]=overlay;
            }
            for(int i=0;i<5;i++){
                int island=i;var root=islands[i];var labelRoot=overlays[i];var p=places[i];
                var stops=IslandJourneyStops(mapPage,i);bool open=stops.Any(IsJourneyStopOpen);
                bool complete=stops.Length>0&&stops.All(n=>sessionClears.Contains(Catalog.PrimaryLevels[n].Definition.CampaignNodeId));
                if(i==focused){var light=ReferenceRect("Reference map active island light",root,-26,-30,p.z+52,p.w+40).gameObject.AddComponent<MapIslandLight>();view.BindLight(light);var contour=ReferencePicture("Reference map active green contour",root,WorldIslandArt.Get(mapPage,i),-7,-7,p.z+14,p.w+14);view.BindContour(contour);}
                var drawing=ReferencePicture("Reference map island artwork "+i,root,WorldIslandArt.Get(mapPage,i),0,0,p.z,p.w);
                if(!open)view.Desaturate(drawing);
                var button=ReferenceButton("Island"+(i+1),root,10,10,p.z-20,p.w-20,()=>OpenJourneyIsland(island));button.interactable=open;
                if(!open){
                    Vector2 pos=i==2?new Vector2(108,98):i==3?new Vector2(100,151):new Vector2(78,86);
                    ReferencePicture("Reference map island lock "+i,labelRoot,MapReferenceArt("Lock"),pos.x,pos.y,64,58);
                }else{
                    Vector2 pos=i==0?new Vector2(111,176):i==1?new Vector2(70,140):new Vector2(p.z*.27f,p.w*.60f);
                    var plaque=ReferencePicture("Reference map island star plaque "+i,labelRoot,MapReferenceArt("Progress"),pos.x,pos.y,151,49,false);plaque.gameObject.AddComponent<PrimaryButtonSkin>().Configure(MapReferenceArt("Progress"));
                    var star=ReferencePicture("Reference map island star "+i,plaque.transform,VictoryStar(0),12,9,25,25);
                    var earned=stops.Select(n=>profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==Catalog.PrimaryLevels[n].Definition.CampaignNodeId)).Where(c=>c!=null).Sum(c=>c.BestStarRating);
                    var stars=ReferenceText("map island stars "+i,plaque.transform,UiCopy.Raw(LevelCompletion.FormatStars(earned)+" / "+stops.Length*3),45,9,94,31,25);CentreContent(plaque.rectTransform,star.rectTransform,stars.rectTransform);
                    if(complete){
                        var ribbon=ReferencePicture("Reference map completed ribbon "+i,labelRoot,MapReferenceArt("Completed"),pos.x-10,pos.y-49,172,60);
                        var completed=ReferenceText("map completed "+i,ribbon.rectTransform,ReferenceCopy("Completed","Завершён","Завершено"),28,11,116,34,23,true);CentreHeading(ribbon,completed);
                    }
                }
            }
            // Separate attached sprigs flex from the branches; artwork retains its natural aspect.
            MapSprig(stage,view,"upper left foliage",-15,-13,179,168,17,false);
            MapSprig(stage,view,"upper right foliage",578,-16,166,168,-17,true);
            MapSprig(stage,view,"lower left foliage",-34,1160,128,130,12,false);
            MapSprig(stage,view,"lower right foliage",632,1158,123,130,-12,true);
            MapFlower(stage,view,"left daisies",-3,1190,99,85,false);
            MapFlower(stage,view,"right daisies",626,1184,102,91,true);
            var header=ReferenceRect("Reference map aligned header",stage,0,0,720,260);view.BindHeader(header);
            ReferencePicture("Reference map world title board",header,CleanMenuArt.WorldTitle(mapPage),110,30,500,156,false);
            var heading=ReferenceText("map world heading",header,JourneyStory.ZoneName(mapPage),174,73,372,86,44,false,true);
            heading.alignment=TextAlignmentOptions.Center;heading.lineSpacing=0;var titleBoard=header.Find("Reference map world title board").GetComponent<Image>();CentreHeading(titleBoard,heading);
            if(mapPage==0){
                var sprig=ReferencePicture("Reference map title wind sprig",header,MapReferenceArt("Foliage"),112,42,47,40);
                view.BindStem(sprig.rectTransform,-9,false);
            }
            MapReferenceSquare(header,"Back",14,119,ShowHome,0);
            MapReferenceSquare(header,"Map settings",618,119,ShowSettings,1);
            var landing=ReferenceRect("Reference map landing controls",stage,0,990,720,250);
            journeyFooter=landing;
            DrawReferenceMapModes(landing);
            var go=ReferenceButton("Travel",landing,81,130,558,112,TravelSelectedJourney);
            MapIllustratedControl("Reference map continue wooden frame",ReferenceButtonBody(go),0,0,558,112);
            var play=ReferenceRect("Reference map continue icon",ReferenceButtonBody(go),52,32,44,48).gameObject.AddComponent<PauseGlyphGraphic>();play.Configure(3);
            string number=DisplayLevelNumber(Catalog.PrimaryLevels[selectedJourneyStop].Definition.CampaignNodeId).ToString();
            var caption=PersonalMissionActive?ReferenceCopy("Continue mission","Продолжить задание","Продовжити завдання"):
                ReferenceCopy("Continue level "+number,"Продолжить "+number+" уровень","Продовжити "+number+" рівень");
            var continueCaption=ReferenceText("map continue",ReferenceButtonBody(go),caption,112,27,391,58,35,true);CentreContent((RectTransform)ReferenceButtonBody(go),play.rectTransform,continueCaption.rectTransform);
            go.interactable=!saveBlocked&&!HasSaveError;
            if(CanTurnJourneyMap(-1))ReferenceButton("Previous world",landing,45,23,48,58,()=>TurnJourneyMap(-1),"BackButton");
            if(CanTurnJourneyMap(1)){
                var next=ReferenceButton("Next world",landing,630,23,48,58,()=>TurnJourneyMap(1));
                ReferencePicture("Reference map next world",ReferenceButtonBody(next),ReferenceShopArt("PagerRight"),2,10,44,38);
            }
            stage.GetComponent<ReferenceShopLayout>().BindLandingControls(landing);
            var islandMotion=Rect("Map islands interactive motion",stage,Vector2.zero,Vector2.one);islandMotion.SetSiblingIndex(2);
            foreach(var island in islands)island.SetParent(islandMotion,true);foreach(var overlay in overlays)overlay.SetParent(islandMotion,true);
            stage.GetComponent<MenuSwipe>().Initialize(this,TurnJourneyMap,islandMotion,CanTurnJourneyMap);
            view.PaintNow();
        }
        void MapReferenceSquare(RectTransform stage,string name,float x,float y,Action action,int glyph)
        {
            var button=ReferenceButton(name,stage,x,y,88,86,action);
            ReferencePicture("Reference map square face",ReferenceButtonBody(button),CleanMenuArt.Get("SquareButton"),0,0,88,86);
            var icon=ReferenceRect("Reference map square glyph",ReferenceButtonBody(button),22,19,45,48).gameObject.AddComponent<PauseGlyphGraphic>();icon.Configure(glyph);icon.color=new Color(.24f,.055f,.008f);CentreContent((RectTransform)ReferenceButtonBody(button),icon.rectTransform);
        }
        void DrawReferenceMapModes(RectTransform stage)
        {
            var strip=ReferenceRect("Reference map mode surface",stage,77,-9,566,125).gameObject.AddComponent<MapModeSurface>();
            strip.Configure(false);
            var modes=new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed};
            for(int i=0;i<3;i++){
                int tab=i;bool active=PreferredMode==modes[i];float x=114+i*164;
                if(active)ReferenceRect("Reference map selected mode",stage,x,12,164,83).gameObject.AddComponent<MapModeSurface>().Configure(true);
                var b=ReferenceButton("Mapmode"+modes[i],stage,x,12,164,83,()=>SelectMode(modes[tab]));b.interactable=active||CanSwitchJourneyMode;
                RectTransform symbols;
                if(i==0){var shapes=ReferenceRect("Reference map shapes glyphs",ReferenceButtonBody(b),0,0,92,26).gameObject.AddComponent<MapModeGlyphs>();shapes.raycastTarget=false;shapes.color=active?MenuPaper:new Color(.24f,.055f,.008f);symbols=shapes.rectTransform;}
                else symbols=ReferenceText("map mode symbols "+i,ReferenceButtonBody(b),UiCopy.Raw(i==1?(UiLanguage=="en"?"ABC":"АБВ"):"● A ♥"),0,0,140,44,32,active).rectTransform;
                var caption=ReferenceText("map mode "+i,ReferenceButtonBody(b),i==0?ReferenceCopy("Shapes","Фигуры","Фігури"):i==1?ReferenceCopy("Words","Слова","Слова"):ReferenceCopy("Mix","Микс","Мікс"),10,35,144,42,26,active);
                CentreStack((RectTransform)ReferenceButtonBody(b),symbols,caption.rectTransform);
            }
        }
        void MapIllustratedControl(string name,Transform parent,float x,float y,float width,float height)
        {
            var face=ReferencePicture(name,parent,Resources.Load<Sprite>(HomeArtPath+"PlayButton"),x,y,width,height,false);
            face.gameObject.AddComponent<PrimaryButtonSkin>().Configure(face.sprite);
        }
        void MapSprig(RectTransform stage,MapReferenceAmbiance view,string name,float x,float y,float width,float height,float angle,bool flip)
        {
            var image=ReferencePicture("Reference map "+name,stage,MapReferenceArt("Foliage"),x,y,width,height);var rect=image.rectTransform;
            rect.pivot=new Vector2(.5f,.8f);rect.anchoredPosition=new Vector2(x+width*.5f,-y-height*.2f);rect.localRotation=Quaternion.Euler(0,0,angle);if(flip)rect.localScale=new Vector3(-1,1,1);view.BindStem(rect,angle,false,name=="upper left foliage"?0:name=="upper right foliage"?1:name=="lower left foliage"?2:3);
        }
        void MapFlower(RectTransform stage,MapReferenceAmbiance view,string name,float x,float y,float width,float height,bool flip)
        {
            var image=ReferencePicture("Reference map "+name,stage,MapReferenceArt("Daisies"),x,y,width,height);var rect=image.rectTransform;
            rect.pivot=new Vector2(.5f,0);rect.anchoredPosition=new Vector2(x+width*.5f,-y-height);if(flip)rect.localScale=new Vector3(-1,1,1);view.BindStem(rect,0,true,name=="left daisies"?2:3);
        }
    }
}
