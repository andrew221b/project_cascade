# CASCADE Craft — 5 October 2026

## Delivered
- Rebuilt bag/keepsakes/workshop as aligned object shelves with Mika, four simultaneous item choices, a large selected object and short progress/action copy. New original WorkshopRoom and WindGardenPanorama raster assets; provenance and exact prompts in Docs/AssetPrompts/Craft.
- One continuous wide sky; tablet map edges blend into it. Regional clouds/weather/pollen and map ambience remain bounded by the existing presentation system. Phone gameplay keeps the full first-region panorama at the lower horizon rather than cropping its windmill/arch away.
- Two continuous hemp ropes connect floors to the receiver. Visual springs carry block weight; exchange arcs, falling rotation, landing bounce/squash, wood chips, wear marks and cracks remain scripted presentation. Resting pieces and contact shadows now meet visible wood. Warm key plus sky fill; direct drag and reduced-motion support remain.
- Six optional R01–R06 Ribbon Reach puzzles. One charge swaps endpoints across one or two pieces; all crossed edges must be open, linked corridors reject, fragile/bridge/contact rules apply once. Undo restores the charge. Authored solver proofs exist for all six; original C01–C160 data/hashes remain unchanged. No online/economy/package change.
- Phone interface is explicitly portrait. Safe-inset simulations and 320/390 portrait, tablet, 1440×900 desktop captures inspected. 844×390 landscape is a retained rejected diagnostic; short desktop windows are not a supported baseline.

## Verification
| Evidence | Result |
| --- | --- |
| Craft/editmode.xml | 475/476 initially; R03 was solvable without the ribbon |
| CraftReach/editmode.xml | All 9 Reach cases pass after correcting R03; all six trails require Reach in the shortest proof |
| Craft/playmode.xml | 7/7 actual scene/input/lifecycle tests pass |
| CraftFinal/playmode.xml | 3/3 affected weight/rope/drag regressions pass after final contact adjustment; subset of the seven |
| 18 native PNGs and matching layout JSON | MCP-rendered actual Unity scene; safe insets simulated, not physical-device captures |
| Motion/00–19.png and events.csv | Real event playback: exchange → fracture → fall → settling → rest, with one unchanged committed state hash throughout |
| impeccable-review.md / impeccable-verdict.md | Seven scored material findings resolved after bounded correction; scoped ship, not whole-product or AAA certification |
| build.txt | One fresh universal Mac player: Succeeded, 0 errors, 346 installed-package warnings, 247,298,651 bytes |

The initial full run hit MCP's 300-second timeout but the test runner wrote its complete 357-second result. The failed Reach case was fixed and its nine-case suite rerun; a second complete suite was intentionally not run. No new Python parity claim is made for rules2.1-longreach. Existing campaign tests passed in the full run.

## Performance and device limits
Actual standalone C160 at 1440×900, Apple M4 Pro, 90 warm-up frames +240 samples, target60: median16.703ms, p9517.520ms. Whole-player allocated memory106,835,389→107,089,661bytes. The GC counter is unavailable in this non-development player; its zero output does **not** mean zero allocation. This short idle-scene sample is not a long-session, thermal, input-stress, GPU-time or mobile benchmark. Mac was locked during native-window verification, so no foreground claim is made. Editor sample median99.998ms/p95101.649ms was consistent with a100ms Editor throttle and is not player performance.

xctrace reported no recording devices and adb was unavailable on PATH. Physical Android/iPhone performance, touch/haptics, safe-area behavior and orientation were not tested. App-window verification of this new player is pending Mac unlock; the actual scene was visually reviewed through Unity MCP. Player.log records no exceptions/errors at this checkpoint. Sandbox-only launch exited134 before initialization; the authorized normal WindowServer launch produced the measured result.

## Artifacts and preserved state
New local app: `CascadeUnity/Builds/CASCADE-Craft.app`; previous `CASCADE.app` preserved. Build outputs stay outside Git. Source/metadata, reports, prompts and docs are retained. The user's UnityConnectSettings cloud-enable edit stays separate. Diagnostic `-cascade-perf` uses a temporary isolated profile and does not overwrite the player's progress.

Not established: human enjoyment/retention, understanding by non-readers, complete bug absence, all-device performance, native language editorial review, App Store metadata/signing/notarization, online play, live store services. These are explicit validation limits, not automated passes.
