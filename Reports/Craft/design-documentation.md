# Craft design documentation checkpoint — 2026-10-05

PRODUCT.md, DESIGN.md and the factual extensions/narrative in
.impeccable/design.json now describe the implemented Craft revision. This is a
refresh of the pinned Sky Wind Workshop identity, not a replacement design.
DESIGN.md frontmatter, all color/type/shape/spacing/component primitives,
existing assets, metadata identities, campaign IDs, prices and package pins
were left unchanged. No game code or TASKS.md was changed by this documentation
pass. The parent task owns the runtime checks; final scoped visual review and Mac
build evidence are now recorded below. Native player visual confirmation is
still blocked by the locked Mac.

## Implementation recorded

- R01–R06 are six optional local Ribbon Reach workshop trials under
  rules2.1-longreach, separate from unchanged C01–C160 rules2.0 progression.
  One charge exchanges endpoints across one or two intervening pieces. Open
  crossed edges, linked-pair exclusion, fragile cost, bridge consumption,
  final-row contacts, one bomb tick, Undo and Retry follow the shared resolver.
- Collection/workshop display four selectable object thumbnails simultaneously,
  a large selected object and saved progress/action copy. Bag displays Mika and
  a parcel on the bench, two object shelves, Settings and map return.
- Continuous rope suspension connects upper viewport, platforms and receiver;
  bounded visual spring weight and a damped two-stage bounce consume committed
  events. Piece mesh bottoms seat at 0.365 of the padded floor rectangle, with
  shadows aligned to visible wood. The light setup is a warm 1.05 key, 0.35 sky
  fill and flat ambient.
  No gameplay physics is introduced.
- Portrait is enforced for mobile. Desktop 1440×900 remains the wide reference;
  short desktop windows are not a supported baseline. The rejected 844×390
  landscape image is retained as diagnostic evidence, not acceptance evidence.
- WorkshopRoom and WindGardenPanorama are original generated backgrounds.
  Docs/AssetPrompts/Craft/provenance.json records source/saved PNG hashes,
  decoded scanline hashes, unchanged generated pixels and verified embedded
  prompts. Exact prompt text is stored beside the provenance record.

## Source and evidence inspected

Sources: Docs/CRAFT_REWORK.md; PRODUCT.md; DESIGN.md; .impeccable/design.json;
CascadeUnity/Assets/_Cascade/Content/ReachChallenges.cs;
Domain/PuzzleResolver.cs and PuzzleState.cs;
Presentation/CascadeGame.Craft.cs, CascadeGame.Rewards.cs, CascadeGame.Touch.cs,
CascadeGame.cs, CascadeGame.Journey.cs, LayeredWorld.cs,
ToyMachineView.cs, ToyMachineView.Ropes.cs, ToyMachineView.Woodland.cs and
PuzzleEventPlayer.cs (all source paths relative to Assets/_Cascade after the
first full source path). The native Unity implementation is authoritative;
there are no invented HTML/CSS component previews.

The parent-provided screenshots and Reports/Craft/impeccable-review.md record
18 supplied native Editor views, including phone, small portrait, tablet,
notched portrait simulation and desktop-wide. The original independent review's
recorded disposition was **fix**. Reports/Craft/impeccable-verdict.md now records
**ship for seven scored findings**, including tablet map edges, portrait policy,
RU Settings, bomb captions, available-charge Reach guidance, documentation and
physical grounding. The final bounded motion confirmation inspected corrected
Motion/09.png and Motion/19.png contact against in-flight Motion/06.png. It does
not certify the complete surface, all Reach states, real-time smoothness,
physical-device behavior or enjoyment. The available-charge correction was
visually confirmed; no spent-charge screenshot was supplied to that reviewer.
This documentation agent read the verdict and source, not the motion pixels.

Existing test XML was read, not rerun:

| Evidence | Result | Scope |
| --- | --- | --- |
| Reports/Craft/editmode.xml | 475/476 | Initial full EditMode run; one failure |
| Reports/CraftReach/editmode.xml | 9/9 | Focused Reach rerun after correction |
| Reports/Craft/playmode.xml | 7/7 | Focused PlayMode run |
| Reports/CraftFinal/playmode.xml | 3/3 | Final affected regressions, subset of the seven |

These results must not be described as a subsequent 476/476 full-suite pass.
The three final regressions are not additional distinct tests.
The documentation check parsed JSON and test XML, searched stale current-state
claims, and confirmed the DESIGN.md frontmatter was unchanged. No Unity
commands, runtime mutation or new gameplay tests were performed by this agent.

Reports/Craft/editor-performance.txt reports Mac16,8 / Apple M4 Pro Editor,
1793×1035, 90 warm-up frames and 240 measured frames: median 99.998 ms,
p95 101.649 ms, 88 renderers. Background throttling is suspected; this sample
cannot establish mobile or standalone performance. Allocated-memory totals
are snapshots, not a per-frame allocation measurement.

Reports/Craft/build.txt records the new Mac CASCADE-Craft.app build as Succeeded:
zero errors, 346 warnings, 247,298,651 bytes. The previous app was preserved.
Warnings include package shaders and iOS localization metadata; this is not an
iOS build result. The parent checked Player.log and reported no exception/error.
Reports/Craft/player-performance.txt records actual C160 standalone player data
on Apple M4 Pro at 1440×900: 240 measured frames after 90 warm-up frames,
median 16.703 ms, p95 17.520 ms and 88 renderers. The Mac was locked during this
sample; foreground performance and native app visual confirmation are not
verified. Allocated-memory snapshots were 106,835,389 → 107,089,661 bytes.
gc_counter_valid=False means the zero GC values are unavailable readings, not
zero allocations. This brief sample cannot establish sustained frame cost,
per-frame allocation rate or mobile performance. Physical mobile devices are not
connected (parent reported no xctrace device). Physical touch, haptics, native
editorial review, non-reader comprehension, enjoyment, retention, AAA quality
and release readiness remain unproven.

## Historical documents and remaining limits

- Docs/P17_META.md and Docs/WOODLAND_SURFACE_BRIEF.md retain their historical
  descriptions, now with current-implementation pointers to the Craft note.
  These historical checkpoints no longer stand without a correction pointer.
- P17 references to carousel captures remain explicitly historical in the root
  documents; current component descriptions now document four-object selection.
- Previous phase validation paragraphs remain historical checkpoints. Root
  documents and sidecar now include the new Craft checkpoint and its limits.
- Final scored reviewer disposition, map evidence, Mac build and the bounded
  locked-Mac performance sample are now recorded. Native app visual confirmation
  remains blocked by the locked Mac. Device, sustained/foreground performance,
  valid GC allocation measurement and human evaluation remain outstanding.
