# Restrained menu composition — October 14

The UI now uses a shared family of leaf-free controls: green/amber/red/disabled actions, square navigation controls, selected/ordinary item cards, reward cards, coin cards, inventory panels, tabs and purchased ribbons. Existing callbacks and hit rectangles remain unchanged. Rounded borders scale with control height; center sections stretch for localized labels. Icons and hero art are retained.

Composition rule: the world map has five route islands and no additional decorative islands. Home, inventory, item/trophy/shop screens and parcels have at most two authored background islands; pause has none. Gameplay shows two smaller islands from the current world's six-asset set. Background aircraft and natural falling leaves are capped at three (two on reduced quality), with fewer/lighter pause leaves. Existing trophy effects remain separate from ordinary menu ambience. Reduced motion/background suspension behavior is preserved.

The dense first-world background had another cause: the sky replacement shader skipped world zero. It displayed baked first-world scenery instead of the world sky. Sky replacement and foreground masks now also apply to the first world. Its new clear azure sky retains the saturated cloud palette without embedded islands. The other eight skies, world lighting, and unique island sets remain in use.

The request for themed names is implemented as the nine world-map headings, using existing EN/RU/UK world names. A clarification was sent asking whether individual puzzle names were intended; no answer arrived during implementation. World plaques use a small daisy/wind motif, mill cog/drop, copper apple, postal seal, glass crystals, archive book/key, snow crystals, beacon lantern/compass, or observatory stars. The first-world sprig moves on the existing map clock. Existing edge flora stays part of the environment; new buttons/cards contain no foliage.

Assets: 12 clean controls, 9 world plaques and 1 sky, generated with the built-in image tool. Original alpha and complete object boundaries are retained; separate sampler gutters prevent adjacent atlas fragments. New sibling assets are under `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/CleanMenuOct14/`. Full prompts/provenance: `AssetPrompts/CleanMenuOct14/`.

Verification:

- 43 PlayMode tests covering map, pause, items, parcels, inventory/trophy/shop navigation and world lifecycle passed after the border/first-world shader fixes.
- Seven world lifecycle tests passed again after the final clear-sky import.
- 76 native Unity captures: map/home/shop/inventory/items/pause/gameplay across all nine worlds, additional trophy/coin/parcel views, EN/RU/UK large-text/safe-inset views. No reported overflowing localized reference labels or detached bridges. Actual images were inspected, including long observatory headings and the corrected pause title.
- A 360-frame 60 fps map clip was encoded and decoded for verification; adjacent frames were compared for continuous motion. Bridge attachment error is zero and animation/capture leaves the puzzle hash unchanged.
- This verifies Editor output, not physical-device performance. No phone/player build, economy/rule change, package change or deployment.

Evidence: `Reports/CleanCompositionOct14/`, final lifecycle results in `Reports/CleanCompositionFinalOct14/`.
