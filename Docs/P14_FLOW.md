# P14 journey and direct-play implementation

Updated 5 October 2026. This author-requested revision supersedes the earlier
button-heavy P14 gameplay and Home layout. It extends Sky Wind Workshop with
Mika's route, three original regional art kits and direct manipulation. Existing
puzzle rules, prices, package pins, schemas and content IDs are unchanged.
`Docs/JOURNEY_REDESIGN.md` is the staged design decision;
`Docs/JOURNEY_SURFACE_BRIEF.md` is the current surface contract;
`DESIGN.md` and `.impeccable/design.json` describe the implemented visual system.
There is no approved comp; this is a native Unity/uGUI, code-led extension.

## Implemented flow

Home opens the isometric journey. Eight pages of five numbered stops bind the
existing 40 primary lesson puzzles: Wind Gardens covers stops 1–15, Rain Mill
16–30 and Copper Orchard 31–40. Each region has its own original background,
map illustration and character portrait. Regional maps repeat across their
five-stop pages. Mika marks the actual next session stop; selecting or browsing
a future page cannot move him or award progress. Future pages explain the
remaining stops and offer Back to my path. Available actions distinguish Let's
go, Continue and Play again. Lessons keeps the existing chapter/mode browser.
The C26 shapes lesson is disclosed and skippable on the Words route.

Mika, Pip and Rue introduce each five-stop page with two short optional dialogue
lines. Story entry explicitly applies the stop's region. Continue or Skip story
starts the concrete pending puzzle; reading Story from the map returns there.
An open dialogue blocks puzzle input. Objectives give the existing puzzles
story context; there is no implemented quest ledger or world-restoration save.

Gameplay presents the destination, one pause control, the complete machine and
receiver, and Undo, Hint and Descend/Deliver. Pressing a piece immediately lifts
and enlarges it with its live glyph; moving toward an adjacent piece previews
legality and durability cost. Release on that neighbour commits one atomic
exchange through the existing resolver. Cancel, release elsewhere, pause or
focus loss returns the held presentation without spending a move. Tap-then-tap
remains available; a long swipe does not chain exchanges. Long rows retain
stable hit regions and an enlarged held preview. Precise input is an explicit
Comfort setting, off by default for every row length. Target zoom remains.

Pause provides Resume, Retry, Devices, Settings and Home. Result controls wait
for visual synchronization and preserve free Undo/Retry. Success includes Mika;
failure shows the delivered row beside the goal for comparison. The device
guide uses short explanations. Optional contextual coaching and an original
glove demonstrate an exchange without issuing a command. Idle assistance only
accumulates during foreground play. Repeated invalid actions and losses offer
help rather than changing rules.

Settings control music, sound, haptics, reduced motion, optional screen shake,
effects, high contrast, larger text, precise input, tutorial help and dead-end
warnings. Preferences, clear flags, dialogue history and resume state are
in-memory. A solver dead-end probe does not reveal or charge a hint.

## Art and motion

The machine retains real beveled meshes, stable symbol identities, Balsamiq
Sans, compact enamel rails, brass hardware and soft contact shadows. Rail
materials change with the three regions. The 12 original journey rasters and
embedded generation provenance are documented in
`Docs/JOURNEY_ASSET_PROVENANCE.md`; no user asset or prior artwork is replaced.

Each far landscape covers the viewport. Shared transparent clouds drift at
three speeds; lower-edge plants sway from their stems. The original windwheel
remains visible lower-right. The whole rotated glider fits in the empty strip
between the feedback band and top floor; it hides while a token is held and
when the strip is unavailable. All scenery is presentation-only. Pause and
application suspension freeze phase progression. Reduced Motion shows a fixed
ambient phase and replaces causal travel/deformation with brief state holds.
Map and dialogue are static ambient views while puzzle interaction is paused.

## Scope and evidence

This is the P14 revision only. The 160-stop/eight-region campaign is a design
target; the runnable content is the existing 40 short lesson puzzles. Deeper
puzzles, durable progress/preferences, keeper ranks, quests, parcels, keepsakes,
records/streak progression and Long Reach are not implemented by this revision.
Long Reach requires a complete Domain/resolver/solver/Undo/save change before
use. Online remains design-only. Full EN/RU/UK UI and native glyph/editorial
review remain P15; durable storage remains P16.

`Reports/Journey` holds native Editor captures and adjacent metadata at
390×844, 320×693 and 1440×900. The 22-view set was
refreshed at 07:13:05–07:13:13 UTC after three reviewer corrections: honest map
marker/future footer, explicit story region and bounded glider flight. The
review record `Reports/Journey/impeccable-review.md` held a fix verdict pending
the same reviewer's follow-up. This document does not convert that verdict to approval.

The final expanded PlayMode run passed 36/36 tests (07:14:25–07:16:15 UTC),
including the three review regressions, with current compilation. The prior
309/309 EditMode result remains the rule/domain checkpoint; this UI correction
batch did not change Domain. A teardown-camera access failure found during the
expanded run was fixed; its reproduction remains in
`Reports/Journey/reviewer-fix-repro.xml`, and the successful rerun is
`Reports/Journey/playmode.xml`. This documentation-only pass did not itself run
Unity tests; it records the parent integration run and its saved result.
Final results and refreshed review evidence must be read from their reports
and TASKS. Editor screenshots and automated checks do not establish physical
iOS/Android touch, haptics, OS safe-area/back behaviour, memory/performance,
build readiness, animation smoothness, native-language quality or human
engagement/enjoyment. The narrow token cells are not covered by a Button-only
touch-target audit.
