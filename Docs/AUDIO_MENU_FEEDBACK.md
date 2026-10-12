# Audio and menu feedback — 7 October 2026

## Confirmed defects

Menu pages pause puzzle commands. The feedback pool previously treated that as
an audio pause and stopped every one-shot voice each frame. Screen changes also
cleared those voices immediately. These two paths clipped menu clicks and
action tails even with Sound enabled.

The pool now stops voices only when sound is disabled, the owner is disabled,
or the application is suspended. Screen/VFX cleanup preserves audio tails.
Buttons give one short selection sound even where older wrappers request it
twice; requests within 75 ms are coalesced. Music and effects remain separate
preferences. Effect quality controls visuals only.

## Loop recovery and signal evidence

Audio configuration changes mark owned loops for recovery. A paused source is
unpaused; a stopped source reloads unloaded clip data and restarts near its
cached sample cursor. The focused test exposed this unloaded streaming-clip case;
waiting for a load state alone would never initiate the reload. Gains
still rise smoothly and themes retain the existing 1.6-second constant-power
blend. Missing/not-loaded clips are retried without touching gameplay.
The output callback only sets a flag; source operations run on the main thread.
See [Unity audio configuration changes](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioSettings.OnAudioConfigurationChanged.html).
Unloaded data is requested through [AudioClip.LoadAudioData](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AudioClip.LoadAudioData.html).

The initial native probe found the isolated game suspended while Unity lacked
focus, with both preferences enabled. After simulating the normal foreground
callback, the menu source played at gain 0.19 and the listener contained nonzero
PCM (peak 0.07477, RMS 0.01317). Menu clicks survived two frames and navigation.
This is evidence of engine output after the fixes, not a recording of the
user's original silent session or proof of physical speaker audibility.
`Reports/AudioNavigation/initial-focus-probe.txt` records the actual values.

All 44 WAV files have valid signal and assigned references; see
`AUDIO_ASSET_AUDIT.md`. The existing score and cues were retained. No new
music generation or loudness increase was justified by the silence defect.

## Response and navigation

Short native haptics accompany UI taps, exchanges and landings. A 120 ms
cooldown prevents overlapping events from buzzing continuously; the preference
and application suspension gate requests. See `HAPTIC_FEEDBACK.md` for platform
behavior and untested device limits.

Back restores the actual entry page and shop selection, including visits from
the map's bag and Trophies; collection actions remain in the collection.
`MENU_RETURN_PATHS.md` describes these routes. Fifteen messages were shortened
in all three languages without dropping tool limits or changing translation
keys. `UI_COPY_TRIM.md` records that scope. The concise-copy rule is in AGENTS.md.

## Validation

Portable compilation: current Presentation (82 files) and PlayMode tests
(7 files) passed with zero errors. Seven pre-existing Presentation warnings
remain (one member hiding warning and six deprecated AdMob configuration
warnings).

Native focused pass: nine of ten coroutine fixtures passed initially; recovery
failed on unloaded streaming data. The diagnostic rerun recorded that state;
after adding the reload, the one affected fixture passed. All ten targeted
cases therefore passed across the scoped runs, not in a fresh full-suite run.
`Reports/AudioNavigation/focused-runtime.txt`, `recovery-rerun.txt` and
`recovery-final.txt` preserve this sequence honestly.

The native listener also produced nonzero signal for menu music alone, world
music alone and effects with music disabled. Four final screenshots (RU,
390×844 and 320×693 with Large Text/simulated safe insets) had no TMP overflow
and were inspected by the implementer and an independent copy reviewer. No
new copy/layout regression was found. Localization values were imported into
the existing EN/RU/UK assets. The temporary runner was removed afterward.

No phone build/install, Android player build, physical haptic feel, subjective
music/speaker listening, full test suite or performance benchmark is implied.
