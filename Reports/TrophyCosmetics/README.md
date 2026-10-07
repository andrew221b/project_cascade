# Cosmetic trophies — focused verification, 7 October 2026

Eight existing trophy IDs have cosmetic previews, fitted clothing/badges and
separate animated ambient sprites. Three generated costume sets include both
hopeful/thinking poses and cropped dialogue portraits. Existing icons, prices,
parcel receipts, save schema and gameplay transition rules are preserved.

`economy.txt`: nine focused pure C# ownership cases PASS. This covers exact
15/30/45/60 unique-clear thresholds, repeat-clear deduplication, rejecting a
purchase of an earned appearance, purchase-before-milestone compatibility,
reload and independent journey-mode switches, existing caps and fragments.

Native captures are from the current Unity Editor, not mockups. RU390×844 and
UK320×693 with Large Text use simulated safe insets. Independent visual review
found no overlap/clipping in the new collection/shop section. Mika clothing and
badge appearance were also inspected in the menu and map.

Capture harness corrections: the first equip probe populated the profile but
not the active commerce ledger; the next Reduced Motion probe sampled before
the restored screen layout's LateUpdate. The milestone witness also needed
matching first-clear receipts, and the dialogue witness needed an existing
localized story line. These were fixture corrections, not
gameplay changes. Only final captures are kept. Unity MCP editor calls timed
out; native editor code captured the live camera, with CUA used for refresh.

The thinking-pose capture uses a forced presentation loss screen; it is an
appearance witness, not a fresh test of the gameplay loss condition. No player
build, phone install, physical-device/performance test or full suite was run.
Human enjoyment is not proved by these checks. Baked story comics retain their
authored historical clothing; runtime Mika illustrations use the equipped set.

Final native batch: **183 focused runtime assertions PASS**, eight final
captures. Outfit/badge previews for all eight sets preserve equipment, coins
and the puzzle state. Durable earned equipment is written and reloaded even
while the shop is in test mode. Journal, menu, map, dialogue, thinking pose and
ending illustrations follow the equipped set. Other dialogue speakers retain
their own artwork. Reduced Motion freezes the ambient objects; quality Off
hides them; decorations cannot intercept touches. Latest portable compilation
of 92 Presentation scripts: 0 errors, seven existing warnings.

The temporary native capture driver was removed; the QA profile was torn down.
