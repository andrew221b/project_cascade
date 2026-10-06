# Physical presentation and light — 6 October 2026

User requests tangible movement, transmitted swap/landing impulses, individual
ropes, physical splinters and correct live lighting. Domain/Solver remain atomic
and authoritative. Cosmetic bodies cannot select slots, spend moves or resolve
contacts. No rules, IDs, packages, serialized schemas or original artwork change.

Use an isolated local PhysX scene for pooled splinters: gravity, mass, velocity,
angular velocity, air force and cosmetic platform colliders, stepped only while
presentation is active. This is rigid-body debris, not a ragdoll. Figures keep
controlled horizontal delivery to the exact slot, gravitational descent and damped
settling; small individual rocking transfers landing/swap forces to board springs
and adjoining constrained rope spans. Ropes remain custom pinned point chains,
not a complete real-world rope model (no knots/self-collision/friction). Motion
stays bounded for readability; pause and Reduced Motion remain authoritative.

Use the existing key/fill light with actual URP clear-coat materials, alpha-tested
wood capable of receiving/casting shadows on mobile too, and live rope lighting.
Painted scenery and screen-space UI remain illustrated, not reconstructed 3D
geometry. Verify actual shadow/highlight response with a native light-direction
comparison, not only renderer flags. Keep full physical-device/performance limits
honest. No extra build or raster generation is required.

Implementation: the existing pool has one local Physics3D scene, reused rigid bodies
and box colliders. Twelve kinematic contact segments follow up to four cosmetic
planks, including sag/roll, and disappear before a broken plank releases. Splinters
have different masses, spin and air response; gravity, collisions and angular
motion are integrated at90Hz with bounded catch-up. Pause does not step that scene;
quiet feedback freezes existing bodies and fades them. Tails/lifecycle unload are
owned by the feedback pool. Figures use9.81m/s² descent, per-piece small rocking and
a bounded landing correction. They are guided presentation, not free PhysX blocks.

Wood has an actual sloped/deforming top to receive projected shadows. Opaque alpha
cutout wood also receives mobile-profile shadows; splinter shadow silhouettes use
their authored alpha. The resource CandyGlaze material references URP Complex Lit,
so the clear-coat variant is real and retained for builds. Rope light uses live
main-light/shadow sampling and cylindrical normals. Background/UI illustrations
stay intentionally unlit. Their painted shadows do not become realtime geometry.

Review fixed a rope shader syntax error, straightened falling shapes before contact
and corrected old active target children participating in the new row layout until
deferred destruction. That target centering has a focused regression. Native paired
shadow-on/off and opposite key directions confirm rendering, with a separate mobile
quality profile sample. See Reports/PhysicalPresentation and PhysicalTargetLayout.

Teardown validation additionally caught the debris scene unloading before the
owner's ClearTails and an existing cancelled presentation redrawing destroyed UI.
Clear is now null-safe across scene ordering; disabling explicitly suppresses
visual completion while still releasing the presentation gate. OnEnable resumes
normal state rendering. Focused physics/impact/scene-reload regressions are retained
in Reports/PhysicalLifecycle; early failed lifecycle evidence is described there.


Touch revision, 6 October: falling and shaken pieces now also borrow this isolated
scene, using at most eight pooled rigid bodies and box collision proxies. They spin,
bounce and collide, then blend to canonical seats; ordinary wind and exchanges remain
guided. The same rendered originals follow those bodies. This replaces the previous
guided-only descent described above. It adds no gameplay physics, ragdoll, exact
shape collider, new lighting system or fully realistic rope simulation. See
Docs/TOUCH_AND_STORES.md and Reports/TouchRevision for current validation limits.
