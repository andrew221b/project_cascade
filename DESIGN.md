---
name: CASCADE — Sky Wind Workshop
description: A small keeper carries light through a living sky of tactile puzzle machines.
colors:
  navigation-cream: "rgb(100% 97% 84%)"
  menu-ink: "rgb(5.5% 16% 17%)"
  menu-paper: "rgb(99% 98% 92%)"
  menu-sage: "rgb(84% 91% 87%)"
  contact-success: "rgb(10% 40% 34%)"
  ui-ink: "rgb(16% 28% 29%)"
  high-contrast-ink: "rgb(6% 17% 18%)"
  ui-paper: "rgb(100% 96% 84%)"
  drop-gold: "rgb(98% 74% 32%)"
  hover-tint: "rgb(100% 94% 78%)"
  pressed-tint: "rgb(82% 88% 81%)"
  disabled-tint: "rgb(88% 90% 84% / 70%)"
  preview-gold: "rgb(95% 73% 29% / 30%)"
  legal-landing: "rgb(35% 76% 66% / 45%)"
  blocked-landing: "rgb(92% 36% 25% / 25%)"
  map-current: "rgb(99% 76% 32%)"
  map-restored: "rgb(49% 78% 66%)"
  path-open: "rgb(88% 60% 22%)"
  path-future: "rgb(51% 60% 53%)"
  setting-paper: "rgb(97% 92% 78%)"
  switch-on: "rgb(32% 68% 61%)"
  switch-off: "rgb(55% 58% 49%)"
  triangle-yellow: "#FFC856"
  circle-mint: "#59CAB1"
  square-coral: "#EB8B7E"
  diamond-lilac: "#B496D9"
  star-sky: "#64B9E1"
  hexagon-apricot: "#F3A45D"
  tile-ivory: "#FFF0CC"
  warm-graphite: "#354C50"
  hardware-gold: "#EFB953"
  receiver-ivory: "#FBE2A3"
  platform-mint: "#53AE9E"
  device-coral: "#E38370"
  contact-shadow: "rgb(11% 23% 25% / 28%)"
  wood-fallback: "rgb(65% 35% 13%)"
  hemp-rope: "rgb(73% 51% 26%)"
  wood-chip-dark: "rgb(64% 32% 12%)"
  wood-chip-light: "rgb(89% 60% 28%)"
typography:
  navigation-heading:
    fontFamily: "Balsamiq Sans"
    fontSize: "29px"
    fontWeight: 700
  navigation-action:
    fontFamily: "Balsamiq Sans"
    fontSize: "24px"
    fontWeight: 700
  navigation-detail:
    fontFamily: "Balsamiq Sans"
    fontSize: "17px"
    fontWeight: 400
  menu-body:
    fontFamily: "Balsamiq Sans"
    fontSize: "20px"
    fontWeight: 400
  menu-action:
    fontFamily: "Balsamiq Sans"
    fontSize: "22px"
    fontWeight: 700
  menu-compact-action:
    fontFamily: "Balsamiq Sans"
    fontSize: "20px"
    fontWeight: 700
  menu-title:
    fontFamily: "Balsamiq Sans"
    fontSize: "30px"
    fontWeight: 700
  region-speaker:
    fontFamily: "Balsamiq Sans"
    fontSize: "27px"
    fontWeight: 700
  level-title:
    fontFamily: "Balsamiq Sans"
    fontSize: "26px"
    fontWeight: 700
  journey-task:
    fontFamily: "Balsamiq Sans"
    fontSize: "21px"
    fontWeight: 700
  dialogue:
    fontFamily: "Balsamiq Sans"
    fontSize: "22px"
    fontWeight: 400
  body:
    fontFamily: "Balsamiq Sans"
    fontSize: "18px"
    fontWeight: 400
  button:
    fontFamily: "Balsamiq Sans"
    fontSize: "18px"
    fontWeight: 700
  focus:
    fontFamily: "Balsamiq Sans"
    fontSize: "20px"
    fontWeight: 400
  secondary:
    fontFamily: "Balsamiq Sans"
    fontSize: "16px"
    fontWeight: 400
  receiver-label:
    fontFamily: "Balsamiq Sans"
    fontSize: "16px"
    fontWeight: 700
  device:
    fontFamily: "Balsamiq Sans"
    fontSize: "14px"
    fontWeight: 400
  token-glyph:
    fontFamily: "Balsamiq Sans"
    fontSize: "28px"
    fontWeight: 700
  target-glyph:
    fontFamily: "Balsamiq Sans"
    fontSize: "24px"
    fontWeight: 700
rounded:
  panel-source: "13px"
spacing:
  row-gap: "3px"
  label-inline: "3px"
  label-block: "2px"
  control-band-gap: "8px"
  setting-label-inset: "16px"
components:
  navigation-primary:
    textColor: "{colors.menu-ink}"
    typography: "{typography.navigation-action}"
    padding: "1px 7px 1px 4px"
    width: "84%"
  navigation-secondary:
    textColor: "{colors.navigation-cream}"
    typography: "{typography.navigation-action}"
    padding: "1px 7px 1px 4px"
    width: "84%"
  level-plaque:
    textColor: "{colors.menu-ink}"
    padding: "0px"
  map-island-target:
    width: "76px"
    height: "72px"
  map-portrait-target:
    width: "64px"
    height: "80px"
  button-secondary:
    backgroundColor: "{colors.ui-paper}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    padding: "5px 10px"
    height: "48px"
  button-primary:
    backgroundColor: "{colors.drop-gold}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    padding: "2px 3px"
    height: "48px"
  button-menu:
    backgroundColor: "{colors.menu-sage}"
    textColor: "{colors.menu-ink}"
    typography: "{typography.menu-action}"
    rounded: "{rounded.panel-source}"
    padding: "3px 8px"
    height: "52px"
  language-carousel:
    backgroundColor: "{colors.menu-sage}"
    textColor: "{colors.menu-ink}"
    typography: "{typography.menu-action}"
    height: "64px"
  button-direction:
    backgroundColor: "{colors.ui-paper}"
    textColor: "{colors.ui-ink}"
    rounded: "{rounded.panel-source}"
    height: "48px"
  map-stop:
    backgroundColor: "{colors.ui-paper}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    width: "62px"
    height: "60px"
  map-stop-current:
    backgroundColor: "{colors.map-current}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    width: "62px"
    height: "60px"
  map-stop-restored:
    backgroundColor: "{colors.map-restored}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    width: "62px"
    height: "60px"
  paper-panel:
    backgroundColor: "{colors.ui-paper}"
    textColor: "{colors.ui-ink}"
    rounded: "{rounded.panel-source}"
  setting:
    backgroundColor: "{colors.setting-paper}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    height: "60px"
  letter-token:
    backgroundColor: "{colors.tile-ivory}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.token-glyph}"
  shape-token:
    backgroundColor: "{colors.triangle-yellow}"
  platform:
    backgroundColor: "{colors.wood-fallback}"
    textColor: "{colors.ui-ink}"
  receiver:
    backgroundColor: "{colors.receiver-ivory}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.receiver-label}"
  target-zoom:
    backgroundColor: "{colors.ui-paper}"
    textColor: "{colors.ui-ink}"
---

# Design System: CASCADE

## Overview

**Creative North Star: "The Sky Wind Workshop"**

A small keeper carries light through floating islands and wind-powered toy machines. Painted honey wood, braided hemp, warm brass, ivory letter tiles and six colored silhouettes make the puzzle tangible. Mika's patched overalls, saffron scarf and lantern connect the mechanism to a warm, imperfect character. Painted regional skies and isometric routes give each delivery a place and a neighbour.

Real beveled meshes, an orthographic view, live lettering and separate illustrated scenery share the same bright storybook world. The hand touches the row directly; quiet controls leave room for the machine and its complete gold destination. Paper panels support short dialogue and navigation. This is the author-delegated, code-led extension of the established world; no approved composite image exists for pixel-perfect fidelity claims.

This record merges the implemented P14 journey revision into the P12/P13 system. Sources are `CascadeGame.cs`, `.Flow.cs`, `.Journey.cs`, `.Touch.cs`, `TokenSlot.cs`, `ToyMachineView.cs`, `LayeredWorld.cs`, `JourneyStory.cs`, `ToyAssetBuilder.cs` and `MotionProfile.asset`. `Docs/JOURNEY_SURFACE_BRIEF.md` owns the surface contract; `Docs/P14_FLOW.md` owns implementation scope and validation limits. Native captures and review live in `Reports/Journey`. `Docs/JOURNEY_ASSET_PROVENANCE.md` records the 12 original regional/character rasters; earlier mesh, font and audio provenance remains in the P12/P13 documents. The sidecar records native components and motion, with no fabricated HTML/CSS counterparts. Its synthesized tonal ramps are preview aids, not shipped palette additions.

P16 extends these existing controls with durable local progress and recovery in `CascadeGame.Save.cs`, `Persistence/` and `Application/AttemptRecord.cs`. `Docs/P16_SAVE.md` owns the save contract; `Reports/P16` holds native recovery fixtures and test evidence. This extension adds no visual tokens or raster assets. Editor evidence does not verify physical-device power loss, OS kill, full player builds, native-language approval or human enjoyment.

P17 extends the same paper, mint and brass surfaces with a travel bag, journal,
requests, workshop, keepsake collection, earned parcel and personal routes in
`CascadeGame.Rewards.cs`. `JourneyRewards.cs`, `PlayerProfile.cs` and
`CascadeGame.Save.cs` supply the local facts behind those views;
`Docs/P17_META.md` owns their progression contract. Nine new transparent
illustrations keep the incumbent painted material character; exact prompts and
alpha/margin limitations are in `Docs/META_ASSET_PROVENANCE.md`. This ordinary
extension adds no shared visual tokens. `Reports/P17/README.md` records 333/333
EditMode, 46/47 full PlayMode and a corrected focused 1/1 navigation result,
plus 44 inspected native frames, including three requested carousel views. The
independent verdict scores both listed findings resolved, disposition ship at P17 scope. The approved wooden workshop extension now follows the contracts in
`Docs/VISUAL_MOTION_REWORK.md` and `Docs/WOODLAND_SURFACE_BRIEF.md`.

P18 makes two narrow clarity corrections within this world: the stable `Hint`
key displays `Помощь`/`Поміч` in RU/UK, and map paging reuses the authored
direction chevron. `Docs/P18_INTEGRATION.md` owns this phase's contract;
`Reports/P18/impeccable-review.md` records ship at this correction scope. Three
final 320 × 693 native Editor frames confirm the corrected RU caption, RU guide
and UK map against the existing system. The 3/3 focused tests and matching
157-puzzle traces establish their stated behavior boundaries; the roughly
100 ms whole-Editor frame observation is not a device performance pass.
Physical devices, final player build, child comprehension and human enjoyment
remain unverified. This historical phase added no visual tokens or artwork. The current wood,
motion and world extension is recorded below.

The Woodland checkpoint uses `ToyMachineView.Woodland.cs`, `PuzzleEventPlayer.cs`,
`CascadeGame.DeviceDemo.cs`, `WoodBoardGraphic.cs`, `WorldMotion.cs`,
`WorldWeatherGraphic.cs`, `LayeredWorld.cs` and the expanded `JourneyStory.cs`.
Eight regional art kits now cover 160 stops over 33 pages; three original kits
are retained and five original Base/Map pairs plus WoodBoard are documented in
`Docs/WOODLAND_ASSET_PROVENANCE.md`. Source-piece trajectories and board springs
are presentation effects over the unchanged deterministic rule core. Native
stills and sampled motion live in `Reports/Woodland`; the 5/5 EditMode and
16/16 focused PlayMode checkpoint predates the final guide/CanvasRenderer fix.
The final renderer correction passed 4/4 affected UI regressions in
`Reports/WoodlandFix/playmode.xml` at 11:29:09–11:29:16 UTC; fresh native guide,
map and bag captures show wood/weather. Review/build were pending at that
historical checkpoint; the Craft results below supersede those statuses. Physical iOS/Android, haptics, mobile performance,
non-reader understanding, native editorial, live Ads/IAP and human enjoyment
remain NOT RUN. The service boundary is disabled mock code; online remains
design-only. These Woodland results are historical checkpoints.

The Craft extension follows `Docs/CRAFT_REWORK.md`: `CascadeGame.Craft.cs`,
`CascadeGame.Rewards.cs`, `ToyMachineView.Ropes.cs`, `ToyMachineView.Woodland.cs`,
`PuzzleEventPlayer.cs` and `LayeredWorld.cs` implement object shelves, simultaneous
selection, continuous ropes, visual weight and composed scenery. Six optional
R01–R06 Ribbon Reach trials now use the shared deterministic resolver under
rules2.1-longreach; C01–C160 retain rules2.0 and their content hashes. The original
WorkshopRoom and WindGardenPanorama generated rasters and metadata are recorded
in `Docs/AssetPrompts/Craft/provenance.json`, with exact prompts alongside them.
The palette, font, symbols and paper/wood/brass identity are preserved.
`Reports/Craft/design-documentation.md` records source evidence and remaining
validation limits. `Reports/Craft/impeccable-verdict.md` records ship for seven
scored findings, including map edges and physical grounding; it does not approve
the entire product. `Reports/CraftFinal/playmode.xml` passed three affected
regressions, a subset of the earlier seven. The Mac standalone build succeeded
with zero errors and 346 warnings. A locked-Mac C160 player sample at 1440 × 900
on M4 Pro measured median 16.703 ms / p95 17.520 ms over 240 frames after 90
warm-up frames. The invalid GC counter supplies no allocation evidence. This
short sample is neither verified foreground nor mobile performance. Native
player visual confirmation is blocked by the locked Mac; physical devices,
human enjoyment and release readiness remain unverified.

The device/map extension follows `Docs/DEVICES_REWORK.md` and the implemented
`CascadeGame.Devices.cs`, `CascadeGame.DeviceDemo.cs`, `CascadeGame.Craft.cs`,
`CascadeGame.Journey.cs` and `MapAvailability.shader`. Six original tool rasters
retain generated pixels and provenance in `Docs/AssetPrompts/Devices`. Mounted
matching-piece sockets, explicit counters and an eleven-part guide replace the
coded beads and floating contact faces. The map uses the island artwork itself
for availability and current-location cues. Palette, typography and SymbolId
identity stay intact. Current native captures and validation status belong to
`Reports/Devices` and `Reports/DevicesRegression`; the pending final confirmation
must not inherit the earlier Craft review or build verdict.

The navigation extension follows `Docs/NAVIGATION_REWORK.md` and the implemented
`CascadeGame.Navigation.cs`, `.Flow.cs`, `.Journey.cs`, `MapParallax.cs`,
`MenuSwipe.cs`, `LayeredWorld.cs`, `SoftSceneryGraphic.cs` and
`MapAvailability.shader`. It keeps WorkshopPortrait, Balsamiq and the painted
wood/brass/enamel world while replacing the former Home-as-map/menu folio flow.
Original MenuIcons, OpeningComic and MapSky rasters support the four-action
Home, separate play choice, three-scene opening and continuous map sky. Source
colors remain normative; the cream navigation caption is live TMP ink, not a
measured enamel texture color. `Reports/Navigation/impeccable-verdict.md` records
SHIP for the two scored sky/target corrections. Six distinct scoped PlayMode
cases have passing evidence across the initial 5/6 and corrected swipe 1/1;
the expanded map 1/1 repeats an existing case. Localization passed 2/2. Sixteen
native Editor PNGs include four corrected map poses; the +6/+4 shifted pose is
explicitly staged. Safe areas are simulated. These results do not establish a
full-suite run, physical touch/gyro, mobile FPS, accessibility, native editorial
approval, human comprehension/enjoyment or release readiness. Current build
status belongs to the Navigation report.

The touch/store extension follows `Docs/TOUCH_AND_STORES.md`,
`CascadeGame.Touch.cs`, `ToyMachineView.TouchFeedback.cs`, `ToyRowPhysics.cs`,
`PhoneShakeFeedback.cs`, `CascadeGame.Store.cs` and `CascadeGame.Navigation.cs`.
It extends the same wood, enamel and painted world with actual-body contours,
free finger tracking, cosmetic physical landings and a disabled native purchase
view. Existing MenuBackground artwork and source assets remain; no new raster
generation or shared visual token change was needed. `Reports/TouchRevision`
holds ten final actual Unity Editor captures. The footer and intentional-y
review corrections are implemented; the expanded axes check passed 1/1 in
`Reports/TouchRevisionReviewFinal`. The initial review run failed on Editor
Device Simulator setup/teardown. The fresh verdict records SHIP for resolved
F01/F02 only, without whole-product certification. These stills and tests do not
prove hardware touch, sensors, timing, haptics,
OS accessibility, device performance, native builds or human enjoyment.

The earned-coin shop is an ordinary extension of this workshop in
`CascadeGame.Shop.cs`, backed by `ShopCatalog.cs`, `TravelShop.cs` and the existing
attempt/save transaction. `Docs/SHOP_AND_UNDO.md` owns its rule note and direction
contract. One original transparent TravelTools atlas supplies the scroll, spool,
return hourglass and coin pouch; exact prompt and embedded provenance are recorded
in `Docs/AssetPrompts/ShopTools.md`. It reuses the palette, Balsamiq lettering,
cloth reading plates, object shelf and enamel actions without new shared tokens.
`Reports/Shop/README.md` records 13/13 focused EditMode checks and Editor runtime
callback assertions. Native 320/390/768 captures use simulated safe insets. The
fresh finish review records ship for the scored UI fixes: readable benefit/count/
price, a separate confirmation, quiet unavailable actions and the clear EN guide
ribbon. Earlier scoped verdicts remain historical. This shop verdict does not
approve the whole game, and no current player build, live-store, physical-device,
performance or human-enjoyment check follows from the Editor evidence.

**Key Characteristics:**
- Direct touch lifts a piece and its live glyph together; the receiver stays visible.
- Six stable silhouettes and colors retain their identity across eight regions.
- Wood grain, hemp suspension, brass joints and soft contact shadows make the machine tangible.
- Mika, Pip and Rue connect short optional dialogue to a concrete destination.
- Illustrated routes distinguish the current path from future-region browsing.
- Separate clouds, rooted plants, a glider and regional weather give the sky restrained life. The glider follows a continuous depth orbit with perspective scale and banked turns. Wind Garden's detached islands use a transparent atlas over `WindGardenSky`; independent slow float phases and a height-bounded size keep the scene calm on phones and wide screens.
- The painted workshop surrounds illustrated enamel actions; Home, play choice and travel each have a clear role.

## Colors

Warm paper and brass anchor a mint, coral, lilac, yellow, sky and apricot toy palette. Native UI colors and lit material inputs retain their source notation in the frontmatter; rendered appearance also depends on lighting and color space.

### Primary

- **Hardware Gold** marks receiving rims, sockets and mechanical joints. Piece selection uses the contour of its actual body.
- **Drop Gold** marks the committed Descend/Deliver action and primary travel/dialogue actions; the historical token name remains stable.
- **Painted honey wood** is the native WoodBoard texture, framed to its solid body band; its grain is not represented by a flat color token. **Wood Fallback** is used only if that texture is unavailable. The platform component's flat fill reference records that fallback, not a measured average of the painted plank.
- **Hemp Rope** colors suspension and knots. **Wood Chip Dark** and **Wood Chip Light** alternate in short collapse debris.
- **Platform Mint** retains its existing ID for legacy assets and enamel details; it no longer describes the current plank surface. Hardware and symbol colors remain independent of regional scenery.

### Secondary

- **Triangle Yellow, Circle Mint, Square Coral, Diamond Lilac, Star Sky and Hexagon Apricot** preserve the six shape identities in the row, contacts, destination and magnifier.
- **Device Coral** retains fragile-edge feedback. New bomb, repair, key, stamp, ribbon and hourglass rasters carry their own painted colors; lilac connecting cords disclose paired edges.
- **Legal Landing**, **Blocked Landing** and **Preview Gold** retain their historical slot-wash token identities. Current direct-touch feedback uses a warm ivory or blocked coral contour of the actual piece, with a slight legal-target lift; slot images stay transparent and no square selection or target overlay is shown.
- **Map Current**, **Map Restored**, **Path Open** and **Path Future** remain historical tokens. The current map conveys availability through colored/desaturated island artwork and a localized arrow with Mika; stop fills and connecting pebbles are absent. The retained `map-stop` frontmatter variants describe the earlier surface and must not be used to recreate its visible cards.

### Neutral

- **UI Ink** carries live lettering and authored control marks. **High Contrast Ink** is the implemented optional text override.
- **UI Paper** carries controls, navigation panels, dialogue and magnifier; **Setting Paper** distinguishes setting rows.
- **Tile Ivory** carries glyph bodies, highlights and small machine insets; **Receiver Ivory** carries the receiving inset and guides.
- **Warm Graphite** carries damage seams, timers, rollers and hinges. **Contact Shadow** grounds the toy locally.
- **Switch On** and **Switch Off** accompany knob position and the labelled setting. Button hover, pressed and disabled tokens are Unity ColorBlock multipliers over each base fill.
- **Navigation Cream** carries live captions and details on teal enamel navigation actions. Amber primary navigation keeps Menu Ink; button artwork supplies its painted surface.

**The Shared Identity Rule.** Use the same silhouette and material for a SymbolId in the row, target, device contact and magnifier; glyph symbols use live text on ivory bodies.

## Typography

Balsamiq Sans Regular and Bold provide the rounded hand-lettered voice. Text remains live TMP; the static atlas assets derive from the independently sourced font files and SIL OFL license under `CascadeUnity/Assets/_Cascade/Art/Fonts/`. No native fallback stack, custom tracking or line-height scale is established.

### Hierarchy

- **Menu title** introduces pause, guide and settings. **Region/speaker** names a map region or dialogue speaker.
- **Navigation heading** names the separate play, shelf, comic and shop screens. Its 29-unit maximum fits down to 21; **navigation action** fits from 24 down to 18, with a 1.10 large-text maximum multiplier. **Navigation detail** fits from 17 down to 14. These local auto-size ranges leave the incumbent menu roles intact.
- **Shop copy** remains live Balsamiq: selected-object titles fit from 26 down to 19 compensated units, while benefits, persistent count and price have an 18-rendered-pixel floor, raised to 20 for Large Text. A purchase confirmation has its own compact 18–20 range and does not replace the benefit or stock/price line. Counter actions use their local 16–22 compensated range; these local values do not redefine other menu roles.
- **Level title** is the lesson header; **journey task** is the shorter destination line and map task. The current Home CASCADE logo is a separate Bold 43-unit mark.
- **Dialogue** uses larger Regular text with short sentences; **body** handles state and help; **button** handles actions. Map counts use **secondary** text; Skip story uses a Bold label at that size.
- **Focus** names the symbol in optional precision controls. **Receiver label** belongs immediately above the cradle.
- **Token glyph** and **target glyph** use Bold and stay centered on their mesh faces. Device glyphs use Bold at the same compensated size as **device** explanations.

Frontmatter `px` type values describe Unity reference-canvas units, not measured physical-device size. Auto sizing is off and ordinary labels wrap. Device and receiver text is compensated by `unit = 1 / min(1, canvasScale)`; device text therefore targets 14 rendered pixels at the supplied small reference scale. Larger text applies a 1.12 multiplier to eligible UI copy, excluding symbol faces; explicit layout/font overrides still exist, so it is not a promise that every label grows. Full localized editorial and glyph review remains P15 work.

**The Live Face Rule.** Move and scale a letter with its mesh face through selection, holding, exchange and landing; never bake gameplay letters into scenery.

## Layout

The camera-space uGUI canvas uses a 390 × 844 reference resolution with height scaling and `Screen.safeArea`. Wide Editor views cap content at 430 reference units while the scenery fills the viewport. Mobile PlayerSettings enforce portrait; 844 × 390 landscape is a rejected diagnostic. Desktop 1440 × 900 remains the wide baseline; short desktop windows are unsupported. The machine stays in a portrait composition over a continuous viewport sky surface. WindGardenSky and four independently moving FloatingGardenIslands now supply the Wind Gardens scene; the original WindGardenPanorama remains preserved and WorkshopRoom supplies the bag/workshop interior. Other regions keep their existing art. Navigation menus retain WorkshopPortrait on phones and WorkshopRoom in wide views. Navigation maps use the authored MapSky plate at viewport cover, with the central island artwork over it; the earlier Craft map feathering is historical evidence.

### Direct play

A drawn Hint lamp, a centred destination plaque and Pause occupy a 48-unit compensated header, 12 units below the safe top. The header follows the actual rail width and remains inside both rope anchors with 12-unit inner clearances; the plaque is 40 units high with centred 12–17-unit type. Contextual cream instructions wait 3.2 seconds and have a dismiss cross; each new message waits, and a dismissed message stays hidden for the current level. Hidden instructions cannot intercept input. The bottom has two centred actions, Undo and Descend/Deliver, in a 52-unit compensated band 8 units above the safe bottom. Retry, map, settings and device help are reached through pause or result flow. The decorative yellow coach ring and player-facing Shortest route label are absent; solver preparation remains unchanged. The tutorial hand renders above the pieces with foreground sorting and an always-visible depth test.

The complete receiver is 100 reference units high. Its bottom is normally 82 compensated units above the safe bottom, moving to 184 for enabled precision controls or win results and 232 for failure comparison. Floors occupy the remaining region above the receiver clearance and below the feedback area. Rail width is `min(91% of content width, symbolCount × 76 + 24)`; receiver width is `min(90% of content width, symbolCount × 68 + 40)`. Three-piece rows are expanded but retain shorter physical rails. Cells share a 3-unit gap.

Precision input is explicitly enabled in Comfort and is off by default, including on seven/eight-symbol rows. Its two rows use a 48/8/48 rhythm. Long rows instead retain stable hit regions and a lifted, enlarged held piece; no claim is made that eight narrow cells each meet a 48-pixel target. The receiver opens a full safe-area magnifier with four columns and up to two rows.

### Home and play choice

Home shows CASCADE, Mika on a small wooden bench and four separate illustrated
actions: Start game, Shop, Settings and Remove ads. The complete painted
workshop stays visible around them. The next screen offers Story, Shapes, Words
and Mixed, each with one short live explanation; Opening story replays the
comic. Navigation actions occupy 84% of safe content width, with the illustration
beside left-aligned lettering. Amber enamel identifies the primary action;
teal enamel carries cream alternatives. Reading copy sits on small ivory plates
rather than a page-sized folio.

Shapes, Words and Mixed show up to nine numbered enamel plaques on a swipeable
shelf, a live page count and one Continue/Play level action. Their concrete-mode
filter uses the existing catalog. A swipe cancels the child plaque's click and
changes only the displayed page. Map and shelf use horizontal swipes and desktop
wheel input, with clamped first/last pages; Back returns to the play choice.

### Shop and purchase state

The current shop is a small counter inside the painted workshop. Back/title and
a coin pouch head the view; Tools/Keepsakes tabs sit above three or four visible
objects on one shelf. Selection gives the object an amber rim and enlarges its
original alpha illustration above a cloth explanation plate. Benefit, stock or
ownership, and full price remain visible when the action changes to an
insufficient-balance or capacity explanation. A separate short confirmation
appears after purchase. The lower action buys one known item or equips an owned
keepsake without charging again; Tool bag and No ads are secondary destinations.

Amber identifies an executable purchase, equip or use action. An unavailable
action uses the quieter sage panel with readable Menu Ink, rather than retaining
the amber face. The tool bag uses the same shelf, selection and explanation,
with one Use action and localized reasons for an empty bag, exhausted stage use,
remaining free Undo, absence of a current-board move or broken-board checkpoint.
Pause and the loss flow open that bag directly. Using an item sends the selected
original object into the existing arrival/pulse/status feedback. The scroll's
live hand renders in front of pieces and follows solver-proved steps; neither
the feedback nor its completion spends an item or changes puzzle state.

The new counter's smallest recorded controls measure 50.80 rendered pixels in
the 320×693 Editor sample. Those bounds and simulated safe insets are local
evidence, not hardware pt/dp or accessibility certification. Existing workshop,
enamel, panel and keepsake artwork are retained; the original TravelTools atlas
adds only the four needed objects. The separate No ads purchase/restore view
still reflects the unconfigured store. Its 64.14/52.66-pixel return-to-play bounds
at 390/320 belong to the historical TouchRevision check, which remains evidence
for that view rather than a current shop verdict.

### Journey and dialogue

The map uses a tall illustrated five-stop route between a compact ivory heading
with Goal: Great Beacon and a lower task/action panel. An invisible island target
is 76 × 72 reference units. The source island image remains stretched into its
normalized map rectangle; viewport cover applies to MapSky, not to that island
image, and no pixel-perfect raster-registration guarantee follows. The current
map reuses each of eight regional illustrations over 33 pages: usually five
stops, with four on the final page of each new 24-stop region. Stop placement
follows the painted plazas using region-specific anchors. Small Mika stands
with her feet at the actual next island; a separate invisible 64 × 80 reference
target opens its conversation, and a localized You are here arrow marks that
same saved progress. None of these cues appear on a locked future preview. The
future footer gives distance and a return action. The compact corrected render
measures Mika's target at 52.5 × 65.7 pixels, with Story and Travel respectively
53.7 and 56.9 pixels high; these are Editor bounds, not hardware pt/dp promises.

Dialogue places a large transparent character above/behind a low paper frame. A thin brass line, Bold speaker name, short Regular sentence and separate Continue/Skip story actions provide hierarchy. Story entry explicitly selects the character's region. Dialogue blocks puzzle input.

### Travel bag and earned objects

The existing travel bag places Mika and a parcel on the upper bench, with
Keepsakes/Workshop and My journal/Power-ups arranged on two object shelves.
The journal unlocks after three restored lights; Power-ups opens six optional
pickup lessons with a live animated example, lesson selectors and a Try this
trail action. A separate Ribbon reach action retains access to the six R trials. Settings and Back to map share
the lower band. Collection and workshop place four selectable thumbnails on
one shelf above a larger selected alpha illustration, short live progress
copy and a clear lower action. Requests retain separate previous/next controls. The journal
pairs Mika with restored-light, forgiving visit and next-parcel summaries.
Progress lives on these optional surfaces rather than adding meters to the
puzzle HUD. Item artwork preserves aspect ratio inside its padded native UI
rectangle; the narrow source margins, especially Beacon Model's vertical
margins, must not be treated as permission to clip painted edges.

**The Complete Machine Rule.** Keep the persistent row, remaining floors and complete receiver order visible together; changing row length must preserve clear physical rail proportions.

**The Honest Path Rule.** Distinguish the browsed stop from the reached stop; future art must never imply unlocked or saved progress.

## Elevation & Depth

Beveled meshes, satin URP materials, a warm directional key and analytic contact shadows create depth. The warm key has intensity 1.05 and color (1, 0.96, 0.88); the sky fill has intensity 0.35 and color (0.64, 0.85, 1). Flat ambient light uses (0.69, 0.79, 0.80). Unity cast shadows are disabled for this view. The rounded extrusion uses contour rings at scale factors `0.88, 0.985, 1, 0.97, 0.89`. Shape plastics use smoothness 0.28 and metallic 0.02; other materials keep authored satin values.

Soft contact quads use `(1 − smoothstep(0.25, 1, radius))²` alpha, tinted by Contact Shadow. Each token, platform and receiver fits its own quad. This is a native shader effect, not a CSS shadow. Playing pieces seat their mesh bottom at 0.365 of the padded floor rectangle, matching the visible painted wood. The contact shadow aligns just below this surface. Selection adds a slight world-space lift and a 1.04 size multiplier, with a contour following the actual body and its live glyph. The legal drag destination also lifts slightly. Square slot frames and target overlays are absent. Final sampled landing frames confirm contact, while in-flight frames retain separation.

### Motion and living scenery

A held piece acknowledges immediately with its glyph, then lifts to 1.6 times the selection lift. Its face aims for 58 rendered pixels with a held-size multiplier clamped to 1.08–1.30; deliberate drag has a minimum multiplier of 1.16. The preserved grab offset and actual pointer x/y control travel freely, with no neighbour-distance or upward movement clamp. Input cells remain stationary and resolver legality alone decides the exchange. Free/cancelled releases return in 180 ms; pause or focus loss settles the presentation without spending a move. Reduced Motion suppresses decorative lift, enlargement and tilt while preserving intentional x/y tracking.

Native scripted exchanges last at least 320 ms for taps and 360 ms for drags. The placed source begins from its actual released position, scale and angle; the partner returns on an upward arc. Normal wind and exchanges remain guided motion. The earlier profile checkpoint recorded contact, bomb and fracture steps at 180, 240 and 130 ms. Falls and shakes use eight pooled real Rigidbody box proxies in the existing isolated cosmetic PhysX scene: the original visible pieces spin, bounce and collide briefly before guided interpolation restores the exact committed seats. These are approximate box contacts, not exact candy-mesh colliders. A landing lasts at least 190 ms, with one short damped upward settle, at most 8% squash and bounded alternating rotation; local rail recoil remains 0.025 world units. The loaded wood strip bends toward 0.045 world units of apparent weight through bounded decorative springs; holding a piece removes its apparent weight. Two continuous hemp ropes run from above the viewport through each rail attachment to the receiver and respond to the same decorative sway. Delivery holds for 340 ms. Reduced Motion substitutes 55 ms state holds and at most two stationary dust fades per burst. Selection focus uses 100 ms or an immediate reduced-motion state. Screen shake is opt-in and off by default; Reduced Motion suppresses it. Dust and audio tails never determine command completion.

Phone acceleration passes through a bounded high-pass response that ignores the initial zero sample and prioritizes deliberate gestures. Its impulse affects decorative board springs, constrained rope chains and the pooled pieces; it never touches Domain, Solver, contacts, row order or move cost. Hardware sensor thresholds and feel remain unverified. The ropes retain their constrained point-chain simulation, not full realistic rope physics.

Eight distinct far landscapes cover the viewport. Shared transparent clouds move at three speeds, lower-edge plants sway about their stems, and rain, snow or pollen occupy separate scenery layers by region. The incidental lower-right windwheel is hidden. Regional tints adapt cloud/plant layers. The whole rotated glider is bounded to the empty gap between feedback and the top floor; it hides during a held-token gesture and if no gap remains. All ambient motion is view-only. Explicit pause and suspension stop phase progression; Reduced Motion displays phase zero. Map and dialogue can keep their decorative clock running while puzzle input is paused. Quiet generated wind/rain beds follow Sound and explicit pause/suspension preferences; physical listening has not been verified.

Map islands, hit targets, the current arrow and Mika share one translation.
Gravity input is calibrated against its first sample where available; desktop
mouse position supplies the fallback. A five-unit tilt contribution combines
with ambient drift of at most 1.5 horizontal and 1.1 vertical reference units,
smoothed with `1 − exp(−4 × unscaledDeltaTime)`. This bounds the target to 6.5
horizontal and 6.1 vertical units. Separate map clouds drift at another depth.
Reduced Motion and suspension set the shared offset to zero. The viewport sky
remains continuous around the moved islands, with gameplay scenery layers
hidden while MapSky is active. Motion and sensor lifecycle cannot advance a
puzzle or alter travel availability; physical gyro quality is unverified.

**The Clear Flight Rule.** Decorative motion must fit wholly outside the row, receiver and feedback; a lower render layer alone is not sufficient separation.

## Shapes

Six original soft silhouettes form the row: rounded triangle, circle, square, diamond, star and hexagon. Letter tiles reuse the rounded square. The square/tile contour is one mesh unit wide with a 0.18-unit corner radius; the 6 × 1 beam has a 0.3-unit radius. Disks form mounts, caps and hinges. Mesh coordinates are not UI pixels.

Panels and buttons reuse the 64 × 64 nine-slice raster. The frontmatter `panel-source` token describes its 13-source-pixel corner construction with 22-pixel slice borders, not a universal runtime radius. Pause bars and precision chevrons use authored geometry. Mika, Pip, Rue, clouds, plants and glider use original raster cutouts with native alpha; preserve painted edges and transparent gaps rather than replacing them with vector approximations or runtime masks.

Navigation icons use an original eight-picture atlas; the opening uses three
original square scenes from its own atlas. Both are sliced at runtime without
altering their source pixels. MapSky is a separate full painted plate; existing
island and WorkshopPortrait art retain their asset and meta identities.

## Components

### Controls and settings

Warm paper controls carry dark Bold labels and Unity multiply tints for hover, pressed and disabled states. Descend changes to Deliver when the final-floor row matches; the Hint lamp turns warm gold for a valid offer and retains its request/apply action. A separate custom keyboard focus treatment is not established. Setting rows use a readable label, an on/off track and positional knob; sound, motion, effects, contrast, larger text, precision, tutorial and dead-end preferences persist in the P16 local profile, alongside language and preferred mode.

### Persistent token and receiver

Pressing a shape or letter gives actual-body contour feedback, a slight lift and a larger held face. Legal destination contour/lift and the feedback line explain command cost; no rectangular slot or target overlay is used. The same body and embossed glyph follow intentional x/y in normal and Reduced Motion. Release commits one atomic exchange through the shared resolver. Ordinary play exchanges adjacent pieces; the six R trials and a collected ribbon in the B lessons additionally permit one charged endpoint swap across one or two intervening pieces. All crossed edges must be open, linked-pair corridors reject Reach, and crossed fragile/bridge rules still apply. Contacts use the final row. The charge and state are undoable; Retry restores the initial charge. Charge-aware live guidance distinguishes available and spent ribbon states. Tap-then-tap remains available; a long swipe does not chain exchanges. The gold rim and ivory inset retain the complete ordered destination. Magnifier faces derive from the same shapes and live glyphs. Failure shows the goal and delivered row separately; success seats the row before result actions appear.

### Platforms and devices

Compact painted wood strips retain the original grain and continuous hemp suspension. Up to three graphite seams show wear, while localized Wood · moves: N copy reports remaining durability. Strength pips, fuse beads and isolated entry arrows are absent from the playable rails. Contact tools use the original transparent Bomb, RepairKit, BrassKey, Stamp, RibbonSpool and Hourglass art, suspended from a wooden socket by two straps. Each attached tool has a separate matching-piece pocket using the shared SymbolId silhouette and live glyph. Device hit areas are 88 × 55 reference units. Tapping one opens its guide without spending a move. A lilac cord connects linked edges; a landing-movement label opens the relevant explanation. The wood remains unlit, with the existing satin shape and brass materials.

A bomb reports remaining moves, Paused: N, Safe or Burst from committed state. Collected tools become subdued and show Collected. Ribbon and hourglass labels disclose Far swap and Pause bomb before collection. An hourglass pauses bomb ticks while the wood continues wearing; it does not imply stopped animation or a real-time countdown. Collapse still emits bounded wood chips, and source pieces travel to their exact destination before settling.

The eleven device-guide demonstrations read precomputed transitions from the same resolver without owning the playable attempt. Entry shows landing and row rotation; linked pairs move together, fragile crossings lose strength, a bridge folds, a key opens its gate, repair mends, bomb defuse/blast change the board, and pickup examples show the corresponding tool outcome. The guide retains roller-mounted direction marks within the entry demonstration; these are distinct from the removed isolated arrows on playable floors. Optional captions stay live in EN/RU/UK. Reduced Motion presents stationary translucent before silhouettes and the settled after state. The workshop Power-ups menu reuses the animated pickup examples. Non-reader understanding remains a human-test question.

### Map route

The isometric islands themselves express travel. Invisible plaza-aligned buttons
retain stop identities; they have no numbered captions, filled cards, shadows
or added path pebbles. Locked buttons reject travel. The dedicated map material
desaturates scenery assigned to locked destinations while retaining available
color. Its nearest-island/color mask is shader-based, not separately authored
island cutouts. Painted blue sky is substituted with the same viewport MapSky
plate at screen coordinates, leaving island contours and original cloud
brushwork in the authored map. Small Mika, her independent conversation target
and the localized You are here arrow share the island world's motion.

Unlocked stops come from the 160-node catalog and durable local first-clear
receipts, deduplicated across modes and locales. The amber action says Go to
level N or Continue where available; future pages explain remaining stops and
offer Back to my path. Talk to the current friend opens its dialogue. Story
chooses the existing campaign bindings automatically; Shapes/Words/Mixed use
their separate filtered shelf. Eight regional kits span 33 map pages. Clouds
and rain/snow retain separate live UI layers. The earlier saved stamp-seal
overlay is absent; saved stamp-goal records and the ten late optional detours
remain part of progression.

Map pages turn through the shared scene's swipe/wheel handler; no Earlier/Later
page controls are shown. Its header Back chevron returns to the separate play
choice. The native `DirectionButton`/`ChevronGraphic` remains the incumbent
primitive for Back and for controls on other surfaces.

### Dialogue and results

Story's opening is a three-page illustrated comic: Gran and the beacon before
the storm, the destructive storm and saved light, then Mika's promise with Pip
the frog and Rue the older fox. The square scene, separate live text, 1/3 count,
Continue/To the map and Skip story make its order explicit. Completing or
skipping records HeardStory 1000 in the existing field; Opening story permits
replay. Its controls do not change the pinned puzzle.

The paper-and-brass frame names one speaker and gives one short idea at a time. Continue and Skip story lead to the concrete pending puzzle or return to the map. Heard dialogue persists in the local profile. Mika accompanies a successful delivery; failure offers free current-stage Retry and the tool bag. Ordinary Undo belongs to the intact current board while playing; a Return hourglass is required for the latest collapse in the current stage. A first-clear receipt is recorded from the committed win before animation; tools cannot rewind that award. The machine remains the visual evidence of the result.

### Journal, workshop and keepsakes

The journal reuses paper controls, Balsamiq live labels and Mika's existing alpha
portrait. Collection and workshop show all four selectable object thumbnails
together, a larger selected object, and live progress/action copy. A warm gold
selection wash identifies the chosen thumbnail. Collection names the gift and
its three-part progress,
then distinguishes choosing a future gift from equipping an owned keepsake.
The map places the equipped keepsake beside Mika without changing symbol faces.
Workshop pairs each of four illustrated miniatures with its exact ten-stop
range. Unfinished views use a subdued translucent illustration and explicit
building copy; only ten matching node receipts produce the fully opaque ready
view. This is a disclosed future miniature preview, not intermediate
construction artwork.

An earned parcel discloses the selected gift before opening. Its saved claim
precedes the short arrival pulse; Reduced Motion omits the pulse. The receipt
never rerolls. Requests show one existing friend and one concrete repair,
bridge or stamp task at a time. Personal routes state mode, puzzle language,
ordinary delivery or stamp objective, best route and optional no-hint/clean
annotations. The stamp-trail view explains that ordinary travel stays open.
All progression labels derive from local saved facts; no global rank is shown.

### Shop and optional no-ads purchase

The prepared one-time NonConsumable `cascade.no_automatic_ads` removes automatic
ads; optional videos remain a choice. Live price lettering comes only from store
metadata. Current resource flags are false and native keys/products do not exist,
so the view explains that the shop is not open and the whole journey remains
available without purchases. Purchase, restore, checking, deferred and retry
states use live localized copy and enabled-state controls. Keepsakes stay free
earned objects; paid cosmetic packs are only a future option. No new product
artwork or palette is introduced.

Official Unity IAP 5.4.4 and Google Mobile Ads Unity 11.5.0 with UMP are installed;
LevelPlay was removed. The existing workshop/enamel visual system also carries
coin packs, without invented cash prices. Wallet, category tabs, two offers,
a contrasting assistance/test-status notice and Back preserve the established
hierarchy. See Docs/ADMOB_STORE_UI.md for the scoped native Editor evidence.
The publisher receipt verifier is prepared in Backend/ReceiptVerifier; it is
not hosted. The client saves verified grants before confirming pending orders.
UMP gates AdMob initialization and required privacy options live in Settings.
Registered store products, publisher IDs, HTTPS deployment, native dependency
builds, physical-device consent and store sandbox checks remain activation work.
These client views do not establish live purchases or ads. C16 policy remains.

### Saved path and recovery

Recovery reuses the quiet paper surface, dark live text and wide gold action. One heading and one short explanation distinguish backup recovery, an unavailable puzzle, an unreadable path and a failed write. Continue acknowledges a backup that may omit the last move; Start a new puzzle retains valid progress, while Start a new path explicitly replaces an unreadable path. Try saving again retains the committed in-memory attempt until writing succeeds. A newer-version save shows update guidance with no invented store link or destructive action.

The local profile checks its payload checksum and retains a valid backup. A pinned attempt restores its exact symbols and committed state through verified replay, including Undo history and hint/Undo flags. Recovery pauses puzzle interaction and never portrays incompatible or unreadable data as restored progress. The five recovery states are captured in EN/RU/UK at 320×693 in `Reports/P16`; disk and scene behavior are evidenced separately by tests.

### Precision and inspection

Comfort enables large previous/next focus controls and left/right exchanges with cost or rejection copy. The selected symbol is named without the rejected default slot-coordinate toolbar. Target magnification pauses gameplay and displays exact order with position numbers. These are implemented access paths; physical-touch effectiveness remains unverified.

## Do's and Don'ts

### Do:

- **Do** reuse each authored silhouette and material wherever its SymbolId appears.
- **Do** move and scale the live glyph with its selected or held body.
- **Do** preserve deliberate x/y tracking when Reduced Motion removes decorative lift, zoom and tilt.
- **Do** preserve the complete target and the direct-touch default.
- **Do** keep precision controls an explicit Comfort preference.
- **Do** bind the map marker to actual local first-clear receipts and label future previews honestly.
- **Do** move map islands, their invisible targets and the small standing Mika together, retaining the separate broad conversation target.
- **Do** select the story's region before showing its character and dialogue.
- **Do** bound ambient art outside gameplay information and freeze its phase while paused or suspended.
- **Do** inspect native 390 × 844, 320 × 693 and wide Editor captures after layout changes.
- **Do** preserve original assets, licenses, generation provenance and Unity meta identities.

### Don'ts:

- **Don't** turn physical rails into full-width rectangular cards or restore the crowded default toolbar.
- **Don't** bake gameplay text into imagery or change symbol identity to indicate state.
- **Don't** let presentation, animation completion, scenery, physics or ad callbacks change puzzle state.
- **Don't** portray browsed islands as reached, an unsuccessful write as saved, or unreadable progress as restored.
- **Don't** describe online concepts or disabled native Ads/IAP preparation as live integration, invent a price or imply receipt-server activation; Ribbon Reach remains local optional play.
- **Don't** copy ShapeMania screens, characters, sprites, props or materials into this world.
- **Don't** equate Editor captures and automated tests with device, haptic, performance, build, native-language or human-enjoyment proof.


### UI and motion polish — 2026-10-05

Shared menu headings use centered 30-point Balsamiq titles and 17-point descriptions inside a 15% side inset, clear of the carved window frame. Standard menu actions have at least 48 physical pixels of height: warm gold for the next action and quieter cream for alternatives, including Back. The opaque yellow menu sheet is removed. A portrait painted workshop window fills the phone, with quiet cool sky behind content and timber, cloth, tools and lanterns at the edges. Wide menus retain the complete landscape workshop. Cloud drift, distant light and sparse dust stay behind the UI; darker menu ink preserves reading contrast. Settings uses full-width language choices, including the next-puzzle language. The collection keeps the cosmetic sky ribbon distinct from the functional blue spool. Selected/equipped gifts no longer offer a redundant action.

Gameplay guidance has a quiet cream backing, and wood/fuse counters use number-safe EN/RU/UK wording. Victory emphasizes Next delivery; failure emphasizes Retry. Menu arrival takes 160 ms without blocking input, button press compresses to 97.8%, and reduced motion skips both. Filled buttons expose a focus outline.

Continuous suspension uses smoothly sampled curves and an antialiased cylindrical hemp shader. Hidden boards cease being rope anchors. Board springs retain their weight through presentation synchronization; exchanges arc above the plank and land with one short damped settle. This is scripted presentation, never gameplay physics.

Hanging tools sit about 19 reference units farther below the deck. Each assembly swings about its wooden socket; both straps follow the illustrated grip points, with the bomb attached to its metal collar. One slow wind field drives deck sway, a subtle roll, rope bow and delayed tool motion. Heavy tools respond less than light ones; socket acceleration adds a bounded inertial response. Shapes, glyphs, shadows and rope knots follow the deck. Pause and suspension freeze the springs; Reduced Motion resets them to a stationary pose. Cosmetic springs use bounded substeps and never spend moves or alter contacts.

Wind now combines smooth irregular breeze with a bounded gust envelope; a landing shift cues one short gust. Holding a piece softens decorative motion. A teal directional arrow is painted on a clear part of the destination plank, separated from fragile-edge notches. The quiet caption says what happens on landing; the existing device guide shows before/after row order and the matching direction. Wind coaching uses short EN/RU/UK copy that fits the phone feedback band. More wood is the single gold action when recovery is offered; ordinary Retry stays cream and keeps the current pin.

Atmosphere uses slower cloud layers, a glider fade while holding a piece, and soft weather with edge fades. UI weather updates at most 30 times per second; world weather is bounded to 8/18 particles for Low/Full. Off removes both world and UI weather. Existing pause/suspension and reduced-motion clocks remain authoritative for scenery. No new bitmap assets or packages were needed.

Three-stage gameplay keeps one receiver target and a compact level/1–3 counter in the existing header. Checkpoint feedback lives in the cream status band, with one gold Go lower action and the same free Retry/Undo controls. Short EN/RU/UK text describes the next stage. The finale's entry caption sits below hanging tools, including their sway clearance. During a 650 ms descent the old connected wooden rig rises away and the next arrives from below; mesh snapshots carry shapes, raised lettering, straps and device artwork together. The distant composition shifts more slowly, without blur or full-screen shake. The next stage is committed and saved before this cosmetic sequence. Reduced Motion replaces travel with a settled rig; focus loss, resize, menus and disable settle without replaying progression. The minimum counter is hidden during travel. Route notes name the level and three stages, and count the whole delivery.

Outcome motion now has an authored lantern sequence: a finite staggered receiver bounce, warm receiver emission and pooled rising colored light seeds, followed by Mika and the result controls. Failure has a smaller uncertain wobble and the new thoughtful Mika; Retry fills the primary row when no stronger-wood action is offered. Receiver height interpolates into the result layout before the buttons appear. The glider stays clear of outcome copy. Reduced Motion preserves the result, color and visible static wood load while removing spatial motion. The required device symbol sits inside an ivory tag suspended on two hemp cords from the tool's wooden crossbar. A lighter independent pendulum follows the common wind; the pause screen's How to play page explains it with the same animated assembly.

The readability follow-up places shared menu headings and grouped reading copy on opaque neutral ivory cloth, RGB(.99,.98,.92), with deep teal ink RGB(.055,.16,.17). Source-color contrast is 14.62:1 on these surfaces. The scenery remains visible around them, with no full-screen wash. Shelf captions use matching cloth labels; thumbnail controls use teal enamel and gold selection. Disabled actions use opaque sage rather than translucent gold. Menu actions adapt to at least 48 physical pixels at the small reference size, with shallow text padding and a 1.10 large-text multiplier for two-line actions; body copy retains 1.12. Gameplay actions have a 52-pixel band and 17-pixel text with compact padding. Device/guide counters reserve real font line height. See Docs/MENU_READABILITY_AND_TAGS.md and Reports/Readability/README.md.

Wood bow grows from about .10 to .27 world units with wear and relaxes after repair. The original logical strip thickness is independent of its expanded deformation/culling bounds. At exhaustion the duplicated center seam lets two halves release from their rope ends, followed by textured splinters. Shapes, letter faces, shadows, knots and hanging sockets follow their corresponding cosmetic deformation; the Domain never sees this simulation.


## Menu folio — 6 October 2026

Historical checkpoint: the current Home, play choice, comic, shelf and map
guidance above supersedes this checkpoint's one-sheet folio composition. Its
language/settings, earned-object and reaction refinements remain where implemented.

This supersedes the fragmented menu cloth cards described above. The user rejected their empty space, weak hierarchy, awkward buttons and merged text. The menu mode is Operate inside the existing Sky Wind Workshop world: a single quiet ivory folio, shared left alignment, warm timber binding, large character/object artwork and direct sage navigation. Shelf thumbnails reserve their area for the actual object, with gold and a check for selection. Workshop progress is a filled green rail alongside an explicit count. Footer actions have fixed physical baselines at 16 and 80 pixels, 52-pixel height and a 12-pixel gap. Menu short actions do not wrap. Primary captions are 22 reference units, compact captions 20; body copy starts at 20, with a real two-line subtitle area. Balsamiq, the game palette and the illustrated workshop remain.

Languages are two carousels with native language names, broad arrow targets and one selected value. Menu changes apply immediately. Word language browsing is separate from confirmation and affects only new puzzles. The confirmation is a compact folio with its heading on the same surface. Sound/comfort/language tabs are direct navigation; settings have full-row tap targets and recognizable switches. Bonus lesson selection also uses arrows and a 1–6 counter. Requests, stamp trails and route records put previous/back/next together in the footer, clear of the preview row.

Device reactions have contrasting folded ribbons and a short damped entrance, with localized live lettering. The bomb's extinguished-fuse sibling preserves its painted contact and body; small colored light seeds rise at the hanging body. Feedback anchors follow its canvas-to-world position through wind and viewport changes. Keys, repair, stamps and power-ups have related colored bands and feedback at their actual props. Guide and preview shapes use beveled color shading. Pause, reduced motion and effect-quality bounds are retained, with no change to resolver transitions.

Actual native Unity inspection and four selected PlayMode checks are recorded in Reports/MenuReset/README.md. This is local evidence for readability and interactions, not a human-enjoyment or device-performance certification. Original source sprites, Cloud settings, prices, packages, persisted IDs, schemas and core rules remain.

## Tool reactions, enamel controls and wind anticipation — 6 October 2026

Matching symbols are painted directly on every tool body, including guides.
No secondary matching plaque. Reaction focus uses a separate enlarged tool over
centered live cloth lettering; tool-specific motifs, bounded timings and quiet
quality settings preserve cause and effect. Wood damage is authored texture
states with matching falling splinters, with existing cosmetic flex retained.
Gameplay actions use wood/brass/enamel, cream-on-teal labels and dark-on-amber
primary action. Pause bars use the same cream. Settings tabs are short; controls
retain physical content padding and safe-area bounds, including large text.
Strong wind gets a brief directional forecast with progressive spring pressure
before its existing landing shift; ambient wind stays subtle. No fake countdown.
See Docs/GAMEPLAY_REACTION_MOTION.md and Reports/Reactions/README.md.

## Single-cord activation sequence — 6 October 2026

Each tool has one softly shaded, continuous hemp cord, attached to its physical
handle and following the existing wind-driven pendulum. Activation clones only
its body and symbol; the duplicate departs from its exact rendered pose, travels
into focus, pulses with separate clean motifs, then reveals status cloth below.
The original remains suspended. Central and local status wait until .88 seconds;
there is no early title flash. Total beat1.72s, Reduced Motion static .55s.
Clean sibling atlases remove embedded atmosphere. A narrow native alpha edge
material excludes residual low-alpha fringes, with separate settings for solids,
glass and smoke. No duplicate Outline or added silhouette. The last smoke pass
uses five smaller, softer wisps so the bomb stays recognizable.

## Original tool departure and gust inertia — 6 October 2026

User correction supersedes all previous duplicate-body descriptions. The original
icon and symbol leave the dock for central reaction focus, keeping object identity.
The consumed dock, socket and cord disappear. After the beat its hidden home pose
is retained for Undo/restart; used contacts stay empty, including saved attempts.
No gameplay transition is added. Flight/pulse/particles/status timings are retained.
Ambient decks and ropes now sway more visibly but gently. Canonical gust pressure
adds a single shared directional velocity impulse to deck/rope/tool springs, then
settles. Deck translation is bounded to .22 world units and roll to2.4 degrees;
quiet/pause settings and the wind direction warning stay intact.


## Flexible hemp suspension — 6 October 2026

Main ropes and short tool cords use cosmetic constrained point chains, with pinned
platform/handle endpoints, inertia, gravity, damping and coherent local wind.
Different heights and sides have distinct seeded air flow. A directional gust
adds velocity; no random gameplay transition is introduced. Slight slack and soft
bend resistance avoid accordion folds. Cubic rendering preserves broad curves;
the hemp braid repeats by world length instead of stretching across the screen.
Visible floor knots remain anchors; removed floors reconnect the free span.
Pause freezes integration, Reduced Motion is taut and still, and viewport/topology
changes reset affected chains. See Docs/FLEXIBLE_ROPE_PRESENTATION.md.


## Impact, physical fragments and live light — 6 October 2026

Lifting, exchanging and landing communicate weight through the platform springs
and neighboring constrained rope spans. Falling and shaken pieces use eight pooled
Rigidbody box proxies to tumble, bounce and collide, then guide into the exact
committed seats in the existing isolated cosmetic PhysX scene. Normal wind and
exchanges remain scripted. Timber fragments use pooled rigid
bodies in an isolated PhysX scene; they spin, receive wind and bounce against the
view's platform contacts before leaving or fading. Pause/quiet remain authoritative.
Candy/ivory materials now use actual URP clear coat. Wood has a sloped deforming
top and alpha cutout shadow masks; rope lighting follows the live main light.
Painted background/UI and supporting contact artwork remain illustrated. Native
light comparisons and mobile pipeline witnesses belong to Reports/PhysicalPresentation.
Target replacement immediately excludes old layout children, preserving centered
receiver symbols before the first command. No gameplay physics or new rules.


## Finger-to-seat continuity — 6 October 2026

Press acknowledges through the actual body's contour before a short lift and
bounded enlargement. The same glossy body and embossed glyph follow deliberate
x/y drag freely, with gentle tilt and a slight legal-target lift. No square slot
or target overlay. Stable hit areas do not chase the moving artwork.
Free/cancelled releases return in 180 ms; committed travel inherits the actual
released position, scale and angle. Selection never fights the touch pose.
Unchanged duplicate symbols keep their seats during a far swap. Quiet motion
removes lift/zoom/tilt while preserving direct x/y finger tracking. Docs/TOUCH_FEEL.md
and Reports/TouchFeel remain the earlier checkpoint; Docs/TOUCH_AND_STORES.md
and Reports/TouchRevision record the current extension. No gameplay or asset
schema change.


## One outcome message — 6 October 2026

Hanging tools have no local result caption or outcome ribbon. Forecast uses the
existing icon tint and shared forecast band. Real results belong to the central
original-tool reaction and woven status animation. The armed bomb's moves/frozen
countdown remains functional information. See Reports/StatusCleanup.

## Paused first encounters — 6 October 2026

A quiet ivory modal dims the full viewport and stays within the device safe
area. One heading, one isolated animated example, a short explanation and an
amber acknowledgement teach each mechanic. The playable row, board motion and
wind remain paused; example animation owns its own cosmetic clock. Known
mechanics do not repeat. Landing wind gates descent before stage advancement;
ambient wind gates the first impulse. A tool's explicit Use resumes only from
the acknowledgement button. Metadata is persisted before resuming, never from
tween completion. New authored raster assets are unnecessary: the examples
reuse the game's actual devices and original tool art.

## Whole-world map and earned celebration — 7 October 2026

The map is an open sky composition: dark ink region and goal at the top,
illustrated islands in the center, current-island progress below Mika, then
task/world progress and framed enamel actions. No cream heading, footer or
current-marker sheet. Amber denotes the level action; ivory arrows read on teal.
Minimum touch size48px; safe-area roots and Large Text remain active. One
world persists for10–24 full levels, with2–5 levels per island.

Earned celebration uses the original sewn gold stars, fabric ribbon, confetti
and parcel. The ribbon opens first, stars arrive in sequence, and score follows.
A parcel anticipates, opens, raises its object, then settles; a bounded warm
light fades with the finite confetti. Focus pauses before clock advancement.
Reduced Motion retains final award information without spatial motion. No
reward or progression is triggered by motion completion.


Five-star completion and shared menu surfaces — 7 October 2026

The completion composition is an open sky, with rank on the existing woven ribbon, five centered sewn gold stars, Mika beside score, an aligned three-column move/Undo/time row, illustrated gold and contained-firefly rewards, then one continuation. No generic cream score/resource boxes. Dark teal text is the foreground on clear sky; the numerical resources accompany their generated emblems rather than repeated text labels. Optional legacy time uses an em dash. The earned star cascade, finite glow/confetti and score count remain interruptible; Reduced Motion settles their final state.

Menu headings and notes now read directly on the world. Workshop sky/window centers were cleared of detailed islands in new portrait/wide artworks, while old assets remain. Titles are centered inside the timber frame. Actions, bag rows, selected tabs and language carousels use the existing authored brass/teal enamel and ivory type; selected tabs turn gold with dark ink. Language arrows have light ink. First-use lessons, dialogue and preview-ad frames use a contrasting dark teal authored plate instead of cream. Physical ivory letter tiles, woven achievement ribbon and small switch knobs remain meaningful materials, not generic menu backplates.


Parallel mode maps and individual celebration paper — 7 October 2026

Map mode tabs use selected amber/dark ink and idle teal/ivory enamel. The tab row is recalculated with the Canvas scale on every map layout, keeping each hit target 52 physical pixels high even at320×693. Mode progress and a drawn gold-star counter sit below the tabs with a separate gap; map illustrations reserve the header region. No additional mode chooser in Home/Settings. Numbered level buttons also use teal enamel and ivory numbers instead of brown wood against wood.

Celebrations instantiate24 individual leaf/fabric objects (12 low-quality,0 effects-off/reduced-motion) from six separated transparent sprite cells. Each has its own launch velocity, drag, gravity, wind phase and spin, with a thin tumble silhouette and a late smooth fade. The whole atlas is never rendered. The simulation is decorative and does not change receipts, currency, progression or puzzle state. Native reward/gift captures and physical-size readbacks are in Reports/ModeJourneys.

Menu icon refinement — 7 October 2026

Retain the workshop/enamel/illustration system. Imported menu atlases preserve their source aspect (NPOT None). Home illustrations use authored silhouette rectangles, a 44-pixel square region centred vertically in the left column and Simple Image/preserveAspect. Inventory rows share this rule. Map inventory uses a 48-pixel square target aligned with the Back control. All square regions refresh with Canvas scale. Disabled shop actions keep a muted teal enamel frame and pale lettering. See Docs/NAVIGATION_ICONS_AND_RETURNS.md.


Actual UI refinements from the screen audit — 7 October 2026

The workshop frame, enamel actions, ivory lettering, original illustrations and
open-sky completion composition remain the design. The audit corrects measured
fit and spacing defects within those surfaces: setting labels and switches sit
inside the enamel edge; navigation titles reserve a right inset; lessons,
no-advertising explanation and word-language guidance have more vertical room.
Device instructions use a narrower inset with bounded type fitting, and wood
engraving numerals fit their actual painted band. These local fits do not create
a new shared type scale.

The compact travel bag keeps its four destination rows and workshop access.
Below a 700-physical-unit menu height, its row height becomes 52 physical units
and Mika/parcel/shelf shift upward; the regular rows remain 62. This preserves
48-plus hit regions rather than dropping an action. Result Undo owns its lower
row after reparenting; the hidden in-game tool bag must not re-anchor it. Retry,
map and continuation reuse the authored teal/gold enamel states. The prologue
page count now sits below the dark illustration, on the clear world.

Music and Sound effects were already independent persisted controls. Keep them
separate: Music owns score sources; Sound effects owns event cues and wind/rain.
The audio refinement preserves the eight world scores and original menu WAV,
selecting a new balanced menu sibling to reduce its measured piano-to-flute
jump. No new music theme, preference schema or visual primitive is introduced.
See `Docs/AUDIO_FORM_AND_CONTROLS.md` for form, source measurements and owned
audio lifecycle behavior. `Docs/VISUAL_AUDIT_OCT07.md` records the before
evidence and the current validation checkpoint; source edits alone do not
prove corrected rendering, listening quality or touch effectiveness.

The final audit checkpoint contains 50 native Editor poses: 47 detailed stable
state audits and three direct catalogue captures with internal state checks.
Independent finish review passed 21 selected after views plus all three direct
catalogue views, with no further material fixes requested. Final compilation
succeeded and the focused audio lifecycle/control run passed 2/2 after a test
crossfade-wait correction; the initial 1/2 result is preserved. The final QA-only
character-mesh repair compiled, but bulk measurement was not rerun after it.
This scoped validation does not establish all-state or physical-device behavior,
audio listening quality, performance or enjoyment; no build/install was made
for this audit. See the audit document and its final validation summary.
