# Physical presentation and live light — 6 October 2026

Pooled timber fragments now use an isolated local PhysX scene: rigid bodies with
mass, gravity, angular velocity, wind force and collisions with other fragments
and twelve kinematic segments following cosmetic planks. Broken planks release
their contacts. Pause stops stepping; Reduced Motion freezes/fades existing wood;
quality and pool caps remain. Clear/reload/disposal retain no authoritative state.

Figures retain guided horizontal placement and exact slots. Their descent uses
9.81m/s², per-piece rocking/weight, last-contact straightening and damped settling.
Lift, swap and landing send bounded impulses into board sag/roll and adjoining
rope spans. Ropes use individual cosmetic constrained chains, not PhysX rigid
bodies or a complete rope model: no self-collision, knot friction or tearing.
No ragdolls; no gameplay physics or rule/package/ID/save changes.

Live light: resource-referenced URP Complex Lit supports real clear coat. Wood has
an alpha-tested face and a deforming sloped top that can receive/cast shadows on
both desktop and mobile pipelines. Rope has cylindrical lit normals, main-light
shadow sampling and casting. Fragment shadow masks use authored alpha. Painted
scenery, UI and supporting contact-shadow artwork remain illustrations.

Native review corrected a rope shader syntax error, particle shadow silhouettes
and deferred target children which displaced new receiver meshes until the next
command. Old children now become inactive immediately. No replacement raster art.

## Validation
- **7/7 PlayMode PASS**,20.707851s, `playmode.xml`: independent rope curvature and
  rebuilt attachments; gust response; weighted glyph support; isolated rigid-body
  gravity/spin, pause/quiet/clear and hash invariance; contact momentum and exact
  completed swap; reduced committed poses; real touch drag to a winning delivery.
- **1/1 target layout PASS**,1.3164565s, `../PhysicalTargetLayout/playmode.xml`: C03
  to C30 in the same frame leaves exactly7 aligned target bodies/live glyphs.
  Final cutout material adjustment was compiled and natively confirmed afterwards.
- Two final native camera GIFs: `swap-and-suspension.gif` and
  `physical-splinters-and-fall.gif`. Hash asserted unchanged during presentation;
  crop only, timing follows native file intervals. Palette reduction and Editor
  capture latency mean these are motion witnesses, not performance recordings.
- Three same-pose390×844 light comparisons: shadows enabled at full strength,
  shadows-off control, opposite key direction at the shipped strength.22437 pixels
  respond to shadow strength; native review confirms actual wood shade and moved
  highlights. The shipped key strength remains.42. `lighting-check.txt`.
- `mobile-quality-light.png` / JSON: native settled rendering using Mobile_RPAsset;
  correct target alignment and materials visually inspected. The first immediate
  resize snapshot was rejected; retained capture waits for native layout. Original
  PC pipeline restored, then Editor stopped. The automatic Mobile_RPAsset
  serialization-version migration was reverted; no intentional settings changes.
- Final lifecycle3/3PASS,4.607344s, `../PhysicalLifecycle/playmode.xml` after fixing
  local-scene unload ordering and cancelled playback redrawing destroyed UI. Native
  Editor Play/Stop repeated with0errors/warnings. Shader diagnostics0.
- Full suite/build, physical devices, performance and human enjoyment NOT RUN.
  No extra build. Original art/meta, prices/data/schema/IDs and user Cloud settings
  preserved. Visual physics remains bounded and stylized, not real-world fidelity.
