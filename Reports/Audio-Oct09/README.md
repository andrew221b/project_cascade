# Lower, bittersweet music — 9 October

Ten active scores revised and installed at their existing WAV paths. All corresponding `.meta` hashes and menu/world asset GUID references retained.

- `score-review.json`: source before/after measurements and hashes; every solo's first-beat note is 12–13 semitones lower, ten distinct loops, unchanged tempo/duration, zero clipped samples and no PCM boundary step. A/B section/adjacent-window measurements remain bounded. Spectral centroids are lower in every theme.
- `render-metrics.json`: final loop lengths, levels and seam measurements.
- `score-routing.txt`: 330 executed combinations of the actual `ToyAudioLibrary.ScoreFor` selection plus two fallbacks, using minimal engine type stubs. This is **not** an AudioSource or Unity PlayMode run.
- `installation.txt`: successful copy and metadata preservation check. Prior active WAVs preserved in `/private/tmp/cascade-oct09-audio-before`; the composer's non-bittersweet path still reproduces the original arrangements.

Native import/readback, new PlayMode map → level/each-world/stop-recovery/map-return regression, speaker/headphone listening and device performance **NOT RUN**. MCP targets another open project; CUA input to the intended editor fails. No other project was changed. The user has been asked to make the CascadeUnity editor available. A short generated WAV preview was emitted, without claiming a listening verdict.

No iPhone export/build/install. See `Docs/AUDIO_BITTERSWEET_OCT09.md` for arrangement and routing details.
