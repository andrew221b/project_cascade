# Unique authored world islands — Oct 14

The previous implementation reused Wind Garden cutouts for map landmarks and menu scenery while swapping only the sky/light. This is replaced by **54 original complete island sprites, six per world**. Wind Garden, Rain Mill, Copper Orchard, Cloud Post, Glass Lagoon, Moss Archive, Snow Spire, Great Beacon and Star Observatory each have their own architecture, cliffs, plants, materials and lighting.

The same world-specific set supplies all five map landmarks, home/inventory/parcel scenery, background islands in other reference menus, and six camera-space gameplay islands. No runtime first-world fallback, quarter-atlas cropping, or night-only atlas substitution remains. Home/inventory construction directly loads the authored world sprite before the first frame. Missing assets fail explicitly.

Source sheets were generated as independent complete objects, with spacing-only edits where necessary. Empty alpha gutters were verified before slicing; original RGBA is retained, with transparent texture-sampling padding. No objects are extracted from screenshots. Uncompressed sRGB textures use trilinear mip sampling, clamp wrapping and FullRect sprite geometry. The 54 source files are different, and every source belongs to its declared world. Provenance and full prompts: `AssetPrompts/WorldIslandsOct14/`.

Authored lighting is preserved; distant islands blend gently toward their actual sky instead of becoming semitransparent or receiving an unrelated recolor. Aspect ratios remain natural. Existing map positions, labels, locks, buttons, saved progression and rewards remain unchanged. Bridge points account for fitted artwork and enter terrace paths; ropes follow both animated endpoints. Chimney smoke is attached to the relevant authored house. Optional scenery uses existing presentation clocks and respects reduced motion/background suspension.

During native gameplay review, inline digit tags appeared after repeated scene reloads. The local TMP implementation confirmed its registry retains a sprite asset by name hash and does not replace an existing entry. Recreated runtime digit assets had reused the old name. Names now include the asset instance ID; rendered glyph art and layout are unchanged. A repeated-scene regression test checks actual resolved sprite counts.

## Verification

- 7 EditMode tests: 54 unique assets, fitted bridge points, existing world lighting.
- 17 PlayMode tests: all nine worlds across home/map/shop/inventory/items/trophies/pause/coins and gameplay, existing map/navigation lifecycle, and repeated-scene digit resolution.
- Native Unity captures include home/map/shop/gameplay for all nine worlds, additional night menus, EN/RU/UK large text with safe insets, and a 360-frame lagoon map animation at 1/60-second steps. Correct world sprites, natural proportions, no overflowing reference labels, zero detached-bridge error, puzzle hash unchanged by capture/motion.
- The encoded clip is decoded again to verify its dimensions and orientation; adjacent frames are compared for continuous motion. This verifies Editor output, not physical device performance.
- Isolated profile cleaned up, original scene restored, no Console errors or compile errors, no player/phone build or cloud/package changes. The capture fixture resets synthetic reward history when moving backwards between worlds, preserving valid receipts without changing the player profile.

Evidence: `Reports/UniqueWorldIslandsOct14/`.
