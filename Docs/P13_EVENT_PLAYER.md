# P13 implementation boundary

The application commits the complete transition before presentation starts.
One event player reads immutable events into a separate, disposable visual
frame. It never calls Resolve, Execute, contact handling or damage application.
Cancel/skip/pause/focus loss/disable synchronize from the actual final snapshot
and release the presentation input gate exactly once. Undo then restores its
normal application snapshot; it never reverses a visual callback.

The same persistent mesh/slot views survive swaps, entry permutations and drops.
Glyph faces follow mesh poses. MotionProfile owns cosmetic durations and travel
curves; Reduced Motion removes arcs, deformation and camera motion. Dust tails
and pooled audio voices live independently from the input gate. Local visual
debris has no Rigidbody or Collider. Device views contain no rule callbacks.

New SFX/music are original synthesized assets, not copied reference audio.
Haptic requests go through a presentation-only adapter and can be disabled;
device behavior will be labelled untested until exercised on hardware.
No package, rule, schema, price or persisted gameplay ID changes are planned.

Acceptance: interrupt each event stage, including by pause/focus/disable, and
prove state hash/route length/history do not change; replay all 40 UI paths;
visually inspect swap, linked swap, entry shift/mirror, key/repair/bomb, collapse
and delivery through Unity MCP. Automated coverage does not prove enjoyment
or phone responsiveness. P18 owns profiling/device/build acceptance.
