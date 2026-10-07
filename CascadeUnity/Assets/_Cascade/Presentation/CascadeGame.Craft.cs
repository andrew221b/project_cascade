using System;
using Cascade.Content;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int reachPage;
        void CraftTitle(UiCopy title,UiCopy body)
        {
            MenuTitle(title,body);
        }
        void Shelf(float bottom)
        {Rect("Carved display shelf",menuRoot,new Vector2(.08f,bottom),new Vector2(.92f,bottom+.023f)).gameObject.AddComponent<WoodBoardGraphic>();}
        Button ShelfChoice(string name,string asset,float left,float bottom,float width,float height,Action action,bool selected=false)
        {
            var b=Button(name,menuRoot,new Vector2(left,bottom),new Vector2(left+width,bottom+height),action);
            b.GetComponent<Image>().color=string.IsNullOrEmpty(name)?selected?new Color(.98f,.74f,.32f):new Color(.84f,.91f,.87f):Color.clear;
            var label=b.GetComponentInChildren<TextMeshProUGUI>();label.rectTransform.anchorMin=new Vector2(.05f,.10f);label.rectTransform.anchorMax=new Vector2(.95f,.37f);label.fontSize=17*(Preferences.LargeText?1.12f:1);label.margin=new Vector4(3,0,3,0);
            if(!string.IsNullOrEmpty(name)){
                var plate=Rect("Shelf caption cloth",b.transform,new Vector2(.025f,.07f),new Vector2(.975f,.37f));plate.SetAsFirstSibling();
            }
            b.name=string.IsNullOrEmpty(name)?asset+"Choice":b.name;
            var icon=Picture("Shelf object",b.transform,new Vector2(.07f,string.IsNullOrEmpty(name)?.06f:.37f),new Vector2(.93f,.95f),Resources.Load<Sprite>(asset=="RibbonSpool"?"CascadeArt/Devices/RibbonSpool":"CascadeArt/Workshop/"+asset),true);
            if(string.IsNullOrEmpty(name)&&selected)MarkSelection(b,true);
            return b;
        }
        void DrawCraftBag()
        {
            CraftTitle("My things",JourneyClearCount<3?"Restore three lights to open your journal.":"Tools and trophies for the journey.");
            var hero=Picture("Mika at the bench",menuRoot,new Vector2(.13f,.59f),new Vector2(.43f,.80f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.5f),0,.45f);
            RewardPicture("Parcel",new Vector2(.55f,.62f),new Vector2(.86f,.79f));Shelf(.575f);
            MenuNavRow("My tools","Hourglass",0,ShowSupplies);
            MenuNavRow("Keepsakes","LeafPin",1,ShowCollection);
            MenuNavRow("My journal","MoonPatch",2,ShowJournal,JourneyClearCount>=3);
            MenuNavRow("Workshop","BridgeLantern",3,ShowWorkshop);
            MenuButton("Practice",.10f,ShowPowerupChallenges);
            MenuButton("Back",.025f,BackFromBag);
        }
        public void ShowPowerupChallenges(){reachPage=0;SetScreen(GameScreen.PowerupChallenges);}
        public void ShowReachChallenges(){reachPage=0;SetScreen(GameScreen.ReachChallenges);}
        void DrawReachChallenges()
        {
            bool pickups=CurrentScreen==GameScreen.PowerupChallenges;int offset=pickups?6:0;
            CraftTitle(pickups?"Power-ups":"Ribbon reach",pickups?"Pick up a tool. Find a new way home.":"One ribbon. Swap across one or two pieces.");
            if(pickups)Picture("Teaching tool",menuRoot,new Vector2(.38f,.735f),new Vector2(.62f,.812f),Resources.Load<Sprite>("CascadeArt/Devices/"+(reachPage<3?"RibbonSpool":"FrostFlask")),true);
            else ShelfChoice("","RibbonSpool",.36f,.735f,.28f,.077f,()=>{});
            var level=Catalog.BonusLevels[offset+reachPage].Definition;
            var example=Rect("Reach example",menuRoot,new Vector2(.10f,.53f),new Vector2(.90f,.73f));
            if(pickups)LiveDeviceDemo(example,level,reachPage<3?9:10);else MiniMachine(example,level.InitialSymbolIds,level.TargetSymbolIds);
            MenuReadingSurface("Tool lesson cloth",new Vector2(.12f,.225f),new Vector2(.88f,.425f));
            Text("Reach number",menuRoot,new Vector2(.28f,.39f),new Vector2(.72f,.475f),23,UiCopy.Raw((reachPage+1)+" / 6")).font=art.titleFont;
            var previous=DirectionButton("ReachPrevious",menuRoot,new Vector2(.08f,.475f),new Vector2(.26f,.475f),false,()=>{reachPage=(reachPage+5)%6;DrawMenu();});RegisterMenuRow(previous,.475f,0,52,0);
            var next=DirectionButton("ReachNext",menuRoot,new Vector2(.74f,.475f),new Vector2(.92f,.475f),true,()=>{reachPage=(reachPage+1)%6;DrawMenu();});RegisterMenuRow(next,.475f,0,52,0);
            Text("Reach instruction",menuRoot,new Vector2(.10f,.22f),new Vector2(.90f,.375f),20,pickups?(reachPage<3?"Match the spool picture. Then drag past a neighbour.":"Match the flask picture. The bomb waits for two swaps."):"Drag past a neighbour. The ribbon works once. Undo brings it back.");
            MenuButton("Try this trail",.082f,()=>StartContent(Catalog.BonusLevels[offset+reachPage]),true);
            MenuButton("Back",.005f,ShowTravelBag,false,.08f,.48f);
            if(pickups)MenuButton("Ribbon reach",.005f,ShowReachChallenges,false,.52f,.92f);
        }
    }
}
