# P16 design documentation check

5 October 2026. Scoped Impeccable documenter pass following the persistence
finding in `Reports/P16/impeccable-review.md`. Existing-document merge only;
the Sky Wind Workshop identity and `Docs/JOURNEY_SURFACE_BRIEF.md` direction
remain authoritative.

## Compared evidence

- `CascadeGame.Save.cs`: local profile loading, attempt mounting, commit saving,
  recovery states and player-facing acknowledgement/retry actions.
- `Persistence/PlayerProfile.cs`: durable preferences, preferred mode, heard
  story, pinned attempt and first-clear receipts deduplicated by CampaignNodeId.
- `Persistence/ProfileStore.cs`: schema 1, SHA256 checksum, flushed temporary
  writes, primary replacement, valid backup and newer-schema write protection.
- `Application/AttemptRecord.cs` and `AttemptService.cs`: pinned identity,
  verified command replay, exact final hash, Undo reconstruction and hint/Undo
  flags. `Docs/P16_SAVE.md` records scope and validation limits.
- Existing `PRODUCT.md`, `DESIGN.md` and `.impeccable/design.json` were compared
  with those implementation facts and the P16 finish review. The review opened
  all 15 final EN/RU/UK recovery fixtures at 320×693. This documentation pass
  checked fixture metadata and existing test results; it did not repeat visual
  review or rerun Unity tests.
- `Reports/P16/editmode.xml` records 323/323 passing; `playmode.xml` records
  43/43 passing. Final `ui-fit-findings.txt` is empty. Recovery fixtures show UI
  poses; scene reload, disk recovery and real button behavior have separate
  automated test evidence. The final recovery copy changed after the PlayMode
  pass; EditMode reran after that change.

## Documentation reconciled

`PRODUCT.md` now describes P16 durable local preferences, story, first-clear
receipts and pinned attempts, checksum/backup/recovery behavior, and the limits
of the evidence. P15's session-only statement is explicitly historical.
`DESIGN.md` now describes the saved source of map/setting/dialogue state and the
existing recovery surface without introducing a new visual system.

The 40 lessons remain short. The 160-stop campaign, further meta progression,
ranks, collectibles and Long Reach are not delivered capabilities; online is
design-only. Physical-device power loss, filesystem-full behavior, OS kill,
complete player build, native-language editorial approval and human enjoyment
remain unverified. The checksum is accidental-corruption detection, not
anti-cheat, encryption or a cloud-save guarantee.

## Preserved system

- Palette: paper, mint ink, brass/gold, six stable symbol colors and regional
  materials are unchanged.
- Typography: Balsamiq Sans, all existing type roles and font values are unchanged.
- Tokens: the full `DESIGN.md` YAML frontmatter is unchanged, including color,
  typography, shape, spacing and component values.
- Rules: shared symbol identity, live glyph faces, the complete machine, honest
  progress and clear decorative flight remain; the progress wording now reflects
  actual P16 local persistence.
- `.impeccable/design.json` is unchanged. SHA256:
  `7902089c1cc0f63ee7849de873b2f4bedefef03efb529726a1965e390774f06f`.

No new defect was canonized. The sidecar's older session-only narrative and
unrelated historical phase wording were not repaired: this pass was explicitly
limited to the reviewer-identified current-capability prose in `PRODUCT.md` and
`DESIGN.md`, with no token changes or sidecar regeneration.
