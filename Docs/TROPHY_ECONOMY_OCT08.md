# Ten more trophies and economy balance — 8 October 2026

Design decision, recorded before catalog/rule changes. Add six Mika looks and
four animated screen styles, using virtual coins only. Stars remain ratings,
never purchase currency. Keep all 28 existing IDs, prices, ownership and minted
parcel recipes unchanged. No new store SKU, package or profile schema.

| ID | Cosmetic | Coins | Base first-clear equivalents |
|---|---|---:|---:|
| mushroom_cap | Mushroom hat | 720 | 18 |
| bee_suit | Bee costume | 840 | 21 |
| winter_hood | Winter hood | 960 | 24 |
| pirate_hat | Sky pirate | 1080 | 27 |
| jester_suit | Cloud jester | 1320 | 33 |
| constellation_mage | Star mage | 1440 | 36 |
| ribbon_kites | Ribbon kites | 600 | 15 |
| origami_cranes | Paper cranes | 720 | 18 |
| bubble_dream | Dream bubbles | 960 | 24 |
| aurora_veil | Aurora veil | 1080 | 27 |

New collection costs 9720 coins; the full 38-item catalog costs 28800. Keep
40 coins per unique published campaign clear. Give a 100-coin completion bonus
per ten unique published clears, derived from clear evidence, including prior
milestones on upgrade. Counting clears rather than route groups also fixes the
client/server disagreement caused by their differing journey order. This never
removes an old group bonus: k complete ten-node groups imply at least 10k clears.
Replays, mode/language switches and repeated sync do not farm it.
Income before parcels/help spending: 40 clears = 2000, 160 = 8000, 540 = 27000.
Late-game average is 50 coins per clear, while the original early curve stays.
The ten new purchases therefore represent about 194 late-game clear equivalents.
Free original looks and rare finds reduce completionist spending; help spending
creates a choice without preventing cosmetic completion in the full campaign.

Newly minted bonus parcels use recipe 2. Existing recipes 0 and 1 are immutable.
Retain the old two nine-parcel cosmetic tracks. Add two expansion positions
per nine later parcels, at b = 2+9k and 5+9k, where b = Order/4 - 3. Rotate
the ten IDs above in table order. New finds are at 60/96/168/204/276/312/384/
420/492/528 unique clears, so every added style can be found within the actual
540-node campaign. Four cosmetic receipts per nine later parcels means about
one per 27 unique clears; all ten new finds require almost the entire campaign,
while players can buy a favourite sooner. No clock/RNG rerolls or increased
parcel frequency. Original
fragment chests remain every three clears, later chests every twelve.
Duplicate cosmetics and full tool stacks keep their existing 20-coin fallback.
Claim, inventory, ownership and equipment must save atomically and reconcile
two-device claims without granting the same reward twice. Screen finds equip
only the scenery slot, never Mika's outfit slot.

Keep tool prices 80/100/120/160/200, stock cap 5, two distinct uses per stage,
one use per tool kind, limited current-floor undo and assisted-run rank cap
of two stars. Cosmetics and coin purchases do not change puzzle state, solve
routes, earned stars or level access. New cosmetics cannot use legacy mock
real-money appearance restoration. Raise the coin-pack purchase threshold
from 960 to the catalog maximum (1440), allowing a player to reach the most
expensive cosmetic; verified pending purchases are still delivered above it.
Rewarded ads remain optional and excluded from balance income; no new ad rule.

The cloud module links the same catalog/recipe/reward source. Redeploy it before
enabling cloud validation of the new IDs/recipe; do not claim live deployment.
Validation and asset provenance will be appended after implementation.

Asset provenance: six new 1448×1086 RGBA Mika two-pose atlases were generated
with the built-in Image Gen tool as identity-preserving edits of SkyPilot.png.
MushroomCap, BeeSuit, WinterHood, PirateHat, JesterSuit and ConstellationMage keep
Mika's face, ears, tail, satchel, boots and lantern. Only the selected final
images were copied unchanged; transparent gutters and complete silhouettes were
checked. Both poses and portrait crops use the shared wardrobe renderer everywhere.
A new unchanged 1254×1254 RGBA SceneryAtlasOct08 has four kites, four crane wing
poses, four pearly bubbles and four aurora folds. Measured sprite bounds/body
pivots avoid atlas bleed and wing-pose jumps. Prompt: Docs/AssetPrompts/SceneryExpansionOct08.txt.

The screen effects reuse the bounded atmosphere pool, existing pause clock and
effect/reduced-motion settings. Side lanes keep puzzle contacts clear; they
never issue puzzle commands. Winter Mika now uses gentle snow accents. Locked
art stays muted gray, owned art is colored. Names have one short benefit line
in EN/RU/UK. The four-item scenery page uses a wider preview; narrow titles
reserve more height and width so increased text can fit without clipping.

Final balance: recipe2 at540 clears gives42 bonus chests:19 cosmetic receipts,
6 bounded tool receipts,17 coin receipts. The repeated Sky Pilot can convert
to20 coins, so total earned wallet is27360 if all other cosmetics are new finds
and all chests are claimed. Replay/mode/locale changes mint no new chest.
This pacing is a deterministic design baseline; retention/player enjoyment
needs real play data and has not been inferred from automated checks.

Validation:109 focused standalone C# assertion cases and59 linked Cloud Code
SDK checks with fake transport passed. Native Unity77 EditMode cases and2
PlayMode cases passed. Native camera inspection covers all10 additions, locked
price presentation, all4 equipped gameplay scenes, and320px Ukrainian Large
Text. Sixty narrow name/benefit checks (10items×3locales×2labels) pass, along
with durable purchases/equipment, exact9720 spending and unchanged puzzle hash.
Motion, reduced motion and effects-off were checked in the isolated fixture;
initial capture timing was corrected before measuring frozen actors. Narrow
checks found real title bounds issues, corrected with the extra label room.
Reports/TrophyEconomy-Oct08 records evidence; availability fixtures use synthetic
clears and never access a player's profile or cloud account. No full suite,
phone build, physical-device feel, live commerce or cloud deployment ran.
