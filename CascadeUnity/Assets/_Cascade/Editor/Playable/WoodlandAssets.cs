using System.IO;
using UnityEditor;
using UnityEngine;
namespace Cascade.Editor.Playable
{
    public static class WoodlandAssets
    {
        public static void Import()
        {
            const string root="Assets/_Cascade/Content/Resources/CascadeArt/Woodland";AssetDatabase.Refresh();
            foreach(string file in Directory.GetFiles(root,"*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);if(importer==null)continue;
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=file.Contains("Board")?1024:2048;
                importer.textureCompression=TextureImporterCompression.Compressed;
                if(file.Contains("WoodBoard")) {
                    var image=new Texture2D(2,2);image.LoadImage(File.ReadAllBytes(file));var pixels=image.GetPixels32();
                    int left=image.width,right=0,bottom=image.height,top=0;
                    for(int y=0;y<image.height;y++)for(int x=0;x<image.width;x++)if(pixels[y*image.width+x].a>=128){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
                    if(right<=left||top<=bottom)throw new System.InvalidOperationException("Wood body alpha is empty.");
                    var frame=new Cascade.Presentation.WoodBodyFrame{left=left/(float)image.width,bottom=bottom/(float)image.height,width=(right-left+1)/(float)image.width,height=(top-bottom+1)/(float)image.height};
                    File.WriteAllText(root+"/WoodBoardFrame.json",JsonUtility.ToJson(frame,true));Object.DestroyImmediate(image);
                }
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
            Debug.Log("CASCADE woodland images imported preserving native alpha.");
        }
    }
}
