using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cascade.Presentation;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Release
{
    // Public IDs only. Verification secrets belong to the server, never this file.
    public static class AdMobConfigImport
    {
        sealed class Ids
        {
            public string iosAdmobAppId,iosRewardedUnitId,iosInterstitialUnitId;
            public string androidAdmobAppId,androidRewardedUnitId,androidInterstitialUnitId;
        }
        [MenuItem("CASCADE/Release/Apply AdMob IDs from Configs")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Apply IDs outside Play Mode.");
            string root=Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
            var ids=JsonConvert.DeserializeObject<Ids>(File.ReadAllText(Path.Combine(root,"Configs","admob.json")));
            if(ids==null)throw new InvalidDataException("Empty AdMob configuration.");
            var fields=new[]{("iosAdmobAppId",ids.iosAdmobAppId,true),("iosRewardedUnitId",ids.iosRewardedUnitId,false),
                ("iosInterstitialUnitId",ids.iosInterstitialUnitId,false),("androidAdmobAppId",ids.androidAdmobAppId,true),
                ("androidRewardedUnitId",ids.androidRewardedUnitId,false),("androidInterstitialUnitId",ids.androidInterstitialUnitId,false)};
            foreach(var field in fields)if(field.Item2==null||field.Item2.Contains("3940256099942544")
                ||!Regex.IsMatch(field.Item2,@"^ca-app-pub-\d{16}"+(field.Item3?"~":"/")+@"\d{10}$"))
                throw new InvalidDataException("Fill a publisher-owned ID: "+field.Item1);
            if(fields.Select(f=>f.Item2).Distinct(StringComparer.Ordinal).Count()!=6)throw new InvalidDataException("Each platform and placement needs its own ID.");
            var game=AssetDatabase.LoadAssetAtPath<MonetizationSettings>("Assets/_Cascade/Content/Resources/CascadeArt/MonetizationSettings.asset");
            var sdk=AssetDatabase.LoadMainAssetAtPath("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            if(game==null||sdk==null)throw new InvalidOperationException("AdMob settings assets are missing.");
            var serialized=new SerializedObject(sdk);var ios=serialized.FindProperty("adMobIOSAppId");var android=serialized.FindProperty("adMobAndroidAppId");
            if(ios==null||android==null)throw new InvalidOperationException("SDK App ID fields are unavailable.");
            Undo.RecordObjects(new UnityEngine.Object[]{game,sdk},"Apply CASCADE AdMob IDs");
            game.iosAdmobAppId=ids.iosAdmobAppId;game.iosRewardedUnitId=ids.iosRewardedUnitId;game.iosInterstitialUnitId=ids.iosInterstitialUnitId;
            game.androidAdmobAppId=ids.androidAdmobAppId;game.androidRewardedUnitId=ids.androidRewardedUnitId;game.androidInterstitialUnitId=ids.androidInterstitialUnitId;
            ios.stringValue=ids.iosAdmobAppId;android.stringValue=ids.androidAdmobAppId;serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(game);EditorUtility.SetDirty(sdk);AssetDatabase.SaveAssets();
            Debug.Log("CASCADE: six AdMob IDs applied to game and SDK settings. Release/test flags remain unchanged until production validation.");
        }
    }
}
