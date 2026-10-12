# Menu navigation review — 2026-10-07

Before: before-home.png / before-bag.png. Native import inspection found MenuIcons 2048×1024 versus original 1536×1024, NPOT ToNearest and preserveAspect=true: distortion occurred before UI layout. Final runtime atlas is 1536×1024.

Final native Unity Editor captures inspected: home-ru-390.png, home-uk-320.png, bag-ru-390.png, shop-ru-320.png, trophies-ru-320.png, map-ru-320.png, languages-ru-320.png and journal-ru-320.png. Safe insets simulated at 1.8% left/right, 4% bottom and 6.6% top. The no-progress inventory/collection states and long translated labels are represented. Artwork aspect and icon vertical centres checked at runtime: 44×44-pixel Simple/preserveAspect regions. Narrow map bag target readback 47.99998×48 pixels.

Native button traversal passed: Home→Shop→Things→Trophies→Things→Tools→Shop→Tools→Things→Practice→Things→Shop→Home; Map→Things→Map→Home; Settings→Languages→Home. Canonical puzzle hash unchanged. The initial traversal exposed a nested shop parent overwrite; retaining the parent shop context fixed it before the final traversal.

Focused PlayMode regression HomePauseAndSettingsPreserveAttemptAndFreezeCoaching: 1/1 PASS, 4.7577023 seconds, playmode.xml. Expanded to cover repeated Settings, collection, nested tools/shop, practice, map/bag, journal/records and requests/trails returns; the original pinned attempt and coaching checks remain. Test ran after navigation fixes, before the final square-region resize adjustment. Final resize adjustment was validated by compilation, native geometry checks and captures rather than repeating the same navigation test.

Project compilation succeeded; final Console zero errors/warnings. Editor stopped and isolated review profile override cleared. Player saves, user Cloud edit, gameplay rules and source raster pixels preserved.

Full suite, OS accessibility, physical-device safe areas/touch, performance, player builds and phone installation NOT RUN. Older broad navigation tests contain legacy flow assumptions; this focused report does not certify the full suite or human enjoyment.
