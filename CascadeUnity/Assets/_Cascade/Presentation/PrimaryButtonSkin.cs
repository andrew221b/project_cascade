using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    // A single clean primary skin; rounded corners scale with the button's height,
    // while only the plain centre stretches to accommodate localized labels.
    [RequireComponent(typeof(Image))]
    public sealed class PrimaryButtonSkin:MonoBehaviour
    {
        Image face;RectTransform rect;Sprite primary;
        public void Configure(Sprite sprite)
        {
            face=GetComponent<Image>();rect=(RectTransform)transform;primary=sprite;
            face.sprite=sprite;face.type=Image.Type.Sliced;face.preserveAspect=false;face.color=Color.white;Fit();
        }
        void Fit()
        {
            if(face==null||face.sprite==null||face.sprite!=primary||rect==null)return;
            float value=face.sprite.rect.height/Mathf.Max(1,rect.rect.height);
            if(Mathf.Abs(face.pixelsPerUnitMultiplier-value)>.001f)face.pixelsPerUnitMultiplier=value;
        }
        void OnRectTransformDimensionsChange()=>Fit();
        void LateUpdate()=>Fit();
    }
}
