# Audio asset audit — 2026-10-07

The checked-in assets do not explain total silence. Every reference in `ToyAudio.asset` resolves to an existing WAV; all 12 `ToyCue` events, the menu theme, nine world themes and natural wind are assigned. The fallback score, rain bed and timber snap also exist. No asset replacement is required to restore audio.

## Signal and reference checks

Python standard-library `wave`/`array` inspected all 44 Cascade WAVs: readable PCM, nonzero frames and nonzero signal in every file. The assigned menu/world themes are stereo, 44.1 kHz, 16-bit PCM. Their levels below are unweighted file RMS, not perceived loudness or device output measurements.

| Route | Assigned theme | Duration | RMS dBFS |
|---|---|---:|---:|
| Menu | Home Among Clouds Balanced | 68.57 s | −18.4 |
| World 0 | Wind Garden — Paper Wings | 62.61 s | −18.4 |
| World 1 | Rain Mill — Rain on Cedar | 75.79 s | −21.6 |
| World 2 | Copper Orchard — Golden Afternoon | 55.38 s | −18.4 |
| World 3 | Cloud Post — Letters on the Breeze | 60.00 s | −19.5 |
| World 4 | Glass Lagoon — Reflected Moon | 72.00 s | −18.4 |
| World 5 | Moss Archive — Stories in the Leaves | 80.00 s | −18.4 |
| World 6 | Snow Spire — Warm Light in Snow | 73.85 s | −20.4 |
| World 7 | Great Beacon — The Way Home | 57.60 s | −18.4 |
| World 8 | Star Observatory — Constellation Lullaby | 48.00 s | −22.5 |

The 12 event clips last 0.20–1.69 s, peak at −8.0 to −6.9 dBFS, and cross −45 dBFS within 0.1–16.1 ms of their starts. Select and Stamp are short clicks; the longer action clips include tails. Land and Fragile have lower full-file RMS (−38.1/−40.4 dBFS), but still have substantial transient peaks. These measurements do not justify raising their gain without listening.

The first four alternate slots repeat their primary references, so existing variation comes from pitch changes. The other eight alternate slots are intentionally empty; playback falls back to the assigned primary. These are not missing feedback events.

## Important follow-up

- Restore and verify listener/source lifecycle first. AudioManager has audio enabled and global volume 1; checked-in clips and importer settings contain no blanket mute.
- Existing `Handheld.Vibrate()` requests do not specify duration or intensity and are not rate limited. Use brief native taps with a cooldown for moderate mobile feedback; avoid adding a buzz to every overlapping event.
- Retain the present score and event library. Any optional variation or gain adjustment should follow audible review after playback works.

This audit changed documentation only. Unity playback, speaker audibility, subjective sound quality and mobile vibration were not validated by these file checks. Runtime/UI validation is recorded separately by the implementing task.
