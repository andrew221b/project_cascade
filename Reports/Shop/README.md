# Earned-coin shop and Undo — 6 October 2026

Completed locally. The fresh finish review records **ship for the scored UI fixes** after final native recapture. Earlier TouchRevision, Navigation and other verdicts remain historical evidence for their own scopes; this verdict does not approve the whole game.

The shop spends existing earned coins on known tools and optional keepsakes. Two free ordinary Undo uses apply only to the intact current board in a stage attempt. Free current-stage Retry and one-step Hint remain. A bought Return hourglass is the only way to restore the checkpoint before the latest collapse in the current stage. It cannot recover an awarded delivery.

| Item | Price | Benefit and limit |
| --- | ---: | --- |
| Wayfinder scroll | 120 coins | Points out current solver-proved moves until this board breaks; one use per stage attempt. |
| Undo spool | 80 coins | One extra current-board Undo after two free uses; one use per stage attempt. |
| Return hourglass | 200 coins | Exact latest pre-collapse checkpoint and removal of subsequent moves; one use per stage attempt. |
| Leaf Pin, Sun Button, Sky Ribbon, Moon Patch | 240 coins each | Bought once or earned in parcels; accompany Mika on the map without gameplay statistics. |

Tool stock caps at five per kind. Retry starts a fresh stage attempt without refunding consumed stock. Tool use records assistance and prevents a clean record or three-star performance; purchases and cosmetic equipment alone do not. Currency, inventory and the attempt use the same durable transaction with rollback on write failure. Schema 1 retains additive optional fields. The rule note and six-block direction contract are in [SHOP_AND_UNDO.md](../../Docs/SHOP_AND_UNDO.md).

## Executed checks

- [editmode-initial.xml](editmode-initial.xml): 95 cases, 91 PASS and four FAIL, 17:19:20–17:20:22 UTC. Failures exposed outdated expectations for lesson descents and Undo across collapse; the initial result is retained.
- [editmode.xml](editmode.xml): final focused 13/13 PASS, 17:52:02 UTC, 0.3236 seconds. Nine shop cases plus the four corrected affected cases cover costs/caps, current-board Undo, exact hourglass restoration, assistance/performance, guide proof/expiry, ready-row protection, atomic rollback, schema compatibility and disk reload. This is not a fresh 95-case run or full-project suite.
- [runtime.txt](runtime.txt): Editor runtime UI callback assertions PASS for purchases, equip without repeat charge, guide/status, two free Undo uses, one spool, collapse restriction, exact rewind, disk reload, once-per-attempt limits, guide expiry and low-balance disabled state. The wallet was a synthetic 1200 coins in an isolated profile; this does not establish physical input or live commerce.
- [replay-count.txt](replay-count.txt): 365 committed prefixes plus 157 initial states verified.
- [table-completeness.txt](table-completeness.txt): 1791 non-empty entries across 597 keys and EN/RU/UK, with matching format arguments. This does not provide native editorial approval.

Final Unity MCP console: zero errors and warnings. Play Mode stopped; the isolated QA profile override was cleared. Your regular profile was not used for purchases or inventory assertions. See [console.txt](console.txt).

The focused checks preceded the later visual/readability/copy edits; the tests were not repeated after those changes. Final native recapture checked the affected appearance and guide caption, while catalog and transactions remained unchanged. The final runtime assertion evidence remained PASS across the ten captures.

## Native Editor inspection

Ten retained PNG/JSON pairs cover the shop, tool bag, ownership, low balance and Wayfinder status/hand at 320×693, 390×844 and 768×1024 with simulated safe insets. They were captured from Unity Editor, not physical devices. Representative views are [small EN shop](shop-en-small.png), [RU shop](shop-ru.png), [RU tablet shop](shop-tablet-ru.png), [UK small bag](bag-uk-small.png) and [playing guide](guide-playing-en.png).

The smallest recorded counter controls are 50.80 rendered pixels in [shop-en-small.json](shop-en-small.json). Count and full price stay visible when unavailable; disabled purchase/use faces are sage, while executable actions use amber. The second bounded correction raises benefit/count/price reading floors to 18 rendered pixels and 20 for Large Text, and separates compact confirmation from the explanation. The ten inspected views have zero TMP overflow findings in the checked sample.

The fresh independent finish review resolved the scored readability corrections in [small EN shop](shop-en-small.png), [small UK bag](bag-uk-small.png) and [small purchase confirmation](shop-after-buy-small.png). It confirmed preserved stock/cap and price, quiet sage unavailable faces, executable amber actions and a separate confirmation. The final [EN guide ribbon](guide-status-en.png) says Follow the hand, is centered, fits and stays clear of the row and receiver. No regression from the scored fixes was observed. Disposition: **ship at this UI-fix scope**, limited to native Editor simulations; no whole-game, physical-device or enjoyment claim follows.

The selected original object enters the existing arrival/pulse/status feedback; the scroll's guide hand draws in front of the pieces. Screens demonstrate state/layout only and do not establish motion timing, ease of use or enjoyment.

## Assets and preservation

One original transparent 2×2 TravelTools atlas supplies scroll, spool, return hourglass and coin pouch. Exact generation prompt and source/shipping locations are in [ShopTools.md](../../Docs/AssetPrompts/ShopTools.md); the shipping PNG carries embedded prompt metadata. Provenance scan found one shipping raster and zero missing prompts. The generated pixels, authored row gap and hourglass rim are preserved. Existing keepsake and workshop artwork and Unity meta identities remain intact.

The established wood/brass/teal/cream world and Balsamiq typography remain unchanged. This extension adds no shared design tokens. `.impeccable/design.json` is preserved: its narrative already stopped at Navigation and omitted the recorded TouchRevision overview, and its historical navigation shop description still reflects that earlier scope. This ordinary extension reports that drift without repairing an unrelated sidecar.

## Not run

No new player build, phone export/install, physical-device input, live App Store/Google Play purchase/restore, receipt backend, live ads, performance measurement, OS accessibility, native-language approval or human enjoyment/balance/retention test was run for this extension. The existing No ads adapter remains unconfigured without the real store catalog and ad identifiers. Automated solver/save/UI evidence does not certify balance, human fun or release readiness.
