---
name: CASCADE — Sky Wind Workshop
description: A small keeper carries light through a living sky of tactile puzzle machines.
colors:
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
typography:
  menu-title:
    fontFamily: "Balsamiq Sans"
    fontSize: "34px"
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
  button-secondary:
    backgroundColor: "{colors.ui-paper}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    padding: "2px 3px"
    height: "48px"
  button-primary:
    backgroundColor: "{colors.drop-gold}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    padding: "2px 3px"
    height: "48px"
  button-menu:
    backgroundColor: "{colors.ui-paper}"
    textColor: "{colors.ui-ink}"
    typography: "{typography.button}"
    rounded: "{rounded.panel-source}"
    height: "60px"
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
    backgroundColor: "{colors.platform-mint}"
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

A small keeper carries light through floating islands and wind-powered toy machines. Mint enamel, warm brass, ivory letter tiles and six colored silhouettes make the puzzle tangible. Mika's patched overalls, saffron scarf and lantern connect the mechanism to a warm, imperfect character. Painted regional skies and isometric routes give each delivery a place and a neighbour.

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
independent verdict scores both listed findings resolved, disposition ship at P17 scope. The wooden/rope,
weight/fall, pictorial-device, wide-screen and menu revision in
`Docs/VISUAL_MOTION_REWORK.md` is later pending work, not this implemented world.

P18 makes two narrow clarity corrections within this world: the stable `Hint`
key displays `Помощь`/`Поміч` in RU/UK, and map paging reuses the authored
direction chevron. `Docs/P18_INTEGRATION.md` owns this phase's contract;
`Reports/P18/impeccable-review.md` records ship at this correction scope. Three
final 320 × 693 native Editor frames confirm the corrected RU caption, RU guide
and UK map against the existing system. The 3/3 focused tests and matching
157-puzzle traces establish their stated behavior boundaries; the roughly
100 ms whole-Editor frame observation is not a device performance pass.
Physical devices, final player build, child comprehension and human enjoyment
remain unverified. This phase adds no visual tokens or artwork and leaves the
wooden/rope, motion, pictorial-device, wide-screen and menu revision pending.

**Key Characteristics:**
- Direct touch lifts a piece and its live glyph together; the receiver stays visible.
- Six stable silhouettes and colors remain readable across all three regions.
- Enamel rails, brass joints and soft contact shadows make the machine tangible.
- Mika, Pip and Rue connect short optional dialogue to a concrete destination.
- Illustrated routes distinguish the current path from future-region browsing.
- Clouds, rooted plants, a glider and the lower-right windwheel give the sky restrained life.

## Colors

Warm paper and brass anchor a mint, coral, lilac, yellow, sky and apricot toy palette. Native UI colors and lit material inputs retain their source notation in the frontmatter; rendered appearance also depends on lighting and color space.

### Primary

- **Hardware Gold** marks receiving rims, sockets, selection frames and mechanical joints.
- **Drop Gold** marks the committed Descend/Deliver action and primary travel/dialogue actions; the historical token name remains stable.
- **Platform Mint** is the Wind Gardens rail, repair body and alternating windwheel sail. Rain Mill rails use Star Sky; Copper Orchard rails use Device Coral with Receiver Ivory highlights.

### Secondary

- **Triangle Yellow, Circle Mint, Square Coral, Diamond Lilac, Star Sky and Hexagon Apricot** preserve the six shape identities in the row, contacts, destination and magnifier.
- **Device Coral** also marks bomb bodies and fragile notches. Lilac links paired edges and stamp hardware.
- **Legal Landing** and **Blocked Landing** tint the adjacent destination during a drag. **Preview Gold** washes affected slots during a command preview; the solid selection frame remains distinct.
- **Map Current** identifies an available selected stop; **Map Restored** identifies a completed stop. **Path Open** and **Path Future** distinguish connecting pebbles. Mika marks actual locally saved progress, independently of browsing.

### Neutral

- **UI Ink** carries live lettering and authored control marks. **High Contrast Ink** is the implemented optional text override.
- **UI Paper** carries controls, navigation panels, dialogue and magnifier; **Setting Paper** distinguishes setting rows.
- **Tile Ivory** carries glyph bodies, highlights and small machine insets; **Receiver Ivory** carries the receiving inset and guides.
- **Warm Graphite** carries damage seams, timers, rollers and hinges. **Contact Shadow** grounds the toy locally.
- **Switch On** and **Switch Off** accompany knob position and the labelled setting. Button hover, pressed and disabled tokens are Unity ColorBlock multipliers over each base fill.

**The Shared Identity Rule.** Use the same silhouette and material for a SymbolId in the row, target, device contact and magnifier; glyph symbols use live text on ivory bodies.

## Typography

Balsamiq Sans Regular and Bold provide the rounded hand-lettered voice. Text remains live TMP; the static atlas assets derive from the independently sourced font files and SIL OFL license under `CascadeUnity/Assets/_Cascade/Art/Fonts/`. No native fallback stack, custom tracking or line-height scale is established.

### Hierarchy

- **Menu title** introduces pause, guide and settings. **Region/speaker** names a map region or dialogue speaker.
- **Level title** is the lesson header; **journey task** is the shorter destination line and map task. The Lessons logo is a separate Bold 42-unit mark.
- **Dialogue** uses larger Regular text with short sentences; **body** handles state and help; **button** handles actions. Map counts use **secondary** text; Skip story uses a Bold label at that size.
- **Focus** names the symbol in optional precision controls. **Receiver label** belongs immediately above the cradle.
- **Token glyph** and **target glyph** use Bold and stay centered on their mesh faces. Device glyphs use Bold at the same compensated size as **device** explanations.

Frontmatter `px` type values describe Unity reference-canvas units, not measured physical-device size. Auto sizing is off and ordinary labels wrap. Device and receiver text is compensated by `unit = 1 / min(1, canvasScale)`; device text therefore targets 14 rendered pixels at the supplied small reference scale. Larger text applies a 1.12 multiplier to eligible UI copy, excluding symbol faces; explicit layout/font overrides still exist, so it is not a promise that every label grows. Full localized editorial and glyph review remains P15 work.

**The Live Face Rule.** Move and scale a letter with its mesh face through selection, holding, exchange and landing; never bake gameplay letters into scenery.

## Layout

The camera-space uGUI canvas uses a 390 × 844 reference resolution with height scaling and `Screen.safeArea`. Wide Editor views cap content at 430 reference units while the scenery fills the viewport. The composition remains portrait; there is no separate landscape machine layout.

### Direct play

A destination and one pause control occupy a 48-unit compensated header, 12 units below the safe top. A 44-unit feedback band sits below. The bottom has three actions, Undo, Hint and Descend/Deliver, in a 48-unit compensated band 8 units above the safe bottom. Retry, map, settings and device help are reached through pause or result flow.

The complete receiver is 100 reference units high. Its bottom is normally 82 compensated units above the safe bottom, moving to 184 for enabled precision controls or win results and 232 for failure comparison. Floors occupy the remaining region above the receiver clearance and below the feedback area. Rail width is `min(91% of content width, symbolCount × 76 + 24)`; receiver width is `min(90% of content width, symbolCount × 68 + 40)`. Three-piece rows are expanded but retain shorter physical rails. Cells share a 3-unit gap.

Precision input is explicitly enabled in Comfort and is off by default, including on seven/eight-symbol rows. Its two rows use a 48/8/48 rhythm. Long rows instead retain stable hit regions and a lifted, enlarged held piece; no claim is made that eight narrow cells each meet a 48-pixel target. The receiver opens a full safe-area magnifier with four columns and up to two rows.

### Journey and dialogue

The map uses a tall illustrated five-stop route between a compact paper heading and a lower task panel. A map stop is 62 × 60 reference units; page controls are 76 × 60. The source image is stretched into its normalized map rectangle by the current uGUI implementation; this is not a cover-fit or pixel-perfect raster-registration guarantee. The current map reuses each regional illustration over its five-stop pages. Mika appears only on the actual next stop from local first-clear receipts, never on a locked future preview. The future footer gives distance and a return action.

Dialogue places a large transparent character above/behind a low paper frame. A thin brass line, Bold speaker name, short Regular sentence and separate Continue/Skip story actions provide hierarchy. Story entry explicitly selects the character's region. Dialogue blocks puzzle input.

### Travel bag and earned objects

Map Menu opens the travel bag. Journal, workshop, stamp trails and personal
routes become available after three restored lights; Settings and Back to map
remain in the same bag. Item views use one large alpha illustration, short live
copy, separate previous/next controls and a clear lower action. The journal
pairs Mika with restored-light, forgiving visit and next-parcel summaries.
Progress lives on these optional surfaces rather than adding meters to the
puzzle HUD. Item artwork preserves aspect ratio inside its padded native UI
rectangle; the narrow source margins, especially Beacon Model's vertical
margins, must not be treated as permission to clip painted edges.

**The Complete Machine Rule.** Keep the persistent row, remaining floors and complete receiver order visible together; changing row length must preserve clear physical rail proportions.

**The Honest Path Rule.** Distinguish the browsed stop from the reached stop; future art must never imply unlocked or saved progress.

## Elevation & Depth

Beveled meshes, satin URP materials, a warm directional key and analytic contact shadows create depth. The key light is warm at intensity 1.20; ambient light is flat. Unity cast shadows are disabled for this view. The rounded extrusion uses contour rings at scale factors `0.88, 0.985, 1, 0.97, 0.89`. Shape plastics use smoothness 0.28 and metallic 0.02; other materials keep authored satin values.

Soft contact quads use `(1 − smoothstep(0.25, 1, radius))²` alpha, tinted by Contact Shadow. Each token, platform and receiver fits its own quad. This is a native shader effect, not a CSS shadow. Selection raises body and glyph from normalized center 0.38 to 0.48, changes size from 0.78 × 0.88 to 0.81 × 0.915 of the cell and reveals a gold frame.

### Motion and living scenery

A held piece lifts immediately with its glyph. Its preview scales toward a 64-rendered-pixel face, clamped to 1.10–2.5 times the base size. Horizontal travel is bounded to 1.15 cell spacings; upward drag adds at most 0.45 spacings above its hold lift. The input cells remain stationary. Releasing elsewhere, pointer cancellation, pause or focus loss returns the presentation without spending a move.

Committed exchanges use 190 ms cubic ease-out with short opposing arcs. Contact, bomb and fracture steps last 180, 240 and 130 ms. A whole-row fall uses 300 ms smoothstep followed by 110 ms landing, at most 8% squash and 0.025 world-unit local recoil. Delivery holds for 340 ms. Reduced Motion substitutes 55 ms state holds and at most two stationary dust fades per burst. Selection focus uses 100 ms or an immediate reduced-motion state. Screen shake is opt-in and off by default; Reduced Motion suppresses it. Dust and audio tails never determine command completion.

Three distinct far landscapes cover the viewport. Shared transparent clouds move at three speeds, lower-edge plants sway about their stems, and the original windwheel turns at 7 degrees per second from an 18-degree base. Regional tints adapt the shared cloud/plant layers. The whole rotated glider is bounded to the empty gap between feedback and the top floor; it hides during a held-token gesture and if no gap remains. All ambient motion is view-only. Pause and suspension stop phase progression; Reduced Motion displays phase zero and the fixed wheel angle. Map and dialogue use a static ambient phase because they pause the puzzle interaction.

**The Clear Flight Rule.** Decorative motion must fit wholly outside the row, receiver and feedback; a lower render layer alone is not sufficient separation.

## Shapes

Six original soft silhouettes form the row: rounded triangle, circle, square, diamond, star and hexagon. Letter tiles reuse the rounded square. The square/tile contour is one mesh unit wide with a 0.18-unit corner radius; the 6 × 1 beam has a 0.3-unit radius. Disks form mounts, caps and hinges. Mesh coordinates are not UI pixels.

Panels and buttons reuse the 64 × 64 nine-slice raster. The frontmatter `panel-source` token describes its 13-source-pixel corner construction with 22-pixel slice borders, not a universal runtime radius. Pause bars and precision chevrons use authored geometry. Mika, Pip, Rue, clouds, plants and glider use original raster cutouts with native alpha; preserve painted edges and transparent gaps rather than replacing them with vector approximations or runtime masks.

## Components

### Controls and settings

Warm paper controls carry dark Bold labels and Unity multiply tints for hover, pressed and disabled states. Descend changes to Deliver when the final-floor row matches; Hint changes to Use hint only for a valid offer. A separate custom keyboard focus treatment is not established. Setting rows use a readable label, an on/off track and positional knob; sound, motion, effects, contrast, larger text, precision, tutorial and dead-end preferences persist in the P16 local profile, alongside language and preferred mode.

### Persistent token and receiver

Pressing a shape or letter gives immediate lift and a larger held face; legal adjacent landing highlights and the feedback line explain command cost. Release commits one atomic adjacent exchange through the shared resolver. Tap-then-tap remains available; a long swipe does not chain exchanges. The gold rim and ivory inset retain the complete ordered destination. Magnifier faces derive from the same shapes and live glyphs. Failure shows the goal and delivered row separately; success seats the row before result actions appear.

### Platforms and devices

Thin colored enamel beams, ivory highlights and gold caps carry the original lock, bomb, key, repair, stamp, fragile, linked and bridge hardware. Up to three graphite seams show damage. Contact faces use the shared symbol identity; visibility follows committed state. Rain Mill and Copper Orchard change the rail material without changing symbol colors or puzzle rules.

### Map route

The isometric illustration, numbered stop buttons, small path pebbles and Mika marker express travel. Restored count and unlocked stop come from the existing 40-puzzle catalog and durable local first-clear receipts, deduplicated by campaign node across modes and locales. The gold action says Let's go, Continue or Play again where available; future pages explain remaining stops and offer Back to my path. Lessons retains the existing level/mode browser. These are three regional art kits and eight five-stop pages, not 160 new campaign puzzles.

Map paging uses the same native `DirectionButton` and `ChevronGraphic` as
precision focus controls. A dark authored left/right chevron replaces the
caption inside the paper target; its graphic does not intercept input. The
`Earlier`/`Later` button identities and existing handlers remain, with the
map's 76 × 60 reference-unit geometry. This local size override does not change
the shared direction-button token or introduce a new visual primitive.

### Dialogue and results

The paper-and-brass frame names one speaker and gives one short idea at a time. Continue and Skip story lead to the concrete pending puzzle or return to the map. Heard dialogue persists in the local profile. Mika accompanies a successful delivery; win/fail actions preserve free Undo and Retry. A first-clear receipt is recorded from the committed win before animation; Undo does not erase it. The machine remains the visual evidence of the result.

### Journal, workshop and keepsakes

The journal reuses paper controls, Balsamiq live labels and Mika's existing alpha
portrait. Collection and workshop show one distinct object at a time with
separate carousel arrows. Collection names the gift and its three-part progress,
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

### Saved path and recovery

Recovery reuses the quiet paper surface, dark live text and wide gold action. One heading and one short explanation distinguish backup recovery, an unavailable puzzle, an unreadable path and a failed write. Continue acknowledges a backup that may omit the last move; Start a new puzzle retains valid progress, while Start a new path explicitly replaces an unreadable path. Try saving again retains the committed in-memory attempt until writing succeeds. A newer-version save shows update guidance with no invented store link or destructive action.

The local profile checks its payload checksum and retains a valid backup. A pinned attempt restores its exact symbols and committed state through verified replay, including Undo history and hint/Undo flags. Recovery pauses puzzle interaction and never portrays incompatible or unreadable data as restored progress. The five recovery states are captured in EN/RU/UK at 320×693 in `Reports/P16`; disk and scene behavior are evidenced separately by tests.

### Precision and inspection

Comfort enables large previous/next focus controls and left/right exchanges with cost or rejection copy. The selected symbol is named without the rejected default slot-coordinate toolbar. Target magnification pauses gameplay and displays exact order with position numbers. These are implemented access paths; physical-touch effectiveness remains unverified.

## Do's and Don'ts

### Do:

- **Do** reuse each authored silhouette and material wherever its SymbolId appears.
- **Do** move and scale the live glyph with its selected or held body.
- **Do** preserve the complete target and the direct-touch default.
- **Do** keep precision controls an explicit Comfort preference.
- **Do** bind the map marker to actual local first-clear receipts and label future previews honestly.
- **Do** select the story's region before showing its character and dialogue.
- **Do** bound ambient art outside gameplay information and freeze its phase while paused or suspended.
- **Do** inspect native 390 × 844, 320 × 693 and wide Editor captures after layout changes.
- **Do** preserve original assets, licenses, generation provenance and Unity meta identities.

### Don'ts:

- **Don't** turn physical rails into full-width rectangular cards or restore the crowded default toolbar.
- **Don't** bake gameplay text into imagery or change symbol identity to indicate state.
- **Don't** let presentation, animation completion, scenery, physics or ad callbacks change puzzle state.
- **Don't** portray browsed islands as reached, an unsuccessful write as saved, or unreadable progress as restored.
- **Don't** describe the 160-stop target, Long Reach, online concept or pending wooden-island presentation revision as implemented.
- **Don't** copy ShapeMania screens, characters, sprites, props or materials into this world.
- **Don't** equate Editor captures and automated tests with device, haptic, performance, build, native-language or human-enjoyment proof.
