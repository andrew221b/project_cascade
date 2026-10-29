using System;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_ANDROID
using System.Xml.Linq;
using UnityEditor.Android;
#endif

namespace Cascade.Editor.Release
{
    // Explicit offline playtest route. It never enables the production service switches.
    public static class AndroidInternalPreviewBuild
    {
        [Serializable] sealed class Identity { public string androidPackageName; }
        [Serializable] sealed class Signing { public string keystorePath,alias,storePassword,keyPassword; }
        public static bool Active { get; private set; }
        public static string ReportDirectory { get; private set; }

        public static void Build()
        {
            if(Active||EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed||EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)
                throw new InvalidOperationException("Stopped, compiled Android Editor required.");
            string root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            var identity=JsonUtility.FromJson<Identity>(File.ReadAllText(Path.Combine(root,"Configs/release-ids.json")));
            if(identity?.androidPackageName!="com.vickand.flyingladders")throw new InvalidOperationException("Owner's registered Android identity required.");
            string credentials=Environment.GetEnvironmentVariable("FLYING_LADDERS_SIGNING_FILE");
            if(string.IsNullOrWhiteSpace(credentials))throw new InvalidOperationException("Protected signing configuration required.");
            var signing=JsonUtility.FromJson<Signing>(File.ReadAllText(credentials));
            if(signing==null||!File.Exists(signing.keystorePath)||string.IsNullOrWhiteSpace(signing.alias)||string.IsNullOrWhiteSpace(signing.storePassword)||string.IsNullOrWhiteSpace(signing.keyPassword))
                throw new InvalidOperationException("Complete upload signing required.");
            string output=Environment.GetEnvironmentVariable("FLYING_LADDERS_INTERNAL_AAB");
            ReportDirectory=Environment.GetEnvironmentVariable("FLYING_LADDERS_INTERNAL_REPORT");
            if(string.IsNullOrWhiteSpace(output)||!Path.IsPathRooted(output)||!output.EndsWith(".aab",StringComparison.Ordinal)||File.Exists(output)||string.IsNullOrWhiteSpace(ReportDirectory))
                throw new InvalidOperationException("Fresh absolute AAB output and report directory required.");
            Directory.CreateDirectory(Path.GetDirectoryName(output));Directory.CreateDirectory(ReportDirectory);
            string package=PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),product=PlayerSettings.productName,version=PlayerSettings.bundleVersion;
            string keystore=PlayerSettings.Android.keystoreName,alias=PlayerSettings.Android.keyaliasName,storePassword=PlayerSettings.Android.keystorePass,keyPassword=PlayerSettings.Android.keyaliasPass;
            bool custom=PlayerSettings.Android.useCustomKeystore,bundle=EditorUserBuildSettings.buildAppBundle;
            int code=PlayerSettings.Android.bundleVersionCode;var target=PlayerSettings.Android.targetSdkVersion;var architecture=PlayerSettings.Android.targetArchitectures;
            var backend=PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android);
#if UNITY_ANDROID
            string sdk=AndroidExternalToolsSettings.sdkRootPath,ndk=AndroidExternalToolsSettings.ndkRootPath,jdk=AndroidExternalToolsSettings.jdkRootPath;
#endif
            try {
#if UNITY_ANDROID
                string tools=Environment.GetEnvironmentVariable("FLYING_LADDERS_ANDROID_TOOLS");
                if(!string.IsNullOrWhiteSpace(tools)) {
                    foreach(string folder in new[]{"SDK","NDK","OpenJDK"})if(!Directory.Exists(Path.Combine(tools,folder)))throw new InvalidOperationException("Complete Android tools required.");
                    AndroidExternalToolsSettings.sdkRootPath=Path.Combine(tools,"SDK");AndroidExternalToolsSettings.ndkRootPath=Path.Combine(tools,"NDK");AndroidExternalToolsSettings.jdkRootPath=Path.Combine(tools,"OpenJDK");
                }
#endif
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,identity.androidPackageName);
                PlayerSettings.productName="Flying Ladders";PlayerSettings.bundleVersion="1.0";PlayerSettings.Android.bundleVersionCode=1;
                PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
                PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.useCustomKeystore=true;PlayerSettings.Android.keystoreName=signing.keystorePath;
                PlayerSettings.Android.keyaliasName=signing.alias;PlayerSettings.Android.keystorePass=signing.storePassword;PlayerSettings.Android.keyaliasPass=signing.keyPassword;
                EditorUserBuildSettings.buildAppBundle=true;
                WebsiteLegalConfigImport.Apply();
                Cascade.Editor.Playable.LocalizationAssets.BuildPlayerTables();
                using var scope=Cascade.Editor.Playable.AndroidInternalPreviewScope.Begin();
                Active=true;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Cascade/Scenes/CascadePlayable.unity"},locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None});
                File.WriteAllText(Path.Combine(ReportDirectory,"unity-build.txt"),"Result "+report.summary.result+"\nErrors "+report.summary.totalErrors+"\nWarnings "+report.summary.totalWarnings+"\nDuration "+report.summary.totalTime+"\nOutput "+report.summary.outputPath+"\n"+
                    string.Join("\n",report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content.Split(new[]{"Environment Variables:"},StringSplitOptions.None)[0])));
                if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Android internal preview failed; inspect protected build log.");
            } finally {
                Active=false;
#if UNITY_ANDROID
                AndroidExternalToolsSettings.sdkRootPath=sdk;AndroidExternalToolsSettings.ndkRootPath=ndk;AndroidExternalToolsSettings.jdkRootPath=jdk;
#endif
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,package);PlayerSettings.productName=product;PlayerSettings.bundleVersion=version;
                PlayerSettings.Android.bundleVersionCode=code;PlayerSettings.Android.targetSdkVersion=target;PlayerSettings.Android.targetArchitectures=architecture;
                PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,backend);
                PlayerSettings.Android.useCustomKeystore=custom;PlayerSettings.Android.keystoreName=keystore;PlayerSettings.Android.keyaliasName=alias;
                PlayerSettings.Android.keystorePass=storePassword;PlayerSettings.Android.keyaliasPass=keyPassword;EditorUserBuildSettings.buildAppBundle=bundle;
                signing.storePassword=null;signing.keyPassword=null;AssetDatabase.SaveAssets();
            }
        }
    }
#if UNITY_ANDROID
    public sealed class AndroidInternalPreviewManifest:IPostGenerateGradleAndroidProject
    {
        public int callbackOrder=>10000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if(!AndroidInternalPreviewBuild.Active)return;
            string file=Path.Combine(path,"src/main/AndroidManifest.xml");var doc=XDocument.Load(file);var root=doc.Root;
            XNamespace android="http://schemas.android.com/apk/res/android",tools="http://schemas.android.com/tools";
            root.SetAttributeValue(XNamespace.Xmlns+"tools",tools.NamespaceName);
            foreach(string permission in new[]{"com.google.android.gms.permission.AD_ID","android.permission.ACCESS_ADSERVICES_AD_ID","android.permission.ACCESS_ADSERVICES_ATTRIBUTION","android.permission.ACCESS_ADSERVICES_TOPICS"})
                root.Add(new XElement("uses-permission",new XAttribute(android+"name",permission),new XAttribute(tools+"node","remove")));
            if(!root.Elements("uses-permission").Any(e=>(string)e.Attribute(android+"name")=="com.android.vending.BILLING"))
                root.Add(new XElement("uses-permission",new XAttribute(android+"name","com.android.vending.BILLING")));
            var app=root.Element("application");if(app==null){app=new XElement("application");root.Add(app);}
            app.Add(new XElement("provider",new XAttribute(android+"name","com.google.android.gms.ads.MobileAdsInitProvider"),new XAttribute(android+"authorities","${applicationId}.mobileadsinitprovider"),new XAttribute(tools+"node","remove")));
            doc.Save(file);File.Copy(file,Path.Combine(AndroidInternalPreviewBuild.ReportDirectory,"internal-unity-library-manifest.xml"),true);
        }
    }
#endif
}
