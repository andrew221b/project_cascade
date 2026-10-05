using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class JourneyAssets
    {
        [MenuItem("CASCADE/Art/Import journey illustrations")]
        public static void Import()
        {
            const string root="Assets/_Cascade/Content/Resources/CascadeArt/Journey";
            AssetDatabase.Refresh();
            foreach(string file in Directory.GetFiles(root,"*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);
                if(importer==null)continue;
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
                importer.maxTextureSize=file.Contains("Base")||file.Contains("Map")?2048:1024;
                importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
            }
            Debug.Log("CASCADE journey rasters imported with preserved native alpha.");
        }
    }
}
