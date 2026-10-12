# Coupled wood and piece contact — 7 October 2026

The raised piece kept its load after a tap release; resting pieces inherited only
the rigid board roll, ignoring its local curved surface. Selecting or holding a
piece now removes its contact load until it actually returns. Filtered point loads
shape the flexible plank between its pinned rope ends; central weights deflect it
more, and uneven weights tilt the suspension. Pickup and landing transfer a small
impulse into the existing board and rope springs.

Each resting original leans onto its local surface tangent and moves slightly
downhill with damped recovery. Its bottom contact, rather than its centre, follows
the surface. A lifted piece retains its starting pose and follows the pointer
independently of the board. Both letter plaques and shapes use these contacts.
The canonical row and hit interpretation remain owned by PuzzleInteraction.

This extends guided physical presentation, not puzzle rules or free gameplay
rigid bodies. Idle offsets are bounded to 8% of piece width, preserve slot order,
and use cached arrays without per-frame allocation. Existing falling collision
bodies, rope integration, artwork and materials are retained. Pause freezes the
integration; Reduced Motion uses static contact and removes sliding/rocking.
No packages, IDs, prices, saves or schemas change.

Native validation: 14 checks passed in the current Unity Editor, using an isolated
profile and actual pointer down/up/drag events. Lifting the central shape reduced
the measured sag from 0.1493 to 0.0858; its neighbours followed the recovering
surface. Return restored load without drift. Drag committed one atomic exchange
and spent one move. Selection preserved puzzle state; letters unloaded the plank
too. Pause froze integration and Reduced Motion kept static contact. Three final
390×844 images and selected native motion frames were visually inspected;
`Reports/LoadContact/contact-motion.gif` shows pickup and return. The temporary
review runner was removed and the Editor stopped. Purpose-built Unity MCP timed
out; native Editor control used CUA MCP. Full suite, phone, build and performance
testing were not run. These checks establish the contact behaviour, not human
enjoyment or unrestricted real-world physics.
