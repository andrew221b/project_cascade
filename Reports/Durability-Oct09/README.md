# Platform durability validation — 9 October 2026

Production Domain/Solver/Application/Content/Persistence C# executed with the bundled Unity Mono runtime outside the Editor. This is actual transition/search/replay testing, not a duplicate Python gameplay model or a player build.

- `result.txt`, `campaign-proofs.csv`: all 540 primary recipes, 1,620 campaign stages; exact saved certificates replayed for 6,471 primary/localized stages. Every starting platform has no entry transformation or timed gust; every winning certificate uses its lesson and every remaining platform.
- `lower-hp-probes.csv`: 2,948 probes of every lower positive platform durability. None preserves a shortest delivery using the lesson/all platforms; no Unknown. Some smaller budgets deliver by skipping the intended lesson, which is explicitly recorded and excluded by the lesson policy. No claim of global optimality over simultaneous durability assignments.
- 641 individual repair/key/hourglass/ribbon ablations returned exhaustive Unsolvable. No benefit bypass accepted.
- `focused-result.txt`: 43 actual NUnit methods, including 15 campaign/mission durability/layout cases, four deterministic combinations of left/right gust with linked/fragile/one-use bridge/key/repair/freeze/bomb/mirror, cancellation, existing mission/save/undo regressions. Combined-transition fixtures assert contact order, net durability, one gust, exact row, Undo restoration and delivery; they are transition fixtures, not generated campaign recipes.
- `coverage.json`: structural lesson coverage and lower-budget classifications. Generator rejects unproved candidates; successful solver searches do not prove human enjoyment or that every imaginable authored layout is covered.

The initial gust relocation needed a lower playable board under the Domain validator; the retained initial failure was resolved by generating at least three boards for a live-gust lesson and rejecting invalid constructive candidates. Calibration also respects the actual early-descent lesson instead of trimming away its deliberate Drop command. No production Domain rule changed.

Run `python3 Tools/check_durability_budgets.py` for the catalog and focused checks, or add `--focused` for just the 43 methods. Focused mode preserves catalog evidence.

Unity-native visual and PlayMode validation NOT RUN: MCP script/project-root requests did not complete; stopped waiting after bounded attempts. Only the ambient impulse was changed in presentation; resolver-driven gust effects remain. Physical-device latency/FPS/player enjoyment NOT RUN. No iPhone export/build/install. Active pinned recipes retain their hashes and commands; policy applies to fresh generation.
