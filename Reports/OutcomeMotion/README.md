# Outcome motion and workshop — 6 October 2026

Current scoped implementation and native Unity evidence. Existing published puzzles, IDs, prices, Domain transitions and packages were preserved. All deformation, particles and outcome motion present already committed state. New runs use version-2 early-stage tools; saved version-1 plans keep their original derivation and hashes.

## Result

- Success: 1.35-second lantern ceremony, staggered receiver bounce, local radiance, pooled colored seeds and Mika lifting his light. Failure: 1.05-second gentle wobble and a separate thoughtful Mika pose. The goal and row remain readable; result actions appear after the ceremony. Checkpoints use a shorter acknowledgment. Reduced Motion uses short static/color feedback; interruption settles poses.
- The full yellow menu wash is replaced by a coherent wooden workshop window. Portrait composition fits its full frame; wide screens retain WorkshopRoom. Clouds, light and sparse dust sit behind controls. Headings and body copy are centered with clear frame margins.
- The required symbol sits in a bolted tag on the hanging tool's wooden crossbar. The same construction appears in the animated guide. How to play is reachable from Pause and the device guide, with EN/RU/UK copy explaining gestures, tags and the receiver.
- A new three-stage run has repair on stage 1, a bomb and lower stamp on stage 2, and the original final devices on stage 3. Replay and whole-run explosion totals include earlier stages.
- Weight bows the timber; wear deepens the bow. A spent deck opens into two supported halves, with falling splinters. Shapes, letters and hanging sockets follow the deformed surface. Static weight remains in Reduced Motion. The strength caption follows bowed wood and is hidden during fracture.

## Tests actually run

| Native Editor evidence | Result | Scope |
| --- | --- | --- |
| [Initial EditMode](editmode-initial.xml) | 9/10 PASS | Checkpoint/save/replay/version-1 compatibility, whole-run records and TutorialCoach. Curve case exposed a 7-tick fuse outside the existing 1–6 range. |
| [Corrected curve case](../OutcomeCurves/editmode.xml) | 1/1 PASS | Fuse clamped to 6. All 132 steady curves solved/replayed; 120 primary and 12 selected EN/RU/UK bindings; no fallback in this sample. Minima 2 → 5–6 → 6–11. |
| [Affected PlayMode](../OutcomeConfirmPlay/playmode.xml) | 7/7 PASS | Both outcomes in normal/reduced motion, interruption, delayed controls, target persistence, guide return, wood load, wind/hanging devices, real three-stage input, failure Undo/Retry. |
| [Visual correction confirmation](../OutcomeFinalPlay/playmode.xml) | 4/4 PASS | Outcomes, failure, delivery and wood load; added an actual rendered-wood-thickness assertion. |
| [Wood-support confirmation](../WoodSupportPlay/playmode.xml) | 3/3 PASS | Wear/support, hanging objects and real three-stage delivery after hinge/socket/caption positioning correction. |

There are 10 distinct EditMode and 7 distinct PlayMode cases with passing evidence; subsets above are repeated affected cases, not extra distinct coverage. The first Play startup was aborted after the Console exposed MaterialPropertyBlock creation in a MonoBehaviour constructor. Creation was moved to initialization before the 7/7 run; that aborted startup has no completed test XML. The final fracture-caption visibility and compact RU guide-margin corrections were compiled and checked in native frames, without rerunning the suite.

## Native visual inspection

Unity MCP operated the Editor. Captures use Camera rendering at 390×844, small RU at 320×693, and wide at 1440×900. Outcome snapshots sample the running coroutine at thresholds rather than a manually set pose. `motion.csv` records the actual elapsed progress, UTC timestamp, sag and state hash. Rendering asserts that state hash does not change. Repeated focused wood rows remain in the CSV; the latest row for a filename corresponds to the retained PNG.

One full inspection, main correction and confirmation were followed by focused wood/support/caption confirmations. The first inspection caught a receiver jump, a glider crossing result typography, menu/frame crowding and thin timber caused by using expanded culling bounds as layout bounds. Confirmation caught the worn-caption overlap and missing hinge-following pose on breaking halves. These were corrected; the final focused frames show clear strength text at rest and no caption over the fracture. A final guide-only check narrowed the Russian body text into the workshop window on 320×693. No extra player build was made.

18 retained final frames:

| Area | Frames |
| --- | --- |
| Success | [Early](win-24.png), [middle](win-64.png), [settled](win-settled.png) |
| Failure | [Early](fail-24.png), [middle](fail-64.png), [settled](fail-settled.png) |
| Timber | [Fresh](wood-fresh.png), [worn](wood-worn.png), [fracture/splinters](wood-break.png) |
| Menus | [Travel bag](workshop-menu.png), [workshop](workshop-model.png), [wide](workshop-wide.png), [small RU collection](keepsakes-small-ru.png) |
| Guidance/tools | [Small RU guide](guide-small-ru.png), [bolted word tag](bolted-word-tag.png), [connected bomb](connected-tool.png) |
| Earlier stages | [First-stage repair](first-stage-tool.png), [second-stage bomb/stamp](second-stage-tools.png) |

Final native Console: 0 errors/warnings. Play Mode stopped after capture. Generated Editor link/localization settings noise was restored; the user's Unity Cloud settings were preserved separately.

## Not run

No new player build, physical Android/iPhone checks, device performance/thermal measurements, audio/haptic inspection, extended session or human enjoyment/retention/nonreader comprehension test. Native visuals and solver proofs do not establish that a child understands the game or that it is fun over a long session.
