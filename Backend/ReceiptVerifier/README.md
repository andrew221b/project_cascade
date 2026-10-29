> Production account APIs and deployment now live in [AccountCloud](../AccountCloud/README.md). This directory retains the publisher verification boundary and its original Node harness; it is not the complete deployed account service.

# CASCADE purchase verifier

Prepared service, not deployed. Store credentials and hosting remain publisher setup.
`POST /verify` is a server-only endpoint called by the authenticated Unity Cloud Code
purchase module. Every call requires `Authorization: Bearer <service secret>` and
must match this host's configured UGS project and environment. Unity clients send
purchase evidence to Cloud Code; they do not hold this secret or call this endpoint.

The request uses camelCase fields:

```json
{
  "playerId": "<authenticated Cloud Code invocation player>",
  "projectId": "<configured UGS project ID>",
  "environmentId": "<configured UGS environment ID>",
  "bundleId": "com.vickand.cascade",
  "productId": "cascade.coins_240",
  "transactionId": "<store transaction or order ID>",
  "store": "AppleAppStore",
  "jws": "<Apple signed transaction>"
}
```

For Google Play use `store: "GooglePlay"` and `receipt` containing the existing Unity
Google receipt envelope. Cloud Code supplies player/project/environment from its
authenticated invocation context, never from client-selected identity fields.
Player IDs are bounded to 1–128 ASCII letters, digits, underscores or hyphens.

Only current publisher-verified evidence returns the exact PascalCase module contract:

```json
{
  "Valid": true,
  "PlayerBindingVerified": true,
  "CatalogVerified": true,
  "PlayerId": "<authenticated invocation player>",
  "ProjectId": "<configured UGS project ID>",
  "EnvironmentId": "<configured UGS environment ID>",
  "ProductId": "cascade.coins_240",
  "TransactionId": "<verified transaction>",
  "Platform": "AppleAppStore",
  "BundleId": "com.vickand.cascade"
}
```

Rejected evidence returns `{ "Valid": false }`. Missing or incorrect service
authorization returns 401 before calling a store; a project/environment mismatch
returns 403 and a malformed player ID returns 400. A store outage returns 503 so
Cloud Code and Unity can retain the pending purchase for retry.

Apple's official library verifies the signed JWS, app/environment, certificate chain
and online certificate status. Current transaction evidence is also fetched from
Apple to reject revocations. Google purchases are checked through the authenticated
Android Publisher API. Cancelled, pending, wrong app/product/order and multiple
quantity requests cannot grant inventory. Cloud Code commits the account grant and
the global transaction ownership receipt before Unity confirms or consumes a pending
order. The module owns idempotency and transaction ownership; this stateless verifier
does not issue currency. Consumables are never restored as new currency.

`PlayerBindingVerified` requires the authorized Cloud Code caller and a matching
Apple `appAccountToken` or Google `obfuscatedExternalAccountId` from verified store
evidence. Unity obtains the binding from `CascadeAccount.PurchaseIdentity` before
purchase; it is derived from the authenticated project/environment/player context.
Unbound legacy purchases need an explicit migration policy and are not assigned
to whichever account submits their receipt.

## Publisher setup

Current catalog/operator checklist: [release handoff, 15 October](../../Docs/RELEASE_HANDOFF_OCT15_RU.md). Six consumable coin packs and three auto-renewable no-ads subscriptions are active offers. The old lifetime ID in the historical setup below is restore-only. This stateless service does not yet implement incoming store notifications or AdMob SSV; deployment remains gated on the account module's atomic first-write/transaction-owner work.

1. Register `cascade.no_automatic_ads` (non-consumable), `cascade.coins_240` and
   `cascade.coins_480` (consumable) in both stores under `com.vickand.cascade`.
   Choose prices in store consoles; the client displays localized store prices.
2. Install Node 22+, run `npm ci`, then `npm test`.
3. Copy `.env.example` into the host's secret configuration. Configure `UGS_PROJECT_ID`
   and `UGS_ENVIRONMENT_ID` with the exact deployment IDs. Generate a private random
   `SERVICE_BEARER_TOKEN` of at least 32 characters and store the identical token in
   UGS Secret Manager for the Cloud Code module. Do not place it in Unity assets,
   client Remote Config or source control. Obtain root
   certificates from [Apple PKI](https://www.apple.com/certificateauthority/), an
   App Store Connect In-App Purchase API key, issuer/key IDs, and numeric app ID.
   Mount certificates and .p8 privately. Start in `Sandbox`; production hosting
   must use `Production` and must not silently accept Sandbox evidence.
4. Supply Application Default Credentials for a Google service account authorized
   for this application's orders in Play Console. Never embed keys in Unity.
5. Host behind HTTPS with request rate limits and an upstream concurrency limit.
   `PORT` defaults to 8080. The provided Docker container runs unprivileged; mount
   certificates and secrets externally.
6. Configure the Cloud Code purchase verifier URL as `https://<your-host>/verify`
   and its Secret Manager reference for the service token. Configure Unity's cloud
   purchase function, disable the test shop and mark `storeCatalogRegistered` after
   both catalogs are available. The old direct-client `receiptValidationUrl` flow
   cannot use this endpoint and must not receive the service token.
7. Sandbox-test purchase, cancel, approval delay, lost response, interrupted save,
   duplicate callback, relaunch, restore No ads, and store/account changes before
   enabling the production release switch.

Contract tests inject store replies and exercise authorization, context boundaries,
bounded identity and the exact module response. Live Apple/Google transactions,
deployed service credentials and refund delivery have not been tested. The service
never logs purchase evidence or secrets.

Primary APIs: [Apple server library](https://github.com/apple/app-store-server-library-node),
[Google purchases.products.get](https://developers.google.com/android-publisher/api-ref/rest/v3/purchases.products/get).
