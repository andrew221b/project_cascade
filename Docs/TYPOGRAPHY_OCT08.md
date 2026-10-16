# Interface typography — 8 October 2026

Add an optional ToyArt.displayFont reference for storybook headings. Existing
Body/Title/Balsamiq assets and their GUIDs remain intact. New static TMP assets
use Nunito SemiBold for readable copy, Nunito ExtraBold for controls and counters,
and Alegreya ExtraBold for page/item/ceremony headings. All EN/RU/UK UI glyphs
and puzzle alphabets are baked; Nunito's missing right-arrow uses Alegreya as a
one-way fallback. No player schema, currency, catalogue or gameplay change.

Source fonts: google/fonts ofl/alegreya and ofl/nunito, SIL OFL1.1; licenses live
beside the three source TTFs. Static weight instances800/600/800 were generated
with fontTools4.60.1 in a temporary tooling directory. No Unity package added.
One shared SDF material per semantic role supplies restrained ivory/gold edges
and a soft shadow; short descriptions keep a readable dark-teal fill. Symbol
faces and button captions retain their own materials and contrast.

Opened rewards show one ownership/result line, and bonus coins show their
amount once. Incomplete trophy part counts and save failures remain; buying a
trophy no longer repeats its Collected status. Reward ribbons use a short
general heading, within the illustrated paper rather than its gold tails.
Mika trophy descriptions name their animation, without repeating the item name;
tool explanations retain their limits in concise EN/RU/UK copy.

Native review uses isolated temporary profiles. Completion statistics use a
valid three-stage completion; opened tool/coin views use temporary visual
fixtures and do not assert acquisition/economy behavior. Menus, settings, map,
shop, trophies, parcel and statistics are captured through Unity MCP at390px;
Ukrainian parcel/statistics are also checked at320px with Large Text. Static
captures use Reduced Motion. New font coverage is verified against every
localized UI character, with one static atlas per font. No phone build or
full gameplay suite requested. Evidence: Reports/Typography-Oct08.

Final native review includes all6 tool/guide variants, premium Mika/scenery,
unopened parcels and incomplete trophy fragments at320px Ukrainian Large Text.
Initial body-copy and narrow page/acquisition overflow was corrected and the
affected views recaptured. Final scoped overflow checks report0 and the final
Console contains0 errors/warnings. Play is stopped; temporary profile selection
is cleared.20 native screenshots document the resulting layouts.

## Cyrillic display companion — 9 October 2026

The current Latin display face is McLaren, which has no Cyrillic glyphs. Replace its Nunito SemiBold fallback with the existing static Balsamiq Sans Regular asset; use the existing Balsamiq Bold as weight 700. Its rounded hand-drawn forms and 58-unit cap height sit close to McLaren's 57 at the same 80-point bake. Russian/Ukrainian headings, buttons and puzzle letters now share that companion; readable Nunito body copy already covers all three languages and remains. The typography builder preserves this wiring on regeneration.

Both Cyrillic assets contain all localized Cyrillic letters, Ё/ё, Ґ/ґ, Є/є, І/і, Ї/ї and the Ukrainian apostrophes. Static atlases, 80/8 sampling/padding, scale 1, source licenses and existing GUIDs remain; no extra font atlas or runtime glyph generation. Coverage and original glyph-table checks passed. Actual-font specimen compared locally; native Unity rendering/locale switching/narrow layout not run because MCP did not return. Evidence: Reports/CyrillicTypography-Oct09/coverage.txt. No iPhone export/build/install.
