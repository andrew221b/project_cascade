# Route metrics, 8 October 2026

The two paper fact cards on **My routes** made the record screen heavy and ambiguous. In particular, the star illustration was paired with `BestClean`, which is a move count for a run without hints or Undo, not a count of earned stars. Showing `13` beside a star implied an impossible 13-star rating.

The screen now shows one transparent row: footsteps **× best moves** and star **× best earned stars**. Stars come from the clear receipt for this record's journey mode and level; if no three-stage rating is available, the value is `—`. This only changes the displayed metric. The saved clean-route record, rewards, gameplay rules, pricing and schema stay as they were. The localized `Stars` UI key was added in EN/RU/UK and built into the existing string tables before the later request to remove captions.

Follow-up: route navigation arrows now flank the level ribbon instead of the Back button. Their gold artwork has a dark tinted outline for sky contrast and keeps a 48-unit hit area. The redundant metric captions were removed and both icons enlarged. The actual move and star values remain unchanged.

Final inset correction: the level papyrus now spans 15–85% of the usable width, and the left/right arrow centres sit at 24.5%/75.5%. Both complete tap areas remain inside the paper with a measured margin, including the 320×693 Ukrainian Large Text layout. The native capture assertions check these bounds directly. A fresh build containing this correction was installed and launched on the paired iPhone; see `Reports/iOS-2026-10-08-arrows/README.md`.

Focused native checks: `Reports/LocalizedJournal-Oct08/13-records-inline-ru.png` at 390×844 and `14-records-inline-uk.png` at 320×693 with Large Text. `validation.txt` confirms both captures, no paper cards or metric captions, footer clearance, arrow alignment and unchanged puzzle/wallet. Console errors from the final review: zero. A full screen-gallery rerun was not performed in this pass.
