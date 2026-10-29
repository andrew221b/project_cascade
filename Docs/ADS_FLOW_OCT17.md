# Advertising flow — 17 October 2026

This user-approved rule supersedes the older cadence/base-parcel text in C16, P19 and earlier parcel notes.

- Interstitial: after every second newly completed whole level, after the saved result, including world-ending levels. Stages, failures and replays do not count. One completion check; readiness/privacy/no-ads still apply. No retry ads.
- Parcel: the earned closed parcel remains pending until the player chooses Open with video and a rewarded callback is verified. Cancelling or failed/unavailable video keeps it closed, with Later available. After the base parcel, More with video is a separate rewarded offer.
- Removing ads suppresses interstitials only; both parcel rewarded offers remain available. Video offers are always entered through a clearly labelled player action.
- Add a zero-value parcel_open grant receipt scoped to the existing parcel subject. It unlocks presentation only, awards no coins/hints and survives reload in the real ledger. Base/bonus inventory claims keep their existing atomic transactions. No save shape, prices, SKU or parcel contents change; old claimed parcels remain claimed.
- Current testMonetization mode defaults to an explicitly labelled local ad screen on device and in Editor. The interstitial lasts two seconds; the rewarded simulation requires five foreground seconds and explicit confirmation. Closing does not grant a reward. Preview receipts remain session-only, following the existing sandbox policy. Production/native AdMob consent and reward callbacks are preserved when local preview is off.

SDK reference: https://developers.google.com/admob/unity/rewarded . Native AdMob callback verification remains separate from the local scenario test; https://developers.google.com/admob/unity/test-ads .
