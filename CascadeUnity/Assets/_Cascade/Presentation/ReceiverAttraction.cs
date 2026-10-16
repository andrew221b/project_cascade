using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    // One fine thread per arriving piece. The resolver owns acceptance.
    public sealed class ReceiverAttraction:MonoBehaviour
    {
        readonly List<Vector3> vertices=new List<Vector3>(512);
        readonly List<Color> colors=new List<Color>(512);
        readonly List<Vector2> uv=new List<Vector2>(512);
        readonly List<int> triangles=new List<int>(1536);
        static readonly int FlowPhase=Shader.PropertyToID("_FlowPhase");
        Mesh mesh;Material material;MeshRenderer surface;Matrix4x4 worldToLocal;
        public float Strength {get;private set;}
        public void Initialize()
        {
            mesh=new Mesh{name="Receiver rainbow threads"};mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            surface=gameObject.AddComponent<MeshRenderer>();material=new Material(Resources.Load<Shader>("CascadeArt/ReceiverMagic"));
            surface.sharedMaterial=material;surface.shadowCastingMode=ShadowCastingMode.Off;surface.receiveShadows=false;surface.enabled=false;
        }
        public void Clear(){Strength=0;if(surface!=null)surface.enabled=false;}
        public void Paint(ToyMachineView rig,float strength,float phase,bool accepted,bool reduced)
        {
            if(strength<=.001f){Clear();return;}
            Strength=strength;worldToLocal=transform.worldToLocalMatrix;
            // Presentation time freezes with the delivery, including its colour flow.
            material.SetFloat(FlowPhase,reduced?0:phase*(accepted?1:-1));
            vertices.Clear();colors.Clear();uv.Clear();triangles.Clear();
            for(int slot=0;slot<rig.TokenCount;slot++){
                Vector3 target=rig.ReceiverTargetPosition(slot),body=rig.TokenVisualPosition(slot);target.z=body.z=-.62f;
                float size=Mathf.Clamp(rig.TokenBodySize(slot).x,.3f,.9f);
                var delta=body-target;if(delta.sqrMagnitude<.001f)continue;
                var color=new Color(1,1,1,strength*.88f);int first=vertices.Count;
                // Shared vertices keep the gentle curve seamless; UV.y spans the whole thread.
                for(int step=0;step<=18;step++){
                    float t=step/18f,bow=size*.045f;
                    var point=Vector3.Lerp(target,body,t)+Vector3.right*(Mathf.Sin(t*Mathf.PI)*bow);
                    var tangent=delta+Vector3.right*(Mathf.Cos(t*Mathf.PI)*Mathf.PI*bow);
                    var across=new Vector3(-tangent.y,tangent.x,0).normalized*.0135f;
                    Vertex(point-across,color,new Vector2(0,t));Vertex(point+across,color,new Vector2(1,t));
                    if(step==0)continue;
                    int k=first+(step-1)*2;
                    triangles.Add(k);triangles.Add(k+1);triangles.Add(k+3);triangles.Add(k);triangles.Add(k+3);triangles.Add(k+2);
                }
            }
            if(mesh.vertexCount!=vertices.Count)mesh.Clear(false);
            mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();surface.enabled=vertices.Count>0;
        }
        void Vertex(Vector3 p,Color color,Vector2 tex){vertices.Add(worldToLocal.MultiplyPoint3x4(p));colors.Add(color);uv.Add(tex);}
        void OnDestroy(){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
