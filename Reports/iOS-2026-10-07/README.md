# Current iPhone build — 7 October 2026

Source: 8adca9c. Unity 6000.3.8f1, iOS IL2CPP, version 0.1.0,
CascadePlayable only. One Unity export: Succeeded in 98.514229 seconds,
0 errors and 346 warnings (Localization iOS App Info metadata and Sentis
Metal variants among them). No extra player export or test suite was run.

Current Unity export: `CascadeUnity/Builds/CASCADE-iOS-2026-10-07`.
Previous Unity export preserved: `CascadeUnity/Builds/CASCADE-iOS`.
Earlier individual iOS exports are not separately preserved. Other
CASCADE*.app folders are Mac applications, not iPhone builds.

The initial signed Xcode project attempt failed before compilation with
No Accounts / cached wildcard profile missing In-App Purchase capability.
The logged-in account was confirmed in Xcode; its Signing & Capabilities
view generated a CASCADE-specific managed profile with In-App Purchase and
the paired iPhone. No SDK/capability was removed to bypass signing.

Xcode 26.6 Release workspace build (including CocoaPods): BUILD SUCCEEDED,
exit 0. App bundle `com.vickand.cascade`, team `F8297DBPDU`, display name
CASCADE, minimum iOS 15.0. Source Unity identifiers were not changed.
Signed app: `CascadeUnity/Builds/CASCADE-iOS-2026-10-07-DerivedData/Build/Products/Release-iphoneos/CascadeUnity.app`.

CoreDevice confirmed installation on the paired iPhone 17, then successful
launch. A separate process query found the same app still running (PID 51097).
No uninstall/data reset was performed. Deployment and process start are
verified; physical screen quality, gameplay, haptics, performance, production
ads and payments were not tested. Monetization remains the configured test
preview, with no registered production catalog or ad keys.

Unrelated export churn in mobile URP/global settings, font fallback and P15
report was restored. Updated iOS Addressables content state is retained.
The pre-existing user Cloud settings edit is preserved and excluded from
the build handoff commit. Generated exports/DerivedData remain ignored.
