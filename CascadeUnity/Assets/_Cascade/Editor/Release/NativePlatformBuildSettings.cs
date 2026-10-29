using System;using UnityEditor;using UnityEditor.Build;using UnityEditor.Build.Reporting;using UnityEngine;
namespace Cascade.Editor.Release {
    // Native identity dependencies are mandatory for a platform build, never silently
    // fall back to a browser when a native provider was requested by the player.
    public sealed class NativePlatformBuildSettings:IPreprocessBuildWithReport {
        public int callbackOrder=>-900;
        public void OnPreprocessBuild(BuildReport report){
            if(report.summary.platform==BuildTarget.iOS){
                var profile=AssetDatabase.LoadMainAssetAtPath("Assets/Apple Plug-In Support/Editor/DefaultAppleBuildProfile.asset");
                if(profile==null)throw new BuildFailedException("Apple GameKit build profile is missing.");
                var data=new SerializedObject(profile);data.FindProperty("MinimumOSVersion_iOS").stringValue="15.6";data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(profile);
                if(Version.Parse(PlayerSettings.iOS.targetOSVersionString)<new Version(15,6))PlayerSettings.iOS.targetOSVersionString="15.6";
                AssetDatabase.SaveAssets();
            }
            if(report.summary.platform==BuildTarget.Android){
                var type=Type.GetType("GooglePlayGames.PlayGamesSettings, Google.Play.Games");
                var settings=type?.GetMethod("LoadInstance")?.Invoke(null,null);
                string app=type?.GetProperty("AppId")?.GetValue(settings) as string,web=type?.GetProperty("WebClientId")?.GetValue(settings) as string;
                if(string.IsNullOrWhiteSpace(app)||string.IsNullOrWhiteSpace(web)||!web.EndsWith(".apps.googleusercontent.com",StringComparison.Ordinal))
                    throw new BuildFailedException("Configure this game's Play Games app ID and server Web Client ID before an Android build.");
            }
        }
    }
}
