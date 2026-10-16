using UnityEditor;
using UnityEngine;
namespace Cascade.Editor.Playable
{
    public sealed class IslandLevelAssets:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/_Cascade/Content/Resources/CascadeArt/IslandLevelsOct16/",System.StringComparison.Ordinal)||!assetPath.EndsWith(".png",System.StringComparison.Ordinal))return;
            var importer=(TextureImporter)assetImporter;int max=assetPath.EndsWith("ProgressScroll.png",System.StringComparison.Ordinal)?4096:2048;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;
            importer.alphaIsTransparency=true;importer.isReadable=false;importer.mipmapEnabled=false;importer.sRGBTexture=true;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.maxTextureSize=max;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            foreach(string platform in new[]{"iPhone","Android"}){
                var settings=importer.GetPlatformTextureSettings(platform);settings.overridden=true;settings.maxTextureSize=max;settings.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(settings);
            }
        }
    }
}
