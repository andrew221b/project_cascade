# Wooden branch stops

`branches-gameplay.png`: native Unity MCP render, C08 at 390×844 with simulated safe insets. Both upper and lower wooden rails carry natural branch stubs, seated into the plank surface. The original illustrated honey wood is preserved.

`runtime.txt`: four visible stopped edges use the generated BranchStops atlas on four-vertex lit cutout planes. The two atlas variants alternate; view positions move with the same board springs. The puzzle hash is unchanged after the movement sample. Final project compilation and Console have no errors.

`playmode.xml`: **1/1 PASS**, the existing UsedBridgeIsShownClosedAndUndoRestoresIt test. Closed-edge visibility and bridge/Undo behavior still work. No new gameplay logic or tests were added.

Artwork and final generation prompt: `Docs/BRANCH_STOP_ART.md`. No player build, physical-device check or phone installation.
