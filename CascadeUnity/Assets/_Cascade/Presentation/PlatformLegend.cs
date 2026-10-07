using System;
using System.Collections.Generic;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly List<PlatformLegend> platformLegends=new List<PlatformLegend>();
        readonly Sprite[] platformProps=new Sprite[4];
        internal Material PlatformPropMaterial=>ToolSilhouette;
        internal Sprite PlatformProp(int index)
        {
            if(platformProps[index]!=null)return platformProps[index];
            var atlas=Resources.Load<Texture2D>("CascadeArt/Reactions/PlatformProps");if(atlas==null)return null;
            var bounds=new[]{new Rect(75,514,691,499),new Rect(795,514,713,499),new Rect(15,58,744,410),new Rect(806,124,710,280)};
            var r=bounds[index];platformProps[index]=Sprite.Create(atlas,r,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);return platformProps[index];
        }
        void BuildPlatformLegend(RectTransform card,FloorDefinition floor,int index)
        {
            var band=Rect("Platform legend",card,new Vector2(.5f,.33f),new Vector2(.5f,.33f));
            var counter=Rect("Remaining wood",band,Vector2.zero,Vector2.one);
            var plaque=Picture("Inset wood strength plaque",counter,Vector2.zero,Vector2.one,PlatformProp(3),true);plaque.material=ToolSilhouette;
            var rim=Text("Engraving edge",counter,Vector2.zero,Vector2.one,13,"");
            var number=floorLabels[index];number.rectTransform.SetParent(counter,false);
            number.rectTransform.anchorMin=Vector2.zero;number.rectTransform.anchorMax=Vector2.one;
            number.rectTransform.offsetMin=number.rectTransform.offsetMax=Vector2.zero;
            foreach(var label in new[]{rim,number}) {
                label.font=art.titleFont;label.fontStyle=FontStyles.Bold;label.alignment=TextAlignmentOptions.Center;
                label.margin=Vector4.zero;label.textWrappingMode=TextWrappingModes.NoWrap;
                label.enableAutoSizing=true;label.overflowMode=TextOverflowModes.Overflow;label.gameObject.AddComponent<SymbolGlyphCenter>();label.raycastTarget=false;
            }
            rim.color=new Color(1,.84f,.48f,.85f);
            var legend=band.gameObject.AddComponent<PlatformLegend>();legend.Initialize(this,index,floor,number,rim,counter);platformLegends.Add(legend);
        }
        void PaintPlatformLegends(PresentationFrame frame)
        {foreach(var legend in platformLegends)legend.Paint(frame);}
        void LayoutPlatformLegends(float unit)
        {foreach(var legend in platformLegends)legend.Layout(unit);}
        void DisposePlatformProps(){foreach(var sprite in platformProps)if(sprite!=null)Destroy(sprite);}
    }

    // The committed presentation frame owns the number; motion only follows the wood.
    public sealed class PlatformLegend:MonoBehaviour
    {
        CascadeGame game;int floor;FloorDefinition definition;
        TextMeshProUGUI number,rim;RectTransform counter;float pulse,unit=1;int previous=-1;
        public int Remaining {get;private set;}
        public void Initialize(CascadeGame owner,int index,FloorDefinition data,TextMeshProUGUI label,TextMeshProUGUI edge,RectTransform inset)
        {game=owner;floor=index;definition=data;number=label;rim=edge;counter=inset;}
        public void Paint(PresentationFrame state)
        {
            bool past=!state.FloorVisible(floor);gameObject.SetActive(!past);if(past)return;
            Remaining=state.Status==PuzzleStatus.Playing&&state.ActiveFloor==floor?Math.Max(0,state.Durability):definition.BaseDurability;
            if(previous!=Remaining){pulse=previous<0?0:.42f;previous=Remaining;var copy=UiCopy.Raw(Remaining.ToString());number.SetCopy(copy);rim.SetCopy(copy);}
            PaintInk();
        }
        public void Layout(float value){unit=value;FollowWood(0,0);}
        public void FollowWood(float sag,float fracture)
        {
            if(game.Machine==null)return;
            var band=(RectTransform)transform;
            var center=game.Machine.SurfacePoint(floor,.5f,.30f);center.z=band.position.z;
            band.position=center;band.rotation=Quaternion.Euler(0,0,game.Machine.FloorVisualAngle(floor));
            float height=game.Machine.FloorFaceHeight(floor)/Mathf.Max(.0001f,band.lossyScale.y);
            band.sizeDelta=new Vector2(height*2.9f,height*.90f);
            number.fontSizeMax=rim.fontSizeMax=Mathf.Min(18*unit,height*1.08f);
            number.fontSizeMin=rim.fontSizeMin=Mathf.Min(number.fontSizeMax,height*.55f);
            rim.rectTransform.anchoredPosition=Vector2.down*Mathf.Min(.65f*unit,height*.04f);
            counter.gameObject.SetActive(fracture<.22f);
        }
        void Update()
        {
            if(game==null||game.Interaction==null||game.CurrentScreen!=GameScreen.Playing||game.Interaction.IsPaused||game.Interaction.IsSuspended)return;
            pulse=Mathf.Max(0,pulse-Time.unscaledDeltaTime);PaintInk();
        }
        void PaintInk()
        {
            rim.color=new Color(1,.84f,.48f,.85f);
            var ink=Remaining<=1?new Color(.46f,.10f,.035f):new Color(.23f,.105f,.035f);
            number.color=Color.Lerp(ink,new Color(.65f,.30f,.05f),game.ReducedMotion?0:Mathf.Sin(pulse/.42f*Mathf.PI)*.35f);
        }
    }
}
