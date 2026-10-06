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
EN is default, RU/UK additional, UI and puzzle locales independent. Two free
Undo uses per stage attempt apply only to moves on the current intact board;
free current-stage Retry and solver-verified one-step hints remain available.
Only a purchased Return hourglass can recover the latest broken board in the
current stage. Source Domain rules, IDs and package pins are stable.
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
pressing a piece acknowledges with its actual body contour and a slight lift,
with its glyph attached. Deliberate drag follows the finger freely in x and y;
stable hit regions and resolver legality do not move with the artwork. Selection
and destination feedback use the piece itself, with no square slot or target
overlay. Exchanges inherit the released source pose and return the partner on
an upward arc. All floors and the complete target stay visible. Long rows offer
a larger held preview, target inspection, tap-then-tap and optional precision
controls under Comfort. Precision controls are never forced by row length.
Reduced Motion removes decorative lift, zoom and tilt while preserving intentional
x/y tracking. Ambient movement must not obscure gameplay information.

Falls and device shakes borrow the existing isolated cosmetic PhysX scene. Eight
pooled Rigidbody box proxies make the original visible pieces spin, bounce and
collide briefly, then guide them into the exact committed seats. Normal wind and
exchanges stay scripted; hemp ropes remain constrained point chains, not full
realistic rope physics. High-pass filtered acceleration ignores the initial zero
sample, bounds its impulse and gives deliberate touch priority. Physics and
sensors never change Domain, Solver, gameplay state, contacts or move costs.
Actual phone sensor behavior remains unverified.

Development workflow: run phone builds or installations only after a separate
explicit user request. Routine UI work uses available Editor inspection and
scoped tests, with validation limits stated in its report.

The current Home offers Start game, Shop, Settings and Remove ads. Start game
opens a separate Story/Shapes/Words/Mixed choice. Story chooses existing primary
and localized campaign bindings automatically after the first three shape
lessons, keeping campaign order and an unfinished concrete attempt pinned.
Its first entry shows a three-scene comic: Gran tends the Great Beacon, the
storm leaves Mika with one light, and Mika sets out with Pip the frog and Rue
the older fox to repair the way home and relight the beacon. Completing or
skipping the opening records HeardStory entry 1000 in the existing local field;
the opening can be replayed from the play choice. Shapes, Words and Mixed each
open a numbered shelf filtered by that concrete mode and puzzle locale, with
up to nine levels per page and swipe navigation. Map and shelf Back return to
the play choice; Settings and help return to the screen that opened them.

At the P14 checkpoint, the journey gave the existing 40 lesson puzzles Mika's story,
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
opening a receipt is idempotent. These are the four legacy parcel trophies,
with three fragments each and a twelve-fragment total cap; the current shop and
collection catalogue contains eight trophies. Mika can wear an owned trophy on
the map. Collection now uses two stocked shelves and one attached description
and action area, with the selected shelf object as its only object illustration.
The workshop retains four miniature thumbnails, a large selected model and its
saved progress.
Four workshop miniatures each require their exact ten lesson-node receipts;
Words skipping C26 does not complete its ten-node workshop. Keeper rank reflects
restored lights and three requests based on repair, bridge and stamp outcomes
in successful deliveries. Seven distinct local visit dates accumulate without
missed-day loss. Personal route records distinguish puzzle, content, mode,
puzzle locale and objective; optional solver-proved stamp trails keep ordinary
travel open. These local milestones remain available across the expanded campaign; the four workshop miniatures still refer to their original ten-lesson ranges.

The earned-coin shop contains five travel tools: a Wayfinder scroll for 120
coins, an Undo spool for 80 and a Return hourglass for 200. The scroll points out
each solver-proved next move on the current board until it breaks; a spool gives
one extra current-board Undo after the two free uses; an hourglass restores the
exact checkpoint before the latest board collapse and removes subsequent moves.
A Signal lantern for 160 coins performs exactly one solver-proved non-drop
command through the existing resolver, with normal wood and contact costs.
A Landing lens for 100 coins shows the immutable prediction of descending now:
the exact next row and outcome, while play is paused, without moving the row.
Each tool kind has a stock cap of five and one use per stage attempt. Retry starts
a fresh current-stage attempt without refunding spent stock. Tools cannot rewind
an awarded delivery. Using any tool counts as assistance, caps that run at two
stars and excludes a clean record. This consequence is visible in the shop and
bag before Use. Free hints and Retry remain available. Buying alone and cosmetic
equipment do not affect puzzle performance or adaptive difficulty.

Coins come from existing new-delivery rewards, chapter bonuses and verified
rewarded grants; replaying a cleared node does not pay again. All eight trophies
cost 240 earned coins each and are bought once. Leaf Pin, Sun Button, Sky Ribbon
and Moon Patch remain earnable through the original parcels. Wind Chime, Rain
Badge, Copper Acorn and Beacon Brooch are earned-coin shop trophies; they add no
parcel fragments or gameplay statistics. The full catalogue preserves the old
four IDs and appends wind_chime, rain_badge, copper_acorn and beacon_brooch.
Player-facing collection labels are Trophies / Трофеи / Трофеї; legacy Gift
storage keys remain compatible. The shop shares the collection's two shelves
and attached copy/action area, without a duplicate large selected object.
Prices appear in the purchase action; test mode hides prices and resets preview
stock on restart. A bag control appears between Undo and Descend during play
when stock exists; Pause and the loss flow also retain access, with localized
eligibility and once-per-stage explanations. Purchases and tool use
save currency, inventory and the pinned attempt in the existing transaction;
failed writes roll them back. Schema 1 remains compatible through optional
fields. The real No ads adapter remains unconfigured while App Store/Google Play
catalog and ad identifiers are absent. The rule note and current validation
limits for the earlier shop checkpoint are in Docs/SHOP_AND_UNDO.md and
Reports/Shop/README.md. That checkpoint passed 13/13 focused checks, retained ten
native captures and received a scoped ship verdict for its readability,
unavailable-action, count/price and guide-ribbon fixes. It does not establish a
verdict for the five-tool/eight-trophy revision. The current design note is
Docs/SHOP_TROPHIES_AND_FIELD_TOOLS.md; Reports/ShopRefresh records 46/46 scoped
EditMode checks, passing Editor runtime checks and seven final native captures,
including small portrait, Large Text and simulated safe insets. Recaptured shop
and bag views show the two-star/no-clean-record warning; the bag also shows its
remaining tool count. Small-portrait action and preview-close targets measure
48 pixels high, and the runtime Console reports zero messages. The independent
finish review verdict remains pending at this documentation pass.
No player build or phone installation was run for this revision. Full-suite,
physical-device, live-commerce, performance and human-enjoyment validation remain
unverified.

First-clear and reward changes enter the same local profile transaction before
command presentation; parcel claims call the save path before their arrival
animation. Failed writes remain explicit and do not become a durable-save claim.
The original 40 lessons remain the tutorial route. P20 adds 120 primary puzzles
C41–C160 with solver minima of 5–12 actions (mean 7.8667), plus ten optional
stamp detours in the new content. The catalog contains 160 primary puzzles and
477 localized bindings, 637 concrete definitions; locale bindings are alternate
renderings of campaign nodes, not 637 distinct campaign stops. The eight-region
journey spans eight whole-world maps (15,15,10,24,24,24,24,24 full levels). Wind Gardens, Rain Mill and Copper Orchard retain
their original art; five new original Base/Map pairs introduce Cloud Post,
Glass Lagoon, Moss Archive, Snow Spire and Great Beacon. Mika's optional story
retains its 33 original level-bound story beats and ends at the beacon. Each illustrated island now groups two to five full levels; levels retain three stages. Cream heading/footer/current-marker cards are removed from the map. Dark ink reads directly on the sky, and teal/amber framed controls form the footer. The current-island label shows cleared/group total; the footer shows cleared/world total and remaining levels before the next world. Opened-level arrows support replay while locked levels remain inaccessible.

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

Fresh campaign deliveries after C40 now have a local director that observes completed attempts and deliberate retries. Two confident deliveries can lead to a tighter wood allowance or a wind passage; a peak is followed by recovery. Thinking time and purchases are never difficulty inputs. Ordinary Retry retains pinned conditions. After repeated failures, existing single attempts offer More wood as a separate attempt with stronger planks; a descent retains its checkpoints and its pinned stages, with free Retry/Hint and director recovery on the next fresh delivery.

Directed conditions are fixed before input and proved by the same exact solver. Unproven fresh variants fall back to the published source. Wind reuses the existing one-place entry shift on a lower board: its arrow and guide show what will happen on landing. A saved recipe reconstructs the exact variant and its commands; local records remain separated by content hash. Irregular breeze and bounded landing gusts only animate the connected toy world.

Fresh uncleared journey deliveries C41–C160 have three stages sharing one exact receiver target and symbol multiset: a two-command warm-up, an arrival wind passage, and a device finale. The solver proves the complete plan before input; starting minima satisfy stage 1 < stage 2 <= stage 3. Completing a receiver enables Go lower, carries the row to a new wooden rig, and preserves that checkpoint. Retry affects the current stage. Stage 1/2 grant no campaign clear or parcel progress; the final delivery grants one level clear. Lessons, standalone contracts and legacy single saves keep their original flow.

New version-2 descents put a repair tool in the two-command warm-up and a bomb plus lower-board stamp in the wind stage. The same exact solver proves all three stages before play. Earlier version-1 saved recipes retain their exact original definitions and route proofs; completed-stage explosion totals participate in full-run clean performance.

Optional versioned descent recipes preserve all three stage definitions and completed command replays. Interruption during camera travel resumes the already committed next stage; motion cannot advance state. Full-descent records aggregate moves, hints and Undo over all stages, identify their original level and show three stages. Play this level starts a fresh descent; it does not claim an exact replay of an older adaptive recipe. No price, package or Domain rule version changed.

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
has a localized You are here arrow with a small Mika standing on its plaza.
A separate invisible portrait target opens the current conversation. Whole-world maps
use swipes; islands, Mika and their targets share bounded view-only parallax
from device gravity where available or desktop mouse input, with quiet ambient
drift. Reduced Motion and suspension return this map offset to zero. The
original MapSky raster covers the viewport; the island shader substitutes its
painted blue sky with the same plate at screen coordinates. Map art retains its
existing normalized placement rather than claiming pixel-perfect registration.
The new MenuIcons, OpeningComic and MapSky rasters extend the painted Workshop
world; existing WorkshopPortrait and gameplay control art remain in use.

P19 established the disabled mock monetization ledger and C16 policy boundary.
The current native-store preparation extends it with official Unity IAP 5.4.4
and LevelPlay 9.5.1 SDKs installed through UPM. Resource configuration remains
disabled, keys are empty and there is no Codeless IAP automatic initializer.
The user has no registered Apple/Google products or LevelPlay app/ad units. The
one prepared draft is `cascade.no_automatic_ads`, a NonConsumable for removing
automatic ads. Its live price must come from localized store metadata; no price
is invented. The original four trophies remain earnable through free parcels;
the current earned-coin shop offers all eight for 240 coins each. Real-money cosmetic packs remain
a future option. The unavailable No ads purchase view's RU 320px readability and
52px-minimum continued-play footer were verified at the historical TouchRevision
checkpoint; those checks do not provide a verdict for the new earned-coin shop.

The publisher's HTTPS verifier must validate Apple JWS / Google purchase proof
for the expected app, product and transaction before the ledger saves entitlement
and before pending-order confirmation. That server is not implemented or hosted.
Duplicate restore receipts remain idempotent. A real privacy/age/ATT bridge,
native dependency resolution, platform build and store sandbox validation remain
external activation work. The existing C16 first-session, first-12-deliveries,
chapter-final and frequency protections remain; automatic ads can follow only a
saved win and base reward. No charge, ad or provider activation was performed,
and this preparation is not live IAP/ads completion. See Docs/TOUCH_AND_STORES.md.

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

Navigation evidence is in Reports/Navigation, Reports/NavigationSwipeFix and
Reports/NavigationMapFix. The initial scoped PlayMode run passed 5/6 and the
corrected swipe case passed 1/1, supplying passing evidence for six distinct
cases without a later complete six-case or full-suite rerun. A further 1/1 pass
expands the existing map case to click Mika's invisible margin, check quiet
motion and preserve the puzzle hash; it is not a seventh distinct case.
Localization checks passed 2/2. Sixteen native Editor PNGs cover the revised
Home, mode choice, shelf, comic, shop, settings, ads-off view, dialogue and map.
Four corrected map poses include compact/normal portrait, wide and an explicit
+6/+4 reference-unit shift. The correction verdict records SHIP for its two
scored sky/target findings only. Simulated safe areas and Editor pixel bounds
do not verify physical touch, gyro, OS behavior, accessibility, mobile FPS,
native-language approval or human comprehension/enjoyment. Phone-build status
belongs to the current report; older phase builds are not navigation validation.

Current touch/store evidence is in Reports/TouchRevision, TouchRevisionSensor
and TouchRevisionFinal. Ledger/policy EditMode passed 13/13. The initial PlayMode
run passed 9/10 and exposed a zero-sample sensor fault; a second failed run
exposed premature ground settling. After fixes, the four affected cases passed
4/4. Together these runs provide passing evidence for ten distinct cases,
not a fresh full-project suite. Ten final actual Unity Editor captures cover
gestures, physical landings, shake, letters and unavailable RU store views at
390×844 and 320×693. Static captures do not prove motion timing.

Fresh finish review found a 45.27px return-to-play target and lost intentional
vertical drag under Reduced Motion. Both implementations are corrected. The
final footer bounds are 64.14px at 390 and 52.66px at 320, inside simulated safe
areas. The expanded normal/Reduced Motion vertical and diagonal pointer check
in Reports/TouchRevisionReview failed in Editor Device Simulator setup/teardown;
its Game View repeat passed 1/1 in Reports/TouchRevisionReviewFinal. This expands
an existing case, not the distinct-case count. The fresh review records SHIP for
resolved F01/F02 only, with no whole-product certification. No hardware sensor,
haptic, OS accessibility, device FPS/thermal, native build/sandbox, production receipt,
live ad/consent or human-enjoyment proof follows from this evidence. Existing
MenuBackground assets and original raster pixels remain; no new raster generation
was needed for this scope.

## First encounters — 6 October 2026

Each new effect pauses for a short illustrated EN/RU/UK explanation. This is
required once even with optional hints disabled. Combined boards teach effects
one at a time. The first gust waits before rocking the rig; row-changing wind
waits before descent. Bag tools explain themselves before consumption. Only
acknowledgement saves learning; an interrupted explanation remains unseen.
Docs/FIRST_MECHANIC_POPUPS.md; Reports/Introductions.

## Product Principles

- Every visual consequence explains an already committed command.
- Keep the complete row and destination understandable.
- Recover from mistakes without coercion or payment.
- Prefer useful choices over added device count or padded move count.
- Inspect the running game, and state what remains unverified.

The outcome pass removes the full yellow menu sheet in favor of a complete painted workshop window, animated clouds, gentle lantern daylight and quiet dust behind controls. Victory and failure have distinct finite lantern/Mika ceremonies; result buttons follow the ceremony, and receiver layout moves continuously into place. Hanging tool tags physically attach to their wooden carrier and an accessible How to play page explains the tag, direct dragging and receiver goal. Wood carries a visible static load, bows more with wear and splits into textured splinters when spent. These are cosmetic springs and event-driven poses, not gameplay physics. Two original raster assets were generated: WorkshopPortrait and MikaThinking; existing assets were preserved.

Map/celebration refinement, 7 October 2026: RewardCelebration unfurls the existing illustrated ribbon, lights earned stars sequentially, counts score after their arrival and uses a finite warm glow/fabric burst. Its locally advanced DOTween timeline pauses before advancing when focus is lost and clamps long frame gaps. Stage outcome ribbons also unfurl. A parcel has anticipation, opening at0.55s and a gift emerging over1.35s, then a settled pose; normal navigation can interrupt it, Reduced Motion settles without the spatial sequence. Receipts are committed before motion. Native Editor samples confirm statistics → parcel → map, final poses, focus pause, immutable row/receipts and small48px map targets.16 focused EditMode tests passed. No new raster was needed: CompletionAtlas/Parcel/Mika/map art retained. Docs/MAP_PACING_AND_CELEBRATION.md and Reports/MapCelebration/README.md contain scope and limitations; physical devices, audio listening, performance and human enjoyment are not certified.


Five-star results, 7 October 2026: new completions use solver-proved total minimum, accepted moves including undone moves, Undo count and active playable time. Five stars require exact minimum, no Undo/help/explosion and time ≤ max(45,8×minimum+10×stages) seconds. Four stars allow minimum+2 and one Undo; slower perfect play retains four. Assistance stays capped at two. No timed failure, no reward/wood-price changes. Legacy receipts are retained until replay, unknown legacy time is shown as an em dash. The result screen uses the open sky, five illustrated stars, score/Mika, an aligned moves/Undo/time row, gold and firefly-lantern resource art, then one continuation. Generic cream menu reading plates are removed; native protected explanations use teal/brass. Quiet portrait/wide workshop backgrounds preserve user originals. See Docs/FIVE_STAR_RESULTS_AND_ACTIVE_TIME.md and Reports/FiveStarResults/README.md.
