# Journey conversations — 7 October 2026

The map's Talk action becomes an optional conversation with the current region's resident and Mika. It explains the next story goal, offers the already-open selected level/continue, and permits returning to the map. It never grants rewards or invents a new gameplay quest. The existing large standalone dialogue is replaced by a bottom speech ribbon and bust avatar over the current map/checkpoint.

Six new residents: Bram (hedgehog carpenter/Wind Gardens), Sora (swallow courier/Cloud Post), Neri (otter/Lagoon), Toma (mole/Archive), Lumi (hare/Snow), Gran (Mika's grandmother/Beacon). Pip and Rue remain in Rain Mill/Orchard. Mika replies as a separate speaker; all lines are localized EN/RU/UK.

After each island's final first-clear, show a short authored exchange after stats and any trophy, on return to the map. Between stages2/3, every third level (startingC02) may show one short exchange. Replays do not retrigger seen conversations. Character banter varies deterministically by island. No interruption mid-swap, descent or wind lesson. Dialogue input pauses the attempt and active-time clock; skip and next are explicit user actions. Typing/slide tweens never advance a puzzle or mark story receipts.

Existing pre-level story receipt IDs0..32 and opening1000 remain. Reserve2000..2074 for40 island conversations and4000..4159 for periodic checkpoint exchanges in the existing per-mode HeardStory list. No save-schema/package/price/puzzle-rule change. Receipt is saved on explicit final next/skip; abandoned unread automatic conversations remain recoverable. Mode switching is blocked while the ribbon is open.

Reduced Motion reveals text immediately and removes motion. Only six new sprites and three cached incumbent portrait crops are used; no portrait sheet is drawn as a whole. Gameplay row, rewards, progression and selected mode remain unchanged by reading/tapping/animation.

## Implemented

40 distinct two-line island exchanges, 16 two-line checkpoint variants, eight regional introductions and contextual repeat visits. One automatic introduction per world; legacy story receipt numbers are retained. The map goal now follows the actual island rather than the former five-level story page. The optional Talk caption names the resident. First meeting introduces the situation; repeat visits give the selected island's goal. Final action starts its open level or resumes the exact unfinished stage. Not now closes optional talk. Automatic story can be skipped immediately.

The ribbon uses the game's brass/teal enamel and woven name banner, with six new bust portraits. DOTween supplies the 0.34s entrance and 0.28s portrait arrival; the text reveals at 42 characters/s. First Next reveals the current line; another Next advances. Reduced Motion reveals the full line immediately. The opaque lining prevents the map text from showing through. Dimming spans the full canvas; controls and text stay in the cloned device-safe layout. Checkpoint captions/receiver/buttons are hidden while speaking. Reading has no reward or gameplay command.

The fresh-attempt/resume distinction is shared by map Travel and Talk: a fresh mode still starts a three-stage descent; an existing unfinished stage resumes. Native Words/Mixed starts and the Shapes stage round trip verified this branch.

Validation: `Reports/JourneyConversations/README.md`. Six focused EditMode tests passed; native checkpoint and stats/trophy/island order passed; 408 authored localized lines fitted the narrow TMP ribbon. No phone build/install, physical-device verification, full suite, FPS benchmark or human enjoyment claim.
