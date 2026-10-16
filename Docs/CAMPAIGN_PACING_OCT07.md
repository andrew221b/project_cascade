# Longer island progression — 7 October 2026

Expand the nine worlds to 60 levels each: five islands of 12 levels, 540 levels in total. Retain every incumbent C01–C184 definition, ID, content hash, localized binding and story anchor. Add C185–C540 as distinct solver-proved consolidation puzzles, rather than repeat the same puzzle with another label. C26 remains shapes-only: its three-identical-symbol pattern has no natural matching four-letter word in the existing EN/RU/UK dictionaries, so the Words route has eleven levels on that island.

The extension gains an optional `journey_order` array (a bijection over primary node IDs) and optional integer `teaching_node` on new definitions/bindings. The importer orders both primary and localized routes identically. Teaching metadata chooses the already introduced three-stage lesson recipe; it never changes the resolver's transition rules. Existing definitions and saved recipes default to their original node number. No rules version, product IDs, prices or save schema change.

Each island retains its original lessons first, followed by practice and combinations using mechanics already encountered there. Early consolidation stays short; minimum route bands rise gradually and settle at 8–12 actions, with occasional lighter tasks. Source definitions still receive the existing three-stage solver preparation, wind budgeting and first-use animation/hand guidance.

For existing saves, the route prefix through the furthest completed node stays open, including newly inserted optional levels. Continue goes to the next node after that frontier. Insertions do not award clears, stars, currency or resources. Each mode keeps its own frontier. Active attempts remain bound to their original node/hash.

The island level list becomes a vertically scrolling list with a fixed readable row height; adding levels must not shrink twelve rows into the previous five-row area. Keep the existing workshop frame, themed controls and per-level record display.

Validation is limited to deterministic generation, incumbent hash preservation, exact C# resolver/solver witnesses, three-stage preparation, localized equivalence, progression regressions and a focused UI inspection. No phone build or installation is required.
