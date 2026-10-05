using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class ToyMachineView
    {
        Material[] candyMaterials;
        Material letterMaterial;
        Texture2D letterGrain;

        void InitializePieceMaterials()
        {
            candyMaterials = new Material[art.colors.Length];
            for (int i = 0; i < candyMaterials.Length; i++) {
                var material = new Material(art.colors[i]) { name = "Glazed " + art.colors[i].name };
                material.SetFloat("_Metallic", .035f);
                material.SetFloat("_Smoothness", .78f);
                material.SetFloat("_ClearCoatMask", .58f);
                material.SetFloat("_ClearCoatSmoothness", .87f);
                material.EnableKeyword("_CLEARCOAT");
                candyMaterials[i] = material;
            }
            letterMaterial = new Material(art.cream) { name = "Warm grained letter tile" };
            letterMaterial.SetFloat("_Metallic", .015f);
            letterMaterial.SetFloat("_Smoothness", .48f);
            letterMaterial.SetFloat("_ClearCoatMask", .18f);
            letterMaterial.SetFloat("_ClearCoatSmoothness", .62f);
            letterMaterial.EnableKeyword("_CLEARCOAT");
            // Quiet fibre and diagonal grain are visible on the ivory tile without competing with its letter.
            letterGrain = new Texture2D(96, 96, TextureFormat.RGB24, false, true) { name = "Letter tile fibre", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[96 * 96];
            for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++) {
                float line = Mathf.Sin((x + y * .37f) * .52f) * .017f;
                float fibre = Mathf.Sin(x * 1.73f + y * 2.11f) * .009f;
                byte shade = (byte)Mathf.RoundToInt(Mathf.Clamp01(.967f + line + fibre) * 255);
                pixels[y * 96 + x] = new Color32(shade, shade, shade, 255);
            }
            letterGrain.SetPixels32(pixels); letterGrain.Apply(false, true);
            letterMaterial.SetTexture("_BaseMap", letterGrain);
        }

        Material PieceMaterial(SymbolId symbol) => symbol.Kind == SymbolKind.Glyph
            ? letterMaterial : candyMaterials[art.ShapeIndex(symbol)];

        void DisposePieceMaterials()
        {
            if (candyMaterials != null) foreach (var material in candyMaterials) if (material != null) Destroy(material);
            if (letterMaterial != null) Destroy(letterMaterial);
            if (letterGrain != null) Destroy(letterGrain);
        }
    }
}
