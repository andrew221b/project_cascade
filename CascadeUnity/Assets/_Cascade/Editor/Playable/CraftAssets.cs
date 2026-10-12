using UnityEditor;
using UnityEngine;
namespace Cascade.Editor.Playable
{
    public static class CraftAssets
    {
        public static void EnsureSceneryMaterial()
        {
            const string path="Assets/_Cascade/Content/Resources/CascadeArt/Craft/IslandScenery.mat";
            if(AssetDatabase.LoadAssetAtPath<Material>(path)==null)AssetDatabase.CreateAsset(new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Cascade/Presentation/IslandScenery.shader")),path);
            AssetDatabase.SaveAssets();
        }
        public static void Import()
        {
            AssetDatabase.Refresh();
            foreach(string file in System.IO.Directory.GetFiles("Assets/_Cascade/Content/Resources/CascadeArt/Craft","*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
            }
            EnsureSceneryMaterial();LocalizationAssets.Build();
        }
    }
}
