---
name: CASCADE — Sky Wind Workshop
description: A bright suspended toy machine in a quiet painted cloud garden.
colors:
  ui-ink: "rgb(16% 28% 29%)"
  ui-paper: "rgb(100% 96% 84%)"
  drop-gold: "rgb(98% 74% 32%)"
  hover-tint: "rgb(100% 94% 78%)"
  pressed-tint: "rgb(82% 88% 81%)"
  disabled-tint: "rgb(88% 90% 84% / 70%)"
  preview-gold: "rgb(95% 73% 29% / 30%)"
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
  title:
    fontFamily: "Balsamiq Sans"
    fontSize: "28px"
    fontWeight: 700
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
  button-direction:
    backgroundColor: "{colors.ui-paper}"
    textColor: "{colors.ui-ink}"
    rounded: "{rounded.panel-source}"
    height: "48px"
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

A small wind-powered toy machine hangs above a painted cloud garden. Mint enamel rails, warm gold hardware, ivory letter tiles and six colored silhouettes make the mechanism feel manufactured, friendly and tangible. The center stays quiet enough to read the complete persistent row and its gold receiving cradle together.

The game uses real beveled meshes, a single orthographic view, live lettering and a separate illustrated backdrop. Compact platforms and visible joints establish the machine; soft buttons borrow its warm materials. This is the original, code-led P12 world chosen within the author's delegated direction. There is no approved composite image or user-research result to claim fidelity against.

This document records the implemented gameplay surface at P12. P13 event animation/audio and P14 full flow remain pending. The source of truth is `CascadeGame.cs`, `ToyMachineView.cs`, `ToyArt.cs`, `ShapeFaceGraphic.cs`, `ChevronGraphic.cs` and `ToyAssetBuilder.cs`; the relevant captures and layout measurements are in `Reports/P12`. Original-art and font provenance are recorded in `Docs/P12_ASSET_PROVENANCE.md`. Native glyph review, physical-device performance and human enjoyment remain unverified. Sidecar samples are flat, self-contained diagrams of the native components; synthesized color ramps are preview aids, not additional shipped palette values.

**Key Characteristics:**
- A calm painted sky surrounds a compact, fully visible toy machine.
- Six beveled silhouettes retain consistent color identity; live letters sit on ivory tiles.
- Thin mint platforms and gold joints give each floor a physical role.
- The stable gold receiver carries the complete destination below the floors.
- Warm rounded controls remain separate from the token bodies they operate.

## Colors

Warm ivory and gold hardware hold a clear mint, coral, lilac, yellow, sky and apricot toy palette against the cooler painted sky. Frontmatter values are native UI colors or unlit material inputs; lighting and color-space conversion change their rendered appearance.

### Primary

- **Hardware Gold** defines the receiving rim, target sockets, selection frame, end caps and mechanical latches.
- **Drop Gold** distinguishes the committed Drop/Deliver control from the surrounding utility controls.
- **Platform Mint** identifies enamel platforms, repair bodies and alternating wind-wheel sails.

### Secondary

- **Triangle Yellow, Circle Mint, Square Coral, Diamond Lilac, Star Sky and Hexagon Apricot** are the six stable shape identities. They belong to the same authored silhouette in the moving row, device contacts, destination and magnifier.
- **Device Coral** marks bomb bodies and fragile notches. Lilac also connects linked-edge hardware and stamp parts.

### Neutral

- **UI Ink** is the live lettering and authored chevron color.
- **UI Paper** is the button and target-magnifier surface.
- **Tile Ivory** is the glyph body, rail highlight, wheel sail and small mechanical inset.
- **Receiver Ivory** is the warm receiving inset and guide material.
- **Warm Graphite** describes timer caps, damage seams, rollers and hinges.
- **Contact Shadow** provides soft local grounding below tokens, rails and receiver; it is not a panel border.

Hover, pressed and disabled tokens are Unity `ColorBlock` multipliers applied to the button's base color, not standalone replacement fills. Preview Gold is the translucent affected-slot wash. Do not substitute that wash for the solid gold selected-token frame.

**The Shared Identity Rule.** Use the same silhouette and material for a SymbolId in the moving row, target, device contact and magnifier; glyph symbols use live text on ivory bodies.

## Typography

**Display and body font:** Balsamiq Sans, with independently sourced Regular and Bold files. The shipped TMP assets are static atlas derivatives; there is no established native fallback stack. The sidecar uses a browser fallback only when Balsamiq Sans is unavailable in its preview environment.

The rounded hand-lettered character supports the manufactured toy world. The hierarchy comes from size and Regular/Bold weight, with dark ink across controls and content. Text remains live TMP. Source and SIL OFL licensing live under `CascadeUnity/Assets/_Cascade/Art/Fonts/`.

### Hierarchy

- **Title** labels the current level; **body** explains state and previews; **button** labels actions. Their normative sizes are in the frontmatter.
- **Focus** names the selected slot in the precision controls. **Receiver label** stays directly above the receiving cradle.
- **Token glyph** and **target glyph** are Bold. World glyphs share the token body's vertical anchor, including its selected position. Device glyphs also use Bold.
- **Device** text receives explicit small-screen compensation: the native font size is `14 × unit`, where `unit = 1 / min(1, canvasScale)`. The audited device labels render at 14 pixels at both supplied portrait capture sizes.
- Durability uses Bold at 15 reference units. The target magnifier uses a Regular 22-unit title, Bold 34-unit glyphs, Regular 12-unit shape names and Regular 13-unit position numbers. Those small supplementary labels are current magnifier details, not a global minimum-size standard.

All `px` type tokens represent Unity reference-canvas units at its nominal scale. They are not a claim of equal physical size on every device. Auto sizing is off; ordinary labels wrap. No custom tracking or line-height system is established.

**The Live Face Rule.** Keep letters centered on the actual mesh face in every selection state; never bake game letters or labels into scenery.

## Layout

The screen uses a camera-space uGUI canvas with a 390 × 844 reference resolution and height-based scaling. `Screen.safeArea` bounds the content. On wide displays, the content column caps at 430 reference units while the background covers the entire viewport. There is no alternate landscape machine composition.

The header and lower actions use 5% horizontal insets. The top utility band is 104 compensated units high and contains two 48-unit control rows separated by 8 units. Feedback has a 48-unit band below it. The bottom action band begins 8 units above the safe-area bottom and is 48 units high. The optional precision band begins at 64 units and is 104 units high; its two rows use the same 48/8 rhythm. Compensation uses the same `unit` described in Typography, preserving approximately 48 rendered pixels for the measured controls at 320 × 693.

The receiver begins 184 compensated units above the safe-area bottom and has 100 reference units of height. Floors occupy the space between the receiver clearance and the feedback region. All floors remain visible; the row is reparented to the active floor after the committed state changes. Platforms are sized to `min(91% of the content width, symbolCount × 55 + 24)`; the receiver uses `min(90% of content width, symbolCount × 55 + 40)`. Three-symbol platforms therefore remain visibly shorter than eight-symbol platforms. Row cells share available width with a 3-unit gap.

For seven or more symbols below 430 reference units of usable width, precision controls expose previous/next focus and left/right exchange commands. They may also be forced on. These controls supply the large targets that narrow token bodies cannot supply. The receiver opens a full safe-area magnifier with four columns and up to two rows of ordered symbols.

`Reports/P12/layout_audit.json` records no measured control overlaps, approximately 48-pixel minimum dimensions in the small reference captures, and 14-pixel device text. The captures include 390 × 844, 320 × 693 and wide Editor proportions. These are Editor-reference checks, not physical-device usability or performance evidence.

**The Complete Machine Rule.** Keep the persistent row, remaining floors and complete receiver order visible together; preserve compact rail lengths when the symbol count changes.

## Elevation & Depth

The system combines beveled mesh geometry, satin URP materials, a warm directional key and small analytic contact shadows. There are no cast Unity shadows in this view. Hardware sits on separate depth planes; the gold receiving rim surrounds a shallower ivory inset and individual target sockets. The raster backdrop is an unlit cover plane behind the mechanism.

The mesh builder uses five contour rings, with scale factors `0.88, 0.985, 1, 0.97, 0.89`; that rounded extrusion is the reusable edge vocabulary. Shared shape plastics use smoothness 0.28 and metallic 0.02. Other materials retain their individually authored satin values rather than a universal gloss preset. The key light is warm, intensity 1.05, and the ambient light is flat.

### Shadow Vocabulary

- **Soft contact:** a camera-facing quad using the Contact Shadow token. Alpha follows `(1 − smoothstep(0.25, 1, radius))²`; this creates a soft elliptical contact when the quad is compressed. Tokens, platforms and receiver each size their own contact plane. This is a shader effect, not a CSS box shadow.
- **Selection lift:** the body and live glyph share a normalized vertical center of 0.38 at rest and 0.48 when selected. Body size moves from 0.78 × 0.88 to 0.81 × 0.915 of its cell, with a gold frame behind it. Selection eases out over 100 ms; Reduced Motion applies the focus state directly.

The live wind wheel turns at 7 degrees per second from an 18-degree base angle. Reduced Motion fixes it at that base angle; pause and application suspension stop its phase progression. The painted clouds do not move.

### Causal Motion

The authored focal sequence is a whole row leaving a spent mint rail, travelling
down its brass guides, and seating in the next rail or gold receiver. Supporting
motion explains adjacent/linked exchanges and local device consequences. The
event player reads already committed results; visual completion cannot apply
damage, collect a stamp or execute another command.

`MotionProfile` sets selection 100 ms, exchange 190 ms, contact 180 ms, bomb
240 ms, fracture 130 ms, fall 300 ms, landing 110 ms and delivery hold 340 ms.
Exchange uses cubic ease-out with short opposing arcs; fall uses smoothstep.
Landing compresses plastic by at most 8%; rails recoil locally by 0.025 world
units. The camera does not shake. A target stays visible throughout delivery;
result controls appear only after the row settles.

Reduced Motion replaces travel, recoil and deformation with 55 ms state holds.
Dust reduces to at most two stationary fades per burst. Normal dust is capped
at 32 pooled elements; sound has eight pooled voices. Their tails do not keep
the command gate closed. Original mallet-like cues and a quiet 24-second loop
share the workshop palette; `Docs/P13_AUDIO_PROVENANCE.md` records their source
and the untested auditory/device limits.

## Shapes

Six original soft silhouettes are extruded into tokens: rounded triangle, circle, rounded square, rounded diamond, rounded star and hexagon. Letter tiles reuse the rounded-square contour. Rail and receiver parts reuse a long rounded beam; circular disks form mounts, caps and hinges. The square/tile source contour is one mesh unit wide with a 0.18-unit corner radius; the beam source is 6 × 1 units with a 0.3-unit corner radius. These are mesh coordinates, not UI pixels.

Buttons use the authored 64 × 64 nine-slice panel raster. The `panel-source` token describes its 13-source-pixel circular corner construction; the sprite has 22-pixel slice borders. It does not establish a 13-pixel radius for every world object. Direction controls use an authored six-vertex chevron mesh, mirrored for the opposite direction, rather than font symbols.

## Components

### Buttons

Warm soft panels with dark Bold labels. Utility controls use UI Paper; Drop/Deliver uses Drop Gold. Text margins are 3 units inline and 2 units vertically. Standard action bands retain the 48-unit compensated height. Hover, pressed and disabled appearances are the multiply tints described in Colors. The current Unity button selected state keeps its default tint; a separate custom keyboard-focus treatment has not been established.

Labels expose the current action: Drop becomes Deliver when the final-floor row is ready, Hint becomes Use hint after a valid offer, and Pause becomes Resume while paused. Disabled states reflect current command availability. Do not describe prototype Previous/Next level browsing or the current Motion utility as the completed P14 navigation system.

### Persistent token and selection

A shape keeps its material identity; a glyph uses an ivory body with a centered Bold face. Selection raises the body and glyph together and reveals the gold frame. The gold translucent cell wash denotes affected edges during preview. The native view reads committed state and never resolves gameplay.

### Platform and attached devices

A thin mint beam carries an ivory top rail and two gold end caps. Durability is attached below its left edge, with device explanation below/right. Up to three graphite seams indicate damage. Bombs, keys, repair devices, stamps, locks, fragile notches, linked loops and one-use bridges are original assemblies of the same shared meshes and material vocabulary. Their required-symbol faces use the common symbol system. Device visibility follows committed state.

### Receiver

The gold rim, ivory inset and gold sockets form a stable destination under the floors. The complete order remains visible with the same shape/glyph identities. The label changes on delivery; the target is inspectable through the magnifier. On loss, the current implementation additionally shows the delivered row below the target. Full terminal-flow composition remains P14 work.

### Precision controls

Two authored-chevron buttons change the focused slot; a central label names its position and symbol. Left/right exchange controls below them expose durability cost or the reason an exchange cannot change the row. Both rows have independently measured touch regions. These controls are especially important for seven/eight-symbol rows at narrow widths.

### Target magnifier

A full safe-area UI Paper surface pauses interaction and displays the exact ordered symbols in a four-column arrangement with position numbers. Shape faces derive from the same mesh contours using `ShapeFaceGraphic`; letters remain live Bold text. It is a gameplay inspection surface, not a general card or modal style for future product flows.

## Do's and Don'ts

### Do:

- **Do** reuse the same authored silhouette and material wherever a SymbolId appears.
- **Do** move the live glyph and its body together when a token is selected.
- **Do** retain the complete destination in the stable gold receiver below the remaining floors.
- **Do** measure controls and device labels at the supplied small Editor reference size after layout changes.
- **Do** use authored chevron geometry for precision focus controls.
- **Do** keep ambient motion view-only and stop its phase progression during pause or application suspension.
- **Do** preserve original assets, licenses, provenance and Unity meta identities.

### Don'ts:

- **Don't** turn compact support rails into full-width rectangular cards.
- **Don't** bake gameplay text into the painted background or token artwork.
- **Don't** use color changes to replace the stable shape identities.
- **Don't** let animation completion, scenery, physics or presentation callbacks determine gameplay state.
- **Don't** copy ShapeMania screens, characters, sprites, props or materials into this world.
- **Don't** treat the temporary P11 presentation gate or P12 utility navigation as the completed P13/P14 experience.
- **Don't** claim physical-device validation, native-language editorial approval, measured enjoyment or approved-mockup fidelity from these Editor captures.
