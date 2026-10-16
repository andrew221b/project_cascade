# AdMob migration and store activation — 7 October 2026

User requested replacing LevelPlay with AdMob. Remove com.unity.services.levelplay,
its imported LevelPlay descriptors, and its Mobile Dependency Resolver copy; install
official Google Mobile Ads Unity 11.5.0 (Google release) including UMP and one
External Dependency Manager. Preserve Unity IAP 5.4.4, all asset GUIDs outside those
vendor imports, and existing Domain/solver rules, reward amounts and ad frequency.
Historical iOS exports are preserved; they still contain the former SDK.

Existing store catalog has one non-consumable, cascade.no_automatic_ads. Its ID
and localized price contract remain unchanged. The user authorized the coin packs and constraints below. No store prices or
paid puzzle strength are invented.
Store and ad testing are separate: ad test IDs can exercise Google's SDK while
fake store ownership remains session-only. Native UMP consent precedes ad SDK
initialization; required privacy options remain accessible in settings. No assumed
consent, tracking permission or adult status. Full-screen ads suspend input and
resume on close/failure; grants persist through the existing transaction boundary
and never issue puzzle commands.

Production credentials are external dependencies, never invented or committed as
secrets. Use Google's documented platform-specific sample App IDs/ad units until
publisher IDs are provided. Native IAP still needs registered products and a
hosted receipt verifier. Complete setup and focused validation here without
rebuilding/installing another phone version unless requested.

## Coin pack economy (authorized by the user)

Draft consumables: cascade.coins_240 and cascade.coins_480. No real currency
prices are invented; Apple/Google localized metadata is authoritative. 240 coins
is six ordinary 40-coin first clears, one 240-coin trophy, three 80-coin spools,
or two 120-coin one-step hints. 480 is double, without a bulk-value advantage.
Tool prices stay 80/100/120/160/200 and stock stays five per type. Earned currency
and paid currency buy identical items; cosmetics never grant puzzle power.

Hard gameplay constraint: at most two inventory tools total per stage, each type
once; one Wayfinder shows one next move, not the entire solution. Tool usage marks
the delivery assisted, limiting it to two stars and excluding clean records.
Two free undos remain available on the same board. Existing save fields derive
the aggregate tool limit; no profile schema change. Legacy attempts using more
than two tools remain loadable but cannot use additional tools.

A displayed-wallet threshold equal to the current maximum trophy price stops new coin-pack purchases (not earned
rewards/spending). Already-paid/pending transactions are still delivered, even
if they cross the threshold; never discard purchased value. Preview pack grants
are session-only and cannot leak into the real profile. No paywall, paid skips,
extra platform life, paid puzzle generation or paid competitive rank.

## Production switch and verifier preparation

Add an explicit productionRelease switch. A production build must reject test-shop
money, sample ads, missing publisher App/ad IDs, an unregistered catalog or absent
HTTPS verifier. Development/test Release builds remain available with the switch
false; Xcode Release alone must not imply live monetization. The Google editor
settings App ID must match the selected platform's publisher ID.

Prepare a separate Node receipt verifier with official Apple 3.1.0 and Google
Auth 11.1.0 libraries. Dependencies are pinned in its lockfile. The user has no
products or hosting yet, so do not deploy or claim real payments. This is a
receipt-validation boundary; cross-device currency/hostile save editing require
an authenticated server wallet and are not covered by local fairness constraints.

## Activation checklist and sample configuration

| Platform | Sample AdMob App ID | Rewarded sample unit | Interstitial sample unit |
| --- | --- | --- | --- |
| iOS | ca-app-pub-3940256099942544~1458002511 | ca-app-pub-3940256099942544/1712485313 | ca-app-pub-3940256099942544/4411468910 |
| Android | ca-app-pub-3940256099942544~3347511713 | ca-app-pub-3940256099942544/5224354917 | ca-app-pub-3940256099942544/1033173712 |

These are Google's public sample identifiers, not publisher credentials.
SDK configuration: Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset.
Game switches/production IDs/verifier URL:
Assets/_Cascade/Content/Resources/CascadeArt/MonetizationSettings.asset.
Test shop and AdMob sample ads are independently enabled. Inventory-tool prices
are hidden in the test showroom; native cash prices come only from store metadata.
Use a store price for 480 coins approximately double the 240 pack; the larger pack
contains no bonus coins. Do not add discounts that encourage stockpiling power.

Before live release:
1. Create the app and rewarded/interstitial units in AdMob for both platforms.
   Set publisher App IDs in both SDK and game settings, and unit IDs in game settings.
2. Configure AdMob privacy/consent messages for the app. Verify the UMP form,
   denial, required privacy options and age policy on physical devices. The current
   project does not request ATT or promise personalized tracking. Any additional
   tracking integration requires its actual disclosure/ATT flow before activation.
3. Register the three products above, sign store agreements and choose prices.
   No ads disables automatic interstitials; voluntary rewarded videos remain.
4. Deploy Backend/ReceiptVerifier behind HTTPS with publisher credentials; set its URL.
5. Disable testMonetization/admobTestAds; enable adsReleaseReady,
   storeCatalogRegistered and productionRelease only after native sandbox checks.
6. Build once when the user requests it. EDM will resolve Google Ads/UMP native
   dependencies. Test purchase/restore/relaunch/duplicate/offline pending paths,
   test-ad close/fail/reward callbacks and app backgrounding on iOS and Android.
7. Complete store privacy/data-safety/age declarations, privacy/support links,
   screenshots and review notes according to the SDKs and app behavior actually shipped.

Current source compiles in the iOS-target Unity Editor. Physical Google test-ad
network delivery, native dependency resolution/build, real Apple/Google store
sandbox, hosted verifier and refund/cross-device wallet infrastructure are NOT RUN.
The previous installed phone build predates this migration and remains unchanged.

Primary setup sources: [Google Unity quick-start](https://developers.google.com/admob/unity/quick-start),
[Google test IDs](https://developers.google.com/admob/unity/test-ads),
[UMP consent](https://developers.google.com/admob/unity/privacy).
