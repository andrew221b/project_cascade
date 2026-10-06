# Flexible rope presentation — 6 October 2026

The previous rope had one damped sine bow per span. Replace it with small view-only
Verlet chains: pinned endpoints, length constraints, soft bend resistance, gravity,
velocity damping and seeded smooth local wind. No gameplay physics or random rule.
Pins remain at each visible platform knot; removed floors merge the neighboring
span. Longer free spans have more visible response; platform motion tensions the
chain. Left/right ropes and different heights have different coherent eddies rather
than synchronized identical bows. A gust imparts velocity to the interior nodes.
Render each chain with smooth cubic samples and continuous world-length hemp braid.

The short single tool cord uses the same bounded chain model in canvas units.
Pause suspends integration; Reduced Motion resets to a taut static line. Resize,
new attempt and changed visible floor topology reset affected spans safely.
Fixed small arrays/substeps/constraint iterations; no rigidbodies, collisions or
allocations during stepping. Verify native 2/3-floor motion and a removed-floor
transition, anchored knots, pause/Reduced Motion/resize and puzzle-state invariance.

The model uses 13 points per long span, 9 per tool cord, 90 Hz fixed steps capped
at .05 s per update and six constraint passes. Length slack is .6%; a weak local
bend filter removes high-frequency folds without replacing the integrated curve.
Reset seeds a small wind-aligned arc to avoid buckling a perfectly straight chain.
Nodes remain within .34 world units / 13% span, with a smooth endpoint envelope.
Different span lengths create different response; the small cord scales in canvas
units. Smooth seeded noise makes the motion irregular but continuous, never jitter.

Native review found first-frame accordion folds; slight slack, reset curvature and
bend smoothing corrected those. An overly stiff smoothing value was also reduced
after native comparison so the wind still produces visible broad flex. Final
motion, attachment and accessibility evidence is recorded in Reports/FlexibleRopes.
