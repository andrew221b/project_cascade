using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // Camera-space scenery only. A shared presentation clock never drives puzzle rules.
    public sealed class LayeredWorld : MonoBehaviour
    {
        readonly List<Material> owned = new List<Material>();
        readonly Transform[] clouds = new Transform[3];
        Transform landscape, plantsLeft, plantsRight, glider;
        Camera cameraView;
        Material landscapeMaterial;
        readonly Texture2D[] backgrounds = new Texture2D[3];
        int zone;
        float flightBottom, flightTop;
        bool gameplayVisible, holdingToken;
        public float Phase { get; private set; }
        public int Zone => zone;
        public Vector3 CloudPosition => clouds[0]==null?Vector3.zero:clouds[0].position;
        public Vector3 GliderPosition => glider==null?Vector3.zero:glider.position;
        public Bounds GliderBounds => glider==null?default:glider.GetComponent<MeshRenderer>().bounds;
        public Vector2 FlightBand => new Vector2(flightBottom,flightTop);
        public bool Ready => landscape!=null;
        public void SetFlightBand(float bottom,float top) { flightBottom=bottom;flightTop=top; }
        public void SetGameplayVisible(bool visible) { gameplayVisible=visible; }
        public void SetTokenHeld(bool held) { holdingToken=held; }

        public void Initialize(ToyArt art,Camera camera)
        {
            cameraView=camera;
            string[] names={"WindGardenBase","RainMillBase","CopperOrchardBase"};
            for(int i=0;i<names.Length;i++)backgrounds[i]=Resources.Load<Texture2D>("CascadeArt/Journey/"+names[i]);
            if(backgrounds[0]==null)return;
            landscape=Plate("Far island sky",backgrounds[0],art.quad,false);
            landscapeMaterial=landscape.GetComponent<MeshRenderer>().sharedMaterial;
            var cloud=Resources.Load<Texture2D>("CascadeArt/Journey/Cloud");
            for(int i=0;i<clouds.Length;i++)clouds[i]=Plate("Moving cloud "+i,cloud,art.quad,true);
            var plants=Resources.Load<Texture2D>("CascadeArt/Journey/GardenPlants");
            plantsLeft=Plate("Near leaves left",plants,art.quad,true);plantsRight=Plate("Near leaves right",plants,art.quad,true);
            glider=Plate("Travelling glider",Resources.Load<Texture2D>("CascadeArt/Journey/Glider"),art.quad,true);
            Paint(0);
        }
        Transform Plate(string name,Texture texture,Mesh quad,bool transparent)
        {
            if(texture==null)return null;
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);
            go.GetComponent<MeshFilter>().sharedMesh=quad;
            var mat=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name=name+" material"};owned.Add(mat);
            mat.SetTexture("_BaseMap",texture);mat.SetColor("_BaseColor",Color.white);
            if(transparent) {
                mat.SetFloat("_Surface",1);mat.SetFloat("_Blend",0);mat.SetFloat("_ZWrite",0);
                mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_Cull",(float)CullMode.Off);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType","Transparent");mat.renderQueue=3000;
            }
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=mat;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return go.transform;
        }
        public void SetZone(int value)
        {
            zone=Mathf.Clamp(value,0,2);
            if(landscapeMaterial!=null)landscapeMaterial.SetTexture("_BaseMap",backgrounds[zone]??backgrounds[0]);
            var tint=zone==0?Color.white:zone==1?new Color(.77f,.92f,.95f):new Color(1,.85f,.67f);
            foreach(var cloud in clouds)if(cloud!=null)cloud.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor",tint);
            foreach(var plant in new[]{plantsLeft,plantsRight})if(plant!=null)plant.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor",tint);
        }
        public void Paint(float phase)
        {
            if(!Ready)return;Phase=phase;
            float h=cameraView.orthographicSize*2,w=h*cameraView.aspect;
            float cx=cameraView.transform.position.x,cy=cameraView.transform.position.y;
            var texture=landscapeMaterial.mainTexture;float ratio=(float)texture.width/texture.height;
            float coverH=Mathf.Max(h,w/ratio);
            landscape.position=new Vector3(cx,cy,3.2f);landscape.localScale=new Vector3(coverH*ratio,coverH,1);
            for(int i=0;i<clouds.Length;i++) {
                var cloud=clouds[i];if(cloud==null)continue;
                float cw=w*(i==0?.60f:i==1?.42f:.75f);
                float x=Mathf.Repeat(phase*(.017f+i*.009f)+i*.42f,1.7f)-.85f;
                cloud.position=new Vector3(cx+x*w,cy+h*(i==0?.29f:i==1?.43f:-.27f),2.9f-i*.05f);
                cloud.localScale=new Vector3(cw,cw*.5f,1);
            }
            Plants(plantsLeft,false);Plants(plantsRight,true);
            if(glider!=null) {
                float angle=phase*.07f;
                float band=flightTop-flightBottom;
                glider.gameObject.SetActive(!holdingToken&&(!gameplayVisible||band>0));
                float height=gameplayVisible?Mathf.Min(h*.05f,band*.5f):Mathf.Min(w,8)*.12f;
                float y=gameplayVisible?(flightBottom+flightTop)*.5f+Mathf.Sin(angle*.8f)*band*.08f:cy+h*(.255f+Mathf.Sin(angle*.8f)*.022f);
                glider.position=new Vector3(cx+Mathf.Sin(angle)*w*.34f,y,2.45f);
                // The whole rotated 3:2 illustration fits in the empty strip above the machine.
                glider.localScale=new Vector3(height*1.5f,height,1);glider.rotation=Quaternion.Euler(0,0,-6+Mathf.Sin(angle)*9);
            }
            void Plants(Transform plant,bool right)
            {
                if(plant==null)return;float size=Mathf.Min(w*.42f,h*.25f);
                var sway=Quaternion.Euler(0,0,Mathf.Sin(phase*.65f+(right?1:0))*1.6f);
                var stem=new Vector3(cx+w*(right?.43f:-.43f),cy-h*.43f-size*.5f,1.9f);
                plant.position=stem+sway*(Vector3.up*size*.5f);
                plant.localScale=new Vector3(size*(right?-1:1),size,1);
                plant.rotation=sway;
            }
        }
        void OnDestroy(){foreach(var material in owned)if(material!=null)Destroy(material);}
    }
}
