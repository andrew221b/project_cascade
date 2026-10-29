using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif
using UnityEngine;
namespace Cascade.Editor.Playable
{
    public static class StoreAndroidCaptureBuild
    {
        public static void Export()
        {
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android||EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Stopped compiled Android Editor required.");
            string root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            string output=Environment.GetEnvironmentVariable("CASCADE_CAPTURE_OUTPUT")??Path.Combine(root,"CascadeUnity/Builds/StoreAndroid-Oct22.apk");
            if(File.Exists(output))throw new InvalidOperationException("Existing APK preserved.");
            string id=PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),product=PlayerSettings.productName;
            bool bundle=EditorUserBuildSettings.buildAppBundle,custom=PlayerSettings.Android.useCustomKeystore;
            var architecture=PlayerSettings.Android.targetArchitectures;var targetSdk=PlayerSettings.Android.targetSdkVersion;
#if UNITY_ANDROID
            string sdk=AndroidExternalToolsSettings.sdkRootPath,ndk=AndroidExternalToolsSettings.ndkRootPath,jdk=AndroidExternalToolsSettings.jdkRootPath;
#endif
            try{
#if UNITY_ANDROID
                string tools=Environment.GetEnvironmentVariable("FLYING_LADDERS_ANDROID_TOOLS");
                if(!string.IsNullOrEmpty(tools)){AndroidExternalToolsSettings.sdkRootPath=Path.Combine(tools,"SDK");AndroidExternalToolsSettings.ndkRootPath=Path.Combine(tools,"NDK");AndroidExternalToolsSettings.jdkRootPath=Path.Combine(tools,"OpenJDK");}
#endif
                PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.vickand.flyingladders.capture");PlayerSettings.productName="Flying Ladders";
                EditorUserBuildSettings.buildAppBundle=false;PlayerSettings.Android.useCustomKeystore=false;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
                LocalizationAssets.BuildPlayerTables();
                using var captureScope=new StoreCaptureBuildScope();
                var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Cascade/Scenes/CascadePlayable.unity"},locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None,extraScriptingDefines=new[]{"CASCADE_STORE_CAPTURE"}});
                string report=Environment.GetEnvironmentVariable("CASCADE_CAPTURE_REPORT")??Path.Combine(root,"Reports/StoreScreensOct22/AndroidBuild");Directory.CreateDirectory(report);
                File.WriteAllText(Path.Combine(report,"unity-build.txt"),"Result "+build.summary.result+"\nErrors "+build.summary.totalErrors+"\n"+string.Join("\n",build.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content.Split(new[]{"Environment Variables:"},StringSplitOptions.None)[0])));
                if(build.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Android capture export failed.");
            }finally{
#if UNITY_ANDROID
                AndroidExternalToolsSettings.sdkRootPath=sdk;AndroidExternalToolsSettings.ndkRootPath=ndk;AndroidExternalToolsSettings.jdkRootPath=jdk;
#endif
                PlayerSettings.Android.targetSdkVersion=targetSdk;PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,id);PlayerSettings.productName=product;EditorUserBuildSettings.buildAppBundle=bundle;PlayerSettings.Android.useCustomKeystore=custom;PlayerSettings.Android.targetArchitectures=architecture;AssetDatabase.SaveAssets();}
        }
    }
}
