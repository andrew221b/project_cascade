using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class WelcomeMotionAssets
    {
        [MenuItem("CASCADE/Configure Welcome Motion Textures")]
        public static void Configure()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Configure welcome texture imports outside Play Mode.");
            AssetDatabase.Refresh();
            foreach(string name in new[]{"HalfBlink","ClosedBlink"}){
                string path="Assets/_Cascade/Content/Resources/CascadeArt/WelcomeMotionOct16/"+name+".png";
                var import=(TextureImporter)AssetImporter.GetAtPath(path);if(import==null)throw new System.IO.FileNotFoundException(path);
                import.textureType=TextureImporterType.Default;import.sRGBTexture=true;import.alphaSource=TextureImporterAlphaSource.FromInput;import.alphaIsTransparency=true;
                import.mipmapEnabled=false;import.isReadable=false;import.wrapMode=TextureWrapMode.Clamp;import.filterMode=FilterMode.Bilinear;import.npotScale=TextureImporterNPOTScale.None;
                import.maxTextureSize=1024;import.textureCompression=TextureImporterCompression.Compressed;import.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();Debug.Log("CASCADE welcome eyelid imports configured; original illustration and scene unchanged.");
        }
    }
}
