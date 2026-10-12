# Journey conversations — native Unity review

Unity 6000.3.8f1, isolated temporary profiles. The user's live save and Cloud settings were preserved. No player build or phone installation.

- `editmode.xml`: **6/6 PASS**, 0.5051124s. Forty distinct island receipts and eighty distinct replies; genuine resident/Mika exchanges; checkpoint cadence only between stage2/3; legacy introduction IDs do not collide; contextual visits and independent per-mode heard-story state; existing 160-stop/island boundaries.
- `checkpoint-runtime.txt`: map Talk introduces Bram/Mika, pauses input/clock, leaves the exact puzzle unchanged, and resumes the saved middle stage. An actual C02 second-stage witness triggers receipt4001 before stage3. Typing/entrance never advances the stage. Explicit Skip saves the receipt then descends. Final confirmation also checked that arrival does not mark an unread scene, repeat Talk gives the current goal, and Not now returns to the map.
- `island-runtime.txt`: actual committed C03 fixture → stats → trophy opening/claim → receipt2000 conversation → map. Disk receipt and absence of a repeated conversation on map revisit. 408 authored lines checked in native TMP across EN/RU/UK at320×693, Large Text and simulated safe insets. Later changes only adjusted full-canvas dimming, checkpoint background visibility and the shared fresh-attempt guard; text dimensions/content remained unchanged.
- `fresh-modes-runtime.txt`: first Words/Mixed travel shows a regional introduction, explicit Skip starts a three-stage descent at stage0, returning to Shapes restores its exact saved stage/hash.
- Final native screens: `visit-ru.png`, `visit-small.png`, `checkpoint-mika.png`, `portrait-pip.png`, `portrait-rue.png`, `neri-wide.png`. The Pip/Rue/Neri views deliberately mount portraits in the isolated Wind Gardens map for crop/layout inspection; they are not screenshots of their shipped region progression. `island-ru.png` and `lumi-uk-small.png` predate the final full-canvas dimming refinement.

390×844 and320×693 portrait, 1440×900 wide. Body copy, name banner, centered actions and safe inset layout were visually inspected. The generated atlas was also inspected. No whole atlas is rendered as a portrait; source Mika/Pip/Rue sprites remain unchanged. Current project compilation succeeded, final console query returned zero errors/warnings.

NOT RUN: physical Android/iPhone safe-area validation, phone gestures/build/install, full test suite, frame-rate profiling, user playtest or proof of enjoyment. Native camera renders and editor checks do not establish those results.

Final source-only refinements also update the cloned safe layout on viewport changes, block Talk during pending travel, and stop rebuilding the TMP mesh once the complete line is visible. Final compilation verified; those guards were not separately exercised on a physical device.
