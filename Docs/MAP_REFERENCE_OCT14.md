# Map reference — 14 October 2026

Rebuild of the campaign world overview from the supplied `853CB973` 720×1280 reference. The original progression rules, nine worlds, five islands per world, mode saves, level picker, pinned attempt and published level IDs stay in their existing owners.

## Visual contract

- The open upper frame, sky/castle horizon and perspective stone floor are a new backdrop. No baked loose leaves or interface elements.
- Five separate full-colour island sprites, desaturated only when locked. New clean lock badges, completed ribbons and progress plaques. The cottage has its own corrected broad silhouette; house/trees are never stretched.
- Recreated raised wood logos for EN/RU/UK. Existing cream leafy square skin, segmented board and shared green primary button are reused.
- 720×1280 composition uses one uniform scale inside the safe area. The frame extends separately across the viewport. Attached sprigs follow the actual canvas corners rather than the inset content; one uniformly scaled, cropped stone foreground continues under the primary button and to the bottom edge without a duplicated row seam. Ordinary ink on pale plaques preserves the reference’s progress typography. Actual rating totals including halves are shown over the offered island’s maximum of three stars per level.
- `28 / 36` and `17 / 36` are isolated review fixtures. With eighteen cleared levels the real next label is level 19; the reference’s example label 7 is not hardcoded.
- Home and settings are the two top controls as in the new design. Worlds still navigate by swipe/scroll and boundary-dependent small arrows. Islands still open the existing level list; Continue uses the original journey/resume action. The previous bag shortcut is reached through Home → My things in this layout.

## Motion and rendering

`MapReferenceAmbiance` uses a foreground decorative clock, capped at 50 ms per rendered update. Small phased vertical drift and less than one degree of sway affect each island and its matching plaque layer together. `MapRopeBridge` recomputes both attachments from those live transforms; plank and rope geometry share the same curve. Labels render above bridges.

The active island has a softly pulsing green silhouette contour, soft mesh light and sparkles. The existing silhouette shader rejects samples outside sprite bounds; the bridge/locked-island material supports uGUI stencil and clipping. Materials are owned and disposed with the screen. Branch sprigs and daisies move about attached pivots. The pooled leaf system supplies mostly blurred drifting leaves and two sharper depth layers, offscreen spawning, gravity, drag and occasional gusts. Existing equipped trophy ambience is above the new background and behind the controls.

App background, reduced motion and quality preferences are respected. These animations do not change the attempt, rating, inventory or save receipts. Gameplay is not resumed by decorative timing.

## Validation

Native Unity MCP captures: EN/RU/UK, large text, 720×1280, 320×693, 390×844 with simulated safe insets, and 768×1024 tablet. Screenshot-specific hit checks run while the camera is at that resolution; labels are checked for truncation/overflow and each bridge for attachment error. Settings/back, earned-island entry and locked-island rejection are invoked through their real UI handlers.

`MapScreenFlowTests` covers initial locks, real and half-star totals, completed ribbons, 360 attachment poses with unchanged attempt/receipts, foreground/background motion, preferences, settings/home navigation, and all nine worlds across three languages with large text. Existing `JourneyBrowseTests` checks the pinned middle descent, replay guards and per-mode saved progress. Its old five-star fixtures and no-argument reflection call to optional `PersistProgress(bool)` were updated to the current rating/save API; the assertions now inspect visible island stars including half units.

`Reports/MapReferenceOct14/` contains screenshots, XML results, `review.txt`, native 360-frame timing/attachment evidence and the 60 fps review clip. `validation-summary.json` records final measured results. No player/phone build was created. The recorded 60 fps decorative sequence verifies presentation cadence; it is not a claim about physical-device performance. Generated illustrations follow the reference composition; pixel identity with the source illustration is not claimed.

## Asset provenance

Built-in image generation was used. `AssetPrompts/MapReferenceOct14/sources.json` records original images and exact alpha-bound/cell trims; `prompts.md` records the generation requests. Deliberate transparent sheets were separated into clean individual sprites, preserving RGB/alpha without colour grading. No source screenshots or existing item/trophy icons were overwritten. Source `.meta` files and unrelated changes were preserved.
