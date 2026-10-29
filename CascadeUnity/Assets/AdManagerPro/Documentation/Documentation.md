# AdManager Pro — Easy Setup

AdManager Pro is a lightweight wrapper around the Google Mobile Ads (AdMob) Unity SDK. It handles SDK initialization, ad loading, automatic reloading with retry backoff, and platform-specific ad unit IDs, so you can show an ad with a single line of code.

**Supported ad formats:** Banner, Interstitial, Rewarded, Rewarded Interstitial, App Open, Native (optional add-on).

---

## 1. Requirements

- Unity 2021.2 or newer (older versions are supported via a legacy API fallback).
- [Google Mobile Ads Unity SDK](https://github.com/googleads/googleads-mobile-unity/releases) (v8 or newer).
- Optional: [Google Mobile Ads Native SDK](https://developers.google.com/admob/unity/native) if you want native ads.
- TextMeshPro (only used by the native ads example).

---

## 2. Installation

1. Import **AdManager Pro** into your project.
2. Import the **Google Mobile Ads SDK** (from `.unitypackage` or via UPM — both are detected automatically).
3. That's it for code setup. AdManager Pro detects the SDK and enables its scripting define symbols on its own:
   - `ADS` — added when the Google Mobile Ads SDK is present.
   - `NATIVE_ADS` — added when the Native Ads SDK is also present.

The symbols are applied to the **Android, iOS and Standalone** build targets, so switching platforms never breaks the setup. If you ever remove the SDK from the project, the symbols are removed automatically and your project keeps compiling.

> All AdManager Pro runtime code is compiled only when the SDK is present, so the asset never causes compile errors in a project without AdMob.

---

## 3. Configuration (Easy Setup window)

Open **Tools → AdManager Pro - Easy Setup**.

The window walks you through four steps:

1. **Setup AdMob SDK** — shows whether the SDK is detected and links to the download. Enable the *AdMob Native* checkbox if you also use native ads.
2. **Test Mode** — while enabled, every ad request uses Google's official demo ad unit IDs (correct ones for Android and iOS are picked automatically). Use this during development. **Disable it before release** — the window shows a warning while it is on.
3. **Ad IDs** — enter your real ad unit IDs. There are separate sections for **Android** and **iOS**, because AdMob ad units are created per platform. At runtime the correct ID is chosen automatically based on the build platform. Fields you don't use can stay empty.
4. **Save** — writes the settings asset (`Assets/AdManagerPro/Resources/MobileAdsSettings.asset`) and also writes your Android and iOS **App IDs** directly into the Google Mobile Ads SDK settings, so you don't have to configure them twice.

---

## 4. How it works

### Initialization

You never call `MobileAds.Initialize` yourself. The first ad request triggers a single, shared SDK initialization (`AdManagerCore`). Requests made while the SDK is still initializing are queued and executed as soon as initialization completes — you can safely request ads from `Start()` on your first scene.

`AdManagerCore` also sets `MobileAds.RaiseAdEventsOnUnityMainThread = true`, so every ad callback is raised on the Unity main thread. You can safely touch your game objects and UI from any AdManager Pro callback.

### Automatic reloading and retry backoff

- Full-screen formats (Interstitial, Rewarded, Rewarded Interstitial, App Open) automatically load the next ad after the current one is closed.
- If a load fails (e.g. no network), the request is retried with an exponential delay: 2 s → 4 s → 8 s → … capped at 64 s. The delay resets after a successful load. This prevents request spam that could get an AdMob account throttled.

### Platform IDs and Test Mode

All ad unit ID resolution lives in `MobileAdsSettings`. At runtime it returns:

- your **Android** ID on Android, your **iOS** ID on iOS;
- the matching **Google demo ID** for the current platform while *Test Mode* is enabled.

---

## 5. Usage

A complete working example is included: open the Easy Setup window and press **Example Scene**, or see `Assets/AdManagerPro/Example/Scripts/AdsManager.cs`.

All managers are static — no prefabs or scene objects required.

### Banner

```csharp
using AdManagerPro;
using GoogleMobileAds.Api;

BannerAdManager.RequestAdBanner();                  // bottom by default
BannerAdManager.RequestAdBanner(AdPosition.Top);    // or any AdPosition

// The banner shows automatically once loaded.
BannerAdManager.HideAdBanner();
BannerAdManager.ShowAdBanner();     // show again after HideAdBanner
BannerAdManager.DestroyAdBanner();  // remove completely
```

Enable **Adaptive Banner** in the Easy Setup window to use Google's adaptive banner size instead of the standard 320×50 banner.

### Interstitial

```csharp
InterstitialAdManager.RequestAdInterstitial();  // preload (e.g. in Start)

// Later, at a natural break point:
InterstitialAdManager.ShowAdInterstitial();
```

After the ad is closed, the next interstitial is loaded automatically.

### Rewarded

```csharp
RewardedAdManager.RequestRewardedAd();  // preload

RewardedAdManager.ShowRewardedAd(watched =>
{
    if (watched)
    {
        // Grant the reward.
    }
    else
    {
        // Ad closed early, failed to show, or was not ready.
    }
});
```

The callback is guaranteed to be invoked **exactly once per call**:

| Situation                                   | Callback value |
|---------------------------------------------|:--------------:|
| User watched the ad and earned the reward   | `true`         |
| User closed the ad before earning the reward| `false`        |
| Ad failed to show                           | `false`        |
| Ad was not ready (a new load starts)        | `false`        |

### Rewarded Interstitial

Same contract as Rewarded:

```csharp
RewardedInterstitialAdManager.RequestRewardedInterstitialAd();

RewardedInterstitialAdManager.ShowRewardedInterstitialAd(watched =>
{
    if (watched) { /* grant reward */ }
});
```

### App Open

```csharp
AppOpenAdManager.LoadAppOpenAd();   // preload once (e.g. in Start)

// Show when the app returns from background:
private void OnApplicationPause(bool pause)
{
    if (!pause)
    {
        AppOpenAdManager.ShowAppOpenAd();
    }
}
```

A loaded App Open ad expires after 4 hours (per Google policy); an expired ad is never shown and a new one is loaded instead.

### Native (requires the Native Ads SDK)

```csharp
#if NATIVE_ADS
// Assign the UI targets, then request the ad.
NativeAdManager.adImage        = myIconImage;      // UnityEngine.UI.Image
NativeAdManager.adHeadlineText = myHeadlineText;   // TMP_Text
NativeAdManager.adBodyText     = myBodyText;       // TMP_Text

NativeAdManager.RequestNativeAd();
// The assigned UI elements are populated automatically when the ad loads.

NativeAdManager.DestroyNativeAd(); // when the UI is no longer shown
#endif
```

> Wrap native ad code in `#if NATIVE_ADS` so your project still compiles when the Native SDK is not installed. Re-assign the UI references after loading a new scene.

### Checking readiness

Each manager exposes a readiness flag you can poll, e.g.:

```csharp
if (RewardedAdManager.isAdmobRewardedReady) { /* enable the reward button */ }
```

Flags: `isAdmobBannerReady`, `isAdmobInterstitialReady`, `isAdmobRewardedReady`, `isAdmobRewardedInterstitialReady`, `isAdmobAppOpenReady`, `isNativeAdReady`.

---

## 6. Troubleshooting

**"MobileAdsSettings not found in Resources folder."**
Open **Tools → AdManager Pro - Easy Setup** once — it creates the settings asset automatically.

**Ads compile away / managers not found.**
The `ADS` symbol is only set when the Google Mobile Ads SDK is detected. Import the SDK and let the editor recompile; check *Project Settings → Player → Scripting Define Symbols*.

**No ads in a release build.**
Make sure *Test Mode* is disabled and real ad unit IDs are entered for the platform you are building (Android and iOS have separate fields).

**Load errors in the console (no fill / network error).**
This is normal — the manager retries automatically with increasing delays. Nothing to do on your side.

**Native ads: UI not updating after scene change.**
Static references to destroyed UI objects are skipped safely; re-assign `NativeAdManager.adImage` / `adHeadlineText` / `adBodyText` in the new scene and the next loaded ad will populate them.

---

## 7. Support

- Documentation: https://codeum-games.gitbook.io/admanager-pro-easy-setup
- Contact: via the Asset Store publisher page.
