# Original tool departure and wind suspension — 6 October 2026

The user corrected the previous duplication request. `GameReactionBanner` reparents
and animates the original Image and painted symbol, without Instantiate. It records
and restores layout, fonts, color and raycast properties. The source body, socket,
cord and local caption are absent once consumed. Existing canonical flags control
that visibility. After the beat the same object is retained under its hidden home;
Undo makes it visible again, with the original pose. New attempts clear/release the
original before rebuilding. No extra visual object or gameplay transition.

Atmosphere: gentle deck target sway .12×wind and roll1.2×wind, damped rope bow.
Canonical gust adds a single directional velocity impulse (.85 deck sway,10 roll,
.38 rope bow,12/18 hanging tool swing) and decays through the existing springs.
Deck limits .22 world units,2.4 degrees; suspended tool angle capped10 degrees.
Reduced Motion remains still; pause/focus suspension freeze time and springs.
The prior directional warning, particle art and reaction timings are retained.

## Validation
- **4/4 PlayMode PASS**,24.7759089s: original instance identity through departure,
  empty source dock, pause/quality/stages, hidden consumed tool and exact Undo restore;
  ambient versus gust deck/rope response with unchanged puzzle hash;
  suspension/painted marks; actual-input winning canonical wind route and warning.
  `playmode.xml`. Gust deck excursion exceeds1.4×measured ambient excursion in the
  targeted case, rope movement >.02 world units, limits and pause/quiet verified.
- Native canonical contact witnesses for repair, bomb defuse and Cyrillic repair:
  six PNG/metrics show departure and completed empty contacts. Original instance
  IDs, no retained source, and same-object Undo assertions passed in the fixture.
- `original-flight.gif`: native sampled camera sequence. `wind-suspension.gif`:
  native close crop, ambient first, then the same cosmetic gust cue injected at
  sample15; the puzzle hash remains unchanged. Actual canonical wind transitions
  are tested separately. These are motion witnesses, not performance videos.
- Console0errors/warnings, compilation successful, Play Mode stopped.
- No new art generation or build. Full suite, physical devices, performance and
  human enjoyment NOT RUN. Original assets/meta, gameplay rules/IDs/data/prices,
  packages/save schema and user Unity Cloud settings preserved.

Earlier Reports/ReactionSequence documents the superseded duplicate presentation;
this report and Docs/ORIGINAL_TOOL_DEPARTURE_AND_WIND.md are the current behavior.
