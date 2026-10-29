using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Cascade.Editor.Playable;
using Cascade.Presentation;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Cascade.Editor.Validation
{
    public static class LanternStoryValidation
    {
        public static void RebuildTables()
        {
            var root=Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
            var renames=JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(Path.Combine(root,"Data/story_lantern_key_renames.json")));
            var authored=JObject.Parse(File.ReadAllText(Path.Combine(root,"Data/story_lantern_oct16.json")));
            var history=authored["key_history"];
            var collection=LocalizationEditorSettings.GetStringTableCollection(UiCopy.Table);
            if(collection==null)throw new InvalidOperationException("Existing story table required.");
            var ids=new Dictionary<string,long>();
            foreach(var pair in renames){
                var old=collection.SharedData.GetEntry(pair.Key);
                if(old==null&&history?[pair.Key] is JArray aliases)old=aliases.Select(a=>collection.SharedData.GetEntry((string)a)).FirstOrDefault(e=>e!=null);
                var current=collection.SharedData.GetEntry(pair.Value);
                if(old!=null){
                    if(current!=null&&current.Id!=old.Id)throw new InvalidOperationException("Duplicate story target: "+pair.Value);
                    ids[pair.Value]=old.Id;if(current==null)collection.SharedData.RenameKey(old.Id,pair.Value);
                }else if(current!=null)ids[pair.Value]=current.Id;
                else throw new InvalidOperationException("Missing story key: "+pair.Key);
            }
            EditorUtility.SetDirty(collection.SharedData);EditorUtility.SetDirty(collection);
            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null,collection);
            AssetDatabase.SaveAssets();LocalizationAssets.Build();
            foreach(var pair in ids)if(collection.SharedData.GetEntry(pair.Key).Id!=pair.Value)throw new InvalidOperationException("Story table ID changed: "+pair.Key);
            var folder=Path.Combine(root,"Reports/LanternStoryOct16");Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"table-key-ids.json"),JsonConvert.SerializeObject(ids,Formatting.Indented));
            Debug.Log("CASCADE story rename: "+ids.Count+" numeric table IDs preserved.");
        }
    }
}
