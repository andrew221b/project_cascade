# CASCADE purchase verifier

Prepared service, not deployed. No store credentials, products or endpoint exist yet.
Unity sends its product ID, transaction ID, app bundle, Apple JWS or Google receipt
to `POST /verify`. Only publisher-verified evidence returns
`{ "valid": true, "productId": "…", "transactionId": "…" }`.

Apple's official library verifies the signed JWS, app/environment, certificate chain
and online certificate status. Current transaction evidence is also fetched from
Apple to reject revocations. Google purchases are checked through the authenticated
Android Publisher API. Cancelled, pending, wrong app/product/order and multiple
quantity requests cannot grant inventory. Unity saves the grant before confirming
or consuming a pending order. Consumables are never restored as new currency.

## Publisher setup

1. Register `cascade.no_automatic_ads` (non-consumable), `cascade.coins_240` and
   `cascade.coins_480` (consumable) in both stores under `com.vickand.cascade`.
   Choose prices in store consoles; the client displays localized store prices.
2. Install Node 22+, run `npm ci`, then `npm test`.
3. Copy `.env.example` into the host's secret configuration. Obtain root
   certificates from [Apple PKI](https://www.apple.com/certificateauthority/), an
   App Store Connect In-App Purchase API key, issuer/key IDs, and numeric app ID.
   Mount certificates and .p8 privately. Start in `Sandbox`; production hosting
   must use `Production` and must not silently accept Sandbox evidence.
4. Supply Application Default Credentials for a Google service account authorized
   for this application's orders in Play Console. Never embed keys in Unity.
5. Host behind HTTPS with request rate limits and an upstream concurrency limit.
   `PORT` defaults to 8080. The provided Docker container runs unprivileged; mount
   certificates and secrets externally.
6. Set Unity `receiptValidationUrl` to `https://<your-host>/verify`, disable the test
   shop and mark `storeCatalogRegistered` after both catalogs are available.
7. Sandbox-test purchase, cancel, approval delay, lost response, interrupted save,
   duplicate callback, relaunch, restore No ads, and store/account changes before
   enabling the production release switch.

Contract tests inject verified store replies; live Apple/Google transactions have
not been tested. The service never logs purchase evidence or secrets. A store
outage returns a failure; Unity retains the pending order for retry.

This service authenticates purchases, not game accounts. The local-profile receipt
ledger prevents accidental duplicate grants on that profile. It is not a server
wallet or cross-device ownership database and cannot stop hostile save editing or
proof sharing. Before adding cloud leaderboards or synchronizing paid currency,
bind purchases to authenticated players and persist a server receipt ledger with
a unique store/transaction key, plus refund notifications. This prepared service
is not complete anti-fraud infrastructure.

Primary APIs: [Apple server library](https://github.com/apple/app-store-server-library-node),
[Google purchases.products.get](https://developers.google.com/android-publisher/api-ref/rest/v3/purchases.products/get).
