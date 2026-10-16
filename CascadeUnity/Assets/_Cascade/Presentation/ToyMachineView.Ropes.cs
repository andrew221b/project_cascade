using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class ToyMachineView
    {
        Material continuousRopeMaterial;
        readonly LineRenderer[] suspension=new LineRenderer[2];
        readonly Vector3[] ropePoints=new Vector3[121];
        readonly PresentationRope[,] ropeSpans=new PresentationRope[2,5];
        float upperRopeHalfWidth=float.PositiveInfinity;
        public void SetUpperRopeHalfWidth(float width)=>upperRopeHalfWidth=Mathf.Max(.01f,width);
        void InitializeRopeRig()
        {
            if(continuousRopeMaterial==null){
                continuousRopeMaterial=new Material(Resources.Load<Shader>("CascadeArt/HempRope")){name="Soft braided suspension rope"};
                continuousRopeMaterial.SetColor("_Color",Color.white);continuousRopeMaterial.SetTexture("_BaseMap",GameplayReferenceArt.Texture("HempRope"));continuousRopeMaterial.SetFloat("_Authored",1);
            }
            for(int side=0;side<2;side++) {
                var go=new GameObject(side==0?"Left braided rope":"Right braided rope",typeof(LineRenderer));go.transform.SetParent(machine,false);
                var line=go.GetComponent<LineRenderer>();suspension[side]=line;line.sharedMaterial=continuousRopeMaterial;line.useWorldSpace=true;
                for(int span=0;span<5;span++)ropeSpans[side,span]=new PresentationRope(13,side*47+span*19+7);
                line.startWidth=line.endWidth=.09f;line.numCapVertices=8;line.numCornerVertices=8;line.textureMode=LineTextureMode.Tile;line.alignment=LineAlignment.View;
                line.generateLightingData=true;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;line.receiveShadows=true;
            }
        }
        void UpdateRopeRig(float seconds=0)
        {
            if(floors.Count==0||suspension[0]==null)return;
            for(int side=0;side<2;side++) {
                // The row's terminal status does not remove its hanging rig.
                // Screen visibility owns the rig; the ropes keep swaying through delivery.
                var line=suspension[side];line.enabled=true;
                float sign=side==0?-1:1;
                var first=floors[0].board;float x=first.basePosition.x+sign*Mathf.Min(first.baseScale.x*.475f,upperRopeHalfWidth);
                Vector3 previous=new Vector3(x,viewCamera.transform.position.y+viewCamera.orthographicSize+.2f,.10f);int index=0;
                ropePoints[index++]=previous;
                for(int segment=0;segment<=floors.Count;segment++) {
                    Vector3 next;
                    if(segment<floors.Count) {
                        if(!Frame.FloorVisible(segment))continue;
                        var rig=floors[segment];var point=rig.knots[side].basePosition;
                        next=point+FloorPointOffset(segment,point,true);next.z=.1f;
                    } else {
                        next=receiverKnots[side].transform.position;next.z=.10f;
                    }
                    var chain=ropeSpans[side,segment];
                    bool moving=!interaction.IsPaused&&!interaction.IsSuspended&&!ambientPaused&&gameplayView;
                    chain.Advance(previous,next,moving?seconds:0,windTime-segment*.17f,WindAt(Mathf.Min(segment,floors.Count-1)),1.8f,.65f,.34f,reducedMotion);
                    for(int p=1;p<=24;p++) {
                        var point=chain.Sample(p/24f);ropePoints[index++]=new Vector3(point.x,point.y,.1f);
                    }
                    previous=next;
                }
                line.positionCount=index;
                for(int p=0;p<index;p++)line.SetPosition(p,ropePoints[p]);
            }
        }
    }
}
