# Receiver alignment and exact route preparation

5 October 2026. Design before revision: `Docs/RECEIVER_AND_ROUTE_BALANCE.md`.

- Target row centre matches the ivory inset at (.5, .53), including delivery height.
  Individual glyph geometry is centred within each face. RefreshLayout now updates
  live text poses immediately; previously a late-frame render could show raised letters.
- Runtime BFS uses the final immutable level and actual stamp goal, with the same
  resolver as play. It includes linked swaps, fragile wear, bridges, keys, repairs,
  bombs, landings, reach charges, ribbon and hourglass pickup. Commands count manual
  descent; durability is a different resource. Async work, cancellation, a separate
  input gate and a bounded solved-only cache avoid stalls and false minimum claims.
- HP-only revision of103/120 extension tasks. Exact minima5–12, mean7.5417.
  All120 completed graphs,360 word bindings and10 optional objectives verified.
  Mean HP discarded by the selected shortest route5.1333→2.1667. Two or more
  near-optimal first choices and forced ratio<.9 retained. Some relief tasks still
  permit early descent; this is not a claim that every floor requires swaps.
- Previous HP and exact pins are retained in campaign_durability_v1.json. Restore
  reconstructs all412 previous primary/locale definitions, verifies their hashes,
  replays committed prefixes, and preserves undo. Existing clear receipts remain.

Validation:

- editmode.xml:160/161PASS; the compatibility test timed out at180s because it
  redundantly searched412 full routes. The MCP call also timed out; neither was
  counted as success. Replaced repeated solving with exact pin/prefix/undo checks.
- ../RouteBalanceRecovery/editmode.xml: affected compatibility test1/1PASS,
  0.77s. Total161distinct affected EditMode cases have passing evidence.
- ../RouteBalancePlay/playmode.xml:1/1PASS,7.65s. EN/RU/UK glyph bounds;
  C81, C88, B03, B04 delivered through actual mouse/drags/buttons with predicted
  states, exact minima, far swap and hourglass. No domain mutation from presentation.
- ../RouteBalancePosePlay/playmode.xml:1/1PASS,7.87s after immediate text-pose fix,
  additionally verifies world XY alignment directly after RefreshLayout.
- Final MCP native renders inspected at390×844:
  ../Polish/receiver-shapes.png and ../Polish/receiver-letters.png.
  Compilation successful; final gameplay Console0errors/warnings.
- Input test fixtures now wait for preparation. Full legacy PlayMode suite was not
  rerun; no new player build, phone test or human enjoyment test performed.

budget_revision.json contains HP and route-use evidence for all120 tasks.
P20 graph evidence is updated; generation-summary retains original authoring metrics
and separately records this durability revision. No additional art assets needed.
