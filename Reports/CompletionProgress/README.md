# Three stages, completion rewards, visible map progress

Normal fresh campaign entries and replays now use three stages from C01 to C160. Only the complete level records a clear, stars, score or first-clear resources. The result sequence is statistics → earned parcel, if waiting → map. Replays improve best stars/score without repeating first-clear rewards. Story and solo modes gate their offered sequence; map progress counts complete levels on the current step. Checkpoints and pending rewards restore after restart.

## Evidence

- `curves.csv`: 637 distinct published primary/localized definitions, every stage has an optimal solver witness and replay validation. The initial exhaustive run proved 625 and identified 12 bindings of C06/C08/C27 that cannot repair a whole-row rotation across locked links. These finales now use a committed, multiset-preserving gust into their original published start. The focused repair proved all 12; the other 625 recipes were unchanged. `editmode.xml` deliberately retains the initial failed diagnostic run, not a claimed final pass.
- `../CompletionProgressFix/editmode.xml`: 7/7 passed. Repaired gated finales, aggregate rewards/tool evidence, access, first-clear/replay records, rating and legacy v1/v2 save/hash compatibility.
- `playmode.xml`: three passed scenarios and the original failing Mixed scenario. Full C01 three-stage result/map/replay; C01–C03 result/parcel/claimed-parcel reload/map without duplicate rewards; checkpoint focus interruption/retry. The Mixed test initially sent input before the descent gate released.
- `../CompletionProgressMixed/playmode.xml`: the corrected Mixed scenario passed, 20.6030164s. Its stage-2 checkpoint, chosen mode and content hash survive scene reload, and only the next offered Mixed level opens. Four distinct runtime scenarios passed across the focused runs; this is not one claimed 4/4 run.
- `native-flow.txt`: real normal-motion C01–C03 progression, nine receiver deliveries, three clears, visible descent at .25s, result → parcel → map. Captures include 390×844 with simulated safe insets, RU large text 320×693 and wide 1440×900. Final review corrected the stage ribbon/unused ledge, themed checkpoint controls, resource columns and map button spacing.
- `map-layout.txt`, `map-short-ru.*`: extra display fixtures with three saved legacy-style receipts for short-screen layout. These fixtures are not three extra played levels. The final map controls are equal height: 47.04 px at 320×693, and 44.20 px at 320×568 with a 7.65 px gap. The footer recalculates on resolution changes and reserves room on short phones.

## Art and motion

[Generated art and complete prompt](../../Docs/COMPLETION_ART.md): painted star, cloth ribbon, open parcel and fabric/leaf confetti. Stars arrive in sequence, the score counts up, and the sealed parcel anticipates before revealing its earned part. Motion is finite/skippable and respects Reduced Motion, effect quality, focus and ads. Canonical progress and rewards commit before animation.

## Validation limits and diagnostics

The initial native harness needed to advance the story dialogue and wait for the input gate after descent. A reflection-based fixture was rejected by the official MCP validator; final layout fixtures use the public profile API in an isolated save. The Editor MCP transport disconnected; the Editor was reopened and final inspections used its installed official relay directly. One cold-start rerun aborted inside Unity Localization's EditorStyles initialization; the focused Mixed run passed after the Game view initialized. Failed fixture attempts are not gameplay passes.

The existing beacon-ending button was wired into the completed-campaign map; the full 160-level ending was not played manually. No new package, phone export, install, full-suite repeat, physical-device touch/gyro test or hardware performance benchmark. No automated check proves human enjoyment. Existing user progress and the pre-existing Cloud-setting edit are preserved.
