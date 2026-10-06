# Gameplay HUD — 6 October 2026

Scope: the requested HUD correction. No rules, solver transitions, packages, economy, IDs or save schema change. No player build or phone installation.

- The 48-pixel minimum Hint and Pause targets flank a centred 40-unit chapter plaque. Hint is a drawn lamp; its warm tint marks a ready hint. Its existing request/apply behaviour and name remain intact.
- Header width follows the actual rail: `railWidth × .95 − 24 compensated units`. The fixed rope anchors therefore leave 12 compensated units on each inner side. Six units separate each control from the plaque. The title uses centred 12–17-unit autosizing, four-unit horizontal and five-unit vertical inner padding. Its existing sliced frame has a 3× pixels-per-unit multiplier to leave real room for lettering. A short Level caption stays on one line; a story task can use two.
- Undo and Descend/Deliver are the only two normal footer actions, centred as a pair. Precision and result controls retain their own flows.
- Contextual cream instructions start invisible and fade in after 3.2 seconds of foreground play. Every new message waits. A 44-pixel close target dismisses that message for the current level; a new level resets dismissal. Hidden instructions cannot intercept input. Pause, zoom, held pieces and reaction beats suppress the instruction. Preparation failures remain actionable without waiting.
- The decorative yellow coaching ring is removed. The hand uses sorting order 30 and a UI shader with `ZTest Always`; it renders in front of opaque pieces. It stays non-interactive and disappears during reaction beats or Reduced Motion.
- There is no Shortest route label. `MinimumMoves`, preflight solvability and the solver still compute exactly as before.

Validation: targeted Play Mode checks, plus native Unity MCP phone/narrow/wide captures. See `Reports/CompactHud/README.md`. Editor captures with simulated safe insets do not validate physical notches or touch feel on a phone.
