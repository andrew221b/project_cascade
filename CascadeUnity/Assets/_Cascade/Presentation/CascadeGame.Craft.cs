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
            b.GetComponent<Image>().color=selected?new Color(.91f,.75f,.40f,.55f):Color.clear;
            var label=b.GetComponentInChildren<TextMeshProUGUI>();label.rectTransform.anchorMin=new Vector2(0,.13f);label.rectTransform.anchorMax=new Vector2(1,.35f);label.fontSize=17*(Preferences.LargeText?1.12f:1);
            b.name=string.IsNullOrEmpty(name)?asset+"Choice":b.name;
            var icon=Picture("Shelf object",b.transform,new Vector2(.13f,.37f),new Vector2(.87f,.98f),Resources.Load<Sprite>(asset=="RibbonSpool"?"CascadeArt/Devices/RibbonSpool":"CascadeArt/Workshop/"+asset),true);
            return b;
        }
        void DrawCraftBag()
        {
            CraftTitle("Mika's workshop",JourneyClearCount<3?"Restore three lights to open your journal.":"Pick something to make or take along.");
            var hero=Picture("Mika at the bench",menuRoot,new Vector2(.08f,.55f),new Vector2(.41f,.77f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.5f),0,.45f);
            RewardPicture("Parcel",new Vector2(.53f,.60f),new Vector2(.86f,.76f));Shelf(.565f);
            ShelfChoice("Keepsakes","LeafPin",.08f,.36f,.40f,.19f,ShowCollection);
            ShelfChoice("Workshop","BridgeLantern",.52f,.36f,.40f,.19f,ShowWorkshop);Shelf(.34f);
            ShelfChoice("My journal","MoonPatch",.08f,.15f,.40f,.18f,ShowJournal).interactable=JourneyClearCount>=3;
            ShelfChoice("Power-ups","RibbonSpool",.52f,.15f,.40f,.18f,ShowPowerupChallenges);Shelf(.13f);
            MenuButton("Back to map",.025f,ShowJourney,true,.08f,.63f);
            MenuButton("Settings",.025f,ShowSettings,false,.65f,.94f).GetComponentInChildren<TextMeshProUGUI>().fontSize=14;
        }
        public void ShowPowerupChallenges(){reachPage=0;SetScreen(GameScreen.PowerupChallenges);}
        public void ShowReachChallenges(){reachPage=0;SetScreen(GameScreen.ReachChallenges);}
        void DrawReachChallenges()
        {
            bool pickups=CurrentScreen==GameScreen.PowerupChallenges;int offset=pickups?6:0;
            CraftTitle(pickups?"Power-ups":"Ribbon reach",pickups?"Pick up a tool. Find a new way home.":"One ribbon. Swap across one or two pieces.");
            if(pickups)Picture("Teaching tool",menuRoot,new Vector2(.40f,.64f),new Vector2(.60f,.76f),Resources.Load<Sprite>("CascadeArt/Devices/"+(reachPage<3?"RibbonSpool":"Hourglass")),true);
            else ShelfChoice("","RibbonSpool",.36f,.64f,.28f,.12f,()=>{});
            var level=Catalog.BonusLevels[offset+reachPage].Definition;
            var example=Rect("Reach example",menuRoot,new Vector2(.15f,.43f),new Vector2(.85f,.63f));
            if(pickups)LiveDeviceDemo(example,level,reachPage<3?9:10);else MiniMachine(example,level.InitialSymbolIds,level.TargetSymbolIds);
            Text("Reach number",menuRoot,new Vector2(.12f,.34f),new Vector2(.88f,.42f),23,new UiCopy(pickups?"Tool challenge {0}":"Try ribbon {0}",reachPage+1));
            Text("Reach instruction",menuRoot,new Vector2(.12f,.23f),new Vector2(.88f,.34f),18,pickups?(reachPage<3?"Match the spool picture. Then drag past a neighbour.":"Match the hourglass picture. The bomb waits for two swaps."):"Drag past a neighbour. The ribbon works once. Undo brings it back.");
            for(int i=0;i<6;i++){int pick=i;float left=.09f+i*.14f;var b=Button(UiCopy.Raw((i+1).ToString()),menuRoot,new Vector2(left,.16f),new Vector2(left+.12f,.22f),()=>{reachPage=pick;DrawMenu();});if(i==reachPage)b.GetComponent<Image>().color=new Color(.97f,.73f,.3f);}
            MenuButton("Try this trail",.075f,()=>StartContent(Catalog.BonusLevels[offset+reachPage]),true);
            MenuButton("Back",.005f,ShowTravelBag,false,.08f,.45f);
            if(pickups)MenuButton("Ribbon reach",.005f,ShowReachChallenges,false,.50f,.92f);
        }
    }
}
