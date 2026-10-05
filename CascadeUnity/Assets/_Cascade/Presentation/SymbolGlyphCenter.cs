using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    // Toy faces contain one glyph. Line boxes and ascender metrics are not its visual centre.
    [DisallowMultipleComponent]
    public sealed class SymbolGlyphCenter : MonoBehaviour
    {
        TMP_Text text;
        void OnEnable()
        {
            text = GetComponent<TMP_Text>();
            text.OnPreRenderText += Center;
            text.SetVerticesDirty();
        }
        void OnDisable() { if (text != null) text.OnPreRenderText -= Center; }
        void Center(TMP_TextInfo info)
        {
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < info.characterCount; i++) {
                var c = info.characterInfo[i]; if (!c.isVisible) continue;
                var vertices = info.meshInfo[c.materialReferenceIndex].vertices;
                for (int v = c.vertexIndex; v < c.vertexIndex + 4; v++) {
                    min = Vector2.Min(min, vertices[v]); max = Vector2.Max(max, vertices[v]);
                }
            }
            if (float.IsInfinity(min.x)) return;
            Vector3 offset = text.rectTransform.rect.center - (min + max) * .5f;
            for (int i = 0; i < info.characterCount; i++) {
                var c = info.characterInfo[i]; if (!c.isVisible) continue;
                var vertices = info.meshInfo[c.materialReferenceIndex].vertices;
                for (int v = c.vertexIndex; v < c.vertexIndex + 4; v++) vertices[v] += offset;
            }
        }
    }
}
