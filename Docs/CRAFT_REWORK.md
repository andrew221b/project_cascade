# Craft revision — 2026-10-05

User explicitly rejected the previous visual quality and delegated this revision. Established CASCADE illustration, Mika, palette and fonts remain the identity; code-led implementation of the specified existing surfaces, no new identity selection. Scope: game machine, continuous rope rig, wood wear, menus/keepsakes/workshop, map, coherent wide/portrait scenery, safe areas, bounded atmosphere, optional Long Reach puzzles, measured performance. Current build 1bfb9aa is the rollback baseline. Existing Unity Cloud setting is preserved separately.

## Direction contract
THESIS: a working little sky workshop whose objects carry weight and whose shelves explain what is owned, being made, or available.
OWN-WORLD: honey wood, braided hemp, carved sockets, warm cream labels and mint painted objects; existing original illustrations and typography.
STORY: choose a destination, arrange real-looking pieces, see the board wear, land the row, bring a useful keepsake home.
FIRST VIEWPORT: map owns the center, current stop has a lantern/pulse and one travel action. Bag has Mika beside a bench, two object shelves, one return action. Keepsakes show four selectable objects together and a large selected item above a short progress/action region. Gameplay keeps a calm center, hanging continuous ropes, carved round strength marks on the board and readable target sockets.
FORM: precise extension/recomposition of the user-pinned toy workshop (no concept seed or unapproved comp). Signature interaction: a picked-up block casts a lifted shadow; release follows an arc, lands with a two-stage bounce, rocks the shared ropes and emits a small dust ring.
FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance.

## Rule note before Long Reach implementation
Add optional separate R01–R06 workshop puzzles, leaving C01–C160 and their content hashes/rules2.0 unchanged. New rules2.1-longreach permit one atomic endpoint swap across one or two intervening pieces per attempt. All crossed edges must be open; linked-pair corridors reject long reach. Durability cost is one plus crossed fragile edges; crossed one-use bridge is consumed. Final-row contacts/bomb tick occur once. Charge is part of immutable state, equality, hash, solver and replay; Undo restores it, Retry resets it. No charge purchase or economy change. New explicit command encoding Rstart:end; old save encodings/hashes remain unchanged for old levels. Bonus receipts and records are separate from campaign progression. Access from bag at any time with pictorial instruction. Online remains design-only per original instruction.

## Acceptance
Compile, full affected pure rules/save/solver tests and targeted lifecycle/input tests. One native baseline, then one batched visual review and one confirmation; independent fresh finish review uses the user's rejection as evidence. Test representative 320/390 phones, notched portrait and landscape safe-area simulations, tablet/wide. Benchmark actual frame/allocations with warm-up and quality-tier counts; distinguish Editor from player/device. No all-device or proven-fun claim. One updated local build only if needed to measure standalone performance; preserve previous app.

## Device composition decision
The portrait falling-floor puzzle is portrait-only on iPhone/Android; the previous template auto-rotation setting exposed unreadable short landscape layouts. Enforce portrait interface orientation rather than pretend the 844×390 diagnostic is supported. Desktop retains a resizable wide continuous world; 1440×900 is the captured wide baseline. Android/iOS safe areas are respected, with simulated 390×844 notched evidence; no physical mobile device is connected. Native phone performance remains unmeasured.
