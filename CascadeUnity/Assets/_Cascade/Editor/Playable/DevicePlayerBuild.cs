using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    // Explicit local-device build only; preserve project/release settings and player saves.
    public static class DevicePlayerBuild
    {
        [Serializable] sealed class StoreIdentity { public string iosBundleId,appleTeamId; }
        [Serializable] sealed class StoreListing { public string version; }
        public static void Build()
        {
            if(EditorApplication.isPlaying||EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Stopped, compiled Editor required.");
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.iOS)
                throw new InvalidOperationException("iOS target required.");
            string root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            string report=Path.Combine(root,Environment.GetEnvironmentVariable("CASCADE_DEVICE_REPORT_DIRECTORY")??"Reports/iPhoneInstall-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(report);
            var identity=JsonUtility.FromJson<StoreIdentity>(File.ReadAllText(Path.Combine(root,"Configs/release-ids.json")));
            var listing=JsonUtility.FromJson<StoreListing>(File.ReadAllText(Path.Combine(root,"Configs/app-store-connect.json")));
            if(string.IsNullOrWhiteSpace(identity?.iosBundleId)||string.IsNullOrWhiteSpace(identity.appleTeamId)||string.IsNullOrWhiteSpace(listing?.version))
                throw new InvalidOperationException("Configured Apple store identity and version required.");
            string buildNumber=Environment.GetEnvironmentVariable("CASCADE_DEVICE_BUILD_NUMBER")??"202610103";
            if(buildNumber.Length==0||buildNumber.Any(c=>c<'0'||c>'9'))throw new ArgumentException("Numeric device build number required.");
            string output=Path.Combine(root,"CascadeUnity/Builds/iPhone-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            var sdk=PlayerSettings.iOS.sdkVersion;var automatic=PlayerSettings.iOS.appleEnableAutomaticSigning;
            string identifier=PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS),product=PlayerSettings.productName,version=PlayerSettings.bundleVersion;
            string team=PlayerSettings.iOS.appleDeveloperTeamID,number=PlayerSettings.iOS.buildNumber;
            try {
                PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,identity.iosBundleId);
                PlayerSettings.productName="Flying Ladders";PlayerSettings.bundleVersion=listing.version;PlayerSettings.iOS.appleDeveloperTeamID=identity.appleTeamId;
                PlayerSettings.iOS.appleEnableAutomaticSigning=true;PlayerSettings.iOS.buildNumber=buildNumber;
                LocalizationAssets.BuildPlayerTables();
                File.WriteAllText(Path.Combine(report,"export-path.txt"),output);
                File.WriteAllText(Path.Combine(report,"unity-status.txt"),"Building");
                var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=new[]{"Assets/_Cascade/Scenes/CascadePlayable.unity"},locationPathName=output,
                    target=BuildTarget.iOS,options=PlayerBuildStorage.OptionsFor(BuildTarget.iOS,BuildOptions.Development|BuildOptions.DetailedBuildReport)});
                var s=build.summary;
                File.WriteAllText(Path.Combine(report,"unity-build.txt"),"Result "+s.result+"\nErrors "+s.totalErrors+"\nWarnings "+s.totalWarnings+"\nDuration "+s.totalTime+"\nOutput "+s.outputPath+"\n"+
                    string.Join("\n",build.steps.SelectMany(x=>x.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content)));
                PlayerBuildStorage.WriteSizeReport(build,report);
                File.WriteAllText(Path.Combine(report,"unity-status.txt"),s.result.ToString());
                if(s.result!=BuildResult.Succeeded)throw new InvalidOperationException("Device export failed; inspect report.");
            } finally {
                PlayerSettings.iOS.sdkVersion=sdk;PlayerSettings.iOS.appleEnableAutomaticSigning=automatic;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,identifier);PlayerSettings.productName=product;PlayerSettings.bundleVersion=version;
                PlayerSettings.iOS.appleDeveloperTeamID=team;PlayerSettings.iOS.buildNumber=number;
                AssetDatabase.SaveAssets();
            }
        }
    }
}
