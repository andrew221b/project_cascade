using System;
using Cascade.Application;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // Ten reused authored sprites at most. This clock never reads wind strength,
    // emits gameplay events or changes the equipped trophy or puzzle state.
    public sealed class TrophyAmbiance:MonoBehaviour
    {
        const int Capacity=10;
        sealed class Fleck { public RectTransform Rect;public Image Image;public float Seed; }
        readonly Fleck[] flecks=new Fleck[Capacity];
        Sprite[] sprites;
        CascadeGame game;
        RectTransform root;
        CanvasGroup group;
        Canvas canvas;
        bool preview;
        int kind=-1,lastCount=-1;
        float clock;
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
            int next=Array.IndexOf(ShopCatalog.TrophyIds,trophyId);
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
                    flecks[i].Image.sprite=sprites[kind];
                }
            }
            Paint(true);
        }
        void LoadSprites()
        {
            var texture=Resources.Load<Texture2D>("CascadeArt/Cosmetics/AmbientAtlas");
            if(texture==null)return;
            sprites=new Sprite[8];
            // Atlas order is left-to-right, top row first. Trim only when the
            // imported texture permits it; unreadable art still keeps its cell gutter.
            Color32[] pixels=texture.isReadable?texture.GetPixels32():null;
            int width=texture.width/4,height=texture.height/2;
            for(int i=0;i<8;i++){
                int x=(i%4)*width,y=(1-i/4)*height;
                var bounds=new Rect(x+2,y+2,Mathf.Max(1,width-4),Mathf.Max(1,height-4));
                if(pixels!=null){
                    int left=x+width,right=x,bottom=y+height,top=y;
                    for(int py=y+2;py<y+height-2;py++)for(int px=x+2;px<x+width-2;px++)
                        if(pixels[py*texture.width+px].a>18){left=Math.Min(left,px);right=Math.Max(right,px);bottom=Math.Min(bottom,py);top=Math.Max(top,py);}
                    if(right>=left&&top>=bottom){left=Math.Max(x+2,left-2);right=Math.Min(x+width-3,right+2);bottom=Math.Max(y+2,bottom-2);top=Math.Min(y+height-3,top+2);bounds=new Rect(left,bottom,right-left+1,top-bottom+1);}
                }
                sprites[i]=Sprite.Create(texture,bounds,new Vector2(.5f,.5f),100,0,SpriteMeshType.Tight);
                sprites[i].name="Trophy ambient "+ShopCatalog.TrophyIds[i];
            }
        }
        void LateUpdate()
        {
            if(game==null||root==null)return;
            if(!game.ReducedMotion&&game.CosmeticAmbianceMotionAllowed)clock+=Mathf.Min(Time.unscaledDeltaTime,.08f);
            Paint(false);
        }
        void Paint(bool force)
        {
            if(game==null||root==null||group==null)return;
            int quality=game.Preferences.EffectQuality;
            int count=kind<0||sprites==null||quality==0?0:game.ReducedMotion?(preview?2:3):preview?(quality==1?4:6):quality==1?6:Capacity;
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
            for(int i=0;i<count;i++){
                var fleck=flecks[i];float seed=fleck.Seed;
                float duration=kind==5?11+i%3:18+(i%4)*2;
                float progress=Mathf.Repeat(clock/duration+seed,1);
                // Side lanes reserve the centre for Mika, symbols, text and touch targets.
                float lane=preview?(i%2==0?.105f:.895f):(i%2==0?.04f+.05f*(i%3):.96f-.05f*(i%3));
                float x=lane,y,angle=0,flutter=1;
                if(game.ReducedMotion){y=.20f+(i%4)*.19f;}
                else {
                    y=1.06f-progress*1.12f;
                    float wave=Mathf.Sin(progress*Mathf.PI*2+seed*7);
                    x+=wave*(preview?.038f:.022f);
                    if(kind==0||kind==6)angle=seed*40-20+wave*28;
                    else if(kind==1){y=.10f+progress*.82f;angle=wave*4;}
                    else if(kind==2){angle=wave*18;flutter=.68f+.32f*Mathf.Abs(Mathf.Sin(clock*.75f+seed*7));}
                    else if(kind==3){y=.15f+seed*.68f+Mathf.Sin(clock*.22f+seed*8)*.014f;}
                    else if(kind==4){y=.08f+progress*.85f;angle=wave*28;}
                    else if(kind==5){x=lane+.025f*(1-progress);angle=8;}
                    else if(kind==7){y=.10f+progress*.78f;x+=Mathf.Sin(clock*.35f+seed*8)*.012f;angle=wave*6;}
                }
                if(resize||force){
                    float height=(preview?17:11)+(i%3)*2;
                    if(kind==1||kind==3||kind==7)height*=.82f;
                    if(kind==5)height*=.72f;
                    var sprite=sprites[kind];float aspect=sprite.rect.width/sprite.rect.height;
                    fleck.Rect.sizeDelta=new Vector2(height*Mathf.Clamp(aspect,.45f,1.8f),height)*unit;
                }
                fleck.Rect.anchoredPosition=Vector2.Scale(new Vector2(x-.5f,y-.5f),size);
                fleck.Rect.localRotation=Quaternion.Euler(0,0,angle);
                float breathe=game.ReducedMotion?1:kind==1||kind==3||kind==7?.92f+.08f*Mathf.Sin(clock*.65f+seed*7):1;
                fleck.Rect.localScale=new Vector3(flutter*breathe,breathe,1);
                float envelope=game.ReducedMotion?1:kind==3?.66f+.26f*Mathf.Sin(clock*.7f+seed*8):Mathf.SmoothStep(0,1,Mathf.Min(progress/.12f,(1-progress)/.12f));
                float alpha=(preview?.78f:.40f)*(kind==5?.65f:1)*envelope;
                fleck.Image.canvasRenderer.SetAlpha(alpha);
            }
            lastSize=size;lastUnit=unit;hasPainted=true;
        }
        void OnDestroy(){if(sprites!=null)foreach(var sprite in sprites)if(sprite!=null)Destroy(sprite);}
    }
}
