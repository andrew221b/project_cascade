using UnityEngine;
using UnityEngine.Rendering;

namespace Cascade.Presentation
{
    public sealed partial class ToyMachineView
    {
        SpriteRenderer receiverSkin;Material receiverSkinMaterial;
        readonly Vector3[] skinCorners=new Vector3[4];
        public void SetReceiverFrame(Sprite sprite)
        {
            if(machine==null)return;
            sprite=sprite??GameplayReferenceArt.Sprite("ReceiverTray");if(sprite==null)return;
            if(receiverSkin==null||receiverSkin.transform.parent!=machine){
                if(receiverSkin!=null)Destroy(receiverSkin.gameObject);
                if(receiverSkinMaterial==null){
                    var shader=Resources.Load<Shader>("CascadeArt/IllustratedSurface");if(shader==null)return;
                    receiverSkinMaterial=new Material(shader){name="Painted target tray",renderQueue=3000};
                    receiverSkinMaterial.SetColor("_BaseColor",Color.white);receiverSkinMaterial.SetFloat("_Relief",.42f);
                    receiverSkinMaterial.SetOverrideTag("RenderType","Transparent");
                }
                var go=new GameObject("Cosmetic receiver rim",typeof(SpriteRenderer));go.transform.SetParent(machine,false);receiverSkin=go.GetComponent<SpriteRenderer>();
                receiverSkin.sharedMaterial=receiverSkinMaterial;receiverSkin.shadowCastingMode=ShadowCastingMode.Off;
            }
            foreach(var part in all)if(part.transform.name=="Golden receiver rim"||part.transform.name=="Ivory inset"||part.transform.name=="Receiver brass rope eye")part.transform.gameObject.SetActive(false);
            receiverSkinMaterial.SetTexture("_BaseMap",sprite.texture);receiverSkin.sprite=sprite;receiverSkin.drawMode=SpriteDrawMode.Sliced;receiverSkin.enabled=true;PaintReceiverFrame();
        }
        void PaintReceiverFrame()
        {
            if(receiverSkin==null||!receiverSkin.enabled||lowerAnchor==null||receiverSkin.sprite==null)return;
            lowerAnchor.GetWorldCorners(skinCorners);var size=skinCorners[2]-skinCorners[0];
            float height=size.y*.84f,scale=height/(receiverSkin.sprite.rect.height/receiverSkin.sprite.pixelsPerUnit);
            if(scale<=0)return;
            receiverSkin.transform.position=new Vector3(skinCorners[0].x+size.x*.5f,skinCorners[0].y+size.y*.53f,.24f);
            receiverSkin.transform.position+=ReceiverPointOffset(receiverSkin.transform.position);
            receiverSkin.transform.rotation=Quaternion.Euler(0,0,ReceiverAngle);receiverSkin.transform.localScale=new Vector3(scale,scale,1);
            var sliceSize=new Vector2(size.x*1.10f/scale,height/scale);if(receiverSkin.size!=sliceSize)receiverSkin.size=sliceSize;
        }
        void DisposeReceiverFrame(){if(receiverSkinMaterial!=null)Destroy(receiverSkinMaterial);}
    }
}
