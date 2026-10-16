using UnityEditor;
using UnityEngine;
namespace Cascade.Editor.Playable
{
    public sealed class TutorialReferenceAssets:AssetPostprocessor
    {
        void OnPreprocessTexture(){
            if(!assetPath.StartsWith("Assets/_Cascade/Content/Resources/CascadeArt/TutorialReferenceOct16/",System.StringComparison.Ordinal)||!assetPath.EndsWith(".png",System.StringComparison.Ordinal))return;
            var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.isReadable=false;importer.mipmapEnabled=false;importer.sRGBTexture=true;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            foreach(string platform in new[]{"iPhone","Android"}){var settings=importer.GetPlatformTextureSettings(platform);settings.overridden=true;settings.maxTextureSize=1024;settings.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(settings);}
        }
    }
}
