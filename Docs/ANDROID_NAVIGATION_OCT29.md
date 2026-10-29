# Android navigation and pause safety — October29

User authorized black text on the yellow age button and Android Back/ANR remediation. This work does not create or upload a player build. Existing artwork, centring,4px icon gaps, IDs, receipt validation and account/economy authority remain intact.

## Runtime changes

- Yellow age choice label is black; green label remains white. EN/RU/UK rendered at390×844 and visually reviewed. Existing layout/insets preserved.
- Back closes the top legal/account/tool/help/preview overlay, cancels a lesson without spending its tool, then uses the screen's authored Back action. Settings subpages, map/island browsing and collection return to their parent. Playing pauses; Pause resumes. Root Home/account/age/save recovery backgrounds the task without selecting an answer, account, destructive action or purchase. World completion uses its existing Home action to avoid reopening itself.
- Active native SDK windows retain Back ownership. Busy purchases are not cancelled; Deferred parental approval does not trap navigation or grant anything. No fake reward or client-side confirmation was introduced.
- API33+ committed Back callback only sets an atomic bit, consumed by Unity Update. Cancelled gestures produce no action. Legacy Android follows Input System Escape. UI dispatch is asynchronous; no wait or Unity callback runs from Android's UI thread. Generation tokens prevent stale registration/unregistration after Activity recreation; weak Activity references avoid retention. JNI fallback resolves the current Activity after dispatch, without capturing a disposed wrapper.

## ANR remediation

Unity6000.3.8f1 bundled GameActivity3.0.5 is affected by the documented onPause UI/render-thread deadlock. Source settings now select Activity, Unity's supported workaround. The Editor version/library packages were not manually replaced. A build preflight rejects GameActivity, generated manifests opt into Back without changing the AdActivity override, and additive/idempotent consumer ProGuard rules preserve JNI class names if minification is enabled later.

Android OnApplicationPause/OnApplicationFocus(false)/OnDisable snapshots now use existing QueueSave rather than waiting for the disk lock/fsync. Background callbacks avoid rebuilding hidden UI. Critical economy/cloud commits retain their durable Save contract. Solver/planner Result reads were reviewed: they follow IsCompleted checks; presentation has no synchronous network waits. The notification SDK already queues actual Android scheduling/storage on its background worker.

Screen.safeArea remains the source of window insets. Tests exercise bottom navigation bars and asymmetric side cutouts; this is Editor geometry evidence, not an exhaustive device matrix.

## Validation

-28 unique Android and27 unique iOS Editor PlayMode checks: navigation15/14 plus existing first-use/help13 per target. All passed.
-4 EditMode build-policy checks passed: Activity requirement, manifest/SDK preservation, GameActivity rejection, additive JNI keep rules.
-16 fake Android-dispatcher assertions passed using the actual Java helper: committed/exactly-once Back, old API guard, registration/recreation, stale queued install/uninstall and nonblocking background dispatch.
- Java helper compiled against Android API36; actual Android-only C# JNI branch compiled against Unity runtime modules without UNITY_EDITOR.
- Android regression holds the profile disk lock while pause/focus/disable callbacks run, asserts they return without waiting for it, then flushes and reloads a valid profile. Purchases, pending approval, tool ownership, unavailable-video hint fallback and tutorial behavior were also covered.

Evidence: Reports/AndroidNavigationOct29/verification.json and NUnit XML. Native harness is reproducible with python3 Reports/AndroidNavigationOct29/native-back-harness.py --android-jar <API36/android.jar>.

## Honest limits

No new APK/AAB/IPA was built/uploaded; installed/store build9 retains its prior code and GameActivity. A separately authorized Android build is needed for real gesture/3-button/IME, SDK-window return, Activity recreation, process death and weak-device cold-load/background stress. Critical durable economic commits and startup profile reads still perform synchronous local I/O; pathological storage stalls, third-party SDK bugs and GPU/OS failures cannot be ruled out by Editor tests. Zero ANRs is not claimed.

References: [Unity workaround](https://discussions.unity.com/t/android-anrs-in-onpause-with-gameactivity-cause-fix-and-workarounds/1740442), [Android Back guidance](https://developer.android.com/guide/navigation/custom-back/predictive-back-gesture), [committed callback](https://developer.android.com/reference/android/window/OnBackInvokedCallback).
