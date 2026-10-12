using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class StoreSimulatorBuild
    {
        public static void Export()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Stopped compiled Editor required.");
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.iOS)
                throw new InvalidOperationException("Switch to the installed iOS target first.");
            string root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            string report=Path.Combine(root,"Reports/StoreAssets-Oct09/SimulatorBuild");Directory.CreateDirectory(report);
            string output=Path.Combine(root,"CascadeUnity/Builds/StoreSimulator-Oct09-final");
            if(Directory.Exists(output))throw new InvalidOperationException("Existing Simulator export preserved; use a new folder.");
            var sdk=PlayerSettings.iOS.sdkVersion;var architecture=PlayerSettings.iOS.simulatorSdkArchitecture;
            string identifier=PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS),product=PlayerSettings.productName;
            try{
                PlayerSettings.iOS.sdkVersion=iOSSdkVersion.SimulatorSDK;
                PlayerSettings.iOS.simulatorSdkArchitecture=(AppleMobileArchitectureSimulator)Enum.Parse(typeof(AppleMobileArchitectureSimulator),"ARM64",true);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,"com.vickand.cascade.capture");PlayerSettings.productName="CASCADE";
                LocalizationAssets.BuildPlayerTables();
                File.WriteAllText(Path.Combine(report,"status.txt"),"Building");
                var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/_Cascade/Scenes/CascadePlayable.unity"},
                    locationPathName=output,target=BuildTarget.iOS,options=BuildOptions.Development,
                    extraScriptingDefines=new[]{"CASCADE_STORE_CAPTURE"}});
                var s=build.summary;
                File.WriteAllText(Path.Combine(report,"unity-build.txt"),"Result "+s.result+"\nErrors "+s.totalErrors+"\nWarnings "+s.totalWarnings+"\nDuration "+s.totalTime+"\nOutput "+s.outputPath+"\n"+
                    string.Join("\n",build.steps.SelectMany(x=>x.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content)));
                File.WriteAllText(Path.Combine(report,"status.txt"),s.result.ToString());
                if(s.result!=BuildResult.Succeeded)throw new InvalidOperationException("Simulator export failed; inspect report.");
            }finally{
                PlayerSettings.iOS.sdkVersion=sdk;PlayerSettings.iOS.simulatorSdkArchitecture=architecture;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,identifier);PlayerSettings.productName=product;
            }
        }
    }
}
