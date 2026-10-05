# CASCADE

<!-- impeccable:product-schema 1 -->

## Platform

adaptive

Unity mobile game for iOS/Android with desktop Editor/mouse support. This is a
custom game surface, not a native productivity app. Platform safe areas, touch
targets and reduced motion apply; gameplay follows the user's game brief.

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
The existing 40 lessons remain short. The eight-region/160-stop campaign, further
meta progression, ranks, collectibles and Long Reach remain staged design work.
Online remains design-only.

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

## Product Principles

- Every visual consequence explains an already committed command.
- Keep the complete row and destination understandable.
- Recover from mistakes without coercion or payment.
- Prefer useful choices over added device count or padded move count.
- Inspect the running game, and state what remains unverified.
