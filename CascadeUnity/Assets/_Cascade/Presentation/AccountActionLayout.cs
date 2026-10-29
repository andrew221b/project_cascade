using TMPro;
using UnityEngine;
namespace Cascade.Presentation
{
    // Compatibility entry point; providers follow the shared four-pixel content rule.
    public sealed class AccountActionLayout:MonoBehaviour
    {
        public void Initialize(RectTransform surface,RectTransform provider,TextMeshProUGUI caption,float width,float height)
        {
            provider.sizeDelta=new Vector2(width,height);
            var row=surface.GetComponent<CenteredContentRow>()??surface.gameObject.AddComponent<CenteredContentRow>();
            row.Initialize(surface,provider,caption.rectTransform);
            row.Padding=18;row.Refresh();
        }
    }
}
