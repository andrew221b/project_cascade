# Map pacing and earned celebrations — 6 October 2026

Design note before changes. Experience surface: one illustrated world, a clear
current island and one level action. Remove the cream heading/footer/marker
cards specifically on the map; retain the authored sky, dark ink, brass and
teal/amber controls. The level action leads; story is secondary. Insets and
minimum 48-pixel touch targets survive short screens and Large Text.

The existing campaign has 160 distinct published levels. Instead of reusing a
world map every four/five levels, show one map per whole region: 15,15,10 and
24 in each remaining region. Each of the five illustrated islands groups two
to five full levels. The player must complete those levels before moving to
the next island; the world counter shows remaining levels before the next map.
Existing 33 story beats still trigger at their original level boundaries.
No IDs, definitions/hashes, region order, rewards, store prices or save schema
change. Old clears remain valid; no new filler copies or invented clears.
Opened-level arrows permit replays without exposing locked levels. Current
marker and Mika follow the active island, not one island per level.

Motion thesis: the lantern's light travels through the award stars, unfurls the
ribbon, then settles. Opening a parcel has an anticipation, a distinct opening,
and a gift rising out of its mouth; confetti marks the reveal rather than looping.
Finite presentation only, skippable through normal navigation, pause/focus and
Reduced Motion supported. Rewards are committed before the ceremony and never
owned by tween/coroutine completion. Bound decorative objects; no new packages.

Validation: tests for world/island coverage and legacy story boundaries, native
map views at small/normal phones, timed award and parcel samples, final settled
poses, interruption/reduced motion, unchanged puzzle state and receipt counts.
No phone build/install or claim of human enjoyment/performance.
