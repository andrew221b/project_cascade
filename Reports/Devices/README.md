# Devices revision — 2026-10-05

## Delivered
- Six original transparent tool sprites, preserved native pixels, generated with built-in ImageGen. Prompts, hashes and inspection sheet: Docs/AssetPrompts/Devices.
- Mounted recognizable bomb/key/repair/stamp/spool/hourglass, separate matching-piece pocket, explicit wood and bomb counters. No strength beads, bomb-as-shape circles, fuse pips or floating entry triangles in gameplay. Entry captions and connecting cord explain floor rules; clicking a tool opens its demonstration without making a move.
- Drag prediction highlights the affected tool and reports its result. It never grants a pickup or changes the puzzle. Actual pickup/defuse/repair uses event-driven local feedback and counter changes.
- Six authored B01–B06 lessons: one-use far swap pickup, carrying that charge to another board, two bomb tick skips, and combining tools. Old C01–C160/R01–R06 content untouched. Rule note: Docs/DEVICES_REWORK.md.
- Eleven device-guide entries and a six-lesson Power-ups screen in Mika's workshop. New copy in EN/RU/UK.
- Map stop cards/numbers removed. Available destinations colored; locked destination artwork desaturated while sky stays blue. Localized You are here arrow. Invisible island targets retain button semantics.

## Evidence
- 21/21 initial EditMode checks; 137/137 regression checks (overlap). Union:146 distinct passing cases. Resolver, solver, replay, save, Undo, original devices, new authored solutions and localization covered. No full campaign graph rerun.
- Initial focused PlayMode3/5: two animation-completion checks exceeded the existing3-second deadline. Unchanged focused reproduction2/2 passed with the same deadline (DevicesTiming). Do not interpret that as proof against timing flakiness.
- Dedicated literal-letter socket regression1/1 passed across EN/RU/UK (DevicesGlyph). Initial visual catch of accidental glyph localization fixed using raw symbol text. No translated symbol IDs.
- Final preview interaction2/2 passed after adding tool prediction feedback (DevicesPreview); six distinct PlayMode cases now have passing evidence. No full PlayMode rerun.
- Native MCP capture matrix:12 core frames plus a shared key/repair socket at320px UK and a pickup-prediction frame. Frame captures check that rendering leaves state hash unchanged. Hourglass before/after is one actual committed exchange. No physical phones used.
- Independent Impeccable reviewer found5 concrete issues: two text collisions, Mika/arrow overlap, old tool lesson imagery/copy, residual turquoise on locked Lagoon scenery. All five corrected and reviewer confirmation disposition:ship for this scoped visual pass.
- First capture attempt hit a double-Graphic component error in the guide; corrected before final capture batch.
- One fresh universal Mac build: CASCADE-Devices.app, Succeeded,0 errors /346 existing package/config warnings,248,911,193 bytes (~237.4MiB report,238MiB disk). MCP wrapper reports warnings as an error; BuildReport and completed app establish success. No rebuild was run.
- CUA could not inspect the fresh standalone window: native inventory confirmed the Mac is locked and automatic unlock failed. No standalone UI/play/performance success is claimed. The14 screenshots are actual running Unity Editor game renders through MCP.

## Limits
Editor screenshots and virtual input do not establish five-year-old comprehension, enjoyable challenge, retention, native-language editorial quality, or phone performance. No fresh physical Android/iPhone test, haptic/listening test, prolonged session, or live online mode. Prior Craft performance sample is historical and does not measure this revision. Rules stay deterministic; visible movement never changes gameplay.
