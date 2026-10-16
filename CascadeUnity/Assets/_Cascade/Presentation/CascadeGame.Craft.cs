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
        void CraftTitle(UiCopy title,UiCopy body)
        {
            MenuTitle(title,body);
        }
        void Shelf(float bottom)
        {Rect("Carved display shelf",menuRoot,new Vector2(MenuColumnInset,bottom),new Vector2(1-MenuColumnInset,bottom+.023f)).gameObject.AddComponent<WoodBoardGraphic>();}
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
            DrawReferenceInventory();
        }
        public void ShowPowerupChallenges(){ShowTravelBag();}
        public void ShowReachChallenges(){ShowTravelBag();}
    }
}
