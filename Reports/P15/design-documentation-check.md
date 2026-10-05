# P15 design documentation check

5 October 2026. Ordinary localization extension of the existing Sky Wind Workshop
identity; no new design system, artwork or tokens were introduced by this pass.

## Evidence inspected

Read PRODUCT.md, DESIGN.md, .impeccable/design.json, both matching journey surface
briefs, Docs/P15_LOCALIZATION.md and Reports/P15/impeccable-review.md. Sampled
CascadeGame.cs, CascadeGame.Flow.cs, CascadeGame.Journey.cs,
CascadeGame.Language.cs and UiCopy.cs, with the affected tracked-file diffs.
Read GamePreferences.cs and LocalizationTests.cs for the implementation limits.

Independently counted Data/ui_localization.tsv: 215 unique source keys and 645
non-empty EN/RU/UK entries. The supplied table-completeness report also records
matching format arguments. Parsed the final test XML: 313/313 EditMode and
38/38 PlayMode passed; the latter includes UI-language attempt preservation and
confirmed next-start word-language tests. This documentation pass did not rerun
Unity tests.

Verified 39 final PNGs and parsed all 39 paired JSON files: 36 images are
320 × 693 and the three Words views are 390 × 844. Opened six representative
PNGs directly: en-languages, ru-languages, uk-languages, uk-story, ru-mixed8 and
en-map. The independent finish review inspected all 39 and records disposition:
ship for the scoped P15 Editor views. The final active-TMP fit findings file is
empty. Addressables build evidence records a successful StandaloneOSX content
build, not a complete player build.

## Incumbent system comparison

The sampled source and rendered views retain warm ivory paper, dark teal ink,
gold selected actions, mint machinery and the stable six-symbol palette.
Balsamiq Sans remains live TMP lettering, with the existing menu/body/button
hierarchy and native reference-canvas sizes. The new language and confirmation
screens reuse the same paper fields and buttons. Line wrapping accommodates
Cyrillic without establishing a new type ramp or palette.

The complete row and receiver, unobscured symbol faces, illustrated journey map
and large character portrait still express the surface brief. SelectedLocale
and LocalizeStringEvent supply the UI language; raw symbols bypass translation.
The separate confirmation writes the next-puzzle locale preference. Those are
implementation facts, not new global visual rules.

DESIGN.md and .impeccable/design.json were preserved byte-for-byte. Their
palette, typography, component and motion tokens remain the incumbent source of
truth; no token was added to legitimize a defect. No new raster provenance is
required by P15. Their SHA-256 values at this pass are:

- DESIGN.md: `800d0284b94bef90f4016bca60b33889f20340a1ef3af413617d8ca4c7406d7d`
- .impeccable/design.json: `7902089c1cc0f63ee7849de873b2f4bedefef03efb529726a1965e390774f06f`

## Product truth and limits

PRODUCT.md already stated English as default with Russian/Ukrainian additional;
there was no literal English-only claim to remove. It now identifies that as
implemented P15 behavior, records the current scoped evidence, and retains the
session-only boundary. GamePreferences remains in memory, and journey clears and
heard dialogue remain session collections. Durable preferences, progress and
attempt recovery belong to P16.

Pre-existing documentation drift was not repaired: DESIGN.md still says full
localized editorial and glyph review remains P15 work, although automated glyph
coverage and Editor fit checks now exist; native-language editorial review is
still outstanding. PRODUCT.md's historical P14 evidence paragraph still records
a then-pending visual follow-up. The design record and sidecar still identify
their P14 capture baseline. These are provenance/status wording, not evidence
for a changed design system, and broader drift repair is outside this pass.

No native-language-editor review, physical-device touch/safe-area/haptic or
performance validation, complete player build, acoustic mix review, or human
enjoyment test was performed here. The original translations remain editorial
drafts. Automated results and still Editor captures do not establish those
qualities or whole-product release readiness.
