# UI copy trim — 7 October 2026

Shortened 15 frequent shop, trophy, reward, tool-guide and ad messages in EN/RU/UK. Their combined length fell by 33% / 31% / 31%. Settings already explain the next-puzzle language change briefly and remain unchanged. Story dialogue remains unchanged.

The copy retains the decisions that matter:

- Automatic-ad removal is one purchase; reward videos stay optional. Test purchases have no charge and last for the current session.
- A scroll shows one move; at most two tools can be used per stage. Tools cap the run at two stars. The separate no-clean-record warning remains intact.
- An extra Undo follows two free Undos on the same board. Return restores this stage's last broken board and cancels later moves.
- The lantern keeps normal wood cost and its once-per-stage limit. The lens previews the next landing without moving pieces.
- Trophies are worn on the map; legacy trophies remain earnable in parcels. Three parts make a trophy.

Only translation values changed. All keys, row order, columns, placeholders, prices, rules and IDs are preserved. The diff contains the per-message before/after text.

Validation: compared every TSV row with the pre-edit version; exactly 15 changed, every locale value shorter, no key/column/placeholder changes and every other row identical. Integrated native import completed. Settings/NoAds at RU320 Large Text and Trophies/Shop at RU390 with simulated safe insets were rendered: TMP overflow0; implementer and independent copy reviewer found no new regression. This does not claim every translated screen/device was rendered.
