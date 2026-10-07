# Visual audit and bounded audio refinement — 7 October 2026

## Scope and before evidence

The requested audit covers the existing screen routes, independent Music and
Sound effects controls, and musical form/dynamics. Changes are restricted to
observed fit/spacing defects and the measured menu instrument imbalance. The
established workshop/enamel/illustration identity, gameplay rules and progression
remain authoritative. No new redesign or all-device acceptance is claimed.

`Reports/VisualAudit-Before/routes.txt` enumerates all GameScreen values and
fixture exceptions. PlayChoice is an unused alias of JourneyMap, Dialogue is
captured as its overlay, Devices has basics and bomb demonstrations, Settings
has three pages, and Prologue has three pages. Terminal/failure/checkpoint
captures use isolated valid attempts; direct presentation routing does not
prove progression eligibility. The folder contains native Editor PNGs and
paired capture/audit metadata for RU, compact UK and wide representatives.
Safe insets are simulated. These captures do not verify audio output.

`Reports/VisualAudit-Before/findings.txt` records overflowing lessons text,
compact UK word-language guidance, no-advertising explanation and wood durability
engraving labels. It also records unavailable shop/supplies fixtures with an
index-out-of-range error: these are missing before evidence, not passing screens.
The parent traced this to stale TMP word-info indexing in the QA measurement
helper, not the production route. The terminal Shop → tab 0 → item 3 → Supplies
path succeeded. The three missing catalogue poses were completed through direct captures in
the same confirmation round. The QA helper now reads current character meshes;
that final helper repair compiled but the bulk measurement sweep was not rerun.

The documentation role inspected the current source diff and before evidence,
including the compact UK bag and result PNGs. It did not operate Unity or rerun
the audit. Findings supplied by the root visual review remain attributed to
that review; this document is not an independent all-screen visual verdict.

## Findings and implemented corrections

| Finding | Bounded correction | Source |
| --- | --- | --- |
| Settings labels/switches crowd the enamel edge | Internal label padding and inset switch track; remove the redundant row separator | CascadeGame.Flow.cs |
| Compact travel bag needs room for all destinations and return | Four rows use 52 physical units below the 700-unit menu threshold, otherwise 62; shift Mika, parcel and shelf upward in compact mode while preserving workshop access | CascadeGame.MenuLayout.cs |
| Field-tool layout can re-anchor result Undo after reparenting | Only position Undo/drop while Undo belongs to gameplay actions; result owns its row | CascadeGame.Shop.cs |
| Result secondary actions do not share the authored menu treatment | Dress Retry/map and primary continuation with existing teal/gold enamel and corresponding type colors | CascadeGame.Flow.cs |
| Prologue counter lies over dark illustration | Place count below the image on the clear world | CascadeGame.Navigation.cs |
| Header titles crowd the right frame | Reserve additional right inset with the existing bounded type fit | CascadeGame.Navigation.cs |
| Lessons, no-advertising and compact language text overflow | Increase their vertical room; bound word-language label fitting | CascadeGame.Navigation.cs; CascadeGame.Language.cs |
| Device guide copy crowds its reading surface | Narrow the inset, lower the text region below the example, and bound fitting to 17–18 with a top inset | CascadeGame.Flow.cs |
| Catalogue guidance approaches the frame | Increase horizontal inset and vertical room; retain existing bounded fit | CascadeGame.Shop.cs |
| Wood engraving labels report overflow | Fit numeral type to the actual painted band, with the original font and tint | PlatformLegend.cs |

All source filenames above belong to
`CascadeUnity/Assets/_Cascade/Presentation/`. These source corrections were followed by the native confirmation and scoped
independent review recorded below.
No shared token changes were made. This documentation handoff only appends
DESIGN.md and creates this report; it does not refresh .impeccable metadata.

## Audio form and controls

Music and Sound effects already exist as separate persisted preferences.
Music controls the two score sources; Sound effects controls transient feedback
and wind/rain. No new preference field, mixer, package or rule is required.
The lifecycle correction makes disabling the audio owner silence/pause all four
loop sources and stop transient tails, preserving loop clips, positions and
crossfade for preference-respecting resumption.

`Docs/AUDIO_FORM_AND_CONTROLS.md` records the nine-score source review. All
scores already have 32 bars in 6/8 with 16-bar A/B sections, phrase replies,
countervoice, acoustic instrumentation, panning and room reverb. World-score
B/A RMS differences were 1.10–2.81 dB; their themes were preserved. Only the menu
showed a measured 9.79 dB B/A jump and 11.34 dB adjacent two-second transition.
The new Menu_HomeAmongCloudsBalanced sibling adjusts flute gain and the first
two B-bar velocities while retaining the original form, notes, tempo and
instruments. The original menu WAV/meta and all eight world scores remain.

The sibling measures 4.82 dB for both B/A and the adjacent transition, retains
68.571 seconds, and contains zero clipped samples. These source-file metrics
are not loudness-unit measurements or listening approval. The exact comparison,
original-file hashes, offline provenance and reproduction path are in the audio
report and `Reports/AudioRefinement/audio-metrics.json`.

## Validation checkpoint

Completed for this documentation handoff: current UI source-diff inspection,
before-route/finding readback, two representative before PNG inspections,
audio evidence readback and narrow DESIGN.md append. No Unity compile, new test
execution, native after-capture review or physical listening was performed by
this role.

`Reports/VisualAudit-After/validation-summary.json` records the final capture
set: 50 unique poses across 30 GameScreen enum values, with the documented alias
and overlay handling. It includes 390×844 portrait, compact 320×693 UK LargeText
with simulated safe insets, and representative 1440×900 wide views. Forty-seven
detailed audits retain stable gameplay-state hashes/attempts. The remaining
three direct catalogue captures are shop-ru, shop-uk-small and supplies-ru;
PlayableCapture checks the immutable puzzle hash internally and writes normal
bounds metadata for those captures. The capture set is complete.

The initial after sweep reduced overflow flags from 32 to one catalogue guidance
label; wood numerals, language, lessons and no-advertising text cleared. The
remaining label received more height and the device-guide text moved below its
example with a top inset. Their replacement captures were confirmed visually.
Four bag rows measured 52 high with a 17-unit gap to Practice. Fail/checkpoint
Undo measured 48 high, compared with 160 before, and the primary action remained
clear. These are bounded UI checks, not evidence of physical touch effectiveness.

The parent reports independent finish reviews passed 21/21 selected after views,
including the repaired device guide, and 3/3 direct catalogue views. No additional
material fixes were requested. This is a scoped verdict for the reviewed poses,
not an independent review of all possible screen states. The original
findings.txt and catalogue-supplement-findings.txt remain historical evidence
of the TMP QA helper failures. They must not be read as a clean rerun report or
as production catalogue-route failures. The final character-mesh measurement
repair compiled; the bulk measurements were **not rerun** after that helper edit.

`Reports/VisualAuditAudio/initial-playmode.xml` records 1/2 passing; the new test
asserted gain during the initial crossfade. After correcting its wait, the
runtime behavior remained unchanged and `Reports/VisualAuditAudio/playmode.xml`
records **2/2 passing**. Coverage includes all four Music/Sound combinations
(10, 00, 01, 11), balanced menu selection, owner-disable silence, and retained
loop clips/positions/crossfade. These tests verify source playback/gain state;
they do not establish headphone/speaker listening quality.

The parent reports the final MCP compile check succeeded: CompilationFailed
false, Compiling false and Playing false; the profile was detached. This includes
the QA-only character-mesh repair. No build or installation was performed for
this audit. The full test suite, physical OS/accessibility checks, live Ads/IAP,
performance profiling and audio listening were **NOT RUN**. Earlier build or
performance checkpoints elsewhere in the project do not validate this batch.

Physical-device touch/safe areas, headphone/speaker listening, native-language
editorial approval, perceived musical quality, sustained performance, enjoyment
and release readiness remain unverified. A screen-route capture set does not
exercise every possible state or progression path.

Evidence retention: all 50 initial poses were rendered and reviewed; 16 initial defect/context PNGs and their JSON measurements remain. All 50 current confirmation PNGs are retained. Redundant unchanged initial PNGs were removed; see `Reports/VisualAudit-Before/retention.json`.
