# Island replay, world navigation and proportionate comics — 7 October 2026

Completed: tapping an opened island shows its offered levels, earned five-star
rating and best score. Only opened cards launch; the pinned level resumes its
current descent, while explicitly selecting an earned level starts replay.
Back retains the browsed world. Previous/next world controls are visible only
for opened worlds in the selected Shapes/Words/Mixed journey. Browsing does not
replace the attempt, route, checkpoint, clear receipts or mode mastery.

Existing brass/enamel buttons and star art are reused. Map controls are smaller,
Home is visually distinct from world arrows, and the map has more space with
larger separated islands. Island lists retain dark lettering over the workshop
sky, including when browsing the night world. Comic panels span the timber
opening at their authored 3:2 aspect, followed by a themed speech ribbon with
speaker portrait, complete spoken line and Continue control. NPOT resizing is
disabled on the three existing 1536×1024 atlases; their pixels and GUIDs are kept.

Validation:
- `initial-runtime.txt`: 10 focused NUnit coroutine cases executed directly in
  the running Unity Editor (4 new browse cases, 2 navigation cases, 4 comic flow
  cases); all PASS. This was not a Unity Test Runner XML suite.
- After visual corrections, the unavailable-world-arrow case passed again.
  `verified-runtime.txt`: the 27-panel/9-world comic case passed with explicit
  active-view aspect assertions, then all 9 final camera captures completed.
- Final RU390×844 and UK320×693 LargeText captures use simulated safe insets.
  Every captured button target is at least 44 pixels in both dimensions;
  no text overflow or illustration aspect failures in the final batch.
- Final captures reviewed visually; an independent reviewer checked the new
  navigation, shelf and comic blocks. These are actual Editor camera renders.
- The two intermediate layout failures are retained in small diagnostic logs:
  callback cleanup during deferred destruction, and an inactive old heading
  found before its replacement. Both were corrected and the final batch passed.

Availability/score fixtures bind real published content to valid synthetic
receipts; these screenshots do not represent the user's own progress. Test
profiles were isolated; the temporary driver was removed and Play Mode stopped.
Compilation succeeded with seven existing Presentation warnings. No gameplay
rules, prices, IDs, packages or save schema changed. Full suite, player build,
phone install, physical-device coverage, performance and human enjoyment NOT RUN.
