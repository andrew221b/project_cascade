using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Cascade.Editor.Release
{
    public static class StoreIconSetup
    {
        [MenuItem("CASCADE/Release/Apply new store icon")]
        public static void Apply()
        {
            const string path="Assets/_Cascade/Art/Icons/CascadeStoreIcon.png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Generated store icon asset is missing.");
            importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;
            importer.alphaSource=TextureImporterAlphaSource.None;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=1024;importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var foreground=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Cascade/Art/Icons/CascadeAdaptiveForeground.png");
            var background=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Cascade/Art/Icons/CascadeAdaptiveBackground.png");
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{texture},IconKind.Any);
            foreach(var target in new[]{NamedBuildTarget.iOS,NamedBuildTarget.Android})
                foreach(var kind in PlayerSettings.GetSupportedIconKinds(target))
                {
                    // Preserve the optional monochrome slot until a proper silhouette is supplied.
                    if(target.Equals(NamedBuildTarget.Android)&&kind.ToString().IndexOf("Monochrome",StringComparison.OrdinalIgnoreCase)>=0)continue;
                    var icons=PlayerSettings.GetPlatformIcons(target,kind);
                    foreach(var icon in icons)
                        for(int layer=0;layer<icon.maxLayerCount;layer++)icon.SetTexture(target.Equals(NamedBuildTarget.Android)&&icon.maxLayerCount==2
                            ?(layer==0?background:foreground):texture,layer);
                    PlayerSettings.SetPlatformIcons(target,kind,icons);
                }
            AssetDatabase.SaveAssets();
            Debug.Log("CASCADE store icon applied to iPhone, iPad and Android icon slots.");
        }
    }
}
