using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform landscapeGate;
        RawImage landscapeSky;
        RectTransform landscapeCard;
        TextMeshProUGUI landscapeTitle,landscapeCaption;
        bool landscapeLayoutBlocked;

        // The authored game is portrait only. A resized desktop view or an OS that
        // ignores the orientation lock gets a complete, legible screen instead of
        // tiny interactive puzzle pieces and a distorted 1280-pixel composition.
        void UpdateLandscapeLayoutGate(RectTransform canvasRect)
        {
            bool wide=canvasRect.rect.width>canvasRect.rect.height*1.12f;
            if(!wide&&landscapeGate==null)return;
            if(landscapeGate==null)BuildLandscapeGate(canvasRect);
            landscapeGate.gameObject.SetActive(wide);
            landscapeLayoutBlocked=wide;
            bool suspended=nativeAdSuspended||applicationPaused||!applicationFocused||wide;
            if(Interaction!=null&&Interaction.IsSuspended!=suspended)Interaction.SetSuspended(suspended);
            if(!wide)return;
            var sky=MenuWorldArt.Sky(EnvironmentWorld);
            if(landscapeSky.texture!=sky){landscapeSky.texture=sky;}
            if(sky!=null){float screenAspect=canvasRect.rect.width/canvasRect.rect.height,skyAspect=sky.width/(float)sky.height;
                landscapeSky.uvRect=screenAspect>skyAspect?new Rect(0,(1-skyAspect/screenAspect)*.5f,1,skyAspect/screenAspect):new Rect((1-screenAspect/skyAspect)*.5f,0,screenAspect/skyAspect,1);}
            float height=canvasRect.rect.height;
            landscapeCard.sizeDelta=new Vector2(Mathf.Min(canvasRect.rect.width*.76f,height*1.38f),height*.56f);
            string language=Preferences?.UiLocale??"en";
            landscapeTitle.text=language=="ru"?"Поверните устройство":language=="uk"?"Поверніть пристрій":"Rotate your device";
            landscapeCaption.text=language=="ru"?"Продолжим на вертикальном экране":language=="uk"?"Продовжимо на вертикальному екрані":"Continue in portrait view";
        }

        void BuildLandscapeGate(RectTransform parent)
        {
            landscapeGate=Rect("Portrait game layout",parent,Vector2.zero,Vector2.one);
            landscapeGate.SetAsLastSibling();
            var overlay=landscapeGate.gameObject.AddComponent<Canvas>();overlay.overrideSorting=true;overlay.sortingOrder=300;
            landscapeGate.gameObject.AddComponent<GraphicRaycaster>();
            landscapeSky=landscapeGate.gameObject.AddComponent<RawImage>();landscapeSky.raycastTarget=true;
            var shade=Rect("Quiet sky shade",landscapeGate,Vector2.zero,Vector2.one);
            Background(shade,new Color(.05f,.13f,.18f,.39f),false);
            landscapeCard=Rect("Turn device wooden card",landscapeGate,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            landscapeCard.pivot=new Vector2(.5f,.5f);landscapeCard.sizeDelta=new Vector2(600,270);
            var panel=Background(landscapeCard,Color.white,false);panel.sprite=CleanMenuArt.Get("Card");panel.type=Image.Type.Sliced;
            var phone=Rect("Portrait phone silhouette",landscapeCard,new Vector2(.12f,.23f),new Vector2(.28f,.78f));
            phone.localRotation=Quaternion.Euler(0,0,-18);
            var rim=Background(phone,new Color(.39f,.20f,.07f),false);rim.type=Image.Type.Sliced;
            var glass=Rect("Portrait phone screen",phone,new Vector2(.13f,.10f),new Vector2(.87f,.88f));
            Background(glass,new Color(.08f,.53f,.57f),false);
            var arrow=Rect("Turn device glyph",landscapeCard,new Vector2(.27f,.31f),new Vector2(.38f,.68f)).gameObject.AddComponent<RotateDeviceMark>();
            arrow.color=new Color(.27f,.12f,.04f);arrow.raycastTarget=false;
            landscapeTitle=Text("Turn device title",landscapeCard,new Vector2(.36f,.42f),new Vector2(.94f,.74f),37,UiCopy.Raw(""));
            landscapeTitle.font=art.titleFont;landscapeTitle.fontStyle=FontStyles.Bold;landscapeTitle.enableAutoSizing=true;landscapeTitle.fontSizeMin=23;landscapeTitle.fontSizeMax=37;landscapeTitle.color=new Color(.25f,.09f,.035f);
            landscapeCaption=Text("Turn device caption",landscapeCard,new Vector2(.37f,.23f),new Vector2(.93f,.45f),24,UiCopy.Raw(""));
            landscapeCaption.enableAutoSizing=true;landscapeCaption.fontSizeMin=17;landscapeCaption.fontSizeMax=24;landscapeCaption.color=new Color(.27f,.14f,.08f);
        }
    }
    public sealed class RotateDeviceMark:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;float radius=Mathf.Min(r.width,r.height)*.40f;
            const int count=16;float start=-1.45f,end=2.45f;
            for(int i=0;i<=count;i++){
                float angle=Mathf.Lerp(start,end,i/(float)count);var unit=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                mesh.AddVert(r.center+unit*radius,color,Vector2.zero);
                mesh.AddVert(r.center+unit*(radius*.75f),color,Vector2.zero);
                if(i>0){int n=(i-1)*2;mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n+1,n+3,n+2);}
            }
            var tip=r.center+new Vector2(Mathf.Cos(end),Mathf.Sin(end))*radius;
            var tangent=new Vector2(-Mathf.Sin(end),Mathf.Cos(end));var outward=new Vector2(Mathf.Cos(end),Mathf.Sin(end));int k=mesh.currentVertCount;
            mesh.AddVert(tip+tangent*radius*.29f,color,Vector2.zero);
            mesh.AddVert(tip-tangent*radius*.17f+outward*radius*.22f,color,Vector2.zero);
            mesh.AddVert(tip-tangent*radius*.17f-outward*radius*.22f,color,Vector2.zero);
            mesh.AddTriangle(k,k+1,k+2);
        }
    }
}
