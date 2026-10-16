# Three-star completion and open-sky results — 11 October 2026

Design note before implementation. The result scale is now three stars. It uses the solver-proven sum of the three stage minima, all accepted moves actually made (Undo does not erase a move), Undo count and foreground playable time. Gameplay transitions, wood budgets, solver proofs, inventory prices and rewards remain unchanged. The rating is a presentation and progression migration; no puzzle solution changes.

- 3: exact minimum, no Undo, no hint/purchased help, no explosion, known active time at or below max(45 seconds, minimum × 8 + stages × 10).
- 2: clean, unassisted completion within minimum + max(4, minimum / 3), with at most two Undos.
- 1: every other successful delivery. No timeout or timed failure.

Active time includes thinking while the puzzle is playable. It excludes preparation, transitions, coaching modals, item selection, pause, background and advertising. Existing saves without time display an em dash and cannot earn the speed-dependent third star until replayed. RatingVersion 2 is additive. Existing five-star receipts are migrated once as 1–2→1, 3–4→2, 5→3; their score and clear status remain unchanged. The score cap remains 2100.

Results use the sky as their canvas: wider rank ribbon → three large stars → enlarged score plaque → gold and lantern rewards → time/moves/undos → Mika holding a lantern behind the action button. Stars enter left to right through empty, half-filled and filled art states. Generic cream reading plates are removed from menu headers/notes. Focus modals keep a contrasting themed dark surface; illustration borders and physical gameplay tiles remain.

Verification and final captures are recorded in Reports/FiveStarResults and TASKS.md. No phone export or installation is authorized for this task.

## Assets and evidence

Built-in imagegen outputs, preserved with alpha and original user assets retained:
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Workshop/RewardEmblems.png` — gold and contained fireflies, 1774×887 RGBA. Exact prompt: `Docs/AssetPrompts/RewardEmblems.md`.
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Craft/WorkshopPortraitQuiet.png` — 941×1672.
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Craft/WorkshopRoomQuiet.png` — 1672×941. Exact prompt for both edits: `Docs/AssetPrompts/QuietWorkshop.md`.

The new victory star and Mika art are UI illustrations, not rewards granted by animation. The shop still caps purchased help at two stars. Level cards and island totals use three centred star positions. Score counting begins after the third star's arrival.
