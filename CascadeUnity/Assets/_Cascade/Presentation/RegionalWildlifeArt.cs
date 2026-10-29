using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    // Read-only source regions. The generated alpha atlas stays byte-identical to its original.
    internal static class RegionalWildlifeArt
    {
        public const string Path="CascadeArt/Atmosphere/RegionalRigOct15";
        static WildlifeRigAtlas.Cell Cell(float x,float y,float w,float h,float px,float py)
            =>new WildlifeRigAtlas.Cell(x,y,w,h,px,py,0,0,0,1,0,1,1,0,1);
        public static readonly WildlifeRigAtlas.Cell[] Cells={
            Cell(12,69,332,300,236,180),Cell(365,73,270,283,624,213),Cell(645,81,249,269,882,210),Cell(921,197,163,153,1074,244),
            Cell(28,387,315,383,220,560),Cell(365,387,278,362,632,557),Cell(655,389,255,351,898,555),Cell(929,569,175,182,1093,618),
            Cell(42,777,321,270,205,880),Cell(392,793,263,209,645,946),Cell(664,803,268,215,921,952),Cell(948,853,154,113,1007,944),
            Cell(27,1060,402,262,220,1200),Cell(422,1089,218,245,551,1103),Cell(660,1080,174,246,783,1100),Cell(929,1058,180,212,939,1250)
        };
        // Measured opaque toe tips in the existing body atlases, relative to each anatomy pivot.
        // Transparent cell padding and tail tips are not the supporting foot.
        internal static Vector2 PerchFeet(WildlifeSpecies species)
        {
            switch(species){
                case WildlifeSpecies.Swallow:return new Vector2(-0.133724f,-0.379497f);
                case WildlifeSpecies.Robin:return new Vector2(-0.106688f,-0.421576f);
                case WildlifeSpecies.Gull:return new Vector2(-0.183012f,-0.424670f);
                case WildlifeSpecies.SnowBunting:return new Vector2(-0.134653f,-0.419010f);
                case WildlifeSpecies.Goldfinch:return new Vector2(-0.096037f,-0.483299f);
                case WildlifeSpecies.Tern:return new Vector2(-0.192775f,-0.346608f);
                case WildlifeSpecies.Kingfisher:return new Vector2(-0.163816f,-0.408114f);
                case WildlifeSpecies.WinterWren:return new Vector2(-0.069498f,-0.430918f);
                case WildlifeSpecies.Bullfinch:return new Vector2(-0.046988f,-0.557229f);
                case WildlifeSpecies.BarnOwl:return new Vector2(-0.053968f,-0.650794f);
                default:return Vector2.zero;
            }
        }
        static readonly Dictionary<int,Sprite> sprites=new Dictionary<int,Sprite>();
        internal static WildlifeRigAtlas.Cell CellFor(WildlifeSpecies species,int part)
        {
            int regional=(int)species-(int)WildlifeSpecies.Bullfinch;
            if(regional>=0)return Cells[regional*4+part];
            bool bird=WildlifeMotion.IsBird(species);int column=!bird&&part==2?3:!bird&&part==3?2:part;
            return (bird?WildlifeRigAtlas.Birds:WildlifeRigAtlas.Insects)[(bird?(int)species:(int)species-8)*4+column];
        }
        public static Sprite Part(WildlifeSpecies species,int part=0)
        {
            int regional=(int)species-(int)WildlifeSpecies.Bullfinch,key=(int)species*4+part;
            if(sprites.TryGetValue(key,out var value)&&value!=null)return value;
            var cell=CellFor(species,part);
            var texture=Resources.Load<Texture2D>(regional>=0?Path:"CascadeArt/Atmosphere/"+(WildlifeMotion.IsBird(species)?"BirdRigOct08":"InsectRigOct08"));
            if(texture==null)return null;
            var box=cell.Pixels;var rect=new Rect(box.x,texture.height-box.yMax,box.width,box.height);
            return sprites[key]=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        }
    }
    public static class WorldFauna
    {
        static readonly WildlifeSpecies[] signatures={WildlifeSpecies.Bumblebee,WildlifeSpecies.Robin,WildlifeSpecies.Goldfinch,
            WildlifeSpecies.Tern,WildlifeSpecies.Kingfisher,WildlifeSpecies.WinterWren,WildlifeSpecies.Bullfinch,WildlifeSpecies.Gull,WildlifeSpecies.BarnOwl};
        public static WildlifeSpecies Signature(int world)=>signatures[Mathf.Clamp(world,0,8)];
        public static WildlifeSpecies Perched(int world,int visit,bool frame)
        {
            world=Mathf.Clamp(world,0,8);
            if(frame&&(world==1||world==5||world==8)&&(visit&1)==0)return WildlifeSpecies.FieldCricket;
            if(frame&&(world==0||world==2||world==4)&&(visit&1)==0)return WildlifeSpecies.Bumblebee;
            var species=Signature(world);
            return WildlifeMotion.IsBird(species)?species:WorldLifeView.BirdSpecies(world,(visit&1)!=0);
        }
    }
}
