# CASCADE iPhone device build — 6 October 2026

The requested device build uses Unity 6000.3.8f1 iOS IL2CPP and the single
CascadePlayable scene. Export to `CascadeUnity/Builds/CASCADE-iOS`; preserve
existing Mac apps and the project’s current content, packages and save data.

The Unity project still contains its inherited template iPhone identifier.
For this local device installation, sign the generated Xcode target as
`com.vickand.cascade` with automatic development provisioning and display name
CASCADE. The Apple Developer Team `F8297DBPDU` is selected in every app target
configuration. This is a device build identifier, separate from ShapeMania’s installed
app. No game IDs, prices, rules or profile schema change in the Unity source.

Record the Unity export and Xcode compilation results separately. Installation
and launch must be confirmed by the paired iPhone, not inferred from a built
Xcode project. Physical-device performance and human play quality require their
own observation.

## Current export — 7 October 2026

Build the current source (8adca9c) once into
`CascadeUnity/Builds/CASCADE-iOS-2026-10-07`; keep the previous export
`CascadeUnity/Builds/CASCADE-iOS` and its DerivedData. Use the generated
`Unity-iPhone.xcworkspace` so the LevelPlay CocoaPods dependencies are included.
The same app identifier and team above apply. Xcode generated a specific
CASCADE development profile including In-App Purchase and the paired iPhone;
no capability, package or source setting was disabled to bypass signing.
`testMonetization=1`, the real store catalog is unregistered, and native ad
keys are empty. Deployment does not validate production payments or ads.

## Current export — 8 October 2026

The working tree after the route-record layout revision was exported to
`CascadeUnity/Builds/CASCADE-iOS-2026-10-08` and built from its
`Unity-iPhone.xcworkspace` with Xcode 26.6. The signed app retained
`com.vickand.cascade`, was installed and launched on the paired iPhone 17.
The previous exports and the app's existing device data were preserved.
See `Reports/iOS-2026-10-08/README.md` for the exact build and device evidence.

## Current installation — 16 October 2026

At the user's explicit request, built and installed the current source as
0.1.0 (202610161), com.vickand.cascade, on the same paired physical iPhone 17.
Unity iOS export and Xcode Release signing succeeded; strict deep signature
verification passed. CoreDevice confirmed the installed version and process
launch/liveness. Current music, island/tutorial screens and frame repairs are
included. Export and signed app are retained under
`CascadeUnity/Builds/CASCADE-iOS-202610161` and its `-DerivedData` sibling.
No uninstall/reset/restore; no existing CASCADE app was found before this
installation. Main unsaved scene and protected backups preserved.
Evidence and physical visual/audio/performance limits:
`Reports/iPhoneOct16/README.md`. Any later deployment requires a new user request.


## Current installation — 16 October 2026, build 202610162

The user explicitly requested installation after the settings-alignment correction. Built the latest authoritative source in an isolated copy, signed and installed CASCADE 0.1.0 (202610162), com.vickand.cascade, over Wi-Fi on the paired iPhone 17. Unity export: succeeded, 0 errors/360 warnings; Xcode Release: succeeded; strict signature verification: passed. Exact installed version, launch and subsequent process liveness confirmed by CoreDevice. All 2,568 snapshotted inputs remained unchanged during export.

Includes current story/descent changes, actual glyph/board centring, portraits attached to their nameplates and restored settings/navigation alignment. Export retained under `CascadeUnity/Builds/CASCADE-iOS-202610162`; signed app under `CascadeUnity/Builds/CASCADE-iOS-202610162-Device/CASCADE.app`. Earlier builds, main Editor and device data preserved; no uninstall/reset/restore. Evidence and physical validation limits: `Reports/iPhoneInstall-202610162/README.md`. Further builds/installations require a new user request.


## Clean installation — 17 October 2026, build 202610171

At the user's explicit request, built, signed and installed CASCADE 0.1.0 (202610171), com.vickand.cascade, on the paired physical iPhone 17 without restoring saves. No previous app was listed; CoreDevice confirmed installation, exact version and a new container without any saved profile/backup before first launch. Unity iOS IL2CPP: succeeded, 0 errors/355 warnings; Xcode Release: succeeded; strict deep signature: passed. All 2,583 authoritative inputs stayed unchanged. Includes generated whole-board damage states and the lower-edge durability plaque, plus the latest source changes. Export and signed app retained beside previous builds.

Foreground launch is currently blocked by iOS because the device is locked; the user was asked to unlock it. Device visual/audio/FPS review is not claimed. Evidence: Reports/iPhoneInstall-202610171/ and Reports/PlatformStatesOct17/. Further installations require a new explicit request.


## Current installation — 17 October 2026, build 202610172

At the user's explicit request, built/signed/installed and launched CASCADE 0.1.0 (202610172), com.vickand.cascade, on the paired iPhone 17. Unity export succeeded (0 errors/355 warnings); Xcode Release succeeded; strict signatures, exact installed version, launch and subsequent process liveness confirmed. All 2,601 snapshotted inputs remained unchanged. Includes the current music, HUD/tutorial/receiver changes, compact checkpoint buttons, rooted tool flowers, shop/pause and victory/comic follow-ups. Main Editor and prior artifacts preserved.

Save preservation failed: the prior 15,382-byte profile became inaccessible before installation; no uninstall/reset/restore was executed, but the newly installed container lacked the previous profile and launch created a new 2,461-byte profile. No compatible local backup was found; signing/storage entitlements are unchanged. Cause undetermined. This is not a claim that user progress survived. Evidence and limitations: Reports/iPhoneInstall-202610172/. Export and signed app retained beside prior builds. Further installations require a new explicit request.


## Current installation — 18 October 2026, build 202610181

At the user's explicit request, installed CASCADE 0.1.0 (202610181), com.vickand.cascade, on the paired physical iPhone 17. Exact version confirmed. Launch and process liveness after four seconds confirmed by CoreDevice. Includes all 19 new prepared music loops and current source. Unity iOS IL2CPP export succeeded (0 errors/360 warnings), Xcode Release succeeded, original and retained strict signatures passed. All 2613 snapshotted authoritative inputs stayed unchanged.

Current profile and recovery file backed up privately, then verified byte-identical after updating before first launch. No uninstall/reset/restore; previous exports and main Editor retained. Export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610181 and -Device. Evidence and physical validation limits: Reports/iPhoneInstall-202610181/README.md. Further installation requires a new user request.


## Current installation — 18 October 2026, build 202610182

At the user's explicit request, installed CASCADE 0.1.0 (202610182), com.vickand.cascade, on the paired physical iPhone 17. Exact version confirmed. Launch and process liveness after four seconds confirmed by CoreDevice.

Includes the updated Mix journey: three-symbol levels alternate shapes/words, and four-or-more-symbol rows combine words and shapes from the first applicable level (first three campaign levels: shapes, words, mixed). Also includes the clear, original-colour active island with external green pulsing contour and corrected Mix selector symbols. included-features.json matches the exact validated sources against the build snapshot. All 19 prepared world-score music loops and their unchanged importer metadata are verified in included-score.json.

Unity iOS IL2CPP export succeeded (0 errors/360 warnings), Xcode Release succeeded, original and retained strict signatures passed. All 2619 snapshotted authoritative inputs stayed unchanged. Current profile and recovery file privately backed up twice, then byte-identical after updating before first launch. No uninstall/reset/restore; previous exports and main Editor retained. Export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610182 and -Device. Evidence and physical validation limits: Reports/iPhoneInstall-202610182/README.md. Further installation requires a new user request.


## Current installation — 18 October 2026, build 202610183

At the user's explicit request, installed CASCADE 0.1.0 (202610183), com.vickand.cascade, on the paired physical iPhone 17. Exact version confirmed. Installed; foreground launch was not confirmed. See launch-state.json for the device response.

Includes the latest raised/inset Home shortcuts, iOS media-playback audio session and Android media volume routing, natural bird/insect paths with measured bird foot contacts, coupled token/platform mass response, and physical opaque falling platform halves. Also retains the validated Mix journey, clear active-island colours with external green pulsing contour, corrected Mix selector symbols, and all 19 prepared world-score loops with unchanged importer metadata. included-features.json verifies all five source groups against the exact build snapshot; included-score.json verifies the 19 music masters.

Unity iOS IL2CPP export succeeded (0 errors/360 warnings), Xcode Release succeeded, original and retained strict signatures passed. All 2627 snapshotted authoritative inputs stayed unchanged. Current profile and recovery file privately backed up twice, then byte-identical after updating before first launch. No uninstall/reset/restore; previous exports and main Editor retained. Export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610183 and -Device. Evidence and physical validation limits: Reports/iPhoneInstall-202610183/README.md. Further installation requires a new user request.


## Current installation — 18 October 2026, build 202610184

At the user's explicit request, installed CASCADE 0.1.0 (202610184), com.vickand.cascade, on the paired physical iPhone 17. Exact version confirmed. Installed; foreground launch was not confirmed. See launch-state.json for the device response.

Includes the latest raised/inset Home shortcuts, iOS media-playback audio session and Android media volume routing, natural bird/insect paths with measured bird foot contacts, coupled token/platform mass response, and 16 closed textured timber splinters with physical lower-deck rebounds and forward slip off the decks. Also retains the validated Mix journey, clear active-island colours with external green pulsing contour, corrected Mix selector symbols, and all 19 prepared world-score loops with unchanged importer metadata. included-features.json verifies all six source groups (the four new splinter-source hashes supersede their earlier LivingPhysics versions) against the exact build snapshot; included-score.json verifies the 19 music masters.

Unity iOS IL2CPP export succeeded (0 errors/355 warnings), Xcode Release succeeded, original and retained strict signatures passed. All 2627 snapshotted authoritative inputs stayed unchanged. Current profile and recovery file privately backed up twice, then byte-identical after updating before first launch. No uninstall/reset/restore; previous exports and main Editor retained. Export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610184 and -Device. Evidence and physical validation limits: Reports/iPhoneInstall-202610184/README.md. Further installation requires a new user request.
