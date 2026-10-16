# Island replay and world navigation — 7 October 2026

Keep independent Shapes/Words/Mixed journeys, existing campaign unlock rules,
best-star/score receipts and suspended descent checkpoint. Browsing an island
or world is read-only; opening a locked node must not begin a puzzle. An opened
island lists its offered levels with earned stars and best score. Selecting an
unlocked level explicitly starts replay, or resumes the existing pinned stage.
Back returns to the same world. Visible previous/next world controls show only
opened worlds with levels in the selected mode. No new prices, gameplay rules,
packages, IDs or save schema.

Use existing brass/teal controls and trophy stars. Reduce map header/footer
bulk while preserving comfortable touch targets and safe insets. Give the map
more vertical space; preserve island artwork proportions and increase spacing
without stretching it. Comic illustrations preserve aspect in the workshop
opening, with spoken lines on the existing themed dialogue surface and a left
portrait. Preserve story receipts and equipped Mika portraits.

One focused native validation/capture batch; targeted fixes only. No phone
build or install, broad regression suite or claim of physical-device coverage.

Implementation: separate read-only island shelf, filtered current-mode level
cards with best stars/score and guarded explicit replay/resume; visible opened
world navigation and distinct Home icon. Compact map chrome keeps at least44px
buttons. Existing map fitting preserves aspect, with modest extra static spacing
along the floating chain and matching hit anchors. Comic importers now preserve
1536×1024 rather than rescaling NPOT width; a measured dialogue layout keeps
portrait, full line, illustration and Continue together. Active-view lookup and
callback teardown handle deferred menu destruction.

Validation:10 focused native coroutine cases PASS; scoped navigation rerun and
27-panel geometry/lifecycle rerun PASS. Nine final Editor camera frames reviewed,
RU390/UK320 LargeText with simulated safe insets; buttons>=44px, no final text
overflow/aspect failures. Intermediate failures and fixes are retained in
Reports/MapIslandBrowse. No build/install/fullsuite/device/performance/humanfun
claim. Test profile and one-shot driver removed; TASKS.md updated.


## Visual replacement — 16 October 2026

The old list presentation described above is superseded by the supplied island-level reference in `Docs/ISLAND_LEVEL_REFERENCE_OCT16_RU.md`. The existing browse/replay/resume/category guards remain. Native evidence and the nine-world gallery are in `Reports/IslandLevelsOct16/`.
