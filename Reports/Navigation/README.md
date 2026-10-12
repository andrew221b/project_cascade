# Navigation and story opening — 6 October 2026

Scope: replace the rejected menu flow while retaining the painted Sky Wind
Workshop, existing puzzle rules, content bindings and local progress. Baseline
commit was `94fbbca`; the pre-existing Unity Cloud edit was preserved.

Home now has Start game, Shop, Settings and Remove ads. Start game opens
Story / Shapes / Words / Mixed. Story opens the three-scene introduction once,
then the journey map; it can be replayed. The other modes open a filtered,
numbered level shelf with real dragging to turn pages. Back returns to the
parent screen. The Shop links to earned Gifts, Workshop and My things; this
version has no live purchase or ads SDK, so Remove ads states ads are already off.

The opening shows Gran's beacon, the storm, and Mika's promise to carry the
last light home, repair bridges and help neighbours. Comic characters match
the existing Mika, Pip and Rue portraits. Current island, goal and next task
stay visible on the map. Small Mika stands on the island, with a separate
larger conversation hit area. Clouds and map scenery move at different depths;
bounded device gravity or mouse input supplies gentle parallax. Reduced Motion
returns the map offset to zero. Full authored sky covers the viewport; no
gameplay suspension layers appear behind the map.

## Executed evidence

| Check | Result |
| --- | --- |
| UI table completeness, format arguments, both font glyph sets | EditMode **2/2 PASS**, 0.3900999 s; `editmode.xml`. |
| Six scoped navigation/input/state cases | Initial **5/6 PASS**, 5.2350358 s; `playmode.xml`. The real swipe failed because its threshold used full window width. |
| Corrected real shelf swipe and all three exact manual modes | Focused **1/1 PASS**, 1.7437582 s; `../NavigationSwipeFix/playmode.xml`. Thus six distinct cases have passing evidence, not one later 6/6 rerun. |
| Expanded Mika target, real margin click to dialogue, no puzzle-state change, Reduced Motion and ads navigation | Expanded existing case **1/1 PASS**, 1.8660373 s; `../NavigationMapFix/playmode.xml`. This is a rerun, not a seventh distinct case. |
| Visual inspection | 16 retained Unity camera PNGs with matching layout JSON: EN home, compact RU home/levels/settings, UK mode/ads, RU comic/dialogue, shop/gifts and map at 320/390/wide. Portrait captures use simulated safe insets. |
| Decorative map response | `map-fix-runtime.txt`: offsets change, Reduced Motion offset is zero, puzzle hash unchanged. `map-shifted.png` is an explicit +6/+4 reference-unit fixture, not hardware tilt evidence. |
| Independent finish review | Initial **FIX** in `impeccable-review.md`: sky bands and narrow conversation targets. `impeccable-verdict.md` scores **both resolved**, **SHIP at F1/F2 Editor scope**. |

The 320 × 693 map now measures Mika's target at 52.5 × 65.7 rendered pixels,
Talk at 53.7 px high and Travel at 56.9 px. These are Editor reference bounds,
not hardware pt/dp certification. All render captures used isolated temporary
profiles; the override was released before export. The latest checked Editor
console had zero Error entries. No scene was saved from Play Mode.

## Original raster assets

All three selected assets were produced with the built-in Image Gen tool,
copied into `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Navigation/`,
and configured as cached non-readable textures without mipmaps. Originals in
the tool cache were retained. Exact prompts are embedded in PNG metadata and
saved under `Docs/AssetPrompts/Navigation/`; scan: **3 rasters, 0 missing**.

| Shipping file | Generated original / reference |
| --- | --- |
| `MenuIcons.png` | `exec-f70d75e5-e93a-4cee-97c3-c1249923ac41.png`; eight painted semantic menu/mode icons, transparent background. |
| `OpeningComic.png` | `exec-9925de0d-177e-4e69-b8f8-e69616c4a89a.png`; final triptych identity edit, based on `exec-f9115a84-5010-434e-a282-755b4664cb26.png` and established Pip/Rue portraits. Only the selected final triptych ships. |
| `MapSky.png` | `exec-e6fae57a-a27f-4d12-896a-841be55563b6.png`; WindGardenMap used as palette/style reference. Continuous sky plate, no islands or wires. |

The generated originals are in
`/Users/andrew/.codex/generated_images/01a1089e-7eaf-75a1-8066-dbfc48df27a1/`.
The icon sheet is sliced 4 × 2; the comic 3 × 1. Unity `.meta` files are retained.

## iPhone update

One iOS IL2CPP export to the existing generated Xcode folder succeeded in
30.505509 s, with 0 errors and 346 warnings; see `ios-preflight.txt`,
`ios-addressables.txt` and `ios-export.txt`. The MCP command envelope did not
return a success result; the on-disk BuildReport reports Succeeded and a
subsequent Editor state/console check found compilation settled and zero Error
entries. This does not require a duplicate export. The warning count matches
the preceding device export; installed AI shader variants and missing App Info
localization metadata remain outside this navigation change.

The generated app configurations retain the prior device bundle identifier,
automatic signing team and CASCADE display name from `Docs/IOS_DEVICE_HANDOFF.md`.
Xcode Release compilation and signing succeeded; `ios-xcode.txt` verifies the
existing bundle ID, display name and team. CoreDevice installation succeeded;
`ios-device.txt` records this separately. Launch was denied because the phone
was locked. The user then requested no automatic phone updates; device actions
stopped without another launch attempt. Future phone builds/installations require
a separate explicit request. No app data was erased.

## Scope limits

Editor captures establish only the inspected layout and visual states. Full
test suites, every regional/progress state, physical phone gyro/touch/haptics,
OS accessibility and insets, Android deployment, FPS/memory/battery and human
comprehension/enjoyment were not validated in this pass. Large Text was enabled
on compact home; the matrix is not an exhaustive accessibility audit. The
localization copy remains an editorial draft. Deployment results are recorded
separately for this one phone update; no release-readiness claim.
