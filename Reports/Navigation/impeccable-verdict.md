# CASCADE navigation correction verdict — 6 October 2026

## Disposition

**SHIP at the scored F1/F2 Editor-review scope.** Both material findings in `impeccable-review.md` are resolved in the supplied recaptures and supporting interaction evidence. This confirmation supersedes that report's fix disposition for those two findings only. It does not certify the complete product, device behavior or release readiness.

## Scoped coverage

Individually inspected the corrected native Unity camera renders and matching JSON: `map-ru.png` (390 × 844), `map-wide.png` (1440 × 900), `map-ru-small.png` (320 × 693), and `map-shifted.png` (1440 × 900). The shifted fixture uses an explicit bounded +6/+4 reference-unit pose; it is not a physical gravity-sensor capture. Also read `map-fix-runtime.txt`, `Reports/NavigationMapFix/playmode.xml`, and the affected conversation-target/source test sections. No broader finding hunt or production edits were performed.

## Material findings

| Finding | Score | Evidence |
| --- | --- | --- |
| **F1 — map sky extension** | **Resolved** | The stretched horizontal bands in the old wide view and vertical streaking in portrait are gone. A painted sky now fills the whole frame, with coherent clouds and no abrupt portrait-image boundary in the four inspected poses. The current island silhouettes, locked-state treatment and small Mika remain. The shifted pose retains continuous sky around the moved route. |
| **F2 — conversation touch targets** | **Resolved** | The separate invisible `TalktoMika` target measures **52.5 × 65.7 rendered pixels** at 320 × 693 and **64 × 80** at 390 × 844. The compact `Story` and `Travel` actions measure **53.7** and **56.9** pixels high; the normal portrait actions measure **65.4** and **69.3** pixels high. Each scored control now exceeds the project's 48-pixel compact reference baseline while Mika's visible art stays small and seated on the island. The expanded existing test clicks the target's invisible margin outside the art through mouse input and reaches dialogue without changing puzzle state. |

## Craft-floor assessment

The correction restores the requested complete illustrated background and preserves the game's timber/brass/enamel identity. Compact map goal, task and action copy remain readable in the inspected pose. Enlarging the invisible target preserves the intended character scale, and the taller footer provides distinct conversation and travel actions without crowding the current island. The two scored findings have no remaining material issue in this evidence.

## Evidence and limits

`Reports/NavigationMapFix/playmode.xml` records **1/1 Passed**, duration **1.8660373 seconds**, for the expanded `MapParallaxAndNoAdsNavigationNeverChangePuzzleState` case. This is a rerun of an existing case, not a seventh distinct navigation test or a full-suite run. The runtime note records changing Editor offsets, **Reduced Motion = (0, 0)**, and an unchanged puzzle hash. These checks support state independence and the conversation hit area, not animation smoothness or hardware tilt quality.

The implementer reports an embedded exact prompt for the new `MapSky` raster and a clean three-raster provenance scan; this reviewer checked its rendered result. All images remain Unity Editor evidence with simulated portrait safe insets. Pixel bounds are not hardware pt/dp certification. No tests, captures or builds were run by this reviewer. Current full-suite regression, device build, physical touch/haptics/gyro, OS insets/back behavior, performance, accessibility, native-language editorial approval and human comprehension/enjoyment remain unverified.
