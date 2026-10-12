# Lower, bittersweet score and level arrival — 9 October 2026

> Historical music revision. Active score now uses [the long-form acoustic arrangements](ACOUSTIC_SCORE_OCT09.md).

User-authorized scope: revise every active music theme and make starting a level change the music. No puzzle rules, durability values, prices, IDs, packages, saved preferences or phone builds change.

## Repair and actual tension

Repair adds durability before the exchange's wear resolves. On ordinary wood, two additional durability buy two more exchanges; cracked links, blasts and gust damage alter that conversion. The current triad generator budgets these actual costs using the production resolver. Its existing necessity gate rejects a candidate if removing any repair, key, ribbon or freeze leaves a solution. The previous [effect-budget audit](EFFECT_BUDGET_OCT09.md) remains applicable: this pass changes no generator/Domain code. The repair tutorial already says it restores durability **for more exchanges**, and the live reaction displays the actual amount.

Winning narrowly should follow from the player's timely contact and chosen route. No hidden extra moves, scripted near-losses or outcome manipulation were added. Human enjoyment and the frequency of clever alternative wins have not been established by solver proofs.

## Music arrangement

The active balanced menu and all nine world themes are re-rendered from their original local composer. Each retains its tempo, duration, melodic contour, A/B form and location instruments. The solo is one octave lower (occasional minor-scale colour changes add one semitone of difference). Lower clarinet replaces the high flute; piano, harp, koto, nylon guitar, strings, cello and restrained vibraphone retain their roles. New quiet seventh/ninth chords, close string voice leading, descending replies, phrase dynamics, a borrowed minor IV in major themes and a hopeful Dorian/dominant colour in minor themes supply the requested bittersweet direction. The observatory retains its shorter, slower pentatonic lullaby.

`--bittersweet` is opt-in. Without that flag the composer can still reproduce the original arrangements. Ten active WAVs are replaced at their existing paths; their `.meta` hashes/GUIDs and `ToyAudio.asset` references are unchanged. The inactive original menu and legacy fallback loop, every ambience recording and all sound effects are untouched. Pre-change active WAVs are also backed up at `/private/tmp/cascade-oct09-audio-before`.

Release tails wrap into the next cycle. The new opt-in finisher `--smooth-score-seams` removes a residual boundary step over five milliseconds, without changing tempo, bar length or adding a silence. No runtime pitch/half-speed trick or new runtime DSP is used.

Reproduce in a fresh temporary directory:

```sh
swift -module-cache-path /private/tmp/cascade-score-cache Tools/compose_cascade_score.swift /private/tmp/cascade-warm --menu-balanced --bittersweet --write-score --scores-only
python3 Tools/finish_cascade_audio.py --music-only --smooth-score-seams --folder /private/tmp/cascade-warm --report /private/tmp/cascade-warm/metrics.json
```

The system CoreAudio sampler requires execution outside the sandbox; the authorized offline render writes only temporary scores/WAVs. Its instrument bank is not copied. No paid generation or third-party recording is used.

## Routing and loading

Evidence: `ToyAudioLibrary.ScoreFor` previously selected the **same world clip** for the map/dialogue and Playing. Consequently map → level had no musical arrival even with correct region routing. Home, map, story and browsing now select the menu theme. Playing, Pause, Win, Fail and Checkpoint select the current world's theme. Existing 1.6-second reversible crossfades, independent preferences and music-position preservation remain.

The audio owner requests the wanted clip's background load and waits for `Loaded` before replacing an audible source. The old theme continues while loading. It also resumes an initially delayed or independently stopped incoming source, and does not advance a fade while that source cannot play. This follows Unity's [AudioClip loading contract](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioClip.LoadAudioData.html). It adds no per-frame allocations or additional sources.

## Evidence and limits

- All ten revised stereo 44.1 kHz/16-bit loops: exact original tempo/duration; every first-beat solo note lowered 12–13 semitones; zero clipping; boundary step no greater than one PCM sample; distinct hashes. Source spectral centroids are lower in all ten themes. A/B contrast is 0.74–3.37 dB and adjacent two-second section contrast is 2.21–5.32 dB. These are signal/notation checks, not perceptual loudness or listening claims. See `Reports/Audio-Oct09/score-review.json`.
- Executed the actual `ToyAudioLibrary.ScoreFor` method against every actual screen enum × valid/invalid region and fallback cases using a minimal engine-type shim. This tests selection, **not AudioSource playback**. See `score-routing.txt`.
- Added `AudioFeedbackTests.MapEntryChangesFromMenuToTheAudibleWorldScoreAndReturns`: actual map action → level, audible world source, nine worlds, stopped-source recovery and map return. Updated the earlier crossfade test to respect async loading and the map/menu decision.
- Native import/readback, the PlayMode regression, speaker/headphone listening and physical-device checks are **NOT RUN** this pass. The live connector targets `shape_sorting-final`; mutations there were avoided. CUA can read the correct CascadeUnity window but input fails with `noWindowsAvailable`. A user request to unlock/open the intended editor is pending. One short menu A→B WAV preview was emitted, which does not establish a listening verdict.
- No iPhone export, Xcode build or installation.
