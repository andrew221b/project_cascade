# Outer HUD controls and transient task — 2026-10-07

Hint and Pause now occupy the outer safe edges with 48-pixel minimum hit regions. The central task remains between the upper ropes. Only the ceiling suspension half-span is capped to make room for controls; platforms, pieces and their touch targets keep their existing sizes. Rope simulation and knot connections remain active.

The whole task plaque, including text and frame, fades in over 0.28 seconds, holds for 2.2 seconds, then fades out over 0.45 seconds with SmoothStep easing. First display waits for playable readiness. New task text or a new level/stage resets the display. Pause, background and non-game screens do not consume its display time. Returning from pause does not restart it. Hidden plaque never intercepts input. Reduced Motion removes interpolation while keeping the brief display.

Native Unity MCP captures inspected: task-visible-390.png, task-hidden-390.png, task-visible-320.png; RU story caption on a review-only C37 word level. Simulated safe insets: left/right 1.8%, bottom 4%, top 6.6%.

Runtime review passed: task fully visible after fade-in, timer freezes while paused, plaque invisible after fade-out, canonical state unchanged, rope sampled positions clear the outer buttons at both capture widths. Capture helper additionally validates canonical state during rendering. Final project compilation succeeded; Console zero errors/warnings, Editor stopped and isolated profile override cleared.

No full automated suite, player build, phone installation or physical-device safe-area tests run for this presentation change. Rules, packages, schemas, prices, IDs and user Cloud settings unchanged.
