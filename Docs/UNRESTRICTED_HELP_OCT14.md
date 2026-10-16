# Help policy — 14 October 2026

The user explicitly requested unlimited inventory help, one free undo instead of two, and rewarded video on every second new hint. This changes application policy, not puzzle transitions, campaign certificates, product IDs or prices.

- Remove the two-tools-per-stage cap and per-type once flags as eligibility gates. Every successful use still consumes exactly one owned item and saves atomically. Boolean flags remain historical replay evidence and support old saves. Following the user's later clarification, a hint or help item does not cap stars; actual extra moves, undos, time and damage still affect the rating.
- One free undo per attempt/stage. An owned spool restores exactly one eligible command. Old saves reporting two spent free undos load with the new allowance exhausted.
- Repair can be used repeatedly. Preserve exact replay with an optional ordered list of repair route checkpoints, including repeated checkpoints at the same route index; retain the old single checkpoint field for backward compatibility. Undo/hourglass removes only repairs crossed by its rewind, without refunding stock. Overflow/malformed replay guards remain.
- Unlimited new hints alternate free, video, free, video. Previously revealed states reopen without another ad; an unfinished/unavailable video gives no hint. Existing earned hint tokens can satisfy a paid hint, preserving previously granted value.
- The window selects first, applies explicitly, shows a video pictogram when a hint requires video, and never changes a puzzle from an animation callback.

Validation must cover repeated uses, persistence rollback, repair replay/rewinds, old-save compatibility, hint alternation and non-mutating UI selection. All 540 packaged levels must be audited with the production resolver for uniqueness, causal effects and difficulty/composition progression; automated proof does not establish subjective enjoyment.

## Inventory capacity completion — 15 October 2026

The verification found a remaining five-per-type stock ceiling in purchases, parcel resolution, profile validation and cloud reconciliation. Remove that gameplay ceiling as part of the user's unlimited-help request. Keep the existing integer save fields and catalog IDs/prices; the maximum representable integer remains only an overflow boundary. Purchases must still be funded, each successful use consumes one owned item, and failed saves restore both inventory and puzzle state. Parcel tools above five stay tools rather than converting to gold. Use checked/wide stock arithmetic during cloud merges and wide arithmetic for displayed inventory totals. Validate purchases beyond five, save/reload, parcel claims, repeated use and concurrent funded purchases.

## Resume and economic migration follow-up

A legacy rewards balance query used to set the shared economy revision before inventory prices were migrated. A subsequent sync could then reject genuine old inventory as unfunded. EarnedCoins is now read-only; PlayerProfile.MigrateCoinEconomy remains the single atomic migration of rewards and inventory spending, preserving the old spendable balance. Prices, IDs and serialized schema remain unchanged.
