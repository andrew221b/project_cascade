# Lossless iPhone packaging — October 27

## Implemented without a player build

`PlayerBuildStorage.OptionsFor` applies `BuildOptions.CompressWithLz4HC` only to iOS and removes the conflicting regular LZ4 flag. StoreReleaseBuild and DevicePlayerBuild share this policy. Release remains release; local device builds preserve their explicitly requested Development flag. Both request detailed build reports. Android keeps its existing compression options, because the current AAB is already ZIP-compressed and a size/performance comparison is needed before changing its packaging.

After a future build, `player-build-size.json` records target, actual build options, Unity total, output file size (or -1 for a directory export), groups by output-file role and asset type, 50 largest output files and 100 largest source assets. Source asset totals are before outer archive compression; an iOS Unity export is not a final signed application. Installed app and store download sizes must be measured separately. `Symbols/` in an IPA is not installed game content.

No asset deletion, texture conversion, downscaling, audio transcoding, atlas modification, rendering change, code stripping adjustment, or remote download was introduced. The prior explicit unused-asset cleanup is already applied; no additional byte-identical resource PNGs were found in the audit. Further asset removal requires proven dynamic and serialized non-use, not a missing text-search hit.

## Measurements from existing artifacts

- Installed-test application artifact 202610229: about 849 MiB on disk.
- Store archive October 27: about 763 MiB on disk, including about 653 MiB Data.
- Read-only LZ4 HC level 9 / 128 KiB block experiment: 633.7 MiB of existing data -> 336.3 MiB, potential 297.3 MiB reduction. This excludes actual Unity archive overhead and is an estimate, not a new build result.
- Planning target: roughly 460–520 MiB signed iOS application; not guaranteed before measuring a real build.

## Checks performed

No Unity Editor/player, Xcode, APK/AAB or AssetBundle build was run. Direct Roslyn compilation of the affected Editor assemblies passed with both existing iOS and Android response-file configurations; compiler outputs were redirected to an isolated temporary folder. Existing CS0618 obsolete-API warnings remain. Ten checks on the compiled option policy passed: iOS release compression, no new Development flag, diagnostic/report options preserved, conflicting regular LZ4 removed, non-iOS options unchanged and idempotence. SHA-256 hashes confirm all 943 protected resource/settings files are unchanged.

## Next actual build

Use the existing store/device build entrypoints when authorized. Compare signed app and installed bytes against the same release baseline. Confirm reports, all nine worlds, Shapes/Words/Mixed, dialogs, trophies and EN/RU/UK. Compare unchanged imported asset data and deterministic rendered frames. Measure cold launches and loading across five runs; median load and frame times must not worsen more than 10% against the release baseline. No store upload or release is part of this change.

Reference: https://docs.unity.com/en-us/engine/6000.0/script-reference/unityeditor/buildoptions/compresswithlz4
