# Menu reset — native Unity review, 6 October 2026

The author rejected the prior menu screenshots and requested a substantial layout/readability reset, then specifically asked for language carousels. This replaces the fragmented menu card stack with one workshop folio. Headings, body copy, artwork, progress and actions have separate roles; the large painted scenery remains around the UI, including on wide screens. Languages and bonus lessons use broad arrows. No puzzle-rule change.

## Evidence and corrections

- Native camera renders were made through the official Unity MCP in Unity 6000.3.8f1. The first implementation batch covered 29 menu/gameplay variants. The expanded confirmation covered 59 saved variants including EN/RU at 320×693, EN at 390×844 and 1440×900, and Ukrainian large text with 4.5% top / 7% bottom simulated safe insets at 320×693. These are render fixtures, not physical devices.
- First inspection found five overflowing areas and visibly broken language/tab words. The author requested carousels during this pass; those replaced the proposed narrow language columns. Native follow-up found ten remaining subtitle/demo/guide/confirmation height failures. Both finding ledgers are retained. The compact confirmation had accidentally changed the previous inactive heading during deferred destruction; current element references now place its own heading on its own paper.
- Large-text teaching paragraphs now have their own sufficient areas; demonstration result text reserves 28 physical pixels. Footer baselines are recalculated when the viewport changes. Compact action captions stay whole. Bonus lessons use arrows and a counter rather than six undersized controls.
- The final inspection also caught carousel arrows over part of the stamp/record receiver previews. Previous/back/next now share the footer; only those pages and the affected bomb reaction were captured again. Final focused text-overflow ledger: **0**. Earlier ledgers are evidence, not hidden failures.
- Bomb feedback has a live folded ribbon, an extinguished-fuse sprite and rising colored glints at its actual hanging body. Feedback follows the canvas anchor across wind and viewport changes. The letter stays painted directly on the bomb; its discarded separate tag was not restored. Other tools retain their own matching tags and get related success bands.
- Existing native-frame hash checks confirm rendering did not execute puzzle commands. Two bomb wind poses have different angles with the same logical hash in bomb-motion.csv. Pauses/Reduced Motion and canonical bomb contacts are checked by the selected test.

## Actual tests

Four existing PlayMode cases: **4/4 PASS**, 14.2090045s, playmode.xml:

1. WordLanguageNeedsConfirmationAndAppliesOnlyAtNextStart — carousel browsing/cancel/confirm, direct UI-language change and unchanged current attempt.
2. SettingsChangeActualPresentationPreferences — actual sound, motion, contrast and large-text controls.
3. HangingDevicesFollowDeckWindAndFreezeWithoutSpendingMoves — actual rendered suspension/painted bomb mark, wind, pause, Reduced Motion, contrast preservation, canonical defuse and safe sprite/ribbon.
4. StampTrailAndNormalDeliveryHaveSeparateRecords — record carousel navigation and retry retains the stamp goal.

Final Editor compilation succeeded; final Console **0 errors/warnings**; Play Mode stopped. Initial editor sessions hit the Localization toolbar initialization/domain-reload issue before captures, leaving nonserialized Interaction absent. Those sessions were discarded and restarted after settled compilation; the incident is retained in initial-editor-reload.txt. It is not a clean-first-launch claim.

No full suite, new player build, physical iPhone/Android test, live performance measurement or human enjoyment/accessibility study was run. The final local verdict is readable and functional in the inspected cases; this does not certify every device, every dynamic state or absence of all bugs. Source-color contrast of folio ink against paper is 14.62:1; this is not a blanket contrast measurement of the artwork.

Only 14 representative PNGs and matching metrics remain from this pass, alongside findings and the actual test result. Native menus: [workshop](workshop-ru.png), [languages](settings-language-ru.png), [large Ukrainian language choice](settings-language-large-safe-uk.png), [safe-area guide](basics-large-safe-uk.png), [clear stamp preview](contracts-large-safe-uk.png), [wide workshop](bag-wide.png). Reaction: [bomb defuse](bomb-defuse-flourish.png).

Asset prompt provenance: ../../Docs/AssetPrompts/MenuReset/BombSafe.txt. Original sprites, user Cloud settings and unrelated files were preserved.
