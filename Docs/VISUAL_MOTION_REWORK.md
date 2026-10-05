# Wooden islands: presentation revision

Author requested after completing P17–P20 and remaining tails on 5 October 2026.
This note records the rule boundary before implementation. Status: implemented and locally validated; device and human checks remain NOT RUN. See Docs/WOODLAND_SURFACE_BRIEF.md and TASKS.md.

## Visual intent

Mika works on wooden rope-supported workshops among floating gardens. Platforms
are recognizably wood: grain, knots, rope attachment, intact/worn/cracked/broken
states. Wind is a shared slow force; a loaded board bends and settles under its
row. Held pieces release their apparent weight and land with distinct compression
and rebound. Collapse throws short-lived wood chips; each existing piece follows
an authored gravity trajectory to its exact committed next slot, with varied
rotation and a soft ordered landing. Nothing should jitter, drift off its assigned
slot, or hide the receiver.

## State and interaction contract

Domain keeps exact deterministic row, contacts and collapse decisions. Visual
spring/gravity simulation consumes committed events only. It cannot commit a
swap, grant a reward, repeat entry transforms or reorder gameplay symbols.
Presentation may be skipped/interrupted and must converge to the same snapshot.
Reduced motion preserves immediate readable pose/state changes. Pause/focus
freeze decoration; hitting a moving piece stays reliable. No gameplay colliders
or random rigid-body puzzle resolution. New gameplay tools require their own
pure transition/solver/save contract.

## Understanding without reading

Bomb shows the matching piece on its face, visible fuse segments for remaining
actions, a shrinking fuse and blast footprint on the affected wooden board.
Defusal visibly cuts/cools the fuse. Forecast while holding shows expected floor damage before release; the bomb displays its actual remaining fuse. A predictive next-fuse overlay is not implemented. Repair closes cracks; a key visibly unlocks
a rope gate; a used bridge folds away; stamp goal appears as a seal silhouette
and fills at collection. Pictorial onboarding demonstrates one cause/effect
at a time, with optional short EN/RU/UK captions. Test understanding with people;
no automated readability scan can certify a non-reading child understands it.

## World composition

Replace incidental background propeller with a thematic anchored mill element
if the current asset cannot belong naturally to the scenery. Original assets
only, no competitor art reuse. Wide screens get complete composed side scenery
and an intentional central play area, not an unexplained stretched underlay.
Map has tappable stops, meaningful route motion and region weather/ambient.
Menus give Mika a believable place in the same world. Recompose still views
first, then add bounded wind/cloud/foliage/weather motion outside information.

## Challenge and rewards

Maintain first-clear progression. Optional stamps and route mastery can carry
one-time useful/cosmetic rewards after proof and a balance note; no mandatory
perfect play or simultaneous conflicting bomb objectives. Longer campaign
should combine established devices into new decisions, not relabel one puzzle.
Retention remains a hypothesis until a human playtest supports it.

## Craft references

State of Play describes Lumino City as handmade using paper, wood, lights and
motors: https://www.stateofplaygames.com/luminocity . Use tangible material
cause/effect as inspiration, not copied art. Ustwo's Monument Valley is a useful
reference for a coherent journey through interactive places:
https://www.monumentvalleygame.com/mv2 . CASCADE keeps its persistent-row identity
and the author's warm cartoon illustration brief.

## Bounded validation

One focused rule/lifecycle regression run after motion integration; native MCP
capture batch across small portrait, normal portrait and wide composition, plus
mid-fall/landed/defuse/blast poses and reduced motion. One correction batch and
one confirmatory inspection, then independent finish review. Device tests and
human enjoyment are recorded separately. Rebuild only when relevant changes
invalidate build evidence.

## Additional presentation/data note, before implementation

New region page starts preserve original eight story/page IDs and add25 regional pages (four-node last page in each24-node region). Region boundaries match P20 content; no C01–C160 IDs/hash changes. Optional stamp mastery grants one visible seal per campaign node (derived from existing proven goal records); no coin price/reward changes. The current paid/ad layer remains disabled. Wood body UV framing reads native PNG alpha>=128 in the Editor; image pixels and alpha are preserved. No new gameplay schema or package.
