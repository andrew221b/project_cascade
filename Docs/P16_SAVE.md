# P16 — durable local journey

5 October 2026. Design note before introducing save schema 1. This extends local
persistence only: no rule, content, puzzle ID, price or package change. Online
remains design-only. Application/Domain state is not changed by animation or ads.

One local profile stores preferences, preferred mode, heard story pages, stable
CampaignNodeId first-clear receipts and a pinned attempt. Attempt identity includes
PuzzleId, node, rules, content hash, locale and objective. The remaining accepted
command route is a verifiable replay log; replay must produce the stored final
state hash. It reconstructs Undo history without presenting events. UndoCount and
HintUsed survive independently of route shortening. A first-clear receipt is
created from a committed win before animation and deduplicates on node across
modes/locales; undoing a delivered row does not erase its receipt.

The JSON envelope carries schema version and SHA256 payload checksum. Write a
flushed temporary file then atomically replace the primary while retaining one
previous valid backup. Interrupted temporary files never count as committed.
Corrupt originals are archived before recovery, not silently overwritten. A
newer unsupported schema is read-only until a compatible game can open it.
Checksums detect accidental corruption; this is not anti-cheat or encryption.

Missing/incompatible attempt content never becomes a different word. Keep valid
meta-progress and explicitly offer a new start from built-in compatible content.
If the primary is corrupt, restore the valid backup and explain that recovery.
If neither is valid, explain that the saved path could not be read; preserve both
files and ask the player to start a new local path. No fabricated restored progress.

Save at accepted command commit before event playback, Undo, Retry, revealed hint,
settings/mode/story changes, application pause/focus loss/quit. Focus return only
renders the committed state; it never enters a floor again. Tests use isolated
storage directories and cannot change the developer's actual journey.

Verification required: every built-in command path round-trip, interruption before
animation, retained history/flags/pins, duplicate wins across locales/modes,
corrupt/truncated/unknown-schema/backup recovery, durable settings/story/progress,
actual scene reload and small-screen recovery copy. Physical disk failure/power
loss and mobile OS kill remain separate from Editor simulations.

## Implementation and validation

Persistence is a pure C# adapter assembly using the existing Newtonsoft dependency.
Schema 1 stores a replay proof, preferences/story state and stable first-clear
receipts. Invalid attempt data is isolated from valid meta-progress. Future schemas
remain read-only; corrupt originals are retained as rejected files. The player
explicitly chooses a new puzzle or, only for an unreadable profile, a new path.
An actual write error pauses play with a persistent retry screen; the in-memory
committed row and receipt survive a successful retry.

All 157 built-in initial states and 365 accepted prefixes round-trip through the
same resolver with exact hashes and reconstructed Undo states. Locale/mode variants
of C01 do not duplicate a clear. Optional stamp goal and UK symbols restore exactly.
Ten storage tests exercise checksum, truncation, interrupted temporary write,
backup rotation, future schema, invalid route/pins, preserved meta and hint/Undo.
Full EditMode 323/323 PASS; PlayMode 43/43 PASS at 08:22:08–08:24:05 UTC.
The latter covers write-before-animation, scene reload after entry transform,
settings/heard dialogue, explicit incompatible-content recovery and failed-write
retry using real UI buttons. Tests use unique temporary directories.

Only recovery text/button copy changed after the PlayMode pass. Final EditMode
reran after that edit. 15 recovery-state fixtures (five × EN/RU/UK), each 320×693,
are native MCP Editor renders; they are UI poses, while actual disk/reload behavior
is covered by the test suite. Two bounded capture rounds. Round one found a
misleading new-puzzle label for complete-path recovery and over-specific wording
that blamed content change; both were corrected. No new raster assets.

Hardware power loss, filesystem-full on a physical device, iOS/Android OS kill,
actual player build and cross-device/cloud save are not verified here. SHA256
is accidental-corruption detection, not tamper resistance or anti-cheat.
The original 40 lessons remain short; no difficulty/fun claim is made by P16.
