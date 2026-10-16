using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // A compact reference composition fits the safe area. Extra height belongs to the sky;
    // the stage, controls and Mika's contact with the planks keep their source geometry.
    public sealed class HomeReferenceLayout:MonoBehaviour
    {
        RectTransform parent,rect,bleed,canvas,logo,noAds,account,welcomeSign,welcomeLogo;
        RectTransform[] welcomeCords;
        RawImage top,bottom,left,right,ground;
        readonly Vector3[] corners=new Vector3[4];
        readonly List<Material> materials=new List<Material>();
        readonly List<(RectTransform Rect,Vector2 Size)> touches=new List<(RectTransform,Vector2)>();
        public void Initialize(CascadeGame owner,RectTransform surface,RectTransform extensions,Texture texture)
        {
            parent=surface;rect=(RectTransform)transform;bleed=extensions;
            canvas=(RectTransform)surface.GetComponentInParent<Canvas>().transform;
            top=Edge("Extended top foliage",texture,new Rect(0,1-16/1280f,1,16/1280f));
            bottom=Edge("Extended boardwalk",texture,new Rect(0,0,1,20/1280f));
            left=Edge("Extended left timber",texture,new Rect(0,0,10/720f,1));
            right=Edge("Extended right timber",texture,new Rect(1-10/720f,0,10/720f,1));
            Fit();
        }
        RawImage Edge(string name,Texture texture,Rect uv)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(bleed,false);
            var image=go.GetComponent<RawImage>();image.texture=texture;image.uvRect=uv;image.raycastTarget=false;image.color=Color.white;
            return image;
        }
        public void BindTopArt(RectTransform title,RectTransform ads){logo=title;noAds=ads;}
        public void BindAccountShortcut(RectTransform button)=>account=button;
        public void BindWelcome(RectTransform sign,RectTransform title,RectTransform[] cords){welcomeSign=sign;welcomeLogo=title;welcomeCords=cords;}
        public void BindTouch(RectTransform target,Vector2 size)=>touches.Add((target,size));
        public void BindGround(Texture texture)
        {
            var go=new GameObject("Home continuous reference ground",typeof(RectTransform),typeof(RawImage),typeof(HomeGroundMesh));go.transform.SetParent(rect,false);
            ground=go.GetComponent<RawImage>();ground.texture=texture;ground.raycastTarget=false;Fit();
        }
        public void PlaceGroundAfter(Transform frame){if(ground!=null)ground.transform.SetSiblingIndex(frame.GetSiblingIndex()+1);}
        public void Own(Material material)=>materials.Add(material);
        public void Fit()
        {
            if(parent==null||rect==null)return;
            float scale=Mathf.Min(parent.rect.width/620f,parent.rect.height/1280f);
            rect.localScale=Vector3.one*Mathf.Max(.001f,scale);
            float height=parent.rect.height/Mathf.Max(.001f,scale);
            float width=Mathf.Min(720,parent.rect.width/Mathf.Max(.001f,scale));
            rect.sizeDelta=new Vector2(width,height);
            float extra=Mathf.Max(0,height-1280);
            bool compact=width<715;
            if(logo!=null){
                logo.sizeDelta=compact?new Vector2(430,256*430/455f):new Vector2(455,256);
                logo.anchoredPosition=new Vector2(0,-118-extra);
            }
            if(noAds!=null){
                noAds.anchorMin=noAds.anchorMax=Vector2.one;noAds.pivot=Vector2.one;
                // The shortcut shares the reference logo band, below the upper timber.
                noAds.sizeDelta=new Vector2(92,94);
                noAds.anchoredPosition=new Vector2(compact?-42:-49,-150-extra);
            }
            if(account!=null){account.anchorMin=account.anchorMax=new Vector2(0,1);account.pivot=new Vector2(0,1);account.sizeDelta=new Vector2(94,94);account.anchoredPosition=new Vector2(compact?42:66,-150-extra);}
            if(welcomeSign!=null)welcomeSign.anchoredPosition=new Vector2(0,-146-extra);
            if(welcomeLogo!=null)welcomeLogo.anchoredPosition=new Vector2(0,-280-extra);
            var physicalTop=rect.InverseTransformPoint(canvas.TransformPoint(new Vector3(0,canvas.rect.yMax,0)));
            float safeTopExtension=Mathf.Max(0,physicalTop.y-rect.rect.yMax);
            if(welcomeCords!=null)for(int i=0;i<2;i++){
                welcomeCords[i].sizeDelta=new Vector2(15,126+extra+safeTopExtension);welcomeCords[i].anchoredPosition=new Vector2((i==0?188f:536f)-360,safeTopExtension-40);
            }
            float pixels=scale*canvas.GetComponent<Canvas>().scaleFactor;
            foreach(var touch in touches)if(touch.Rect!=null)touch.Rect.sizeDelta=Vector2.Max(touch.Size,Vector2.one*(44/Mathf.Max(.001f,pixels)));
            bleed.anchorMin=bleed.anchorMax=new Vector2(.5f,.5f);bleed.sizeDelta=canvas.rect.size;bleed.position=canvas.position;
            if(ground!=null&&ground.texture!=null){
                var groundLow=rect.InverseTransformPoint(canvas.TransformPoint(canvas.rect.min));
                var groundHigh=rect.InverseTransformPoint(canvas.TransformPoint(canvas.rect.max));
                var area=ground.rectTransform;area.anchorMin=area.anchorMax=new Vector2(.5f,0);area.pivot=new Vector2(.5f,0);
                area.anchoredPosition=new Vector2((groundLow.x+groundHigh.x)*.5f,groundLow.y-rect.rect.yMin);
                area.sizeDelta=new Vector2(groundHigh.x-groundLow.x,Mathf.Max(0,rect.rect.yMin+382-groundLow.y));
                ground.uvRect=new Rect(0,0,1,1);
            }
            rect.GetWorldCorners(corners);
            var low=canvas.InverseTransformPoint(corners[0]);var high=canvas.InverseTransformPoint(corners[2]);
            float l=Mathf.Clamp01((low.x-canvas.rect.xMin)/canvas.rect.width);
            float r=Mathf.Clamp01((high.x-canvas.rect.xMin)/canvas.rect.width);
            float b=Mathf.Clamp01((low.y-canvas.rect.yMin)/canvas.rect.height);
            float t=Mathf.Clamp01((high.y-canvas.rect.yMin)/canvas.rect.height);
            Bounds(top,l,t,r,1);Bounds(bottom,l,0,r,b);Bounds(left,0,0,l,1);Bounds(right,r,0,1,1);
            float above=Mathf.Clamp((1-t)*canvas.rect.height/Mathf.Max(.001f,scale)/1280f,.001f,64/1280f);
            float below=Mathf.Clamp(b*canvas.rect.height/Mathf.Max(.001f,scale)/1280f,.001f,100/1280f);
            top.uvRect=new Rect(0,1,1,-above);bottom.uvRect=new Rect(0,below,1,-below);
        }
        static void Bounds(RawImage image,float x0,float y0,float x1,float y1)
        {
            var area=image.rectTransform;area.anchorMin=new Vector2(x0,y0);area.anchorMax=new Vector2(x1,y1);area.offsetMin=area.offsetMax=Vector2.zero;
        }
        void LateUpdate()=>Fit();
        void OnDestroy(){foreach(var material in materials)if(material!=null)Destroy(material);}
    }
}
