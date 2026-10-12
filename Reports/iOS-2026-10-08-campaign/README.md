# iPhone campaign update — 8 October 2026

- Unity iOS export: Succeeded, 0 errors, 346 existing warnings (same Sentis shader/iOS localization metadata warnings as the preceding build).
- Xcode Release: BUILD SUCCEEDED.
- Installed `com.vickand.cascade` over the existing app on the connected iPhone 17, without uninstalling or resetting its data.
- CoreDevice accepted initial launch and a second launch with `--terminate-existing`. The subsequent process snapshot still contained the newly launched CASCADE process.
- Launch acceptance and a surviving process do not prove the UI is game-ready. Physical touch/playthrough, device screenshots and FPS were not measured. Visual/platform behavior was inspected in native Unity Editor captures under `Reports/CampaignLogic-Oct08`.

The first process-list filter failed because CoreDevice represents executable as a URL. The subsequent unfiltered local snapshot was read only to find CASCADE. Other app details were not copied into this report.
