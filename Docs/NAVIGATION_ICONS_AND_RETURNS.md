# Menu icons and navigation — 7 October 2026

Refinement of the existing illustrated workshop, enamel controls and three parallel journeys. ShapeMania's MainMenuUIController/MenuTabBar were inspected read-only as a navigation reference: distinct destinations, a clear play action and deliberate return to the parent. No ShapeMania asset or implementation was copied.

## Icon correction

MenuIcons source is 1536×1024. Unity imported it as 2048×1024 under NPOT ToNearest, stretching width by one third. Preserve original aspect with NPOT None for MenuIcons, OpeningComic and TravelTreasures. Existing pixels and GUIDs remain. Mika already used None and was left unchanged.

MenuIcons now uses authored opaque silhouette bounds with four source pixels of padding. Each illustration is centred vertically in a 44-pixel square and rendered as a Simple Image with preserveAspect. Icons stay in their left illustration column beside the caption. Inventory rows use the same centre/size convention. Square regions refresh with Canvas scaling; map inventory has a 48-pixel minimum target. No generated replacement artwork is needed.

## Destination and return rules

- Home keeps Start game / Shop / Settings / Remove ads. Start opens the introductory story once, then the map; the map alone selects Shapes / Words / Mixed.
- Map has an illustrated bag at top right. It opens My things, containing Tools, Trophies, Journal and Workshop. Back returns to the same map selection. Practice has been removed.
- Shop opened from Home leads to My things. My things restores the original shop tab, item and shop parent after nested tools/shop visits.
- A shop opened from game tools returns to those tools; a shop opened from the trophy collection returns to the collection. Neither creates another root inventory entry.
- Journal leads to Requests, Trophies and My routes. Requests/trails return to Requests, records return to Journal, trophy collection returns to its caller. Journal retains the existing three-light unlock.
- Settings retains its caller even if shown twice. Existing mode saves, current descent stage, puzzle state, prices and rules are untouched.

First-use teaching stays inside the current level. Old training definitions and localization IDs are retained for save compatibility and developer fixtures; they have no player entry.

Disabled shop actions retain the same muted enamel frame instead of switching to a flat cream panel.

See Reports/MenuNavigation/README.md for validation and limits.
