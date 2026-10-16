using UnityEngine;
namespace Cascade.Presentation
{
    public static class WorldFrameArt
    {
        public const string Path="CascadeArt/WorldFramesOct14/";
        public const float SillWidthRatio=180f/941f;
        public const float MapLandingViewportFraction=.075f;
        // Matches LayeredWorld's proportional nine-slice, including tablet viewports.
        public static float SillViewportHeight(float aspect)=>Mathf.Min(.48f,SillWidthRatio*aspect);
        public static Vector2 FrameContact(bool right,float seed,float aspect)
        {
            float v=180f/1672f,y=SillViewportHeight(aspect);
            // These are the upper painted edges of attached leaves, not arbitrary air beside a post.
            float authoredY=right?1-(seed<.5f?381:722)/1672f:1-(seed<.5f?656:1133)/1672f;
            float viewportY=y+(authoredY-v)/(1-2*v)*(1-2*y);
            return new Vector2(right?(seed<.5f?891:881)/941f:(seed<.5f?50:60)/941f,viewportY);
        }
        // Worlds reskin the first world's silhouettes; architecture and clear space stay fixed.
        public static Sprite MenuFrame()=>Resources.Load<Sprite>("CascadeArt/HomeOct13/FrameForeground");
        public static Sprite MapFrame()=>Resources.Load<Sprite>("CascadeArt/WorldMenusOct14/MapFrameMask");
        public static Texture2D Texture(int world)=>GameplayReferenceArt.Texture("GameplayFrame");
        public static Texture2D Accents()=>Resources.Load<Texture2D>(Path+"ReskinAccents");
        public static Sprite Bridge(int index)=>Resources.Load<Sprite>(Path+"Bridge"+index);
        public static readonly Vector2[] BridgeStart={new Vector2(.11218837f,.19688109f),new Vector2(.21696801f,.18164436f),new Vector2(.12460568f,.24168514f),new Vector2(.08265583f,.68196721f)};
        public static readonly Vector2[] BridgeEnd={new Vector2(.88781163f,.80116959f),new Vector2(.89151599f,.81644359f),new Vector2(.86593060f,.78270510f),new Vector2(.90108401f,.64590164f)};
    }
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public sealed class WorldFrameFit:UnityEngine.UI.BaseMeshEffect
    {
        RectTransform rect,canvas;UnityEngine.UI.Image face;readonly Vector3[] corners=new Vector3[4];
        float compactFloor;
        public void Initialize(float compactFloorFraction=0)
        {
            compactFloor=compactFloorFraction;
            rect=(RectTransform)transform;
            face=GetComponent<UnityEngine.UI.Image>();
            face.type=UnityEngine.UI.Image.Type.Sliced;
            face.pixelsPerUnitMultiplier=1;
            canvas=(RectTransform)GetComponentInParent<Canvas>().transform;
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=Vector2.zero;
            Fit();
            face.SetVerticesDirty();
        }
        float RemapHeight(float y)
        {
            if(compactFloor<=0||rect==null||face==null||face.sprite==null)return y;
            float height=rect.rect.height,ppu=face.pixelsPerUnit*face.pixelsPerUnitMultiplier;
            float bottom=face.sprite.border.y/Mathf.Max(.001f,ppu),top=face.sprite.border.w/Mathf.Max(.001f,ppu);
            float landing=Mathf.Min(bottom,height*compactFloor),middle=height-top-bottom;
            if(y<=bottom)return y*landing/Mathf.Max(.001f,bottom);
            if(y<height-top&&middle>0)return landing+(y-bottom)*(height-top-landing)/middle;
            return y;
        }
        public Vector2 RemapNormalizedContact(Vector2 point)
        {
            if(rect!=null&&rect.rect.height>0)point.y=RemapHeight(point.y*rect.rect.height)/rect.rect.height;
            return point;
        }
        public Vector2 ProjectSourcePoint(Vector2 point)
        {
            if(face==null||face.sprite==null||rect==null)return point;
            var sprite=face.sprite;float ppu=Mathf.Max(.001f,face.pixelsPerUnit*face.pixelsPerUnitMultiplier);
            float source=point.y*sprite.rect.height,bottom=sprite.border.y,top=sprite.border.w,height=rect.rect.height;
            float y=source<bottom?source/ppu:source>sprite.rect.height-top?height-(sprite.rect.height-source)/ppu
                :bottom/ppu+(source-bottom)*(height-(bottom+top)/ppu)/Mathf.Max(.001f,sprite.rect.height-bottom-top);
            point.y=RemapHeight(y)/Mathf.Max(.001f,height);return point;
        }
        public override void ModifyMesh(UnityEngine.UI.VertexHelper vertices)
        {
            if(!IsActive()||compactFloor<=0||rect==null)return;
            var vertex=new UIVertex();float bottom=rect.rect.yMin;
            for(int i=0;i<vertices.currentVertCount;i++){
                vertices.PopulateUIVertex(ref vertex,i);var position=vertex.position;
                position.y=bottom+RemapHeight(position.y-bottom);vertex.position=position;vertices.SetUIVertex(vertex,i);
            }
        }
        void Fit()
        {
            if(rect==null||rect.parent==null||canvas==null)return;
            // Decoration reaches the physical screen edges; safe-area insets only contain controls.
            canvas.GetWorldCorners(corners);var low=rect.parent.InverseTransformPoint(corners[0]);var high=rect.parent.InverseTransformPoint(corners[2]);
            rect.sizeDelta=new Vector2(high.x-low.x,high.y-low.y);rect.position=canvas.position;
            face.pixelsPerUnitMultiplier=face.sprite.rect.width/Mathf.Max(1,rect.rect.width);
        }
        void LateUpdate()=>Fit();
    }
}
