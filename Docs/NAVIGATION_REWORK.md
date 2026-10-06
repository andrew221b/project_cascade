# Navigation and story opening — 6 October 2026

The user's iPhone screenshot rejects the old level/mode/control mixture.
Keep the sky workshop, honey wood, enamel, brass and Mika; replace that flow.

Home has four destinations: Start game, Shop, Settings, Remove ads. Start game
opens a separate choice: Story, Shapes, Words, Mixed. Story leads through a
three-page illustrated introduction to the journey map; the other choices lead
to a swipeable numbered level shelf filtered by the existing concrete mode.
Back always returns to the immediately understandable parent. The map returns
to the play choice. Settings and help return to the screen that opened them.

Story alternates published primary and localized bindings after the first
three shape lessons. It keeps campaign order, pinned attempts, exact rules,
solver proofs and rewards. Existing explicit journey-mode calls retain their
binding behavior. The introduction records completion as HeardStory entry
1000, within that field's existing validated range; no save schema change.
It can be replayed. A tapped portrait opens the current conversation. Regional
dialogues introduce the next obstacle; the goal remains carrying the last
light to the Great Beacon after the storm.

No real ads or store SDK exists in this version. The Remove ads destination
states that ads are already off. Shop gives access to the existing earned
keepsakes and workshop collection; do not fabricate a purchase or a price.

The full painted background remains visible around individual enamel buttons.
New menu icons and comic scenes are original raster assets. Existing gameplay
control art supplies the sliced button and numbered level plaques.

Home's Start game is the amber action; Shop, Settings and Remove ads use teal
enamel with cream live captions. The play choice retains that hierarchy for
Story and its three concrete alternatives. Ivory plates carry short reading
copy. MenuIcons supplies eight illustrated actions; OpeningComic supplies
three square scenes: Gran and the Great Beacon, the storm and last light, then
Mika with Pip the frog and Rue the older fox setting out to relight the beacon.
Both atlases retain original source pixels. WorkshopPortrait remains the phone
background and WorkshopRoom remains the wide workshop view.

The concrete-mode shelf shows up to nine existing numbered levels, completed
checks and a live page count. Horizontal swipes cancel a level button's click
before changing the page; desktop wheel input also pages. Map uses the same
gesture handler. Both clamp page boundaries and show no Earlier/Later controls.
Shop's Keepsakes and Workshop return to Shop; My things opens the retained
travel bag, whose Back also returns to Shop. Neither adds a purchase flow.

Map art, island hit regions and Mika move together by at most a few pixels.
Clouds drift at a different depth. Device gravity supplies gentle tilt where
available, mouse movement supplies desktop feedback, and Reduced Motion
disables decorative movement. Map sky contains no gameplay suspension layers.
The hero's feet rest at the island hit point, smaller than the prior portrait.

Island selection retains invisible 76 × 72 reference-unit targets. Mika has a
separate invisible 64 × 80 reference-unit conversation target while her art
stays small. The map heading names the region and Goal: Great Beacon; the lower
plate separates the task, Go to level/Continue and Talk to the current friend.
Header Back returns to the play choice. Current-location cues derive from
saved first-clear receipts and remain absent from locked future previews.

The original MapSky plate covers the whole viewport at source aspect. Island
art retains its normalized map rectangle and its existing painted plazas;
this is not a pixel-perfect raster-registration guarantee. MapAvailability
substitutes only painted blue sky with MapSky at screen coordinates and keeps
the nearest-island desaturation mask, island contours and original cloud
brushwork. Full-screen gameplay clouds, foliage, glider and weather are hidden
while the map plate is active; separate map clouds/weather remain.

MapParallax calibrates the first available gravity sample, falls back to mouse
position, and adds restrained ambient drift. Its five-unit tilt contribution
plus 1.5/1.1-unit horizontal/vertical drift bounds the target offset to
6.5/6.1 reference units. Smooth response uses unscaled presentation time.
Reduced Motion and suspension set this shared island/art/input offset to zero.
These view-only effects and sensor cleanup never change the puzzle hash.

Validation: scoped navigation/state tests and native Unity portrait captures,
including RU/UK, compact safe areas and a wide view. No enjoyment, mobile FPS,
physical gyro or release-quality claim follows from Editor captures.

Reports/Navigation contains 16 native Editor PNGs, including four corrected map
poses at 320 × 693, 390 × 844, 1440 × 900 and an explicit +6/+4 shifted wide pose.
The compact Mika target is 52.5 × 65.7 rendered pixels; Story and Travel are
53.7 and 56.9 pixels high. These are simulated-safe-area Editor bounds, not
hardware pt/dp certification. The scoped correction verdict in
Reports/Navigation/impeccable-verdict.md resolves both scored sky/target
findings. Initial PlayMode 5/6 plus the swipe correction 1/1 supply six distinct
passing cases. The later map correction 1/1 expands an existing case and is not
a seventh test or a full-suite result. Localization passed 2/2. No documenter
tests, builds or captures were run; current phone-build status belongs to the
Navigation report. Physical touch/gyro, OS behavior, mobile FPS, accessibility,
native editorial and human comprehension/enjoyment remain unverified.
