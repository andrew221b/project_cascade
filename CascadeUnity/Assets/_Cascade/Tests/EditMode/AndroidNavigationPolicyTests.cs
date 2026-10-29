#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
namespace Cascade.Tests
{
    public sealed class AndroidNavigationPolicyTests
    {
        const string Ns="http://schemas.android.com/apk/res/android";
        Type Policy=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.Editor.Release.AndroidNavigationBuildPolicy")).First(t=>t!=null);
        void Invoke(string method,object argument)=>Policy.GetMethod(method).Invoke(null,new[]{argument});
        [Test] public void AndroidBuildRequiresTheUnaffectedActivityEntry()
        {
            var before=PlayerSettings.Android.applicationEntry;
            try{
                PlayerSettings.Android.applicationEntry=AndroidApplicationEntry.Activity;Assert.DoesNotThrow(()=>Invoke("ValidateEntry",BuildTarget.Android));
                PlayerSettings.Android.applicationEntry=AndroidApplicationEntry.GameActivity;
                var error=Assert.Throws<TargetInvocationException>(()=>Invoke("ValidateEntry",BuildTarget.Android));Assert.That(error.InnerException,Is.InstanceOf<BuildFailedException>());
                Assert.DoesNotThrow(()=>Invoke("ValidateEntry",BuildTarget.iOS));
            }finally{PlayerSettings.Android.applicationEntry=before;}
        }
        [Test] public void FutureMinificationKeepsJniNamesWithoutReplacingUnityRules()
        {
            string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".pro");
            try{
                File.WriteAllText(path,"-keep class com.unity3d.player.** { *; }\n");Invoke("ConfigureKeepRules",path);string first=File.ReadAllText(path);
                Invoke("ConfigureKeepRules",path);Assert.That(File.ReadAllText(path),Is.EqualTo(first));
                Assert.That(first,Does.Contain("com.unity3d.player.**"));Assert.That(first,Does.Contain("com.vickand.flyingladders.platform.CascadeAndroidBack**"));
            }finally{File.Delete(path);}
        }
        [TestCase("com.unity3d.player.UnityPlayerActivity",false)]
        [TestCase("com.unity3d.player.UnityPlayerGameActivity",true)]
        public void ManifestOptInPreservesSdkActivitiesAndRejectsGameActivity(string entry,bool rejected)
        {
            string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".xml");
            try{
                File.WriteAllText(path,"<manifest xmlns:android='"+Ns+"'><application><activity android:name='"+entry+"'/><activity android:name='com.google.android.gms.ads.AdActivity' android:enableOnBackInvokedCallback='false'/></application></manifest>");
                if(rejected){var error=Assert.Throws<TargetInvocationException>(()=>Invoke("ConfigureManifest",path));Assert.That(error.InnerException,Is.InstanceOf<BuildFailedException>());return;}
                Invoke("ConfigureManifest",path);var doc=new XmlDocument();doc.Load(path);var app=doc.DocumentElement["application"];
                Assert.That(app.GetAttribute("enableOnBackInvokedCallback",Ns),Is.EqualTo("true"));
                Assert.That(((XmlElement)app.SelectNodes("activity")[1]).GetAttribute("enableOnBackInvokedCallback",Ns),Is.EqualTo("false"));
                Invoke("ConfigureManifest",path);Assert.That(app.SelectNodes("activity").Count,Is.EqualTo(2));
            }finally{File.Delete(path);}
        }
    }
}
#endif
