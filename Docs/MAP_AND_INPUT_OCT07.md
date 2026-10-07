# Map, input and coaching — 7 October 2026

Scope: preserve deterministic puzzle rules, prices, tools and mode progression. Cosmetic map deformation and finger hit testing do not issue extra commands.

Save design note: add optional global SeenCoachPrompts list of stable English UI keys to PlayerProfile. Old saves initialize an empty list; no existing field, ID or version changes. Dismissal or a fulfilled contextual instruction retires that hint across stages, modes and relaunch. Mechanic discovery remains separate. Preparation/save errors are not suppressed.

Baseline: map artwork is 863×1823 but displayed in a nearly square rect; this compresses islands. TokenSlot only uses UI raycast targets, while visible token bodies move below those rects. ResetInstruction clears dismissed hints each new coach. Mode mount passes through a different screen, re-triggering the map's leftward arrival tween. The reported tall Undo matches the previously fixed result-parent issue; current checkpoint/pause sequence needs regression validation before attributing a new cause.


Implemented:
- Fit the existing map artwork at its true cropped aspect ratio. Taller island silhouettes restore vertical separation without stretching the authored buildings. Five independent low-amplitude offsets deform the continuous map mesh smoothly; markers follow the same motion. Clouds have wider drift, and an existing glider passes through a shallow depth arc. Reduced Motion freezes the island motion.
- Mode changes retain ambient offset and bypass the page-arrival slide. Saved progression and mode rules are unchanged.
- Release hit testing includes each visible token body and a small margin above it, excluding the held piece. Atomic command validation remains authoritative.
- Closed coaching prompts and successfully learned swapping are persisted across level/mode changes and relaunch. The initial hand gesture also respects the learned receipt. First-time mechanic explanations are unchanged.
- Disabled checkpoint Undo keeps an opaque, legible treatment. The existing result-parent layout guard was retained. The submitted oversized Undo was not reproduced in the current native pause/resume sequence; no new completion-state cause was invented.

Validation: native Unity MCP, isolated profile. Reports/MapInput/checks.txt has 23 passing assertions: direct body and +18 px release each issue one swap with no UI raycast; learned and closed hints survive disk serialization; next-level hint receipt; active puzzle pause stays active; a genuinely solved stage stays solved after pause; Undo/Map equal height; all three mode switches have no menu slide; map source aspect, island/cloud motion and Reduced Motion. Reviewed native captures at 390×844 and 320×693 with simulated safe insets, RU/UK and large text, plus a later world. Final corrections increased Mika's physical size and moved the current-location label to the island's side. No new assets were required.

Limits: this is Editor validation, not physical phone touch/gyro or measured device performance. No phone build/install, full regression suite, or claim of bug-free gameplay. Settings were unchanged here; their padding was corrected and reviewed in the preceding MenuClarity pass. Five final captures retained, intermediate captures overwritten.

Review harness correction: the initial Reduced Motion assertion ran before LateUpdate after the capture restored its layout. A focused native check after LateUpdate verified all five anchor transforms at zero. The helper now waits for end-of-frame; gameplay code needed no change. Final checks/status include that focused confirmation.
