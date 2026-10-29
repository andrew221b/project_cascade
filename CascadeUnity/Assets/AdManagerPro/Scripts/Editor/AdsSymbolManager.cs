#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

namespace AdManagerPro
{
    /// <summary>
    /// Keeps the ADS / NATIVE_ADS scripting define symbols in sync with the
    /// presence of the Google Mobile Ads SDK. Detection is done by type, so
    /// it works whether the SDK is installed in Assets or via UPM. Symbols
    /// are also removed when the SDK is deleted, so the project keeps
    /// compiling.
    /// </summary>
    [InitializeOnLoad]
    public static class AdsSymbolManager
    {
        private const string AdsSymbol = "ADS";
        private const string NativeAdsSymbol = "NATIVE_ADS";

        private static readonly BuildTargetGroup[] TargetGroups =
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
            BuildTargetGroup.Standalone,
        };

        static AdsSymbolManager()
        {
            EditorApplication.delayCall += SyncSymbols;
        }

        public static void SyncSymbols()
        {
            bool sdkPresent = IsTypePresent("GoogleMobileAds.Api.MobileAds");
            bool nativeSdkPresent = IsTypePresent("GoogleMobileAds.Api.NativeAd");

            bool adsChanged = SetSymbolEverywhere(AdsSymbol, sdkPresent);
            bool nativeChanged = SetSymbolEverywhere(NativeAdsSymbol, nativeSdkPresent);

            if (adsChanged)
            {
                Debug.Log(sdkPresent
                    ? "AdManagerPro: Google Mobile Ads SDK detected, ADS symbol enabled."
                    : "AdManagerPro: Google Mobile Ads SDK not found, ADS symbol removed.");
            }
            if (nativeChanged)
            {
                Debug.Log(nativeSdkPresent
                    ? "AdManagerPro: Google Mobile Ads Native SDK detected, NATIVE_ADS symbol enabled."
                    : "AdManagerPro: Google Mobile Ads Native SDK not found, NATIVE_ADS symbol removed.");
            }
        }

        public static bool IsGoogleMobileAdsInstalled()
        {
            return IsTypePresent("GoogleMobileAds.Api.MobileAds");
        }

        public static bool IsGoogleMobileAdsNativeInstalled()
        {
            return IsTypePresent("GoogleMobileAds.Api.NativeAd");
        }

        private static bool IsTypePresent(string fullTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType(fullTypeName, false) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool SetSymbolEverywhere(string symbol, bool enabled)
        {
            bool changed = false;
            foreach (var group in TargetGroups)
            {
                List<string> symbols = GetSymbols(group);
                // Exact match — a plain Contains would confuse ADS with NATIVE_ADS.
                bool present = symbols.Contains(symbol);

                if (enabled && !present)
                {
                    symbols.Add(symbol);
                    SetSymbols(group, symbols);
                    changed = true;
                }
                else if (!enabled && present)
                {
                    symbols.RemoveAll(s => s == symbol);
                    SetSymbols(group, symbols);
                    changed = true;
                }
            }
            return changed;
        }

        private static List<string> GetSymbols(BuildTargetGroup group)
        {
#if UNITY_2021_2_OR_NEWER
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
#else
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
            return defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        }

        private static void SetSymbols(BuildTargetGroup group, List<string> symbols)
        {
            string defines = string.Join(";", symbols);
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group), defines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
#endif
        }
    }
}
#endif
