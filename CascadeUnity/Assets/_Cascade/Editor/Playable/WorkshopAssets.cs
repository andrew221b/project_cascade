using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class WorkshopAssets
    {
        public static void Import()
        {
            const string root="Assets/_Cascade/Content/Resources/CascadeArt/Workshop";
            AssetDatabase.Refresh();
            foreach(string file in Directory.GetFiles(root,"*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
            }
            Debug.Log("CASCADE workshop art imported with native alpha.");
        }
    }
}
