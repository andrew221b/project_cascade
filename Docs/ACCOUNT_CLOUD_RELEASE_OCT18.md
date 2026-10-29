# Account security completion — 18 October 2026

Design note before schema/package changes. Existing gameplay, product IDs, prices, account envelope version and public Cloud Code methods remain compatible.

Cloud Save write locks cannot atomically create an absent record. Replace the fabricated absent lock with a durable SQL compare-and-swap account store behind authenticated Cloud Code. First creation uses a primary-key insert; subsequent updates require the exact storage version. Purchase ownership uses a unique, immutable transaction key and subscription-chain key. Existing protected Cloud Save records are read only for one-time migration; SQL becomes the single writable authority. Packed envelopes are bounded below D1's 2 MB row limit, with explicit rejection rather than truncation.

Reward offers are issued by Cloud Code against the authoritative account. AdMob's signed SSV callback, not its client callback, confirms a durable reward receipt. Read/sync reconciles receipts idempotently, including callbacks after app closure. Preview ads keep their separate test-only ledger.

Apple V2 and authenticated Google Pub/Sub notifications refresh current publisher state. Event inbox entries persist before processing; duplicate deliveries retry unfinished work. Subscription-chain ownership is immutable, cancellations retain paid time, grace uses its grace expiry, expired/revoked purchases remove only their own entitlement. Reconciliation aggregates all valid owned chains per product.

Deletion first creates a tombstone, then removes account/game data, pending rewards and store proof material. A durable deletion job removes legacy Cloud Save and Unity Authentication last and can retry. Minimal keyed, nonreversible ownership/tombstone fingerprints prevent deleted accounts and replayed receipts from resurrecting. This does not delete the player's global Apple/Google/Unity Player Account. Deleting the game account does not cancel store subscriptions; users are told to manage subscriptions in the store.

The external deletion page accepts a short-lived single-use code issued inside the authenticated game, and offers a support request path for people without access to the game. It never deletes an arbitrary submitted player ID. Credentials stay in runtime secrets, never client assets or source. Deployment readiness and production activation are separate; no enabled flag is set without live checks.

Hosting: migrate the existing unpublished Sites project to its supplied Vinext runtime in `Backend/AccountCloud`, preserve `Website` marketing sources and archived assets, add D1 and explicit API routes. Store verifier dependencies remain the pinned official Apple and Google libraries. Public access is necessary for external deletion and store callbacks; do not expose private service endpoints without their bearer authorization.


## Actual result

Worker version 1 is published: https://cascade-sky-gardens.active-seed-4233.chatgpt.site . Public deletion page: `/delete-account`. The native deployment result is `succeeded`, runtime secret revision 1. Source commit: `c2e03b2996fb39a37284a175fd6e0927ace64d4d`.

253 automated checks passed (157 Cloud Code, 68 actual-source client with SDK fakes, 28 Node/SQL/protocol checks). Final Unity Editor compilation and EN/RU/UK localization generation passed. The prepared module is under `Reports/BackendReleaseOct18/CloudCodePublish`. Existing gameplay and user device saves were not reset, and no iPhone build was installed for this backend task.

The production service is NOT activated in the game. The local UGS service account receives HTTP 403/code 53 for this project. Player Accounts client ID, store publisher credentials and production AdMob slots are missing. No real renewal/refund, SSV grant or remote-account deletion is claimed. No financial transaction or operator account was used as test data.

Local compiled Worker responses were checked: account support and deletion HTML 200; health 200 with D1 available; readiness correctly false without store/project credentials. External production HTTP probes from this environment returned Cloudflare 403/error 1010; the Sites Worker error log query was empty. This is an unresolved external reachability check, not a successful end-to-end API test. Do not configure production webhooks as verified until requests reach the Worker and store test events complete.

Finish requires: grant the UGS deployment service account the target-project permissions; set the public Player Accounts client ID; set runtime Apple/Google publisher and push-verification secrets, AdMob slot IDs and SSV callback; configure deletion service scopes and all environment GUIDs; schedule authenticated `/api/maintenance`; deploy the prepared Cloud Code module and its two Secret Manager values; resolve external routing/reachability; run the disposable-account/store sandbox matrix in `Backend/AccountCloud/README.md`. Store dashboard setup cannot be replaced by invented credentials.

`DATA_KEY` and `SERVICE_BEARER_TOKEN` were generated and saved only as masked Sites runtime secrets. Their values are absent from repository files/reports/player assets. The existing website marketing draft was retained and not published as approved store/legal copy.

Sources used for protocol details: [Unity write locks](https://docs.unity.com/en-us/cloud-save/concepts/write-locks), [AdMob SSV](https://developers.google.com/admob/unity/ssv), [Apple official server library](https://github.com/apple/app-store-server-library-node), [Google authenticated Pub/Sub push](https://docs.cloud.google.com/pubsub/docs/authenticate-push-subscriptions), [Google subscription lifecycle](https://developer.android.com/google/play/billing/lifecycle/subscriptions), [Unity player deletion](https://docs.unity.com/en-us/services/player-management/delete-a-player).
