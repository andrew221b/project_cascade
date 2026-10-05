# UI and motion refinement — 2026-10-05

Preserve the sky-workshop palette, illustrated assets, Balsamiq type, deterministic rules, saves and all IDs. This pass changes presentation only.

Observed problems: oversized undifferentiated cream sheets, uneven title/subtitle positions, same-weight actions, floor-colored bands bleeding through workshop panels, tiny floor captions, flat polygonal ropes, hard square weather particles, duplicate render work, and glider disappearance when a piece is held.

Motion thesis: the suspended machine carries weight. A landing transfers a small impulse into smoothly braided ropes; a held piece and returning board settle without a hard pose reset. Menus acknowledge navigation with a short readable paper fade, preserve immediate input, and distinguish the primary action. Atmosphere moves slowly at different depths and fades near boundaries, never across important foreground feedback. Reduced motion and effects-off remain supported. No animation owns puzzle state.

Scope: shared menu layout/type/actions, all menu routes, gameplay HUD/device counters, rope material/sampling, button focus/press response, cloud/glider/weather pacing. Use existing illustrations; mathematical rope shading and soft particles do not require new bitmap generation. No new packages, economy, rules, schema or IDs.

Validation: one native screen matrix spanning all menus plus phone/wide/large text, targeted interaction and presentation tests, and a bounded motion sequence. One correction batch and confirmation; report OS/hardware and human-readability limits. No broad campaign solver rerun for presentation-only changes.

## Review corrections

The native review matrix covered all 22 screen routes, eleven device-guide pages, narrow EN/RU/UK layouts, wide layouts and simulated safe insets. It exposed a wrapped last letter in the narrow language selector, singular/plural errors in board/fuse counts, and insufficient emphasis on the result action. Corrections were batched: full-width language choices; existing localization keys retained with neutral count wording; explicit gold Next delivery/Retry. A dialogue capture initially followed an already-heard story into gameplay; the confirmation fixture explicitly opens the story.

One runtime defect in the new weather code was caught before review: MaterialPropertyBlock initialization ran in a MonoBehaviour field initializer. It was moved into Initialize. No profile schema, puzzle rule, package, economy, content ID or authoritative transition changed.

## Completion evidence

15 distinct selected PlayMode cases have passing evidence; final affected run 4/4 PASS includes all 13 event stages × 4 interruption paths. The measured locked-Editor slowdown required a 30-second functional-test hang watchdog and background execution restoration. It does not establish 30-second gameplay latency or a device performance pass. Original watchdog failures and the rejected frame-count experiment remain in reports.

47 native stills and 32 motion frames were reviewed; final narrow-language capture confirms the corrected bottom inset. One Mac build succeeded with zero errors and 346 existing package/configuration warnings. New player foreground inspection is blocked by the confirmed lock screen. See Reports/Polish/README.md for precise limits.
