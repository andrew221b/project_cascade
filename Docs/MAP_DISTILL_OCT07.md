# Map clarity — 7 October 2026

The map retains the selected mode, world title/goal, earned stars, one world-level
completion count and its existing travel/talk controls. Remove the duplicate
campaign count, next-world countdown, task sentence and island count/arrow.
Mika's original alpha silhouette receives a soft pulsing halo, steady with
Reduced Motion. More space goes to the map; buttons retain 48-point minimum
hit areas. Save data, map unlocks, story and level rules remain unchanged.

Before changing identifiers: add only the UI translation key
`Completed: {0}/{1}` for this single concise progress line. Existing Mixed key
is retained; displayed values become Mix / Микс / Mix. No content, store or
campaign IDs change. Validation is recorded in TASKS.md and Reports/MapDistill.

Native Unity through CUA MCP: two bounded capture passes, RU390×844 and
UK320×693/Large Text with simulated safe insets. Nine focused runtime checks
passed: removed duplicates, supported/nonraycast halo, changing pulse,
steady Reduced Motion, translated Mix, no overflow and unchanged pinned row.
Captured button bounds preserve 48px targets and at least 8px between tabs.
The original map aspect ratio is preserved; its viewport height grew from
47% to 63% of the safe root. Header Back and bag are matching 48px squares.

The first pass found the halo too faint; the final batch increased its alpha
and contour spread. Final captures and an independent copy/layout review found
no overlap/control regression. The halo remains intentionally subtle on320px;
still screenshots do not prove subjective animated prominence. The runtime
renderer alpha pulse was measured. Temporary capture code was removed.
No phone build/install, device motion, full suite or FPS benchmark was run.
