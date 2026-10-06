# Physical presentation lifecycle regression

Initial2-test run: debris passed; scene reload failed because a cancelled
presentation redrew already-destroyed menu/layout UI. A separate native Editor
shutdown also exposed debris transforms destroyed before ClearTails.

Both ownership-order bugs were fixed. Final3/3PASS,4.607344s: isolated debris
gravity/pause/quiet/clear; contact momentum and exact swap settlement; scene reload
during presentation releases the old attempt without exceptions or state changes.
Then native Editor Play/Stop was repeated; console remained0errors/warnings.
See ../PhysicalPresentation for visual and physical-device/performance limits.
