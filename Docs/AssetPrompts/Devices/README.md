# Recognizable gameplay devices

Six original transparent device sprites generated with the built-in `image_gen.imagegen` tool on 2026-10-05. One generation per device; no retries. The pinned CASCADE Sky Wind Workshop world was retained. `Workshop/LeafPin.png` and `Woodland/WoodBoard.png` were visually inspected before generation; the new-image prompts describe their painted mint, honey wood, brass, brush texture, and rounded volume. They were contextual style references, not edit targets.

## Deliverables

All final assets are under `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Devices/`:

- `Bomb.png`: charcoal ball, golden rope fuse, ember; no painted matching symbol.
- `RepairKit.png`: mint toolbox with large ivory plus and hammer.
- `BrassKey.png`: open round bow and two broad teeth.
- `Stamp.png`: wooden knob, coral rubber base, ivory star seal.
- `RibbonSpool.png`: wood reel, blue ribbon, loose ribbon end.
- `Hourglass.png`: brass/wood frame, glass bulbs, turquoise sand.

Each final PNG is the native 1254 × 1254 RGBA output, with only exact prompt metadata embedded by the Impeccable `embed-prompt` command. Native decoded pixels and alpha are preserved. Nothing was cropped, redrawn, resized, or recolored for the runtime assets. Native file, final file, and decoded RGBA SHA-256 hashes are in `provenance.json`; exact tool prompt strings are both embedded and saved in the six matching `.txt` files. `generation-inputs.json` also preserves source output paths and prompts.

## Inspection

All six outputs were inspected at native presentation and together on warm ivory and deep teal grounds in `inspection-light-dark.png`, with 180px and 48px previews. The bomb reads as a ball bomb and its fuse remains distinct; the other five silhouettes remain recognizable. The generated toolbox includes a small cloth and pot within the kit, but its large plus and hammer dominate. The hourglass glass has painted pale translucency rather than physically accurate clear-glass refraction; open gaps around the glass are transparent. These are accepted stylistic details, not reasons to regenerate.

Every PNG has real alpha ranging from 0 through 255. Fully transparent pixel coverage ranges from 39% to 74%; detailed metrics and alpha bounds are in `provenance.json`. The inspection sheet is a derived QA preview only, never a runtime atlas. The final objects retain their native alpha margins.

Unity import, final in-game size, device-symbol dock placement, in-game overlap, and human comprehension remain integration checks for the main task. No Unity refresh, code edit, or `.meta` manipulation was performed by the asset producer.
