# Release readiness — 8 October 2026

The installed iPhone build is a development-signed preview, not a store candidate. The newest record-arrow inset was validated in Unity, then rebuilt, installed and launched on the paired iPhone; evidence is in `Reports/iOS-2026-10-08-arrows`.

## Blocking work in this project

1. Set the final `com.vickand.cascade` bundle identifier, version/build number, iPhone/iPad support and localized iOS app metadata in Unity source settings. The current iOS export inherits the Unity template identifier and is patched only in generated Xcode output. Make the Android identity/version and API target explicit; verify the final AAB meets the current Play target API requirement.
2. Register the existing store product IDs `cascade.no_automatic_ads`, `cascade.coins_240`, `cascade.coins_480` in App Store Connect and Play Console. Choose real-money prices there. Native purchase, restore, duplicate delivery, offline/pending purchase and refund tests have not been completed. Do not enable live purchase controls until they pass.
3. Create publisher AdMob apps plus rewarded/interstitial units for both platforms. Supply the actual App IDs and unit IDs, configure AdMob privacy messages, test UMP denial/reopen flows and child/age settings, and replace the Google sample IDs. Ads/consent on physical devices remain unverified.
4. Verify the intended Unity Cloud Project ID, configure production Authentication/Cloud Save/Cloud Code and a hosted HTTPS receipt verifier with publisher credentials. Validate account linking, cross-device restore, replay/duplicate prevention, refunds and account deletion. Live rewarded payouts require signed AdMob SSV. Current `CloudPlayerSettings` switches are all off.
5. Only after the above, turn off test monetization and sample ads and enable the release flags. The `MonetizationReleaseGuard` deliberately blocks production iOS/Android builds while these dependencies are missing.
6. Make store-signed release candidates, then run end-to-end device QA: all main screens and EN/RU/UK at small/large text, full onboarding, progression/save migration, music/SFX/haptics, background/resume/offline, performance/crashes, purchase/ads/privacy/account flows. Current checks prove editor visuals and development iPhone launch, not this release matrix. An Android device build has not been validated in the latest pass.

## Store and business setup

7. Prepare final localized listings, screenshots/video if used, icon, support and privacy-policy URLs, age/content ratings, rights to all art/audio, app review notes and test credentials as needed. Answer Apple App Privacy and Google Play Data Safety from the actual shipped SDK/data behavior. Complete export-compliance and account-deletion disclosures where applicable.
8. Complete Apple/Google developer accounts, legal agreements, banking/tax setup, app records, product review and pricing/regions. Plan TestFlight and Play internal/closed tests before production rollout; Google may require a 12-tester/14-day closed test for eligible new personal accounts.

## Inputs needed from the owner

- Decide launch platforms, regions/languages, audience age/child-directed status, supported devices and final displayed name.
- Provide access to the proper Apple Developer/App Store Connect, Google Play, AdMob and Unity Cloud projects via account roles; do not send passwords or private keys in chat.
- Approve real-money prices for the three existing products and the ad placement policy; provide policy/support contact or authorize their creation.
- Confirm ownership or licenses of shipped visual, font and audio assets and recruit store testers where account policy requires them.

Do not silently change existing product IDs, in-game prices, gameplay rules or storage schemas during release preparation. New requirements are recorded here before implementation.
