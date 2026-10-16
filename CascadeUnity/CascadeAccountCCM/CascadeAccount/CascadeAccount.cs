using Cascade.Application;
using Cascade.Persistence;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Text;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace CascadeAccountModule;

public sealed class CascadeAccount
{
    public const string AccountKey = "cascade_account_v1";
    public const int MaxRequestBytes = 1536 * 1024, MaxAccountBytes = 4 * 1024 * 1024, MaxReplyBytes = 1536 * 1024;
    // A nonmatching lock must never be replaced by an unlocked write during first-account races.
    const string AbsentAccountLock = "c45c9c40f7734ae8885b8d2ecb01297f";
    readonly IGameApiClient api;
    readonly PurchaseVerifier verifier;
    public CascadeAccount(IGameApiClient gameApiClient, PurchaseVerifier purchaseVerifier)
    { api = gameApiClient; verifier = purchaseVerifier; }

    [CloudCodeFunction("ReadAccount")]
    public async Task<CloudSyncReply> ReadAccount(IExecutionContext context, string deviceId = "")
    {
        RequirePlayer(context);
        if (!string.IsNullOrEmpty(deviceId)) ValidateDevice(deviceId);
        var read = await Load(context);
        return Reply(read.State, read.Exists ? "Ok" : "Empty", deviceId);
    }

    [CloudCodeFunction("SyncAccount")]
    public async Task<CloudSyncReply> SyncAccount(IExecutionContext context, string deviceId, string requestId,
        long expectedRevision, string baseProfileJson, string localProfileJson)
    {
        RequirePlayer(context); ValidateDevice(deviceId);
        long sequence = ParseSequence(requestId);
        if (expectedRevision < 0 || Utf8Bytes(baseProfileJson) + Utf8Bytes(localProfileJson) + Utf8Bytes(deviceId) + Utf8Bytes(requestId) > MaxRequestBytes)
            throw new ArgumentException("Sync request exceeds its supported budget.");
        var read = await Load(context);
        var state = AccountJson.Clone(read.State);
        var device = state.DeviceStates.FirstOrDefault(d => d.DeviceId == deviceId);
        PlayerProfile local;
        try {
            local = AccountJson.Decode<PlayerProfile>(CloudProfilePayload.Unpack(localProfileJson)) ?? throw new InvalidDataException("Profile missing.");
            ScrubDeviceOnly(local);
            local.Validate();
        } catch (Exception ex) when (ex is JsonException || ex is InvalidDataException || ex is ArgumentException || ex is OverflowException || ex is NullReferenceException) {
            return Reply(state, "Rejected", deviceId, error: "InvalidProfile");
        }
        if(!PublishedCatalog.ValidProgress(local,state.Account.Profile))return Reply(state,"Rejected",deviceId,error:"UnverifiedProgress");
        string digest = AccountJson.Digest("sync|" + deviceId + "|" + requestId + "|" + AccountJson.Encode(local));
        var duplicate = state.Receipts.FirstOrDefault(r => r.DeviceId == deviceId && r.RequestId == requestId);
        if (duplicate != null)
            return duplicate.Digest == digest ? Reply(state, "Ok", deviceId, duplicate.EconomyConflict) : Reply(state, "Rejected", deviceId, error: "RequestIdReused");
        if (device != null && sequence <= device.LastSequence) return Reply(state, "Rejected", deviceId, error: "StaleSequence");
        if (expectedRevision != state.Account.Revision) return Reply(state, "Conflict", deviceId);
        CloudMergeResult result;
        bool joiningEconomyConflict = false;
        try {
            if (device != null) {
                // Caller-supplied baseProfileJson is never an economic authority.
                result = CloudReconciler.Merge(state.Account.Profile, device.ReadBaseline(state.Account.Profile.Monetization), local, PublishedCatalog.AllowedNodes, false);
            } else if (!read.Exists) {
                result = CloudReconciler.ImportLocal(local, PublishedCatalog.AllowedNodes);
            } else {
                var imported = CloudReconciler.ImportLocal(local, PublishedCatalog.AllowedNodes);
                joiningEconomyConflict = UnrepresentedEconomy(local.Monetization, state.Account.Profile.Monetization) || imported.EconomyConflict;
                result = CloudReconciler.Merge(state.Account.Profile, state.Account.Profile, imported.Profile, PublishedCatalog.AllowedNodes, false, retainRemoteEconomy: true);
            }
        } catch (Exception ex) when (ex is InvalidDataException || ex is ArgumentException || ex is OverflowException) {
            return Reply(state, "Rejected", deviceId, error: "InvalidProgress");
        }
        state.Account.Profile = result.Profile;
        ScrubDeviceOnly(state.Account.Profile);
        state.Account.Revision = checked(state.Account.Revision + 1);
        if (device == null && state.DeviceStates.Count == Cascade.Persistence.CloudAccount.MaxDevices) {
            var oldest = state.DeviceStates.OrderBy(d => state.Account.Devices.FirstOrDefault(c => c.DeviceId == d.DeviceId)?.UpdatedUnixSeconds ?? 0)
                .ThenBy(d => d.DeviceId, StringComparer.Ordinal).First();
            state.DeviceStates.Remove(oldest);
            state.Account.Devices.RemoveAll(d => d.DeviceId == oldest.DeviceId);
        }
        state.Account.UpsertDevice(DeviceCheckpoint.Capture(deviceId, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), local));
        if (device == null) { device = new DeviceSyncState { DeviceId = deviceId }; state.DeviceStates.Add(device); }
        SaveBaseline(device, state.Account.Profile); device.LastSequence = sequence;
        bool economyConflict = result.EconomyConflict || joiningEconomyConflict;
        state.Receipts.Add(new SyncReceipt { DeviceId = deviceId, RequestId = requestId, Sequence = sequence,
            Digest = digest, Revision = state.Account.Revision, EconomyConflict = economyConflict });
        if (state.Receipts.Count > 256) state.Receipts.RemoveRange(0, state.Receipts.Count - 256);
        return await Commit(context, read, state, deviceId, economyConflict);
    }

    [CloudCodeFunction("PurchaseIdentity")]
    public PurchaseIdentity PurchaseIdentityForPlayer(IExecutionContext context)
    {RequirePlayer(context);return PurchaseIdentity.For(context.ProjectId,context.EnvironmentId,context.PlayerId);}

    [CloudCodeFunction("VerifyPurchase")]
    public async Task<CloudSyncReply> VerifyPurchase(IExecutionContext context, string productId, string transactionId,
        string platform, string receipt, string jws, string bundleId, string deviceId = "")
    {
        RequirePlayer(context);
        if (!string.IsNullOrEmpty(deviceId)) ValidateDevice(deviceId);
        if (!PurchaseCatalog.IsKnown(productId) || string.IsNullOrWhiteSpace(transactionId) || transactionId.Length > 180
            || string.IsNullOrWhiteSpace(bundleId) || bundleId.Length > 256 || platform is not ("AppleAppStore" or "GooglePlay")
            || string.IsNullOrWhiteSpace(receipt) && string.IsNullOrWhiteSpace(jws)
            || Utf8Bytes(productId) + Utf8Bytes(transactionId) + Utf8Bytes(receipt) + Utf8Bytes(jws) + Utf8Bytes(bundleId) > MaxRequestBytes)
            throw new ArgumentException("Invalid store purchase request.");
        var read = await Load(context);
        if (!read.Exists) return Reply(read.State, "Rejected", deviceId, error: "SyncBeforePurchase");
        string grantId = "store_" + AccountJson.Digest(platform + "|" + transactionId);
        var existing = read.State.Account.Profile.Monetization.Grants.FirstOrDefault(g => g.Id == grantId);
        bool subscription = PurchaseCatalog.IsNoAdsSubscription(productId);
        if (existing != null && !subscription)
            return existing.Product == productId ? Reply(read.State, "Ok", deviceId) : Reply(read.State, "Rejected", deviceId, error: "TransactionReused");
        if (existing != null && existing.Product != productId)
            return Reply(read.State, "Rejected", deviceId, error: "TransactionReused");
        var verified = await verifier.Verify(api, context, productId, transactionId, platform, receipt, jws, bundleId);
        if (verified == null) return Reply(read.State, "Rejected", deviceId, error: "PurchaseNotVerified");
        // Global ownership survives a later wallet write failure; only its owner can retry.
        if (existing == null && !await ClaimTransaction(context, productId, transactionId, platform, bundleId))
            return Reply(read.State, "Rejected", deviceId, error: "TransactionOwnership");
        var candidate = AccountJson.Clone(read.State);
        var ledger = candidate.Account.Profile.Monetization;
        int amount = PurchaseCatalog.Coins(productId);
        if (existing == null) ledger.Grants.Add(new GrantReceipt { Id = grantId, Kind = amount > 0 ? "store_coins" : subscription ? "subscription" : "no_auto", Product = productId, Amount = amount });
        if (subscription) ledger.SetSubscriptionExpiry(productId,verified.SubscriptionExpiresUnix);
        else if (amount > 0) ledger.BonusCoins = checked(ledger.BonusCoins + amount);
        else ledger.NoAutomaticAds = true;
        candidate.Account.Revision = checked(candidate.Account.Revision + 1);
        var device = candidate.DeviceStates.FirstOrDefault(d => d.DeviceId == deviceId);
        if (device != null) SaveBaseline(device, candidate.Account.Profile);
        return await Commit(context, read, candidate, deviceId, false);
    }

    async Task<bool> ClaimTransaction(IExecutionContext context, string productId, string transactionId, string platform, string bundleId)
    {
        string digest = AccountJson.Digest(platform + "|" + transactionId);
        string customId = "iap_" + digest.Substring(0, 46); // Cloud Save custom IDs are limited to 50 characters.
        async Task<PurchaseOwner> ReadOwner()
        {
            var loaded = await api.CloudSaveData.GetPrivateCustomItemsAsync(context, context.ServiceToken, context.ProjectId, customId);
            var item = loaded.Data.Results.FirstOrDefault(i => i.Key == "owner");
            return item == null ? null : AccountJson.Decode<PurchaseOwner>(item.Value.ToString());
        }
        bool IsOwner(PurchaseOwner owner) => owner != null && owner.Digest == digest && owner.PlayerId == context.PlayerId
            && owner.ProductId == productId && owner.Platform == platform && owner.BundleId == bundleId;
        var existing = await ReadOwner();
        if (existing != null) return IsOwner(existing);
        var claim = new PurchaseOwner { Digest = digest, PlayerId = context.PlayerId, ProductId = productId,
            Platform = platform, BundleId = bundleId, ClaimedUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
        try {
            await api.CloudSaveData.SetPrivateCustomItemAsync(context, context.ServiceToken, context.ProjectId, customId,
                new SetItemBody("owner", claim, AbsentAccountLock));
            return true;
        } catch (ApiException ex) when (ex.Response.StatusCode == HttpStatusCode.Conflict) {
            return IsOwner(await ReadOwner());
        } catch (ApiException ex) when (ex.Response.StatusCode == HttpStatusCode.BadRequest) {
            return false; // Never retry without a create guard.
        }
    }

    async Task<LoadedAccount> Load(IExecutionContext context)
    {
        var response = await api.CloudSaveData.GetProtectedItemsAsync(context, context.ServiceToken, context.ProjectId,
            context.PlayerId, new List<string> { AccountKey });
        var item = response.Data.Results.FirstOrDefault(i => i.Key == AccountKey);
        if (item == null) return new LoadedAccount { State = new StoredAccount() };
        string json = item.Value?.ToString() ?? throw new InvalidDataException("Account missing.");
        if (Utf8Bytes(json) > MaxAccountBytes) throw new InvalidDataException("Stored account exceeds supported budget.");
        var state = AccountJson.Decode<StoredAccount>(CloudProfilePayload.Unpack(json,MaxAccountBytes)) ?? throw new InvalidDataException("Account missing.");
        state.Validate();
        Canonicalize(state);
        if (string.IsNullOrEmpty(item.WriteLock)) throw new InvalidDataException("Account has no concurrency token.");
        return new LoadedAccount { State = state, WriteLock = item.WriteLock, Exists = true };
    }
    async Task<CloudSyncReply> Commit(IExecutionContext context, LoadedAccount loaded, StoredAccount state, string deviceId, bool conflict)
    {
        Canonicalize(state);
        state.Validate();
        string json = AccountJson.Encode(state);
        // Cloud Code responses have a 2 MiB limit; leave room for runtime output framing.
        string packed;
        try {packed=CloudProfilePayload.Pack(json);}
        catch(InvalidDataException){return Reply(loaded.State,"Rejected",deviceId,error:"AccountBudget");}
        if (Utf8Bytes(packed) > MaxAccountBytes || Utf8Bytes(JsonConvert.SerializeObject(Reply(state, "Ok", deviceId, conflict))) > MaxReplyBytes)
            return Reply(loaded.State, "Rejected", deviceId, error: "AccountBudget");
        try {
            await api.CloudSaveData.SetProtectedItemAsync(context, context.ServiceToken, context.ProjectId, context.PlayerId,
                new SetItemBody(AccountKey, packed==json?JsonConvert.DeserializeObject<object>(json):packed, loaded.Exists ? loaded.WriteLock : AbsentAccountLock));
            return Reply(state, "Ok", deviceId, conflict);
        } catch (ApiException ex) when (ex.Response.StatusCode == HttpStatusCode.Conflict) {
            var latest = await Load(context);
            if (!loaded.Exists && !latest.Exists)
                return Reply(latest.State, "Rejected", deviceId, error: "SafeInitializationUnavailable");
            return Reply(latest.State, "Conflict", deviceId);
        } catch (ApiException ex) when (!loaded.Exists && ex.Response.StatusCode == HttpStatusCode.BadRequest) {
            // Some Cloud Save versions may reject write locks for new keys. Never drop the lock.
            return Reply(new StoredAccount(), "Rejected", deviceId, error: "SafeInitializationUnavailable");
        }
    }
    static void SaveBaseline(DeviceSyncState device, PlayerProfile profile)
    { var baseline = AccountJson.Clone(profile); ScrubDeviceOnly(baseline); device.StoreBaseline(baseline,profile.Monetization); }
    static void ScrubDeviceOnly(PlayerProfile profile)
    {
        profile.CloudAppliedRequestId = null;
        profile.CloudAppliedEconomyConflict = false;
        profile.Preferences.NotificationsEnabled = false;
    }
    static void Canonicalize(StoredAccount state)
    {
        ScrubDeviceOnly(state.Account.Profile);
        foreach (var device in state.DeviceStates) {
            var baseline=device.ReadBaseline(state.Account.Profile.Monetization);
            ScrubDeviceOnly(baseline);
            device.StoreBaseline(baseline,state.Account.Profile.Monetization);
        }
        // Lean DeviceCheckpoint / ModeJourney DTOs have no local reply marker fields.
    }
    static CloudSyncReply Reply(StoredAccount state,string status,string deviceId,bool conflict=false,string error=null)
    {
        string json=AccountJson.Encode(state.Account);
        bool large=Utf8Bytes(json)>768*1024;
        return new() { Status=status,Account=large?null:state.Account,PackedAccount=large?CloudProfilePayload.Pack(json):null,EconomyConflict=conflict,Error=error,
            LastSequence=Math.Max(state.DeviceStates.FirstOrDefault(d=>d.DeviceId==deviceId)?.LastSequence??0,state.Receipts.Where(r=>r.DeviceId==deviceId).Select(r=>r.Sequence).DefaultIfEmpty(0).Max()) };
    }
    static int Utf8Bytes(string value) => string.IsNullOrEmpty(value) ? 0 : Encoding.UTF8.GetByteCount(value);
    static void RequirePlayer(IExecutionContext context)
    {
        if (string.IsNullOrEmpty(context.PlayerId) || string.IsNullOrEmpty(context.ProjectId) || string.IsNullOrEmpty(context.ServiceToken))
            throw new UnauthorizedAccessException("Authenticated player context required.");
    }
    static void ValidateDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || deviceId.Length > 128 || deviceId.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
            throw new ArgumentException("Invalid device identifier.");
    }
    static long ParseSequence(string requestId)
    {
        var parts = requestId?.Split(':');
        if (parts?.Length != 2 || !long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out long sequence)
            || sequence <= 0 || !Guid.TryParseExact(parts[1], "D", out _)) throw new ArgumentException("Request ID must be sequence:UUID.");
        return sequence;
    }
    static bool UnrepresentedEconomy(MonetizationLedger local, MonetizationLedger remote)
    {
        bool nonempty = local.SpentCoins > 0 || local.BonusCoins > 0 || local.HintTokens > 0 || local.WayfinderScrolls > 0 || local.UndoSpools > 0
            || local.ReturnHourglasses > 0 || local.SignalLanterns > 0 || local.LandingLenses > 0 || local.RepairKits > 0 || local.NoAutomaticAds
            || local.NoAdsWeekExpiresUnix > 0 || local.NoAdsMonthExpiresUnix > 0 || local.NoAdsYearExpiresUnix > 0
            || local.Cosmetics.Count > 0 || local.Grants.Count > 0 || local.PaidHintStates.Count > 0;
        if (!nonempty) return false;
        return local.SpentCoins != remote.SpentCoins || local.BonusCoins != remote.BonusCoins || local.HintTokens != remote.HintTokens
            || local.WayfinderScrolls != remote.WayfinderScrolls || local.UndoSpools != remote.UndoSpools
            || local.ReturnHourglasses != remote.ReturnHourglasses || local.SignalLanterns != remote.SignalLanterns || local.LandingLenses != remote.LandingLenses || local.RepairKits != remote.RepairKits
            || local.NoAutomaticAds != remote.NoAutomaticAds || local.NoAdsWeekExpiresUnix != remote.NoAdsWeekExpiresUnix
            || local.NoAdsMonthExpiresUnix != remote.NoAdsMonthExpiresUnix || local.NoAdsYearExpiresUnix != remote.NoAdsYearExpiresUnix
            || !local.Cosmetics.ToHashSet().SetEquals(remote.Cosmetics)
            || !local.PaidHintStates.ToHashSet().SetEquals(remote.PaidHintStates) || local.Grants.Count != remote.Grants.Count
            || local.Grants.Any(g => !remote.Grants.Any(r => r.Id == g.Id && r.Kind == g.Kind && r.Product == g.Product && r.Amount == g.Amount));
    }
    sealed class LoadedAccount
    {
        public StoredAccount State;
        public string WriteLock;
        public bool Exists;
    }
    sealed class PurchaseOwner
    {
        public string Digest { get; set; }
        public string PlayerId { get; set; }
        public string ProductId { get; set; }
        public string Platform { get; set; }
        public string BundleId { get; set; }
        public long ClaimedUnixSeconds { get; set; }
    }
}
