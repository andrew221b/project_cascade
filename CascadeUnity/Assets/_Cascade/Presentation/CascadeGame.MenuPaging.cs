using System;
using System.Linq;
using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        public int MenuPageDirection{get;private set;}
        public void TurnMenuPage(Action select,int direction)
        {
            MenuPageDirection=direction;try{select();}finally{MenuPageDirection=0;}
        }
        void SwipeReferencePage(RectTransform stage,float top,float height,Func<Transform,bool> includes,Action<int> select,Func<int,bool> canTurn,bool wholeSurface=false)
        {
            var members=stage.Cast<Transform>().Where(includes).ToArray();
            var viewport=ReferenceRect("Swipe shelf viewport",stage,35,top,650,height);Background(viewport,Color.clear,true);viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            viewport.SetSiblingIndex(members.Length==0?2:members.Min(t=>t.GetSiblingIndex()));
            var moving=Rect("Interactive shelf contents",viewport,Vector2.zero,Vector2.one);
            foreach(var member in members)member.SetParent(moving,true);
            (wholeSurface?stage:viewport).gameObject.AddComponent<MenuSwipe>().Initialize(this,select,moving,canTurn);
        }
    }
}
