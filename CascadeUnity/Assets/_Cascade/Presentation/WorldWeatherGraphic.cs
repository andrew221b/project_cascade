using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WorldWeatherGraphic:MaskableGraphic
    {
        CascadeGame game;bool snow,dust;float previous=-1;
        public void Initialize(CascadeGame owner,bool snowing){game=owner;snow=snowing;raycastTarget=false;color=snowing?new Color(1,.98f,.87f,.72f):new Color(.44f,.73f,.79f,.48f);}
        public void InitializeRegion(CascadeGame owner,int zone){Initialize(owner,zone==6);dust=zone!=1&&zone!=4&&zone!=6;if(dust)color=new Color(1,.82f,.48f,.5f);}
        void LateUpdate(){if(game==null)return;float t=game.ReducedMotion?0:game.Machine.AmbientPhase;if(t!=previous){previous=t;SetVerticesDirty();}}
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();var r=rectTransform.rect;float t=Mathf.Max(0,previous);
            for(int i=0;i<(game==null?10:game.Preferences.EffectQuality==0?6:game.Preferences.EffectQuality==1?10:16);i++) {
                float x=r.xMin+Mathf.Repeat(i*.618f+t*(dust?.003f:snow?.006f:.014f),1)*r.width;
                float y=r.yMin+Mathf.Repeat(i*.317f-t*(dust?.005f:snow?.024f:.11f),1)*r.height;
                float size=dust?1.3f:snow?1.7f:.8f,length=dust?1.3f:snow?1.7f:6;int v=helper.currentVertCount;
                helper.AddVert(new Vector3(x-size,y-length),color,Vector2.zero);helper.AddVert(new Vector3(x-size,y+length),color,Vector2.zero);
                helper.AddVert(new Vector3(x+size,y+length),color,Vector2.zero);helper.AddVert(new Vector3(x+size,y-length),color,Vector2.zero);
                helper.AddTriangle(v,v+1,v+2);helper.AddTriangle(v,v+2,v+3);
            }
        }
    }
}
