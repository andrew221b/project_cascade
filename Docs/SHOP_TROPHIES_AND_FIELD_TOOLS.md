# Shop and trophies — 6 October 2026

This is a code-led revision within CASCADE's authored Sky Wind Workshop. Timber,
brass, teal enamel, ivory reading surfaces and live Balsamiq lettering remain.
Shop and collection now use two stocked shelves followed by one attached
description/action area. The selected object stays on its shelf; there is no
duplicate floating hero. The shop tabs are Tools and Trophies. Player-facing
collection labels are Trophies / Трофеи / Трофеї across EN/RU/UK; legacy Gift
field and localization keys retain their storage identities.

The gameplay bag control sits between Undo and Descend when any tool stock
exists. It opens the same shelf-based bag directly; Pause and the loss flow
retain access. Unavailable actions explain empty stock, free undos remaining,
missing current-board history, no broken board or a tool already used this stage.
Tool descriptions and the visible assistance consequence precede Use:
“Tools: up to 2 stars. No clean record.”

## Five tools

The original enum ordinals and prices remain. SignalLantern and LandingLens
append two values. Prices are earned coins, with stock capped at five per kind
and one use of each kind per stage attempt.

| Tool | Coins | Effect |
| --- | ---: | --- |
| Wayfinder scroll | 120 | Points out solver-proved next moves on the current board until it breaks. |
| Undo spool | 80 | One extra current-board Undo after the two free uses. |
| Return hourglass | 200 | Restores the exact checkpoint before the latest broken board in the current stage and removes subsequent moves. |
| Signal lantern (`SignalLantern`) | 160 | Executes one solver-proved non-drop command through the existing resolver, with normal wood, contact and bomb costs. |
| Landing lens (`LandingLens`) | 100 | Displays the immutable exact prediction of descending now: the next row and outcome, without applying the drop. |

Any used tool marks the run assisted, caps it at two stars and excludes a clean
record. Buying stock alone and cosmetic equipment do not affect performance or
adaptive difficulty. Free hints and current-stage Retry remain available. Retry
starts a fresh attempt without refunding used stock. Tools cannot rewind an
awarded delivery. Test mode hides prices, uses preview inventory and resets that
stock on restart; the live purchase action contains the earned-coin price.

Landing lens captures `AttemptService.Predict(PuzzleCommand.Drop)` before use.
The closable overlay shows that resolver result's row and outcome while play and
ambient motion are paused. Closing resumes presentation without moving the row.
Signal lantern requires a current solver proof, rejects EarlyDrop, commits its
command before motion and publishes the exact saved transition through the
interaction presenter. Neither tween completion nor preview/close callbacks
change the row or grant an item.

## Eight trophies, four parcel recipients

Shop and collection use the same eight-item catalogue. Every trophy costs 240
earned coins, is bought once, has no gameplay statistics and can be worn by Mika
on the map. The original IDs `leaf`, `sun`, `sky` and `moon` remain Leaf Pin, Sun
Button, Sky Ribbon and Moon Patch. They remain earnable through the original
parcel route. Four shop trophies append `wind_chime` (Wind Chime), `rain_badge`
(Rain Badge), `copper_acorn` (Copper Acorn) and `beacon_brooch` (Beacon Brooch).
These are virtual earned-coin catalogue IDs, not native store SKUs.

The legacy parcel recipient list remains exactly four IDs, with three fragments
per original trophy and a twelve-fragment total cap. Pending fragments still
reserve capacity; order selection and idempotent receipts are preserved. The
new four trophies do not extend parcel rewards. An unowned new trophy links from
collection to the shop; an original one retains its parcel progress/action.
Owned trophies offer Take along, and the equipped trophy shows With Mika.

## State and compatibility

Inventory, currency and attempt-use metadata commit together through the
existing transaction; failed persistence rolls tool use back. Schema 1 keeps
optional zero-default stock/use fields and existing Gift storage names. Original
prices, IDs, parcel receipts, campaign content, package pins and Domain/Solver
rules are preserved. There is no gameplay physics change.

## Source and visual comparison

`Application/ShopCatalog.cs` defines five enum entries, stock limit five and eight
trophy IDs; `Application/JourneyRewards.cs` retains four parcel IDs and cap
twelve while accepting equipment from the full catalogue.
`Application/AttemptService.cs` proves and executes lantern use, returns the
unchanged resolver prediction for lens use, and restores the previous state on
a failed commit. `Presentation/CascadeGame.Shop.cs` creates five tools in a 3+2
shelf arrangement or eight trophies in a 4+4 arrangement, then one title,
benefit, assistance/state line and action area. `CascadeGame.Rewards.cs` routes
collection through that shared surface; workshop miniatures retain their
existing separate layout. `CascadeGame.LandingLens.cs` uses the same ivory
cloth, dark ink, live title type and amber close control for the paused preview.

Compared with `DESIGN.md`, the source and final recaptured native views
`Reports/ShopRefresh/tools-ru.png`, `trophies-ru.png` and `bag-ru.png` retain the
painted timber frame, brass shelf hardware, teal/amber enamel controls, ivory
reading surface and Balsamiq lettering. This is a surface composition change,
not a replacement visual world or a new global token system. `tools-ru.png`,
`tools-uk-small.png` and `bag-ru.png` show the two-star/no-clean-record warning;
the bag also shows its remaining tool count. The existing DESIGN
material/type system remains intact; this pass does not repair its stale
sidecar or unset buildPath.

## Validation at this documentation pass

`Reports/ShopRefresh/editmode.xml` records 46/46 scoped EditMode tests passing
with zero failures. `runtime.txt` records passing Editor checks for five preview
tools, eight trophies, new equipment on Mika, conditional direct bag access,
first-use popups, one proved lantern move, immutable lens prediction, paused
preview/close behavior and reloaded attempt-use flags. Seven final native
Editor captures were recaptured and opened after the final correction batch,
including 320×693, Large Text and simulated safe insets. The final EditMode run
remains 46/46 passing, the runtime checks pass, and the Unity runtime Console
reports zero messages. `tools-uk-small.json` records 48-pixel heights for the
coin-video action, both tabs, purchase action and bag link;
`landing-uk-small.json` records a 48-pixel preview-close target. The independent
finish reviewer marked all six scored corrections resolved and gave a scoped
ship verdict: landing truth, assistance disclosure, 48-pixel targets, action
hierarchy, bag summary and product documentation. This is not a whole-game or
release approval. This documentation pass ran no new tests.

No player build or phone installation was run for this revision, in accordance
with the user's instruction. Full-suite, physical-device, live-commerce,
performance, native-language editorial and human-enjoyment validation remain
unverified. Earlier `Reports/Shop` evidence and its scoped verdict describe the
three-tool/four-trophy checkpoint, not final validation of this revision.
