# P13 verification

- EditMode: 305/305, 2026-10-05 05:35:15–05:35:32 UTC.
- Final PlayMode: 21/21, 05:46:04–05:47:40 UTC, after target visibility fix.
- The 40 primary levels delivered through real Input System/EventSystem UI:
  92 commands and all 15 event kinds. Thirteen timed kinds were interrupted
  by Skip, focus loss, component disable and Pause (52 cases).
- Scene reload, destroyed player, Reduced Motion, pooled tails, selection
  alignment and visible target during delivery have specific assertions.
- `playmode-unload-repro.xml` is the preserved 15-pass/3-fail first run.
  Its common cause was an inactive Canvas lookup during scene teardown.
  Including inactive parents fixed it; a direct reload test now covers it.
- `motion/timeline.json`: 468 real Unity camera samples from 12 scenarios.
  Representative frames were visually reviewed via MCP-driven captures.
  `retained` marks 111 kept PNGs; redundant frames were discarded after review.
  Within-command state hashes stayed unchanged; max glyph/body vertical error
  was 4.76837158203125e-7 world units.
- The mirror case is an in-memory QA fixture; no campaign content was edited.
- `motion-before-target-fix` preserves two visual regression witnesses plus
  measurements from the first capture round. The final target remains visible
  during travel; Retry appears only after synchronization.
- `audio-audit.json` checks source signal/import facts, not perceived sound.
- `meta-audit.json`: 145 entries, no missing/orphan/duplicate GUIDs.

These are Editor checks. No physical-device input/performance/audio/haptic
validation, player build or external human enjoyment assessment is claimed.
