using System;
using System.Text.RegularExpressions;
using Cascade.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public sealed class MonetizationReleaseGuard:IPreprocessBuildWithReport
    {
        public int callbackOrder=>-1000;
        public void OnPreprocessBuild(BuildReport report)
        {
            if(report.summary.platform!=BuildTarget.iOS&&report.summary.platform!=BuildTarget.Android)return;
            var s=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            if(s==null)throw new BuildFailedException("Missing CASCADE monetization settings.");
            if(!s.productionRelease)return;
            if(s.testMonetization||s.admobTestAds||!s.adsReleaseReady||!s.storeCatalogRegistered)
                throw new BuildFailedException("Store submission requires live ads, a registered store catalog and the test shop disabled.");
            if(!Uri.TryCreate(s.receiptValidationUrl,UriKind.Absolute,out var url)||url.Scheme!="https")
                throw new BuildFailedException("Store submission requires a publisher HTTPS receipt verifier.");
            bool ios=report.summary.platform==BuildTarget.iOS;string appId=ios?s.iosAdmobAppId:s.androidAdmobAppId;
            if(!Valid(appId,true)||!Valid(s.AdUnit(ios,true),false)||!Valid(s.AdUnit(ios,false),false))
                throw new BuildFailedException("Store submission requires publisher AdMob App, rewarded and interstitial IDs for this platform.");
            var sdk=AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            if(sdk==null||new SerializedObject(sdk).FindProperty(ios?"adMobIOSAppId":"adMobAndroidAppId").stringValue!=appId)
                throw new BuildFailedException("Google Mobile Ads App ID must match CASCADE's platform configuration.");
        }
        public static bool Valid(string id,bool app)=>id!=null&&!id.StartsWith("ca-app-pub-3940256099942544",StringComparison.Ordinal)&&Regex.IsMatch(id,app?@"^ca-app-pub-\d{16}~\d{10}$":@"^ca-app-pub-\d{16}/\d{10}$");
    }
}
