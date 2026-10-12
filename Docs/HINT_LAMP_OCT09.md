# Hint lamp correction — 9 October 2026

Confirmed cause: the Hint button invoked PuzzleInteraction.ApplyHint on its second click. That method executed the solver command; the next click requested another unrestricted offer. The UI bypassed the inventory hint path.

Decision (default proposed to the user): one free revealed step per stage attempt. Further help is selected explicitly from the existing bag. Repeating the lamp only toggles the current pointer; the player makes the move. A paid Signal Lantern remains the explicitly selected, inventory-charged one-move tool. Prices and product IDs are unchanged. Retry starts a fresh stage attempt; Undo and reopening the save do not refill the allowance. A new stage has its own allowance. Unknown/dead-end/stale offers never consume it.

An optional FreeHintStateHash on AttemptRecord preserves the one granted state and permits showing that same step again after Undo/reload. Old records without the field retain one free grant; no destructive migration. Free grant and save are atomic. The hint-executes-command APIs are removed so hint consumers cannot accidentally auto-play. Existing tutorial command gates still require the player to perform the shown move.

## Rewarded hint cadence — 10 October 2026

The user requested an ad on alternating new hint states. Keep each stage's existing free allowance: the first new hint is free; subsequent new hint states alternate rewarded/free while free allowance remains. Once the free allowance is spent, each additional new hint requires an explicitly accepted rewarded video. An ad-due hint cannot fall back to a free hint when video is unavailable; the free allowance is preserved and the player can retry later. Canceling the offer spends nothing. Each confirmed video adds one token, spent atomically against that attempt and state. The player still performs every move. `PaidHintStates` supplies the existing durable per-state idempotency; no profile schema, price, product ID, or hint free-budget change.
