using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class ToyMachineView
    {
        Material continuousRopeMaterial;
        readonly LineRenderer[] suspension=new LineRenderer[2];
        readonly Vector3[] ropePoints=new Vector3[121];
        void InitializeRopeRig()
        {
            if(continuousRopeMaterial==null){continuousRopeMaterial=new Material(Resources.Load<Shader>("CascadeArt/HempRope"));continuousRopeMaterial.SetColor("_Color",new Color(.88f,.68f,.39f));}
            for(int side=0;side<2;side++) {
                var go=new GameObject(side==0?"Left continuous hemp rope":"Right continuous hemp rope",typeof(LineRenderer));go.transform.SetParent(machine,false);
                var line=go.GetComponent<LineRenderer>();suspension[side]=line;line.sharedMaterial=continuousRopeMaterial;line.useWorldSpace=true;
                line.generateLightingData=false;line.startWidth=line.endWidth=.065f;line.numCapVertices=6;line.numCornerVertices=6;line.textureMode=LineTextureMode.Tile;line.alignment=LineAlignment.View;
                line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
            }
        }
        void UpdateRopeRig()
        {
            if(floors.Count==0||suspension[0]==null)return;
            for(int side=0;side<2;side++) {
                var line=suspension[side];line.enabled=Frame.Status==Cascade.Domain.PuzzleStatus.Playing;
                int pointCount=0;
                float sign=side==0?-1:1;
                var first=floors[0].board;float x=first.basePosition.x+sign*first.baseScale.x*.475f;
                Vector3 previous=new Vector3(x,viewCamera.transform.position.y+viewCamera.orthographicSize+.2f,.10f);int index=0;
                ropePoints[index++]=previous;
                for(int segment=0;segment<=floors.Count;segment++) {
                    Vector3 next;
                    if(segment<floors.Count) {
                        if(!Frame.FloorVisible(segment))continue;
                        var rig=floors[segment];next=rig.board.basePosition+new Vector3(sign*rig.board.baseScale.x*.475f+rig.sway,rig.board.baseScale.y*.24f+floorOffsets[segment],-.05f);
                    } else {
                        lowerAnchor.GetWorldCorners(corners);next=new Vector3(Mathf.Lerp(corners[0].x,corners[2].x,side==0?.035f:.965f),Mathf.Lerp(corners[0].y,corners[2].y,.48f),.10f);
                    }
                    float span=Vector3.Distance(previous,next);
                    for(int p=1;p<=24;p++) {
                        float t=p/24f;var point=Vector3.Lerp(previous,next,t);
                        float wave=reducedMotion?0:Mathf.Sin(windTime*1.4f+segment*.7f+side)*.035f*Mathf.Min(1,span);
                        point.x+=Mathf.Sin(t*Mathf.PI)*(sign*.016f+wave);ropePoints[index++]=point;
                    }
                    previous=next;
                }
                pointCount=index;line.positionCount=pointCount;
                for(int p=0;p<pointCount;p++)line.SetPosition(p,ropePoints[p]);
            }
        }
    }
}
