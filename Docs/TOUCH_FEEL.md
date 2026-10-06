# Direct manipulation — 6 October 2026

Presentation-only refinement of press, drag and release. No rules, IDs, prices,
packages, content or save schema change. Domain swaps remain atomic and the
persistent row keeps its multiset.

Press gives an immediate small acknowledgement, then lifts and enlarges the
same body and glyph over about 100 ms. Drag follows the finger with short,
bounded damping and gentle tilt. Hit regions stay still. A legal destination
gets a quiet gold highlight; previews are cached until the destination changes.
Cancelled/free gestures return to their current seat in 180 ms without a move.
Committed exchanges inherit the released body's position, scale and angle;
there is no intermediate snap to the original slot. Selection animation must
not write a pose owned by a held/returning gesture. Pauses, scene teardown and
Reduced Motion retain their explicit paths. No new particles or raster assets
are necessary for this small tactile interaction.

Validation: actual InputSystem PlayMode scenarios and bounded native Unity
portrait/letter captures. Editor capture is not touch latency, device haptics,
performance or human enjoyment evidence.

Actual validation: focused PlayMode 8/8 passed (13.129887 s), then duplicate far
swap/handoff 2/2 passed (4.5364689 s), covering nine distinct scenarios. Native
InputSystem gestures captured at 390×844 for shapes and RU words; quiet/narrow
layout at 320×693. Shapes commit once; cancelled letters commit zero times.
Full suite, player build, real phones, hardware latency/haptics/performance and
human enjoyment were not tested. No new generated sprite is needed here.
