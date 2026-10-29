# CascadeAccount Cloud Code module

Current design (27 October 2026): Unity Cloud Code owns the game backend. Private Cloud Save stores immutable snapshots, account pointers, reward intents, entitlements and transaction ownership; pre-existing shard write locks provide atomic CAS, including the first account write. The external HTTPS gateway is stateless and has no database binding or grant authority. Legacy `cascade_account_v1` Protected Player Data is read only for migration. The aggregate retains the account schema, up to eight coherent device checkpoints, server-owned economic baselines and monotonically increasing active-device sequences. The latest 256 request receipts support idempotency; retained device sequences reject older requests after receipt eviction. A ninth installation replaces the oldest baseline/checkpoint. An evicted device returns through the new-device path: it adopts the remote economy, merges only monotonic earned progress, and archives conflicting pending local economy. Old active economic deltas cannot be replayed after eviction. This permits lifetime reinstalls without deleting the account. Device notification consent is always removed from cloud state.

`CloudAppliedRequestId` and `CloudAppliedEconomyConflict` are local atomic-save recovery markers. The server JSON contract excludes them on both deserialization and serialization; canonical account/baseline objects also explicitly reset them. Incoming markers cannot influence merge hashes or cloud progress. Read, duplicate, conflict and purchase replies reset accidental legacy markers; the next successful account write removes them from stored legacy JSON. Lean checkpoints never contain these fields.

`ReadAccount(context, deviceId = "")` returns `Status`, `Account`, `LastSequence`. `SyncAccount(context, deviceId, requestId, expectedRevision, baseProfileJson, localProfileJson)` returns those fields plus `EconomyConflict` and `Error`. `LastSequence` takes the maximum of the active baseline sequence and any retained receipts for that device, including after checkpoint eviction. The client must adopt the remote economy and archive pending local economy when a reply's sequence is newer than its pending request. Field names are PascalCase; function argument names are camelCase. Request IDs are `<positive sequence>:<UUID>`. Only `context.PlayerId`, `ProjectId`, `EnvironmentId` and `ServiceToken` identify the caller; no body player ID is accepted. `baseProfileJson` is budgeted but never trusted as an economic baseline.

First-account migration uses `ImportLocal`, excluding all store/ad grants, paid entitlements and bonuses. When another device joins an existing account, it merges earned progress while retaining the existing account economy. Subsequent offline deltas use that device's exact saved server baseline. A rejected economic merge reports a conflict so the client can archive local work. Published node IDs are embedded from the actual `Data/levels.json` and `Data/campaign_extension.json`; changing content requires rebuilding/redeploying the module. Individual sync/purchase payloads are limited to 1536 KiB and the complete stored aggregate to 4 MiB. Replies are separately limited to 1536 KiB, leaving space below the [Cloud Code 2 MiB response limit](https://docs.unity.com/en-us/cloud-code/modules/reference/limits). Budget rejection never commits the candidate or returns uncommitted progress.

Account creation and updates publish a content-addressed immutable snapshot using a real Cloud Save CAS on an existing directory shard. Missing-key synthetic locks and unlocked mutable first writes are prohibited. Storage versions are monotonic and part of snapshot hashes. Transaction and subscription-chain owners use the same true CAS discipline. Packed profiles remain limited to1536 KiB; unpacked profiles to4 MiB; full snapshots to3 MiB. Three directory keys are individually capped at1536 KiB, below the5 MiB combined limit. Custom IDs fit Unity's64-character limit.

Production cutover is complete: five profiles, two owners and two deletion fingerprints verified; old records and sealed publisher credentials purged without a backup. The notification gateway has no D1 binding. Live concurrency and complete disposable-account lifecycle checks passed. Evidence: `Reports/UnityBackendMigrationOct27/verification.json`. Real store payment and AdMob console verification remain separate checks.

## Build and deploy

Use .NET 9 SDK and `dotnet build CascadeAccount.sln -c Release`. `dotnet publish CascadeAccount/CascadeAccount.csproj -c Release -p:PublishProfile=FolderProfile` publishes the Linux x64 module. The module links pure Domain/Solver/Application/Content/Persistence sources; it has no UnityEngine dependency. Its NuGet references follow Unity's Cloud Code template: APIs 0.0.21, Core 0.0.1, Logging 7.0.1; Newtonsoft.Json 13.0.3 matches the persisted model.

In Unity, select the intended project and explicit environment, enable Authentication, Cloud Save and Cloud Code, then deploy `Assets/_Cascade/Cloud/CascadeAccount.ccmr` through Services > Deployment. It points at this solution. With the UGS CLI, deploy the explicit `CascadeAccount.sln` instead: the CLI silently produces an empty plan for `.ccmr`. Do not use `--reconcile`. Protected Player Data and Private Custom Data writes must remain server-only. Preserve the client's project/environment identity guard. Cross-device recovery needs a linked identity; anonymous credentials alone cannot recover a removed installation.

The boundary checks run with `dotnet run --project CascadeAccount.Tests/CascadeAccount.Tests.csproj -c Release`. They use actual SDK types through a fake transport and are not a live Unity service verification. The test project is not publishable.

The wardrobe/chest expansion links the client's `ShopCatalog` and `JourneyRewards` recipe directly. Recipe0 receipts retain their original rewards; missing/zero `RecipeVersion` is omitted from JSON to preserve old device baseline checksums. New recipe1 receipts can grant a bounded tool through a matching canonical `parcel_tool_<Order>` grant. Sync accepts that earned grant without accepting unverified ad/store grants, and two devices claiming the same receipt merge one tool. Joining installations retain remote inventory and leave any unrepresented tool chest claim available to retry. Deploy the matching module and client together: previous clients cannot validate recipe1 tool chests. These local checks do not validate remote service concurrency or a complete account lifecycle.

The shared catalog includes the earlier12 coin-only screen cosmetics and16 Mika looks. Those screen IDs reuse permanent cosmetic ownership and financed spending and do not enter the original chest recipes. Nullable `EquippedScenery` and `EquippedFrame` are independent of Mika's look and omitted when null, preserving legacy baseline JSON. Three-way sync adopts remote changes to unchanged local slots and retains locally changed slots, including unequip. A joining installation adopts remote slots; rejected ownership falls back to valid account equipment.

The October8 expansion adds6 Mika looks and4 screen styles, for38 trophies. Newly minted bonus receipts use recipe2; existing recipes0/1 and their claims remain unchanged. Recipe2 preserves both earlier cosmetic tracks and adds10 finds at60/96/168/204/276/312/384/420/492/528 unique clears. Chest-earned scenery owns and equips only its independent screen slot; new looks cannot use legacy paid cosmetic grants. Shared income is40 per unique published clear plus100 per ten published clears, regardless of route order. This resolves client/server milestone differences without removing previous income; the540-node catalog earns27000 before chest claims. Update/redeploy the matching module and client before using new IDs or recipe2. These local checks do not claim live deployment.

## Purchase verification

`VerifyPurchase(context, productId, transactionId, platform, receipt, jws, bundleId, deviceId = "")` accepts the existing catalog and Unity store names. `UnityDirectPublisher` calls Apple's authenticated App Store Server API or Google's Android Publisher API inside Cloud Code. Client receipts select the lookup; client verdicts and JWS payloads are not authoritative. Bundle, product, store, quantity, transaction, revocation and context-derived account binding must match.

Publisher signing material is encrypted in Private Cloud Save. Its encryption master remains in the existing environment-scoped Unity Secret Manager value `CASCADE_RECEIPT_VERIFIER_TOKEN`. This secret's historical name is retained; it is now a vault master, not an external HTTP bearer. Do not rotate it without re-encrypting the vault. `CASCADE_RECEIPT_VERIFIER_URL` is obsolete and is not read by the module. No external verifier fallback remains.

Purchase identity derives the same Apple appAccountToken and Google obfuscatedExternalAccountId as before migration. Global purchase and chain ownership prevents reassignment. Verified Apple/Google notifications are forwarded to Unity; Cloud Code re-reads current publisher state instead of trusting a notification's expiry. ACTIVE, GRACE and valid cancelled periods retain access; expiry, hold or revocation remove it. A non-superseded chain's current expiry is aggregated by product.

Google's signed SSV callback is validated inside Unity using trusted ECDSA keys. A synthetic console probe acknowledges reachability without a grant. Served rewards require an issued, unexpired intent matching player, unit, timestamp and amount, plus immutable global transaction/nonce ownership. Reward amounts remain server-catalog values. External deletion codes and child-local purchase verification are thin HTTPS relays; validation and erasure execute in Unity.

Hourly Unity Scheduler invokes Maintenance. It visits the durable used-shard registry, refreshes subscription chains and continues deletion/retired-snapshot cleanup. Account deletion covers production and the historical development environment, and retains only replay fingerprints after user data/identity removal.

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
