# Star Observatory and journey story

Design note, 7 October 2026. The ninth world is a night epilogue after the
Great Beacon. Its light makes the islands visible again, but travellers above
the cloud line still need the constellations. Aster, a careful owl astronomer,
asks Mika to help restore their routes. Mika leaves after a rest at home;
the original homecoming remains a complete story.

Append the world at stop 160 with 24 published levels and five islands.
Existing worlds 0–7, stops 0–159, pages 0–32, campaign node IDs and story
receipts retain their meanings. New world index 8 uses StarObservatoryMap,
the title Star Observatory and resident Aster. No puzzle rules or save schema
change. Old completed journeys can continue into the epilogue.

Every world has a three-page introductory comic. Every island has its own
opening conversation; existing island completion conversations remain separate
payoffs. Occasional conversations between levels reflect the resident's worry
and progress. Reading, replaying or skipping these scenes never changes a row,
spends a move or completes a level.

Receipts use the existing per-mode HeardStory set: world comics 6000–6008,
island openings 8000–8084, between-level scenes 7000–7183. Existing introductions
use page receipts, island payoffs 2000–2084, and checkpoints 4000–4183.
All new receipts remain inside the existing profile validation limit.

Character arcs: Bram learns to trust repaired bridges; Pip replaces haste with
care; Rue protects tomorrow's seeds as well as today's fruit; Sora writes the
letter she has postponed; Neri gives small boats a dependable route; Toma lets
his maps leave their shelves; Lumi accepts company when visibility fails;
Gran lets Mika carry the keeper's responsibility; Aster learns to share an
imperfect observation instead of working alone until it is perfect.

English source text is the UiCopy localization key. New EN/RU/UK entries are
provided separately in Data/star_story_localization.tsv for import into the
existing localization tables. Pure C# verifies coverage and receipt separation;
comic composition and conversation timing also require a Unity visual check.
