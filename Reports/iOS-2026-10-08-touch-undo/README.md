# iPhone touch and Undo update — 8 October 2026

- Unity iOS export succeeded: 0 errors, 346 existing warnings.
- Xcode signed Release build: BUILD SUCCEEDED.
- Installed `com.vickand.cascade` over the existing app on the paired iPhone 17, without uninstalling or resetting saved progress.
- The first install attempt encountered a transient CoreDevice disconnection (4000); the device remained available, and one retry succeeded.
- Physical-device touch latency, FPS and game-ready UI have not been measured. Event-handler checks and native visual evidence are under `../TouchUndo-Oct08`.

Launch verification: CoreDevice rejected launching because the iPhone was locked (10002 / SBMainWorkspace Locked). This is a device unlock requirement, not evidence of a game crash. Installation succeeded; cold startup and physical play are unverified in this pass. The unlock request is pending.
