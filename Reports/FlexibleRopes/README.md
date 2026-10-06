# Flexible hemp ropes — 6 October 2026

The prescribed sine bow is replaced by a cosmetic constrained point chain.
Pinned endpoints follow visible floor knots and hanging tool grips. Inertia,
gravity, damping, soft bend resistance and different coherent wind eddies across
height/sides produce irregular continuous flex. Canonical gusts add a directional
velocity kick, then settle. Cubic sampling and world-length braided shading keep
the rope smooth. No gameplay physics, random rules, IDs or package changes.

Native comparison caught initial small accordion folds and then excessive bend
stiffness. Slight slack (.6%), a wind-aligned reset arc and weak bend smoothing
corrected both. Final native frames were inspected across ambient and gust beats.

## Validation
- **4/4 PlayMode PASS**,19.5599533s, in `playmode.xml`: distinct local curvature,
  left/right response, unchanged puzzle hash, exact pause freeze, first surviving
  knot attachment after real floor destruction, Reduced Motion and simulated safe
  insets/resize, stronger gust excursion, bounded decks and hanging tool behavior.
- `two-floors-wind.gif` (C18) and `three-floors-wind.gif` (C30): final native Unity
  camera sequences, cropped only to emphasize suspension. Ambient first; cosmetic
  gust injected at frame18 in opposite directions; puzzle hash asserted unchanged.
  Frame timing follows native capture file intervals (~.20s). GIF palette reduction
  is a display limitation; these are motion witnesses, not performance recordings.
- `removed-floor.png` / JSON: actual C03 swap destroys first floor; continuous rope
  reconnects to the surviving platform. `reduced-wide.png` / JSON:1440×900 static
  quiet view after that transition. Native 390×844 phone-aspect motion inspected.
- Compilation successful; Console0errors/warnings; Play Mode stopped.
- Full suite, build, physical devices, performance and human enjoyment NOT RUN.
  No new bitmap assets or extra build. User Cloud settings and existing assets/meta
  preserved. Historical OriginalDeparture rope bow is superseded by this model.
