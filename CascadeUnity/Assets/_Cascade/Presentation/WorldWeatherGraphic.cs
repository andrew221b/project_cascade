using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WorldWeatherGraphic:MaskableGraphic
    {
        CascadeGame game;bool snow,dust;float previous=-1;int previousQuality=-1;
        public void Initialize(CascadeGame owner,bool snowing){game=owner;snow=snowing;raycastTarget=false;color=snowing?new Color(1,.98f,.87f,.62f):new Color(.44f,.73f,.79f,.34f);}
        public void InitializeRegion(CascadeGame owner,int zone){Initialize(owner,zone==6);dust=zone!=1&&zone!=4&&zone!=6;if(dust)color=new Color(1,.82f,.48f,.38f);}
        void LateUpdate()
        {
            if(game==null)return;
            float t=game.ReducedMotion?0:Mathf.Floor(game.Machine.AmbientPhase*30)/30;
            if(t!=previous||previousQuality!=game.Preferences.EffectQuality){previous=t;previousQuality=game.Preferences.EffectQuality;SetVerticesDirty();}
        }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();var r=rectTransform.rect;float t=Mathf.Max(0,previous);
            int quality=game==null?1:game.Preferences.EffectQuality;
            for(int i=0;i<(quality==0?0:quality==1?8:16);i++) {
                float u=Mathf.Repeat(i*.618f+t*(dust?.003f:snow?.004f:.012f),1);
                float v=Mathf.Repeat(i*.317f-t*(dust?.004f:snow?.018f:.09f),1);
                float x=r.xMin+u*r.width+Mathf.Sin(t*.35f+i)*3,y=r.yMin+v*r.height;
                float size=dust?2.4f:snow?2.8f:1.2f,length=dust?2.4f:snow?2.8f:8;
                var tint=color;tint.a*=Mathf.SmoothStep(0,1,Mathf.Min(u,1-u)/.1f)*Mathf.SmoothStep(0,1,Mathf.Min(v,1-v)/.1f);
                int start=helper.currentVertCount;helper.AddVert(new Vector3(x,y),tint,Vector2.zero);tint.a=0;
                const int sides=10;
                for(int edge=0;edge<sides;edge++){
                    float angle=edge*Mathf.PI*2/sides;
                    helper.AddVert(new Vector3(x+Mathf.Cos(angle)*size,y+Mathf.Sin(angle)*length),tint,Vector2.zero);
                }
                for(int edge=0;edge<sides;edge++)helper.AddTriangle(start,start+1+edge,start+1+(edge+1)%sides);
            }
        }
    }
}
