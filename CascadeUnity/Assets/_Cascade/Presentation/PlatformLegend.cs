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
            var band=Rect("Platform legend",card,new Vector2(.035f,.08f),new Vector2(.965f,.08f));band.sizeDelta=new Vector2(0,34);
            var counter=Picture("Remaining wood",band,Vector2.zero,new Vector2(.27f,1),PlatformProp(3),true);counter.material=ToolSilhouette;
            var number=floorLabels[index];number.rectTransform.SetParent(counter.transform,false);
            number.rectTransform.anchorMin=new Vector2(.40f,.05f);number.rectTransform.anchorMax=new Vector2(.93f,.95f);
            number.rectTransform.offsetMin=number.rectTransform.offsetMax=Vector2.zero;number.font=art.titleFont;
            number.fontStyle=FontStyles.Bold;number.alignment=TextAlignmentOptions.Center;number.margin=Vector4.zero;number.textWrappingMode=TextWrappingModes.NoWrap;
            var preview=Rect("Landing forecast",band,new Vector2(.35f,0),new Vector2(.98f,1));
            var hit=Background(preview,Color.clear,true);var button=preview.gameObject.AddComponent<Button>();button.targetGraphic=hit;
            button.onClick.AddListener(()=>OpenDevice(floor.LinkedPair.HasValue?6:1));
            var icon=Picture("Movement prop",preview,new Vector2(0,.16f),new Vector2(.24f,.84f),PlatformProp(floor.LinkedPair.HasValue?2:floor.EntryTransform==EntryTransform.MirrorNext?1:0),true);icon.material=ToolSilhouette;
            if(floor.EntryTransform==EntryTransform.ShiftLeftOne)icon.rectTransform.localScale=new Vector3(-1,1,1);
            var row=Rect("Animated exact row",preview,new Vector2(.30f,.10f),new Vector2(1,.90f));
            int count=Interaction.Attempt.Level.InitialSymbolIds.Count;var pieces=new RectTransform[count];var faces=new ShapeFaceGraphic[count];var glyphs=new TextMeshProUGUI[count];
            for(int i=0;i<count;i++){
                pieces[i]=Rect("Forecast piece "+i,row,new Vector2((i+.5f)/count,.5f),new Vector2((i+.5f)/count,.5f));pieces[i].sizeDelta=new Vector2(18,23);
                faces[i]=Rect("Shape",pieces[i],Vector2.zero,Vector2.one).gameObject.AddComponent<ShapeFaceGraphic>();faces[i].raycastTarget=false;
                glyphs[i]=Text("Letter",pieces[i],Vector2.zero,Vector2.one,18,"");glyphs[i].font=art.titleFont;FinishLetter(glyphs[i]);glyphs[i].raycastTarget=false;
            }
            var legend=band.gameObject.AddComponent<PlatformLegend>();legend.Initialize(this,index,floor,number,counter,preview,row,pieces,faces,glyphs,art);platformLegends.Add(legend);
        }
        void PaintPlatformLegends(PresentationFrame frame)
        {foreach(var legend in platformLegends)legend.Paint(frame);}
        void LayoutPlatformLegends(float unit)
        {foreach(var legend in platformLegends)legend.Layout(unit);}
        void DisposePlatformProps(){foreach(var sprite in platformProps)if(sprite!=null)Destroy(sprite);}
    }

    // A forecast animates symbols only. It never executes commands or mutates a PuzzleState.
    public sealed class PlatformLegend:MonoBehaviour
    {
        CascadeGame game;int floor;FloorDefinition definition;ToyArt art;
        TextMeshProUGUI number;Image counter;RectTransform preview,row;RectTransform[] pieces;ShapeFaceGraphic[] faces;TextMeshProUGUI[] glyphs;
        Vector2 counterRest;SymbolId[] symbols;int previous=-1;float time,pulse,unit=1;bool future;Color ink=new Color(.13f,.27f,.28f);
        public int Remaining {get;private set;}
        public bool ForecastVisible=>preview!=null&&preview.gameObject.activeSelf;
        public void Initialize(CascadeGame owner,int index,FloorDefinition data,TextMeshProUGUI label,Image plaque,RectTransform forecast,RectTransform lane,RectTransform[] items,ShapeFaceGraphic[] shapes,TextMeshProUGUI[] letters,ToyArt assets)
        {game=owner;floor=index;definition=data;number=label;counter=plaque;preview=forecast;row=lane;pieces=items;faces=shapes;glyphs=letters;art=assets;symbols=new SymbolId[items.Length];}
        public void Paint(PresentationFrame state)
        {
            bool past=!state.FloorVisible(floor);gameObject.SetActive(!past);if(past)return;
            Remaining=state.Status==PuzzleStatus.Playing&&state.ActiveFloor==floor?Math.Max(0,state.Durability):definition.BaseDurability;
            if(previous!=Remaining){pulse=previous<0?0:.42f;previous=Remaining;number.SetCopy(UiCopy.Raw(Remaining.ToString()));}
            number.color=Remaining<=1?new Color(.62f,.22f,.12f):ink;
            future=floor>state.ActiveFloor;
            bool show=(definition.EntryTransform!=EntryTransform.None&&floor>state.ActiveFloor)||(definition.LinkedPair.HasValue&&floor>=state.ActiveFloor);
            preview.gameObject.SetActive(show);
            for(int i=0;i<symbols.Length;i++){
                var symbol=state.Row[i];if(symbols[i]==symbol)continue;symbols[i]=symbol;
                bool glyph=symbol.Kind==SymbolKind.Glyph;faces[i].gameObject.SetActive(!glyph);glyphs[i].gameObject.SetActive(glyph);
                if(glyph)glyphs[i].text=symbol.Value;else faces[i].SetShape(art.SymbolMesh(symbol),art.SymbolMaterial(symbol).GetColor("_BaseColor"),true);
            }
            Animate();
        }
        public void Layout(float unit)
        {
            this.unit=unit;
            ((RectTransform)transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,34*unit);
            preview.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,44*unit);
            number.fontSize=18*unit;
            counterRest=Vector2.up*(.25f*((RectTransform)transform.parent).rect.height-12*unit);
            counter.rectTransform.anchoredPosition=counterRest;preview.anchoredPosition=counterRest+Vector2.up*(future?46*unit:0);
            for(int i=0;i<pieces.Length;i++){pieces[i].sizeDelta=new Vector2(Mathf.Min(20*unit,row.rect.width/pieces.Length*.86f),23*unit);glyphs[i].fontSize=18*unit;}
            Animate();
        }
        public void FollowWood(float sag,float fracture)
        {
            var offset=counterRest+Vector2.down*sag*.3f;
            counter.rectTransform.anchoredPosition=offset;preview.anchoredPosition=offset+Vector2.up*(future?46*unit:0);
            counter.gameObject.SetActive(fracture<.40f);
        }
        void Update()
        {
            if(game==null||game.Interaction==null||game.CurrentScreen!=GameScreen.Playing||game.Interaction.IsPaused||game.Interaction.IsSuspended)return;
            pulse=Mathf.Max(0,pulse-Time.unscaledDeltaTime);if(!game.ReducedMotion)time+=Time.unscaledDeltaTime;
            counter.rectTransform.localScale=Vector3.one*(1+(game.ReducedMotion?0:Mathf.Sin(pulse/.42f*Mathf.PI)*.08f));Animate();
        }
        void Animate()
        {
            if(!ForecastVisible)return;float phase=Mathf.Repeat(time,3.2f),progress=game.ReducedMotion?1:Mathf.SmoothStep(0,1,Mathf.InverseLerp(.75f,1.65f,phase));
            for(int i=0;i<pieces.Length;i++){
                int destination=i,n=pieces.Length;bool wrap=false;
                if(definition.EntryTransform==EntryTransform.ShiftLeftOne){destination=(i+n-1)%n;wrap=i==0;}
                else if(definition.EntryTransform==EntryTransform.ShiftRightOne){destination=(i+1)%n;wrap=i==n-1;}
                else if(definition.EntryTransform==EntryTransform.MirrorNext)destination=n-1-i;
                if(definition.LinkedPair.HasValue){var p=definition.LinkedPair.Value;if(i==p.FirstEdge||i==p.SecondEdge)destination=i+1;else if(i==p.FirstEdge+1||i==p.SecondEdge+1)destination=i-1;}
                float x=Mathf.Lerp((i+.5f)/n,(destination+.5f)/n,progress);pieces[i].anchorMin=pieces[i].anchorMax=new Vector2(x,.5f);
                pieces[i].anchoredPosition=new Vector2(0,game.ReducedMotion?0:Mathf.Sin(progress*Mathf.PI)*(wrap?10:3));
                pieces[i].localRotation=Quaternion.Euler(0,0,game.ReducedMotion?0:Mathf.Sin(progress*Mathf.PI)*(i%2==0?5:-5));
            }
        }
    }
}
