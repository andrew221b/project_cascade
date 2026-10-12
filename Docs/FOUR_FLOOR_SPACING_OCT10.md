# Four-deck presentation spacing

Presentation-only adjustment. Keep authored deck order, durability, contacts, witnesses and IDs unchanged. Distribute the existing vertical span by contact weight, with a minimum row-height-plus-air gap before allocating remaining space. The floor gap buffer is reused. Recompute anchors together with receiver clearance during bag reveal/close and safe-area refresh.

Hanging bodies now protect the highest of the next deck's left, center and right surface points, rather than one point directly below the hook. When the live row is on that next deck, include its actual rendered bounds (selection and arrival pose included). Existing body fitting, rope constraints and upright timer reserve continue to bound the presentation. Retired decks' props remain hidden.

PASS: isolated native Unity review exited 0 with no recorded game exception or C# compiler error. All 97 packaged four-deck stages were checked at 390×844 and 320×693 (194 initial layouts). Dense arrangements exercised alternating wind, lateral bag opening/closing, actual witness descent and animated Undo. 2,923 geometry samples / 5,616 contact samples; minimum art/timer clearance 13.5 logical points even during the compressed bag layout. Wind left the puzzle hash unchanged. Native normal and narrow bag renders visually inspected. Evidence: Reports/FourFloorSpacing-Oct10/native.log, native-review.txt and native camera captures.

The ordinary screen keeps generous weighted spacing; the expanded bag temporarily compresses the field and fits cosmetic bodies/cords to the available gap. Closing restores the ordinary anchors. Only one persistent live row exists: prior deck contacts are hidden as the row arrives below. This review covers geometry and sampled actual descent/Undo, not exhaustive playback of all witnesses.

Earlier missing namespace compile diagnostic and empty-bag review-fixture failure are preserved and corrected (the bag correctly refuses to stay open without owned tools). The final review uses an isolated stocked profile. No gameplay rules, new packages or player build.

Not assessed: physical phone touch/performance, whole EditMode suite, human enjoyment. No iPhone build/install.
