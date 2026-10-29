using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Android;
namespace Cascade.Editor.Release
{
    public sealed class AndroidNavigationBuildPolicy:IPreprocessBuildWithReport,IPostGenerateGradleAndroidProject
    {
        public int callbackOrder=>10000;
        public void OnPreprocessBuild(BuildReport report){ValidateEntry(report.summary.platform);}
        public static void ValidateEntry(BuildTarget target)
        {
            if(target==BuildTarget.Android&&PlayerSettings.Android.applicationEntry!=AndroidApplicationEntry.Activity)
                throw new BuildFailedException("Flying Ladders requires Activity while using this Unity patch; affected GameActivity must not ship.");
        }
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            ConfigureManifest(Path.Combine(path,"src/main/AndroidManifest.xml"));
            ConfigureKeepRules(Path.Combine(path,"proguard-unity.txt"));
        }
        public static void ConfigureKeepRules(string path)
        {
            // JNI names live in C# strings; a future R8 toggle must not strip them.
            const string rule="-keep class com.vickand.flyingladders.platform.CascadeAndroidBack** { *; }";
            string text=File.Exists(path)?File.ReadAllText(path):string.Empty;
            if(!text.Contains(rule))File.AppendAllText(path,"\n# Flying Ladders Android Back JNI bridge\n"+rule+"\n");
        }
        public static void ConfigureManifest(string path)
        {
            const string ns="http://schemas.android.com/apk/res/android";
            var doc=new XmlDocument();doc.Load(path);var app=doc.DocumentElement["application"];
            if(app==null)throw new BuildFailedException("Missing Android application.");
            foreach(XmlElement activity in app.SelectNodes("activity"))
                if(activity.GetAttribute("name",ns).Contains("UnityPlayerGameActivity"))throw new BuildFailedException("Affected GameActivity found in generated manifest.");
            app.SetAttribute("enableOnBackInvokedCallback",ns,"true");doc.Save(path);
        }
    }
}
