using System.Collections.Generic;
using UnityEngine;
namespace Cascade.Presentation
{
    // The whole reference keeps one scale; input margins grow without stretching the artwork.
    public sealed class TutorialReferenceLayout:MonoBehaviour
    {
        RectTransform safe,stage;Canvas canvas;public float Entrance{get;set;}=1;
        readonly List<KeyValuePair<RectTransform,Vector2>> touches=new List<KeyValuePair<RectTransform,Vector2>>();
        public void Initialize(RectTransform area){safe=area;stage=(RectTransform)transform;canvas=area.GetComponentInParent<Canvas>().rootCanvas;Fit();}
        public void BindTouch(RectTransform rect,Vector2 size){touches.Add(new KeyValuePair<RectTransform,Vector2>(rect,size));Fit();}
        public void Fit()
        {
            if(safe==null||stage==null||canvas==null)return;
            float scale=Mathf.Max(.001f,Mathf.Min(safe.rect.width/720,safe.rect.height/1280));stage.sizeDelta=new Vector2(720,1280);stage.localScale=Vector3.one*scale*Entrance;
            float minimum=44/Mathf.Max(.001f,scale*canvas.scaleFactor);
            foreach(var entry in touches)if(entry.Key!=null)entry.Key.sizeDelta=new Vector2(Mathf.Max(entry.Value.x,minimum),Mathf.Max(entry.Value.y,minimum));
        }
        void LateUpdate()=>Fit();
    }
}
