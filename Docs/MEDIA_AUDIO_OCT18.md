# Media audio and silent mode — 18 October 2026

The user reported that iPhone music and effects stayed silent while the Ring/Silent switch was on, even after increasing volume. This is consistent with the default iOS Ambient/SoloAmbient category. Inspection found no playback-category configuration in the project; muteOtherAudioSources remains 0, no background-audio mode is enabled, and the existing sound/music preferences and foreground recovery remain independent. The old physical player's active native category was not instrumented, so the diagnosis is strongly supported rather than a direct readout of its AVAudioSession.

Added a small iOS-only Objective-C++ bridge selecting AVAudioSessionCategoryPlayback, default mode and MixWithOthers. This category ignores the Ring/Silent switch while retaining system media volume. MixWithOthers preserves the project's existing policy of not interrupting another app's audio. The bridge changes category only when needed; it does not activate/deactivate sessions, force speaker/Bluetooth routes, enable a microphone or background mode, change volume or override user music/effects preferences.

ToyFeedbackPool requests media configuration once when audio is allowed, then reapplies after focus/foreground return, ad suspension, audio configuration change or re-enabling sound/music. It defers configuration while the game is disabled/suspended or both audio groups are off. Calls are event-driven, not native polling each frame. The pause menu can retain enabled music. Failed configuration logs once, allowing another event-driven attempt.

On Android, the Unity activity's volume keys are explicitly associated with AudioManager.STREAM_MUSIC on the UI thread. The activity is resolved inside the callback, with wrappers disposed after use; no stale/disposed Java reference is captured. No ringer/DND setting or system volume is altered. This is consistent with Android's media volume guidance; it does not assert an Android bug was reproduced.

Validation:
- Native Unity 6000.3.8f1 PlayMode: 5/5 existing checks pass, covering independent sound/music switches, feedback disable/re-enable and loop positions, foreground/output-reset recovery, and menu click lifecycle. Temporary test profiles only; no user saves changed.
- Actual Apple iOS ARM64 bridge compiled and linked to AVFoundation/Foundation against iOS 27 SDK with iOS 15 minimum, warnings-as-errors, exported C entry point verified with nm.
- Actual AVAudioSession probe in the already running iPhone 17 Pro / iOS 26.5 simulator: Ambient replaced by Playback, MixWithOthers/default mode verified, 20 repeated calls succeed, outputVolume unchanged, and Playback restored after a simulated category reset to SoloAmbient. The probe does not activate audio or alter another app's session. This checks the native category contract, not physical audibility.
- Both managed mobile conditional branches compile against the installed Unity Core/AndroidJNI assemblies with warnings-as-errors. iOS plugin importer enables only iPhone and declares AVFoundation, following the existing native haptics importer convention.
- Focused source diff inspected; no package/settings/scene/save schema changes. No new permanent test fixture, sound assets or external package.

Not performed: full IL2CPP player export/link, physical iPhone silent-switch/speaker/headphone listening, actual Android device routing, real phone-call/ad SDK interruption, or physical volume-zero roundtrip. The phone remains on build 202610182; the fix is in source and needs a new installed player before physical behaviour can change. Source hashes identify validated inputs.

Primary references:
- Apple Playback category: https://developer.apple.com/documentation/avfaudio/avaudiosession/category-swift.struct/playback
- Apple configuring an audio session and MixWithOthers: https://developer.apple.com/library/archive/documentation/Audio/Conceptual/AudioSessionProgrammingGuide/AudioSessionBasics/AudioSessionBasics.html
- Android media volume controls: https://developer.android.com/media/platform/output


Deployment follow-up — 18 October 2026: validated player sources included in build 202610183, installed on the paired physical iPhone 17 at the user's request. Current profile and recovery file verified byte-identical after updating. Installed; foreground launch was not confirmed. See launch-state.json for the device response. See Reports/iPhoneInstall-202610183/README.md.


Deployment follow-up — 18 October 2026: validated player sources included in build 202610184, installed on the paired physical iPhone 17 at the user's request. Current profile and recovery file verified byte-identical after updating. Installed; foreground launch was not confirmed. See launch-state.json for the device response. See Reports/iPhoneInstall-202610184/README.md.
