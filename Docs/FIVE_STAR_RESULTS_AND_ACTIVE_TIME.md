# Five-star completion and open-sky results — 7 October 2026

Design note before implementation. The five-star rating uses the solver-proven sum of the three stage minima, all accepted moves actually made (Undo does not erase a move), Undo count and foreground playable time. Gameplay transitions, wood budgets, solver proofs, inventory prices and rewards remain unchanged. Minimum + 2 is the generous rating allowance, not a silent modification of a published puzzle.

- 5: exact minimum, no Undo, no hint/purchased help, no explosion, known active time at or below max(45 seconds, minimum × 8 + stages × 10).
- 4: clean unassisted exact minimum without a speed requirement, or minimum + 2 with at most one Undo.
- 3: clean unassisted within minimum + max(4, minimum / 3), at most two Undos.
- Otherwise 2 for a clean completion within that allowance, 1 for remaining completions. Assistance retains the existing ceiling of two stars. No timeout or timed failure.

Active time includes thinking while the puzzle is playable. It excludes preparation, transitions, coaching modals, item selection, pause, background and advertising. Existing saves without time display an em dash and cannot earn the speed-dependent fifth star until replayed. New optional attempt fields MovesMade/ActiveSeconds and completion RatingVersion/ActiveSeconds are additive; legacy receipts retain their old score and up-to-three stars. New score cap is 2100, including three stages and the optional stamp.

Results use the sky as their canvas: rank ribbon → five stars → score and Mika → aligned moves/Undo/time → gold and lantern illustrations → one continuation. Generic cream reading plates are removed from menu headers/notes. Focus modals keep a contrasting themed dark surface; illustration borders and physical gameplay tiles remain.

Verification and final captures are recorded in Reports/FiveStarResults and TASKS.md. No phone export or installation is authorized for this task.

## Assets and evidence

Built-in imagegen outputs, preserved with alpha and original user assets retained:
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Workshop/RewardEmblems.png` — gold and contained fireflies, 1774×887 RGBA. Exact prompt: `Docs/AssetPrompts/RewardEmblems.md`.
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Craft/WorkshopPortraitQuiet.png` — 941×1672.
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Craft/WorkshopRoomQuiet.png` — 1672×941. Exact prompt for both edits: `Docs/AssetPrompts/QuietWorkshop.md`.

The new atlas is a UI illustration, not a spendable gold reward granted by animation. The shop still caps purchased help at two stars. Clear receipts persist five-star bests; the level shelf shows five centered star positions. Timing for five-star score counting starts near the fifth star's arrival; three-star legacy animation timings remain unchanged.
