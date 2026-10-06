# Shop, trophies and field tools — 6 October 2026

The former three-thumbnail shop with a duplicate floating object is replaced by
one shared catalogue surface: 3+2 tool shelves and 4+4 trophy shelves, followed
by the selected item’s benefit, availability, assistance rule and action.
Timber, brass, ivory, teal and existing live lettering are preserved. Video is
secondary; the selected-item action carries amber emphasis. Test shop stays
enabled without prices as requested; preview inventory is temporary.

Five tools include the new Signal lantern (one solver-proved non-drop command)
and Landing lens (immutable exact next-entry preview). Used assistance permits
up to two stars and no clean record. Existing caps and transaction rollback
remain. Eight cosmetic trophies can accompany Mika on the map; original four
parcel recipients and twelve-fragment cap remain. UI uses Trophies / Трофеи /
Трофеї while legacy storage keys retain their identity.

The gameplay bag button is shown only with tool stock. It opens selection
directly between Undo and Descend. Both new tools have first-use explanations.
The preview pauses input and ambient motion; closing never applies the drop.
Lantern commits through the normal resolver before presenting that transition.

## Evidence

- `editmode.xml`: final focused 46/46 PASS across Shop, Attempt, mechanic lessons,
  localization, preview monetization and save tests. Repeated runs overlap;
  this is not a full-suite result.
- `runtime.txt`: actual Editor UI callbacks verify five preview tools, eight
  trophies, beacon equipment on Mika, field access and empty-stock hiding,
  first-use popups, one proved lantern move and finished presentation, exact
  lens prediction, pause/close without state mutation and restored use flags.
- `replay-count.txt`: 365 committed prefixes plus 157 initial states verified.
- `table-completeness.txt`: 1,998 non-empty entries, matching EN/RU/UK arguments.
- Seven final native captures opened after the correction batch:
  `tools-ru`, `tools-uk-small`, `trophies-ru`, `collection-ru`, `field-tools-ru`,
  `bag-ru`, `landing-uk-small`. Paired JSON records target bounds and state.
  Small view uses 320×693, Large Text and simulated safe insets. Its video,
  tabs, purchase, bag-link and lens-close targets are 48 pixels high.
- Final runtime Console: zero messages. Play Mode stopped; isolated QA profile
  detached. Field captures use an isolated synthetic `field-review` fixture,
  not campaign progression or a production player save.

Initial UI probes were corrected for popup readiness and a fresh isolated
profile: restoring a synthetic fixture through the published campaign catalogue
correctly blocks a save. Final probes wait for first-use readiness and do not
reuse that fixture as a campaign save. This is not a device performance result.

## Independent finish review

Initial review requested six corrections. One correction batch and final seven
recaptures resolved all six: truthful intermediate landing copy, visible
assistance consequences, minimum touch targets, quieter video hierarchy, bag
stock summary and current PRODUCT.md. Final disposition: **ship for those six
scored corrections only**; no material regression observed. This code-led
revision did not produce a formal image comp/FORM seed; fidelity was evaluated
against the actual user brief and existing material world.

No new player build or phone installation. Full suite, live App Store/Google
Play transactions, production ad service, physical touch/safe areas, performance,
editorial native-language review and human enjoyment remain unverified.

Design/rule/catalogue note: `Docs/SHOP_TROPHIES_AND_FIELD_TOOLS.md`.
Asset prompts/provenance: `Docs/AssetPrompts/TravelTreasures.md`.
Six new transparent props: `TravelTreasures.png`; final prompt also embedded in
PNG metadata. Existing assets and `.meta` identities retained.
The existing stale Impeccable sidecar and unset buildPath were left unchanged.
