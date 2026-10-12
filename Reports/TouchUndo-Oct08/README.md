# Touch, Undo and first-use controls — 8 October 2026

Implemented: a Canvas-scaled tap/drag threshold; a lower touch-area extension; early visual settlement for a new deliberate touch during an ordinary exchange; drag and tap/tap explanations displayed together; automatic first-available Undo lesson; a reverse animation that reserves unmoved duplicate occurrences and uses separate upper/lower paths. The oversized procedural score glint was removed. Slow frames no longer stretch the score ceremony.

Existing rules and save schema are unchanged. New acknowledgements live in the existing SeenCoachPrompts list. Undo commits its existing authoritative snapshot immediately; animation only protects presentation. Contacts, falls, fragile damage, pause/suspension and Undo animation cannot be skipped by a new touch.

Validation:
- 17 EditMode interaction/conflict tests passed (`editmode.xml`).
- 6 focused PlayMode tests passed (`playmode.xml`).
- 2 focused tests passed after return-path and cached-level refinements (`../TouchUndo-Final-Oct08/playmode.xml`).
- 1 additional held-drag check passed after refining the tutorial-off gate (`../TouchUndo-Drag-Oct08/playmode.xml`).
- Inspected the first UK Large Text explanation at 320×693: both methods and their full labels fit (`../GuidedLessons-Oct08/two-controls-uk-small.png`).
- Inspected native Undo before/moving/restored renders, including duplicate triangles, and the settled completion screen with no score diamond. One bounded visual refinement separated the two return paths.

The synthetic Undo fixture is isolated from campaign content and the user's save. Its header is a QA identifier. Native screenshots and event-handler tests do not measure physical-device touch latency, thermal behavior or FPS. iPhone build/install evidence is under `../iOS-2026-10-08-touch-undo`.
