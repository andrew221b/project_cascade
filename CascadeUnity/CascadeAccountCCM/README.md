# CascadeAccount Cloud Code module

Design note: this deployment stores one `cascade_account_v1` Protected Player Data aggregate with account schema, up to eight coherent device checkpoints, server-owned economic baselines and monotonically increasing active-device sequences. The latest 256 request receipts support idempotency; retained device sequences reject older requests after receipt eviction. A ninth installation replaces the oldest baseline/checkpoint. An evicted device returns through the new-device path: it adopts the remote economy, merges only monotonic earned progress, and archives conflicting pending local economy. Old active economic deltas cannot be replayed after eviction. This permits lifetime reinstalls without deleting the account. Device notification consent is always removed from cloud state.

`CloudAppliedRequestId` and `CloudAppliedEconomyConflict` are local atomic-save recovery markers. The server JSON contract excludes them on both deserialization and serialization; canonical account/baseline objects also explicitly reset them. Incoming markers cannot influence merge hashes or cloud progress. Read, duplicate, conflict and purchase replies reset accidental legacy markers; the next successful account write removes them from stored legacy JSON. Lean checkpoints never contain these fields.

`ReadAccount(context, deviceId = "")` returns `Status`, `Account`, `LastSequence`. `SyncAccount(context, deviceId, requestId, expectedRevision, baseProfileJson, localProfileJson)` returns those fields plus `EconomyConflict` and `Error`. `LastSequence` takes the maximum of the active baseline sequence and any retained receipts for that device, including after checkpoint eviction. The client must adopt the remote economy and archive pending local economy when a reply's sequence is newer than its pending request. Field names are PascalCase; function argument names are camelCase. Request IDs are `<positive sequence>:<UUID>`. Only `context.PlayerId`, `ProjectId`, `EnvironmentId` and `ServiceToken` identify the caller; no body player ID is accepted. `baseProfileJson` is budgeted but never trusted as an economic baseline.

First-account migration uses `ImportLocal`, excluding all store/ad grants, paid entitlements and bonuses. When another device joins an existing account, it merges earned progress while retaining the existing account economy. Subsequent offline deltas use that device's exact saved server baseline. A rejected economic merge reports a conflict so the client can archive local work. Published node IDs are embedded from the actual `Data/levels.json` and `Data/campaign_extension.json`; changing content requires rebuilding/redeploying the module. Individual sync/purchase payloads are limited to 1536 KiB and the complete stored aggregate to 4 MiB. Replies are separately limited to 1536 KiB, leaving space below the [Cloud Code 2 MiB response limit](https://docs.unity.com/en-us/cloud-code/modules/reference/limits). Budget rejection never commits the candidate or returns uncommitted progress.

Existing items always use Cloud Save write locks. Missing account/transaction keys use a synthetic nonmatching create guard. Unity's [write-lock documentation](https://docs.unity.com/en-us/cloud-save/concepts/write-locks) requires omitting the lock for a new item and documents that omission can overwrite an existing item; it does not promise atomic create-if-absent. Therefore guarded creation is an unverified deployment blocker, not a documented service guarantee. Smoke-test the selected environment: the API must accept a lock on a missing key and reject that guard after another creator wins. If it refuses, the module fails closed (`SafeInitializationUnavailable`) and never retries with an unlocked overwrite. Keep cloud sync and purchases disabled until this check passes or a trusted atomic provisioning/transaction store is implemented. In-process locks cannot solve races across Cloud Code workers.

## Build and deploy

Use .NET 9 SDK and `dotnet build CascadeAccount.sln -c Release`. `dotnet publish CascadeAccount/CascadeAccount.csproj -c Release -p:PublishProfile=FolderProfile` publishes the Linux x64 module. The module links pure Domain/Solver/Application/Content/Persistence sources; it has no UnityEngine dependency. Its NuGet references follow Unity's Cloud Code template: APIs 0.0.21, Core 0.0.1, Logging 7.0.1; Newtonsoft.Json 13.0.3 matches the persisted model.

In Unity, select the intended project and explicit environment, enable Authentication, Cloud Save and Cloud Code, then deploy `Assets/_Cascade/Cloud/CascadeAccount.ccmr` through Services > Deployment. It points at this solution. Protected Player Data and Private Custom Data writes must remain server-only. Preserve the client's project/environment identity guard. Cross-device recovery needs a linked identity; anonymous credentials alone cannot recover a removed installation.

The boundary checks run with `dotnet run --project CascadeAccount.Tests/CascadeAccount.Tests.csproj -c Release`. They use actual SDK types through a fake transport and are not a live Unity service verification. The test project is not publishable.

The wardrobe/chest expansion links the client's `ShopCatalog` and `JourneyRewards` recipe directly. Recipe0 receipts retain their original rewards; missing/zero `RecipeVersion` is omitted from JSON to preserve old device baseline checksums. New recipe1 receipts can grant a bounded tool through a matching canonical `parcel_tool_<Order>` grant. Sync accepts that earned grant without accepting unverified ad/store grants, and two devices claiming the same receipt merge one tool. Joining installations retain remote inventory and leave any unrepresented tool chest claim available to retry. Deploy the matching module and client together: previous clients cannot validate recipe1 tool chests. No live cloud deployment or guarded first-account creation has been validated by these local checks.

The shared catalog includes the earlier12 coin-only screen cosmetics and16 Mika looks. Those screen IDs reuse permanent cosmetic ownership and financed spending and do not enter the original chest recipes. Nullable `EquippedScenery` and `EquippedFrame` are independent of Mika's look and omitted when null, preserving legacy baseline JSON. Three-way sync adopts remote changes to unchanged local slots and retains locally changed slots, including unequip. A joining installation adopts remote slots; rejected ownership falls back to valid account equipment.

The October8 expansion adds6 Mika looks and4 screen styles, for38 trophies. Newly minted bonus receipts use recipe2; existing recipes0/1 and their claims remain unchanged. Recipe2 preserves both earlier cosmetic tracks and adds10 finds at60/96/168/204/276/312/384/420/492/528 unique clears. Chest-earned scenery owns and equips only its independent screen slot; new looks cannot use legacy paid cosmetic grants. Shared income is40 per unique published clear plus100 per ten published clears, regardless of route order. This resolves client/server milestone differences without removing previous income; the540-node catalog earns27000 before chest claims. Update/redeploy the matching module and client before using new IDs or recipe2. These local checks do not claim live deployment.

## Purchase verification

`VerifyPurchase(context, productId, transactionId, platform, receipt, jws, bundleId, deviceId = "")` accepts only existing `PurchaseCatalog` products and Unity store names `AppleAppStore` / `GooglePlay`. Configure these environment-scoped UGS Secret Manager values, granting Cloud Code read access:

- `CASCADE_RECEIPT_VERIFIER_URL`: HTTPS URL to the deployed receipt verifier's `/verify` endpoint.
- `CASCADE_RECEIPT_VERIFIER_TOKEN`: shared server bearer credential; matches the verifier's `SERVICE_BEARER_TOKEN`.

The module POSTs camelCase `{playerId, projectId, environmentId, productId, transactionId, store, receipt, jws, bundleId}` with a server Bearer header. The verifier must configure its actual project, environment, bundle/package and registered store products. It must return PascalCase `{Valid, PlayerBindingVerified, CatalogVerified, PlayerId, ProjectId, EnvironmentId, ProductId, TransactionId, Platform, BundleId}`. Every context/store/product field must match; booleans must be true. Missing config, redirects, timeout, oversized/invalid replies and mismatches all reject the grant. Secrets, receipts and raw transactions are not logged or stored in player data.

Before a wallet grant, the module claims global transaction ownership in Private Custom Data: custom ID `iap_<first 46 hex SHA256(platform|transactionId)>`, key `owner`; it retains the full hash to detect collisions. Only the same player/product/store/bundle owner can retry. A wallet CAS failure leaves ownership pending for safe retry. A different player cannot redeem the transaction. Each permanent account grant ID is `store_<full SHA256(platform|transactionId)>`; the amount comes from the unchanged server PurchaseCatalog, never the request/verifier response.

New purchases require the authenticated caller's binding in Apple `appAccountToken`
or Google `obfuscatedExternalAccountId`, returned by signed/publisher-verified evidence.
`PurchaseIdentity` derives that binding from UGS project/environment/player context.
Legacy unbound transactions are not silently reassigned. The global owner claim still
requires a proven atomic create store before production activation. Refund/revocation
ingestion remains unimplemented.

New offline clear claims carry three stage replays, matched to the embedded catalog,
localized source hash, exact published recipe, attempt identity and computed ratings.
Trusted existing receipts may be retained; unverified new ratings are rejected before
wallet writes. Old local progress remains readable; migration must attach valid proof
or use a separately authorized trusted policy.

`gz1:` bounded gzip payloads are accepted alongside old JSON. Device baselines and
large account replies can use optional packed fields. Decompression is capped at
8 MiB; no economic receipts are discarded. The legacy schema remains readable.

Server device baselines omit duplicate gameplay replay proofs and may reference a
hash-checked immutable prefix of the authoritative grant ledger. They reconstruct
all original economic receipts before checksum validation or three-way merging;
old full baselines and non-prefix histories remain readable. Receipt IDs and grant
amounts cannot be supplied through this reference mechanism by clients. The actual
1,074-clear/10,000-grant, eight-device stress case fits both storage and reply
budgets. It does not remove the ledger ceiling or prove live service concurrency.

When star and score bests come from different attempts, optional `ScoreReplay`
retains score evidence independently. Verification replays new claims for each
metric; already trusted legacy ratings remain usable without minting new clears.
Null evidence is omitted, preserving old save/baseline encodings.
