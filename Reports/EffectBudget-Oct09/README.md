# Effect budgets — 9 October 2026

The solver includes actual durability costs, repair amounts, bomb damage/countdown/freeze, key edges, ribbon charges and structural effects. Earlier witness checks exposed three alternative winning routes that bypassed intended benefits; initial results remain in `benefit-audit-before.csv`.

New generation accepts a stage only if its winning witness is valid and removing any individual helpful device makes delivery provably impossible. All other conditions are retained. Unknown or cancelled counterfactuals reject the candidate. Hourglass bombs on multi-platform stages cost two durability if freeze is missed, preventing a later board from absorbing that mistake. This is a recipe parameter; the domain transition function is unchanged. Existing pinned attempts retain their saved recipe.

Validation passed:
- 540 primary campaign triads and 6,471 primary/localized stages generated and replayed; accepted recipes include the new necessity gate. See `campaign-proofs.csv` and `campaign-result.txt`.
- Detailed campaign + personal mission sample: 48 stage witnesses replayed and 28 independent helpful-device removals; every removal returned Unsolvable. See `benefit-audit.csv` and `logic-result.txt`.
- 23 actual focused NUnit methods invoked with setup on production pure C# sources using the bundled Mono runtime; required benefits, harder/different missions, cancellation, real profile save/mode switching, checkpoint and undo all passed. See `pure-tests.txt`. The focused runner avoids a player build.

Limits: Unity-native test runner/UI rerun after this change was unavailable while the Mac was locked and its MCP endpoint switched between Editors. The same pure NUnit methods ran outside the Editor; no new native/performance/device claim. No iPhone build or installation was attempted. Optional stamps remain optional, and structural effects do not grant extra moves.
