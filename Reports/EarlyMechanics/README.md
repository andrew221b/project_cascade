# Early mechanics and platform clarity — 6 October 2026

Fresh runs use recipe v4. First island introduces repair (C02), bomb (C03), key and branch stops (C04), ribbon (C05), stamp (C06), hourglass (C07), one-use bridge (C08), linked swaps (C09), and fragile wood (C10). Subsequent early levels combine them. Each remains a three-stage delivery. Canonical rules, SymbolId multiset, prices, source IDs and packages stay unchanged; saved v1–3 recipes remain supported.

Generated wood counter and movement props replace overlapping landing labels. Future previews sit above empty planks; paired-swap previews animate both pairs. Printed bomb shapes use the matching piece colour. Tool docks remain near their actual contact slots. Wood wears, bends, snaps and emits 22 pooled cosmetic PhysX fragments in two waves; normal fragments live 4.3–4.7 seconds and fade during the final quarter. TimberSnap is an original, non-clipping 0.94-second wooden foley clip.

Wind warning now comes before the platform loses support. Ambient gusts also sway the rig without changing the puzzle. Only canonical landing transitions can shift the row. Director begins after five introduction levels, eases failed runs, and proves its variants before play.

Final MCP compilation successful; Console error entries 0; Play Mode stopped; isolated review profile detached.

## Targeted validation

- `proofs.xml`: 1/1 PASS, 56.885s; three-stage canonical replay / level validation for 169 unique early primary/localized definitions and C41/C81/C160 representatives.
- `wide-proofs.xml`: 1/1 PASS, 9.339s; affected seven/eight-piece recipes re-proved after removing the overloaded double shift / full reversal. `curves.csv` contains the final merged 169 definitions.
- `intro-tests.xml`: 2/2 PASS, 0.383s; every first introductory contact is reachable, and C03/C07 primary/localized witnesses actually use bomb / hourglass contacts.
- `runtime.xml`: 2/2 PASS, 16.918s; actual forecast motion, bomb reaction, gust, fracture, canonical hash stability, three stages before one reward; isolated cosmetic PhysX falling/rotation/pause/Reduced Motion.
- `wind-order.xml`: 1/1 PASS, 14.759s; recheck only the changed anticipation order, requiring intact support during the warning.
- Native Unity MCP: nine retained images and layout metrics, EN portrait/wide and RU compact with large text / simulated safe insets; real fracture, bomb reaction and gust. Final changed views were refreshed separately. Forecast bounds do not intersect active hanging-tool bodies in this matrix. `native-runtime.txt` records observed ambient gust deviation and unchanged canonical state.

Calibration caught an initially closed one-use bridge, a misplaced hourglass, a shortest route bypassing the introduction bomb, and a wide reverse exceeding the bounded search. Visual inspection caught an uncoloured bomb square, legacy counter-number hiding, forecast overlap with a stamp, and a two-line countdown crowded by a third platform. These were corrected before final confirmation. Intermediate failed reports and redundant copies were removed.

Not run: full suite, player / phone build or installation, actual phone safe areas / gyro, device performance benchmark, human enjoyment / retention study. Automated solvability and Editor pictures do not establish these.

Art prompt / original source: `Docs/PLATFORM_PROPS_ART.md`. Design note: `Docs/EARLY_MECHANICS_AND_PLATFORMS.md`.
