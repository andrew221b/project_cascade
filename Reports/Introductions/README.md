# First mechanic explanations — 6 October 2026

Implemented in the incumbent Unity presentation, using actual device examples
and original tool artwork. No new raster assets, packages, puzzle rules or prices.
Schema 1 gains optional, validated SeenMechanics metadata. Missing metadata
starts unseen. Persistent tutorial identifiers are documented in
Docs/FIRST_MECHANIC_POPUPS.md.

## Executed evidence

- EditMode **18/18 PASS**, 18:41:42–18:41:43 UTC: MechanicLessonTests,
  SaveTests and TutorialCoachTests. Detector coverage, duplicate rejection,
  legacy defaults, metadata round-trip and incumbent save/coach checks.
- Final MCP native Play Mode review **PASS**: all twelve puzzle effect lessons,
  first real ambient gust, first landing wind and all three shop tools.
  Actual modal button callbacks block pointer/commands; row hash and wind clock
  remain unchanged while reading. Focus loss/return keeps the gate. Explicit
  acknowledgement saves discovery; known effects do not repeat.
- Wind on descent waits before stage advancement. Acknowledgement continues
  that explicitly requested descent. Ambient wind resumes after acknowledgement
  without changing order. Tool acknowledgement resumes the requested Use once;
  an interrupted tool popup neither learns nor consumes stock.
- Optional hints were disabled during the review; introductions remain required.
  Animated examples own a separate cosmetic clock. No animation completion
  issues puzzle commands. All active popup text was checked for overflow.
- Final Editor script compilation succeeded; MCP console returned 0 error/warning
  entries. Play Mode stopped; isolated review profile override cleared.

Two early runtime checks incorrectly expected a bomb in C03's warm-up stage;
the bomb belongs to a later stage. The review fixture was corrected, then the
complete final review passed. An NUnit collection assertion initially failed
compilation and was corrected before the 18-test passing run.

## Native views inspected

| Capture | View |
| --- | --- |
| bomb-ru.png | Bomb, RU, 390×844 |
| hourglass-uk-small.png | Hanging hourglass, UK Large Text, 320×693 |
| landing-wind-en.png | Landing wind before descent, EN, 390×844 |
| gust-en.png | First actual ambient gust, EN, 390×844 |
| scroll-ru.png | First Wayfinder use, RU, 390×844 |

Simulated safe insets are 1.8% left/right, 4% top and 6.6% bottom. The modal's
acknowledgement measures 84.5 pixels high at 390 and 69.4 at 320. These are Editor
pixels, not certified hardware touch units. Five capture/bounds pairs are retained;
the final pass overwrites these same files rather than adding duplicate batches.

Not run: full project suite, player/iPhone/Android builds, installation, physical
touch/gyro, OS accessibility, mobile performance, or human comprehension testing.
User saves and the pre-existing Cloud setting were preserved.
