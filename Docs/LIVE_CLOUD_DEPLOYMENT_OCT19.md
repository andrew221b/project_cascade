# Verified update — 21 October 2026, 09:55 UTC

The earlier Secret Manager URL error is resolved. A fresh production check completed anonymous Authentication (200), `CascadeAccount/ReadAccount` (200) and disposable-player deletion (200). Protected hosted read also returns 200. Scheduler contains recurring `cascade-account-maintenance` at `0 * * * *`; Trigger `e61a3bf1-55a4-407d-886a-b2591c45d29c` invokes `urn:ugs:cloud-code:CascadeAccount/Maintenance`. No real player was modified.

Cloud/account infrastructure is therefore ready for an internal build. Real Apple/Google transactions, device OAuth callbacks, provider restore, RTDN/refunds and rewarded SSV still require store/device tests and are not inferred from the successful infrastructure smoke. Evidence: `Reports/ReleaseReadinessOct21/live-audit.json`.

---

# Historical update — 21 October 2026

This section supersedes the historical 20 October status below. Publisher credentials and the prepared service token are now installed server-side without exposing their contents. Protected hosted read: 200; health storage/publishersConfigured/deletionConfigured all true. Signed import permanently closed for this installation, temporary authorizer removed, route 404. Site version 7, env revision 7, source 9150c759fb45df80f40f7b48c733d8d2e1b7d090. DATA_KEY was not changed. Scheduler listing succeeds.

Fresh production Cloud Code diagnostic deployment and disposable-user check identify `Backend URL secret is not an absolute URL.` The required owner action is one URL field correction in production Secret Manager, described in Docs/CLOUD_REMAINING_MANUAL_OCT20_RU.md. The stored Unity token cannot be verified through Cloud Code until that URL is valid. Server token acceptance alone is not an end-to-end Unity check. Temporary test users were deleted. No new private files or roles requested.

40 backend tests, four actual local Worker checks and 157 Cloud Code boundary checks passed. Full maintenance scheduling, live CAS/deletion/purchase/provider callbacks and phone OAuth remain outstanding; activation flags stay disabled. Evidence: Reports/SecureConnectionOct21/verification.md.

---

# Unity Cloud activation — current result, 20 October 2026

The owner-supplied Unity pair is installed in the hosted masked `UGS_SERVICE_KEY_ID` / `UGS_SERVICE_SECRET` fields. It was verified through real disposable-user Cloud Save operations across all four access classes and Authentication deletion; no replacement key is needed. Previous detailed evidence: `Reports/UnityServerKeyOct20/verification.md`.

The backend SDK initialization fix and isolated retry processing are now published as Sites source version 6, runtime environment revision 5. Health is HTTP 200, storage=true, deletionConfigured=true, publishersConfigured=false. The former source-token publication block is resolved. DATA_KEY and the existing service bearer were preserved. No publisher private keys were imported: the stdout transfer was rejected before execution.

Existing Unity project: `a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`. Production environment: `2cf87fca-ee3b-46cc-acc5-b5cbf2ad12d7`; development: `7e2d1cd6-b8fa-49f8-a689-cbb2bf4abee8`. UPA Client ID: `7739ad6c-68cc-421f-9673-6eef1b436b68`. Retain account schema, IDs, prices and disabled production switches.

CascadeAccount is deployed in both environments with eight existing endpoints. Latest real production smoke: anonymous sign-in 200, PurchaseIdentity 200, ReadAccount 422/9009 missing Secret Manager data. Development PurchaseIdentity had a 504 cold call and ReadAccount the same missing-secret error. Both disposable users were deleted (200); no real user was modified. Scheduler list returned 403 using the provided service pair. Evidence: `Reports/CloudFinishOct20/cloudcode-live.json` and `scheduler-access.json`.

Required production secrets with Cloud Code access: `CASCADE_RECEIPT_VERIFIER_URL` and `CASCADE_RECEIPT_VERIFIER_TOKEN`. The latter must equal the hosted `SERVICE_BEARER_TOKEN`. The new 0600 `/Volumes/MY DISK/FlyingLadders/Credentials/Server/FlyingLadders-ServerConnection.json` contains a prepared URL/token pair, but neither hosted service nor Unity has been changed to that new token. It must be applied consistently before testing. Unity documents no public direct Secret Manager administration API; the CLI does not supply a secret-write command.

The Scheduler/Triggers management roles are still missing; a closed maintenance action, schedule and live retry run remain to be connected. A 403 listing schedules is not proof that an automatic retry job exists. Details and exact role names: `Docs/CLOUD_REMAINING_MANUAL_OCT20_RU.md`.

Existing validation from the earlier Unity pass remains: 68 actual-source client checks and 157 Cloud Code boundary checks, importer compilation in isolated Unity 6000.3.8f1, exact Client ID/openid-only scope and generated settings asset/meta copied back. No Unity source was changed by the current backend-only correction, so Unity compilation was not repeated.

The hosted Player Accounts authorization entry accepts openid and reaches its login page. Both game buttons currently open that same browser flow, where the player chooses a provider. A completed human Apple/Google sign-in, device callback, linked-account sync and restore remain untested. Full account read/create/concurrency/sync/delete and real publisher ownership/renewal/refund tests must follow connection. Client switches remain off until they pass. Advertising is excluded by the latest instruction; no store build, screenshot or submission was made.

Sources: [Unity Player Accounts](https://docs.unity.com/en-us/authentication/unity-player-accounts), [Secret Manager](https://docs.unity.com/en-us/services/secret-manager), [store secrets](https://docs.unity.com/en-us/services/secret-manager/tutorials/store-secrets), [Scheduler/Triggers access](https://docs.unity.com/en-us/triggers/authentication).
