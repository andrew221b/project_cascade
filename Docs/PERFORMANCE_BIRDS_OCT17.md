# Soft mobile shadows, first-entry performance and living birds — 17 October 2026

## Changes

- The mobile URP profile disabled soft shadows even though the sun requested them. Enabled the Low four-tap filter, retaining its existing 1024-square atlas, one cascade, render resolution and artwork. Reduced the unused shadow range from 50 to 24; the camera is fixed and the gameplay casters/receivers remain inside it. Timber/rope shaders now compile URP's quality-specific soft-shadow keywords, in addition to the legacy keyword.
- LayeredWorld no longer eagerly loads all nine world skies. It loads the active world on demand. Mode/language/campaign lists are reused while immutable catalog, mode and locale match; progress eligibility still reads current clears. A repeated label scan reuses its list instead of allocating a component array. Removed repeated primary-level list copies while resolving the menu world.
- Optional sequential asynchronous prefetch starts after the first frame for packaged stage certificates and the next comic/map art. Parsing uses the existing worker. A fast Play tap does not wait on a new preload gate, no all-resource sweep, thread-priority changes, quality downgrade, save/schema/rule changes, new packages or audio sources.
- Four additional 2.5-second robin/wren excerpts from the already licensed CC0 source recordings. Complete recordings play in two/three-call phrases with short answers and longer breathing room, plus quiet distant birds when visible fauna leave the viewport. Small pitch/pan variation; no immediate repeat. The same two reusable wildlife sources handle both foreground and distant calls. Night observatory excludes daytime ambience; pause, conversations, mute, focus, ad suspension and output reset retain quiet re-entry. Four new clips add 640,176 source WAV bytes / 1,280,000 decoded PCM bytes.

## Measurements

Fresh Unity Editor processes, same native Mac, Unity 6000.3.8f1, mobile quality profile 0, portrait 390×844, isolated new profiles. OS disk caches were not flushed. The measurement excludes Editor startup/import; scene entry includes loading/integration/Awake. These are NOT physical-phone launch times, GPU timings, phone RAM or a low-end-device FPS guarantee.

| Metric | Before | After |
| --- | ---: | ---: |
| First scene entry | 1410.2 ms | 1359.8 ms |
| First Play synchronous work | 34.1 ms | 32.6 ms |
| First level until preparation ready | 171.6 ms | 113.6 ms |
| First map synchronous work | 70.8 ms | 45.8 ms |
| Home → map repeat | 52.9 ms | 31.2 ms |
| Editor allocated native+managed memory at sample | 824.3 MiB | 680.8 MiB |
| Main thread p50 / p95 | 1.078 / 1.199 ms | 1.024 / 1.291 ms |
| GC allocated/frame p50 / p95 (Editor, whole frame) | 13928 / 14130 B | 13952 / 13952 B |

The improvements concern entry stalls/memory, not a claim of zero allocations or faster GPU rendering. Low soft shadows add bounded filtering work. Frame CPU is similar; p95 rose slightly in this short Editor sample. See before-after.json for all metrics.

## Validation

- Baseline capture 1/1 PASS. Final targeted PlayMode run 22/22 PASS: existing first-play/persistence routes, all wildlife audio tests including three new cadence/recording cases, cache invalidation over Shapes/Words/Mixed and EN/RU/UK, lazy sky loading, and the mobile pipeline/shader configuration. Final independent cold-process performance/capture run 1/1 PASS. No C# compilation or shader errors in the final runs.
- Native 60-second scheduler regressions cover visible songbird bursts (10–24 starts), distant answers (9–22 starts, at least four recordings, both stereo sides, under ten-second maximum inter-call spacing), source-count stability, no puzzle-state change, and no spurious insect voice. Fake-clock tests complete recordings explicitly; they are not subjective listening checks.
- Soft, hard and no-shadow images share identical geometry, camera and pose. 2293 rendered pixels differ between hard and soft. Full images were visually inspected; the four-tap change smooths shadow boundaries, not image texture silhouettes.
- Broader run: 31/36 PASS before a quiet-re-entry refinement. Three bird assertions expected completely silent empty sky / immediate insect rest; the implementation gained a 1.7-second foreground grace and the two empty-sky assertions now distinguish ambient birds from the tracked animal. Final full wildlife selection passes.
- Two map UI tests still fail: a covered Back button on a compact map and a Completed caption outside its ribbon. Original UI/load/list source control also reproduces both failure families (the covered Back combination varies by world/locale). These are pre-existing layout defects, not fixed or hidden by this performance change. Original/final/control XML retained; no full-suite-green claim.
- Intermediate validation-only failures (missing namespace import and a test URP assembly reference) were corrected without package/assembly changes. One initial capture was aborted because batch mode does not produce WaitForEndOfFrame; capture now uses explicit camera rendering. No aborted/compile-failed run is counted as passing.
- Final relevant 27 source/assets match the isolated validation copy (the offline preparation tool is hashed separately). Temporary instrumentation/capture classes removed from clone Assets; original Editor scenes and real profiles were never changed. No iOS build/install, physical-phone test, GPU benchmark, full-suite run or human headphone listening is claimed.

## Remaining verification

Measure release-player cold first-use and sustained GPU/CPU/memory on an older iPhone or Android device. Review the quiet wildlife mix through the actual phone speaker and headphones. Two unrelated map-layout regressions remain open.
