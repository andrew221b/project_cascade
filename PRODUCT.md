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

## Brand Commitments

The author requires unique CASCADE artwork and a bright cartoon theme, with
ShapeMania as a reference for drawing character and broadly related colors.
Do not transfer ShapeMania screens, characters, sprites, props or materials.
Compact platforms, an unmistakable receiver, authored background, ambient
motion and coherent effects are required. The author delegated theme selection
and implementation and requested following P01–P20 without approval pauses.

## Operating Context

Short portrait puzzle sessions; touch or mouse. All floors and the complete
target stay visible. Narrow seven/eight-symbol rows offer large precision
controls. Typical rows have four to six symbols. Ambient movement must not
obscure causal gameplay information.

## Evidence on Hand

Docs/C01–C21 are the canon. P11 has 305 passing EditMode and 12 PlayMode tests,
including all 40 primary levels through actual Unity input. Reports/P11 and
Docs/POC_REVIEW.md contain evidence and known content-depth limitations.
These results do not prove enjoyment, physical-device performance, or release
readiness. Native-language editorial review is not complete.

## Product Principles

- Every visual consequence explains an already committed command.
- Keep the complete row and destination understandable.
- Recover from mistakes without coercion or payment.
- Prefer useful choices over added device count or padded move count.
- Inspect the running game, and state what remains unverified.
