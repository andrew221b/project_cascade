# World lighting and atmosphere — 7 October 2026

Presentation only: no puzzle rules, prices, schemas, packages or gameplay IDs change. One directional light per world with ambient sky fill replaces the second directional fill. Nine sun/moon profiles change source color, direction, intensity and shadow softness while preserving readable pieces. Illustrated sky layers retain their authored lighting; they are not dynamically relit surfaces.

## Motion and ambience

Eight wildlife species use two reusable camera-facing meshes, three authored wing poses, individual curved visits, scale changes for distance, gentle bank and long quiet intervals. Near/far visits stay in side lanes or the upper open sky. World's ecology pairs vary by climate. Effects Off hides optional wildlife and glints; Reduced Motion hides wildlife and meteors, keeping static stars. All movement uses the existing shared presentation clock and never modifies puzzle state.

Night gameplay adds sparse blue-white glints and rare tapered meteors; the map shares their deterministic timing, with light behind its island layer. Lunar mist stays cool. One original cricket phrase uses a cached source, 4.5 seconds initial quiet and 9–15 seconds between phrases; foreground cues duck it. SFX mute, focus/pause, owner disable, output reset and indoor/non-night contexts stop it.

## New art provenance

Generated with the built-in image generation tool; source RGBA files are copied unchanged to Resources/CascadeArt/Atmosphere/Birds.png and Insects.png. Their actual sprite regions/pivots are authored in WorldLifeView because generated spacing differs from equal rows. Alpha transparency is preserved, with a .025 render cutoff for faint edge specks. No background plate is drawn around wildlife.

Birds prompt:

Use case: stylized-concept. Asset type: game-ready transparent bird animation sprite atlas for CASCADE, an illustrated floating-island woodland puzzle game with honey oak, teal enamel, gentle sunny fantasy atmosphere. Create EXACTLY 3 equal columns by 4 equal rows, 12 separate centered birds, no grid drawn. Each ROW is one identical bird species shown in three animation poses, all side view flying right, body/head/tail same centre and scale: column 1 wings swept UP, column 2 wings spread horizontally, column 3 wings swept DOWN. Row 1 TOP: navy-and-cream swallow with forked tail. Row 2: warm russet robin with cream chest. Row 3: white-and-soft-slate gull. Row 4 BOTTOM: small ivory snow bunting with muted brown wings. Delicate painted storybook 3D volume, friendly natural proportions (not mascot face), clean recognizable silhouettes visible at small size, restrained highlights. Equal cells, each bird fits within central 64% width and 70% height of its cell, generous transparent margins all around, no parts crossing cells. Square canvas, perfect regular atlas alignment. Truly alpha-transparent everywhere outside birds. NO background, scenery, ground, shadow blob, glow, dust, words, outlines, labels, border or watermark.

Insects prompt:

Use case: stylized-concept. Asset type: game-ready transparent insect animation sprite atlas for CASCADE, an illustrated floating-island woodland puzzle game, warm polished storybook materials and teal/cream/gold palette. Create EXACTLY 3 equal columns by 4 equal rows, 12 separate centered flying insects, no grid drawn. Each ROW is one identical species in three clear animation poses with fixed body centre/scale and head pointing right, slight three-quarter view: column 1 wings swept UP, column 2 wings spread/level, column 3 wings swept DOWN. Row 1 TOP: slender turquoise dragonfly with four translucent warm ivory wings. Row 2: soft amber and cream butterfly. Row 3: tiny jewel-teal lantern beetle with gold abdomen, light ivory flying wings (no surrounding glow). Row 4 BOTTOM: pale mint luna moth with curved wing tails and subtle blue-violet markings. Delicate hand-painted 3D storybook volume, natural anatomy with readable small silhouettes, restrained highlights. Equal cells, each insect fits central 64% width and 70% height, generous transparent gutters, every wing completely inside own cell. Square canvas and perfectly regular alignment. Genuine alpha transparency outside silhouettes. NO background, leaves, flowers, shadow blob, separate halo, dirt, sparkles, words, labels, border, grid or watermark.

Audio is synthesized by Tools/build_night_ambience.py; no borrowed recording or melody.

## Validation

Latest source compiles against the Unity6000.3.8f1 engine with zero errors. Eight daytime worlds were captured at two poses and visually reviewed. Review caught a skiff crossing the puzzle row and a first-meteor property-block error; both were corrected. Final native night/map/skiff review and the scoped cricket lifecycle test remain pending: the Mac locked before restart. See Reports/WorldAtmosphere-Oct07/validation.txt. No phone build requested. Image captures/engine checks cannot establish device audio/haptic feel or human enjoyment.


Final native validation — Mac unlocked,7 October:
9 worlds at two poses plus night meteor/reduced mode and390×844/320×693 night map. Reviewed native night/map/skiff frames; capture changes world UI contrast using the existing RenderFrame path. Checks pass for same-phase stability, one directional source, meteor, Reduced Motion, effects off and unchanged puzzle hash. Scoped cricket lifecycle PlayMode test1/1 PASS. Evidence: Reports/WorldAtmosphere-Oct07. No phone build/install, live speaker listening or physical-device performance validation.
