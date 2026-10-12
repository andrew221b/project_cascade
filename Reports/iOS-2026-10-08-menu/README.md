# CASCADE menu and startup iPhone build — 8 October 2026

The prior device crash report in `Reports/iOS-2026-10-08-startup-crash/last.ips` identifies a main-thread Localization/Addressables wait during `CascadeGame.Awake`. This build disables synchronous locale initialization and keeps captions visible while the translation tables load.

Unity 6000.3.8f1 iOS IL2CPP export succeeded with 0 errors and 346 warnings (`unity-build.txt`); Xcode 26.6 Release build succeeded (`xcode-build.log`). The signed app was installed over `com.vickand.cascade` on the paired iPhone 17 (`install-retry.json`). The first installation attempt lost its CoreDevice connection; closing the old app and retrying succeeded. The post-install command-line launch was denied because the phone was locked (`launch-1.log`). No on-device relaunch, touch, or frame-rate result should be inferred yet.

Native Unity Editor flow assertions and 13 visually reviewed RU/UK screen captures are in `Reports/CatalogFlow-Oct08`. The export keeps the existing app identifier and did not request removal of the old app or its profile.
