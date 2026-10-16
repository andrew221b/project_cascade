# Acoustic long-form score and gameplay HUD — 9 October 2026

Supersedes the music arrangements in AUDIO_BITTERSWEET_OCT09, AUDIO_FORM_AND_CONTROLS and NIGHT_MUSIC. Original compositions; no copied song or generated recording. No gameplay rules, prices, IDs, packages or save schemas changed.

## Sound and form

Ten separate themes now use real soft upright-piano, concert-harp and bowed-cello recordings. No General MIDI synthesizer or woodwind lead remains in the active music references. Japanese colour comes from pentatonic-like melodic contours, spacious plucks and added-sixth/ninth harmony; it is not a claim to use traditional Japanese instrument recordings. Major themes borrow minor IV; minor themes use Dorian/dominant colours. The main line stays in a restrained middle register.

Every theme has 64 bars: intro 4, verse 8, chorus 8, varied verse 8, answering chorus 8, bridge 8, returning chorus 8, resolving chorus 8, turnaround 4. Lengths range from 178.6 to 232.7 seconds. Each location has its own melody, key/tempo and variations. Sparse phrase endings, contrasting accompaniment, small timing/velocity deviations and a diffuse acoustic room replace the old short repeating arpeggio/woodwind texture. Release and room tails wrap across the musical boundary.

The eleven existing score WAV paths (menu original/balanced plus nine worlds) are overwritten with their GUID/import metadata preserved; one-time prior WAV copies are in Reports/AcousticScore-Oct09/previous. The library's legacy fallback now references the same new menu clip, so missing world/menu data cannot revive CloudWorkshopLoop. That old user asset is preserved unused. AtmosphereAssetBuilder maintains this fallback on future configuration.

## Source and license

Samples: **VS Chamber Orchestra 2 Community Edition**, Versilian Studios / Sam Gossner and Ivy Audio / Simon Dalzell; sample cutting by Elan Hickler / Soundemote.

- Official library: https://versilian-studios.com/vsco-community/
- Source: https://github.com/sgossner/VSCO-2-CE
- License: CC0 1.0 Universal, full text in Tools/AudioSamples/VSCO2/LICENSE.
- Exact source URLs and SHA-256 hashes for 28 selected recordings: Tools/AudioSamples/VSCO2/sources.json.

The 70 MB source subset lives outside Assets and is not included in the player. Music remains streaming Vorbis, background-loaded with preload disabled; longer PCM masters do not become full decoded buffers on startup. No new runtime library, network dependency, Unity package or paid subscription is needed. Runway rejected music generation because the connected plan was free; no generation was started.

## Reproduction

```
python3 Tools/compose_acoustic_score.py --install
python3 Tools/check_acoustic_score.py
python3 Tools/check_score_routing.py
```

Offline dependency: numpy/scipy only. The old Swift/General MIDI tools are historical and must not be used for the active score. The legacy finishing script now rejects long scores rather than truncating them to 16/32 bars.

## UI

LayoutGameplayHeader takes its width and horizontal centre from the existing lower Actions band (5% inset inside the safe area), retaining 48-pixel hit regions. The rope's old ceiling anchor span is preserved independently, so this padding change does not alter the board/rope presentation.

## Validation and limits

PASS: ten distinct PCM scores, 64-bar section structure, developing refrains, exact musical durations, headroom, sample-continuous loop seam, all 11 active/fallback references, existing streaming import settings, and sample/license integrity. Actual ToyAudioLibrary.ScoreFor: 330 screen/zone combinations plus fallback cases pass. Reports/AcousticScore-Oct09 contains notation, render/validation reports and a 45-second menu verse-to-chorus preview.

Native Unity state request did not return and was stopped. Editor audio import, live crossfades, speaker/headphone listening and visual HUD verification are **NOT VERIFIED**. Signal checks do not establish subjective musical quality. No whole-project compilation, iPhone build or installation was run.
