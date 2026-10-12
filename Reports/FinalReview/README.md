# Final bomb simplification and visual review — 6 October 2026

The separate sign made a simple contact rule look complicated. Bombs now carry their required letter or shape directly on the dark metal body, in warm ivory paint. No bomb crossbar, sign plate, sign pendulum or sign cords are instantiated. The platform suspension still supports the bomb. The mark follows the body, including its wind motion and contact pulse. Other tools keep their existing tags.

The same mark is used in the animated bomb guide. Its example is larger, and its redundant tag explanation is removed. No gameplay transition, move budget, price, package, schema or ID changed. Existing sprites and their metadata were preserved; no additional art was necessary.

## Review and correction

Unity MCP ran and rendered the actual playable, rather than a mockup. One batch produced 26 frames: 22 menu variants and four gameplay frames. The inspection covered the map, lessons, parcel, workshop hub, journal, owned/locked collectibles, workshop construction, requests, records, stamp contracts, power-up lessons, ribbon lessons, basics, bomb guide, all three settings pages and pause. It included RU 320×693, EN 390×844, EN 1440×900 and a UK collection with Large Text.

The reviewed menu frames showed readable headings/captions, aligned actions, recognizable selected/disabled controls and consistent reading surfaces. The wide workshop is one continuous background. This is a visual review of the inspected states, not a claim that every possible screen/state or device is bug-free.

The initial batch reported three instances of a tight line box on the painted RU letter. The letter was visible, but its text rectangle was too short for the font metrics. Its rectangle was expanded vertically while preserving the visual center. Only the two moving word-bomb frames and the defused frame were recaptured; the final text-overflow file is empty. The initial findings remain in `text-overflow-initial.txt`.

Eleven representative PNGs and their native layout metrics are retained, avoiding a duplicate archive of every unchanged menu. Temporary contact sheets were used only for inspection. The menu/shape-guide frames precede the final letter line-box correction; that correction only affects glyph marks. `bomb-letter-0.png`, `bomb-letter-1.png` and `bomb-defused.png` show the final corrected implementation.

## Actual validation

- `playmode.xml`: one extended existing PlayMode test, **1/1 Passed**, total 5.8786 seconds. Checks absence of the bomb's extra sign parts; paint fixed to the moving body; wood/tool motion; suspension attachment; pause and Reduced Motion; no moves/state changes from animation; the correct RU symbol; canonical bomb defusal; and the unchanged independent repair tag's rendered cords and attachment points.
- `bomb-motion.csv`: native bomb angles 1.4858° → 0.2010° at identical logical state hashes. The mark remains attached in the two final timed frames.
- Final native console: zero warning/error entries. Editor stopped, not compiling/updating. A first refresh command encountered a transient dynamic-script DLL reload; subsequent native commands compiled and executed successfully.
- Capture fixtures disable persistence before seeding review progress and changing preferences. Tests use the existing isolated profile setup. No real player progress was intentionally written.

**Not run for this bounded revision:** full test suite, player build, physical Android/iPhone, new performance/audio/haptics measurements, human/non-reader usability or enjoyment playtest. Earlier validation remains in its original reports and was not represented as rerun here.

Reproduce the native batch in compiled Play Mode with `Cascade.Editor.Playable.ReadabilityCapture.RunFinalReview()`. Use `ConfirmBombMark()` for the three affected word-bomb frames. Run the focused test with `PlayModeValidation.RunFocused("FinalReview", "Cascade.Tests.PlayableTests.HangingDevicesFollowDeckWindAndFreezeWithoutSpendingMoves")` outside Play Mode.
