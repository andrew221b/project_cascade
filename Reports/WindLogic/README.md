# Wind, calibration and interaction audit — 7 October 2026

## Fixed conflicts

- Fresh single-delivery director recipes now receive a complete background proof before publication. An impossible or Unknown recipe cannot open input; bounded Recovery or published-source fallback remains subject to final preparation. Saved recipe versions and hashes are unchanged.
- Terminal Signal Lantern transitions publish the exact committed result once, before the result screen. Winning tool moves now reach completion receipts, persistence and animation; stale or duplicate publication is rejected.
- Restored Won checkpoints skip Playing-only mechanic discovery and can continue downstairs, including after disable/enable.
- Pending descent/direction requests survive component disable. Worker completion respects Pause, Settings, Home or Shop. Provisional source attempts never overwrite the previous saved attempt/descent, including reward or purchase transactions.
- A failed descent keeps the old save and offers working Retry. Its matching map node resumes that request instead of re-entering blocked story navigation.
- Disabling an open introduction avoids repainting already destroyed canvas labels. This exception was observed when leaving the actual Editor Play Mode, then reproduced and fixed with a teardown regression.
- Wind-loop gain follows the actual visible floor pressure, including gusts; tiny breezes have squared quiet gain. Smooth release, mute/suspension and preserved playback position remain covered. No independent sound-gust clock or duplicate gust sound route.

Atmospheric gusts are cosmetic. Gameplay wind is the deterministic landing permutation; the shared resolver applies it before final-row device contacts. No timers secretly reorder a puzzle or consume wood.

## Evidence

| Check | Result |
| --- | --- |
| Focused Unity EditMode mechanics, director, three-stage curves, saves, preparation, atomic interaction and audio envelope | 71/71 passed; `editmode.xml` |
| Ad-service transaction/rollback and pending-grant persistence | 16/16 passed; `../WindSave/editmode.xml` |
| Final unique runtime scenarios across selected runs | 14 passed, no remaining failing scenario |
| Pure C# recipe sweep | 1,289 searches: 176 raw Tight and 8 raw Wind candidates exhausted without solutions; zero Unknown. These were candidates, not demonstrated opened unplayable rounds |
| Existing complete three-stage plans | All 672 eligible Wind/Tight v4 plans built successfully in the pure C# audit; no Unity-wide campaign rerun claimed |
| Native Unity camera review | `ambient.png` and `gust.png`, visually inspected at 390×844 |

Runtime result files retain the original failed test evidence. `playmode.xml` initially passed 10/13; three legacy scenarios acted over first-use lessons or expected a removed guide button. After acknowledging the lessons and using the current guide, `../WindInteractions/playmode.xml` passed 2/3; the remaining assertion incorrectly expected free Undo to restore a broken board. `../WindFinal/playmode.xml` passed the corrected broken-board rule and the expanded map/failed-load recovery (2/2). `../WindShutdown/playmode.xml` passed the additional teardown regression (1/1). Counting each latest scenario once yields 14 passing runtime scenarios, not a fresh full-suite result.

The camera probe sampled actual C18 pressure and loop gain: ambient maxima 0.407972 / 0.01935813; gust maxima 0.8991209 / 0.05084715. Puzzle state hash remained identical. This verifies dynamic gain coupling, not subjective listening quality. Existing music and wind recordings were preserved.

Independent final source review found no remaining P1/P2 issue in the modified loading, Retry, tool publication, persistence or wind-envelope paths. After the last regression: Editor stopped, compilation successful, Console had zero current errors; QA profile detached. Unrelated Addressables/report churn removed; the user's Unity Cloud edit preserved.

## Not run

No phone/player build or installation, full Unity suite, physical-device ads/purchases/audio/performance, or human playability study. These targeted checks do not guarantee the entire game has no bugs or prove enjoyment.
