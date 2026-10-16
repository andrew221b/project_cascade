# Thematic world environments — Oct 14

Menus now follow the actual active/progression world instead of resetting to Wind Gardens. Browsing the map uses its selected world; pause and results retain the world of the actual level. The existing nine world skies and lighting profiles drive scenery in the map, home, shop, inventory, items, trophies, pause and legacy menus. Workshop alone retains its authored interior. Existing button/card/label/icon geometry, costs, progression, saves and rewards remain unchanged.

The rain sky is now overcast with silver mist and warm mill windows. Copper Orchard foliage warms toward copper; Snow Spire islands receive frost; night menus reuse the existing animated observatory islands. Foreground illustrations receive bounded world light with warm lantern/window highlights preserved. Alpha remains opaque on Mika; proportions remain unchanged. Bare night headings stay light after typography.

Original sky had been baked into opaque menu backgrounds. Generated alpha-only mattes retain ORIGINAL frame/floor RGB and their positions while replacing the entire opening with the current-world sky. This prevents blue strips/cloud remnants and retains the pause stone platform. A very slow bounded sky drift is continuous across the scene; islands/bridges, foliage, daisies, trophy ambience and existing bird/insect behavior remain active.

Six reusable aircraft sprites travel along curved drifting paths with offscreen entry/fade. A small smoke mesh rises from the unlocked map cottage. Rain, snow, archive fireflies, postal flutter and night glints provide world-specific motion. Four additional craft are available in gameplay only inside its computed empty sky band, fade while holding a token, and hide when no space remains. Reduced motion/effects-off stop optional animation; background suspension freezes clocks. Pools and transient materials/sprites are disposed with their owner; visual code does not alter attempts or grants.

## Validation

- 5 EditMode lighting tests and 15 PlayMode environment/map/navigation tests passed. A diagnostic initially assumed every level had an open flight band; the dense level correctly had `(3.71, 3.67)` and hid craft. The corrected test verifies this condition plus bounded craft in a supplied open band.
- Native Unity camera screenshots cover home/map/shop for all nine worlds, other night menus, real playing/pause transitions, and EN/RU/UK large text with safe insets at 390×844. Interactive hit checks run inside the active capture resolution; no reference labels overflow.
- Nine-second native motion sequence: rainy map, snowy home, night home, 540 frames at 1/60-second increments. Bridge attachment error is zero; puzzle state is unchanged. Encoded video is decoded again to verify dimensions/orientation. This checks Editor animation cadence, not physical-device frame rate.
- Original scene/profile override restored after isolated review. No player/phone build, external deployment, package or account changes.

Evidence: `Reports/WorldMenusOct14/`; provenance: `AssetPrompts/WorldMenusOct14/`.
