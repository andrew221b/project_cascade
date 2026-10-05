# CASCADE Journey art — provenance and review

Created 5 October 2026 for `Docs/JOURNEY_REDESIGN.md`. The asset-only producer followed the established PRODUCT/DESIGN world. These are original game assets, not screenshots, UI mockups, or copies of ShapeMania artwork.

## Production and scope

- Built-in `image_gen` only; 12 accepted PNG assets from 14 calls (two targeted repairs). No CLI/API fallback, external source art, or API key.
- The existing original `CascadeArt/SkyWorkshop.png` was inspected before use as the Wind Garden base edit target. It is preserved unchanged. WindGardenMap became the composition reference for the two regional maps.
- Generated source files remain at the source paths below. Selected images were copied into `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/` without overwriting prior assets. No resizing, trimming, alpha extraction, or other pixel edits were applied to shipping files.
- Each shipping PNG embeds the exact final tool prompt under `impeccable:prompt`, written with `/Users/andrew/.codex/plugins/cache/openai-curated-remote/impeccable/4.3.1/skills/impeccable/scripts/impeccable embed-prompt`. A scan found **12 rasters, 0 missing**. All embedded strings were compared with the prompts below.
- Native alpha was preserved. Mika and Glider have a generated alpha maximum of 254 (approximately 99.6% opacity); the other four cutouts reach 255. Their transparent background pixels contain nonzero RGB data, but those pixels are invisible when properly alpha-composited. White/cream paint and clear gaps remain intact.
- All 12 outputs were visually inspected. The six cutouts were additionally inspected over pale ivory and dark blue grounds using temporary diagnostic composites; those composites are not shipping assets. No repair was needed for their alpha edges.
- A follow-up halo check composited Mika, Pip and Rue over opaque pale blue `#B8DEEC`. The three full-resolution diagnostic previews are saved as `Reports/P14/JourneyArt/Mika-pale-blue-alpha-check.png`, `Pip-pale-blue-alpha-check.png` and `Rue-pale-blue-alpha-check.png`. No broad baked halo or vignette is visible after correct alpha blending. The character sources were not changed; no `MikaClean.png` is needed. These diagnostic rasters embed their source/operation provenance rather than a generation prompt.
- This producer did not edit code, scenes, import settings or `.meta` files and did not run Unity MCP. Root owns integration and TASKS updates. Sprite cropping, native UI contrast, safe-area/header fit, map-marker registration, reduced-motion behavior, device memory/performance and human enjoyment remain to be verified in the integrated game.

## Accepted asset inventory

| Shipping PNG | Size | Format | Fully transparent pixels | Role |
| --- | --- | --- | --- | --- |
| `Mika.png` | 1024 × 1536 | RGBA; alpha 0–254 | 45.84% | Full-body hero cutout; dialogue and map marker |
| `WindGardenBase.png` | 862 × 1825 | RGB; opaque | — | Far sky, lower-corner distant islands |
| `Cloud.png` | 1774 × 887 | RGBA; alpha 0–255 | 63.16% | Reusable drifting cloud group |
| `GardenPlants.png` | 1254 × 1254 | RGBA; alpha 0–255 | 57.66% | Left-rooted corner foliage; may be mirrored |
| `Glider.png` | 1536 × 1024 | RGBA; alpha 0–254 | 76.06% | Small distant animated prop |
| `RainMillBase.png` | 910 × 1728 | RGB; opaque | — | Far sky and distant wooden mill |
| `CopperOrchardBase.png` | 862 × 1825 | RGB; opaque | — | Peach sky and distant copper orchard |
| `WindGardenMap.png` | 863 × 1823 | RGB; opaque | — | Five connected garden destinations |
| `RainMillMap.png` | 863 × 1823 | RGB; opaque | — | Five connected mill destinations |
| `CopperOrchardMap.png` | 863 × 1823 | RGB; opaque | — | Five connected orchard destinations |
| `Pip.png` | 1199 × 1312 | RGBA; alpha 0–255 | 39.75% | Frog mechanic dialogue bust |
| `Rue.png` | 1194 × 1317 | RGBA; alpha 0–255 | 42.11% | Fox orchard keeper dialogue bust |

Background aspect ratios are the actual built-in outputs: Wind Garden and Copper Orchard are close to 9:19; Rain Mill is wider at 910×1728. Use cover fitting in Unity and verify corner landmarks after crop. Do not claim every output has an exact 9:19 pixel ratio.

## Map landing guidance

The positions below are approximate visual landing centers, normalized from the **top-left of the full raster**; they are implementation guidance, not tested UI registration. Map cover fitting must transform them with the same scale and crop as the art.

| Map | Stop 1 | Stop 2 | Stop 3 | Stop 4 | Stop 5 |
| --- | --- | --- | --- | --- | --- |
| WindGardenMap | (0.25, 0.705) shed | (0.59, 0.602) winch | (0.40, 0.493) pump | (0.65, 0.403) lift | (0.81, 0.290) beacon |
| RainMillMap | (0.26, 0.709) workshop | (0.58, 0.602) sluice | (0.39, 0.493) waterwheel | (0.65, 0.398) lift | (0.81, 0.267) mill |
| CopperOrchardMap | (0.26, 0.723) cottage | (0.59, 0.627) winch | (0.35, 0.507) press | (0.63, 0.409) basket lift | (0.81, 0.275) lantern tower |

Wind Garden has about 16% clear space above destination objects and 20% below. Rain Mill has about 17% above and 18% below. Copper Orchard's highest finial begins near 15%, satisfying the requested header band approximately but not the stricter 18% inset in its prompt; verify header overlap in Unity. The maps intentionally contain static bridges and small landmark machinery. Their stop count is an illustrated five-stop view, not a claim of five or 160 implemented campaign puzzles.

## Exact tool prompts, inputs and review

### Mika.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/Mika.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-53df8610-f9e0-42b6-82fd-f9e079c9e4eb.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `true`
- Image inputs: none; generated from the prompt.
- Review: Full body clear; scarf repair, canvas bag and lantern readable. Light composite clean. Ears rendered somewhat larger than brief; accepted for silhouette and expression. No repair required.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: illustration-story
Asset type: shipping transparent character cutout for CASCADE, an original cozy mobile puzzle game; one character only, not a concept sheet.
Primary request: Mika, a small dormouse apprentice lighthouse keeper, full body in a readable three-quarter view facing slightly right. Warm brown fur, cream muzzle and belly, small subtly uneven round ears, dark kind eyes and a sympathetic determined expression. Mint patched overalls, a worn saffron scarf with one visible stitched repair, simple little shoes, a canvas shoulder bag, and a small brass lantern held securely at his side. His other hand lightly holds his bag strap. Short cozy proportions and a warm furry tail that fits in the silhouette; capable and imperfect, neither a baby nor a generic congratulating mascot.
Style/medium: original bright tactile hand-painted storybook toy illustration, softly sculpted forms with painterly detail, warm brass and satin mint enamel, crisp readable large shapes. Distinctive original character design. Soft upper-left sunlight and restrained shaded edges.
Composition/framing: full body from ear tips to feet fully visible, centered with generous transparent margin, approximately portrait 2:3 canvas. Clear silhouette that reads both at small map-marker size and enlarged dialogue portrait size. Lantern separated enough to read but not a dangling isolated object. Natural paws and coherent anatomy.
Background: actual alpha transparency, no background, no ground plane, no drop shadow, no scenery.
Constraints: no words, letters, logos, watermark, UI, speech bubbles, numbered elements, multi-view sheet, frame or copied character. Preserve white and cream paint inside the silhouette; clean soft antialiased alpha around fur.
```

### WindGardenBase.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/WindGardenBase.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-ccd8f797-fa6e-45b4-8e06-36ec31fd8900.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `false`
- `referenced_image_paths`: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/SkyWorkshop.png`
- Review: Quiet blue/cream sky; foreground clutter removed; small landscape confined to extreme lower corners. Opaque. Accepted.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: precise-object-edit
Asset type: shipping opaque portrait far-background plate for CASCADE Wind Gardens, approximately 9:19 portrait, full-frame art.
Input image 1: the existing original CASCADE SkyWorkshop illustration. It is a palette, light and material reference and an edit target to simplify, not a whole scene to preserve unchanged.
Primary request: turn this into a CALM FAR SKY LAYER. Preserve the original bright sky-blue to warm-cream light gradient, soft painterly toy-world rendering and warm upper-left illumination. Keep only tiny very distant floating workshop islands around the extreme lower left and right edges. A hint of a terracotta workshop roof may appear at the outermost bottom-left, and small mint-topped distant rock islands at outermost bottom-right. The central 75 percent of the image width and entire central height must be open quiet sky; let the sky extend to every edge above the low distant islands.
Remove completely: ALL clouds and cloud wisps; the glider; the foreground leaves, vines, blossoms and plants; the wind wheel; the giant top-left beam, sail and bracket; any other foreground object. Do not replace removed objects with new props. Do not paint a wind wheel anywhere.
Composition: a calm spacious sky that will sit behind a gameplay machine. Tiny far landscape detail occupies only the outer edges within the bottom 15 percent of the frame, never across the bottom center. Cloud, plants, glider and wheel will be separate moving runtime layers.
Style/medium: bright original hand-painted storybook toy landscape, softly sculpted edges and gentle brush texture. Soft blue/cream, distant sage mint and muted terracotta; low-contrast, atmospheric edges.
Constraints: opaque background, no alpha; no text, logo, UI, frame, character, grid, game tokens, platforms, mechanical rails, photorealistic scenery, or visible cloud objects. Keep the useful quiet space as the dominant composition.
```

### Cloud.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/Cloud.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-6d7578cb-162e-4e85-a823-8772fddd43b8.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `true`
- Image inputs: none; generated from the prompt.
- Review: Broad irregular cream cloud; warm top light and blue undersides; no sky plate. Accepted; clean edges and preserved pale paint on both light and dark diagnostic grounds.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: illustration-story
Asset type: shipping transparent scenery cutout for CASCADE Wind Gardens; one reusable cloud group on a landscape canvas approximately 2:1.
Primary request: an irregular fluffy group of three softly overlapping cream-white cumulus puffs, broad and low with one higher rounded lobe to the left and smaller wispy ends. A single natural cloud silhouette, not a row of repeated circles. Soft warm upper-left sunlight, pale peach-gold highlights and restrained pale blue undersides. Bright tactile hand-painted storybook toy-world style with painterly cotton-soft modeling, broad readable masses and subtle brush texture. Calm and friendly.
Composition/framing: entire cloud group isolated and fully visible, generous empty margin on all four sides. Balanced low horizontal footprint, useful for drifting past the outside edges of a portrait game screen and wrapping offscreen. No directional action or perspective.
Background: ACTUAL ALPHA TRANSPARENCY around the cloud and in any open gaps. Keep cream/white paint in the cloud itself, softly antialiased outer edge. No sky, no ground, no rectangular backdrop, no cast shadow, no scenery, no sun, no particles, no face, no rain.
Constraints: one standalone original cloud group, no text, watermark, UI or frame. Do not create an atlas or multiple disconnected cloud variants.
```

### GardenPlants.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/GardenPlants.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-b12d9564-b9ef-4bcb-b796-0ba97a4816b9.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `true`
- Image inputs: none; generated from the prompt.
- Review: Triangular left-rooted mint/sage foliage, three ivory blossoms and clear stem gaps; no pot/ground/sky. Accepted; clean edges and preserved pale paint on both light and dark diagnostic grounds.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: illustration-story
Asset type: shipping transparent foreground plant cutout for CASCADE Wind Gardens, one original cluster on a square canvas.
Primary request: a lively but elegant wind-garden cluster of broad softly curled mint, sage and teal leaves with three small warm-ivory and pale-saffron blossoms. Leaves have hand-painted central veins, soft golden upper-left light and rounded tactile toy-like forms. Stems converge toward a low root point near the bottom-left of the asset, and foliage rises upward and gently right. Taller leaves at the left, smaller trailing leaves toward the right; this can be mirrored for the opposite screen edge. Friendly breezy silhouette, organic spacing, clear gaps between some stems and leaves.
Style/medium: original bright hand-painted storybook toy illustration, softly sculpted leaf surfaces, subtle paint brush texture, warm light, no photorealistic foliage. Palette matches a blue/cream sky world with mint enamel and warm brass props.
Composition/framing: entire cluster fully visible with transparent margin. Roots/lowest cut stems near bottom-left but not clipped by canvas. Roughly triangular composition rising on the left, tapering to the right. A readable silhouette intended to occupy a lower screen corner and sway gently as one piece around the base.
Background: actual native alpha transparency outside leaves, blossoms and stems and inside their clear gaps. NO ground, soil, pot, planter, island, sky, clouds or cast shadow. No loose detached leaves or extra plants elsewhere.
Constraints: no characters, letters, text, logo, watermark, UI, frame or atlas; one standalone cluster only. Preserve the ivory blossoms as opaque paint.
```

### Glider.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/Glider.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-eaa9b2c3-1af4-4a0a-8229-4be1446ad291.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `true`
- Image inputs: none; generated from the prompt.
- Review: Ivory/mint cloth wing, brass supports and empty harness. Full silhouette retained; no pilot. Clean edges and preserved canvas on both light and dark diagnostic grounds.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: illustration-story
Asset type: shipping transparent glider scenery cutout for CASCADE, one original small glider on a landscape canvas around 3:2.
Primary request: a charming handcrafted ivory-and-mint cloth hang-glider with a delicate brass frame, a small central keel and simple suspended empty harness. Viewed in a clear three-quarter side view gliding slightly upward toward the right. A wide gently swept triangular wing is made of warm ivory canvas with two restrained mint cloth panels and visible stitched seams. Brass curved leading edge, coherent slender support struts and a few taut rigging lines. No pilot or character.
Style/medium: original bright tactile hand-painted storybook toy-world illustration; soft sculpted light on brass, canvas brush texture, warm upper-left sunlight, calm craftsmanship. Rounded friendly design readable when tiny, limited fine detail. Use mint, ivory, warm brass and restrained brown rope only.
Composition/framing: entire glider with wing tips, frame and harness fully visible, centered with generous margin on all sides. Clean distinctive silhouette. One glider, no other objects. It will move as one small sprite in the distant sky.
Background: actual alpha transparency with clear gaps between supports. No sky, cloud, wind line, motion trail, floor or cast shadow. Preserve opaque ivory canvas, clean antialiased edges.
Constraints: no person, animal, words, letters, logos, numbers, watermark, UI, frame, multi-view sheet or mechanical wheel.
```

### RainMillBase.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/RainMillBase.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-1458c175-e01e-4e2f-ac39-e7fc28a590f2.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `false`
- `referenced_image_paths`: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-eca04291-d4f4-4cf2-8ba0-334cef710e68.png`
- Initial source retained but not shipped: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-eca04291-d4f4-4cf2-8ba0-334cef710e68.png`
- Review: Initial generation included a cloud bank; one repair removed baked clouds. Accepted plate retains wooden mill and waterfall islands at outer lower edges, quiet middle and teal/cream gradient. Watermill wheel is intentional distant regional architecture, distinct from runtime foreground windwheel.

Initial generation prompt (the first output required one targeted repair):

```text
Use case: illustration-story
Asset type: shipping opaque portrait far-background plate for CASCADE Rain Mill region, 9:19 portrait composition, full-frame illustrated landscape with a very quiet center.
Primary request: bright calm teal-blue misty sky fading into warm cream light near the bottom, in an original floating-island storybook toy world. Only at the extreme lower left edge, a small distant wooden watermill sits on a mint-green floating rock island; its warm honey timber body, quiet teal roof and little brass-edged water wheel are visible in atmospheric perspective. Only at the extreme lower right edge, a second smaller floating island with a narrow falling ribbon of water. Distant architecture and landform occupy no more than the bottom 15 percent and outermost edges. The central 75 percent of the image must remain uninterrupted softly painted sky, all the way down through the bottom center. Keep the top 85 percent essentially open sky.
Style/medium: bright tactile hand-painted storybook toy illustration, soft sculpted geometry, subtle brush texture, warm upper-left sunlight through gentle teal atmospheric haze. Limited palette of teal, sky-blue, mint, ivory, honey wood and small brass accents. Friendly, airy daylight; not dark or stormy. Consistent soft warm light and crafted materials as CASCADE Wind Gardens.
Constraints: opaque RGB full frame, no text, logo, watermark, UI, border, grid, numbers or characters. No large foreground plants, leaves, glider, wind wheel, foreground clouds or discrete cloud puffs; cloud motion will be a separate runtime layer. Do not fill the central sky with scenery or frame it with tall objects. Do not make a diorama platform in the center; this is a very distant backdrop behind a gameplay machine.
```

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: precise-object-edit
Asset type: CASCADE Rain Mill shipping far-background plate, opaque portrait.
Input image 1 is the Rain Mill plate to repair. Keep its exact portrait framing, wooden watermill on the lower-left floating island, lower-right island and waterfall, warm upper-left light, teal-to-cream palette and hand-painted toy-world style.
Change only the atmosphere: REMOVE ALL baked clouds from the entire image, including the large cream cloud bank at the bottom and all wispy white cloud strokes at the sides and top. Replace them with a smooth gently brushed teal-blue to warm cream SKY GRADIENT. No distinct cloud shapes anywhere. Keep the middle 75 percent clear and empty through the bottom center. The waterfall ribbons may fade into subtle uniform haze below their island, but do not add cloud puffs or a sea of clouds. Keep the two small far landscape islands only at the extreme lower outer edges. Reduce any central distant mountains to near-invisible haze so gameplay has an uninterrupted quiet sky behind it.
Constraints: no new props, no text, no characters, no foreground leaves, no glider, no windwheel, no UI. This is the fixed far-sky layer; all clouds will be separately animated at runtime, so absolutely no baked cloud objects.
```

### CopperOrchardBase.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/CopperOrchardBase.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-3d028558-0620-4cce-ba23-419bd8fda97d.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `false`
- Image inputs: none; generated from the prompt.
- Review: Peach/cream quiet sky and copper trees on distant lower corner islands. No foreground clouds. Accepted.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: illustration-story
Asset type: shipping opaque portrait far-background plate for CASCADE Copper Orchard region, approximately 9:19 portrait, full-frame illustrated background.
Primary request: a calm sunset sky with muted peach high above, soft apricot at the edges and luminous warm cream through the center and lower center. Tiny floating orchard islands appear only at the extreme lower-left and lower-right corners. Their rounded faraway tree canopies are copper, rust and golden leaf colors with slender warm brown trunks, sage moss and pale rock underneath. One tiny orchard shed with a muted mint roof may be tucked into the far lower-left edge. These distant silhouettes are small, atmospheric and never compete with gameplay.
Composition: uninterrupted quiet painted sky fills the top 85 percent and central 75 percent of the width, continuing through the bottom center. Small distant orchard islands occupy only the outermost lower corners. Airy and spacious, no horizon line across the image, no center island.
Style/medium: original bright tactile hand-painted storybook toy landscape; softly sculpted tree/rock shapes, restrained brush texture, warm upper-left sunlight. Friendly early evening palette, not dark; muted copper details and peach/cream sky, gentle mint accents. Consistent crafted material and lighting language with CASCADE Wind Gardens and Rain Mill.
Constraints: opaque full frame. No baked clouds anywhere: no puffs, wisps, cloud bank or sea of clouds. Use smooth softly brushed sky with subtle atmospheric haze only. No foreground leaves, plants, glider, windwheel, characters, text, UI, logo, watermark, border, numbered path, game board or mechanical platforms. Cloud and plant motion will be separate runtime layers.
```

### WindGardenMap.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/WindGardenMap.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-2bf82bc4-4e99-4a8d-8fac-80826f556418.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `false`
- `referenced_image_paths`: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-97cdc80d-ad68-45c8-a46b-e45c24c0c9a8.png`
- Initial source retained but not shipped: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-97cdc80d-ad68-45c8-a46b-e45c24c0c9a8.png`
- Review: Exactly five connected foreground stop islands: shed, winch, pump, lift and beacon. One repair reduces route footprint and removes extra distant islands. Top ~16% and bottom ~20% have no destination objects; clouds remain decorative edge haze. No baked text, UI or character. Accepted.

Initial generation prompt (the first output required one targeted repair):

```text
Use case: illustration-story
Asset type: shipping original illustrated campaign MAP BACKDROP for CASCADE Wind Gardens, opaque portrait 9:19 mobile composition. This is game-world scenery with no UI or text.
Primary request: FIVE DISTINCT floating garden islands forming an obvious connected journey from the lower-left toward the upper-right, seen in a consistent isometric three-quarter overhead view. Small warm brass repair machines and short timber-and-mint footbridges make the route readable. Mint grass terraces, rounded sage foliage, tiny ivory and saffron blossoms, warm pale-stone rock undersides and little terracotta workshop roofs. The five stop islands differ in role: first low-left tiny keeper shed and brass lantern post; second slightly higher/right a little broken footbridge repair winch; third centered small brass garden pump with water trough; fourth higher/right a compact mint-and-brass lift; fifth upper-right a small ivory beacon tower with a brass cap. Link the islands in that order with narrow rope and timber bridges. Five destinations total, no extra islands resembling stops. Clearly separated silhouettes and open sky gaps between islands.
Composition: tall 9:19 image. Keep TOP 15% and BOTTOM 18% as calm blue/cream sky for future overlay controls. Arrange the five islands within the middle band, stepping upward diagonally with a gentle readable stagger: start around x25%, y75%; next x52%, y63%; middle x36%, y50%; next x65%, y38%; beacon x72%, y24%. Each island about 22–28% of image width; connect them with a visible continuous path while keeping all five distinct. Keep little open landing patches beside each machine for runtime markers. No large foreground objects.
Style/medium: original bright tactile hand-painted storybook toy diorama map, softly sculpted forms, clear broad shapes and subtle painterly material texture. Consistent isometric perspective and warm upper-left sunlight; mint, warm brass, pale stone, ivory, saffron, tiny coral accents against clear sky-blue/cream. Polished friendly craftsmanship, calm airy scene.
Constraints: NO text, letters, numbers, numbered circles, badges, pins, markers, drawn UI buttons, dotted-line UI route, frame, border, watermark, characters or people. No copied artwork. Do not render a phone or a screen mockup. Entire output is illustrated map scenery only. Clouds may be a faint distant edge haze only, never hiding bridges or destinations.
```

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: precise-object-edit
Asset type: CASCADE Wind Gardens shipping campaign map backdrop.
Input image 1: existing five-stop Wind Gardens map. Keep its exact original art style, five main foreground islands, their roles, route order, warm upper-left light, isometric angle, mint/brass/ivory materials and blue/cream palette.
Change only composition and distant clutter: scale down the complete five-island connected route slightly and fit ALL of it into the vertical middle band from y=18% to y=78% of this portrait image. The highest tip of the beacon must be below y=18%, and the lowest rock point of the starting shed island must be above y=78%. Top 15% and bottom 18% must be quiet open blue/cream sky for overlay controls. Preserve the recognizable alternating route from lower-left to upper-right, all five bridges/stop relationships, and clear open landing patches beside the props. There are five main destinations: shed, winch, pump, lift, beacon.
Remove ALL unrelated distant floating islands from the surrounding sky so they cannot look like extra stops. Remove the cream foreground cloud bank along the bottom and reduce remaining clouds to very subtle outer-edge haze. Do not add new objects. Keep the five main islands clearly separated, with a generous continuous sky gap around the group.
Constraints: full-frame opaque portrait map art, no text, numbered circles, markers, characters, UI, border, logo or watermark.
```

### RainMillMap.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/RainMillMap.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-c76d7159-1ddc-442c-bd62-269e8fe1922d.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `false`
- `referenced_image_paths`: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/WindGardenMap.png`
- Review: Five connected stop islands match the Wind Garden route; workshop, sluice, waterwheel, lift, watermill. Teal sky and honey timber; bright waterfalls. Quiet top ~17%, bottom ~18%. No UI or characters. Accepted.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: style-transfer
Asset type: shipping original CASCADE Rain Mill campaign MAP BACKDROP, opaque portrait 9:19, scenery only.
Input image 1 is the CASCADE Wind Gardens map, used as an exact composition, perspective, material and lighting reference. Keep the five-island zigzag route topology, all five separate stop footprints, bridge connections, isometric view, quiet top/bottom areas and original tactile storybook quality. Transform every destination to the Rain Mill region; do not keep its garden-specific beacon/shed/pump designs literally.
Primary request: five floating mossy pale-stone islands with wet teal grass terraces, honey wood, teal-painted roofs, brass repair mechanisms and very small cream flowers. Start lower-left with Pip's squat wooden waterside workshop and a little brass tool rack; second stop slightly higher-right has a compact sluice-gate winch over a narrow turquoise channel; middle-left stop holds an elegant small wooden water wheel and a brass axle bracket; fourth higher-right stop is a little mint-and-brass lift with timber decking; final upper-right stop is a warm wooden watermill cottage with a modest teal roof and its water wheel. Tiny waterfall ribbons descend from two islands and dissolve into teal mist. Link all five with visible narrow timber-and-rope bridges in the same route as the reference. Exactly five destination islands, no unrelated distant islands. Clear landing patches beside props for runtime markers.
Composition: the complete route, every rooftop and every rock tip fits BETWEEN y=18% and y=79% of the portrait. TOP 15% and BOTTOM 18% must remain quiet teal-blue/cream open sky. Keep clear sky gaps around and between the islands, and retain the reference's approximate destination positions. Soft teal atmospheric sky with warm cream lower light, no dark storm clouds.
Style/medium: original bright hand-painted storybook toy diorama map, broad readable shapes, gently sculpted timber and pale stone, restrained painterly texture, consistent warm upper-left sunlight and isometric angle. Mint, teal, honey wood, warm brass, ivory.
Constraints: no characters, text, letters, numbers, badges, numbered circles, pins, markers, UI, border, logo or watermark. No extra stop islands. No foreground cloud bank; faint edge haze only, never obscuring route or props. Do not produce a screen mockup or phone frame.
```

### CopperOrchardMap.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/CopperOrchardMap.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-c34f1cd0-2214-44af-9376-50617935c479.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `false`
- `referenced_image_paths`: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/WindGardenMap.png`
- Review: Five connected stop islands match route; cottage, harvest winch, cider press, basket lift, lantern tower. Copper foliage and peach sky. Highest tower finial begins near 15% instead of stricter 18% prompt inset; bottom ~18% quiet. Accepted for requested 15% header clearance, pending native layout review.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: style-transfer
Asset type: shipping original CASCADE Copper Orchard campaign MAP BACKDROP, opaque portrait 9:19, scenery only.
Input image 1 is the CASCADE Wind Gardens map, used as the exact route composition, perspective, tactile material and light reference. Keep its five separate island footprints, zigzag path topology, all four bridge connections, consistent isometric view and generous quiet header/footer areas. Transform every destination to the Copper Orchard region while preserving the shared bright handcrafted storybook world.
Primary request: FIVE floating orchard islands with warm pale-stone undersides, sage moss, rounded copper/russet/gold tree canopies, warm honey timber, muted mint painted hardware and brass repair mechanisms. Start lower-left with Rue's tiny orchard cottage under a copper-leaf tree and a small fruit crate; second slightly higher-right has a small brass winch serving a short timber harvest bridge; third middle-left has a charming brass-and-wood cider press with two barrels; fourth higher-right has a compact mint-and-brass basket lift among two copper trees; final upper-right has a little ivory signal lantern tower with a copper roof and a curved orchard gate. Do not use readable signs. Connect all five with visible narrow rope-and-timber bridges in the same order as the reference. Exactly five destinations and no extra distant floating islands. Leave an open landing patch beside every landmark for runtime markers.
Composition: every rooftop, tree tip and rock point of the complete route fits within the vertical middle band y=18% to y=79% of this portrait. TOP 15% and BOTTOM 18% remain calm peach/cream sky with no destination objects. Keep the reference route's approximate stop positions and clear sky gaps. Sunset peach/apricot outer sky with warm cream center, airy and friendly, no dark purple night.
Style/medium: original bright tactile hand-painted storybook toy diorama, gently sculpted canopies and pale rock, broad clear shapes, restrained brush texture, consistent isometric angle and warm upper-left sunset light. Copper foliage, sage, ivory, warm brass and honey wood with small mint accents.
Constraints: no characters, text, letters, numbers, numbered circles, badges, pins, markers, UI, border, logo or watermark. No foreground cloud bank or large leaves; minimal soft edge haze only. Do not create a phone frame or a screen mockup. This is full-frame illustrated map scenery.
```

### Pip.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/Pip.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-a7e55aeb-0180-4d15-833f-7b63e1ab48cd.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `true`
- Image inputs: none; generated from the prompt.
- Review: Stocky kind frog with blue apron, cream shirt and wooden pencil at eye ridge (frog has no invented mammal ears). Patient grin, coherent hand. Natural bust crop below apron. Accepted; clean edges and preserved pale paint on both light and dark diagnostic grounds.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: illustration-story
Asset type: shipping transparent NPC bust portrait for dialogue in CASCADE, one original character named Pip.
Primary request: Pip is a kind stocky adult frog watermill mechanic. A friendly moss-green frog with a cream throat, broad rounded head, softly raised gold-brown eyes and a patient gentle grin. Blue work apron in faded deep sky-blue canvas with a small stitched pocket over a cream rolled-sleeve work shirt. A short wooden pencil is tucked just behind the side of his head near the eye ridge, suggesting the familiar behind-the-ear mechanic gesture without inventing mammal ears. A few soft freckle-like frog spots, rounded reliable shoulders, one hand lightly resting at his apron strap. His body turns slightly right, his gaze meets someone just right of the viewer. Reassuring, capable, thoughtful; not comic slapstick.
Style/medium: original bright tactile hand-painted storybook toy character art, softly sculpted rounded forms, warm upper-left sunlight, subtle painterly skin and woven-cloth texture, expressive eyes and restrained details. Coherent with a mint/brass/ivory floating-island workshop world and a warm brown dormouse hero with patched mint overalls and saffron scarf. No resemblance to a known franchise frog.
Composition/framing: head, shoulders, upper torso and near hand visible, portrait crop ending naturally at lower torso/apron. All sides of the character silhouette have generous transparent margin; nothing clips the head, pencil, shoulders or hand. Strong readable face for small dialogue portrait size. One single bust only.
Background: true native alpha transparency, no sky, backdrop, ground, oval portrait frame, cast shadow or painted vignette. Keep cream shirt and throat opaque; clean antialiased edges.
Constraints: no text, letters, logo, watermark, UI, speech bubble, other characters, props unrelated to the pencil/apron, multiple poses or atlas.
```

### Rue.png

- Shipping: `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Journey/Rue.png`
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10abe-bf15-7e52-84a4-20c404f06c01/exec-19bc2b20-8e8c-4e9c-a854-04b5417f1a9a.png`
- Tool: `image_gen.imagegen`
- `transparent_background`: `true`
- Image inputs: none; generated from the prompt.
- Review: Russet fox with graying brow, warm knowing gaze, moss knitted shawl and copper-leaf brooch. Natural bust crop, no frame. Ear has a narrow top margin; no clipping of visible silhouette. Accepted; clean edges and preserved pale paint on both light and dark diagnostic grounds.

Exact selected-output prompt, also embedded in the shipping PNG:

```text
Use case: illustration-story
Asset type: shipping transparent NPC bust portrait for dialogue in CASCADE, one original character named Rue.
Primary request: Rue is an elderly small russet fox orchard keeper with a warm knowing expression. Soft russet-orange fur, a cream muzzle and cheek ruff, gently graying eyebrow and ear-edge fur, kind amber eyes with fine age lines, small weathered dark nose and a subtle closed-mouth smile. A simple moss-green knitted shawl wraps around her shoulders and fastens with a copper-leaf brooch. Cream modest blouse below. Her compact shoulders turn in three-quarter view toward the right; she looks gently at someone just right of the viewer. One relaxed paw lightly touches the shawl near its fastening. Fox ears fully visible and asymmetrically alert, dignified and kind, visibly older without caricature.
Style/medium: original bright tactile hand-painted storybook toy character illustration, softly sculpted furry forms, warm upper-left sunlight, subtle painterly fur and knitted fabric texture. Large readable friendly facial shapes and restrained detail. Coherent with a mint/brass/ivory floating-island workshop world, a warm brown dormouse in patched mint overalls and saffron scarf, and a patient frog mechanic in a blue apron. Distinct original character, no franchise resemblance.
Composition/framing: one bust with head, shoulders, upper torso and one paw visible; crop ends naturally below the shawl. Leave transparent margins around ear tips and both shoulders, no clipping of the face, ears, brooch or paw. Clear silhouette and readable expression for a small dialogue portrait.
Background: actual native alpha transparency, no painted backdrop, sky, ground, cast shadow, portrait oval, frame or vignette. Preserve cream fur/blouse as opaque paint and natural clean antialiased fur edges.
Constraints: no words, letters, logo, watermark, UI, speech bubbles, other characters, hats, handheld tools, multiple poses or atlas.
```

