# CASCADE navigation finish review — 6 October 2026

## Disposition

**FIX.** The revised navigation and illustrated opening satisfy the direction at this captured scope, but the map's background extension and conversation touch targets need correction. No rebuild of the direction is warranted. All required evidence is present and usable; this is not a recapture-only verdict. Confirm the two corrections with fresh portrait and wide map captures before marking this review shipped.

## Scoped coverage

Authority: `Docs/NAVIGATION_REWORK.md`, the current product platform/brand commitments in `PRODUCT.md`, and `DESIGN.md`. This is a code-led extension of the Sky Wind Workshop with no approved composite. The rejected legacy screenshot was inspected as the flow to replace, not a fidelity target.

Individually inspected all 14 required native Unity camera renders:

| Captures | Observed coverage |
| --- | --- |
| `home-en.png`, `home-ru-small.png` | Four clear destinations only: Start game, Shop, Settings, Remove ads. Illustrated workshop remains visible; EN and compact RU captions fit. |
| `modes-uk.png`, `levels-ru-small.png` | Separate Story/Shapes/Words/Mixed choice; semantic painted icons; numbered level shelf with swipe instruction and page count, without Earlier/Later. |
| `opening-1.png`, `opening-2.png`, `opening-3.png` | Grandma and the Great Beacon, the destructive storm and saved light, then Mika's promise to repair bridges and relight the beacon. Three distinct illustrations make the goal understandable. |
| `map-ru.png`, `map-wide.png` | Goal and next task are explicit; smaller Mika's feet meet the current colored island; future islands are visibly unavailable. Background defect described below. |
| `shop-ru.png`, `keepsakes-ru.png`, `settings-ru-small.png`, `no-ads-uk.png`, `dialogue-ru.png` | Earned collection and workshop routes, identifiable selections/switches, readable compact settings, honest ads-already-off copy, and a clear character conversation. |

Source sample: `CascadeGame.Navigation.cs`, `CascadeGame.Journey.cs`, `MapParallax.cs`, relevant menu color declarations, and `NavigationCapture.cs`. Story binding selection is automatic after the first three shape lessons in the sampled route code. Other regions, conversations, progress states and the complete application were not visually covered by this matrix.

## Material findings

1. **F1 — P1: The map sky becomes stretched bands outside the central image.** In `map-wide.png`, long horizontal cream/blue stripes occupy both sides of the route and end abruptly at the portrait composition. `map-ru.png` also has vertical streaking above the heading and below the task panel. This breaks the requested complete illustrated background and makes the map look like an incomplete render. Replace the edge-sample extension with a continuous authored sky composition that survives viewport expansion and maximum parallax offset; retain the existing islands and character. Confirm both map sizes and a shifted pose.

2. **F2 — P2: Mika's required conversation shortcut has a narrow hit area.** `map-ru.json` measures the `Mika on the path` Button at **37.6 × 65.6 rendered pixels**. Source attaches that Button directly to the small portrait rect, without an expanded hit area. The broad `Story` action is also **46.6 pixels high**, below the project's 48-pixel compact reference baseline. Keep Mika's visible size, but provide an independent invisible target at least 48 × 48 at the smallest supported portrait size; give the broad conversation action at least 48 pixels of height. Confirm that taps around Mika open conversation rather than merely selecting the underlying island. These Editor pixel measurements are not a hardware pt/dp certification.

## Craft-floor assessment

The game has a coherent authored identity: honey timber, brass, teal/amber enamel, illustrated action icons, original story pictures and Balsamiq lettering. The custom controls are appropriate to the supplied game world; replacing them with stock OS productivity navigation would contradict the brief. The picture/buttons composition removes the rejected full-page menu sheet and gives the primary action a clear hierarchy.

Copy fits the inspected EN/RU/UK poses without clipping or overlap. Opaque ivory reading surfaces and deep teal ink provide strong contrast; the declared menu ink/paper pair is approximately 14.6:1. Cream lettering on teal enamel and dark lettering on amber remain readable in the inspected renders, but no exhaustive rendered-pixel contrast audit was performed. Buttons have painted offset/soft depth rather than arbitrary halos. Main actions, level numbers and compact settings targets meet the 48-pixel reference baseline in their metadata; F2 is the measured exception.

The selected settings tab, switches, selected keepsake and locked islands communicate state. Loading/error/focus behavior is not established by these stills. The sampled map motion uses one shared transform for scenery, Mika and island targets, with bounded tilt and a zero-offset Reduced Motion path. Separate cloud movement supplies depth. Stills and invariance tests do not establish animation smoothness or physical gravity-sensor quality. Browser-specific craft checks and the HTML detector are inapplicable to this native Unity surface.

## Evidence and limits

All 14 PNGs are complete, plausible native camera frames with matching dimensions/JSON. `capture-complete.txt` and the capture source identify Unity Editor evidence, an isolated profile, simulated portrait safe insets, 320 × 693 / 390 × 844 portrait references and a 1440 × 900 wide map. Only compact home explicitly enables the game's Large Text option in this matrix. JSON `minimumDeviceTextPixels` is zero and is not usable evidence of a minimum font-size measurement.

Verified report contents: `playmode.xml` initially passed **5/6**; the corrected swipe case passed **1/1** in `Reports/NavigationSwipeFix/playmode.xml`. This supplies passing evidence for **six distinct scoped cases**, not a later complete six-case rerun or a full PlayMode suite. `editmode.xml` passed **2/2** localization checks. `table-completeness.txt` reports 1530 non-empty EN/RU/UK entries with matching format arguments. The implementer reports exact embedded prompts and a clean 2/2 scan for the two new rasters; this reviewer inspected their rendered use, not their full provenance audit.

No tests, builds or new captures were run by this reviewer. Current full-suite regression, mobile/device build, physical touch/haptics/gyro, OS insets/back behavior, performance, screen-reader accessibility, native-language editorial approval and human comprehension/enjoyment remain unverified. Editor captures support this scoped visual finding; they do not certify release readiness.
