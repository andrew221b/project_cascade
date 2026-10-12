# Woodland documentation reconciliation

Scoped current-state pass, 5 October 2026. Wrote only `PRODUCT.md`, `DESIGN.md`,
`.impeccable/design.json` and this report. Read the shipped
`impeccable_documenter.toml` and all of `reference/document.md`; no context
interview, identity replacement, Unity mutation or source/assets edit was run.
The author explicitly approved this wood/world/motion system extension.

## Implemented facts

- The playable catalog has 160 primary definitions plus 477 localized bindings:
  637 concrete definitions, not 637 campaign stops. C01–C40 remain the original
  tutorial route. C41–C160 are 120 new puzzles; shortest ordinary solutions span
  5–12 accepted actions, mean 7.8667. Ten optional stamp detours are in this new
  content. `CampaignExtensionTests` asserts the 160/477 catalog counts;
  `Reports/P20/generation-summary.json` and completed graph/replay evidence hold
  the authoring measurements. This is challenge evidence, not enjoyment proof.
- `JourneyStory.cs` implements eight named regions and 33 page starts, with
  four stops on the last page of each new 24-stop region. The original story/page
  identities are retained; 25 pages extend Mika's optional journey to the Great
  Beacon. Region-specific plaza anchors bind the map's live controls to artwork.
- `LayeredWorld.cs` retains Wind Gardens, Rain Mill and Copper Orchard artwork,
  then loads five original Woodland Base/Map pairs. WoodBoard is the eleventh
  raster. Native alpha and source rasters remain intact; the solid board body
  band is framed through WoodBoardFrame. Exact prompts and provenance remain in
  `Docs/WOODLAND_ASSET_PROVENANCE.md`. Wide scenery uses full-height side wings,
  the central sky crop and Mika; separate clouds, plants and regional weather
  remain presentation layers. The incidental wheel is no longer shown.
- `ToyMachineView.cs` and `.Woodland.cs` create wood platforms, hemp strands,
  knots and fixed upper pegs. Strength pips, cracks, fuse beads and held-command
  forecasts describe resolver outcomes. Decorative load/wind springs are
  bounded. `PuzzleEventPlayer.cs` maps each original source piece to its committed
  destination; quadratic fall, small stagger/rotation and damped settling do
  not choose gameplay order. Pause/focus/Reduced Motion/skip converge through
  the existing presentation lifecycle.
- `CascadeGame.DeviceDemo.cs` and `DeviceDemonstration.cs` present nine pictorial
  cause/effect guides from precomputed resolver outcomes. They include entry
  landing/rotation, paired motion, consumed bridge and bomb outcomes. Reduced
  Motion uses stationary translucent before silhouettes and the settled after
  pose. Captions remain live and localized. Comprehension by non-readers is
  untested. `WoodBoardGraphic` and `WorldWeatherGraphic` now require the native
  CanvasRenderer; the fix is source correction, not an alpha alteration.
- The travel bag seats Mika above a wooden bench. Map seals derive from saved
  proven stamp-goal records and add no save field, currency grant or travel gate.
  Domain/Solver/rules, schemas, prices, packages and prior IDs are unchanged by
  this extension. Long Reach and online remain design-only. P19 is a disabled
  mock service boundary, with no live Ads/IAP integration or real receipt proof.

## Token and sidecar mechanism

Used `reference/document.md` Steps 2b, 4 and 4b: normative primitive values live
in DESIGN frontmatter; the schemaVersion 2 sidecar carries keyed color metadata,
preview ramps, native component descriptions and motion/narrative extensions.
This was a targeted merge, not wholesale metadata regeneration.

Added exactly four material tokens from actual literals:
`wood-fallback` (missing-texture branch), `hemp-rope` (suspension/knots),
`wood-chip-dark` and `wood-chip-light` (collapse debris). Their matching colorMeta
keys have eight-step synthesized OKLCH ramps; ramps are preview aids rather than
new shipped colors. The platform's frontmatter flat fill points to the documented
fallback, while its actual painted texture/UV framing lives in the native
component extension. Retained every incumbent token ID, including platform-mint
for legacy assets, all font/type values and the Sky Wind Workshop identity.
Only changed-scope sidecar platform/map, fall/world/guide motion and narrative
statements were reconciled.

Local consistency check passed: 34 frontmatter color keys match 34 colorMeta
keys; each of the four new ramps has eight entries; component token references
resolve; component properties stay within the documented eight-property schema;
the eight canonical DESIGN sections remain ordered. JSON parsed successfully.
This is artifact consistency validation, not a new Unity test run or a browser
preview claim. Native-only components keep empty HTML/CSS preview fields.

## Evidence and current validation limits

Read source, `Reports/P20/README.md`, `Reports/P20/generation-summary.json`,
Woodland test XML and the native renderer diagnosis. Personally inspected the
fresh full PNGs `Reports/Woodland/guide-2.png` and `map-13.png`: the first shows
native plank grain and stationary before/after silhouettes; the second shows
Glass Lagoon stops on painted plazas and separate weather marks. Root confirmed
all nine fresh guide textures/meshes, rain weather and refreshed guide/map/bag
captures. Broader captures and motion samples remain at `Reports/Woodland`;
prior earned-object evidence remains at `Reports/P17`.

Historical P20 evidence: 466/466 EditMode and focused 1/1 PlayMode.
Woodland checkpoint: 5/5 EditMode and 16/16 focused PlayMode before the final
renderer/guide correction. Final affected UI regression: **4/4 PlayMode PASS**,
`Reports/WoodlandFix/playmode.xml`, 11:29:09–11:29:16 UTC. No second full-suite
run was performed or claimed. Final independent review disposition is pending
at this documentation pass. A final local player build is **PENDING**; root owns
its separate build report and later evidence reconciliation.

Physical iOS/Android touch, haptics, performance, mobile OS kill/power loss,
speaker/headphone listening, native-language editorial approval, non-reader
comprehension, live Ads/IAP, human enjoyment and retention are **NOT RUN**.
Online is design only. No no-bugs, release-readiness or proven-fun verdict is
inferred from captures or automated tests.

## Drift not canonized or repaired

Unrelated pre-existing P14 sidecar prose still describes settings as in-memory,
uses session-progress language, and carries obsolete ranks/parcels/keepsakes
implementation wording. The root prose already reflects later persistence/meta
phases. These inherited sidecar discrepancies were reported, not broadened into
this authorized wood/world/campaign reconciliation. They must not be read as new
rules or a fresh current-state assertion. No visual defect was turned into a
normative token to conceal a finding; source fallbacks are labelled as fallbacks.

## Final root reconciliation

The same scoped reviewer resolved all four scored findings after the two correction rounds and documentation update: disposition **ship for the scored scope**. Root completed one successful local Mac player build and CUA launch/first-level drag-to-victory smoke; see ../FinalPlayer/README.md. No repeat full suite or additional build was run. Root also corrected the four disclosed inherited sidecar prose statements to current persisted preferences/receipts/local meta; token and component structure are unchanged. NOT RUN limits above remain in force.
