# Menu clarity — 7 October 2026

Design decision before implementation: retain five balanced consumable tools and add a sixth, permanent Field guide object to the Tools catalogue and bag. It opens the existing animated rules, is always available, costs nothing, has no stock, grants no moves and does not affect stars or records. No price, save schema, product ID or solver rule changes. The catalogue is a complete three-column, two-row shelf.

Expand the portrait workshop framing so the existing safe-area UI fits within its sky opening. Keep map and gameplay framing unchanged. Standardize framed back and paging controls; remove duplicate exit actions on catalogue and no-ad pages. Put device paging beside its count, not beside the screen exit. Preserve readable physical touch sizes.

Implemented changes:
- Portrait and landscape workshop framing keep the safe-area menu over the sky opening. Existing artwork is reused; map and gameplay composition stay unchanged.
- Framed 48 px header back buttons; themed page arrows. Device arrows sit beside the page count. Bottom Basics / Back buttons have identical widths. No duplicate exit in Supplies, Collection, No ads or Coin packs.
- Device practice CTA says Try it instead of exposing internal C/B IDs. Locked campaign examples explain their unlock level. Wind guide has one return action.
- Six catalogue cells: five consumables plus the free Field guide with the existing illustrated book (Navigation/MenuIcons.png, slot 4; no generated replacement). The guide also appears in the tool bag but does not make an empty field bag button appear. Its purchase entry point is guarded.
- Record identity wraps within its own reading region; basic instructions have breathing room above actions. Shorter bomb explanation in RU/UK. New copy is localized EN/RU/UK. Existing Balsamiq font assets include the required Ukrainian apostrophe glyph.

Validation: initial native Unity batch covered all 30 GameScreen routes in 56 states / locale-size variants, with zero TMP overflow or split-word findings and stable puzzle identities. Visual inspection found landscape framing, internal example IDs and the small-screen basics gap, fixed together. Final targeted confirmation covers 13 affected views plus actual manual navigation (six cells, next/previous, one Back exit, return to shop, wallet/stock/puzzle unchanged). The first navigation-check attempt used spaced names for buttons that strip spaces; corrected the QA selector before running the flow again. This was a check-harness failure, not a player navigation failure.

Native Editor camera captures with simulated safe insets: 390×844, 320×693 (large text), 1440×900. No new phone build, device installation, physical-device gesture/accessibility validation, performance benchmark, store purchase or broad gameplay test suite. Existing Music / FX controls and audio implementation unchanged by this layout task.

Final verdict: PASS for the reviewed menu layouts and exercised guide flow. Final 13 native images retained; the 56 intermediate images were inspected then removed to avoid duplication, keeping their measurements. Two filtered catalogue poses initially inherited the trophy tab; recaptured the correct tools category explicitly, confirmed six objects and no overflow, and made future QA poses independent. Final text measurement findings: zero. No claim that automated checks certify every localization/state or human enjoyment.
