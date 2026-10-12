# Original workshop audio

The 17 shipped WAV files in `CascadeArt/Audio` were authored for CASCADE by
`ToyMotionAssetBuilder`. They contain generated sine partials, deterministic
filtered noise and amplitude envelopes. No ShapeMania or third-party recording,
sample library, generated-service output or licensed melody was imported.

Twelve cues cover selection, swap, landing, fragile rail, unlock, repair, blast,
defuse, bridge, stamp, success and rejection. Selection, swap, landing and fragile
rail each have a second variant. The 24-second background phrase is an original
sequence of soft mallet-like notes. It uses short fades at its file boundaries.

Sources are mono PCM, 22,050 Hz, 16 bit. Unity imports short cues as PCM with
decompressed preloading and the music as streaming Vorbis (quality 0.65).
Runtime playback uses eight reusable voices and one music source; foreground
effects briefly lower the music volume. Suspension/pause mutes music and stops
voices. Music, sound and haptics have separate presentation switches.

`Reports/P13/audio-audit.json` records source durations, sample peaks and first/
last samples. All source endpoints are zero; no source clips. This is a signal
and import check, not an auditory mix review. Physical speakers/headphones and
mobile vibration remain untested. Settings UI is P14; persistence is P16.
