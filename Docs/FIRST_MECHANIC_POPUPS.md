> Superseded for presentation and acknowledgement behavior on 7 October by [Visual first encounters](VISUAL_LESSONS_OCT07.md). The current modal shows an isolated animation; closing arms real practice, never auto-uses a tool. Discovery persists after the relevant real action. The historical validation below applies to the earlier text-modal implementation.

# First encounter explanations — 6 October 2026

Design note before additive save metadata and tutorial identifiers. Every previously unseen mechanic on the visible rig pauses play before input is enabled. Combination levels teach one mechanic at a time. The first ambient gust waits for its explanation; landing wind has a separate explanation because it changes the row. Bought tools explain themselves before their first use and consumption.

An opaque cream modal with wood-and-brass controls uses the existing live device examples and original illustrations, one short EN/RU/UK explanation and one acknowledgement. Only an explicit acknowledgement records discovery. Timers, input, wind and physical presentation stay paused underneath; the isolated example can animate. Focus loss, reload and interrupted explanations cannot silently mark a lesson seen. Ordinary hints remain optional; first encounters are required once.

The gentle ambient gust and row-changing landing wind have separate explanations. An unseen landing wind waits at the previous stage's Continue action, before advancing or playing the descent. The ambient gust waits before its first impulse. Both resume only after acknowledgement. Combination boards queue explanations; known effects are skipped on replay.

Schema 1 gains an optional `SeenMechanics` list; missing metadata means no acknowledged lessons. `MechanicLesson` values 0–16 are persistent tutorial identifiers, separate from puzzle/store IDs. No Domain/Solver, prices, packages or puzzle definitions change. Acknowledgement persists metadata without issuing a puzzle command. A purchased tool resumes its explicitly requested Use action only from the acknowledgement button, never from animation completion.

Verified: 18 focused EditMode tests and native Editor callbacks for all 17
lessons, including pre-descent wind, first actual gust, interruption and all
three deferred tool uses. Five native views were inspected in EN/RU/UK with
simulated safe areas and a small Large Text case. Reports/Introductions/README.md
records limits; no phone build or human-comprehension claim.
