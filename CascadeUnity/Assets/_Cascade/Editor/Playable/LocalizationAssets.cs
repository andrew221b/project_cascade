using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Cascade.Presentation;

namespace Cascade.Editor.Playable
{
    public static class LocalizationAssets
    {
        const string Root="Assets/_Cascade/Content/Localization";
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Leave Play mode first.");
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            var settings=LocalizationEditorSettings.ActiveLocalizationSettings;
            if(settings==null) {
                var found=AssetDatabase.FindAssets("t:LocalizationSettings",new[]{"Assets"});
                settings=found.Length>0?AssetDatabase.LoadAssetAtPath<LocalizationSettings>(AssetDatabase.GUIDToAssetPath(found[0])):ScriptableObject.CreateInstance<LocalizationSettings>();
                if(!AssetDatabase.Contains(settings))AssetDatabase.CreateAsset(settings,Root+"/LocalizationSettings.asset");
                LocalizationEditorSettings.ActiveLocalizationSettings=settings;
            }
            foreach(string code in new[]{"en","ru","uk"})if(!LocalizationEditorSettings.GetLocales().Any(l=>l.Identifier.Code==code)) {
                var locale=Locale.CreateLocale(code);AssetDatabase.CreateAsset(locale,Root+"/Locale_"+code+".asset");LocalizationEditorSettings.AddLocale(locale);
            }
            settings.GetStartupLocaleSelectors().Clear();settings.GetStartupLocaleSelectors().Add(new SpecificLocaleSelector{LocaleId=new LocaleIdentifier("en")});
            LocalizationSettings.InitializeSynchronously=true;EditorUtility.SetDirty(settings);
            var collection=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table)??LocalizationEditorSettings.CreateStringTableCollection(UiCopy.Table,Root);
            var lines=File.ReadAllLines(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Data/ui_localization.tsv"))).Skip(1);
            var keys=new System.Collections.Generic.HashSet<string>();int count=0;
            foreach(string line in lines) {
                if(string.IsNullOrWhiteSpace(line))continue;var cells=line.Split('\t').Select(v=>v.Replace("\\n","\n")).ToArray();
                if(cells.Length!=4||cells.Any(string.IsNullOrWhiteSpace)||!keys.Add(cells[0]))throw new InvalidDataException("Invalid UI translation row: "+line);
                foreach(var table in collection.StringTables) {
                    int col=table.LocaleIdentifier.Code=="en"?1:table.LocaleIdentifier.Code=="ru"?2:table.LocaleIdentifier.Code=="uk"?3:throw new InvalidDataException("Unexpected UI locale.");
                    table.AddEntry(cells[0],cells[col]);EditorUtility.SetDirty(table);count++;
                }
            }
            collection.SetPreloadTableFlag(true);EditorUtility.SetDirty(collection);EditorUtility.SetDirty(collection.SharedData);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null,collection);
            // The initial three locales share the same font/art. Reserved asset collection is empty;
            // Resources assets are deliberately not entered as Addressable sub-assets.
            if(LocalizationEditorSettings.GetAssetTableCollection("CascadeLocaleArt")==null)LocalizationEditorSettings.CreateAssetTableCollection("CascadeLocaleArt",Root);
            var characters=new string(collection.StringTables.SelectMany(t=>t.Values).SelectMany(e=>e.Value).Where(c=>!char.IsControl(c)).Distinct().ToArray());
            foreach(var font in new[]{ToyArt.Load().bodyFont,ToyArt.Load().titleFont}) {
                string needed=new string(characters.Where(c=>!font.HasCharacter(c,false,false)).ToArray());
                if(needed.Length>0) {
                    font.atlasPopulationMode=TMPro.AtlasPopulationMode.Dynamic;
                    // TMP clears the runtime source reference for static atlases on reload.
                    var serializedFont=new SerializedObject(font);
                    serializedFont.FindProperty("m_SourceFontFile").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Cascade/Art/Fonts/BalsamiqSans-"+(font.name=="Title"?"Bold":"Regular")+".ttf");
                    serializedFont.ApplyModifiedPropertiesWithoutUndo();
                    try {if(!font.TryAddCharacters(needed,out string missing))throw new InvalidDataException("Missing UI glyphs: "+missing);}
                    finally {font.atlasPopulationMode=TMPro.AtlasPopulationMode.Static;}
                }
                font.atlasPopulationMode=TMPro.AtlasPopulationMode.Static;EditorUtility.SetDirty(font);
                foreach(var texture in font.atlasTextures){if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,font);EditorUtility.SetDirty(texture);}
            }
            AssetDatabase.SaveAssets();Debug.Log("CASCADE UI tables: "+count+" non-empty entries / "+keys.Count+" keys.");
        }
        public static void BuildPlayerTables()
        {
            AddressableAssetSettings.BuildPlayerContent(out var result);
            if(!string.IsNullOrEmpty(result.Error))throw new InvalidOperationException(result.Error);
            var path=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/P15"));Directory.CreateDirectory(path);
            File.WriteAllText(path+"/addressables-build.txt","Success: "+EditorUserBuildSettings.activeBuildTarget+"; duration="+result.Duration+"; path="+result.OutputPath+"\n");
        }
    }
}
