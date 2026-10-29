using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
namespace Cascade.Presentation
{
    // Readability is present on frame one; navigation never waits for the fade.
    public sealed class MenuTransition:MonoBehaviour
    {
        sealed class Entrance
        {
            public RectTransform Rect;public Quaternion Rotation;public CanvasGroup Group;public float Alpha=1,Delay;
            public ButtonFeel Button;
        }
        readonly List<Entrance> layers=new List<Entrance>(24);
        CanvasGroup group;CascadeGame game;RectTransform page;Vector2 rest;Tween arrival;float progress;bool fromRight,move;
        public bool IsArriving=>arrival!=null&&arrival.IsActive();
        public void Initialize(CascadeGame owner,bool movePage=true){game=owner;move=movePage;page=(RectTransform)transform;rest=page.anchoredPosition;group=gameObject.AddComponent<CanvasGroup>();}
        public void Arrive(bool reverse=false)
        {
            Settle();if(game==null||game.ReducedMotion||!game.IntroductionAnimationAllowed)return;
            fromRight=reverse;if(move)BindLayers();progress=0;Paint();
            float duration=move?(game.CurrentScreen==GameScreen.Prologue?.32f:.42f):.22f;
            arrival=DOTween.To(()=>progress,value=>{progress=value;Paint();},1,duration).SetEase(Ease.Linear).SetUpdate(true).SetTarget(this).OnComplete(Settle);
        }
        void BindLayers()
        {
            // The architectural frame, sky and planted character contacts never move with the UI.
            // One heading group keeps its board and lettering together; button art has one scale owner.
            var all=page.GetComponentsInChildren<RectTransform>();RectTransform heading=null;
            foreach(var rect in all)if(rect.name=="Animated heading entrance"){heading=rect;break;}
            if(heading==null){
                var candidates=new List<RectTransform>();
                foreach(var rect in all){
                    string name=rect.name.ToLowerInvariant();
                    float height=page.InverseTransformPoint(rect.TransformPoint(rect.rect.center)).y-page.rect.yMin;
                    if(height>page.rect.height*.68f&&(name=="navigation heading"||name=="reference map aligned header"||name=="flying ladders illustrated logo"||name=="welcome hanging sign"||name.Contains("header board")||name.Contains("title board")||name.Contains("hanging title")||name.EndsWith(" heading")||name.EndsWith(" title"))){
                        bool nested=false;for(var parent=rect.parent;parent!=null&&parent!=page;parent=parent.parent)if(candidates.Contains(parent as RectTransform)){nested=true;break;}
                        if(!nested)candidates.Add(rect);
                    }
                }
                if(candidates.Count>0){
                    var host=candidates[0].parent;var go=new GameObject("Animated heading entrance",typeof(RectTransform));heading=(RectTransform)go.transform;heading.SetParent(host,false);
                    heading.anchorMin=Vector2.zero;heading.anchorMax=Vector2.one;heading.pivot=new Vector2(.5f,.91f);heading.offsetMin=heading.offsetMax=Vector2.zero;
                    heading.SetSiblingIndex(candidates[0].GetSiblingIndex());foreach(var rect in candidates)if(rect.parent==host)rect.SetParent(heading,true);
                }
            }
            if(heading!=null){var opacity=heading.GetComponent<CanvasGroup>();if(opacity==null)opacity=heading.gameObject.AddComponent<CanvasGroup>();layers.Add(new Entrance{Rect=heading,Rotation=heading.localRotation,Group=opacity,Alpha=opacity.alpha});}
            int index=0;
            foreach(var button in page.GetComponentsInChildren<ButtonFeel>()){
                var opacity=button.GetComponent<CanvasGroup>();if(opacity==null)opacity=button.gameObject.AddComponent<CanvasGroup>();
                layers.Add(new Entrance{Rect=(RectTransform)button.transform,Button=button,Group=opacity,Alpha=opacity.alpha,Delay=Mathf.Min(.16f,index++*.018f)});
            }
        }
        void Paint()
        {
            if(group==null||page==null)return;float fade=1-Mathf.Pow(1-progress,3);group.alpha=Mathf.Lerp(.86f,1,fade);
            foreach(var layer in layers){
                if(layer.Rect==null)continue;float p=Mathf.Clamp01((progress-layer.Delay)/(1-layer.Delay));
                float settled=1-Mathf.Pow(1-p,3);layer.Group.alpha=layer.Alpha*Mathf.Lerp(.88f,1,settled);
                if(layer.Button!=null)layer.Button.SetEntrance(1-.035f*Mathf.Exp(-8*p)*Mathf.Cos(10*p));
                else layer.Rect.localRotation=layer.Rotation*Quaternion.Euler(0,0,(fromRight?-1:1)*1.25f*Mathf.Exp(-8*p)*Mathf.Cos(10*p));
            }
        }
        void Update()
        {
            if(arrival==null||!arrival.IsActive())return;
            if(game.ReducedMotion){Settle();return;}
            if(!game.IntroductionAnimationAllowed)Settle();
        }
        void OnApplicationFocus(bool focused){if(!focused)Settle();}
        void OnApplicationPause(bool paused){if(paused)Settle();}
        public void Settle()
        {
            arrival?.Kill();arrival=null;progress=1;if(group!=null)group.alpha=1;if(move&&page!=null)page.anchoredPosition=rest;
            foreach(var layer in layers){if(layer.Rect==null)continue;if(layer.Group!=null)layer.Group.alpha=layer.Alpha;if(layer.Button!=null)layer.Button.SetEntrance(1);else layer.Rect.localRotation=layer.Rotation;}
            layers.Clear();
        }
        void OnDisable()=>Settle();
        void OnDestroy()=>Settle();
    }
}
