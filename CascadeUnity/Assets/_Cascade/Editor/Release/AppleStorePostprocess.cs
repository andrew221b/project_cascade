#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
namespace Cascade.Editor.Release
{
    public static class AppleStorePostprocess
    {
        [PostProcessBuild(1000)]
        public static void Apply(BuildTarget target,string path)
        {
            if(target!=BuildTarget.iOS)return;
            var project=new PBXProject();string file=PBXProject.GetPBXProjectPath(path);project.ReadFromFile(file);
            project.AddFrameworkToProject(project.GetUnityFrameworkTargetGuid(),"AppTrackingTransparency.framework",false);
            project.WriteToFile(file);
            var plist=new PlistDocument();file=Path.Combine(path,"Info.plist");plist.ReadFromFile(file);
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption",false);
            plist.root.SetString("NSUserTrackingUsageDescription","This lets our advertising partners show relevant ads and measure their performance. You can keep playing without allowing tracking.");
            plist.WriteToFile(file);
        }
    }
}
#endif
