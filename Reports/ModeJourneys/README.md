# Parallel journeys and celebration paper

7 October 2026. Native Unity6000.3.8f1 Editor inspection through Unity MCP; isolated temporary profiles. No phone build/install.

## Validation

- EditMode30/30 PASS,2.2541812s, editmode.xml: ModeJourneyTests, selected migration/checksum/backup/future-version/first-clear SaveTests, RewardTests, ShopTests. No full solver or full test suite rerun.
- runtime.txt: actual three map-tab clicks; independent Words/Mixed; Shapes mid-level stage1 and exact row/time restored; disk reload; no duplicate Home selector.24 separate paper bodies using six sprites, every body moves, finite fade, reduced-motion suppresses objects.
- final-runtime.txt: actual five-star reward and trophy opening, then map; reward ceremony preserves state and account resources; independent mode round trip and disk reload.
- final-spacing.txt: final source RU/LargeText/simulated safe insets320×693; exact second-stage round trip. Latest map-small-final.json gives all three tabs52px physical height,88.77px width. Early small-map captures preceded the dynamic height/gap fix; they were superseded and omitted.
- Native PNGs opened visually: Home, numbered levels, Word map, final small map, actual reward and trophy screens. Reward image is captured during score counting (final receipt2000), not the settled score. Motion probes used an isolated particle fixture on a map, not a product celebration on that screen; those disposable images were omitted. The retained Words-tab image shows its state before the final counter-gap refinement.
- Final successful compilation and Console0 errors/warnings. A draft test initially referenced nonexistent PuzzleCommand.Parse and was corrected before tests. First animation probe outlived its particles while camera capture waited; the corrected probe checks movement before rendering and freezes focus for repeatable phase captures. These were not counted as successful probes.

Mode migration leaves root journey in its saved PreferredMode; inactive journeys never contain the active mode. Old attempts stay pinned, including legacy mixed puzzles saved within the former automatic story route. Separate mastery records are migrated only into that old journey. Shared account reward ledger validates against the union of all mode clears. No duplicate coin/light/parcel award for the same node, no changed shop prices or product IDs.

New asset: `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Workshop/CelebrationPaper.png`,1536×1024RGBA, true alpha0–254, six512×512 cells. Built-in imagegen; exact prompt, reference, preserved original and final path: [CelebrationPaper.md](../../Docs/AssetPrompts/CelebrationPaper.md). Non-power-of-two scaling disabled; Clamp/Bilinear/no mipmaps/alphaIsTransparency; no whole-atlas Image.24 small UIImage silhouettes are integrated with cosmetic gravity/air drag/wind/tumble, no gameplay physics or reward callbacks.

The final compatibility helper also imports published legacy durability content when restoring an inactive slot after a restart; compilation checked, that legacy-only reload branch was not separately exercised in Editor.

Full suite, physical Android/iPhone safe-area behavior, real phone sensor, device FPS and human enjoyment NOT RUN. Native captures are Editor evidence, not physical device screenshots.
