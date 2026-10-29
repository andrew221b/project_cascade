using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class ToyFeedbackPool
    {
        struct TimberVertex
        {
            public Vector3 point,normal;public Vector2 uv;
            public static TimberVertex Lerp(TimberVertex a,TimberVertex b,float t)=>new TimberVertex{point=Vector3.LerpUnclamped(a.point,b.point,t),normal=Vector3.LerpUnclamped(a.normal,b.normal,t).normalized,uv=Vector2.LerpUnclamped(a.uv,b.uv,t)};
        }
        // Cut the currently painted, bent timber into staggered diagonal slivers.
        // All cuts share boundaries: the first fragment frame covers the original board.
        public void ReleaseTimber(MeshFilter face,Vector3 velocity,float spin)
        {
            if(particles==null||particles.Length<2||Quality<=0||game.ReducedMotion)return;
            var paint=new MaterialPropertyBlock();var renderer=face.GetComponent<MeshRenderer>();renderer.GetPropertyBlock(paint);
            int columns=Mathf.Min(Quality==1?5:8,particles.Length/2);
            var sources=new[]{face};var points=new TimberVertex[1][];var indices=new int[1][];
            for(int source=0;source<sources.Length;source++){
                var mesh=sources[source].sharedMesh;var vertices=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;indices[source]=mesh.triangles;points[source]=new TimberVertex[vertices.Length];
                for(int i=0;i<vertices.Length;i++)points[source][i]=new TimberVertex{point=sources[source].transform.TransformPoint(vertices[i]),normal=sources[source].transform.TransformDirection(normals[i]).normalized,uv=uv[i]};
            }
            var input=new TimberVertex[10];var output=new TimberVertex[10];
            var v=new List<Vector3>(256);var n=new List<Vector3>(256);var tex=new List<Vector2>(256);var triangles=new List<int>(256);
            for(int band=0;band<2;band++)for(int column=0;column<columns;column++){
                v.Clear();n.Clear();tex.Clear();triangles.Clear();
                float left=column==0?-1:(column+band*.40f)/columns,right=column==columns-1?2:(column+1+band*.40f)/columns;
                for(int source=0;source<sources.Length;source++)for(int i=0;i<indices[source].Length;i+=3){
                    for(int j=0;j<3;j++)input[j]=points[source][indices[source][i+j]];
                    int count=ClipTimber(input,output,3,new Vector2(1,.06f),left,true);Swap();
                    count=ClipTimber(input,output,count,new Vector2(1,.06f),right,false);Swap();
                    count=ClipTimber(input,output,count,new Vector2(-.12f,1),.43f,band==1);Swap();
                    if(count<3)continue;
                    for(int j=1;j<count-1;j++)Triangle(input[0],input[j],input[j+1]);
                    // Close each sliver with its own shallow back and edges; a copied full-depth lid
                    // would protrude beyond these smaller pieces when they tumble.
                    var depth=Vector3.forward*.085f;
                    for(int j=1;j<count-1;j++){var a=input[0];var b=input[j+1];var c=input[j];a.point+=depth;b.point+=depth;c.point+=depth;a.normal=-a.normal;b.normal=-b.normal;c.normal=-c.normal;Triangle(a,b,c);}
                    for(int j=0;j<count;j++){
                        var a=input[j];var b=input[(j+1)%count];var c=b;var d=a;c.point+=depth;d.point+=depth;
                        a.normal=b.normal=c.normal=d.normal=Vector3.Cross(b.point-a.point,depth).normalized;Triangle(a,b,c);Triangle(a,c,d);
                    }
                }
                if(v.Count==0)continue;
                Vector3 min=v[0],max=v[0];foreach(var point in v){min=Vector3.Min(min,point);max=Vector3.Max(max,point);}var pivot=(min+max)*.5f;
                for(int i=0;i<v.Count;i++)v[i]-=pivot;
                var p=AcquireParticle();RetireParticle(p);
                if(p.fragmentMesh==null)p.fragmentMesh=new Mesh{name="Pooled timber splinter"};
                p.fragmentMesh.Clear();p.fragmentMesh.SetVertices(v);p.fragmentMesh.SetNormals(n);p.fragmentMesh.SetUVs(0,tex);p.fragmentMesh.SetTriangles(triangles,0);p.fragmentMesh.RecalculateBounds();
                p.filter.sharedMesh=p.fragmentMesh;p.renderer.sharedMaterial=renderer.sharedMaterial;
                p.timberFragment=p.wood=true;p.seed=p.reduced=false;p.source=null;p.origin=pivot;p.age=0;p.life=8f;p.size=Mathf.Max(.1f,max.x-min.x);p.color=Color.white;
                p.properties.Clear();p.properties.SetVector("_BaseMap_ST",paint.GetVector("_BaseMap_ST"));p.properties.SetColor("_BaseColor",Color.white);p.renderer.SetPropertyBlock(p.properties);
                p.transform.SetPositionAndRotation(pivot,Quaternion.identity);p.transform.localScale=Vector3.one;p.transform.gameObject.SetActive(true);
                p.collider.center=p.fragmentMesh.bounds.center;var bounds=p.fragmentMesh.bounds.size;
                p.collider.size=new Vector3(bounds.x*.82f,Mathf.Max(.045f,bounds.y*.45f),Mathf.Max(.055f,bounds.z*.65f));p.collider.sharedMaterial=splinterContact;p.collider.enabled=true;
                var body=p.body;body.isKinematic=false;body.useGravity=true;body.mass=.035f+bounds.x*.045f;body.linearDamping=.065f;body.angularDamping=.26f;body.maxAngularVelocity=7;
                // Depth travel lets a chip bounce on a lower deck, then slide over its front edge.
                body.constraints=RigidbodyConstraints.None;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;body.position=pivot;body.rotation=Quaternion.identity;
                float side=(column+.5f)/columns-.5f;int variation=column+band*columns;
                body.linearVelocity=velocity+new Vector3(side*.55f+Mathf.Sin(variation*2.4f)*.22f,-.14f+(variation%3)*.10f,-.30f-(variation%3)*.045f);
                body.angularVelocity=new Vector3((variation%3-1)*.75f,(variation%2==0?1:-1)*1.1f,spin+(variation%2==0?1:-1)*(1.15f+(variation%4)*.32f));body.WakeUp();
                p.renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;p.renderer.receiveShadows=true;
            }
            void Swap(){var a=input;input=output;output=a;}
            void Triangle(TimberVertex a,TimberVertex b,TimberVertex c){Vertex(a);Vertex(b);Vertex(c);}
            void Vertex(TimberVertex point){triangles.Add(v.Count);v.Add(point.point);n.Add(point.normal);tex.Add(point.uv);}
        }
        static int ClipTimber(TimberVertex[] input,TimberVertex[] output,int count,Vector2 axis,float edge,bool above)
        {
            if(count==0)return 0;int result=0;var previous=input[count-1];float a=Vector2.Dot(previous.uv,axis)-edge;bool inside=above?a>=0:a<=0;
            for(int i=0;i<count;i++){
                var next=input[i];float b=Vector2.Dot(next.uv,axis)-edge;bool nextInside=above?b>=0:b<=0;
                if(inside!=nextInside)output[result++]=TimberVertex.Lerp(previous,next,a/(a-b));
                if(nextInside)output[result++]=next;
                previous=next;a=b;inside=nextInside;
            }
            return result;
        }
    }
}
