# Pause reference, October 14

Rebuild only the existing pause screen as a uniformly scaled 720×1280 stage with native safe-area fitting. Reuse the established title board, green primary button and amber secondary. New alpha layers: seated apple Mika and bag, copper lantern, daisies and blank square controls; bright wooden/castle background contains no static airborne leaves or baked UI.

Home and Settings use existing routes; Settings returns to Pause. Resume preserves the exact attempt. Restart is gated by the existing retry/save conditions and invokes the existing retry path, preserving puzzle/seed rules. No economy, IDs, save schema or retry rules change.

The puzzle and its ambient phase stay paused. Only the pause screen's unscaled foreground decoration clock animates lamp brightness, daisy sway and falling background leaves. Decorations freeze when the app loses focus or is paused, respect reduced motion and effect quality, and never dispatch puzzle commands. Leaves are initialized using their natural prior offscreen trajectories rather than appearing in midair after the screen opens.

Validation evidence: Reports/PauseReferenceOct14.

## Validation completed

Unity 6000.3.8f1 MCP in the project on the external disk. 17 EditMode input tests and 14 PlayMode tests passed; new tests cover all four controls, committed attempt preservation, fresh retry and saved reset, locale/large-text fit, foreground animation, reduced motion and practice restrictions. The previous item screen’s six tests passed again.

Native review: all three locales, four viewport sizes, safe insets and large text; 52/52 active button centres resolve to their own controls, no overflowing/truncated reference captions. Mika’s sprite keeps its aspect and white tint. Settings returns to Pause; Home preserves the attempt; Continue resumes it unchanged; Restart uses the same pinned puzzle with a fresh attempt and zero moves.

Lamp, daisies and leaves animate while the puzzle and world phase stay paused. App pause freezes the decoration clock; reduced motion keeps a steady flame, stops flowers and hides leaves. Native six-second, 360-frame 60 fps clip recorded and decoded for visual QA. No phone/player build.

One pre-existing test fixture called the current optional PersistProgress(bool durable=false) API via reflection with no parameter, causing TargetParameterCountException before its loading test ran. Its explicit disk-read assertions now request PersistProgress(true); production persistence code is unchanged. The loading/navigation check then passed.

The reference artwork was recreated as separated illustration layers; these are close visual recreations, not identical original illustration pixels. Native screenshots are the comparison evidence.
