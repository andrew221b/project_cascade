# Current role — 29 October 2026

This Site is a compatibility relay for previously installed clients. It has no game database or publisher/Unity authority credentials. Production game data and deletion jobs live in Unity Cloud Code / Cloud Save Private Custom Data. Its old /delete-account page now redirects to https://flyingladders.vickand.com/delete-account/ (v18, source d9e3dd5fbe540fd960f90dd91046b76b3961bda8). Existing API relays are retained. Do not reactivate old SQL bindings, credential importers or retired account-write paths from historical notes below. Current routes and migration evidence: Docs/UNITY_CLOUD_MIGRATION_OCT27.md and Backend/WebFront/README.md.

## Historical implementation and deployment notes

# CASCADE account backend

This deployment contains the external account deletion page and server APIs. The previous marketing source is retained under `retained-marketing`; it is not promoted as an approved public store/legal page.

Run `npm test`, `npm run db:generate` after schema edits, then the Sites build/source/publish workflow. SQL migrations are schema-only and immutable once applied. The production D1 store is bound as `DB`. Never copy a preview database over production.

Private service endpoints (all require the service bearer and exact project/environment/player context):
- POST /api/account/read, /api/account/write: atomic first insert and version CAS.
- POST /api/verify: current Apple/Google evidence, immutable transaction and subscription-chain owner.
- POST /api/purchase/claim: idempotent ownership check, called by trusted Cloud Code only.
- POST /api/reward/intent, /api/reward/status: server-issued AdMob nonce and receipt status.
- POST /api/account/effects: signed rewards and aggregate subscription expiries.
- POST /api/account/deletion-code, /api/account/delete: authenticated deletion lifecycle.
- POST /api/maintenance: retry pending events and deletion jobs, refresh subscriptions, expire unconfirmed intents/codes. Invoke periodically from an authenticated server scheduler (one minute recommended for deletion jobs). Scheduling cannot be activated without project access.

Public endpoints:
- GET /api/health: readiness booleans only; no secrets, players or purchase proofs.
- POST /api/purchase/local-verify: account-free Apple/Google receipt verification for under-16 local profiles. It accepts only product ID, transaction ID, bundle ID and store receipt/JWS, stores none of them, and returns only the verified catalog result. It never accepts a name, email, player ID or caller-supplied store verdict.
- GET /api/admob/ssv: Google's ECDSA signed callback. Reward amount in AdMob is 1; game grant is catalog-controlled (40 coins / 1 hint / parcel access).
- POST /api/notifications/apple: App Store Server Notifications V2, official Apple chain/JWS verification and current-state refresh.
- POST /api/notifications/google: authenticated Pub/Sub push with exact audience and service-account email, current subscriptionsV2 refresh.
- /delete-account and POST /api/deletion/confirm: explicit single-use code, same-origin confirmation. Without app access, the page provides the published support email request path, with ownership verification before removal.

Runtime secrets/IDs are listed in `.env.example`. Use Sites environment tools or native secret UI; never put values in source, public JSON or a player build. DATA_KEY is stable: rotating it without migrating ownership fingerprints and encrypted proofs breaks their recovery.

Design note, 21 October: local-file-to-native-Sites-secret input is unavailable without rejected plaintext tool output. Add a separate `server_credentials` table for one initially installed, AES-256-GCM sealed publisher/service credential record, using the existing unchanged DATA_KEY. A temporary import endpoint accepts only a body signed by a pinned, short-lived P-256 public key, bound to exact origin/project/environment/bundle and timestamp. It allows exactly three known fields, validates key formats/Google identity, and atomically refuses overwrite/replay. Local file contents travel directly over HTTPS to this account backend; no credential is printed, returned, committed, archived or sent to another service. Close the endpoint by removing its two public activation settings after installation; the durable single-record constraint also prevents reuse. Runtime loads only these three encrypted fields; existing private runtime bindings and all ad behavior remain unchanged. Never expose a read/export endpoint for the record or rotate DATA_KEY independently of the existing encrypted data.

Deploying the Worker is separate from configuring stores and Cloud Code. Keep game activation flags false until UGS module, secrets, provider IDs, Apple Sandbox/Production separation, Google Pub/Sub push identity, AdMob SSV URLs, all-environment deletion scopes and scheduled maintenance have passed real disposable-account tests. Root CA certificates must come from Apple's official certificate authority; online chain checks remain enabled.

Cloud Code secrets: CASCADE_RECEIPT_VERIFIER_URL = deployed origin + /api/verify, CASCADE_RECEIPT_VERIFIER_TOKEN = runtime SERVICE_BEARER_TOKEN. Cloud Code sends the authenticated UGS context; clients never receive this credential. Use UGS access rules to prohibit client writes to server-controlled game data.

Deletion blocks account writes in the same database transaction that removes game data. Cleanup deletes all Cloud Save access classes in every explicitly configured project environment, removes readable legacy purchase-owner records, then deletes Unity Authentication. On failed provider calls the job remains retryable and the public response reports pending, not completed. It never deletes a global Apple/Google/Unity Player Account or cancels store subscriptions. Keyed one-way security fingerprints survive to reject purchase replay and account resurrection. Erasure from provider backups follows provider retention policies, not an invented application guarantee.

Maintenance isolates failed publisher events and deletion jobs: a provider outage leaves its item pending but does not stop later items or account erasure. Cloud Save deletion sends the environment of each target path, including secondary environments. Any unfinished item keeps the maintenance response retryable (503); success is reported only after all selected work completes. This correction changes no schema, catalog, price, identity, or ad behavior.

Deployment update, 21 October: Site version 7/env revision 7 safely received publisher credentials and the prepared matching service token. The runtime sealed record overrides only its three allowlisted fields; native DATA_KEY and masked Unity keys remain unchanged. Protected hosted read is 200; import is closed (404), and its temporary private signing key was removed locally. Publisher API authorization, device/store tests and automatic maintenance remain unverified. Production Cloud Code reaches Secret Manager but rejects CASCADE_RECEIPT_VERIFIER_URL as an invalid absolute URL, before any backend request. Correcting that single field is the currently confirmed owner action. Evidence: Reports/SecureConnectionOct21/verification.md.

Deployment verification, 20 October: the request-scoped official SDK factories fix the Worker global-initialization failure; a real Google Pub/Sub TEST reached the hosted endpoint with HTTP 200. Apple's public root CAs are installed and signed TEST verification is independent of the private publisher API client. Its pinned node-fetch transport is mapped at build time to Workers native fetch, retaining timeout/cancellation and binary OCSP response bytes; official signature, chain and revocation checks remain enabled. Publisher private-key import, the matching Unity connection secrets, unattended maintenance and real store/device checks remain pending. Runtime health is not proof of their completion; client production switches remain disabled.

For release, test: simultaneous first writes; two owners redeeming one receipt; repeated/delayed AdMob SSV; close-before-callback and restart; Apple renewal/grace/expiry/refund; Google canceled/on-hold/replacement/refund; callback duplicates and out-of-order delivery; deletion during an in-flight save; external code expiry/replay; retry after authentication deletion timeout. Local fake publisher tests do not replace those live tests.
