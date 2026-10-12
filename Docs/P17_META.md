> Current implementation update (2026-10-05): see [CRAFT_REWORK.md](CRAFT_REWORK.md). Six Ribbon Reach trials are implemented; wide scenery uses one continuous field. This document retains its earlier checkpoint.

# P17 — rewards that belong to Mika's journey

5 October 2026. Design note before adding optional profile fields and presentation.
This is an extension of Sky Wind Workshop. No puzzle rule, price, existing ID,
package pin or online service changes. Long Reach remains design-only until P20
implements its shared resolver/solver/save contract. Hints and Undo stay free.

## Purpose and constraints

First clear opens the path. An optional stamp contract and a shorter route give
replay a different purpose; neither blocks ordinary travel. Mika earns keeper
ranks from actual restored lights and completed character requests. No loss or
missed day resets an order, rank or gift. Only local personal records are shown;
no invented opponents, world leaderboard or network implementation.

Four workshop miniatures correspond to ten real lesson nodes each: Bridge
Lantern, Rain Pump, Post Glider and Beacon Model. A miniature is complete only
at ten of its own node receipts. The Words skip of C26 must remain 9/10 until
that shape lesson is actually cleared. These are current 40-lesson rewards,
not a claim that the planned 160-stop campaign already exists.

An order uses three NEW node clears. A known selected gift gets one fragment;
three fragments finish the gift. Four gifts: Leaf Pin, Sun Button, Sky Ribbon,
Moon Patch. Earned parcels are a presentation of those saved order receipts,
not random paid boxes. Preview the named gift; opening is idempotent, never
rerolls, and never generates duplicate filler. Pending fragments count toward
three-per-gift capacity. After all twelve fragments are earned, this gift set
is explicitly complete; further wins do not advertise a nonexistent parcel.
Player selection directs the next order; receipts already earned keep their gift.

Four keeper promotions follow 3, 10, 25 and 40 keeper points: one per new light
and two per completed character request (short titles, no second currency bar). Optional character requests recognize successful deliveries
with real device outcomes/stamps on distinct nodes, not repeated failing actions.
Rewards are one-time keepsake/journal milestones. A forgiving seven-visit stamp
ladder records distinct local dates, never resets, and makes no anti-cheat claim.

Records use PuzzleId + ContentHash + mode + puzzle locale + objective. Remaining
route length is independent of UndoCount. No-hint and clean attempts are separate
annotations. A stamp contract uses its own goal and solver proof; ordinary Won
still grants a first clear even if the optional stamp was missed.

The existing economy hypothesis remains 40 coins per first clear and 100 per
complete ten-node chapter, so the forty-node total is 2000. Arithmetic is tested
as a local ledger; no paid hint, shop purchase or advertising SDK is introduced
here. Functional puzzle tools from parcels are deferred to the complete P20
Long Reach contract rather than exposed as unusable buttons.

## Native UI and art

Extend existing paper/mint/brass controls and Balsamiq text. An optional travel
journal links rank/order, one character request at a time, workshop, collection
and personal routes. Reveal these after initial play; do not put five progression
meters on the puzzle HUD. Keep original direct manipulation and three actions.
One large item illustration, a short purpose and a clear action are preferable
to a wall of identical reward cards. Final wording EN/RU/UK stays child-readable.

New transparent art: four workshop miniatures, one earned parcel and four gifts.
Keep the same warm handcrafted storybook materials and lighting as Mika's world;
no ShapeMania asset copying, rendered words, pseudo-lettering or rarity neon.
Miniature stage views may show clearly disclosed construction progress; only the
complete stage is called built. Gift artwork is used in the collection and a
small chosen keepsake marker beside Mika; it never changes symbol identity.

## Save compatibility and acceptance

Optional profile fields have defaults when absent in schema1 saves. First-clear
receipts remain the authority; initialization does not regrant them. New parcel,
quest and collection receipts are persisted in the same atomic transaction as
their triggering win/claim. Record timestamps are not competitive trust evidence.
Any incompatible payload keeps the existing explicit recovery route.

Tests: rewards across modes/locales/Undo/retry/reload, pending parcel capacity,
idempotent claim, exact workshop membership, optional-goal records, no punitive
streak, 2000 arithmetic; actual UI claim/selection/contract flows and small-screen
EN/RU/UK visuals. Human enjoyment, native editorial and hardware testing remain
separate from algorithmic proof. This document is a design contract, not completion.

## Implementation checkpoint

JourneyRewards owns idempotent receipts, known selection/capacity, unique delivery
facts and goal-specific records. Profile schema1 adds optional defaults; computed
values are omitted from JSON. CascadeGame commits clears/meta before animation.
The travel bag appears from map Menu; journal links unlock at three lights.
The collection equips owned keepsakes beside Mika, while workshop shows clearly
disclosed unfinished/ready views. Stamp trail C35 is four actions versus two for
normal delivery and keeps the ordinary path open.

Reports/P17/README.md contains actual tests, focused navigation correction and
41 native visual fixtures. Independent reviewer scored documentation currency and the three requested carousel frames resolved; disposition ship at P17 scope. All 44 native captures were inspected.
