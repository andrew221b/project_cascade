# Hanging devices and shared wind — 2026-10-05

Devices hang about 19 reference units lower on a 132-unit floor card (about 16 on the compact four-floor layout). Their assembly pivots about the deck socket; straps update from that socket to the illustrated grip points. Bomb straps attach to its metal collar. Heavy tools swing less than light ones, with bounded inertia from socket movement.

The same slow wind field drives platform sway/roll, rope bow and device swing. Deck deflection still reflects apparent piece weight. Shapes, letter tiles, their glyphs, contact shadows and rope knots follow the moving deck. This is a cosmetic spring simulation, not gameplay physics; rules, budgets, IDs, content, saves and prices are unchanged. No packages or raster assets were added.

## Validation performed

- `../WindPlay/playmode.xml`: 3 passed, 1 failed, 13.81 s. The rope test still queried the obsolete object name `continuous hemp`; the current rope renderers are named `braided rope`. Corrected that test and made its preparation wait explicit.
- `../WindRopeConfirm/playmode.xml`: the corrected rope/reduced-motion/resize case passed, 4.21 s.
- `../WindDeviceConfirm/playmode.xml`: the final affected hanging-device case passed after moving the bomb straps onto its collar, 6.30 s.
- Four distinct PlayMode cases have passing evidence: hanging motion and pause/reduced-motion freeze; continuous ropes and resize; wood weight and glyph depth; receiver alignment and prepared playable routes.
- Hanging-motion checks include device travel, deck tilt, connected rope knots, longer straps, unchanged state hash and zero spent moves.
- Native Unity MCP captures: three successive C13 bomb/deck poses at 390×844 and C81 repair kit at 320×693. Initial visual review found one bomb strap ending above its body; the final captures replace those frames after correction. Final bomb poses and compact repair-kit view were visually inspected. Attachments remain connected and clear of the wood counter and receiver.
- C# compilation passed. Final Console: 0 errors, 0 warnings.

No player build, mobile profiling, physical phones, haptics or human enjoyment assessment was run. Successive stills and automated assertions verify bounded motion and attachment continuity; they do not establish real-device smoothness.
