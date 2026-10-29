using Cascade.Application;
using Microsoft.Extensions.Logging;
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
    readonly IAccountBackend backend;
    readonly IGameApiClient api;
    readonly PurchaseVerifier verifier;
    readonly ILogger logger;
    public CascadeAccount(IGameApiClient gameApiClient, PurchaseVerifier purchaseVerifier, IAccountBackend accountBackend, ILogger<CascadeAccount> logger = null)
    { api = gameApiClient; verifier = purchaseVerifier; backend = accountBackend; this.logger=logger; }

    // Called only by trusted service accounts/Triggers; authenticated players cannot run global maintenance.
    [CloudCodeFunction("Maintenance")]
    public Task<MaintenanceResult> Maintenance(IExecutionContext context)
    {
        return ServerExecution.Run<MaintenanceResult>("Maintenance",logger,()=>{
        if(context==null||!string.IsNullOrEmpty(context.PlayerId)||string.IsNullOrEmpty(context.ProjectId)
            ||string.IsNullOrEmpty(context.EnvironmentId)||string.IsNullOrEmpty(context.ServiceToken))
            throw new UnauthorizedAccessException("Trusted service context required.");
        return backend.Send<MaintenanceResult>(api,context,"maintenance");
    
        });
    }

    [CloudCodeFunction("ReadAccount")]
    public async Task<CloudSyncReply> ReadAccount(IExecutionContext context, string deviceId = "")
    {
        return await ServerExecution.Run<CloudSyncReply>("ReadAccount",logger,async ()=>{
        RequirePlayer(context);
        if (!string.IsNullOrEmpty(deviceId)) ValidateDevice(deviceId);
        var read = await Load(context);
        return Reply(read.State, read.Exists ? "Ok" : "Empty", deviceId);
    
        });
    }

    [CloudCodeFunction("SyncAccount")]
    public async Task<CloudSyncReply> SyncAccount(IExecutionContext context, string deviceId, string requestId,
        long expectedRevision, string baseProfileJson, string localProfileJson)
    {
        return await ServerExecution.Run<CloudSyncReply>("SyncAccount",logger,async ()=>{
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
    
        });
    }

    [CloudCodeFunction("PurchaseIdentity")]
    public PurchaseIdentity PurchaseIdentityForPlayer(IExecutionContext context)
    {RequirePlayer(context);return PurchaseIdentity.For(context.ProjectId,context.EnvironmentId,context.PlayerId);}

    [CloudCodeFunction("VerifyPurchase")]
    public async Task<CloudSyncReply> VerifyPurchase(IExecutionContext context, string productId, string transactionId,
        string platform, string receipt, string jws, string bundleId, string deviceId = "")
    {
        return await ServerExecution.Run<CloudSyncReply>("VerifyPurchase",logger,async ()=>{
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
        if(subscription&&await UnityBackendRouter.Mode(api,context)=="Unity"){
            await new UnityOwnedAccountBackend(api,context).RememberSubscription(context.PlayerId,platform,productId,verified);
            // Remembering a subscription advances the same atomic snapshot. Reload
            // the latest version before committing the wallet, keeping CAS honest.
            read=await Load(context);
            existing=read.State.Account.Profile.Monetization.Grants.FirstOrDefault(g=>g.Id==grantId);
        }
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
    
        });
    }

    async Task<bool> ClaimTransaction(IExecutionContext context,string productId,string transactionId,string platform,string bundleId)
        => (await backend.Send<ClaimResult>(api,context,"purchase/claim",new{productId,transactionId,platform,bundleId})).Claimed;

    [CloudCodeFunction("CreateRewardIntent")]
    public async Task<RewardIntent> CreateRewardIntent(IExecutionContext context,string kind,string subject,string unit){
        return await ServerExecution.Run<RewardIntent>("CreateRewardIntent",logger,async ()=>{
        RequirePlayer(context);var loaded=await Load(context);if(!loaded.Exists)throw new InvalidOperationException("Sync before requesting an ad.");
        if(kind is not ("coins" or "hint" or "parcel_open" or "parcel_bonus"))throw new ArgumentException("Unknown reward.");
        if(kind is "parcel_open" or "parcel_bonus"){
            if(subject==null||!subject.StartsWith("parcel:")||!int.TryParse(subject.Substring(7),out int order))throw new ArgumentException("Unknown parcel.");
            var parcel=loaded.State.Account.Profile.Rewards.Parcels.FirstOrDefault(p=>p.Order==order&&p.Kind=="loot");
            var grants=loaded.State.Account.Profile.Monetization.Grants;
            if(parcel==null||kind=="parcel_open"&&grants.Any(g=>g.Kind=="parcel_open"&&g.Product==subject)||kind=="parcel_bonus"&&(!parcel.Claimed||grants.Any(g=>g.Kind=="coins"&&g.Product==subject)))throw new InvalidOperationException("Reward unavailable.");
        }else subject="";
        return await backend.Send<RewardIntent>(api,context,"reward/intent",new{kind,subject,unit});
    
        });
    }
    [CloudCodeFunction("ReadReward")]
    public async Task<CloudSyncReply> ReadReward(IExecutionContext context,string intentId,string deviceId=""){
        return await ServerExecution.Run<CloudSyncReply>("ReadReward",logger,async ()=>{
        RequirePlayer(context);if(!string.IsNullOrEmpty(deviceId))ValidateDevice(deviceId);
        if(intentId==null||intentId.Length!=32)throw new ArgumentException("Invalid reward intent.");
        var confirmed=(await backend.Send<RewardStatus>(api,context,"reward/status",new{id=intentId})).ReceiptId;
        // Once a confirmed receipt is returned, load its committed effects before
        // returning the wallet. A callback arriving between reads cannot expose
        // an acknowledged reward with an older balance.
        var read=await Load(context);var result=Reply(read.State,"Ok",deviceId);
        result.RewardId=confirmed;return result;
    
        });
    }
    [CloudCodeFunction("GetDeletionCode")]
    public async Task<DeletionResult> GetDeletionCode(IExecutionContext context){
        return await ServerExecution.Run<DeletionResult>("GetDeletionCode",logger,async ()=>{RequirePlayer(context);return await backend.Send<DeletionResult>(api,context,"account/deletion-code");
        });
    }
    [CloudCodeFunction("DeleteAccount")]
    public async Task<DeletionResult> DeleteAccount(IExecutionContext context,string confirmation){
        return await ServerExecution.Run<DeletionResult>("DeleteAccount",logger,async ()=>{RequirePlayer(context);if(confirmation!="DELETE")throw new ArgumentException("Confirmation required.");return await backend.Send<DeletionResult>(api,context,"account/delete");
        });
    }

    async Task<LoadedAccount> Load(IExecutionContext context)
    {
        for(int retry=0;retry<5;retry++){
            var record=await backend.Send<AccountRecord>(api,context,"account/read");
            string json=record.Payload;bool exists=json!=null;
            if(!exists){
                // Import a legacy protected save once. SQL's atomic insert chooses the winner.
                var response=await api.CloudSaveData.GetProtectedItemsAsync(context,context.ServiceToken,context.ProjectId,context.PlayerId,new List<string>{AccountKey});
                var item=response.Data.Results.FirstOrDefault(i=>i.Key==AccountKey);json=item?.Value?.ToString();exists=json!=null;
            }
            var state=json==null?new StoredAccount():AccountJson.Decode<StoredAccount>(CloudProfilePayload.Unpack(json,MaxAccountBytes));
            if(state==null)throw new InvalidDataException("Account missing.");state.Validate();Canonicalize(state);
            var effects=record.Effects??await backend.Send<AccountEffects>(api,context,"account/effects");bool changed=false;var ledger=state.Account.Profile.Monetization;
            foreach(var reward in effects.Rewards){
                string id="ssv_"+reward.Id;if(ledger.Grants.Any(g=>g.Id==id))continue;
                string kind=reward.Kind=="parcel_bonus"?"coins":reward.Kind;int amount=kind=="parcel_open"?0:kind=="coins"?40:1;
                if(kind is not ("coins" or "hint" or "parcel_open")||reward.Id?.Length!=64)throw new InvalidDataException("Invalid server reward.");
                ledger.Grants.Add(new Cascade.Application.GrantReceipt{Id=id,Kind=kind,Amount=amount,Product=reward.Subject==""?null:reward.Subject});
                if(kind=="coins")ledger.BonusCoins=checked(ledger.BonusCoins+amount);else if(kind=="hint")ledger.HintTokens=checked(ledger.HintTokens+amount);changed=true;
            }
            foreach(var sub in effects.Subscriptions){if(!PurchaseCatalog.IsNoAdsSubscription(sub.Product)||sub.Expires<0)throw new InvalidDataException("Invalid subscription.");
                long prior=sub.Product==PurchaseCatalog.NoAdsWeek?ledger.NoAdsWeekExpiresUnix:sub.Product==PurchaseCatalog.NoAdsMonth?ledger.NoAdsMonthExpiresUnix:ledger.NoAdsYearExpiresUnix;
                if(prior!=sub.Expires){ledger.SetSubscriptionExpiry(sub.Product,sub.Expires);changed=true;}
            }
            if(changed){state.Account.Revision=checked(state.Account.Revision+1);state.Validate();Canonicalize(state);
                var write=await backend.Send<WriteResult>(api,context,"account/write",new{version=record.Version,payload=CloudProfilePayload.Pack(AccountJson.Encode(state))});if(!write.Written)continue;record.Version=write.Version;exists=true;}
            return new LoadedAccount{State=state,WriteLock=record.Version.ToString(CultureInfo.InvariantCulture),Exists=exists};
        }
        throw new InvalidOperationException("Account is busy; retry safely.");
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
        if(Utf8Bytes(packed)>1536*1024)return Reply(loaded.State,"Rejected",deviceId,error:"AccountBudget");
        var written=await backend.Send<WriteResult>(api,context,"account/write",new{version=long.Parse(loaded.WriteLock??"0",CultureInfo.InvariantCulture),payload=packed});
        if(written.Written)return Reply(state,"Ok",deviceId,conflict);
        var latest=await Load(context);
        return !loaded.Exists&&!latest.Exists?Reply(latest.State,"Rejected",deviceId,error:"SafeInitializationUnavailable"):Reply(latest.State,"Conflict",deviceId);
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
