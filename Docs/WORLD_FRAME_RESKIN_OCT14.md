# First-world frame reskins — 14 October

The other worlds must use the first world's composition and frame geometry. Themes change surface materials, corner details, backdrop and lighting. They do not introduce a new architectural border or a different clear area around the UI.

The runtime now uses these original silhouettes in every world:

- Map: `CascadeArt/WorldMenusOct14/MapFrameMask`.
- Menus: `CascadeArt/HomeOct13/FrameForeground`.
- Gameplay: `CascadeArt/GameplayReferenceOct14/GameplayFrame`, including its original mesh slicing ratios.

The shared frame shader retains the original architectural silhouette at its original UV. Seasonal colour work changes the original surfaces; compact corner accents use their own alpha inside the reserved corner patches so their edges remain whole. The menu shader removes retained pale sky pixels from the timber cutout. Gameplay and map cutouts keep their authored alpha. The generated tower-frame assets remain preserved in the project but are no longer used for world framing. Safe areas position controls; decorative menu frames reach the physical canvas edges.

`FrameReskin.cginc` supplies the common wet cedar, copper foliage, pale postal wood, glass-lagoon colours, moss, frost, warm beacon and night materials. Stone ledges and the pause floor use the same material colours. Corner foliage keeps subtle internal wind motion; frost remains still. World lighting, backdrops and existing ambient life remain active.

## Generated asset and prompt

Mode: built-in `image_gen`, transparent output. One new atlas, reused by the menu and gameplay shaders:

`CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/WorldFramesOct14/ReskinAccents.png`

Reference: the original `MapFrameMask.png`. The PNG is saved unchanged; the shader samples its eight 4×2 cells. It has no new frame architecture. Its alpha supplies complete edges for the modest corner details.

Final generation prompt:

> Use case: precise-object-edit. Asset type: a transparent 2D game decorative sprite atlas, 4 columns by 2 rows, eight square cells, same painterly polished 3D fairytale game style as the input. The input is a STYLE AND SCALE reference for the original game's modest corner foliage. Produce exactly eight isolated small decorative sprigs that will replace the existing corner sprigs without changing the frame's silhouette or architecture. Each sprig has the same curved diagonal footprint and the same modest amount of detail, with a transparent margin all around. No frame, no columns, no tower, no arch, no ground, no background, no text, no labels, no shadows outside the sprite. All eight sprites point along the same up-right diagonal, they are simply thematic skins of the same compact leaf-and-flower cluster. Grid reading order: 1 wet dark cedar leaves with a few clear raindrops; 2 warm copper autumn leaves with one tiny amber berry; 3 pale teal leaves with a small cream blossom for an airy postal world; 4 green leaves with two small pink rose blossoms for a glass lagoon; 5 mossy fern leaves with one soft violet flower for a forest archive; 6 the same cluster fully covered in soft white snow and blue frost, with three short naturally hanging icicles and absolutely no green exposed; 7 amber leaves and a tiny warm brass flower for a lighthouse at sunset; 8 midnight blue leaves with two tiny pale star-shaped flowers for a night observatory. Keep all eight small, equally sized and equally dense, clear negative space, identical cell dimensions. A clean transparent alpha background. Wide 2:1 canvas so all eight cells are square, exact 4x2 grid, no separators.

## Validation

`Cascade/Review/Map World Frames` checks all nine map and gameplay frames against the same source sprites, dimensions and mesh. It also checks all four bridge attachments, full canvas coverage, and the shared accent atlas. Fresh settled screenshots and runtime assertions are saved in `Reports/FrameReskinOct14`.

No economy, purchase IDs, gameplay rules, save schema or device build changed in this pass.
