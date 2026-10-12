# Mika wardrobe expansion — 7 October 2026

User requests cosmetic hats, masks, funny and heroic looks bought only with
in-game currency, with rare chest finds. Star ratings are never a currency.
No new real-money SKU or puzzle advantage is introduced.
Legacy mock-purchase restoration remains limited to the earlier looks; the four
new IDs cannot be granted through that purchase route.

Append four trophy IDs without renaming existing IDs: explorer_hat, frog_suit,
star_knight, fox_mask. Planned prices: explorer_hat480, fox_mask720,
frog_suit960, star_knight1200 virtual coins. Keep existing published prices.
New looks have no campaign gate. Remove purchase-only campaign gates on the
four previously appended premium looks, keeping already earned legacy ownership.
A new clear normally earns40 coins: these prices represent12/18/24/30 clear
income equivalents before chapter/chest bonuses or tool spending. Replays do
not farm first-clear rewards. Buying all four new looks costs3360 coins (84
clear income equivalents); all eight rare/premium looks cost7200 coins.

Preserve all already minted/claimed chest receipts, original12 fragment parcels,
and existing rare premium cosmetic finds. Extend future chest rewards with rare
new looks, modest currency, and bounded tool finds where the authoritative ledger
can support them. Use a published deterministic recipe without clock/random
rerolls; ownership converts duplicate looks into small coin compensation.
Keep stock and per-stage tool limits, clean-score restrictions, atomic spending,
save rollback and cloud reconciliation. Record any recipe compatibility fields
and final cadence here before implementation.

Receipt compatibility decision: append integer `ParcelReceipt.RecipeVersion`.
Missing/zero means the published legacy recipe and is never rewritten. Newly
minted bonus receipts use version1; the first12 fragment receipts remain version0.
Zero is omitted from serialized JSON so existing cloud device baseline hashes
remain compatible with profiles saved before this field existed.
For bonus index `b = Order/4 - 3`, retain the old four looks when
`b >= 4 && (b-4)%9 == 0`, rotating their unchanged order. Version1 additionally
finds explorer_hat, fox_mask, frog_suit and star_knight at `b >= 8 &&
(b-8)%9 == 0`, rotating in that order. First finds are132/240/348/456 unique
clears. Each look cycle spans432 unique clears; eight cosmetic chests among36
later chests is one cosmetic per54 clears on average. The existing finds at
84/192/300/408/516 remain intact. All other receipts at `b%4 == 1` contain one
tool, rotating Landing Lens, Undo Spool, Wayfinder, Signal Lantern, Hourglass;
remaining receipts contain20 coins. A full stack converts the tool to20 coins.
Duplicate cosmetics likewise convert to20 coins. No new look is auto-earned at
a clear milestone, and replay/locale/mode changes never mint another receipt.

Tool grants use an existing ledger `GrantReceipt` with new kind `parcel_tool`,
fixed amount1, a published tool name in Product, and canonical ID
`parcel_tool_<Order>`. The claimed version1 receipt must match that grant.
Claiming mutates receipt and inventory in one durable save and rolls both back
on save failure. Cloud merging recognizes only matching earned receipts,
subtracts the same tool grant when two devices claim the same chest, and resets
unfunded tool claims after an economy conflict so they remain claimable. These
merges retain a previously claimed remote chest's reward/fallback outcome.
If an incoming cosmetic duplicate claim loses its locally purchased ownership
when remote economy is adopted, leave that new claim available to reclaim as
the actual look; previously claimed remote receipts remain immutable. These
are additive fields/kinds within existing profile/cloud schema1; old clients
cannot validate newly minted version1/tool parcels and require the matching
client/module update. The linked module uses the same catalog and recipe code.

Merge integrity correction: conflicting local fragment choices never rewrite
an existing remote receipt. Report the conflict and keep the rejected local
snapshot in the existing archive flow. Reconstruct only missing canonical
orders up to the earned clear count: original orders1–12 use the first gift with
remaining fragment capacity, later orders16/20/... use recipe1. This prevents
different offline gift choices or disjoint clear progress from leaving receipt
holes and subsequently minting an invalid original fragment after order12.

Each full-body sprite keeps Mika's existing two poses and identity; portrait and
map/ceremony images use the same equipped look. New cosmetics map to existing
visual ambiance families only. Shop and collection show owned color versus
muted gray; icon plus number expresses coin cost. Preview taps animate the
cosmetic image and never issue puzzle commands or grant ownership.

Economy validation:85 focused pure-C# source checks passed through a standalone
NUnit assertion runner using the installed Unity compiler/Mono runtime. The
linked Cloud Code module passed43 checks with actual SDK DTOs and fake transport.
They cover original receipts/prices/ownership, save rollback/retry, durable
reload, stock overflow compensation, two-device claims, duplicate cosmetic
conflicts, missing canonical receipt repair, baseline hashes and wallet bounds.
These runs do not verify Unity runtime visuals, human enjoyment, a phone build,
live Cloud Save or production deployment. No phone build requested. Existing
guarded cloud initialization/activation limits remain documented in the module.

Presentation decision: the earned coin wallet, virtual tool purchases, trophy
purchases, equipment and chest claims now use one durable GameEconomy. In native
test mode the fake IAP/advertising ledger remains session-only and cannot replace
or spend this inventory. The shop calls its optional fake ad a Test video; real
coin packs and ad removal retain their existing explicit test labels. A cloud
profile application rebinds the durable ledger, so a later purchase cannot save
an earlier cloud inventory. No paid trophy product is added.

Asset provenance: built-in image generation, identity-preserving edit of
CascadeArt/Cosmetics/SkyPilot.png, actual transparent alpha, copied unchanged into
CascadeArt/Cosmetics. Shared prompt: preserve Mika's cinnamon fur, face, round
pink ears, whiskers, tail, lantern, satchel, boots and original hopeful/thinking
poses; two equal-width full-body cells, aligned baseline and clear gutter; tactile
storybook rendering, warm light, no backdrop/labels/particles/floor shadows.
Variant instructions:
- ExplorerHat: broad ochre felt travel hat with teal feather, sage overalls,
  cream blouse, mustard scarf, visible ears.
- FrogSuit: turquoise plush frog onesie, open hood with plush frog eyes,
  Mika's face and ears visible, cream belly, original lantern/bag/boots.
- StarKnight: rounded teal/gold woodland armor, navy star-embroidered cape,
  open face and gold circlet; no weapon.
- FoxMask: tilted ivory/red/gold fox mask on forehead, unobstructed face,
  indigo embroidered robe, cream trousers and ochre sash.
All four atlases are1448×1086 RGBA with clear centre gutters. Runtime portrait
crops retain the full cell width and upper55% so hats and mask tassels stay intact.
The two8-item shelves preserve touch space and gray locked silhouettes; selection
provides a color try-on without ownership. Leaf/rain/star/petal atmosphere reads
only presentation settings. The first-use finger points by its authored fingertip
pivot; selecting another trophy no longer marks the Mika tap explanation seen.

Native validation completed after unlocking/restarting the Editor:15 Unity
Play Mode frames at390×844/320×693, all four looks, English/Russian/Ukrainian,
small-screen Large Text and the long hat title, colored/gray ownership, fingertip
shelf/Mika poses, eased tap rise/rest, equipped map/dialogue and a tool chest.
Review corrected a missing Choose a look translation, narrow page targets and
dark disabled labels on enamel buttons. Checked shelf/back/page/action/buy hit
regions are at least44×44 pixels and stay inside the capture. All rendering
captures preserve the puzzle hash. Reports/TrophyWardrobe-Oct07 records evidence.
Current native Editor import and scoped Presentation/Editor compilation passed;
no physical-device build, live cloud deployment or full animation/pleasure claim.
