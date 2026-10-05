# CASCADE P17 reward art — provenance and review

Created 5 October 2026 for `Docs/P17_META.md`. Nine original illustrations extend the established Sky Wind Workshop materials and light. They are game ingredients, not screenshots, UI mockups or copies of ShapeMania assets.

## Production contract

- Built-in `image_gen.imagegen` only: nine calls, one distinct asset per call, no repair calls, external service, CLI/API fallback or API key.
- Before generation, the producer inspected `Journey/Mika.png`, `Journey/WindGardenMap.png` and `Journey/RainMillMap.png` and read the relevant established art direction in `DESIGN.md`. Those existing files served as visual style evidence only and were not changed. The nine new generations used text prompts; no image was passed as a tool input and no source pixels were copied into the artwork.
- Every call used `transparent_background: true`. The selected native outputs are square 1254 × 1254 RGBA PNGs. There was no resizing, trimming, recoloring, alpha extraction, chroma-keying or other pixel editing of shipping art. A decoded RGBA byte comparison with every generated source passed after prompt embedding.
- Shipping PNGs live under `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Workshop/`. Exact UTF-8 prompt files and JSON source/check records live under `Docs/AssetPrompts/Workshop/`, outside Resources. The few Unity-generated prompt-sidecar `.meta` files were moved alongside those source sidecars at the parent's explicit direction. Shipping PNG `.meta` files were not edited or moved by this producer.
- Each shipping PNG embeds the exact string sent to the tool under `impeccable:prompt`, written with the shipped Impeccable `embed-prompt` tool. The scan reported **9 rasters, 0 missing**. All nine embedded strings were recovered and compared against the exact tool input with no added newline.
- This was the parent's explicit asset brief for an extension of an established native game. There is no approved web comp or measured plate spec; assets are therefore **unscored** by the comp/plate gate and await integrated native UI review.

## Alpha and visual checks

All nine images have native alpha ranging from 0 to 255, fully transparent corners, intact cream/white painted areas, and clear interior openings where appropriate. The generator emits many nearly opaque pixels below 255 (typically 251–253), so partial-alpha counts should not be mistaken for missing object paint. Native alpha and invisible RGB were preserved.

Every output was inspected individually at source size. Additional temporary inspection composites showed all nine at 300 pixels over warm ivory `#FFF6D6` and dark blue-green `#193C42`, and at 180 pixels over pale blue `#B8DEEC`. No broad baked halo, rectangular ground, checkerboard, lettering, watermark or background scene was visible after correct compositing. The four miniatures and five reward objects remain distinguishable at 180 pixels. These diagnostic composites were created solely for inspection in `/tmp/cascade-p17-art-review/`; they are not shipping assets and no shipping artwork was altered to create them.

**Margin limitation:** the tool delivered less than the requested 10–15% empty margin on most assets despite the prompt. No object is visibly clipped. Leaf Pin meets the requested margin approximately; the other eight need normal UI fitting/padding, especially the Beacon Model at the top and bottom. Margins below use the visible alpha ≥16 bounding box; extremely faint antialias/noise pixels below that threshold may lie nearer a raster edge. This is recorded as integration guidance, not a claim that the source meets every requested margin.

| Shipping PNG | Native size | Fully transparent pixels | Visible margins left / top / right / bottom |
| --- | --- | --- | --- |
| `BridgeLantern.png` | 1254 × 1254 | 61.27% | 5.5% / 7.2% / 4.8% / 7.0% |
| `RainPump.png` | 1254 × 1254 | 42.46% | 5.2% / 5.8% / 3.3% / 5.2% |
| `PostGlider.png` | 1254 × 1254 | 61.77% | 4.7% / 3.7% / 2.8% / 3.9% |
| `BeaconModel.png` | 1254 × 1254 | 63.55% | 18.3% / 1.4% / 17.5% / 1.8% |
| `Parcel.png` | 1254 × 1254 | 54.18% | 6.2% / 14.1% / 6.1% / 12.8% |
| `LeafPin.png` | 1254 × 1254 | 65.08% | 13.1% / 11.2% / 10.8% / 14.3% |
| `SunButton.png` | 1254 × 1254 | 57.87% | 5.7% / 4.8% / 5.5% / 8.6% |
| `SkyRibbon.png` | 1254 × 1254 | 75.53% | 11.7% / 8.2% / 8.1% / 6.1% |
| `MoonPatch.png` | 1254 × 1254 | 60.11% | 11.4% / 7.9% / 10.8% / 7.5% |

## Exact prompts, selected sources and review

The `.prompt.txt` file for each asset is the complete exact tool input. Its matching JSON stores that prompt, tool parameters, source path, inspected style references, pixel/alpha checks and review notes.

### BridgeLantern.png

- Exact prompt: [AssetPrompts/Workshop/BridgeLantern.prompt.txt](AssetPrompts/Workshop/BridgeLantern.prompt.txt)
- Check record: [AssetPrompts/Workshop/BridgeLantern.json](AssetPrompts/Workshop/BridgeLantern.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-7f5374a1-2457-41ab-91d0-96b55d4de6a1.png`
- Review: Lantern, exactly two posts and open bridge gap read clearly. Native visible margins are about 5–7%; give the UI additional padding.

### RainPump.png

- Exact prompt: [AssetPrompts/Workshop/RainPump.prompt.txt](AssetPrompts/Workshop/RainPump.prompt.txt)
- Check record: [AssetPrompts/Workshop/RainPump.json](AssetPrompts/Workshop/RainPump.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-f94c1998-af00-4206-a80e-dc1b11a00b6d.png`
- Review: Mint wheel, copper spout and ivory bucket remain distinct; clear spoke gaps. Native visible margins are about 3–6%; give the UI additional padding.

### PostGlider.png

- Exact prompt: [AssetPrompts/Workshop/PostGlider.prompt.txt](AssetPrompts/Workshop/PostGlider.prompt.txt)
- Check record: [AssetPrompts/Workshop/PostGlider.json](AssetPrompts/Workshop/PostGlider.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-44229862-dc46-43a8-b29a-9ee093285db8.png`
- Review: Ivory wing, mail bundle and supports read clearly; envelopes are blank. Native visible margins are about 3–5%; give the UI additional padding.

### BeaconModel.png

- Exact prompt: [AssetPrompts/Workshop/BeaconModel.prompt.txt](AssetPrompts/Workshop/BeaconModel.prompt.txt)
- Check record: [AssetPrompts/Workshop/BeaconModel.json](AssetPrompts/Workshop/BeaconModel.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-ba2cf6d8-8569-4c5f-80bf-54ff152ccb35.png`
- Review: Ivory tower and warm brass light read clearly. Top/bottom visible margins are about 1–2%; it requires generous vertical UI padding.

### Parcel.png

- Exact prompt: [AssetPrompts/Workshop/Parcel.prompt.txt](AssetPrompts/Workshop/Parcel.prompt.txt)
- Check record: [AssetPrompts/Workshop/Parcel.json](AssetPrompts/Workshop/Parcel.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-95b9b096-d0a4-4643-8039-8134bb8cc205.png`
- Review: Cloth, sage bow and blank coral wax seal read clearly. About 6% side margins and 13–14% vertical margins; add side padding in UI.

### LeafPin.png

- Exact prompt: [AssetPrompts/Workshop/LeafPin.prompt.txt](AssetPrompts/Workshop/LeafPin.prompt.txt)
- Check record: [AssetPrompts/Workshop/LeafPin.json](AssetPrompts/Workshop/LeafPin.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-f65823e1-9457-4eea-b7b7-7270e5058e77.png`
- Review: Mint enamel and brass vein are clear; 11–14% visible margin meets the brief approximately.

### SunButton.png

- Exact prompt: [AssetPrompts/Workshop/SunButton.prompt.txt](AssetPrompts/Workshop/SunButton.prompt.txt)
- Check record: [AssetPrompts/Workshop/SunButton.json](AssetPrompts/Workshop/SunButton.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-6f312ab2-a379-483b-96c3-84f7b0a54be5.png`
- Review: Eight uneven rays, four sewing holes and two stitches are clear. About 5–9% native margins; give the UI additional padding.

### SkyRibbon.png

- Exact prompt: [AssetPrompts/Workshop/SkyRibbon.prompt.txt](AssetPrompts/Workshop/SkyRibbon.prompt.txt)
- Check record: [AssetPrompts/Workshop/SkyRibbon.json](AssetPrompts/Workshop/SkyRibbon.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-fb10fad7-b9b5-400e-add0-8d07f6f9a77a.png`
- Review: Sky-blue cloth, brass clasp and frayed ends are clear. About 6–12% native margins; give the UI additional padding.

### MoonPatch.png

- Exact prompt: [AssetPrompts/Workshop/MoonPatch.prompt.txt](AssetPrompts/Workshop/MoonPatch.prompt.txt)
- Check record: [AssetPrompts/Workshop/MoonPatch.json](AssetPrompts/Workshop/MoonPatch.json)
- Selected generated source: `/Users/andrew/.codex/generated_images/01a10b3a-39d2-7350-bd89-7eb4386c5f0c/exec-0b94a1b3-6f7a-4b98-864b-b6675ba91e7c.png`
- Review: Ivory cloth, celadon backing and stitches read clearly; crescent opening remains transparent. About 7–11% margins; give the UI additional padding.

## Validation limits and integration ownership

No Unity scene, import setting, script, gameplay state, package, schema, ID, TASKS entry or Git operation was changed by this asset producer. The parent owns texture/sprite import settings, optional UI padding, reward layout, source scale/cropping and native Unity visual review. Rendering at 180/300 pixels is an asset inspection, not proof of integrated UI readability, physical-device performance, human enjoyment or the correctness of rewards and saving.
