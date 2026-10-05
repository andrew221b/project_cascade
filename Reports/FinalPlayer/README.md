# Local Mac player — final evidence

One native universal Mac build succeeded after Woodland corrections: Unity6000.3.8f1, Mono, Metal, CascadePlayable scene, x86_64 +arm64. Result0errors,346warnings,237832095bytes,72.754s. See baseline.txt/build.txt. Build start/end ISO conversion appears offset relative to Editor wall-clock; duration/result are the authoritative measurements.

Artifact: ../../CascadeUnity/Builds/CASCADE.app (ignored build output retained on disk). Existing productName CascadeUnity, version0.1.0 and IDs retained. Mach-O architectures and Info.plist/executable consistency verified; signature ad-hoc, no team/notarization.

Initial shell LaunchServices attempt returned kLSNoExecutableErr -10827. CUA native app launch subsequently succeeded. On the actual fullscreen3024×1898 player, CUA observed the map, clicked Let's go, advanced two Mika story cards, dragged circle onto adjacent triangle in C01, observed gravity fall and final settled victory: exact row delivered, one move, Retry/Next delivery/Undo/Map. Screenshot evidence appears in the task tool history; no on-disk player screenshot is claimed. Player.log copied at that checkpoint contains no exception/error. Existing progress was not erased; this smoke creates the first-clear receipt normally.

Warnings are chiefly installed AI inference/Sentis shader variants plus missing iOS localization metadata. These are not zero-warning release evidence. No additional build is justified by the warning-report wrapper alone. Unity-generated preloads/batching/URP runtime/linker/content-state are retained. Build-induced unrelated default TMP fallback mutation was restored.

NOT RUN: mobile builds/device touch/haptics/performance, speaker/headphone listening, native editorial approval, non-reader understanding, human enjoyment/retention, signing/notarization/public distribution, live Ads/IAP and online. Local leaderboard only; online design in Docs/ONLINE_IDEAS.md.
