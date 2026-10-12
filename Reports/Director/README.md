# Director and wind validation — 5 October 2026

The adaptive director applies only to fresh uncleared journey deliveries C41–C160. It pins a variant before input, uses the exact transition solver and falls back to published content if the variant is not proven. It never changes an active puzzle because of hesitation. Ordinary Retry keeps its pinned definition; More wood starts a separate recovery attempt after repeated failures.

## Executed checks

- `editmode.xml`: 7/7 PASS, 0.818 s. Cadence/deduplication, retries and recovery, recipe bounds across late primary/localized content, save replay/tamper rejection, legacy schema-1 defaults, wind/contacts and smooth bounded view wind.
- `../DirectorPlay/playmode.xml`: initial run 2 PASS/2 FAIL, 12.150 s. Hanging assemblies and deliberate failure/restart flows passed. The guide test queried the wrong existing button name; the fallback fixture incorrectly assumed C81 Tight was unsolvable. Neither failure was presented as a gameplay pass.
- `../DirectorConfirm/playmode.xml`: corrected targeted cases 2/2 PASS, 9.415 s. The actual Landing movement button opens the pictorial wind guide, pointer swaps/drop follow the solver route to victory, save restoration and publication retain the recipe. C42 Tight provides the verified unsolvable fixture and falls back before input opens. Four distinct PlayMode cases therefore have passing evidence; no second full suite was run.
- `wind-proofs.txt`: C81–C88 wind variants all solved and replayed; shortest routes 6, 6, 5, 6, 4, 6, 4, 6 commands. Multiset preservation and final-row repair/bomb contact ordering checked. Other recipes received definition/bounds validation, not an exhaustive offline solve; runtime preparation proves the actual selected puzzle.

## Native visual evidence

- `wind-game.png`: EN gameplay, 390×844. Longer attached device straps, separated teal wind arrow, target alignment and compact forecast.
- `wind-small-ru.png`: RU gameplay, 320×693. Coach fits its panel in two lines; destination wind caption fits above the receiver.
- `wind-guide-ru.png`: RU pictorial before/after guide, 320×693. Direction matches the shift; concise copy and return to current puzzle.
- `stronger-wood.png`: EN failure screen, 390×844. More wood is the sole gold action; ordinary Retry remains available.

First inspection found RU overflow and competition between the wind arrow and fragile notch. One correction shortened translations and moved the arrow to a clear part of the plank; the four final frames replace the initial frames and were inspected. Corresponding JSON files contain viewport/control measurements; their unset device-text metric is not an accessibility pass. Capture assertions retain the gameplay state hash. The editor connection was interrupted by a new Unity session; native MCP reconnected and the final captures completed in that session. Final console returned zero errors/warnings; Play Mode was stopped.

## Limits

No player build, physical iPhone/Android test, device performance profile, haptics or human playtest was run. Static frames cannot prove all animation timing, and automated routes cannot prove enjoyment or retention. The shared irregular breeze and gust/attachment/freeze behavior have focused automated evidence; real-device motion comfort remains unverified. No new art or package was needed for this scope.
