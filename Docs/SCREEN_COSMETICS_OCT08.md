# Gameplay screen cosmetics — 8 October 2026

Append12 permanent cosmetic IDs. Animated scenery: petal_drift480,
firefly_dance600, butterfly_garden720, swallow_flock840,
dragonfly_stream720, lantern_flight960, snow_wings840, comet_trails1080.
Frames/receivers: garden_frame720, sky_frame840, copper_frame960,
lunar_frame1200. Prices use the existing in-game coin wallet only; no stars,
real-money products, earned-clear milestones or new chest rewards are added.
Existing16 Mika IDs/prices and receipt recipe0/1 rotations remain unchanged.
Screen cosmetics alter presentation only and grant no puzzle advantage.

Keep one existing permanent ownership list (`MonetizationLedger.Cosmetics`).
Expose the original16 separately as `ShopCatalog.MikaTrophyIds`, scenery/frame
lists separately, and all28 as `TrophyIds` for shared ledger/catalog validation.
Existing atomic coin purchase supports every ID; reject the new IDs from the
legacy mock-purchase restoration route. No new ledger kind or schema is needed.
Existing paid `cosmetic` grants remain valid for Mika IDs only; screen IDs
cannot arrive through that grant kind.

Append independent nullable `JourneyRewards.EquippedScenery` and
`EquippedFrame` fields. Missing/null means the default screen and is omitted
from JSON, preserving previous cloud baseline hashes. Slot validation requires
the matching category and permanent ownership. Mika's existing equipped look
remains independent. Each durable equip/unequip saves the slot and unchanged
ledger together, restoring the prior slot on save failure.

Cloud equipment merging uses the persisted three-way baseline: unchanged local
slots adopt remote changes; changed local slots retain local intent, including
unequip, and win simultaneous-change ties. An unregistered joining device
adopts the existing account's screen slots. Rejected purchase/ownership deltas
fall back to a valid remote slot or default rather than equipping an unowned
item. These policies add no clock or receipt reroll.

Validation:93 focused pure-C# source checks passed using a standalone NUnit
assertion runner and the installed Unity compiler/Mono runtime, including8 new
screen cosmetic checks. The linked Cloud Code module passed50 fake-transport
boundary checks, including7 new price/slot/ownership/legacy-hash scenarios.
Whitespace check passed. Unity MCP connectivity recovered: all four focused
PlayMode cases passed after the final catalogue layout adjustments. Native
camera review uses an isolated temporary profile and preserves the puzzle hash.
No phone build, package install or live cloud deployment ran. Redeploy the linked
module before activating cloud validation of the twelve new IDs.


Presentation: three catalogue categories (Mika, Scene, Frames), eight-item
shelves, coin-icon prices, grayscale locked art and animated miniature previews.
Ownership and equipment remain separate. A selected scene/frame can be removed;
both coexist with Mika's outfit. Frames use the authored hollow rim in the actual
receiver plus small outer-corner accents; the receiver symbol positions,
platform lifetimes, inputs and solver transitions stay unchanged.

Nine additional wildlife species have four authored wing poses each. Shared
measured atlas bounds/torso pivots feed scene visitors and UI previews. At most
three extra scene actors (one on low quality) use the existing presentation clock,
curved routes, depth scaling and stage/island salts. The existing birds/insects,
world lighting and world background motion remain. Cosmetic motes use capped
reusable UI objects in edge lanes. Reduced Motion/Effects Off suspend or remove
visitor movement; pause/focus preserve the existing shared-clock behavior.

Current Application and Presentation source compilation succeeded; Presentation
has seven pre-existing warnings. Four focused PlayMode cases (two visitors, two
catalogue/equipment flows) executed successfully in Unity. EN/RU/UK have31 new
unique source translation keys plus the existing Mika key; native String Tables
were refreshed with LocalizationAssets.Build. Narrow catalogue labels retain
their own type-size limits across resize; four-item frame shelves omit the empty
second shelf and use a larger receiver preview. Evidence is recorded in
Reports/ScreenCosmetics-Oct08. Human enjoyment is not inferred from tests.
