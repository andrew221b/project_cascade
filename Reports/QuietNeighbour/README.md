# Quiet neighbour feedback — 2026-10-07

Removed the destination lift and hover selection sound. A legal destination receives only a thin ivory contour (scale 1.025, alpha 0.10); illegal destinations have no contour. The source retains its existing lift, finger tracking and exchange animation. Domain, legality, durability forecasts and device forecasts are unchanged.

Native Unity Editor MCP captures at 390×844 inspected: `shapes-hover.png`, `letters-hover.png`. Runtime checks passed: destination position unchanged, destination alpha 0.10, blocked contour hidden, preview preserves canonical state hash. Native TokenSlot pointer down/drag/up passed: exactly one atomic swap and dragged symbol in destination slot. Capture helper also asserts unchanged state during rendering.

Final Editor stopped, isolated profile override cleared; compilation succeeded and Console had zero errors/warnings. No automated suite, builds, phone installation or physical-device tests run for this presentation-only change.
