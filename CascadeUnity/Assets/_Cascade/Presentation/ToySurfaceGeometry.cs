using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    // Preserve authored silhouettes; add a gently convex ceramic cap to runtime copies only.
    public static class ToySurfaceGeometry
    {
        public static Mesh RoundFace(Mesh source)
        {
            var original=source.vertices;int count=(original.Length-2)/5,front=original.Length-2;
            if(count<3||count*5+2!=original.Length)return Object.Instantiate(source);
            var vertices=new List<Vector3>(original);var uv=new List<Vector2>(source.uv);
            var triangles=new List<int>();var old=source.triangles;
            for(int t=0;t<old.Length;t+=3)if(old[t]!=front&&old[t+1]!=front&&old[t+2]!=front)
                {triangles.Add(old[t]);triangles.Add(old[t+1]);triangles.Add(old[t+2]);}
            float bulge=source.bounds.size.z*.42f;
            var centre=original[front];centre.z-=bulge;vertices[front]=centre;
            int previous=0;
            for(int ring=0;ring<3;ring++){
                float radius=ring==0?.78f:ring==1?.52f:.26f;int first=vertices.Count;
                for(int i=0;i<count;i++){
                    var p=original[i];p.x*=radius;p.y*=radius;p.z-=bulge*(1-radius*radius);
                    vertices.Add(p);uv.Add(Vector2.Lerp(new Vector2(.5f,.5f),source.uv[i],radius));
                }
                for(int i=0;i<count;i++){
                    int a=previous+i,b=previous+(i+1)%count,c=first+i,d=first+(i+1)%count;
                    triangles.Add(a);triangles.Add(c);triangles.Add(b);triangles.Add(c);triangles.Add(d);triangles.Add(b);
                }
                previous=first;
            }
            for(int i=0;i<count;i++){triangles.Add(front);triangles.Add(previous+(i+1)%count);triangles.Add(previous+i);}
            var mesh=new Mesh{name=source.name+" rounded glaze"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }
    }
}
