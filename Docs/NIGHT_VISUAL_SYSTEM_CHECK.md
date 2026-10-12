# Night extension visual system check — 7 October 2026

Documenter checkpoint for the ordinary Star Observatory extension. The ninth
world follows the Great Beacon, with resident owl Aster and three introductory
comic panels per world. The incumbent **Sky Wind Workshop** remains the visual
authority. This pass preserves `PRODUCT.md`, `DESIGN.md` and
`.impeccable/design.json`; it introduces no shared tokens or brand direction.
The scope is defined by [Night world and journey flow](NIGHT_WORLD_AND_STORY.md)
and [Star Observatory story](STAR_OBSERVATORY_STORY.md).

## System fit

| Incumbent evidence | Observatory extension |
| --- | --- |
| Painted honey wood, hemp suspension, Hardware Gold, ivory receiving inset and six stable SymbolId colors in `DESIGN.md` | Native night gameplay retains the same glossy bodies, timber rig, ropes, device socket and complete receiver. Cool lighting changes their setting without changing their symbol materials. |
| Authored teal/amber enamel actions, ivory captions and live Balsamiq Sans | Map tabs, travel, conversation and comic actions reuse those controls and type. Night map copy uses the incumbent `menu-paper` color rather than a new global text token. |
| Painted floating islands, brass/stone construction and warm character materials | `StarObservatoryMap`, `StarObservatoryBase` and the four-cell `ObservatoryIslands` atlas add navy sky, turquoise roofs, brass telescopes and compact amber lamps. Those raster colors are local art evidence, not measured shared color tokens. |
| Original Mika, separate live localized story text and restrained ambient motion | Three 3×3 WorldComic atlases supply 27 scene cells. `CascadeGame.WorldComics.cs` selects three cells per world and draws live captions, exchanges and a 1–3 count. Aster's cream face, blue-grey wings, brass spectacles and teal star vest agree between the transparent portrait and observatory comic row. |

## Local night inputs

The following are scoped source inputs, not additions to the normative
frontmatter. Rendered color also depends on Unity lighting and color space.

| Source | Zone 8 evidence |
| --- | --- |
| `LayeredWorld.SetZone` | Sky top RGB(.045,.105,.23); bottom RGB(.09,.20,.34). Four separate observatory islands float in gameplay; the daylight glider and near plants are suppressed there. |
| `ToyMachineView.SetZone` | Warm key RGB(1,.87,.66), intensity 1.08; cool fill RGB(.42,.65,1), intensity .64; ambient RGB(.38,.48,.65). Existing worlds retain their previous daylight branch. |
| `NightSkyGraphic` | Sixteen sparse glints use RGB(.87,.95,1) with alpha .24–.54, read the presentation phase and receive no input. Reduced Motion fixes the phase; effects-off omits them. |
| `CascadeGame.Journey` / `CascadeGame.Flow.FinishMenuContrast` | Zone 8 uses its night sky plate. Nonbutton map copy finishes in `MenuPaper` RGB(.99,.98,.92); teal buttons retain ivory lettering and amber actions retain dark ink. `ApplyPreferences` reapplies that finish after text preferences. |

## Evidence and drift

Inspected the new base/map/island rasters, Aster portrait, WorldComic2 and native
night gameplay, dialogue, small map and small comic captures in
`Reports/NightJourney`. The refreshed small map has readable ivory header,
progress and footer copy; the island availability mask no longer leaves the
earlier rectangular dark sky area. The refreshed comic preserves the workshop
frame and a clear reading band. These are review screenshot iterations, not an
approval or a pixel-perfect match claim.

`DESIGN.md` and the sidecar still contain historical eight-world/160-node/33-page
descriptions and older paper-story/opening-comic component descriptions. Their
later enamel/open-sky prose remains the relevant incumbent treatment. This is
documentation drift; this scoped pass reports it without refreshing or repairing
the system files. Observatory accents belong to this local extension record.

No tests were run by this documentation pass. The current root verification
reports 11 runtime regressions, 11 pure story methods, 49 night-level methods and
seven overflow captures passing; those checks do not establish visual quality.
Independent finish scoring is pending at this checkpoint. No phone build or
installation, physical-device touch, performance, native editorial approval or
human-enjoyment validation is claimed.

Final scoped reviewer verdict: SHIP; all four listed findings resolved.
See NIGHT_FINISH_REVIEW.md. The final second correction changed only the comic
header right inset; incumbent DESIGN/sidecar remain untouched. PRODUCT.md now
records the factual ninth-world/184-node extension.
