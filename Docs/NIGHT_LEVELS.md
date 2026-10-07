# Star Observatory levels

Design note, 7 October 2026: append C161–C184 as the ninth campaign world,
Star Observatory (world index 8, first stop index 160). Existing C01–C160 IDs,
order, data, rules 2.0, schemas, dictionary IDs and durability compatibility
records remain unchanged. The extension adds Shapes puzzles and EN/RU/UK
Words bindings through the existing SymbolId mapping; it introduces no rules.
The campaign and descent eligibility bounds expand to 184 while existing
recipes, saved source hashes and descent versions retain their behavior.

The three-floor layouts combine existing mirrors, roller entries, final-row
contacts, linked and fragile edges, and one-use bridges. Live gusts continue
to come from the existing version 5 descent calibration; published JSON does
not gain fields. Deterministic solver proofs establish solvability and command
minima, not human enjoyment. Unity visual review remains part of the parent
task's validation.

`Tools/build_star_observatory.py` uses seed 26100709 and accepted 24 layouts
from 538 candidates. It refuses to rewrite the published extension. The new
entries are compact append blocks; the generator splices the original text
without reserializing incumbent objects. Removing the two blocks and their
required commas reconstructs the original file byte for byte.
The new layouts are distinct from all previous 160 after normalizing symbols and
reflection; the prefix containing 120 extension levels and 360 bindings has
the same canonical SHA-256 before and after:
`2bc315845290ecedcb7d0ffae7fb641a9cdea4ca2a0b093dde5ce067c8a42357`.
The catalog now contains 184 primary puzzles, 549 localized bindings and the
same 111 dictionary entries. The runtime extension matches the authored file
byte for byte.

| Nodes | Existing device combination | Published minimum commands |
|---|---|---|
| C161–C164 | One-use bridge, roller, repair, fragile mirror landing | 8, 8, 8, 8 |
| C165–C168 | Keyed shutter, mirror, linked exchanges | 9, 11, 9, 12 |
| C169–C172 | Fuse, roller, repair, fragile approach | 9, 10, 9, 10 |
| C173–C176 | Linked exchanges, key, repair, mirror | 9, 9, 9, 12 |
| C177–C180 | Bridge, linked fragile exchanges, mirror | 8, 8, 8, 8 |
| C181–C184 | Fuse, repair, mirror, linked roller landing | 9, 9, 9, 9 |

Python reference validation completed 96 shortest-route proofs and replays:
all 24 Shapes puzzles and all 72 word bindings share their published minimum.
Every selected source path uses exchanges on at least two floors and has at
least two first states within minimum + 2. The largest completed reference
graph contains 22,122 states. Evidence: `Reports/NightLevels/python_balance.json`
and `Reports/NightLevels/reference_validation.json`.

`StarObservatoryLevelTests` adds native same-resolver graph/minimum checks,
word-binding replay/restore checks, all-three-stage v5 descent proofs including
live gust damage, and a fingerprint over all 637 pre-existing puzzles and
bindings. `CampaignExtensionTests` retains its 120 original route cases and
now checks the 184-node catalog. Native execution and Unity visual validation
are owned by the parent task and were not run by this content subagent.
