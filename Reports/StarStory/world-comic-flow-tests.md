# World comic and conversation flow fixtures

Authored 7 October 2026. These four PlayMode coroutine tests use an isolated
temporary profile and the actual playable scene. They do not claim a test run;
the integrating agent must run them in Unity after art and localization import.

- All nine worlds display three different assigned illustration panels. Opening,
  advancing and finishing comics preserve an already played middle descent
  stage, the exact attempt, row, route length and saved state hash.
- Comic receipts stay with Shapes, Words or Mixed. Reloading a profile with a
  read comic returns to its map without showing that comic again.
- Actual island buttons and world browsing show every unseen island opening
  once, preserve the selected map destination and leave the pinned row intact.
- Actual opened-level arrows show the occasional exchange only at the authored
  fourth-island offset, once per world, without resetting the browsed stop.

The browsing fixtures grant route availability only in the in-memory session
set. They neither execute those puzzles nor invent persisted clear receipts.
The all-island fixture selects the Mixed journey before taking its row snapshot,
because Shapes correctly excludes original primary lessons containing glyphs.
The comic preservation fixture earns C01/C02 clears and a first-stage delivery
with the shared solver and transition function before making an actual move
in the middle stage.

Source review found that island buttons and map/level browsing initially only
redrew the map, bypassing the comic/opening gates. Returning from an opening
also recalculated selection. The integrating agent added ShowSelectedJourney
to retain the chosen destination and run those gates on each browsing path.

These fixtures verify presentation state and navigation contracts. Comic
legibility, art composition, pacing and human enjoyment still require visual
inspection and human play evidence.
