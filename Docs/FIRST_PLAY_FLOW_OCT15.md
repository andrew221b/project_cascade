# First Play flow — 15 October 2026

The user requests Home → Play → opening comic → first playable campaign level, with no map or separate opening conversation in between. After that first delivery, the result continues to the map. Later Home Play visits open the map and let the player choose/resume a level.

Use the existing opening-comic/story receipts and actual campaign progress to distinguish a fresh profile from returning players. Do not replay onboarding over an existing unfinished journey or completed progress. The first comic covers the first island introduction, so its old map conversation is not repeated. Defer unseen later-world comics to starting a level from the map. Keep standalone/replayed comics' usual map destination. The closing button of the first-start comic says Play; other comics retain To the map.

The first campaign result's primary action returns to the map, also when restoring an old pending result that recorded a next-level destination. Rewards, parcels, published level recipes, gameplay rules, store products and saved field types remain unchanged. No new receipt ID or schema field is needed. A failed comic save must leave the comic unread and must not start a puzzle.

Validate native UI clicks through all opening-comic pages, actual first-level delivery, later Play/level selection, a reload with an unfinished first level, existing-player entry, a failed durable comic save, and the unchanged default comic callback. Use an isolated profile and preserve the user's main unsaved scene.

## Validation

Unity 6000.3.8f1 compiled the current source and passed 7/7 PlayMode cases in an isolated temporary source copy. Five FirstPlayFlowTests exercise the actual Home/comic/map controls, all three stages of the first level, selection of level two, save/reload with one committed move, returning-player entry, delayed unseen-world comic, failed comic persistence and the standalone comic's default map destination. Two existing foreground/navigation cases also pass.

The first native run exposed an old comic Canvas pre-render subscription firing while the direct game entry rebuilt the puzzle, after the active comic had already been cleared. WorldComicLayout now unsubscribes as soon as the comic is no longer current, including that same-frame transition. The original failing evidence is retained in `Reports/FirstPlayFlowOct15/BeforeFix/`; final evidence is in `Reports/FirstPlayFlowOct15/playmode.xml`, `editor.log`, and `validation.json`.

No scenes, assets, profile schema, product catalog or gameplay/economy rules changed. The primary Editor's unsaved scene was preserved. No phone/tablet build, installation or external deployment was performed.
