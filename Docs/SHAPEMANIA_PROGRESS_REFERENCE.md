# ShapeMania progression reference — read only, 5 October 2026

Inspected the author's `shape_sorting-final` source; no source code, service IDs,
art, save keys or packages were copied into CASCADE.

- `DayStrikeController`: one visit counted per local date; a seven-day ladder
  leads to a selectable booster. A missed date resets the ladder; the seventh
  milestone is separately saved. CASCADE borrows a visible, finite ladder and
  a meaningful choice at its end. Proposed adaptation: seven visit stamps with
  a gentle pause across missed days, cosmetic reward, no campaign power gate.
- `LeaderboardLocalController`: ten personal results per difficulty, distinct
  difficulty tabs and an explicit empty state. CASCADE should separate puzzle
  identity/rules/locale and assisted versus unaided records. A short verified
  route matters more than fast tapping; first clears remain separate from bests.
- `GlobalLeaderboardService`: distinct best-round, weekly and all-time records,
  pending submission handling and score plausibility checks. Future CASCADE
  online design needs authoritative replay verification, retry-safe submission
  and honest offline state. There is no global leaderboard implementation in
  this revision, and no generated scores are presented as real players.

Author's online-mode request is design-only. Relay and equal-loadout concepts
are in JOURNEY_REDESIGN.md; do not install networking or leaderboard SDKs as a
side effect of this reference review.
