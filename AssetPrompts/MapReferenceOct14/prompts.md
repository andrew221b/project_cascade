# Map reference prompts — built-in image_gen

Reference: user’s map screenshot `853CB973-6A4E-4EBC-BE60-1DD049F972B0/1-Photo-1.jpg`. Final source paths and trims are in `sources.json`.

1. **Backdrop / precise-object-edit:** preserve exact saturated cyan sky, clouds, blurred floating castle islands, open foliage/rope upper corners and golden perspective stone floor. Remove five main islands, bridges, all UI/logo/badges and every loose sky leaf. Reconstruct uninterrupted sky. No top horizontal timber.
2. **Islands / background-extraction:** transparent spaced 3×2 sheet, five full-colour three-quarter fantasy islands: terracotta cottage, garden pool, stone arch, domed tower, roofed chapel. Match architecture, lush greens, cream stone, gold fences and taper cliffs. No bridges/UI/shadows. Equal cells with gutters.
3. **Localized logos / logo-brand:** transparent sheet with three stacked raised curved wood/leaf/daisy logos. First row “Летающие / ЛЕСТНИЦЫ”; second “Flying / LADDERS”; third “Літаючі / ДРАБИНИ”. Gold orange upper lettering, ivory lower lettering and warm brown extrusion. Exact spelling and clean margins.
4. **Details / background-extraction:** transparent 3×2 sheet of blank cream/gold rounded progress plaque, blank emerald completed ribbon, pale rounded grey padlock badge, attached leafy branch sprig, white/gold daisy cluster, golden wood stair plank. No text and no backdrops. Spaced equal cells.
5. **Cottage correction / background-extraction:** one wider cottage garden island matching the reference’s broad asymmetrical floating land, terracotta roof and chimney, trees, gold fences and lanterns, stone path and right gate, white daisies. Clean transparency, no bridge/UI; no stretched architecture.
6. **Final cottage / precise-object-edit:** preserve the cottage correction’s garden, architecture, colour and scale. Extend only the rocky underside to a tapered natural cliff to make the opaque silhouette about 1.13:1 in width/height. Do not stretch the house or trees.

Transparent cell separation/alpha-bound trims were used only to import deliberately generated sheets; original colours, antialiasing and alpha were preserved. Unity uncompressed sRGB sprites, bilinear clamp, uniform artwork aspect. Only locked island rendering is grey; no tint washes the active island.

7. **Deep ground / stylized-concept:** landscape 3:2 golden honey sandstone terrace, narrow far cap at the top, continuous detailed paving toward viewer, small stones at far edge and larger near tiles. Match reference warmth and cracks. No sky, plants, daisies, wood, objects or interface. A uniformly scaled crop exposes more true perspective on taller screens; never stretch stone rows.
