# P19 local monetization boundaries

344/344 EditMode PASS, 2026-10-05 10:01:10–10:01:13 UTC. Current Unity compilation passed; Console zero errors/warnings.

Eleven new tests cover forbidden surfaces and each eligibility gate, skipped NotReady opportunities, 2/session, reward interval reset, late/duplicate/unverified receipts, separate hide/no-auto preferences, rollback/retry, verified mock non-consumable restore, unknown/stale/revealed hints, atomic disk hint commit and backward-compatible absent profile field.

No live provider, ad/store UI, catalog, packages or real purchase. The mock accepts a caller-provided verification boolean solely as a test seam; it is not receipt validation. SDK/sandbox/device/consent certification NOT RUN, gated on core acceptance and real platform credentials. Existing free hints remain free. No extra PlayMode/full player build was warranted by this pure service/disk adapter change.
