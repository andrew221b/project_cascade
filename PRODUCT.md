# CASCADE

<!-- impeccable:product-schema 1 -->

## Platform

adaptive

Portrait-only Unity mobile game for iOS/Android with desktop Editor/mouse support. This is a
custom game surface, not a native productivity app. Platform safe areas, touch
targets and reduced motion apply; gameplay follows the user's game brief. Mobile
PlayerSettings enforce portrait. The retained desktop-wide reference is
1440×900; short desktop windows are not a supported layout baseline.

## Product Purpose

Reorder one persistent row across destructible platforms and deliver that exact
row into a stable receiver. The interesting choice is which exchange belongs on
this floor, which belongs below, and when to give up remaining durability.

## Capabilities and Constraints

Unity 6000.3.8f1, URP, orthographic camera, uGUI/TMP. Domain and Solver are pure
C# with one transition function. No matching, clearing, spawning puzzle pieces,
grid board or gameplay physics. Shape/word/mixed puzzles share SymbolId.
EN is default, RU/UK additional, UI and puzzle locales independent. Free Undo
and Retry, solver-verified hints. Source rules, IDs and package pins are stable.
P15 implements EN/RU/UK menus, guidance and story copy. A menu-language change
keeps the current attempt and exact symbols intact; a confirmed word-language
choice applies to the next started puzzle. Translations remain editorial drafts.

## Brand Commitments

The author requires unique CASCADE artwork and a bright cartoon theme, with
ShapeMania as a reference for drawing character and broadly related colors.
Do not transfer ShapeMania screens, characters, sprites, props or materials.
Compact platforms, an unmistakable receiver, authored background, ambient
motion and coherent effects are required. The author delegated theme selection
and implementation and requested following P01–P20 without approval pauses.

## Operating Context

Short portrait puzzle sessions; touch or mouse. Direct dragging is the default;
pressing a piece lifts it with its glyph. All floors and the complete target stay
visible. Long rows offer a larger held preview, target inspection, tap-then-tap
and optional precision controls under Comfort. Precision controls are never
forced by row length. Ambient movement must not obscure gameplay information.

The implemented P14 journey gives the existing 40 lesson puzzles Mika's story,
a five-stop-page isometric map, optional dialogue and three original regional
art kits: Wind Gardens, Rain Mill and Copper Orchard. P16 adds a durable local
profile for preferences, preferred mode, heard dialogue, first-clear receipts
and a pinned attempt. Restore verifies the saved command route and exact state,
reconstructs Undo history and retains hint/Undo flags. First-clear receipts are
deduplicated by campaign node across modes and locales.

The local profile uses a checksum, a flushed temporary write and atomic primary
replacement with one valid backup. Recovery explains a restored backup, an
unavailable attempt, an unreadable path or a failed write; newer save versions
remain read-only. The checksum detects accidental corruption, not cheating.
P17 adds the travel bag and optional journal after three restored lights. Every
three new node clears earns one saved parcel fragment for a known gift; three
claimed fragments make an owned Leaf Pin, Sun Button, Sky Ribbon or Moon Patch.
Selection directs the next order, pending fragments count toward capacity, and
opening a receipt is idempotent. An owned keepsake can accompany Mika on the map.
Collection and workshop each show four selectable object thumbnails together,
a large selected object, and its saved progress and available action.
Four workshop miniatures each require their exact ten lesson-node receipts;
Words skipping C26 does not complete its ten-node workshop. Keeper rank reflects
restored lights and three requests based on repair, bridge and stamp outcomes
in successful deliveries. Seven distinct local visit dates accumulate without
missed-day loss. Personal route records distinguish puzzle, content, mode,
puzzle locale and objective; optional solver-proved stamp trails keep ordinary
travel open. These local milestones remain available across the expanded campaign; the four workshop miniatures still refer to their original ten-lesson ranges.

First-clear and reward changes enter the same local profile transaction before
command presentation; parcel claims call the save path before their arrival
animation. Failed writes remain explicit and do not become a durable-save claim.
The original 40 lessons remain the tutorial route. P20 adds 120 primary puzzles
C41–C160 with solver minima of 5–12 actions (mean 7.8667), plus ten optional
stamp detours in the new content. The catalog contains 160 primary puzzles and
477 localized bindings, 637 concrete definitions; locale bindings are alternate
renderings of campaign nodes, not 637 distinct campaign stops. The eight-region
journey spans 33 map pages. Wind Gardens, Rain Mill and Copper Orchard retain
their original art; five new original Base/Map pairs introduce Cloud Post,
Glass Lagoon, Moss Archive, Snow Spire and Great Beacon. Mika's optional story
now follows all 33 pages and ends at the beacon.

The wooden workshop revision in Docs/VISUAL_MOTION_REWORK.md and
Docs/WOODLAND_SURFACE_BRIEF.md is implemented in presentation: honey-wood
platforms with hemp suspension, held-command forecasts,
source-piece falling and settling, pictorial device demonstrations and stationary
before/after ghosts under Reduced Motion. Separate cloud, foliage and regional
weather layers use the presentation clock; maps have live cloud/weather motion,
and scenery now uses a continuous viewport sky composition. The travel bag
places Mika and a parcel on a bench above two object shelves. The earlier map
seal derived from an already proven saved stamp goal; the current device/map
revision omits that overlay. These visuals consume committed
state and do not change gameplay rules. The Craft revision in
Docs/CRAFT_REWORK.md adds a continuous hanging rope rig, bounded visual spring
weight, a two-stage landing bounce, and warm key/sky fill. Its two original
backgrounds, WorkshopRoom and WindGardenPanorama, retain generated pixels and
embedded prompt metadata; prompts and hashes are in Docs/AssetPrompts/Craft.
Prices, packages and existing campaign IDs remain stable; there is no gameplay
physics. Online remains design-only.

Fresh campaign deliveries after C40 now have a local director that observes completed attempts and deliberate retries. Two confident deliveries can lead to a tighter wood allowance or a wind passage; a peak is followed by recovery. Thinking time and purchases are never difficulty inputs. Replays and ordinary Retry retain their pinned conditions. After repeated failures, More wood starts a separate, clearly offered attempt with stronger planks.

Directed conditions are fixed before input and proved by the same exact solver. Unproven fresh variants fall back to the published source. Wind reuses the existing one-place entry shift on a lower board: its arrow and guide show what will happen on landing. A saved recipe reconstructs the exact variant and its commands; local records remain separated by content hash. Irregular breeze and bounded landing gusts only animate the connected toy world.

Ribbon Reach is implemented as six separate optional workshop trials R01–R06,
accessible through Power-ups in the bag. Their rules version is 2.1-longreach. One charge permits
one atomic endpoint exchange across one or two intervening pieces; the middle
pieces stay in place and the multiset is preserved. Every crossed edge must be
open, and a corridor containing a linked-pair edge rejects Reach. Cost is one
plus crossed fragile edges; a crossed one-use bridge is consumed. Contacts use
the final row and the bomb ticks once. Undo restores the charge; Retry resets
it. The same transition supplies the solver, command replay and state identity.
Bonus records/receipts stay separate from campaign progression. C01–C160 retain
their existing rules2.0 content and hashes. There is no charge purchase.

The device revision in Docs/DEVICES_REWORK.md adds six optional B01–B06
workshop lessons under rules2.2-pickups. Matching a final-row contact collects
its tool once on that floor; entry contacts also apply. Ribbon grants one Reach
charge, capped at one and carried to the next floor. Hourglass skips two bomb
tick opportunities on its floor, including the collection move's tick. Wood
still wears; the hourglass neither defuses the bomb nor freezes real time.
Defusing takes priority, and entering the next floor resets the pickup-used and
bomb-freeze state. Undo, Retry and replay restore the corresponding state. The
same transition supplies the solver. Existing C01–C160 and R01–R06 definitions
and hashes remain unchanged; no prices, packages or existing save schema change.

Six original transparent illustrations—Bomb, RepairKit, BrassKey, Stamp,
RibbonSpool and Hourglass—occupy attached wooden sockets with a separate
matching-piece pocket and explicit wood/bomb counters. Their exact prompts,
hashes and alpha bounds are recorded in Docs/AssetPrompts/Devices. Tapping a
tool opens its entry in the eleven-part animated device guide. The Power-ups
workshop menu presents six lessons with animated pickup demonstrations and a
link to the retained Ribbon Reach trials. Map islands are the destinations:
invisible hit targets replace numbered stop panels and path pebbles; available
scenery remains colored, locked scenery is desaturated, and the current island
has a localized You are here arrow with Mika.

P19 provides disabled mock monetization service boundaries; it has no live ads,
IAP provider, store UI or validated production receipts.

## Evidence on Hand

Docs/C01–C21 preserve the rule canon; Docs/JOURNEY_REDESIGN.md and
Docs/JOURNEY_SURFACE_BRIEF.md record the author-requested P14 revision.
Reports/Journey contains native Editor captures, metadata, test reports and
the independent Impeccable review. The final expanded PlayMode run passed
36/36 tests after the review corrections; 309/309 EditMode remains the prior
rule/domain checkpoint, with no Domain changes in the correction batch. The
independent visual follow-up was pending at this documentation pass. Earlier
P11 input coverage exercised all 40 primary levels; Docs/POC_REVIEW.md retains
known content-depth limits. These checks do not prove enjoyment, physical-device
touch/haptics/performance, build readiness or release readiness. Native-language
editorial review is not complete.

P15 evidence is in Docs/P15_LOCALIZATION.md and Reports/P15: 215 UI keys with
645 non-empty entries, 313/313 EditMode and 38/38 PlayMode results, and 39 native
Editor captures across EN/RU/UK. The independent P15 review records a scoped
ship verdict for those views. Addressables content built for StandaloneOSX;
this is not a complete player build. At the P15 checkpoint, preferences and
progress were session-only; P16 adds the local persistence described above.
Native-language editing, physical-device behavior and human enjoyment remain
unverified.

P16 evidence is in Docs/P16_SAVE.md and Reports/P16: 323/323 EditMode and 43/43
PlayMode results, plus 15 final native Editor recovery fixtures across EN/RU/UK
at 320×693 with no recorded UI-fit findings. The tests cover replay round-trips,
scene reload, storage recovery and retry; screenshots show recovery UI poses.
Physical-device power loss, filesystem-full behavior, mobile OS kill and a
complete player build remain unverified. No cross-device or cloud save is
implemented, and these checks do not establish native-language approval or fun.

P17 evidence is in Docs/P17_META.md and Reports/P17: 333/333 EditMode passed;
the full PlayMode run passed 46/47, followed by the corrected navigation test
passing 1/1. This gives passing evidence for all 47 distinct tests, without a
second full-suite run. The final report records 44 inspected native Editor
frames across EN/RU/UK, small/normal portrait and a wide workshop. The three
additional carousel asset frames were captured and inspected. Nine original
transparent illustrations and exact generation prompts are recorded in
Docs/META_ASSET_PROVENANCE.md. The independent verdict scores the two listed findings resolved, disposition ship at P17 scope;
these checks do not certify a player build, physical-device behavior, native
editorial approval, enjoyment or retention.

Current expansion evidence is in Reports/P20, Reports/Woodland and
Reports/WoodlandFocused. P20 records 466/466 EditMode and a focused 1/1 native
input PlayMode pass. The Woodland checkpoint records 5/5 EditMode and 16/16
focused PlayMode before the final device-guide/CanvasRenderer correction.
Native full-size PNG stills and sampled motion frames exist. The final renderer
correction passed four affected UI regressions (Reports/WoodlandFix/playmode.xml,
11:29:09–11:29:16 UTC); fresh guide/map/bag captures show native wood and weather.
Those Woodland review/build statuses were pending at their original checkpoint;
the newer Craft results below supersede them. Earlier phase results above
remain historical checkpoints, not a final expanded full-suite run.
Physical iOS/Android touch, haptics, performance, speaker/headphone listening,
human enjoyment/retention, non-reader comprehension, native-language editorial
approval and live Ads/IAP remain NOT RUN. No no-bugs or enjoyment claim follows
from these tests or captures.

The current Craft evidence is in Reports/Craft and Reports/CraftReach. The
initial EditMode run passed 475/476; after the Reach-test correction, all nine
Reach tests passed in the focused rerun. Seven focused PlayMode tests passed;
Reports/CraftFinal/playmode.xml then passed three affected regressions from that
seven-test set. These are distinct checkpoints, not a subsequent full 476/476
result or ten distinct PlayMode tests. The original review recorded fix;
Reports/Craft/impeccable-verdict.md now records ship for seven scored findings,
including map edges and piece contact with visible wood. That disposition covers
the scored fixes, not the complete product or all interaction states. The 844×390 landscape capture is a rejected
diagnostic, not a supported mobile view. Portrait safe areas are simulated;
no physical iOS/Android device is connected.

Reports/Craft/editor-performance.txt records 240 frames after 90 warm-up frames
in the Editor at 1793×1035: median 99.998 ms and p95 101.649 ms. Background
throttling is suspected, not established; this sample is neither a player nor a
mobile performance pass. Total allocated-memory snapshots are not a measured
per-frame allocation rate.

Reports/Craft/build.txt records a successful Mac standalone build with zero
errors, 346 warnings and 247,298,651 bytes. CascadeUnity/Builds/CASCADE-Craft.app
preserves the previous app. Warnings include package shaders and missing iOS
localization metadata; this Mac result is not an iOS build pass. The parent
checked Player.log without recorded exceptions/errors.
Reports/Craft/player-performance.txt records actual C160 standalone performance
on Apple M4 Pro at 1440×900: 240 frames after 90 warm-up frames, median 16.703 ms
and p95 17.520 ms. The Mac was locked during this benchmark; it is not verified
foreground performance. Total allocated-memory snapshots are 106,835,389 and
107,089,661 bytes. The GC counter is invalid, so its reported zeros mean
unavailable, not zero allocations. This short sample does not establish sustained
performance or mobile performance. Native app visual confirmation remains
blocked by the locked Mac. Device, human enjoyment, AAA quality and release
readiness remain unproven.

The device revision checkpoint is recorded in Reports/Devices and
Reports/DevicesRegression. 137/137 regression EditMode and21/21 preliminary
focused cases passed (146 distinct). Initial PlayMode3/5 had two animation
completion timeouts; unchanged focused reproduction2/2 passed under the same
deadline. A separate literal-letter regression1/1 passed in EN/RU/UK. Native
MCP screenshots cover the changed device, tutorial and map surfaces, including
320px UK shared contacts. The five scored Impeccable visual corrections have a
scoped ship confirmation. Prior Craft performance results remain historical;
physical-device behavior, non-reader comprehension, native-language editing,
performance and human enjoyment remain unverified. See the Devices report for
final preview checks (2/2 PASS) and actual new build outcome: CASCADE-Devices.app
built successfully with0 errors /346 package/config warnings. CUA could not
inspect its native window because the Mac is locked; standalone UI/performance
remain unverified.

## Product Principles

- Every visual consequence explains an already committed command.
- Keep the complete row and destination understandable.
- Recover from mistakes without coercion or payment.
- Prefer useful choices over added device count or padded move count.
- Inspect the running game, and state what remains unverified.
