# Session-only unfinished rounds — 28 October 2026

User request: stop restoring unfinished levels; reopen Home on a cold start and after ten minutes away.

Earned campaign clears, stars, replay evidence, coins, verified purchase/reward grants, collected gifts, equipment, tutorials and preferences remain durable and cloud-synchronised. Attempt/Descent, transient completion-screen state and active personal-mission checkpoints stay in memory for the current session. They are removed from disk/cloud snapshots. The existing schema and product identifiers do not change. Legacy attempt fields are discarded on load without replaying them.

Short app interruptions retain the in-memory round. Ten minutes in the background returns to Home and clears all mode-specific round checkpoints. Native purchase, ad and sign-in controllers are allowed to finish before a pending reset. Parcel rewards already earned remain available in inventory.

Root cause observed on iPhone build 3: PaintHintCounter dereferenced GameEconomy during RestoreProfile, before monetisation was created. This aborted Awake before cloud/IAP/ad initialisation. Retain a null-safe counter even though legacy round restoration is removed, and cover both cold start and pre-economy rendering.

Account flow follow-up: opening account status must not wait for catalog connection or a background cloud save. Explicit sign-in is queued behind background purchase-identity preparation, with visible pending state and cancellation, instead of rejecting the first tap. A real paid grant retains its identity lock until confirmation; it is never cancelled to start sign-in. Game Center/Play Games authenticate the game identity linked to Unity; App Store/Google Play Billing perform their own purchase authentication. User confirmed the Apple Account sheet appeared after Buy: this is expected Sandbox authentication for the development-signed build, not an unsolicited restore. Preserve the store's system purchase sheet and server verification.
