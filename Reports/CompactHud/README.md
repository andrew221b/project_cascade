# Compact gameplay HUD — native Unity review

Final native captures:
- `gameplay-en.png`: story task, fresh hidden instruction, teaching hand above the original candy piece.
- `instruction-en.png`: the same layout after the 3.2-second delay; dismiss cross visible.
- `gameplay-ru-small.png`: 320×693, RU, Large Text and the shortest three-piece rail; the level caption fits on one line.
- `gameplay-wide.png`: 1440×900 after instruction dismissal.

`runtime.txt` records actual rendered bounds: the Hint and Pause targets are 48 pixels on narrow phone views; each header edge stays 12 pixels inside its fixed rope anchor. Undo and Descend form a centred pair. The close target is 44 pixels. Instruction opacity goes 0 → 1 → 0, and hidden instructions do not intercept input. The hand reports sorting order 30 and the foreground shader. Removed decorations are absent. Each capture checks that rendering preserves the puzzle hash. Mobile safe insets are simulated `.018/.04/.018/.066`; physical devices were not tested.

`playmode.xml`: **3/3 PASS**, instructions/dismissal/pause without puzzle changes, relocated Hint request/apply/Undo with precision controls, and the glider flight band. These checks passed after the pointer correction; subsequent frame thickness and caption wrapping changes were verified by native rendering rather than another test run. Unity project compilation and final Console were clean. No player build, Xcode export or device installation.

The first visual pass exposed an oversized sliced-frame border and a split Russian caption; final images replace those captures. A capture-only zero-argument overload mistake was corrected before completing the story review. The teaching gesture is explicitly replayed for the foreground image, and its instruction age reset for the initial hidden state; the real delay and dismissal are independently covered by the Play Mode test. The review used a temporary profile, preserving the user's progress.
