# Night world, story and resume — native validation, 7 October 2026

The running Unity 6000.3.8f1 Editor was operated through CUA MCP after the
Unity provider had timed out. A temporary isolated-profile Editor coroutine
invoked the existing compiled NUnit methods and UnityTest coroutine fixtures
by reflection, then captured the actual native Game view. This is focused
native execution, not a full Test Runner suite or player build.

Passed in the initial native pass, retained rather than needlessly repeated:
- 11 pure story/map/localization tests.
- 1 preservation/unique-layout test and 48 night source/minimum/descent tests
  (two methods for each C161–C184). Every source and EN/RU/UK binding replayed
  its published shortest route; all 24 three-stage descents were calibrated.
  The original 637 puzzle pins remain unchanged.
- 8 of the 11 navigation/story-flow coroutine fixtures.

The focused correction pass completed the other 3 fixtures: wrong deliveries
now traverse all remaining floors in the test, unlocked arrow browsing begins
on the second island, and all-island coverage uses Mixed instead of excluding
original glyph nodes through Shapes. All original recovery, row, attempt,
stage, receipt and gate assertions remain. Total: 7 navigation and 4 comic-flow
fixtures passed, including profile reload, all 27 comic panels, 45 island
openings, occasional exchanges and independent mode story receipts.

Seven native views passed TMP overflow checks: night map, comic and small
comic/map, first-world comic, night gameplay and Aster dialogue. Portrait sizes
390×844 and 320×693 include simulated safe insets; small views use Large Text.
The final correction recaptured only the two night comic sizes. Exact passes
are in Reports/NightJourney/checks.txt; prior passes are identified there.

Visual correction findings: preference changes no longer reset ivory map text
to dark ink; the locked-island sky mask supports dark blue; headers/images stay
inside their frame; moving clouds do not cross comic reading lines. The fresh
Impeccable reviewer scored all four findings resolved after two bounded rounds:
Docs/NIGHT_FINISH_REVIEW.md. This is a scoped static-capture verdict.

An initial startup null reference in the new result contrast branch was caught
and fixed by waiting for flow initialization. The final native runs completed.
All seven generated rasters have embedded prompt provenance; the bounded scan
reported 7 rasters, 0 missing. Old eight music WAVs retain their HEAD bytes.
The ninth score was imported as Streaming/Vorbis and linked in ToyAudio.

Not run: full EditMode/PlayMode suite, physical touch/gyro, mobile performance,
Android/iPhone builds, live advertising/purchases, human enjoyment testing or
in-game subjective music listening. Screenshots do not prove animation quality
or absence of every bug. Temporary runner removed; the Editor was stopped.
