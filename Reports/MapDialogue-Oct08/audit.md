# Focused native review, 8 October 2026

The first capture pass produced and inspected `01-map-ru.png`, `02-dialogue-ru.png`, `03-map-uk-large.png`, `04-dialogue-uk-large.png`, and `05-night-fail.png`. Native assertions checked that the map has no Talk button, the available world arrow has no blue face and sits above the level action, all active map button captions fit, and four complete Bram/Mika dialogue pages fit both 390×844 RU and 320×693 UK Large Text. Each screenshot capture verified the puzzle-state hash remained unchanged.

The cloud still reduced the contrast of the first night row caption. A small paper backing with dark ink replaced the shadow-only treatment; the final `05-night-fail.png` was recaptured and inspected. `06-settings-arrows.png` confirms the authored arrow art in settings. `editmode.xml` records 3/3 dialogue pagination tests passed. The final capture-only `validation.txt` covers the last night recapture; the earlier map/dialogue captures remain in this folder. Unity Console errors: zero. Editor stopped, temporary profile detached, background playback disabled.

Latest correction: `02-dialogue-ru.png`, `04-dialogue-uk-large.png` and `05-night-fail.png` were recaptured. Native checks confirm a framed speaker name overlaps both portrait and papyrus, four pages still fit, and the failure has no row label, duplicate retry summary or ribbon. The earlier `editmode.xml` still reports 3/3 pagination tests; no new EditMode tests were needed for these visual-only edits.

Not checked here: full screen gallery, phone build or physical-device rendering.
