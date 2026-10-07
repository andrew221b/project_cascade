# Menu and completion details — 7 October 2026

Ordinary actions now use the existing blue enamel face with ivory captions.
Warm faces remain on selected tabs. The live gameplay Descend face is unchanged.
Completion uses only “All 3 stages complete”; its three metric icons, numerals
and captions share a centre line. The redundant trophy-waiting line is removed.
Visible dot separators are absent from Presentation and all EN/RU/UK values.

Validation: one portable compile of 87 Presentation files passed with no errors
and seven existing warnings (OutcomeCeremony.light / obsolete AdMob flags).
One native Unity camera capture batch passed 135 focused assertions: action
sprites, selected-tab contrast, text fitting, separator removal, metric centres,
and reward-note absence. Temporary isolated profiles were used and removed;
Play Mode stopped after capture. Four settled images were inspected visually:

- home-ru.png: RU, 390×844.
- shop-ru.png: RU, 390×844, selected Tools tab.
- completion-ru.png: RU, 390×844, three-stage C03 result.
- completion-uk-small.png: UK, 320×693, Large Text.

Simulated safe insets: left/right 1.8%, top 4%, bottom 6.6%. The automated
resolver produces a near-instant result (0:00); these captures are layout
witnesses, not human play-time samples. Text/action assertions are recorded
in runtime.txt; button rectangles are in the corresponding capture JSON.

No new assets, gameplay changes, full test suite, player build, device install,
physical-device validation or human-enjoyment claim. Native app-window access
returned noWindowsAvailable; fresh images came from the running Editor camera,
not an OS screenshot. Temporary capture code was removed after validation.
