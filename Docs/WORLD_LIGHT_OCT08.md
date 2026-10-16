# Source-based world light — 8 October 2026

Presentation only. Retain the nine worlds, original artwork, materials' property
names and palette. No packages, prices, puzzle rules, persistent schemas, IDs or
pipeline assets change. Extend WorldLighting and its existing directional light.

Replace flat ambient illumination with sky/ground irradiance and a cached HDR
environment reflection. Sun/moon direction is fixed per world; cloud transmission
uses the same paused presentation clock. Moon light is cool white, with sufficient
adapted exposure to recognise colours. The workshop has its own warm window/lamp
context, independent of the last story world. No additional real-time light/shadow
camera is required. Dispose transient environments and restore previous settings.

Round the front caps of runtime copies of the existing toy meshes, preserving their
outline, UVs and original assets. URP PBR/clear-coat surfaces receive the actual key,
ambient probe, reflections and geometric shadows. Contact shadows follow the key
and lift, soften and fade with separation. Wood and ropes share that key.

Illustrations are 2.5D surfaces, not recovered 3D scenes. Their shaders use bounded
volume normals and the same source; sky, stars and warm windows retain emitted
radiance. This cannot reconstruct baked shadows or physically trace occlusion
inside painted buildings. UI stays legible and unlit. Reduced Motion freezes
optional lighting motion; Effects Off keeps essential surface lighting. Use cached
GPU/material data and one small cubemap per visited context, never render reflections
per frame. Review nine gameplay worlds plus map, workshop, result and mobile quality
in native Unity, and test direction, ambient, shadow projection and teardown.

## Validation

Five native EditMode cases passed: lunar direction/lifted shadow, all nine source
profiles and bounded transmission, independent workshop light, preserved source
silhouettes with convex runtime caps, cached reflection and render-state restoration.
Unity MCP rendered 17 final contexts, with nine gameplay worlds, lifted and opposite
key diagnostics, clear/cloud-filtered moonlight, Mobile_RPAsset, map, result, workshop
and shop. The illustrated map's duplicate moon is replaced by the common sky source,
without modifying the original image. Cloud transmission also attenuates reflection.

Reviewed both native contact sheets and full-size night, map, lifted, workshop and
result images. Portrait reflow restarts the result ceremony, so its final evidence
uses an explicit completed presentation pose after reflow; it does not validate
ceremony timing. Final Console has zero errors. All scene lights: one directional
key. Puzzle hash unchanged. Play stopped, QA profile override cleared, temporary
reflection cubes remaining: zero. No new phone build, physical-device performance,
or complete gameplay suite was run. Evidence: Reports/WorldLight-Oct08.
