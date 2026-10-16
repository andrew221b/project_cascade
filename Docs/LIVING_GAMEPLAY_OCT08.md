# Gameplay plaque, visual teaching and ambient variety — October 8

Presentation only: no puzzle rules, prices, packages, IDs or save schema changes.

The existing plaque sprite uses tighter runtime crop bounds around its solid painted silhouette. Its aspect ratio is retained, height matches the timber face, centre follows the bent wood, and the number uses the open paper area with a slightly larger cap. Original atlas and metadata remain untouched.

Remove the legacy delayed coaching text above gameplay. First encounters still use the existing demonstration and restricted guided action; completing the action dismisses the finger. Only a blocking puzzle-preparation failure may use the old status strip alongside Retry. The brief chapter announcement is not an action hint.

Extend the existing pool from five to seven rigs: two distinct bird species and two insect species in the base world, plus three existing stage/cosmetic visitors. Clear flight corridors use actual rendered platform/piece bounds and reserve hanging-device clearance. Near birds are larger in those free corridors; the outer gutters stay small. Visits are intermittent rather than seven constantly visible objects. Low effects retain two slots; map retains its existing footprint. Reduced Motion and Effects Off hide moving wildlife. Shared atlas materials and cached rig geometry avoid per-frame spawning.

Each wildlife owner receives a private session salt, independent of gameplay randomness. Cycle seeds vary paths, altitude, timing and approach direction; a repeated presentation phase remains stable. Landing chooses random eligible lower floors, positions on wood/receiver margins, or shapes. Avoid the last nearby support and vary rest/cooldown duration. At most one bird perches, occupied surfaces require 11 seconds idle, and input, wind, teaching or lost support interrupts the visit. Wildlife has no gameplay weight, collider or input target.

Realtime PBR highlights and geometry/contact shadows on moving pieces already use the preceding world-light implementation. Illustrated scenery uses bounded 2.5D surface relief; it is not reconstructed 3D geometry.

Validation: six focused native EditMode motion cases and five distinct PlayMode guided-action/visitor cases passed across scoped runs. Corrected the visitor test sampling from 20 fps to 60 fps to avoid aliasing a 4 Hz wing cycle; its 60-second observation window is unchanged. Nine world configurations exercised; native eight day/night, narrow, landing and teaching frames plus a six-second flight preview inspected. Unity Console: zero errors. Puzzle hash unchanged; profile isolated. Play stopped and override cleared. No phone build/device performance or full suite in this pass. Evidence: Reports/LivingGameplay-Oct08/preview.html and validation.txt.
