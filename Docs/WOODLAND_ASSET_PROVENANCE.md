# Woodland asset provenance

Original CASCADE raster extension produced 5 October 2026 with the built-in
`image_gen` tool under the imagegen skill and Impeccable asset-production
workflow. The author delegated visual choice and authorized the needed original
assets. This is an extension of the established cartoon toy storybook world,
with no new identity exercise or approved composite screenshot.

## Sources and preservation

Before generation, inspected the actual local Journey assets
`WindGardenBase.png`, `RainMillMap.png` and `Mika.png` through `view_image`.
They supplied open gameplay composition, five-stop rope-route geometry, warm
handmade material and lighting references. They were inspected rather than
passed as edit targets: every selected output is a new generation with no
copied source pixels. Also read PRODUCT.md, DESIGN.md,
Docs/VISUAL_MOTION_REWORK.md and Docs/P20_CAMPAIGN.md.

Existing artwork, metadata and files were preserved. The eleven selected native
PNGs were copied from the built-in generated-image location into
`CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Woodland/`.
No alpha extraction, matte removal, resize, image conversion or pixel editing
was performed. Exact final prompts are in `Docs/AssetPrompts/Woodland/`;
the skill launcher embedded those exact strings as PNG text metadata.
`embed-prompt --scan` reported **11 rasters, 0 missing**.

## Selected deliverables

| Asset | Native format | Exact prompt | Native generated source |
| --- | --- | --- | --- |
| WoodBoard.png | 2172 × 724 RGBA | [WoodBoard.txt](AssetPrompts/Woodland/WoodBoard.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-83e1e164-932f-401c-b7ef-1870b8bb8a2b.png` |
| CloudPostBase.png | 862 × 1824 RGB | [CloudPostBase.txt](AssetPrompts/Woodland/CloudPostBase.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-69907cb3-5cae-4490-8e2b-58db93b45a9d.png` |
| CloudPostMap.png | 864 × 1821 RGB | [CloudPostMap.txt](AssetPrompts/Woodland/CloudPostMap.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-4c22229a-91d7-44d9-94de-2a6e506e86fe.png` |
| GlassLagoonBase.png | 863 × 1823 RGB | [GlassLagoonBase.txt](AssetPrompts/Woodland/GlassLagoonBase.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-2f6eb61c-7904-47a3-b6bc-1adf66acc4e3.png` |
| GlassLagoonMap.png | 862 × 1825 RGB | [GlassLagoonMap.txt](AssetPrompts/Woodland/GlassLagoonMap.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-ef8f1018-69c9-44a0-8cfa-15863cb837c7.png` |
| MossArchiveBase.png | 862 × 1824 RGB | [MossArchiveBase.txt](AssetPrompts/Woodland/MossArchiveBase.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-ceefa696-9afb-4248-bd8b-07994dba564f.png` |
| MossArchiveMap.png | 863 × 1823 RGB | [MossArchiveMap.txt](AssetPrompts/Woodland/MossArchiveMap.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-c3d782bf-0694-4d18-8c4d-fc4fc6535304.png` |
| SnowSpireBase.png | 863 × 1823 RGB | [SnowSpireBase.txt](AssetPrompts/Woodland/SnowSpireBase.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-dd022f12-48cf-49ef-89e0-e3ee1b8d396f.png` |
| SnowSpireMap.png | 863 × 1822 RGB | [SnowSpireMap.txt](AssetPrompts/Woodland/SnowSpireMap.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-1f98535c-326f-423b-97dc-902e3e2ca2b3.png` |
| GreatBeaconBase.png | 863 × 1823 RGB | [GreatBeaconBase.txt](AssetPrompts/Woodland/GreatBeaconBase.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-e35bacb4-a31b-4cb9-8093-9ef54fddb540.png` |
| GreatBeaconMap.png | 864 × 1821 RGB | [GreatBeaconMap.txt](AssetPrompts/Woodland/GreatBeaconMap.txt) | `/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-d623e359-7043-4dbd-8f57-773cf7a93398.png` |

WoodBoard is an isolated frontal honey timber plank with rounded ends, grain,
two knots, darker underside and two transparent attachment holes. It contains
real native alpha, range 0–255; exterior corner and both hole centers have alpha
0. Native canvas is 2172 × 724. Its visually solid board is approximately
`(x=50,y=262,w=2072,h=185)` at alpha ≥128, around 11.2:1. It is slimmer than
the requested roughly 8:1 body; modest vertical presentation stretching is
possible. The generated image also contains extremely faint alpha below 16
outside the body. Do not infer useful sprite bounds from alpha >0; use an
explicit body-band sprite/UV rect while preserving the source native alpha.

The five Base scenes put recognizable architecture at the sides and bottom with
a calm central play area. Cloud Post uses mail canvas awnings and hatches;
Glass Lagoon uses glass roofs and water basins; Moss Archive uses open bookshelves;
Snow Spire uses snowcaps and scarf-like saffron cloth; Great Beacon uses lantern
architecture in sunset light. Weather and moving cloud/glider/plant layers remain
runtime responsibilities. Some distant green landscaping is static painted
detail at the bottom of warm-region Bases; no large living foliage occupies the
central gameplay area. MossArchiveBase has tiny texture marks in a lower side
open book; they are not authored readable UI text.

Each Map has five primary connected floating stations with new themed
architecture, opaque sky and four rope bridges. Distant rim architecture is
scenery, not additional tappable stops. Small plants on Map stations are static
painted landscaping. Live stop numbers, progress, Mika and interaction remain
runtime UI.

## Visual inspection and integration anchors

All eleven saved workspace files were opened once after copying and embedding.
Checked subjects, distinct themed architecture, five primary Map stations,
rope trail, calm Base center, no characters or unrelated propellers, and native
opaque portrait coverage. WoodBoard was visually inspected with its native
alpha and checked numerically; no separately saved light/dark composite was made.
These are asset inspections, not a Unity layout or human-enjoyment verdict.

Approximate final Map plaza centers for runtime UI alignment follow, expressed
as normalized `(x,y)` measured from the **top left** of the full native artwork,
in ascending stop order. These are visual estimates; native Unity captures must
verify the final placements.

| Map | Stop 1 | Stop 2 | Stop 3 | Stop 4 | Stop 5 |
| --- | --- | --- | --- | --- | --- |
| CloudPost | (0.316,0.763) | (0.667,0.612) | (0.326,0.467) | (0.612,0.329) | (0.810,0.191) |
| GlassLagoon | (0.366,0.786) | (0.666,0.620) | (0.321,0.458) | (0.567,0.329) | (0.793,0.166) |
| MossArchive | (0.280,0.820) | (0.681,0.694) | (0.280,0.533) | (0.615,0.371) | (0.805,0.194) |
| SnowSpire | (0.278,0.791) | (0.690,0.627) | (0.330,0.510) | (0.640,0.358) | (0.814,0.198) |
| GreatBeacon | (0.325,0.759) | (0.661,0.607) | (0.344,0.479) | (0.630,0.344) | (0.796,0.196) |

## Bounded correction and remaining evidence

Issued one built-in call per selected asset. The initial CloudPostBase render
contained baked fog/cloud banks and envelope pictograms, so one targeted
regeneration strengthened separate-layer constraints. Only the corrected
CloudPostBase ships. The rejected native output remains at
`/Users/andrew/.codex/generated_images/01a10b8e-e6b7-7253-9691-34df2e248fe8/exec-5d91bf44-a5cd-4b18-a0af-11c31bb038d3.png`
outside the project and is not referenced by gameplay.

No extra design variants were generated. Runtime imports, UV fitting, stop
placement, small/normal/wide native game captures and integration checks belong
to the main implementation. No Unity calls, package/settings changes, rule
changes or gameplay-state changes were made by this asset task. Device quality,
physical touch effectiveness and enjoyment were not tested.


## Original ambient audio
WindBed.wav and RainBed.wav are deterministic synthesized quiet beds, not sourced recordings. Exact authoring recipe: Tools/build_woodland_ambience.py (24 kHz mono PCM16, 8 seconds, seeds240510/240511, filtered noise/gust envelope). They follow Sound preference and explicit pause/suspension. Physical speaker/headphone listening has not been verified.
