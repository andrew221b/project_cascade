using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // One frozen landed frame dissolves over the already committed result.
    // It never blocks controls, grants a reward or changes the gameplay state.
    public sealed class LevelResultTransition:MonoBehaviour
    {
        public const float Duration=.64f;
        CascadeGame game;RawImage image;RenderTexture snapshot;float elapsed;bool revealing;
        public float Opacity=>image==null?0:image.color.a;
        public bool Capture(CascadeGame owner,Camera camera,RectTransform canvas)
        {
            Settle();if(camera==null||canvas==null||owner.ReducedMotion)return false;
            var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
            try{
                int width=Mathf.Min(720,Mathf.Max(1,camera.pixelWidth));
                int height=Mathf.Max(1,Mathf.RoundToInt(camera.pixelHeight*width/(float)Mathf.Max(1,camera.pixelWidth)));
                snapshot=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){name="Landed level handoff",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                snapshot.Create();Canvas.ForceUpdateCanvases();camera.targetTexture=snapshot;camera.Render();
                var host=new GameObject("Landed level dissolve",typeof(RectTransform),typeof(Canvas),typeof(RawImage));
                host.transform.SetParent(canvas,false);var rect=(RectTransform)host.transform;
                rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                var layer=host.GetComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=35;
                image=host.GetComponent<RawImage>();image.texture=snapshot;image.raycastTarget=false;image.color=Color.white;
                game=owner;elapsed=0;return true;
            }catch(System.Exception error){Debug.LogWarning("Result transition capture unavailable: "+error.Message);Settle();return false;}
            finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;}
        }
        public void Reveal(){revealing=image!=null;}
        void Update()
        {
            if(!revealing)return;
            if(game==null||game.ReducedMotion||!game.IntroductionAnimationAllowed){Settle();return;}
            elapsed=Mathf.Min(Duration,elapsed+Mathf.Max(0,Time.unscaledDeltaTime));
            float t=elapsed/Duration,ease=t*t*t*(10+t*(-15+6*t));
            image.color=new Color(1,1,1,1-ease);
            if(elapsed>=Duration)Settle();
        }
        public void Settle()
        {
            revealing=false;elapsed=0;
            if(image!=null){image.gameObject.SetActive(false);Destroy(image.gameObject);image=null;}
            if(snapshot!=null){snapshot.Release();Destroy(snapshot);snapshot=null;}
        }
        void OnApplicationFocus(bool focused){if(!focused)Settle();}
        void OnApplicationPause(bool paused){if(paused)Settle();}
        void OnDisable()=>Settle();
        void OnDestroy()=>Settle();
    }
}
