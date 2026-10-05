# P12 asset provenance

- `CascadeArt/SkyWorkshop.png`: original generated background for this game;
  exact prompt in `P12_BACKGROUND_PROMPT.txt` and the PNG metadata. No ShapeMania
  image was supplied to generation or copied into the game.
- `CascadeArt/Meshes/`: original beveled contours and extrusions authored in
  `ToyAssetBuilder`; six silhouettes, letter tile, rail, disk and quad.
- `CascadeArt/Materials/`: original shared URP plastic/enamel/metal palette;
  `SoftContact.shader` provides a small analytic contact shadow.
- `CascadeArt/Panel.png`: original deterministic rounded rectangle for nine-slice
  UI; generation method embedded in PNG metadata. This is geometric UI, not
  an illustration substituted for authored scenery.
- `Art/Fonts/`: Balsamiq Sans Regular/Bold independently sourced from the
  official Google Fonts repository; SIL OFL license and source URL included.
  TMP static atlases are generated derivatives. Full language QA is P15.
- World device bodies are original assemblies of those shared meshes. They
  read the committed state; there are no gameplay colliders or rigidbodies.

The ShapeMania references live outside this package, read-only:
`/Users/andrew/Desktop/unity projects/unity_projects/shape_sorting-final/Tools/Tripo/Review/`.
Their screenshots informed material/lettering expectations; they are not game
assets. P12 raster provenance scan: two shipped rasters, none missing metadata.

Procedural mesh rebuilding preserves existing asset GUIDs. Existing project
content, package pins, schemas, prices and gameplay IDs are unchanged.
