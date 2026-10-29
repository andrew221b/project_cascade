#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace AdManagerPro
{
    public class MobileAdsWindow : EditorWindow
    {
        bool testMode = false;
        bool nativeSDK = false;
        bool adaptiveBanner = false;

        string androidAppID = "";
        string androidBannerID = "";
        string androidInterstitialID = "";
        string androidRewardedVideoID = "";
        string androidRewardedInterstitialID = "";
        string androidNativeID = "";
        string androidAppOpenID = "";

        string iosAppID = "";
        string iosBannerID = "";
        string iosInterstitialID = "";
        string iosRewardedVideoID = "";
        string iosRewardedInterstitialID = "";
        string iosNativeID = "";
        string iosAppOpenID = "";

        Vector2 scrollPos;
        private const string settingsAssetPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
        MobileAdsSettings settings;

        [MenuItem("Tools/AdManager Pro - Easy Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<MobileAdsWindow>("AdManager Pro - Easy Setup");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            settings = AssetDatabase.LoadAssetAtPath<MobileAdsSettings>("Assets/AdManagerPro/Resources/MobileAdsSettings.asset");
            if (settings == null)
            {
                settings = CreateInstance<MobileAdsSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/AdManagerPro/Resources/MobileAdsSettings.asset");
                AssetDatabase.SaveAssets();
            }
            AdsSymbolManager.SyncSymbols();
            nativeSDK = AdsSymbolManager.IsGoogleMobileAdsNativeInstalled();
            LoadSettings();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("Step 1. Setup Admob SDK", EditorStyles.whiteLabel);
            GUILayout.Space(5);
            if (AdsSymbolManager.IsGoogleMobileAdsInstalled())
            {
                EditorGUILayout.HelpBox("Google Mobile Ads SDK detected.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Google Mobile Ads SDK not found. Download and import it, then reopen this window.", MessageType.Warning);
            }
            if (GUILayout.Button("Download Admob SDK"))
            {
                Application.OpenURL("https://github.com/googleads/googleads-mobile-unity/releases");
            }

            GUILayout.Space(10);
            var label = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                wordWrap = true
            };
            GUILayout.Label("If you will be using AdMob native ads, check this box and download the SDK.", label);
            GUILayout.Space(5);
            nativeSDK = EditorGUILayout.Toggle("Admob Native", nativeSDK);

            if (nativeSDK)
            {
                GUILayout.Space(10);
                GUILayout.Label("Step 1.1 Setup Admob Native SDK", EditorStyles.whiteLabel);
                GUILayout.Space(5);
                if (GUILayout.Button("Download Admob Native SDK"))
                {
                    Application.OpenURL("https://developers.google.com/admob/unity/native");
                }
            }
            GUILayout.Space(10);
            GUILayout.Label("Step 2. Enable Ad Testing Mode for Editor", EditorStyles.whiteLabel);
            GUILayout.Space(5);
            testMode = EditorGUILayout.Toggle("Test Mode", testMode);
            GUILayout.Space(5);
            if (testMode)
            {
                EditorGUILayout.HelpBox("Warning: Test Mode is enabled. Be sure to disable it before releasing your application.", MessageType.Warning);
            }
            GUILayout.Space(10);
            GUILayout.Label("Step 3. Enter Your Ad IDs. Do not use test IDs.", EditorStyles.whiteLabel);
            GUILayout.Space(5);
            adaptiveBanner = EditorGUILayout.Toggle("Adaptive Banner", adaptiveBanner);

            GUILayout.Space(5);
            GUILayout.Label("Android", EditorStyles.boldLabel);
            androidAppID = EditorGUILayout.TextField("App ID", androidAppID);
            androidBannerID = EditorGUILayout.TextField("Banner ID", androidBannerID);
            androidInterstitialID = EditorGUILayout.TextField("Interstitial ID", androidInterstitialID);
            androidRewardedVideoID = EditorGUILayout.TextField("Rewarded Video ID", androidRewardedVideoID);
            androidRewardedInterstitialID = EditorGUILayout.TextField("Rewarded Interstitial ID", androidRewardedInterstitialID);
            androidAppOpenID = EditorGUILayout.TextField("App Open ID", androidAppOpenID);
            if (nativeSDK)
            {
                androidNativeID = EditorGUILayout.TextField("Native Ads ID", androidNativeID);
            }

            GUILayout.Space(5);
            GUILayout.Label("iOS", EditorStyles.boldLabel);
            iosAppID = EditorGUILayout.TextField("App ID", iosAppID);
            iosBannerID = EditorGUILayout.TextField("Banner ID", iosBannerID);
            iosInterstitialID = EditorGUILayout.TextField("Interstitial ID", iosInterstitialID);
            iosRewardedVideoID = EditorGUILayout.TextField("Rewarded Video ID", iosRewardedVideoID);
            iosRewardedInterstitialID = EditorGUILayout.TextField("Rewarded Interstitial ID", iosRewardedInterstitialID);
            iosAppOpenID = EditorGUILayout.TextField("App Open ID", iosAppOpenID);
            if (nativeSDK)
            {
                iosNativeID = EditorGUILayout.TextField("Native Ads ID", iosNativeID);
            }

            GUILayout.Space(10);
            GUILayout.Label("Step 4. Save the Changes.", EditorStyles.whiteLabel);
            GUILayout.Space(5);
            if (GUILayout.Button("Save"))
            {
                SaveSettings();
            }
            GUILayout.Space(10);
            var labelStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                wordWrap = true
            };
            GUILayout.Label("If you have any questions, you can always refer to the documentation using the button below.", labelStyle);

            GUILayout.Space(10);
            if (GUILayout.Button("Documentation"))
            {
                Application.OpenURL("https://codeum-games.gitbook.io/admanager-pro-easy-setup");
            }
            GUILayout.Space(10);
            GUILayout.Label("To view the asset usage, click the Example Scene button.", labelStyle);
            GUILayout.Space(5);
            if (GUILayout.Button("Example Scene"))
            {
                string scenePath = "Assets/AdManagerPro/Example/Scenes/Example.unity";
                EditorSceneManager.OpenScene(scenePath);
            }

            EditorGUILayout.EndScrollView();
        }

        private void SaveSettings()
        {
            settings.testMode = testMode;
            settings.adaptiveBanner = adaptiveBanner;

            settings.androidAppID = androidAppID;
            settings.androidBannerID = androidBannerID;
            settings.androidInterstitialID = androidInterstitialID;
            settings.androidRewardedVideoID = androidRewardedVideoID;
            settings.androidRewardedInterstitialID = androidRewardedInterstitialID;
            settings.androidNativeID = androidNativeID;
            settings.androidAppOpenID = androidAppOpenID;

            settings.iosAppID = iosAppID;
            settings.iosBannerID = iosBannerID;
            settings.iosInterstitialID = iosInterstitialID;
            settings.iosRewardedVideoID = iosRewardedVideoID;
            settings.iosRewardedInterstitialID = iosRewardedInterstitialID;
            settings.iosNativeID = iosNativeID;
            settings.iosAppOpenID = iosAppOpenID;

            var settingsID = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAssetPath);
            if (settingsID != null)
            {
                var type = settingsID.GetType();
                bool dirty = false;
                dirty |= SetPrivateField(settingsID, type, "adMobAndroidAppId", testMode ? MobileAdsSettings.TestAppIDAndroid : androidAppID);
                dirty |= SetPrivateField(settingsID, type, "adMobIOSAppId", testMode ? MobileAdsSettings.TestAppIDIOS : iosAppID);
                if (dirty)
                {
                    EditorUtility.SetDirty(settingsID);
                }
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("Settings Saved");
        }

        private static bool SetPrivateField(ScriptableObject target, System.Type type, string fieldName, string value)
        {
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                return false;
            }
            field.SetValue(target, value);
            return true;
        }

        public void LoadSettings()
        {
            testMode = settings.testMode;
            adaptiveBanner = settings.adaptiveBanner;

            androidAppID = settings.androidAppID;
            androidBannerID = settings.androidBannerID;
            androidInterstitialID = settings.androidInterstitialID;
            androidRewardedVideoID = settings.androidRewardedVideoID;
            androidRewardedInterstitialID = settings.androidRewardedInterstitialID;
            androidNativeID = settings.androidNativeID;
            androidAppOpenID = settings.androidAppOpenID;

            iosAppID = settings.iosAppID;
            iosBannerID = settings.iosBannerID;
            iosInterstitialID = settings.iosInterstitialID;
            iosRewardedVideoID = settings.iosRewardedVideoID;
            iosRewardedInterstitialID = settings.iosRewardedInterstitialID;
            iosNativeID = settings.iosNativeID;
            iosAppOpenID = settings.iosAppOpenID;
        }
    }
}
#endif
