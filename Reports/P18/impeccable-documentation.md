# P18 design documentation

Ordinary extension of the incumbent Sky Wind Workshop. Updated only DESIGN.md
narrative: the P18 validation boundary and the map's reuse of native authored
chevrons. Its frontmatter and `.impeccable/design.json` remain byte-for-byte
unchanged; no code, art, Unity assets, tokens, schemas or IDs were edited.

Source checks: `CascadeGame.cs` (`DirectionButton`), `CascadeGame.Journey.cs`
(`MapTurn`) and `Data/ui_localization.tsv`. The stable Hint key now displays
RU `Помощь` and UK `Поміч`; map paging retains Earlier/Later names, handlers and
76 × 60 reference geometry. The icon clears the caption, uses existing Ink,
and disables its own raycast target. This preserves the paper target and
direction affordance while addressing the recorded narrow-screen wrapping.

Inspected all three final native 320 × 693 frames in `.impeccable/review/P18`:
RU Mixed8, RU bomb guide and UK map. Paper, dark ink, Balsamiq lettering,
illustrated sky and toy forms match the current tokens/material guidance.
RU help fits on one line and the UK map shows a clear right chevron. The UK
help caption is source-confirmed, not shown in these final gameplay frames.
No HTML/CSS surrogate component or substitute raster was generated.

Evidence boundary: independent review records ship for these two corrections.
The existing report records 157 matched puzzles, 16,900 states and 139,376
command attempts, plus 3/3 focused tests. Map/preferences checks used pointers;
language preservation called SetUiLocale directly. No tests were rerun by this
documentation pass. The approximately 100 ms frame interval describes an
uncontrolled long-running Editor observation, not a device target pass.
Physical devices, final player build, native-language approval, child
comprehension and human enjoyment remain unverified. Wooden/rope presentation,
weight/fall motion, pictorial bomb teaching, wide views and menu work stay
pending after P20.

Pre-existing sidecar drift is reported only: its route description and Do list
still refer to session progress, its saving Don't assumes session-only memory,
and its implementation Don't still excludes P17 ranks/parcels/keepsakes. Its
overview also predates the P16/P17 narrative extensions in DESIGN.md. The
native precision-control description remains accurate. No sidecar repair or
preview translation was performed.
