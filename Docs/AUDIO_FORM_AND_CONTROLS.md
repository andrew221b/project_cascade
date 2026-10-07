# Music form and independent controls — 7 October 2026

## Evidence and bounded change

All nine authored scores were inspected in the existing Swift composer and measured from their shipped stereo 16-bit WAVs. They already have 32 bars in 6/8, a 16-bar A and 16-bar B, distinct melodic phrases, four-bar reply gaps, a restrained B countervoice, panned acoustic instruments and room reverb. The eight world scores have B/A RMS differences of 1.10–2.81 dB and bar-level ranges of 4.34–8.55 dB. There was no evidence for replacing their themes or regenerating them.

The menu was the exception: its lead changes from decaying piano to sustained flute at bar 16, while both samplers use the same −2 dB gain. Measured B/A RMS increased 9.79 dB; the two seconds immediately after the change were 11.34 dB louder than the preceding two seconds. This justified a menu-only instrument balance correction.

`Menu_HomeAmongCloudsBalanced.wav` is a new sibling. The original `Menu_HomeAmongClouds.wav` and its `.meta` are unchanged. The sibling preserves pitches, tempo, 32-bar duration, harmony, A/B melodies, phrase replies, stereo placement and instruments. Only its flute sampler receives −8.5 dB instead of −2 dB; the first two B bars use MIDI velocity offsets −12 and −6 before the existing phrase dynamics resume. The whole score retains the original .12 RMS target and .72 peak ceiling. Its acoustic release tail wraps into the opening through the existing finisher.

| Source metric | Original menu | Balanced sibling |
| --- | ---: | ---: |
| Duration | 68.571 s | 68.571 s |
| B/A RMS contrast | +9.79 dB | +4.82 dB |
| Adjacent two-second A→B boundary | +11.34 dB | +4.82 dB |
| RMS | .119986 | .119985 |
| Peak absolute sample | .659821 | .705078 |
| First/last sample discontinuity | .001373 | .001862 |
| Clipping samples | 0 | 0 |

These are source-file measurements, not loudness-unit or perceptual listening results. The remaining B contrast is intentional musical movement; no compressor or runtime phrase gain flattens the arrangement. Exact measurements and preserved original-file hashes are in [Reports/AudioRefinement/audio-metrics.json](../Reports/AudioRefinement/audio-metrics.json).

## Offline provenance

The existing local original-composition path was used; no paid generation, third-party recording or new music service was involved. macOS supplies its existing instruments, without copying the bank. The first sandboxed render failed before audio production because CoreAudio could not instantiate its sampler (`comp != nullptr`); the same offline render succeeded with sandbox escalation. The existing `masterGain` API emits a macOS deprecation warning.

Reproduce the sibling in a temporary folder from the project root:

```sh
swift -module-cache-path /private/tmp/cascade-score-review-cache Tools/compose_cascade_score.swift /private/tmp/cascade-menu-balanced --theme Menu_HomeAmongClouds --menu-balanced --scores-only
python3 Tools/finish_cascade_audio.py --music-only --folder /private/tmp/cascade-menu-balanced --only-score Menu_HomeAmongCloudsBalanced --report /private/tmp/cascade-menu-balanced/metrics.json
```

The composer’s default path still writes the incumbent arrangements; balancing is opt-in and scoped to the menu. The finisher’s new folder/score/report arguments make the isolated run possible without touching other shipped audio. Run finishing on a fresh raw render. The checked sibling was then copied into `CascadeArt/Audio/Atmosphere` with its own `.meta`, using the original menu importer settings: Streaming, stereo, background load, preload disabled. `ToyAudio.asset` changes only `menuMusic`. `AtmosphereAssetBuilder.Configure` selects the sibling and continues recognizing the preserved original as a Streaming score.

## Controls and owned source lifecycle

Settings already owns separate persisted `Music` and `Sound` preferences. `CascadeGame.ApplyPreferences` applies these to `MusicEnabled` and `SoundEnabled` independently. Music controls the two score sources; Sound effects controls pooled feedback and wind/rain. This change adds no preference field, serialized schema, mixer, package or gameplay rule.

The existing reversible 1.6-second score crossfade and live feedback ducking remain. Muting fades owned score/ambience gains to zero before sleeping and retains playback positions. Actual shared `WindAt` pressure still drives the nonlinear wind envelope; no second audio gust clock or duplicate gust sound was added.

Previously, disabling `ToyFeedbackPool` paused its wind alone while its music/rain sources could remain audible. `OnDisable` now zeros and pauses both score sources and both ambience sources, stops transient feedback tails and clears their ducking. It retains the loops’ clips, positions and current crossfade, resets the wind envelope, and marks the existing music/ambience sleeping flags. Re-enabling resumes through the same normal tick and gain ramps, respecting the independent Music/Sound preferences.

`WindAudioLifecycleTests.IndependentMusicAndSoundControlsAndOwnerDisablePreserveLoopPositions` covers music on with sound off, music off with sound on, balanced menu selection, zero output from all four loops and stopped cue tails while the owner is disabled, retained loop positions/clips/crossfade, and audible gain/playback resuming. The earlier wind-only lifecycle and music transition/mute checks remain applicable.

## Validation limits

Completed here: source/form review, offline generation, finishing, comparison of all nine original score measurements, source clipping/seam checks, original WAV/meta hash preservation and static diff review. No other theme or original audio asset was regenerated.

Unity import/compilation, the new PlayMode regression and final native playback validation are delegated to the parent task and were **NOT RUN by this audio role**. Physical-device/headphone listening, subjective musical quality, performance and human enjoyment were **NOT RUN**. Metrics alone cannot establish perceived smoothness or musical preference.
