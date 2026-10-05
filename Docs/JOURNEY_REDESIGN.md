# CASCADE — Mika's journey

Author-requested scope revision, 5 October 2026. This supersedes the P14
button-heavy gameplay layout and expands P14–P20. Existing content IDs,
packages and the persistent-row rules stay intact. This is a design decision,
not a claim that the features below are already implemented or fun-tested.

## Why the journey matters

Mika (Мика / Міка) is a small dormouse, an apprentice lighthouse keeper. A
storm scattered the lights that let floating islands find each other. Mika
carries the first light toward the Great Beacon, where his grandmother is
waiting to help relight it. Mika fears heights, mends his own worn scarf and
helps others before admitting he needs help himself. Small, capable, imperfect;
never a mascot who only congratulates the player.

Every machine restores something visible: a footbridge, a water pump, a lift,
a post route. Order serves the island's people. Restoring the final beacon
opens journeys to further islands; the story has a conclusion, not an endlessly
withheld destination. Short optional dialogue, one idea per sentence, skippable
without losing objectives. No puzzle input behind an open dialogue.

## Campaign and challenge

Target: eight regions, twenty authored/audited main stops per region (160),
plus optional quests and later free daily puzzles. This is a content target,
not a generated-count claim. A stop cannot unlock until its concrete puzzle
passes validation. Existing C01–C40 remain available as lessons and regressions.

Regions: Wind Gardens, Rain Mill, Copper Orchard, Paper Harbour, Glass Marsh,
Night Observatory, Snow Bridges, Great Beacon. Each region has its own NPC,
restoration objective, platform materials, layered atmosphere and a new
combination of familiar devices. Start with three art kits, extend later.

Pacing: teach one thing, use it with an old thing, choose between two routes,
then a short story payoff. Do not mistake an eight-letter row for difficulty.
Later main puzzles aim for 4–9 meaningful commands with multiple viable first
choices. Audit shortest paths, dead ends, decision forks and removal of a
device. Reject padded loops, redundant devices and merely renamed duplicates.
Optional stamps can cost a longer route; no perfect score is required to travel.
Show the next concrete destination and remaining stops, not an arbitrary power
number. Quests use actual events (repair, rescue, stamp, delivery), not timers.

## Direct play contract

One large active row. Touch and hold a piece: it lifts immediately with its
letter. Move toward a neighbour: the neighbour and legal landing light up,
with a small preview of the board cost. Release there: one atomic exchange.
Release elsewhere, cancel, lose focus or pause: return without spending a move.
No long-swipe chain. Tap-then-tap remains possible; precision arrows are an
explicit accessibility setting, never automatically forced by row length.

Gameplay HUD: destination/title and one pause button; the machine and full
target dominate. Bottom: small Undo and Hint controls plus a clear Descend
action. Retry, settings, guide, map and mode selection live in pause/map.
Do not show slot coordinates, debug navigation or motion controls. Expand
short rows and leave space around them. Long rows receive a lifted preview and
accurate direct hit areas. Reduced Motion preserves feedback without travel.

## Progress, parcels and tools

Mika gains keeper ranks from first clears and quests. Ranks unlock journal
pages, scarf/bag decorations and side paths, not stronger base puzzle rules.
Save first-clear rewards exactly once. Undo/replay cannot farm rewards.

Earned parcels: each checkpoint grants a guaranteed new keepsake plus a
clearly named tool. No paid boxes, no duplicate filler, no countdown or loss
of rewards for missing a day. The collection explains the places/people.
Contents follow a saved reward sequence; reopening cannot reroll or duplicate.

Proposed tool **Long Reach**: one charge exchanges pieces two slots apart;
later variant three slots apart. This is a NEW RULE, not a UI shortcut. Before
implementation extend immutable state, command validation, shared resolver,
solver enumeration, hash, Undo/replay and persistence together. No bypass of
locked/linked/bridge edges without a separately specified rule. Safer initial
restriction: usable only across ordinary open, unlinked edges; pay the sum of
crossed edge costs; run contacts once on the final row. One use, not two/three
hidden adjacent commands. Tool-assisted records are separate from unaided
mastery. All campaign puzzles must be solvable without tools. This rule remains
DESIGN ONLY until its complete test suite exists.

Streak/order: three new deliveries fill a visible order. Undo, Retry, a loss or
a missed day never erase it. A seven-visit stamp ladder is cosmetic and forgiving.
Leaderboard work must inspect ShapeMania's principles read-only; no fake human
opponents or invented scores. Local personal bests can precede real services.

## Online concept — DO NOT IMPLEMENT

An asynchronous relay: 2–4 friends each repair a different station on the same
route, exchanging one optional hint postcard. Everyone receives the same
seeded rules; no latency during a puzzle, no attack that ruins another row.
A weekly shared restoration reveals a landscape rather than selling power.
An optional equal-loadout challenge ranks verified command logs by completed
objectives, then commands, not speed. Server validation, moderation, friend
privacy and reconnect/save conflict design are prerequisites for later work.
No networking package, backend, matchmaking or simulated online population now.

## Living world and visual direction

Extend the existing bright, hand-painted Sky Wind Workshop with Mika's travel
story. Keep the distinctive silhouettes, mint/coral/brass roles and friendly
Balsamiq lettering. Render lighting gives pieces a bright readable face, soft
contact shadow and edge highlights. Decorative movement never covers a symbol.

Separate background assets: calm far sky/landscape; transparent clouds; plants
at the lower edges; a small glider; existing windwheel in the visible lower
right. Different speeds create depth. Cloud wrap occurs offscreen, plants sway
around stems, glider follows a bounded arc. Freeze ambient on pause/background
and with Reduced Motion. No runtime masking of painted organic objects.

Map: isometric islands connected by a readable path; Mika's marker moves only
after saved progress. Current stop is obvious, future regions are previews.
Dialogue: character portrait beside a low paper-and-brass JRPG-style frame,
large short text, one continue button and a visible skip. Map, journal and
dialogue must work at 320px without piling panels over one another.

## Delivery sequence / validation

1. P14 revision: direct manipulation, compact HUD, simple copy; first layered
   zone and Mika portrait; map/dialogue/quest flow with concrete content.
2. P15: EN/RU/UK UI, independent puzzle locale and full glyph/long-row QA.
3. P16: versioned durable progress/preferences, corruption recovery, receipts.
4. P17: quests, keeper ranks, collection/parcels, local records and streak.
5. P18: input/lifecycle/build/performance/visual integration, human playtest
   limits stated. Do not equate solver victories with enjoyment.
6. P19: monetization boundaries only until core is accepted; no new prices.
7. P20: audited deeper campaign expansion; Long Reach only after full shared
   transition/save/solver support. Online remains a document only.

Each phase updates TASKS and evidence. The latest user explicitly rejected
the previous game's simplicity and crowded UI; prior visual approval does not
approve this revision. Fresh independent Impeccable review is required.
