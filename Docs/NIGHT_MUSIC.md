# Star Observatory music

> Historical music revision. Active score now uses [the long-form acoustic arrangements](ACOUSTIC_SCORE_OCT09.md).

7 October 2026: `StarObservatory_ConstellationLullaby.wav` is one original
48-second stereo score for world index 8. It was composed in the local
`observatory()` function in `Tools/compose_cascade_score.swift`, using D-major
pentatonic pitches, 16 bars of 6/8 at 60 BPM, an eight-bar A phrase and a gentle
eight-bar B response. Its four instruments are harp, soft flute, warm strings
and cello; phrase gaps and a tonic return leave space for the puzzle sounds.

The existing macOS General MIDI instrument playback renders the authored
notes through the project's offline AVAudioEngine path, with medium-hall
reverb and subdued voice gains. No network, third-party composition,
recording, reference track or copied sound bank was used. The existing eight
world WAVs match their Git HEAD bytes; their hashes are recorded in the report.

Reproduce the isolated raw render and finish (finish a fresh render once):

```text
swift -module-cache-path /private/tmp/cascade-star-score-cache Tools/compose_cascade_score.swift /private/tmp/cascade-star-score --theme StarObservatory_ConstellationLullaby --scores-only
python3 Tools/finish_cascade_audio.py --music-only --folder /private/tmp/cascade-star-score --only-score StarObservatory_ConstellationLullaby --report /private/tmp/cascade-star-score/metrics.json
```

The existing finisher wraps the three-second acoustic release tail into the
opening. This score alone uses 16 bars, .075 RMS target and .50 peak ceiling;
incumbent scores retain their 32-bar/.12/.72 settings. The final WAV is stereo
16-bit PCM, 44.1 kHz; peak .43359, RMS .07499, clipping samples 0. B/A RMS
contrast is +1.93 dB. The loop jump is .00174 full scale on the left channel
(−55.19 dBFS) and zero on the right; mean DC is below .000001 per channel.
The joined last/first 12 ms waveform was inspected along with the whole
envelope and bar dynamics: `Reports/NightLevels/observatory_waveform.png`.
Exact measurements and provenance: `Reports/NightLevels/observatory_audio_metrics.json`.

The render succeeded; the existing sampler `masterGain` API emitted macOS
deprecation warnings. Editor import, Streaming/Vorbis settings, the ninth
world-music reference, and in-game listening are owned by the parent task.
This content subagent did not operate Unity or edit `ToyAudio.asset`.
