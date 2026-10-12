## verdict

All 22 required current captures were reopened at the same paths used for the initial review. Each PNG and adjacent JSON exists; viewport dimensions and named content remain valid. This is the bounded scoring pass for the four findings in `impeccable-review.md`, with source/document inspection limited to those corrections.

1. **Resolved — honest map progress.** `rain-map.png` and `copper-map.png` retain 1/40 restored lights while removing Mika and the gold selection from locked future stops. The footer now explains “14 stops to reach this island” / “29 stops to reach this island” and provides “Back to my path”. `CascadeGame.Journey.cs` binds Mika to `NextJourneyStop()` independently of the browsed selection; the return action navigates back without granting a clear.
2. **Resolved — glider separation.** `copper-orchard.png` and `mixed8.png` now show the glider as a small distant silhouette in the gap above the active row, with no contact against the star, diamond, circle or letters. The supplied lowest-approach captures retain clear symbol silhouettes. `held-piece.png` shows no glider during the hold. `LayeredWorld.cs` bounds its rotated illustration to the reserved flight band and hides it while holding or when that band is unavailable; this corrects composition rather than relying on a lower render layer.
3. **Resolved — dialogue region.** `pip-dialogue.png` now has Rain Mill's teal ground and mill edge; `rue-dialogue-small.png` has Copper Orchard's peach ground and orchard edges. `OpenJourneyStory()` applies the stop's zone before opening the dialogue, and `ShowGame()` reapplies the current puzzle's zone. These fixtures demonstrate the intended presentation without establishing unlocked progression.
4. **Resolved — current design record.** `DESIGN.md` now records the implemented P14 direct-play/journey extension, character and regional art, optional precision input, current map semantics, bounded glider and dialogue scene selection. It points to the current journey evidence and preserves the actual 40-puzzle/session-only scope, staged campaign/save/meta work, online-as-design-only boundary and device/human-enjoyment limits. Its former P12-only/P13–P14-pending description has been replaced.

No material regression introduced by this fix batch was visible in the supplied recaptures.

## remaining

Clear for the four scored fixes. **Ship covers the scored fixes, not the whole surface.** This does not certify the full game, the broader user request, campaign depth, durable progress, meta systems, physical touch/haptics/performance or human enjoyment. P15–P20 work remains staged; online remains design only. The parent reports final 309/309 EditMode and 36/36 PlayMode passes; this reviewer did not run Unity or tests. No HTML/CSS detector or approved-comp comparison applies to this native code-led review.

disposition: ship
