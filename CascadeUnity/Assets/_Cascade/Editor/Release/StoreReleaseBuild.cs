using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif
namespace Cascade.Editor.Release
{
    public static class StoreReleaseBuild
    {
        [Serializable] sealed class Identity {public string iosBundleId,androidPackageName,appleTeamId;}
        [Serializable] sealed class Signing {public string keystorePath,alias,storePassword,keyPassword;}
        public static void IOS()=>Build(BuildTarget.iOS);
        public static void Android()=>Build(BuildTarget.Android);
        static void Build(BuildTarget target)
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed||EditorUserBuildSettings.activeBuildTarget!=target)throw new InvalidOperationException("Stopped, compiled target Editor required.");
            string root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            var identity=JsonUtility.FromJson<Identity>(File.ReadAllText(Path.Combine(root,"Configs/release-ids.json")));
            string output=Environment.GetEnvironmentVariable("FLYING_LADDERS_STORE_OUTPUT"),report=Environment.GetEnvironmentVariable("FLYING_LADDERS_STORE_REPORT");
            if(string.IsNullOrWhiteSpace(output)||!Path.IsPathRooted(output)||File.Exists(output)||Directory.Exists(output)||string.IsNullOrWhiteSpace(report))throw new InvalidOperationException("Fresh absolute build output and report required.");
            string revisionText=Environment.GetEnvironmentVariable("FLYING_LADDERS_STORE_REVISION");
            int revision=target==BuildTarget.iOS?1:2;
            if(!string.IsNullOrWhiteSpace(revisionText)&&(!int.TryParse(revisionText,out revision)||revision<1))throw new InvalidOperationException("Positive store build revision required.");
            Directory.CreateDirectory(report);Directory.CreateDirectory(Path.GetDirectoryName(output));
            string version=PlayerSettings.bundleVersion,product=PlayerSettings.productName;
            var group=target==BuildTarget.iOS?BuildTargetGroup.iOS:BuildTargetGroup.Android;
            string identifier=PlayerSettings.GetApplicationIdentifier(group),number=PlayerSettings.iOS.buildNumber,team=PlayerSettings.iOS.appleDeveloperTeamID;
            bool automatic=PlayerSettings.iOS.appleEnableAutomaticSigning,bundle=EditorUserBuildSettings.buildAppBundle,custom=PlayerSettings.Android.useCustomKeystore;
            var sdk=PlayerSettings.iOS.sdkVersion;string minIos=PlayerSettings.iOS.targetOSVersionString;
            int code=PlayerSettings.Android.bundleVersionCode;var targetSdk=PlayerSettings.Android.targetSdkVersion;var arch=PlayerSettings.Android.targetArchitectures;
            string store=PlayerSettings.Android.keystoreName,alias=PlayerSettings.Android.keyaliasName,storePass=PlayerSettings.Android.keystorePass,keyPass=PlayerSettings.Android.keyaliasPass;
            var named=target==BuildTarget.iOS?UnityEditor.Build.NamedBuildTarget.iOS:UnityEditor.Build.NamedBuildTarget.Android;var backend=PlayerSettings.GetScriptingBackend(named);
            Signing signing=null;
#if UNITY_ANDROID
            string androidSdk=AndroidExternalToolsSettings.sdkRootPath,androidNdk=AndroidExternalToolsSettings.ndkRootPath,androidJdk=AndroidExternalToolsSettings.jdkRootPath;
#endif
            try{
#if UNITY_ANDROID
                string tools=Environment.GetEnvironmentVariable("FLYING_LADDERS_ANDROID_TOOLS");
                if(!string.IsNullOrWhiteSpace(tools)){
                    foreach(string part in new[]{"SDK","NDK","OpenJDK"})if(!Directory.Exists(Path.Combine(tools,part)))throw new InvalidOperationException("Installed Android toolchain required.");
                    AndroidExternalToolsSettings.sdkRootPath=Path.Combine(tools,"SDK");AndroidExternalToolsSettings.ndkRootPath=Path.Combine(tools,"NDK");AndroidExternalToolsSettings.jdkRootPath=Path.Combine(tools,"OpenJDK");
                }
#endif
                PlayerSettings.productName="Flying Ladders";PlayerSettings.bundleVersion="1.0";PlayerSettings.SetScriptingBackend(named,ScriptingImplementation.IL2CPP);
                PlayerSettings.SetApplicationIdentifier(group,target==BuildTarget.iOS?identity.iosBundleId:identity.androidPackageName);
                if(target==BuildTarget.iOS){PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;PlayerSettings.iOS.appleEnableAutomaticSigning=true;PlayerSettings.iOS.appleDeveloperTeamID=identity.appleTeamId;PlayerSettings.iOS.buildNumber=revision.ToString(System.Globalization.CultureInfo.InvariantCulture);PlayerSettings.iOS.targetOSVersionString="15.6";}
                else{
                    signing=JsonUtility.FromJson<Signing>(File.ReadAllText(Environment.GetEnvironmentVariable("FLYING_LADDERS_SIGNING_FILE")));
                    if(signing==null||!File.Exists(signing.keystorePath)||string.IsNullOrWhiteSpace(signing.storePassword)||string.IsNullOrWhiteSpace(signing.keyPassword))throw new InvalidOperationException("Existing upload signing configuration required.");
                    PlayerSettings.Android.bundleVersionCode=revision;PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
                    EditorUserBuildSettings.buildAppBundle=true;PlayerSettings.Android.useCustomKeystore=true;PlayerSettings.Android.keystoreName=signing.keystorePath;PlayerSettings.Android.keyaliasName=signing.alias;PlayerSettings.Android.keystorePass=signing.storePassword;PlayerSettings.Android.keyaliasPass=signing.keyPassword;
                }
                WebsiteLegalConfigImport.Apply();Cascade.Editor.Playable.LocalizationAssets.BuildPlayerTables();
                var built=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Cascade/Scenes/CascadePlayable.unity"},locationPathName=output,target=target,options=Cascade.Editor.Playable.PlayerBuildStorage.OptionsFor(target,BuildOptions.DetailedBuildReport)});
                var s=built.summary;File.WriteAllText(Path.Combine(report,"unity-build.txt"),"Result "+s.result+"\nErrors "+s.totalErrors+"\nWarnings "+s.totalWarnings+"\nDuration "+s.totalTime+"\nOutput "+s.outputPath+"\n"+string.Join("\n",built.steps.SelectMany(x=>x.messages).Where(x=>x.type==LogType.Error||x.type==LogType.Exception).Select(x=>x.content.Split(new[]{"Environment Variables:"},StringSplitOptions.None)[0])));
                Cascade.Editor.Playable.PlayerBuildStorage.WriteSizeReport(built,report);
                if(s.result!=BuildResult.Succeeded)throw new InvalidOperationException("Store build failed; inspect report.");
            }finally{
#if UNITY_ANDROID
                AndroidExternalToolsSettings.sdkRootPath=androidSdk;AndroidExternalToolsSettings.ndkRootPath=androidNdk;AndroidExternalToolsSettings.jdkRootPath=androidJdk;
#endif
                PlayerSettings.productName=product;PlayerSettings.bundleVersion=version;PlayerSettings.SetApplicationIdentifier(group,identifier);PlayerSettings.SetScriptingBackend(named,backend);
                PlayerSettings.iOS.buildNumber=number;PlayerSettings.iOS.appleDeveloperTeamID=team;PlayerSettings.iOS.appleEnableAutomaticSigning=automatic;PlayerSettings.iOS.sdkVersion=sdk;PlayerSettings.iOS.targetOSVersionString=minIos;
                PlayerSettings.Android.bundleVersionCode=code;PlayerSettings.Android.targetSdkVersion=targetSdk;PlayerSettings.Android.targetArchitectures=arch;EditorUserBuildSettings.buildAppBundle=bundle;PlayerSettings.Android.useCustomKeystore=custom;
                PlayerSettings.Android.keystoreName=store;PlayerSettings.Android.keyaliasName=alias;PlayerSettings.Android.keystorePass=storePass;PlayerSettings.Android.keyaliasPass=keyPass;
                if(signing!=null){signing.storePassword=null;signing.keyPassword=null;}AssetDatabase.SaveAssets();
            }
        }
    }
}
