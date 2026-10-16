using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Application;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int trophyCategory;
        readonly List<Sprite> screenCosmeticSprites=new List<Sprite>();
        readonly Dictionary<string,Sprite> cosmeticPreviewSkies=new Dictionary<string,Sprite>();
        Sprite[][] visitorPoses;
        Sprite[][] sceneryPoses;
        Sprite[] cosmeticFrames,cosmeticCorners,ambientIcons;
        ScreenCosmeticFrame equippedFrameView;
        AttachedPlants gameplayFramePlants;
        static readonly string[] SceneryNames={"Petal breeze","Firefly dance","Butterfly garden","Swallow flock","Dragonfly stream","Flying lanterns","Snow wings","Comet trails","Ribbon kites","Paper cranes","Dream bubbles","Aurora veil"};
        static readonly string[] FrameNames={"Garden frame","Sky frame","Copper frame","Lunar frame"};
        static readonly string[] SceneryBenefits={"Petals and garden visitors","Soft firefly lights","Butterflies and drifting pollen","Birds in the distance","Dragonflies over the islands","Slow floating lanterns","Snowflakes and winter birds","Falling stars and moon moths","Kites with trailing ribbons","Folded wings in the breeze","Pearl bubbles drifting upward","Soft curtains of northern light"};
        static readonly string[] FrameBenefits={"Leaves around the game screen","Feathers around the game screen","Copper around the game screen","Moonlight around the game screen"};
        static readonly Rect[] FrameCells={new Rect(14,310,606,252),new Rect(638,306,604,261),new Rect(13,714,605,240),new Rect(638,705,603,258)};
        // Measured bounds in the preserved 1254-square source. Unequal row heights
        // and fixed crane torso pivots keep every pose clear of its neighbours.
        static readonly Rect[] SceneryCells={
            new Rect(96,15,163,297),new Rect(406,15,161,297),new Rect(718,15,164,297),new Rect(1028,15,168,297),
            new Rect(42,348,261,233),new Rect(355,396,259,185),new Rect(669,396,256,183),new Rect(971,396,260,185),
            new Rect(40,641,241,237),new Rect(352,643,241,235),new Rect(661,643,244,237),new Rect(973,643,243,236),
            new Rect(105,909,144,336),new Rect(415,909,145,336),new Rect(729,909,146,337),new Rect(1028,910,138,336)
        };
        static readonly Vector2[] CraneBodyPivots={new Vector2(188,550),new Vector2(501,550),new Vector2(815,550),new Vector2(1124,550)};
        string[] CurrentTrophyGroup=>CurrentScreen==GameScreen.Shop
            ?ShopCatalog.TrophyIds.Where(id=>!ShopCatalog.IsParcelExclusive(id)).ToArray()
            :ShopCatalog.TrophyIds.Where(OwnsGift).ToArray();
        internal string EquippedScenery=>Rewards.EquippedScenery;
        internal string EquippedFrame=>Rewards.EquippedFrame;
        UiCopy TrophyTitle(string id){int i=Array.IndexOf(ShopCatalog.SceneryTrophyIds,id);if(i>=0)return SceneryNames[i];i=Array.IndexOf(ShopCatalog.FrameTrophyIds,id);return i>=0?(UiCopy)FrameNames[i]:GiftNames[Array.IndexOf(ShopCatalog.MikaTrophyIds,id)];}
        UiCopy TrophyBenefit(string id){int i=Array.IndexOf(ShopCatalog.SceneryTrophyIds,id);if(i>=0)return SceneryBenefits[i];i=Array.IndexOf(ShopCatalog.FrameTrophyIds,id);return i>=0?(UiCopy)FrameBenefits[i]:TrophyBenefits[Array.IndexOf(ShopCatalog.MikaTrophyIds,id)];}
        bool TrophyEquipped(string id)=>ShopCatalog.IsScenery(id)?EquippedScenery==id:ShopCatalog.IsFrame(id)?EquippedFrame==id:EquippedGift==id;
        public void SelectTrophyCategory(int category){trophyCategory=Mathf.Clamp(category,0,2);shopItem=Array.IndexOf(ShopCatalog.TrophyIds,(trophyCategory==1?ShopCatalog.SceneryTrophyIds:trophyCategory==2?ShopCatalog.FrameTrophyIds:ShopCatalog.MikaTrophyIds)[0]);if(CurrentScreen==GameScreen.Collection)giftPage=shopItem;shopNotice=default;DrawMenu();}
        void LoadScreenCosmeticArt()
        {
            if(visitorPoses!=null)return;
            visitorPoses=new Sprite[9][];
            var texture=Resources.Load<Texture2D>("CascadeArt/Atmosphere/WorldVisitorsOct08");
            for(int row=0;row<9;row++){
                visitorPoses[row]=new Sprite[4];
                for(int pose=0;pose<4;pose++)if(texture!=null){var source=WorldVisitorAtlas.Cells[row*4+pose].Pixels;source.y=texture.height-source.yMax;var sprite=Sprite.Create(texture,source,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);visitorPoses[row][pose]=sprite;screenCosmeticSprites.Add(sprite);}
            }
            sceneryPoses=new Sprite[4][];texture=Resources.Load<Texture2D>("CascadeArt/Cosmetics/SceneryAtlasOct08");
            for(int row=0;row<4;row++){
                sceneryPoses[row]=new Sprite[4];
                for(int pose=0;pose<4;pose++)if(texture!=null){var source=SceneryCells[row*4+pose];source.y=texture.height-source.yMax;var sprite=Sprite.Create(texture,source,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name="Screen scenery "+row+" pose "+pose;sceneryPoses[row][pose]=sprite;screenCosmeticSprites.Add(sprite);}
            }
            cosmeticFrames=new Sprite[4];cosmeticCorners=new Sprite[4];texture=Resources.Load<Texture2D>("CascadeArt/Cosmetics/ScreenFramesOct08");
            for(int i=0;i<4;i++)if(texture!=null){
                var r=FrameCells[i];r.y=texture.height-r.yMax;
                cosmeticFrames[i]=Sprite.Create(texture,r,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(148,76,148,76));screenCosmeticSprites.Add(cosmeticFrames[i]);
                var corner=new Rect(r.x,r.yMax-FrameCells[i].height*.52f,FrameCells[i].width*.24f,FrameCells[i].height*.52f);
                cosmeticCorners[i]=Sprite.Create(texture,corner,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);screenCosmeticSprites.Add(cosmeticCorners[i]);
            }
        }
        internal Sprite VisitorPose(int row,int frame){LoadScreenCosmeticArt();return visitorPoses[Mathf.Clamp(row,0,8)][Mathf.Clamp(frame,0,3)];}
        internal static int ScreenSceneryFamily(string id)=>id=="ribbon_kites"?0:id=="origami_cranes"?1:id=="bubble_dream"?2:id=="aurora_veil"?3:-1;
        internal Sprite ScreenSceneryPose(int row,int frame){LoadScreenCosmeticArt();return sceneryPoses[Mathf.Clamp(row,0,3)][Mathf.Clamp(frame,0,3)];}
        internal static Vector2 ScreenSceneryMaximumSize(int row){var size=Vector2.zero;for(int i=0;i<4;i++){var cell=SceneryCells[Mathf.Clamp(row,0,3)*4+i];size=Vector2.Max(size,cell.size);}return size;}
        internal static Vector2 ScreenSceneryBodyOffset(int row,int frame){if(row!=1)return Vector2.zero;var cell=SceneryCells[4+Mathf.Clamp(frame,0,3)];var pivot=CraneBodyPivots[Mathf.Clamp(frame,0,3)];return new Vector2(cell.center.x-pivot.x,pivot.y-cell.center.y);}
        internal Sprite CosmeticFrameArt(string id,bool corner=false){int i=Array.IndexOf(ShopCatalog.FrameTrophyIds,id);if(i<0)return null;LoadScreenCosmeticArt();return corner?cosmeticCorners[i]:cosmeticFrames[i];}
        static int SceneryVisitorRow(string id)=>id=="firefly_dance"?7:id=="butterfly_garden"?5:id=="swallow_flock"?3:id=="dragonfly_stream"?1:id=="lantern_flight"?5:id=="snow_wings"?6:id=="comet_trails"?8:0;
        Sprite ScreenAmbientIcon(int index){
            if(ambientIcons==null){var texture=Resources.Load<Texture2D>("CascadeArt/Cosmetics/AmbientAtlas");ambientIcons=AmbientAtlasCells.Create(texture);if(ambientIcons!=null)screenCosmeticSprites.AddRange(ambientIcons);}
            return ambientIcons!=null&&index>=0&&index<ambientIcons.Length?ambientIcons[index]:null;
        }
        Sprite ScreenTrophyArt(string id)=>ScreenSceneryFamily(id)>=0?ScreenSceneryPose(ScreenSceneryFamily(id),0):ShopCatalog.IsFrame(id)?CosmeticFrameArt(id):id=="lantern_flight"?ScreenAmbientIcon(1):id=="petal_drift"?ScreenAmbientIcon(4):id=="firefly_dance"?ScreenAmbientIcon(7):id=="comet_trails"?ScreenAmbientIcon(3):VisitorPose(SceneryVisitorRow(id),0);
        internal Vector2 VisitorBodyOffset(int row,int frame){int i=row*4+frame;var source=WorldVisitorAtlas.Cells[i];var cell=source.Pixels;return new Vector2(cell.center.x-source.BodyPivot.x,source.BodyPivot.y-cell.center.y);}
        bool EquipScreenTrophy(string id)
        {
            if(GameEconomy==null||saveBlocked||!OwnsGift(id))return false;
            bool changed=ShopCatalog.IsScenery(id)?GameEconomy.TryEquipScenery(EquippedScenery==id?null:id,Rewards):GameEconomy.TryEquipFrame(EquippedFrame==id?null:id,Rewards);
            if(!changed){shopNotice="Could not save. Nothing changed.";return false;}
            CreateGlobalCosmeticAmbiance();feedback.Sound(ToyCue.Select);DrawMenu();return true;
        }
        void UpdateScreenCosmetics()
        {
            if(machine?.LivingWorld==null||profile==null||safeRoot==null)return;
            if(gameplayFramePlants==null){var plants=Rect("Game frame attached plants",safeRoot.parent,Vector2.zero,Vector2.one);plants.SetSiblingIndex(safeRoot.GetSiblingIndex());gameplayFramePlants=plants.gameObject.AddComponent<AttachedPlants>();gameplayFramePlants.Initialize(this,machine.LivingWorld.Zone,false,true);}
            gameplayFramePlants.SetWorld(machine.LivingWorld.Zone);gameplayFramePlants.gameObject.SetActive(CurrentScreen==GameScreen.Playing&&!WorldSeasonalArt.Rooted(machine.LivingWorld.Zone));
            machine.LivingWorld.CosmeticScenery=EquippedScenery;
            machine.LivingWorld.CosmeticIsland=Mathf.Clamp(selectedJourneyStop%5,0,4);
            machine.LivingWorld.CosmeticStage=Mathf.Max(0,selectedJourneyStop*3+(descent?.StageIndex??0));
            if(equippedFrameView==null&&safeRoot!=null){var layer=Rect("Equipped game frame",safeRoot,Vector2.zero,Vector2.one);layer.SetAsFirstSibling();equippedFrameView=layer.gameObject.AddComponent<ScreenCosmeticFrame>();}
            equippedFrameView?.Configure(this,EquippedFrame,CurrentScreen==GameScreen.Playing);
            machine.SetReceiverFrame(CosmeticFrameArt(EquippedFrame));
        }
        Image DrawScreenTrophyPreview(RectTransform parent,string id,bool showEffect)
        {
            var scene=Background(parent,new Color(.40f,.76f,.82f),false);scene.raycastTarget=false;
            string asset=id=="lunar_frame"||id=="comet_trails"||id=="firefly_dance"||id=="aurora_veil"?"Woodland/StarObservatoryBase":id=="bubble_dream"?"Woodland/GlassLagoonBase":id=="snow_wings"?"Woodland/SnowSpireBase":id=="copper_frame"?"Journey/CopperOrchardBase":id=="sky_frame"||id=="swallow_flock"||id=="origami_cranes"||id=="ribbon_kites"?"Woodland/CloudPostBase":"Journey/WindGardenBase";
            if(!cosmeticPreviewSkies.TryGetValue(asset,out var sky)){
                var texture=Resources.Load<Texture2D>("CascadeArt/"+asset);if(texture!=null){sky=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);cosmeticPreviewSkies[asset]=sky;screenCosmeticSprites.Add(sky);}
            }
            if(sky!=null){var background=Picture("Miniature sky",parent,Vector2.zero,Vector2.one,sky,false);var fit=background.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=sky.rect.width/sky.rect.height;}
            var clip=parent.gameObject.AddComponent<RectMask2D>();clip.padding=Vector4.zero;
            var root=Rect("Miniature game",parent,Vector2.zero,Vector2.one);
            Image focus;
            if(ShopCatalog.IsFrame(id)){
                focus=Picture("Frame preview",root,new Vector2(.02f,.25f),new Vector2(.98f,.67f),CosmeticFrameArt(id),false);focus.type=Image.Type.Sliced;focus.pixelsPerUnitMultiplier=4;
                for(int i=0;i<3;i++){var shape=Rect("Preview goal "+i,root,new Vector2(.24f+i*.19f,.37f),new Vector2(.37f+i*.19f,.55f));var face=shape.gameObject.AddComponent<ShapeFaceGraphic>();face.SetShape(art.shapes[i],art.colors[i].GetColor("_BaseColor"),true);face.raycastTarget=false;}
            }else{
                if(showEffect)CosmeticAmbiance(root,id,true);
                focus=Picture("Visitor preview",root,new Vector2(.14f,.22f),new Vector2(.86f,.78f),ScreenTrophyArt(id),true);
                focus.gameObject.AddComponent<ScreenVisitorPreview>().Initialize(this,focus,id,SceneryVisitorRow(id));
            }
            return focus;
        }
        void DisposeScreenCosmetics(){foreach(var sprite in screenCosmeticSprites)if(sprite!=null)Destroy(sprite);screenCosmeticSprites.Clear();}
    }
}
