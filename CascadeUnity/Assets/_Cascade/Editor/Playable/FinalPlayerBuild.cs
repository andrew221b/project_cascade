using System;
using System.IO;
using System.Linq;
using Cascade.Editor.Playable;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cascade.Editor.Validation
{
    public static class FinalPlayerBuild
    {
        public static void BuildMac()=>BuildMacTo("FinalPlayer","CASCADE.app");
        public static void BuildDevicesMac()=>BuildMacTo("Devices","CASCADE-Devices.app");
        public static void BuildCraftMac()=>BuildMacTo("Craft","CASCADE-Craft.app");
        static void BuildMacTo(string reportFolder,string appName)
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Compiled stopped Editor required");
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneOSX)throw new InvalidOperationException("Mac target must already be active");
            string root=Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            string folder=Path.GetFullPath(Path.Combine(root,"../Reports/"+reportFolder));Directory.CreateDirectory(folder);
            var architecture=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.OSXStandalone.UserBuildSettings")).Where(t=>t!=null).Select(t=>t.GetProperty("architecture").GetValue(null)).FirstOrDefault();
            const string scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
            if(!File.Exists(scene))throw new FileNotFoundException(scene);
            string output=Path.Combine(root,"Builds/"+appName);Directory.CreateDirectory(Path.GetDirectoryName(output));
            if(Directory.Exists(output))throw new InvalidOperationException("Preserve existing build; select a fresh output before rebuilding");
            File.WriteAllText(folder+"/baseline.txt","Unity "+UnityEngine.Application.unityVersion+"\nTarget "+EditorUserBuildSettings.activeBuildTarget+"\nArchitecture "+architecture+"\nBackend "+PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone)+"\nProduct "+PlayerSettings.productName+"\nVersion "+PlayerSettings.bundleVersion+"\nScene "+scene+"\nGraphics "+string.Join(",",PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneOSX))+"\n");
            LocalizationAssets.BuildPlayerTables();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName=output,target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
            var summary=report.summary;var messages=report.steps.SelectMany(step=>step.messages).Where(m=>m.type==LogType.Warning||m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.type+": "+m.content);
            File.WriteAllText(folder+"/build.txt","Result "+summary.result+"\nStart "+summary.buildStartedAt.ToUniversalTime().ToString("O")+"\nEnd "+summary.buildEndedAt.ToUniversalTime().ToString("O")+"\nDuration "+summary.totalTime+"\nBytes "+summary.totalSize+"\nErrors "+summary.totalErrors+"\nWarnings "+summary.totalWarnings+"\nOutput "+summary.outputPath+"\n"+string.Join("\n",messages)+"\n");
            if(summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Mac build failed; inspect Reports/FinalPlayer/build.txt");
            Debug.Log("CASCADE final Mac build succeeded: "+output);
        }
    }
}
