using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class ToyMachineView
    {
        Material continuousRopeMaterial;
        Texture2D railGrain;
        readonly LineRenderer[] suspension=new LineRenderer[2];
        readonly Vector3[] ropePoints=new Vector3[121];
        void InitializeRopeRig()
        {
            if(continuousRopeMaterial==null){
                railGrain=new Texture2D(128,16,TextureFormat.RGB24,false,true){name="Carved rail grain",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
                var pixels=new Color32[128*16];
                for(int y=0;y<16;y++)for(int x=0;x<128;x++){
                    float grain=Mathf.Sin(y*2.4f+Mathf.Sin(x*.095f+y)*.8f)*.07f+Mathf.Sin(x*.31f+y*.57f)*.025f;
                    float edge=1-Mathf.Abs(y-7.5f)/7.5f;
                    float value=Mathf.Clamp01(.76f+grain+edge*.10f);
                    pixels[y*128+x]=new Color32((byte)(value*255),(byte)(value*228),(byte)(value*190),255);
                }
                railGrain.SetPixels32(pixels);railGrain.Apply(false,true);
                continuousRopeMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Carved flexible wooden rails"};
                continuousRopeMaterial.SetTexture("_BaseMap",railGrain);
                continuousRopeMaterial.SetColor("_BaseColor",new Color(.66f,.43f,.23f));
                continuousRopeMaterial.SetFloat("_Smoothness",.29f);
            }
            for(int side=0;side<2;side++) {
                var go=new GameObject(side==0?"Left articulated wooden rail":"Right articulated wooden rail",typeof(LineRenderer));go.transform.SetParent(machine,false);
                var line=go.GetComponent<LineRenderer>();suspension[side]=line;line.sharedMaterial=continuousRopeMaterial;line.useWorldSpace=true;
                line.generateLightingData=true;line.startWidth=line.endWidth=.086f;line.numCapVertices=8;line.numCornerVertices=8;line.textureMode=LineTextureMode.Tile;line.alignment=LineAlignment.View;
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
                        float wave=reducedMotion?0:Mathf.Sin(windTime*.85f+segment*.7f+side)*.018f*Mathf.Min(1,span);
                        point.x+=Mathf.Sin(t*Mathf.PI)*(sign*.01f+wave);ropePoints[index++]=point;
                    }
                    previous=next;
                }
                pointCount=index;line.positionCount=pointCount;
                for(int p=0;p<pointCount;p++)line.SetPosition(p,ropePoints[p]);
            }
        }
    }
}
