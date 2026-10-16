using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    // Content is one 720x1280 composition; only the transparent straight frame edges stretch.
    public sealed class SettingsReferenceLayout:MonoBehaviour
    {
        RectTransform surface,canvas,content,backdrop,sky,frame;
        UnityEngine.UI.Image frameImage;
        readonly List<(RectTransform Rect,Vector2 Size)> touches=new List<(RectTransform,Vector2)>();
        readonly RectTransform[] cords=new RectTransform[2];
        public void Initialize(RectTransform parent,RectTransform environment,RectTransform landscape,UnityEngine.UI.Image border)
        {
            surface=parent;content=(RectTransform)transform;backdrop=environment;sky=landscape;frame=border.rectTransform;frameImage=border;
            canvas=(RectTransform)parent.GetComponentInParent<Canvas>().transform;Fit();
        }
        public void BindCord(int index,RectTransform cord)=>cords[index]=cord;
        public void BindTouch(RectTransform rect,Vector2 size)=>touches.Add((rect,size));
        public void Fit()
        {
            if(surface==null||canvas==null)return;
            float scale=Mathf.Max(.001f,Mathf.Min(surface.rect.width/720f,surface.rect.height/1280f));
            content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);
            content.sizeDelta=new Vector2(720,1280);content.localScale=Vector3.one*scale;content.anchoredPosition=Vector2.zero;
            backdrop.sizeDelta=canvas.rect.size;backdrop.position=canvas.position;
            var sprite=sky.GetComponent<UnityEngine.UI.Image>().sprite;
            float cover=Mathf.Max(canvas.rect.width/sprite.rect.width,canvas.rect.height/sprite.rect.height);
            sky.sizeDelta=sprite.rect.size*cover;
            frame.sizeDelta=canvas.rect.size;frameImage.pixelsPerUnitMultiplier=frameImage.sprite.texture.width/Mathf.Max(1,canvas.rect.width);
            float pixelScale=scale*canvas.GetComponent<Canvas>().scaleFactor;
            foreach(var touch in touches)if(touch.Rect!=null)touch.Rect.sizeDelta=Vector2.Max(touch.Size,Vector2.one*(44/Mathf.Max(.001f,pixelScale)));
            // Continue the two rope shafts through a phone's safe top inset, never leaving cut ends in mid-air.
            Vector3 top=content.InverseTransformPoint(canvas.TransformPoint(new Vector3(0,canvas.rect.yMax,0)));
            float extra=Mathf.Max(0,top.y-content.rect.yMax+32);
            for(int i=0;i<2;i++)if(cords[i]!=null){cords[i].anchoredPosition=new Vector2(i==0?178:525,extra);cords[i].sizeDelta=new Vector2(16,extra+1);}
        }
        void LateUpdate()=>Fit();
    }
}
