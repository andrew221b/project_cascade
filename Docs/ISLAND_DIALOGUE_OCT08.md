# Island dialogue expansion — 8 October 2026

Add one brief two-line exchange around the midpoint of each of45 islands,
before the sixth canonical level of12. Keep the existing island openings,
payoffs, world comics and nine later-world asides. Each resident's voice follows
STORY_ARC_OCT07.md; the new lines show partial progress, a personal concern or
a changing relationship with Mika, without announcing an unfinished repair.

Reuse the existing LevelAside selector and published receipt formula
`7000 + stable campaign node number - 1`. Previously published receipts retain
their text and position. New scenes use unused values of that same formula;
no save schema, catalogue, puzzle rule, price or package changes. Heard scenes
remain stored independently for Shapes, Words and Mixed. The map and level
entry share the selector, so closing and reopening cannot offer two copies.

Use the existing animated paper strip, left portrait and Next/Not now actions.
The map offers a scene after results and any reward flow; it cannot interrupt
an active move or the mandatory mechanic demonstration/practice.

Dialogue layout now recalculates after the localized line arrives and reserves
more space above its44px controls. The localization listener belongs to that
dialogue instance and is removed on close. This also applies to the existing
paper conversations; comics retain their own scrolling reading area.

Validation:5 focused native EditMode checks and2 PlayMode flow checks pass.
The latter cover the new midpoint and the old later aside, saved once-only
delivery, replay and unchanged puzzle state while reading. Updated older test
assumptions: a visit's goal may belong to either speaker, and island lists have
no map footer. Production goal/navigation behaviour was not changed for tests.

All270 new localized spoken lines are measured with a real320×693 render target,
Large Text and simulated safe insets: no glyph/bounds or footer-gap violations;
minimum gap8.00px, puzzle hash unchanged. Twelve native captures include all
nine resident portraits, Ukrainian Mika and standard390px English. Text uses
the shared InterfaceBody font; other roles keep InterfaceTitle/StorybookDisplay.
All45 midpoint nodes are available in Words for EN/RU/UK, as well as the full
Mixed route; heard receipts remain isolated by mode. Final Console is clean.
Screenshots use Reduced Motion; no new animation timing, full-suite, physical
device or live cloud proof. Visual world availability is seeded only in a
temporary profile. Play is stopped and the profile override cleared.
Evidence: Reports/IslandDialogue-Oct08.
