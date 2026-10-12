# Touch feel — 6 October 2026

A small immediate press acknowledgement leads into a soft lift/enlargement. Drag
tracks the finger with a short bounded response and a gentle directional tilt.
Cancellation returns the same body to its live seat in 180 ms. Successful travel
inherits position, scale and angle without snapping back. Letters follow the
body through all poses. Stable slot hit regions, atomic commands and quiet
motion remain intact. Preview is cached until the destination changes.

## Evidence

- `shapes-gesture.gif`: native Unity InputSystem drag, one committed swap.
- `letters-gesture.gif`: RU letter drag followed by cancellation, zero moves.
- `*-held.png`: unmodified native 390×844 camera frames.
- `quiet-narrow.png`: native 320×693 Reduced Motion layout.
- `native-check.txt`: actual gesture move counts and released hold state.
- `playmode.xml`: 8/8 passed, 13.129887 s. Actual touch/mouse input, cancellation,
  selected/held glyph alignment, handoff continuity and reduced motion.
- `../TouchFeelContinuity/playmode.xml`: final 2/2 passed, 4.5364689 s. Released
  pose transfer and a far swap past an identical untouched symbol; nine distinct
  scenarios across both runs.

GIFs crop the native frames to the active row and its wooden support, without
retiming motion; file capture intervals are preserved in `*-capture-timing.json`.
Editor capture cadence does not measure input latency or frame rate. The first
batched native pass covered both symbol modes and narrow quiet layout. One
confirmation followed a destination-cache reset and duplicate identity fix.
Console after the final native Play/Stop: 0 errors/warnings. Play Mode stopped.
No new gameplay physics, rules, IDs, prices, packages, saved schemas or art.

Full suite, builds, physical phones, hardware haptics/latency/performance and
human enjoyment: NOT RUN.
