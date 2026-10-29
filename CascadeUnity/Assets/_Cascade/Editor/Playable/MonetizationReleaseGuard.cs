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
            if(Cascade.Editor.Playable.StoreCaptureBuildScope.Active){Cascade.Editor.Playable.StoreCaptureBuildScope.Validate(report);return;}
            var s=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            if(s==null)throw new BuildFailedException("Missing CASCADE monetization settings.");
            if(AndroidInternalPreviewScope.Active){AndroidInternalPreviewScope.Validate(report);return;}
            if(!RequiresReleaseChecks(s.productionRelease,report.summary.options))return;
            if(s.testMonetization||s.admobTestAds||!s.adsReleaseReady||!s.storeCatalogRegistered)
                throw new BuildFailedException("Store submission requires live ads, a registered store catalog and the test shop disabled.");
            if(!PublicReceiptVerifier.ValidEndpoint(s.receiptValidationUrl))
                throw new BuildFailedException("Store submission requires the exact HTTPS local receipt verification endpoint for under-16 profiles.");
            var cloud=Resources.Load<CloudPlayerSettings>("CascadeArt/CloudPlayerSettings");
            if(cloud==null||!cloud.enabledForPlayers||!cloud.purchaseVerificationConfigured||!cloud.playerAccountsConfigured||cloud.environment!="production"||cloud.projectId!=UnityEngine.Application.cloudProjectId)
                throw new BuildFailedException("Store submission requires verified production cloud saves, linked accounts and server receipt verification.");
            bool ios=report.summary.platform==BuildTarget.iOS;string appId=ios?s.iosAdmobAppId:s.androidAdmobAppId;
            if(!Valid(appId,true)||!Valid(s.AdUnit(ios,true),false)||!Valid(s.AdUnit(ios,false),false))
                throw new BuildFailedException("Store submission requires publisher AdMob App, rewarded and interstitial IDs for this platform.");
            var sdk=AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            if(sdk==null||new SerializedObject(sdk).FindProperty(ios?"adMobIOSAppId":"adMobAndroidAppId").stringValue!=appId)
                throw new BuildFailedException("Google Mobile Ads App ID must match CASCADE's platform configuration.");
        }
        public static bool RequiresReleaseChecks(bool production,BuildOptions options)=>production||(options&BuildOptions.Development)==0;
        public static bool Valid(string id,bool app)=>id!=null&&!id.StartsWith("ca-app-pub-3940256099942544",StringComparison.Ordinal)&&Regex.IsMatch(id,app?@"^ca-app-pub-\d{16}~\d{10}$":@"^ca-app-pub-\d{16}/\d{10}$");
    }
}
