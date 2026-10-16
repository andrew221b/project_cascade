# Calm wildlife flight — 10 October 2026

Scope: presentation only. No puzzle transitions, campaign rules, economy, IDs or packages change.

The old rig rotated its entire flat picture up to 90° when horizontal velocity approached zero. This produced the reported thin strip. Tiny high-frequency path offsets could also flip its facing during hovers. Seven independently scheduled actors and short visits made the scene restless.

Use broad continuous return arcs instead of micro-jitter, a 0.64-second route lookahead for heading, longer visits and visible glides. Dragonfly pauses remain; night moths orbit lights more slowly. Outside the open flight corridor, trajectories remain confined to the side gutters.

A new generated transparent dorsal-body atlas supplies all 17 species. Existing painted wings form symmetrical rear silhouettes. Bodies blend through turns while one solid wing pair follows the heading; the whole rig remains camera-facing, so perspective depth changes size without flattening the art. Original atlases and their metadata are preserved. Rear artwork shares one material; parts and deformation buffers are pooled.

At most three main visitors (including the perched bird) and one stage/cosmetic guest are scheduled. Insects alternate in one main slot; extra visitors have non-overlapping slots. Perching retains contact and input/gust/support-loss departure behavior, with longer rests and cooldowns. Reduced Motion continues to hide decorative fauna.

Validation completed:
- 16 native Unity EditMode tests passed: six styles return along broad arcs, continuous paths, wing/glide transitions, dragonfly hovering with the return-arc heading, stable rear view during facing changes, and landing/takeoff timing.
- Two native PlayMode lifecycle tests passed: all nine visitor families, side-gutter silhouettes, reserved flight corridors, 21 pooled renderers, stage/island/cosmetic variation, frozen clock, Reduced Motion, disabling and camera replacement. Sampling covers the longer visit schedule.
- Native isolated gameplay: nine worlds sampled over 180 seconds each at 10 Hz; max four visitors and one extra guest; 49 stable pooled renderers; frozen transforms, Reduced Motion and unchanged puzzle hash. Final runtime log has zero captured errors.
- All 17 species rendered from the side and back; 18-second actual `Fly` recording at 12 fps for bird, butterfly, dragonfly and moth. The first visual pass revealed doubled translucent wing silhouettes; the final rig renders one wing pair. Dorsal renders and selected final motion frames were visually inspected.

Evidence: `Reports/WildlifeFlight-Oct10/editmode-final.xml`, `playmode.xml`, `native-review.txt`, `native-final.log`, `rear-species.png`, `side-species.png`, `gameplay.png`, and `frames/`. The editor-only recorder scales its four film subjects for inspection after sampling the production `Fly` path; the gameplay capture uses the actual game camera. Generated rear-body source was copied without altering the source PNG or older atlases.

No Unity MCP callable was exposed in this session; native Unity camera rendering was used. Physical-device frame rate and visual appearance on the phone were not measured. No iPhone build/install was requested or performed. Native QA uses an isolated save and restores the review profile and background preference.
