# Whole-world map and celebrations — 7 October 2026

## Delivered

Map heading, footer and current-marker cream sheets are removed. Authored
MapSky/islands remain, with region/goal in dark ink, current island/Mika,
task/world progress, one amber level action and a teal story action. Small
framed arrows select only open levels for replay. Current-island count is
shown under its illustration rather than overlapping the next island.

Eight whole-world maps replace the repeating four/five-level pages. Existing
160 unique published levels are grouped15/15/10/24/24/24/24/24; five islands in
each world group2–5 complete three-stage levels. This is a pacing/layout
change, not newly generated content. All33 original story beats still trigger
at their original level boundaries. IDs, hashes, schemas, rewards and clears
remain. Existing scene parallax/weather/clouds retained.

The existing ribbon unfurls, earned stars arrive in order, score counts after
the stars begin arriving, warm lantern light/fabric confetti subside. Parcel
anticipation lasts0.55s; its item emerges over1.35s and settles. Stage outcome
ribbons also unfurl. The DOTween timeline advances locally only when focus is
allowed and clamps long frame gaps. Reduced Motion retains settled information;
effect qualityOff suppresses decorative glow/confetti. Normal navigation can
skip the ceremony; no completion callback grants progress or rewards.

## Validation

- Final focused EditMode16/16 PASS in `editmode.xml`: JourneyStoryTests,
  LocalizationTests, RewardTests. Repeated runs overlap; not a full suite.
- World/island coverage contains every published index exactly once,2–5 levels
  per island. Existing33 story boundaries remain independently tested.
- `runtime.txt` records passing native UI/runtime checks. An isolated profile
  replays all three C03 stage witnesses; the first two clear receipts are
  explicit fixtures, not claims of a human campaign playthrough.
- Statistics → committed parcel → map, progressing award clock, focus pause,
  finished poses, exact row/receipt counts, open-level replay and Reduced Motion
  map behavior passed. Reward counts were checked before/after presentation.
- Eight retained native samples opened: `award-start`, `award-light`,
  `award-settled`, `parcel-open`, `parcel-reveal`, `parcel-settled`, `map-ru`,
  `map-uk-small`. These are timed running-Editor frames, not design mockups or
  a video/performance measurement. Final two map recaptures confirm the arrow
  foreground correction. Pair JSON records actual control bounds.
- Compact320×693 uses Large Text and simulated safe insets. All map controls
  measured at least48px wide/high; back48×50.74, level181.20×56.16,
  conversation247.09×56.16 and arrows48×56.16. TMP overflow/translation failures
  were checked in the sampled states.
- After final runtime captures, terminal clock guards avoid repainting the settled
  ceremony every frame and effect qualityOff explicitly suppresses the glow.
  These final narrow guards compiled; the runtime matrix was not repeated after
  them. Full-quality sampled poses are unaffected.
- Final runtime Console: zero entries. Existing compiler warning about
  OutcomeCeremony.light hiding Component.light predates this revision.
  Play stopped and isolated profile detached.

## Probe corrections and limits

Initial probes held an old RewardCelebration awaiting deferred destruction
and looked for non-normalized DirectionButton names. They failed before the
full sequence completed; the helper now observes the fresh component after
cleanup and uses actual names. Initial actor images proved the sequence was
moving despite the stale-reference assertion. Those failures are recorded
here; final runtime result replaces the incomplete probes.

Own bounded visual review found the marker crossing locked scenery and dark
arrows on teal. The correction moves marker below the island and uses ivory
arrow graphics. Final views retain sky, current/restored color versus locked
black-and-white, open controls and no cream cards. Scope is this map/ceremony
revision, not whole-game approval.

No new raster assets/packages or phone build/install were needed. Original
CompletionAtlas, Parcel, Mika and map assets and metadata preserved. Full
suite, player/simulator/device rendering, physical gyro/touch, audio listening,
performance, native-language editorial review and human enjoyment NOT RUN.

Design note: `Docs/MAP_PACING_AND_CELEBRATION.md`; updated PRODUCT.md,
DESIGN.md, journey surface brief and TASKS.md. Impeccable's previously stale
sidecar/config were not repaired as a side effect.
