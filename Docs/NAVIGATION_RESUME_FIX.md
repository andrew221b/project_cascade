# Navigation and resume — 7 October 2026

Design note: this changes menu access and resuming an existing attempt only. Rules,
prices, packages, save schemas, content IDs and existing clear receipts are unchanged.

The map offered Continue for a failed descent, then called ShowGame, which correctly
rendered its committed Lost state as Fail. Continue now explicitly uses the existing
free Retry path for that stage. A playing row, delivered checkpoint and final
unacknowledged completion retain their exact attempt and receipt. The map also pins
failed final-stage replays, so returning through Home cannot select another level.

Practice launched a bonus puzzle through the same single live-attempt slot as the
campaign. Mounting it discarded the pinned campaign attempt and descent, although
the stored first-clear receipts were retained. Practice is removed from My things;
legacy practice actions return to My things. The tool guide retains its isolated
animated examples and removes the action that replaced the campaign with an example.
An old bonus result returns to the campaign map. Existing bonus definitions and art
are retained for developer fixtures and compatible old saves.

Final Practice access review: old bonus-lesson records are also hidden from My
routes, and the shared menu content-opening predicate rejects training content.
This closes the saved-record replay route without deleting profile history.
First-use lesson films and coaching on the current campaign attempt remain.
Presentation compiled successfully after this correction (0 errors, 7 existing
warnings); native tests and new screenshots were not rerun for this narrow guard.

Optional stamp trails and stamp-record replay also used the same live-attempt slot.
Their shared entry method now refuses to replace a resumable campaign attempt; both
launch buttons explain that the current level must be finished first. The guard also
applies to direct calls and stale callbacks. No second attempt slot or save schema
change is introduced.

Unavailable level arrows are hidden. Map swipes cannot enter a world without an open
stop. Both controls use the same open-stop predicate as the level-selection action.

NavigationResumeTests adds seven native PlayMode regressions covering middle/final
failure recovery with clears, exact playing/checkpoint resumption, pending completion,
removed practice access with saved-stage preservation across reload, and available
and unavailable level arrows and first-island navigation. The optional-trail regression
checks disabled launch/replay controls, direct and reflected calls, and unchanged
saved attempt/descent hashes and clear receipts.
These tests use an isolated profile and real resolver proofs. The editing agent did
not run Unity, native tests, or a player build; integration validation is owned by
the coordinating agent. Automated assertions do not establish enjoyment or physical
device behavior.
