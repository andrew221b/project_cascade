using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // One capture on opening. No full-screen render or blur runs while the lesson is visible.
    [RequireComponent(typeof(RawImage))]
    public sealed class TutorialBackdrop:MonoBehaviour
    {
        RenderTexture snapshot;
        public void Initialize(RenderTexture texture){snapshot=texture;var image=GetComponent<RawImage>();image.texture=texture;image.color=Color.white;image.raycastTarget=false;}
        public static RenderTexture Capture(Camera camera,int quality)
        {
            if(camera==null||quality==0)return null;var shader=Resources.Load<Shader>("CascadeArt/TutorialReferenceOct16/BackdropBlur");if(shader==null||!shader.isSupported)return null;
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;RenderTexture full=null,a=null,b=null,result=null;Material material=null;
            try{
                int width=Mathf.Max(1,camera.pixelWidth),height=Mathf.Max(1,camera.pixelHeight),divisor=quality==1?4:3;
                full=RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.ARGB32);camera.targetTexture=full;camera.Render();camera.targetTexture=oldTarget;
                int w=Mathf.Max(1,width/divisor),h=Mathf.Max(1,height/divisor);a=RenderTexture.GetTemporary(w,h,0);b=RenderTexture.GetTemporary(w,h,0);material=new Material(shader);
                Graphics.Blit(full,a);material.SetVector("_Step",new Vector4(.6f/w,0,0,0));Graphics.Blit(a,b,material);material.SetVector("_Step",new Vector4(0,.6f/h,0,0));
                result=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32){name="Frozen tutorial backdrop",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};result.Create();Graphics.Blit(b,result,material);return result;
            }catch(System.Exception){if(result!=null){result.Release();Destroy(result);}return null;}
            finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;if(full!=null)RenderTexture.ReleaseTemporary(full);if(a!=null)RenderTexture.ReleaseTemporary(a);if(b!=null)RenderTexture.ReleaseTemporary(b);if(material!=null)Destroy(material);}
        }
        void LateUpdate(){
            if(snapshot==null)return;var image=GetComponent<RawImage>();var rect=image.rectTransform.rect;if(rect.width<=0||rect.height<=0)return;
            float source=snapshot.width/(float)snapshot.height,aspect=rect.width/rect.height;
            image.uvRect=aspect>source?new Rect(0,(1-source/aspect)*.5f,1,source/aspect):new Rect((1-aspect/source)*.5f,0,aspect/source,1);
        }
        void OnDestroy(){if(snapshot!=null){snapshot.Release();Destroy(snapshot);snapshot=null;}}
    }
}
