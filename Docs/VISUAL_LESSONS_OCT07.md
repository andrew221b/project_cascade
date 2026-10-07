# Visual first encounters — 7 October 2026

Replace text-heavy first encounters with one short looping miniature: start,
finger action, visible consequence, a held result, then reset. Close returns to
the actual screen with the same finger above the relevant piece or control and
a restrained silhouette highlight. No tutorial gesture issues gameplay commands.
Contact lessons appear on their active board, entry lessons before the next
landing, wind before its first relevant gust, and tools before their first use.
Show only one lesson at a time. Practice observes accepted real actions; closing
a demonstration does not consume tools, move pieces or mark practice complete.
Interrupted lessons remain available. Completed lessons use existing SeenMechanics
metadata and do not repeat. Existing acknowledgement records remain compatible.

Before identifiers change: append persistent MechanicLesson values 20–25 for
swap, descent/wood, undo, hint, far swap and toolbag. Never renumber values 0–19.
No save schema version, puzzle/store ID, package, economy or Domain/Solver rule
changes. Add only concise UI translations required by the new demonstration.
Pure lesson completion observes the same real transition outcomes as gameplay.

Typography keeps the established title and reading faces. Informational stage,
level and outcome labels receive shared readable raised lettering, warm edges
and restrained shadows; values and labels have distinct hierarchy. Use text,
not bitmap digits, to preserve localisation and text scaling.

Validation scope covers isolated demonstrations, first-use gating, real practice,
interruption/replay, state/stock preservation, focus and Reduced Motion, plus settled native visual poses and a targeted correction pass. No phone build.

Implemented lettering uses five cached TMP SDF roles on the existing Balsamiq
Bold face: teal information, gold numbers, parchment titles and ivory enamel
labels plus contrasting sky numbers. Fine warm edges and a soft downward underlay preserve readable contours
without duplicate text objects or image digits. Completion captions increase
to 20 reference units; supporting labels to 16, metrics to 27 and rewards to 30.
The result ribbon uses a compensated 17–24 range, with existing Large Text
scaling and normal wrapping retained for the longer RU/UK stage phrases.
The narrow ribbon keeps the text within its painted centre and permits two
lines rather than shrinking it to the previous 13-unit minimum. Material
ownership ends with the existing completion-art cleanup. Portable compilation
passed with the seven incumbent warnings. Native review and its limits are
recorded below and in Reports/VisualLessons/README.md.
Static font-metric review used the actual Title asset: its 96-unit line height
at an 80-unit point size gives 20.4 units per line at the 17-unit minimum.
The longest word in the actual RU/UK outcome phrases measures 90.6 units there;
normal wrapping can keep the longer stage phrase on two lines. This confirms
the intended wrapping strategy, not device fit or the native rendered result.

The pure completion observer handles accepted arrival contacts on the next
board, while requiring the teaching board for ordinary command completion.
Leaving a board retires its local contact/constraint cue; it cannot complete
Undo, Hint, tools, toolbag or ambient wind. Those finish through their explicit
successful application action. Retrying clears unfinished practice. Checkpoint
entry learning persists together with the mounted next attempt. Read-only
solver guidance never falls back to an unverified Drop; an unanswered search
shows no finger until a valid command is available. Far-swap guidance requires
a legal reach and a verified continuation.

A single-board miniature centres its action vertically; landing/contact/tool
films retain room for the actual consequence below. The SDF outline keyword is
explicitly enabled so gold score remains readable on enamel. Values directly on sky use a teal fill with a restrained warm edge and soft underlay.

Validation: native Unity TestRunner 19/19 focused cases passed; independent
isolated resolver fixture 234 cases passed; native isolated input review 36
checks passed. Settled films/guide/live swap were independently inspected in
EN/RU/UK, including 320px Large Text and simulated safe areas. Statistics were
inspected after the ceremony reached its completed pose. Gold values on sky
required a separate contrast role; the final unperformed-gesture retirement compiles and passes portable focused
tests, but was not exercised again in native Editor because the Mac locked.
The speculative extra contour refinement was reverted to the independently
approved, natively captured sky-number material; no unverified type variation
is shipped. The earlier
native19-case XML predates those final gesture assertions; the updated19-case
portable fixture passes. Full suite, phone/build/device comprehension not run.
