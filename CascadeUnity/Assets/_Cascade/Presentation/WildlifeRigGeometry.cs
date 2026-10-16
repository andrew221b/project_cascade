using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Presentation
{
    // Tight painted contours exclude adjacent atlas art without changing any source PNG pixels.
    internal static class WildlifeRigGeometry
    {
        public static Mesh CreateFlexibleBody(out Vector3[] rest)
        {
            const int columns=12,rows=8;rest=new Vector3[(columns+1)*(rows+1)];var uv=new Vector2[rest.Length];var triangles=new int[columns*rows*6];int n=0;
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++){int i=y*(columns+1)+x;uv[i]=new Vector2(x/(float)columns,y/(float)rows);rest[i]=(Vector3)(uv[i]-Vector2.one*.5f);}
            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++){int i=y*(columns+1)+x;triangles[n++]=i;triangles[n++]=i+columns+1;triangles[n++]=i+1;triangles[n++]=i+1;triangles[n++]=i+columns+1;triangles[n++]=i+columns+2;}
            var mesh=new Mesh{name="Flexible cricket antennae and abdomen"};mesh.MarkDynamic();mesh.vertices=rest;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateBounds();return mesh;
        }
        public static Mesh Create(WildlifeRigAtlas.Cell cell,out Vector3[] rest)
        {
            int count=cell.Outline.Length/2;rest=new Vector3[count];var uv=new Vector2[count];
            for(int i=0;i<count;i++) {
                uv[i]=new Vector2(cell.Outline[i*2],cell.Outline[i*2+1]);
                rest[i]=new Vector3(uv[i].x-.5f,uv[i].y-.5f,0);
            }
            var mesh=new Mesh{name="Painted wildlife contour"};mesh.MarkDynamic();
            mesh.vertices=rest;mesh.uv=uv;mesh.triangles=Triangulate(rest);mesh.RecalculateBounds();return mesh;
        }

        static int[] Triangulate(Vector3[] vertices)
        {
            var remaining=new List<int>(vertices.Length);float area=0;
            for(int i=0;i<vertices.Length;i++){var a=vertices[i];var b=vertices[(i+1)%vertices.Length];area+=a.x*b.y-b.x*a.y;}
            for(int i=0;i<vertices.Length;i++)remaining.Add(area>0?i:vertices.Length-1-i);
            var triangles=new List<int>((vertices.Length-2)*3);int guard=vertices.Length*vertices.Length;
            while(remaining.Count>3&&guard-->0) {
                bool cut=false;
                for(int i=0;i<remaining.Count;i++) {
                    int a=remaining[(i+remaining.Count-1)%remaining.Count],b=remaining[i],c=remaining[(i+1)%remaining.Count];
                    if(Cross(vertices[b]-vertices[a],vertices[c]-vertices[b])<=.0000001f)continue;
                    bool inside=false;
                    for(int k=0;k<remaining.Count;k++) {
                        int p=remaining[k];if(p==a||p==b||p==c)continue;
                        if(InTriangle(vertices[p],vertices[a],vertices[b],vertices[c])){inside=true;break;}
                    }
                    if(inside)continue;
                    triangles.Add(a);triangles.Add(b);triangles.Add(c);remaining.RemoveAt(i);cut=true;break;
                }
                if(!cut)throw new System.InvalidOperationException("Invalid wildlife contour; inspect authored atlas geometry.");
            }
            if(remaining.Count!=3)throw new System.InvalidOperationException("Incomplete wildlife triangulation.");
            triangles.Add(remaining[0]);triangles.Add(remaining[1]);triangles.Add(remaining[2]);return triangles.ToArray();
        }
        static float Cross(Vector3 a,Vector3 b)=>a.x*b.y-a.y*b.x;
        static bool InTriangle(Vector3 p,Vector3 a,Vector3 b,Vector3 c)
            =>Cross(b-a,p-a)>=-.0000001f&&Cross(c-b,p-b)>=-.0000001f&&Cross(a-c,p-c)>=-.0000001f;
    }
}
