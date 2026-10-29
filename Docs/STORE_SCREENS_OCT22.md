# Native store screenshot capture, October 22

The capture build uses a separate application ID `com.vickand.flyingladders.capture`, explicitly scoped `CASCADE_STORE_CAPTURE` code, offline fixture progress and existing authored content. Ordinary/release players contain no capture controller. Before writing its fixture the controller checks its application ID. Cloud, purchases and advertising are disabled only in this build. No changes to production store IDs or the connected iPhone save.

Account UI: existing pale settings row under the sync label; native login actions 552 × 120 reference units, centered icon/caption with a 4 logical pixel gap and increased content padding. EN/RU/UK validation required.

Requested output: six unadorned native screenshots for each device and locale (iPhone medium Dynamic Island, iPad 13-inch, Android phone; EN/RU/UK). Four gameplay scenes plus map and trophies. No upload or review submission.

The final Android capture uses an ordinary optimized player (`BuildOptions.None`) to exclude its Development Build watermark. The iOS export uses Xcode Release and native screenshots; its captured framebuffer has no Development Build label. The builders now default to optimized capture players for future runs. A narrowly scoped Editor guard accepts only the exact `.capture` application ID and rejects production configuration; normal store-release guards are unchanged outside that scope. `CASCADE_STORE_CAPTURE` is provided only through these build calls, not persisted in project defines. Android's standard immersive-mode onboarding is completed on the dedicated emulator before capture.

## Final verification

- 54 native PNGs: 18 iPhone + 18 iPad + 18 separate Android. Six scenes per EN/RU/UK. Four gameplay frames plus trophies and map.
- All images are opaque 24-bit RGB PNGs at native dimensions. Every final RGB byte matches the retained framebuffer PNG; no resizing, overlay or retouch.
- iPhone 1206×2622; iPad 2064×2752; Android 1080×1920. Native source manifests and SHA-256 recorded.
- Account UI PlayMode validation passed for EN/RU/UK at 320×693, 390×844, 768×1024, including caption padding, popup bounds and sync-paper legibility. Test XML retained in Reports/StoreScreensOct22.
- iOS Simulator Unity export and Xcode compilation succeeded; Android optimized native export succeeded, zero errors, not debuggable.
- Capture scenes use C03, C19, C70 Mixed and C145. The internal legacy command `shapes-snow` binds C145; the published journey places it in the Beacon world, so delivered files are correctly named `shapes-beacon`. No game world or content was changed for screenshots.
- Native capture tools install only `com.vickand.flyingladders.capture`. No store upload/review submission, physical iPhone save changes, production auth, purchase or ads configuration changes.
- UI fixes are in project source and capture players; no replacement build was installed over the owner's physical iPhone in this screenshot task.

Delivery: /Volumes/MY DISK/FlyingLadders/StoreScreenshots/2026-10-22
