using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class FaunaUiAssets
    {
        public static void ImportRearAtlas()
        {
            foreach(string name in new[]{"RegionalRearOct15","RegionalFlightBodiesOct15"})Import(name);
        }
        static void Import(string name)
        {
            string path="Assets/_Cascade/Resources/CascadeArt/Atmosphere/"+name+".png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.alphaIsTransparency=true;
            importer.isReadable=false;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            var phone=importer.GetPlatformTextureSettings("iPhone");phone.overridden=true;phone.maxTextureSize=2048;phone.format=TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(phone);importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/FaunaUiOct15"));Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root,name+"-import.txt"),path+"\n"+texture.width+" x "+texture.height+"\nASTC 6x6; RGBA; no mipmaps; not readable\n");
        }
    }
}
