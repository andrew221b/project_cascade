# P19 late monetization boundary

Design note before adding optional saved inventory/receipt fields. Status: mock
and deterministic local acceptance only; no SDK, live payments or ad UI enabled.
Core remains under revision at the author's request. C16 explicitly gates real
monetization on accepted core. Preserve package pins, rules2.0 and existing IDs.

One AdService owns fullscreen reservation, eligibility and mock callback receipt
handling. Saved grant ledger owns earned hint inventory/bonuscoins, nonconsumable
no-automatic-ads and known cosmetic entitlements. Receipt grant must be staged,
persisted, then published in memory; failure leaves inventory unchanged. Duplicate
and late callbacks cannot grant twice. Closing alone is never a reward.

Automatic eligibility: saved win/base reward before active continue, no first
session/first12clear,3newwins/180foregroundseconds,at most2shows/session,not a
chapter finale. AdNotReady skips that win opportunity; no pending show debt.
Rewarded is voluntary only from inventory; grants inventory rather than changing
row/HP/history. Hide offers and no-auto entitlement are honored separately.
A validated current Step may reveal a hint and spend one inventory token;
Unknown/stale/alreadyrevealed offers spend nothing. Free core hint remains free.

Mock purchase fulfillment accepts known products only after verified-provider
status, is receipt-idempotent, and restore only grants nonconsumable entitlement.
A mock verification flag is NOT production receipt validation. Store catalogs,
app/store IDs, verified receipts, consent/age audience choices and a real sandbox
session are release setup, not simulated claims.

## Official sources checked 5 October 2026

Unity LevelPlay regulation settings depend on installed SDK version and include
consent/privacy handling before initialization. Use the pinned official API
documentation at real SDK integration, not hardcoded remembered calls:
https://docs.unity.com/en-us/grow/levelplay/sdk/unity/regulation-advanced-settings

Unity IAP purchase management distinguishes receipt validation and entitlement
restoration. Restore must use matching product metadata; local mock state cannot
prove genuine ownership:
https://docs.unity.com/en-us/iap/manage-purchases
https://docs.unity.com/en-us/iap/restore-purchases
https://docs.unity.com/en-us/iap/receipt-validation

These sources do not certify the project's legal/audience settings. No real
SDK/trial/store/consent setup or sandbox transaction is performed in P19's local
mock boundary. Prices remain C16 hypotheses:40firstclear/100chapter,120hint,
240cosmetic,40voluntarybonus. No paid hint replaces onboarding/repairing controls.

## Local mock API details

Optional profile.Monetization contains known inventory, receipt grants, owned
cosmetic IDs and paid-hint state keys. Missing field has an empty default in
schema1. Mock product IDs are mock.no_automatic_ads and mock.keepsake.<gift>;
none are claimed as store catalogs. Atomic opt-in paid hint reveal stages flags,
saves inventory plus attempt, then keeps the reveal; write failure restores
flags/cache and balance. Previously charged attempt/state reveals are not charged
again after reload. This API is not wired to the free gameplay hint button.
