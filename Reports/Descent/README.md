# Three-stage descent — 6 October 2026

Fresh uncleared C41–C160 journey deliveries have three checkpoints with one receiver target and symbol multiset. Stage 1 is a short warm-up; stage 2 shifts the carried row on arrival; stage 3 keeps the published device/topology set with a proven arrival and wood budget. The whole plan is proved before input. Go lower commits/saves the next stage before its cosmetic travel. Ordinary Retry preserves completed checkpoints. Clear, currency/parcel progress and a full-run route record are published only after the final receiver.

## Executed logic and runtime checks

- `editmode.xml`: 12/12 PASS, 44.607 s. Five new descent cases plus seven director regression cases. `curves.csv` covers all 120 late primary puzzles and 12 selected EN/RU/UK bindings: 132/132 solved/replayed plans, no fallback. Starting minima are 2, then 5–6, then 6–11 commands; C81 is 2 / 5 / 10. Every stage retains target/segments/multiset; the finale retains source devices. The remaining language bindings were not exhaustively prebuilt offline; runtime proves whichever concrete plan is selected.
- `../DescentRecords/editmode.xml`: the changed aggregate route/record case 1/1 PASS, 0.427 s. Whole-run moves/hints/Undo, source identity/stage count and no premature clear. The earlier scoped suite also validates save restore, future-recipe tampering, retry pins, legacy singles and search-budget fallback.
- `../DescentPlay/playmode.xml`: initial 1 PASS / 3 FAIL, 6.262 s. The new per-frame preparing setter cancelled active drag pointers. This was a real regression; it was corrected to change the gate only when its value changes. Legacy single recovery passed initially.
- `../DescentConfirm/playmode.xml`: affected three cases 3/3 PASS, 11.215 s. Real mouse swaps/drop follow predictions; checkpoints survive focus loss and Retry without clears; reduced travel cannot double-advance; the wind guide opens; the final receiver publishes one clear and an aggregate record. Existing single-wind input also passes.
- `../DescentFinalPlay/playmode.xml`: updated full-run record/replay case 1/1 PASS, 4.197 s. Journal identifies Level 81 / 3 stages, and Play this level starts a fresh three-stage delivery without awarding another first clear. Four distinct PlayMode cases have passing evidence across these runs; no second full suite was run.

## Native Unity visual evidence

| Final frame | Viewport | Purpose |
|---|---|---|
| `stage-one.png` | 390×844 EN | Short warm-up, stage counter, proven starting minimum, shared target. |
| `checkpoint-ru.png` | 320×693 RU | Readable cream checkpoint cue and one gold descent action. |
| `stage-wind-ru.png` | 320×693 RU | Carried row, arrival arrow/caption and short wind coach. |
| `stage-devices-words-ru.png` | 320×693 RU | Raised centred letters, repair hanger and entry caption with clearance. |
| `stage-routes-ru.png` | 320×693 RU | Source level, three stages, whole-run 17-move record and fresh-play action. |
| `descent-mid.png` | 390×844 EN | Departing receiver and arriving connected wooden rig. |
| `descent-words-mid.png` | 390×844 RU | Letters, device art and captions travel with their rig. |

Initial inspection found low-contrast checkpoint copy and an entry caption colliding with the repair hanger. One correction moved short checkpoint feedback to its cream band, moved the caption below tools, hid the minimum during travel and completed route identity metadata. Confirmation exposed a concrete font-atlas defect in the new carried text: TMP Graphic.mainTexture was UnityWhite. Font snapshots now keep their font material atlas; only the two affected travel frames were confirmed again. Final files replace prototypes. No open-ended visual polish loop or repeated player build was used.

Travel previews hold the actual cosmetic transition at 48% through an Editor-only pose override; progression is already committed. JSON files record viewport/hit-region measurements, and capture assertions require an unchanged gameplay hash. Their unset device-text metric is not an accessibility certification. Native final probe: stage index 2, prepared, no active travel, zero temporary rigs. Console returned zero errors/warnings; Play Mode was stopped. Original art and packages were reused.

## Limits

No player build, physical iPhone/Android test, frame-time/memory benchmark, haptics or human playtest was run. Exact route length is a balancing input, not proof that a player will find the stages equally difficult or enjoy them. Real-device motion comfort and retention remain unverified. Legacy single recovery keeps More wood; new descent checkpoints keep their pinned recipes and free Retry/Hint, and the director can ease the next fresh delivery.
