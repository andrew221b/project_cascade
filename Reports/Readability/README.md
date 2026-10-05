# Readable menus and independently hanging tags — 6 October 2026

Scoped presentation correction. Existing workshop art, palette, content, prices, puzzle rules, profile schemas and IDs are preserved. No new raster assets, packages or player builds.

## What changed

The required-symbol sign now hangs on two continuous shaded hemp cords from the tool's wooden crossbar. It has its own bounded, damped pendulum and reacts to the common wind independently of the heavier tool. The guide uses the same assembly. The sign is a contact instruction, not another playable piece.

Shared headings and related reading copy sit on opaque ivory cloth surfaces rather than over detailed islands/clouds/timber. Deep teal ink has 14.62:1 source-color contrast against these surfaces. Shelf labels, thumbnail selection and disabled actions now have stable contrast. Readable button padding, physical minimum heights and actual counter line height avoid localized text clipping.

## Actual validation

- [Initial scoped PlayMode](playmode-initial.xml): 1/1 PASS, 3.87 seconds. It checked motion, attachment coordinates, pause/reduced motion and unchanged gameplay state but did not establish that the new cords rendered.
- First native inspection exposed absent cord rendering. The diagnostic found the missing CanvasRenderer; the Graphic now explicitly requires it. The case was strengthened to require the renderer and a nonempty rendered mesh.
- [Corrected scoped PlayMode](playmode.xml): 1/1 PASS, 3.31 seconds. Independent sign motion, rendered mesh, cord/plate attachment, pause, Reduced Motion, unchanged hash and zero spent moves.
- Native full inspection and correction/confirmation retained 22 frames. A final focused four-menu confirmation adjusted two remaining localized button captions and their neighboring controls. No test suite was repeated after this final menu-only spacing change.
- [Initial text findings](text-overflow-initial.txt) record the first labels encountered. Final [text findings](text-overflow.txt) are empty in the inspected matrix. Final scanning occurs inside the actual capture viewport, after font/layout settling; earlier scanning happened after returning to the Editor viewport, so it is not a complete small-screen fit audit.
- [Live tag samples](tag-motion.csv) record actual angle/time and the same logical state hash for all three frames. Camera rendering itself asserts unchanged state. Native visual review confirmed both cords are visible and remain attached.
- Final Console 0 errors/warnings. The renderer diagnostic error was investigated and cleared before the corrected runtime checks. Play Mode stopped; generated Editor link/localization settings noise was restored. The user's Cloud settings remain separate.

## Native frames

| Area | Captures |
| --- | --- |
| Workshop/collection | [Bag RU](bag-ru.png), [collection](collection-ru.png), [locked gift](collection-locked-ru.png), [model](workshop-ru.png) |
| Journal/requests/records | [Journal](journal-ru.png), [requests](requests-ru.png), [routes](records-ru.png), [stamp trails](contracts-ru.png) |
| Tools/help | [Power-ups](powerups-ru.png), [ribbon](ribbon-ru.png), [basics](basics-ru.png), [bomb guide](bomb-guide-ru.png) |
| Settings/pause | [Sound](settings-sound-ru.png), [comfort](settings-comfort-ru.png), [languages](settings-language-ru.png), [pause](pause-ru.png) |
| Size/locale references | [EN phone](bag-en.png), [wide](bag-wide.png), [large-text UK collection](collection-large-uk.png) |
| Connected letter sign | [Live pose 0](letter-tag-0.png), [pose 1](letter-tag-1.png), [pose 2](letter-tag-2.png) |

RU menu references use 320×693, EN phone 390×844, wide 1440×900; the three word-tag poses are 390×844 RU. Journal, Power-ups, sound settings and wide bag were recaptured after the final shared button-space change. Other stored frames precede that final action-height/padding change. Visual fixture progress is solver-proved in memory with persistence disabled; it is not the user's earned progress. Temporary review contact sheets were kept outside the repository.

Not run: full suites, new player build, physical Android/iPhone, device performance, audio/haptics, extended play or nonreader comprehension testing. This is cosmetic pendulum motion, not gameplay Rigidbody physics or proof of long-term enjoyment.
