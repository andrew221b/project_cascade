using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Cascade.Presentation
{
    public static class FailureReferenceArt
    {
        static readonly Rect[] regions={new Rect(19,932,426,134),new Rect(456,899,394,220),new Rect(860,944,376,102),new Rect(42,434,360,373),new Rect(447,432,360,375),new Rect(849,432,362,375),new Rect(20,141,501,178),new Rect(538,158,312,142),new Rect(877,127,356,213),new Rect(476,1040,57,81),new Rect(770,1040,57,81)};
        static readonly Sprite[] sprites=new Sprite[11];
        static readonly Sprite[] sprigs=new Sprite[9];
        static readonly Sprite[] referenceSprites=new Sprite[8];
        static readonly Rect[] referenceRegions={new Rect(40,463,451,471),new Rect(512,640,511,80),new Rect(1049,602,151,160),new Rect(1202,602,148,161),new Rect(1354,602,148,161),new Rect(42,165,530,155),new Rect(618,163,416,169),new Rect(1158,128,293,221)};
        static Sprite hero;
        static readonly Vector2[] heroOutline={new Vector2(326,311),new Vector2(343,314),new Vector2(357,330),new Vector2(363,337),new Vector2(380,330),new Vector2(391,318),new Vector2(393,329),new Vector2(408,315),new Vector2(403,330),new Vector2(418,322),new Vector2(414,336),new Vector2(433,330),new Vector2(441,347),new Vector2(463,340),new Vector2(479,344),new Vector2(490,356),new Vector2(493,380),new Vector2(484,402),new Vector2(469,414),new Vector2(455,420),new Vector2(456,431),new Vector2(460,445),new Vector2(451,461),new Vector2(444,477),new Vector2(440,486),new Vector2(451,508),new Vector2(453,538),new Vector2(449,557),new Vector2(451,567),new Vector2(447,579),new Vector2(449,602),new Vector2(440,612),new Vector2(416,613),new Vector2(401,607),new Vector2(394,614),new Vector2(363,613),new Vector2(347,606),new Vector2(345,590),new Vector2(347,580),new Vector2(340,567),new Vector2(325,562),new Vector2(294,562),new Vector2(272,559),new Vector2(255,551),new Vector2(244,545),new Vector2(239,537),new Vector2(236,530),new Vector2(212,525),new Vector2(201,520),new Vector2(195,515),new Vector2(191,505),new Vector2(188,495),new Vector2(183,482),new Vector2(186,461),new Vector2(197,445),new Vector2(218,434),new Vector2(246,432),new Vector2(262,439),new Vector2(275,449),new Vector2(280,437),new Vector2(273,428),new Vector2(283,425),new Vector2(269,418),new Vector2(283,414),new Vector2(307,417),new Vector2(314,400),new Vector2(315,392),new Vector2(298,389),new Vector2(288,371),new Vector2(287,349),new Vector2(297,329),new Vector2(313,315)};
        public static Rect HeroBounds=>new Rect(183,311,310,303);
        public static Vector2[] HeroContour=>heroOutline;
        public static Vector2[] MedalContour(int index){float x=index==0?211:index==1?361:510;var points=new List<Vector2>{new Vector2(x,634),new Vector2(x+10,646)};for(int i=1;i<40;i++){float angle=-Mathf.PI*.5f+i*Mathf.PI*2/40;points.Add(new Vector2(x+51*Mathf.Cos(angle),695+51*Mathf.Sin(angle)));}points.Add(new Vector2(x-10,646));return points.ToArray();}
        public static Sprite Hero(){if(hero==null)hero=PhotoSprite(heroOutline,"Exact reference Mika");return hero;}
        public static Sprite Get(int index)
        {
            if(index==2||index>=6&&index<=8)return Reference(index==2?1:index-1);
            if(index>=3&&index<=5){if(sprites[index]==null){float x=index==3?211:index==4?361:510;var points=new List<Vector2>();points.Add(new Vector2(x,634));points.Add(new Vector2(x+10,646));for(int i=1;i<40;i++){float angle=-Mathf.PI*.5f+i*Mathf.PI*2/40;points.Add(new Vector2(x+51*Mathf.Cos(angle),695+51*Mathf.Sin(angle)));}points.Add(new Vector2(x-10,646));sprites[index]=PhotoSprite(points.ToArray(),"Exact silver medal "+index);}return sprites[index];}
            if(sprites[index]==null){var texture=GameplayReferenceArt.Texture("FailureAtlas");sprites[index]=Sprite.Create(texture,regions[index],new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprites[index].name="Failure reference asset "+index;}
            return sprites[index];
        }
        static Sprite Reference(int index){if(referenceSprites[index]==null){var texture=GameplayReferenceArt.Texture("FailureReferenceAtlas");referenceSprites[index]=Sprite.Create(texture,referenceRegions[index],new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);}return referenceSprites[index];}
        static Sprite PhotoSprite(Vector2[] points,string name)
        {
            var texture=GameplayReferenceArt.Texture("FailureReferenceSource");var region=Rect.MinMaxRect(points.Min(p=>p.x),texture.height-points.Max(p=>p.y),points.Max(p=>p.x),texture.height-points.Min(p=>p.y));
            var sprite=Sprite.Create(texture,region,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name=name;return sprite;
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        public static ushort[] Triangulate(Vector2[] p)
        {
            float area=0;for(int i=0;i<p.Length;i++)area+=Cross(p[i],p[(i+1)%p.Length]);float sign=area>0?1:-1;
            var remaining=Enumerable.Range(0,p.Length).ToList();var triangles=new List<ushort>();
            while(remaining.Count>2){bool removed=false;for(int i=0;i<remaining.Count;i++){int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];if(Cross(p[b]-p[a],p[c]-p[b])*sign<=.000001f)continue;bool inside=false;foreach(int v in remaining){if(v==a||v==b||v==c)continue;if(Cross(p[b]-p[a],p[v]-p[a])*sign>=0&&Cross(p[c]-p[b],p[v]-p[b])*sign>=0&&Cross(p[a]-p[c],p[v]-p[c])*sign>=0){inside=true;break;}}if(inside)continue;triangles.Add((ushort)a);triangles.Add((ushort)b);triangles.Add((ushort)c);remaining.RemoveAt(i);removed=true;break;}if(!removed)throw new System.InvalidOperationException("Reference contour cannot be triangulated");}
            return triangles.ToArray();
        }
        public static Sprite Sprig(int world)
        {
            if(world==0)return Resources.Load<Sprite>("CascadeArt/MapReferenceOct14/Daisies");
            if(sprigs[world]==null){var texture=WorldFrameArt.Accents();int cell=world-1;float w=texture.width/4f,h=texture.height/2f;sprigs[world]=Sprite.Create(texture,new Rect(cell%4*w,(1-cell/4)*h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);}
            return sprigs[world];
        }
    }
    // Force contour geometry even when a skin/layout system requests a sliced Image.
    public sealed class ReferenceCutoutImage:UnityEngine.UI.Image
    {
        Vector2[] points,uv;ushort[] triangles;
        public void SetContour(Vector2[] pixels){
            float left=pixels.Min(p=>p.x),right=pixels.Max(p=>p.x),top=pixels.Min(p=>p.y),bottom=pixels.Max(p=>p.y);
            points=pixels.Select(p=>new Vector2((p.x-left)/(right-left),1-(p.y-top)/(bottom-top))).ToArray();
            uv=pixels.Select(p=>new Vector2(p.x/sprite.texture.width,1-p.y/sprite.texture.height)).ToArray();triangles=FailureReferenceArt.Triangulate(points);raycastTarget=false;SetVerticesDirty();
        }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh){
            mesh.Clear();if(points==null)return;var rect=GetPixelAdjustedRect();
            for(int i=0;i<points.Length;i++)mesh.AddVert(new Vector3(rect.xMin+points[i].x*rect.width,rect.yMin+points[i].y*rect.height),color,uv[i]);
            for(int i=0;i<triangles.Length;i+=3)mesh.AddTriangle(triangles[i],triangles[i+1],triangles[i+2]);
        }
    }
}
