# Shop and travel tools — 6 October 2026

Design note before changing economy/application rules and additive save fields.

The shop spends earned coins: 40 per new delivery, existing chapter bonuses and verified rewarded coins. Replays do not pay again. No package, store product ID, puzzle transition or solver rule changes. Remove Ads keeps its real store adapter; live payments need the author's unregistered store catalog and keys.

Ordinary Undo gets two uses per stage attempt, only while playing on the same intact floor as the last command. It cannot recover a collapsed floor or undo an awarded win. Retry is free and restarts the current stage with a fresh attempt allowance; it does not refund consumed inventory. Undo history, usage and assistance survive reload.

Three consumables, bought one at a time with known contents:

- Wayfinder scroll: 120 coins. Automatically points out the next solver-proved move on the current board until it breaks. The normal one-step Hint remains free. One scroll per stage attempt; an impossible, unknown or already-ready position cannot consume a scroll.
- Undo spool: 80 coins. One extra ordinary Undo after the two free uses are spent. Same-board restriction remains. At most one spool per stage attempt.
- Return hourglass: 200 coins. Restores the position immediately before the latest floor exit in this stage, discarding subsequent commands. One per stage attempt. Cannot rewind a completed stage, delivery or reward. Unlike the hanging bomb hourglass, this is a player tool for returning upward.

Each stock is capped at five. Currency and stock change in the existing durable transaction. Tool use saves the spent item and restored attempt together, with rollback on failure. A purchase never executes a puzzle command. Using a purchased tool records assistance: no clean record or three-star performance from that run. Cosmetic keepsakes cost the existing 240 coins, are bought once, can accompany Mika, and remain earnable through parcels. No random paid boxes or paid difficulty inputs.

The baseline 40-coin delivery reward makes a spool two new deliveries, a scroll three, a return hourglass five and a keepsake six before existing bonuses. These are bounded optional conveniences alongside free Retry/Hint. They are a design cadence, not measured player-balance or enjoyment evidence. Buying inventory or equipping a cosmetic does not mark an attempt assisted; using a tool does.

Save schema stays 1 with additive optional fields: spent coins and three tool stocks in MonetizationLedger; free Undo uses, purchased-tool flags and active guide floor in AttemptRecord. Old saves default conservatively from UndoCount; expired guide floors cannot reactivate. The local checksum is corruption protection, not server anti-cheat.

## Direction contract

THESIS: A small workshop counter lets the player inspect one real object and buy its clear, bounded benefit; no maze of linked destinations.

OWN-WORLD: Existing honey wood, brass, deep teal enamel, cream cloth and rounded CASCADE lettering; original transparent tool art, tactile shelf selection, amber purchase action.

STORY: Coins earned on the journey buy a little help or something Mika can wear. Counts, price and limits remain visible together. Purchases are optional.

FIRST VIEWPORT: Back/title and coin pouch at the top, Tools/Keepsakes tabs, three or four objects on one shelf, enlarged selected object, one short explanation and stock count, then one purchase button and My tools/Remove ads links. The tool bag offers the same objects and one Use action, with its precise eligibility explained.

FORM: Extension of the committed native workshop counter; code-led, existing system and component vocabulary. No new visual-world selection.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

## Implemented surface

`CascadeGame.Shop.cs` extends the incumbent workshop with Tools/Keepsakes tabs, simultaneous object selection, an enlarged original illustration, a short benefit, persistent stock/ownership and price, and one purchase/equip action. Confirmation is a separate compact line. The Tool bag uses the same layout and exposes one Use action. It is reached from Pause and the loss flow; Back returns to the originating game or menu.

Amber purchase/equip/use faces are reserved for executable actions. Unavailable actions switch to the quiet sage panel and readable dark text. EN/RU/UK copy explains balance, stock cap, empty inventory, stage use, remaining free Undo, current-board history and availability of a collapsed-board checkpoint. Benefits, counts and prices have an 18-rendered-pixel reading floor, raised to 20 with Large Text. Each stock can be purchased one at a time; a keepsake already owned through purchase or parcels offers Take along/With Mika instead of a repeat charge.

Tool feedback carries the selected original object into the existing arrival, pulse and status sequence. The Wayfinder hand stays in front of the row and refreshes from current solver proof. The gameplay/stock transaction finishes before presentation; motion, ad callbacks and tween completion cannot purchase, consume or restore anything. A stale, unknown or unsuitable scroll proof leaves inventory untouched.

The original transparent 2×2 `TravelTools.png` contains the Wayfinder scroll, Undo spool, Return hourglass and coin pouch. Its generated pixels are retained; the row slice follows the authored gap to preserve the hourglass rim. Exact prompt and source/shipping paths are in `Docs/AssetPrompts/ShopTools.md`, with prompt metadata embedded in the shipping PNG. Existing keepsake, workshop and menu assets and Unity meta identities are preserved.

## Validation checkpoint

`Reports/Shop/README.md` separates the initial 91/95 EditMode result from the corrected focused 13/13 result, retained runtime callback assertions, replay/localization evidence and native Editor captures. Ten shop/bag/guide views cover 320×693, 390×844 and 768×1024 with simulated safe insets; captured TMP overflow findings are zero. The smallest captured counter control is 50.80 rendered pixels. The fresh finish review records ship for the scored UI fixes: larger readable benefit/count/price, separate confirmation, sage unavailable faces with executable amber actions, and the shorter centered EN Follow the hand ribbon clear of the row and receiver. No regression from those fixes was observed. This is a native Editor scope verdict, not whole-game approval; no prior verdict is reused. Focused tests were not repeated after the visual/copy edits.

No new player build, phone export/install, physical-device input, live store/ad purchase or restore, performance measurement, native-language approval or human enjoyment/balance/retention test was run for this extension. The existing No ads adapter stays unconfigured because no real App Store/Google Play catalog or ad identifiers have been supplied. Disabled provider preparation is not a live-payment success claim.
