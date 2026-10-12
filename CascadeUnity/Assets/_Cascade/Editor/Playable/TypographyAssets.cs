using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Cascade.Editor.Playable
{
    public static class TypographyAssets
    {
        const string Root="Assets/_Cascade/Content/Resources/CascadeArt/Fonts/";
        [MenuItem("CASCADE/Build Interface Typography")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Build typography outside Play Mode.");
            AssetDatabase.Refresh();
            string copy=File.ReadAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Data/ui_localization.tsv")));
            string characters=new string(copy.Where(c=>!char.IsControl(c)).Concat(Enumerable.Range(32,95).Select(c=>(char)c))
                .Concat(Enumerable.Range(0x410,64).Select(c=>(char)c)).Concat("ЁёҐґЄєІіЇї−→←‹›…×").Distinct().ToArray());
            var display=Make("McLaren-Regular","ShapeManiaInterface",new string(characters.Where(c=>c<256).ToArray()));
            var body=Make("Nunito-SemiBold","InterfaceBody",characters);
            var title=Make("Nunito-ExtraBold","InterfaceTitle",characters);
            title.fallbackFontAssetTable=new List<TMP_FontAsset>();
            var cyrillic=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/Body");
            var cyrillicBold=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/Title");
            if(cyrillic==null||cyrillicBold==null)throw new InvalidOperationException("Missing Balsamiq Sans Cyrillic fonts.");
            cyrillic.fontWeightTable[7].regularTypeface=cyrillicBold;
            display.fallbackFontAssetTable=new List<TMP_FontAsset>{cyrillic};
            body.fallbackFontAssetTable=new List<TMP_FontAsset>{cyrillic};
            var art=ToyArt.Load();art.bodyFont=body;art.titleFont=display;art.displayFont=display;
            foreach(var item in new UnityEngine.Object[]{body,title,display,cyrillic,art})EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();
            Debug.Log("CASCADE storybook/interface typography ready; original fonts preserved.");
        }
        static TMP_FontAsset Make(string source,string name,string characters)
        {
            var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root+name+".asset");if(existing!=null)return existing;
            var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Cascade/Art/Fonts/"+source+".ttf");
            if(font==null)throw new InvalidOperationException("Missing typography source: "+source);
            var asset=TMP_FontAsset.CreateFontAsset(font,80,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            asset.name=name;AssetDatabase.CreateAsset(asset,Root+name+".asset");
            asset.TryAddCharacters(characters,out string missing);
            if(missing.Any(c=>c!='→'&&c!='←'))throw new InvalidDataException("Missing typography glyphs: "+missing);
            asset.atlasPopulationMode=AtlasPopulationMode.Static;
            AssetDatabase.AddObjectToAsset(asset.material,asset);
            foreach(var texture in asset.atlasTextures)if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,asset);
            EditorUtility.SetDirty(asset);return asset;
        }
    }
}
