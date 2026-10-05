# UI and motion polish — 5 October 2026

Presentation-only refinement of the established sky workshop. No gameplay rules, prices, save schemas, content IDs or packages changed.

## Implemented

- Shared left-aligned menu headings, consistent description insets, quieter secondary actions, compact 52-unit buttons and more legible paper surfaces. Full-width language choices prevent broken words on narrow screens.
- Functional spool art stays distinct from the cosmetic ribbon. Selected/equipped collection actions are disabled instead of doing nothing. Next delivery and Retry receive the correct primary emphasis.
- Quiet gameplay guidance backing; clearer floor/tool counters with grammatical EN/RU/UK number labels.
- Smooth continuous hemp curves with cylindrical braid shading, connected visible-board anchors, retained board spring weight, upward exchange arcs and a shorter damped landing.
- Softer region weather, slower layered clouds, gradual glider fade while holding, reduced-motion restoration, 160 ms non-blocking menu arrival and gentler press response. Effects Off now removes UI and world weather.
- Fixed-size weather pools (8/18 world particles, 8/16 UI particles); UI weather throttled to 30 updates/s; duplicate scenery painting removed. This bounds work but does not establish mobile performance.

## Native visual evidence

`PolishCapture` drives the running Unity scene through MCP and captures its real camera output. The matrix covers all 22 screen routes, all eleven device pages, phone/wide/tablet/landscape layouts, RU/UK narrow text and simulated phone safe insets. The first dialogue fixture followed an already-heard story into gameplay; confirmation explicitly opened the dialogue. Empty/populated routes and unopened/opened parcels are included. Every still capture asserts that drawing did not mutate puzzle state. Fixtures disable profile persistence.

Key files: `game-phone.png`, `game-notch.png`, `TravelBag.png`, `Collection.png`, `Workshop.png`, `languages-small-ru.png`, `dialogue.png`, `win.png`, `fail.png`, `bag-wide.png`, `bag-tablet.png`, `map-landscape.png`. The first-pass device pages retain their capture-time copy; subsequent neutral counter translations do not change their layout.

`Motion/00.png` through `31.png` record a real C03 swap, break, fall and settle. `Motion/events.csv` shows the same committed state hash in every frame, 73→49 rope vertices as the upper board disappears, and lower-board sag converging to 0.045 without being reset at animation completion.

## Validation and diagnostic history

Initial 15-case PlayMode selection: 12 passed; three three-second presentation watchdogs failed. The unchanged three-case rerun failed again. Enabling/restoring background execution in the test fixture fixed the mouse-path check but not two cascade checks. Delaying the test start did not resolve them. Exact-stage diagnostics showed 1.0006–1.0009 second Editor frame times at the deadline, not an incorrect puzzle state. `timing.csv` separately records completed C09/C32 commands and intermittent 0.9-second frame stalls.

The tests are functional ordering/input/state checks, not latency benchmarks. Their watchdog now uses a finite 30-second real-time limit, with the same state, target, input-gate and interruption assertions. A trial 180-frame guard was rejected because the Editor also runs above 1,000 Hz between stalls; that intermediate failure is retained in `../PolishFinal/playmode.xml`. Production animation timings were not enlarged to accommodate tests. Final affected run: **4/4 PASS** in `../PolishValidated/playmode.xml`, including the full 13-stage × 4-interruption-path matrix. Across the selected runs, **15 distinct cases have passing evidence** (`validation.json`); this is not a claim that the initial batch passed. See sibling `PolishTiming`, `PolishBackground`, `PolishIsolated`, `PolishTrace`, `PolishFinal` and `PolishValidated` reports for the complete audit trail.

Compilation caught one implementation mistake before visual review: a MaterialPropertyBlock was created in a MonoBehaviour field initializer. Initialization was moved to Initialize.

## Limits

Mac lock-screen was confirmed through native computer use during this pass; standalone foreground inspection remains unavailable. Physical iOS/Android rendering, touch/haptics, safe-area hardware behavior, frame-rate/thermal performance, screen readers, native-language editorial review and human understanding/enjoyment are not validated. Screen sizes and notches here are Editor simulations. No new full campaign solver sweep was run for these presentation-only changes.

## Final build

One fresh universal Mac player: `CascadeUnity/Builds/CASCADE-Polish.app`, 248,923,582 bytes, 14.24-second build, **Succeeded / 0 errors / 346 warnings**. The warnings are the existing Sentis/Metal warnings and missing iOS localization app metadata. The MCP wrapper reports warnings as an error response; the actual Unity BuildReport confirms success. Previous builds are preserved. Foreground launch/visual confirmation remains untested because the Mac is locked.
