using System;
using System.Collections.Generic;
using Cascade.Application;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    internal static class AmbientAtlasCells
    {
        const int Gutter=12;
        static readonly Dictionary<int,Rect[]> rectCache=new Dictionary<int,Rect[]>();
        public static Sprite[] Create(Texture2D texture)
        {
            if(texture==null)return null;
            var bounds=Bounds(texture);var result=new Sprite[bounds.Length];
            for(int i=0;i<result.Length;i++){
                result[i]=Sprite.Create(texture,bounds[i],new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                result[i].name="Ambient atlas icon "+i;
            }
            return result;
        }
        static Rect[] Bounds(Texture2D texture)
        {
            int id=texture.GetInstanceID();if(rectCache.TryGetValue(id,out var cached))return cached;
            var bounds=new Rect[8];Color32[] pixels=texture.isReadable?texture.GetPixels32():null;
            int width=texture.width/4,height=texture.height/2;
            for(int i=0;i<bounds.Length;i++){
                int x=(i%4)*width,y=(1-i/4)*height;
                var crop=new Rect(x+Gutter,y+Gutter,Mathf.Max(1,width-Gutter*2),Mathf.Max(1,height-Gutter*2));
                if(pixels!=null){
                    int left=x+width,right=x,bottom=y+height,top=y;
                    for(int py=y+Gutter;py<y+height-Gutter;py++)for(int px=x+Gutter;px<x+width-Gutter;px++)
                        if(pixels[py*texture.width+px].a>18){left=Math.Min(left,px);right=Math.Max(right,px);bottom=Math.Min(bottom,py);top=Math.Max(top,py);}
                    if(right>=left&&top>=bottom){
                        left=Math.Max(x+Gutter,left-Gutter);right=Math.Min(x+width-Gutter-1,right+Gutter);
                        bottom=Math.Max(y+Gutter,bottom-Gutter);top=Math.Min(y+height-Gutter-1,top+Gutter);
                        crop=new Rect(left,bottom,right-left+1,top-bottom+1);
                    }
                }
                bounds[i]=crop;
            }
            rectCache[id]=bounds;return bounds;
        }
    }

    // Ten reused decorative objects at most. This clock never reads wind strength,
    // emits gameplay events or changes the equipped trophy or puzzle state.
    public sealed class TrophyAmbiance:MonoBehaviour
    {
        const int Capacity=10;
        sealed class Fleck {
            public RectTransform Rect;public Image Image;public float Seed;
            public Vector2 Position,Velocity;public float Angle,Spin;public int Cycle;public bool DriftReady;
        }
        readonly Fleck[] flecks=new Fleck[Capacity];
        Sprite[] sprites;
        CascadeGame game;
        RectTransform root;
        CanvasGroup group;
        Canvas canvas;
        bool preview;
        int kind=-1,lastCount=-1;
        float clock,motionStep;
        Vector2 lastSize;
        float lastUnit;
        bool hasPainted;
        public int VisibleCount { get; private set; }

        public void Configure(CascadeGame owner,string trophyId,bool isPreview)
        {
            if(root==null){
                root=(RectTransform)transform;
                group=gameObject.AddComponent<CanvasGroup>();group.interactable=false;group.blocksRaycasts=false;
                canvas=GetComponentInParent<Canvas>();
            }
            game=owner;
            // A cleared equipment slot must remove its emitter from the hierarchy.
            // Hiding just the particles leaves the updater alive and makes stale
            // foliage reappear after the next canvas/layout refresh.
            if(string.IsNullOrEmpty(trophyId)){
                kind=-1;preview=isPreview;VisibleCount=0;lastCount=0;hasPainted=false;
                if(group!=null)group.alpha=0;
                if(flecks!=null)for(int i=0;i<flecks.Length;i++)if(flecks[i]?.Image!=null)flecks[i].Image.enabled=false;
                enabled=false;gameObject.SetActive(false);return;
            }
            gameObject.SetActive(true);enabled=true;
            int next=Array.IndexOf(ShopCatalog.MikaTrophyIds,trophyId);
            if(next>=16)next=next==17?1:next==18?8:next==19?2:next==20?4:next==21?3:0;
            else if(next>=8)next=next==8?2:next==9||next==14?3:next==10||next==15?4:next==13?5:0;
            if(ShopCatalog.IsScenery(trophyId))next=trophyId=="petal_drift"||trophyId=="butterfly_garden"?4:trophyId=="firefly_dance"?7:trophyId=="swallow_flock"?2:trophyId=="dragonfly_stream"?5:trophyId=="lantern_flight"?1:trophyId=="snow_wings"?8:9;
            int scenery=CascadeGame.ScreenSceneryFamily(trophyId);if(scenery>=0)next=10+scenery;
            bool changed=next!=kind||preview!=isPreview;
            kind=next;preview=isPreview;
            if(kind>=0&&sprites==null)LoadSprites();
            if(changed){clock=0;hasPainted=false;}
            if(!changed&&hasPainted){Paint(false);return;}
            if(sprites!=null&&kind>=0){
                for(int i=0;i<Capacity;i++){
                    if(flecks[i]==null){
                        var go=new GameObject("Trophy flourish "+(i+1),typeof(RectTransform),typeof(Image));
                        var rect=(RectTransform)go.transform;rect.SetParent(root,false);
                        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
                        var image=go.GetComponent<Image>();image.raycastTarget=false;image.preserveAspect=true;
                        image.canvasRenderer.cullTransparentMesh=true;
                        flecks[i]=new Fleck{Rect=rect,Image=image,Seed=Mathf.Repeat(i*.618034f+.13f,1)};
                    }
                    flecks[i].Image.sprite=kind>=10?game.ScreenSceneryPose(kind-10,i%4):sprites[kind];
                    if(changed)flecks[i].DriftReady=false;
                }
            }
            Paint(true);
        }
        void LoadSprites()
        {
            var texture=Resources.Load<Texture2D>("CascadeArt/Cosmetics/AmbientAtlas");
            if(texture==null)return;
            var cells=AmbientAtlasCells.Create(texture);sprites=new Sprite[10];Array.Copy(cells,sprites,cells.Length);
            for(int i=0;i<cells.Length;i++)sprites[i].name="Trophy ambient "+ShopCatalog.TrophyIds[i];
            sprites[8]=Sprite.Create(texture,sprites[3].rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.Tight);
            sprites[9]=Sprite.Create(texture,sprites[3].rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.Tight);
        }
        void LateUpdate()
        {
            if(game==null||root==null)return;
            motionStep=!game.ReducedMotion&&game.CosmeticAmbianceMotionAllowed?Mathf.Min(Time.unscaledDeltaTime,.05f):0;
            clock+=motionStep;
            Paint(false);
        }
        void DriftInWind(Fleck fleck,float seconds,out float x,out float y,out float angle)
        {
            if(!fleck.DriftReady){
                fleck.Position=new Vector2(.04f+fleck.Seed*.92f,.06f+Mathf.Repeat(fleck.Seed*3.7f,1)*.94f);
                fleck.Velocity=new Vector2(0,-.035f);fleck.Angle=fleck.Seed*90-45;fleck.DriftReady=true;
            }
            // Drag, gravity and a shared changing air current affect presentation only.
            float gust=.06f*Mathf.Sin(clock*.27f)+.095f*Mathf.Sin(clock*.71f)+.035f*Mathf.Sin(clock*1.47f);
            float flutter=.018f*Mathf.Sin(clock*1.9f+fleck.Seed*8);
            float weight=kind==8?.55f:.75f+fleck.Seed*.4f;
            fleck.Velocity+=new Vector2((gust+flutter-fleck.Velocity.x)*1.3f,-.085f*weight-fleck.Velocity.y*1.55f)*seconds;
            fleck.Position+=fleck.Velocity*seconds;
            fleck.Spin=Mathf.Lerp(fleck.Spin,gust*180+Mathf.Sin(clock*1.6f+fleck.Seed*7)*24,1-Mathf.Exp(-seconds*2));
            fleck.Angle+=fleck.Spin*seconds;
            // Recycle only after the whole sprite has left the screen.
            if(fleck.Position.y<-.14f||fleck.Position.x<-.14f||fleck.Position.x>1.14f){
                fleck.Cycle++;fleck.Position=new Vector2(.04f+Mathf.Repeat(fleck.Seed+fleck.Cycle*.381966f,1)*.92f,1.14f);
                fleck.Velocity=new Vector2(gust,-.035f);fleck.Angle=fleck.Seed*90-45;
            }
            x=fleck.Position.x;y=fleck.Position.y;angle=fleck.Angle;
        }
        void Paint(bool force)
        {
            if(game==null||root==null||group==null)return;
            int quality=game.Preferences.EffectQuality;
            int count=kind<0||sprites==null||quality==0?0:game.ReducedMotion?(preview?2:3):preview?(quality==1?4:6):quality==1?6:Capacity;
            if(count>0&&kind>=10){
                count=kind==10?preview?2:quality==1?2:4:kind==11?preview?2:quality==1?1:3:kind==12?preview?4:quality==1?4:8:preview?2:quality==1?2:4;
                if(game.ReducedMotion)count=Mathf.Min(count,kind==12?3:2);
                if(game.ScreenSceneryPose(kind-10,0)==null)count=0;
            }
            VisibleCount=count;
            if(lastCount!=count){
                group.alpha=count==0?0:1;
                for(int i=0;i<Capacity;i++)if(flecks[i]!=null)flecks[i].Image.enabled=i<count;
                lastCount=count;force=true;
            }
            if(count==0)return;
            var size=root.rect.size;
            float unit=canvas!=null?1/Mathf.Min(1,canvas.scaleFactor):1;
            bool resize=lastSize!=size||!Mathf.Approximately(lastUnit,unit);
            bool frozen=game.ReducedMotion||!game.CosmeticAmbianceMotionAllowed;
            if(frozen&&hasPainted&&!force&&!resize)return;
            if(kind>=10){PaintScenery(count,size,unit);lastSize=size;lastUnit=unit;hasPainted=true;return;}
            for(int i=0;i<count;i++){
                var fleck=flecks[i];float seed=fleck.Seed;
                float duration=kind==9?10+i%3:kind==5?11+i%3:18+(i%4)*2;
                float progress=Mathf.Repeat(clock/duration+seed,1);
                // Side lanes reserve the centre for Mika, symbols, text and touch targets.
                float lane=preview?(i%2==0?.105f:.895f):(i%2==0?.04f+.05f*(i%3):.96f-.05f*(i%3));
                float x=lane,y,angle=0,flutter=1;
                if(game.ReducedMotion){y=.20f+(i%4)*.19f;}
                else if(kind==0||kind==4||kind==6||kind==8){
                    DriftInWind(fleck,force?0:motionStep,out x,out y,out angle);
                    flutter=.90f+.10f*Mathf.Sin(clock*1.4f+seed*8);
                }
                else {
                    y=1.06f-progress*1.12f;
                    float wave=Mathf.Sin(progress*Mathf.PI*2+seed*7);
                    x+=wave*(preview?.038f:.022f);
                    if(kind==1){y=-.10f+progress*1.20f;angle=wave*4;}
                    else if(kind==2){angle=wave*18;flutter=.68f+.32f*Mathf.Abs(Mathf.Sin(clock*.75f+seed*7));}
                    else if(kind==3){y=.15f+seed*.68f+Mathf.Sin(clock*.22f+seed*8)*.014f;}
                    else if(kind==5){x=lane+.025f*(1-progress);angle=8;}
                    else if(kind==7){y=-.10f+progress*1.20f;x+=Mathf.Sin(clock*.35f+seed*8)*.012f;angle=wave*6;}
                    else if(kind==9){x+=.035f*(1-progress);angle=25;}
                }
                if(resize||force){
                    float height=(preview?17:11)+(i%3)*2;
                    if(kind==1||kind==3||kind==7||kind==9)height*=.82f;
                    if(kind==8)height*=.44f;
                    if(kind==5)height*=.72f;
                    var sprite=sprites[kind];float aspect=sprite.rect.width/sprite.rect.height;
                    fleck.Rect.sizeDelta=new Vector2(height*Mathf.Clamp(aspect,.45f,1.8f),height)*unit;
                }
                fleck.Rect.anchoredPosition=Vector2.Scale(new Vector2(x-.5f,y-.5f),size);
                fleck.Rect.localRotation=Quaternion.Euler(0,0,angle);
                float breathe=game.ReducedMotion?1:kind==1||kind==3||kind==7?.92f+.08f*Mathf.Sin(clock*.65f+seed*7):1;
                fleck.Rect.localScale=new Vector3(flutter*breathe,breathe,1);
                fleck.Image.canvasRenderer.SetAlpha(1);
                fleck.Image.color=kind==8?new Color(.86f,.96f,1):Color.white;
            }
            lastSize=size;lastUnit=unit;hasPainted=true;
        }
        void PaintScenery(int count,Vector2 size,float unit)
        {
            int family=kind-10;var maximum=CascadeGame.ScreenSceneryMaximumSize(family);
            for(int i=0;i<count;i++){
                var fleck=flecks[i];float seed=fleck.Seed;
                float progress=Mathf.Repeat(clock/(family==1?19+i*2:family==2?23+i%3*3:35)+seed,1);
                int frame=family==1&&!game.ReducedMotion?Mathf.FloorToInt(Mathf.Repeat(clock*.8f+seed,1)*4):i%4;
                var sprite=game.ScreenSceneryPose(family,frame);fleck.Image.sprite=sprite;
                float side=i%2==0?-1:1;
                float x=preview?(side<0?.13f:.87f):(side<0?.04f:.96f),y=.18f+seed*.64f;
                float wave=game.ReducedMotion?0:Mathf.Sin(clock*.36f+seed*8),angle=0,breathe=1;
                float height=family==0?preview?28:42:family==1?preview?21:32:family==2?preview?19:16+i%3*5:preview?52:112+i%2*24;
                if(!game.ReducedMotion){
                    if(family==0){y+=wave*.036f;x+=Mathf.Sin(clock*.21f+seed*9)*(preview?.02f:.008f);angle=wave*8;}
                    else if(family==1){y=1.08f-progress*1.16f;x+=Mathf.Sin(progress*Mathf.PI*2+seed*8)*(preview?.018f:.006f);angle=-side*(6+wave*5);}
                    else if(family==2){y=-.06f+progress*1.12f;x+=Mathf.Sin(progress*6+seed*8)*(preview?.025f:.008f);breathe=1+wave*.045f;}
                    else{y=.28f+i/2*.42f+wave*.013f;x+=Mathf.Sin(clock*.16f+seed*8)*(preview?.015f:.004f);angle=wave*3;breathe=1+wave*.026f;}
                }
                // Reserve a fixed outer gutter for the largest authored pose, including
                // its bank and breathing. No element can sweep through puzzle controls.
                float scale=Mathf.Min(height*unit/maximum.y,size.x*(preview?.17f:.060f)/maximum.x);
                fleck.Rect.sizeDelta=sprite.rect.size*scale;
                fleck.Rect.localRotation=Quaternion.Euler(0,0,angle);
                fleck.Rect.localScale=new Vector3(breathe,breathe,1);
                var body=Vector2.Scale(new Vector2(x-.5f,y-.5f),size);
                fleck.Rect.anchoredPosition=body+(Vector2)(fleck.Rect.localRotation*(Vector3)(CascadeGame.ScreenSceneryBodyOffset(family,frame)*scale));
                fleck.Image.canvasRenderer.SetAlpha(1);
                fleck.Image.color=Color.white;
            }
        }
        void OnDestroy(){if(sprites!=null)foreach(var sprite in sprites)if(sprite!=null)Destroy(sprite);}
    }
}
