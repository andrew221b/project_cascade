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
            letterMaterial = new Material(art.cream) { name = "Glazed ivory letter tile" };
            letterMaterial.SetFloat("_Metallic", .015f);
            letterMaterial.SetFloat("_Smoothness", .68f);
            letterMaterial.SetFloat("_ClearCoatMask", .38f);
            letterMaterial.SetFloat("_ClearCoatSmoothness", .82f);
            letterMaterial.EnableKeyword("_CLEARCOAT");
            // Soft marbled ceramic, kept broad enough to survive phone downsampling.
            letterGrain = new Texture2D(128, 128, TextureFormat.RGB24, true, false) { name = "Warm ivory glaze", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear };
            var pixels = new Color32[128 * 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++) {
                float u=x/127f,v=y/127f;
                float swirl=Mathf.Sin(u*11f+v*4.5f+Mathf.Sin(v*8f)*.8f)*.035f;
                float cream=Mathf.Sin(u*5f-v*7f)*.025f;
                float grain=Mathf.Sin(x*.71f+y*.24f)*.007f;
                float shade=Mathf.Clamp01(.95f+swirl+cream+grain);
                pixels[y * 128 + x] = new Color32((byte)(shade*255),(byte)(shade*250),(byte)(shade*226),255);
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
