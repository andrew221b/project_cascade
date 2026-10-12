using Cascade.Application;
using Cascade.Persistence;
using CascadeAccountModule;
using System.Net;
using System.Reflection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Module = CascadeAccountModule.CascadeAccount;

var suite = new ModuleChecks();
if(args.Length==2&&args[0]=="--scale-only"){ModuleChecks.Scale(args[1]);return;}
await suite.Run();
if(args.Length==2&&args[0]=="--scale")ModuleChecks.Scale(args[1]);
Console.WriteLine("Cloud Code boundary checks: " + suite.Count + " PASS; fake SDK transport, no live Cloud Save claim.");

public sealed class ModuleChecks
{
    public int Count;
    readonly FakeTransport transport = new();
    readonly IExecutionContext alice = Context("alice"), bob = Context("bob");
    Module Module => new(transport.Client, new PurchaseVerifier());
    static IExecutionContext Context(string player) => (IExecutionContext)Proxy.Make(typeof(IExecutionContext), (m, _) => m.Name switch {
        "get_PlayerId" => player, "get_ProjectId" => "project", "get_EnvironmentId" => "development", "get_ServiceToken" => "trusted", _ => ""
    });
    void Check(bool condition, string name) { if (!condition) throw new Exception(name); Count++; }
    static string Request(int sequence) => sequence + ":" + Guid.NewGuid().ToString("D");
    static readonly Dictionary<string,ClearReceipt> earned=new();
    static (DescentRun Run,AttemptService Final,LevelCompletion Result) Delivered(string node,bool hint=false,int undos=0,DescentStage[] historical=null)
    {
        var source=PublishedCatalog.Primary(node);var run=historical==null?PublishedCatalog.Run(source.Definition):new DescentRun{Version=6,SourceHash=source.Definition.ContentHash,Stages=historical};
        AttemptService final=null;
        static Cascade.Domain.PuzzleCommand Parse(string c)=>c=="D"?Cascade.Domain.PuzzleCommand.Drop:c.StartsWith("R")?Cascade.Domain.PuzzleCommand.Reach(int.Parse(c.Substring(1).Split(':')[0]),int.Parse(c.Split(':')[1])):Cascade.Domain.PuzzleCommand.Swap(int.Parse(c));
        for(int stage=0;stage<3;stage++){
            final=new AttemptService(run.Level(source.Definition,stage),source.Locale);
            if(stage==0){
                for(int i=0;i<undos;i++){final.Execute(Parse(run.Stages[0].Witness[0]));if(!final.Undo())throw new Exception("Undo fixture must remain on its intact board");}
                if(hint&&!final.RevealHint(AttemptService.CalculateHint(final.CaptureHintRequest())))throw new Exception("Hint fixture must reveal a real hint");
            }
            foreach(string command in run.Stages[stage].Witness)final.Execute(Parse(command));
            if(stage<2)run.AcceptCheckpoint(source.Definition,final);
        }
        return (run,final,LevelCompletion.From(final,null,run.Performance(final)));
    }
    static PlayerProfile Cleared(string node)
    {
        if(!earned.TryGetValue(node,out var receipt)) {
            var source=PublishedCatalog.Primary(node);var run=PublishedCatalog.Run(source.Definition);AttemptService final=null;
            for(int stage=0;stage<3;stage++) {
                final=new AttemptService(run.Level(source.Definition,stage),source.Locale);
                foreach(string command in run.Stages[stage].Witness)final.Execute(command=="D"?Cascade.Domain.PuzzleCommand.Drop:command.StartsWith("R")?Cascade.Domain.PuzzleCommand.Reach(int.Parse(command.Substring(1).Split(':')[0]),int.Parse(command.Split(':')[1])):Cascade.Domain.PuzzleCommand.Swap(int.Parse(command)));
                if(stage<2)run.AcceptCheckpoint(source.Definition,final);
            }
            var profile=new PlayerProfile{Descent=run};profile.RecordClear(final,LevelCompletion.From(final,null,run.Performance(final)));
            receipt=profile.Clears.Single();earned[node]=receipt;
        }
        var p=new PlayerProfile();p.SelectJourney(new AttemptService(PublishedCatalog.Primary(node).Definition).Mode.ToString());p.Clears.Add(AccountJson.Clone(receipt));p.Rewards.Synchronize(new[]{node});return p;
    }
    static CloudSyncReply Expand(CloudSyncReply reply){if(reply.Account==null&&reply.PackedAccount!=null)reply.Account=AccountJson.Decode<CloudAccount>(CloudProfilePayload.Unpack(reply.PackedAccount));return reply;}
    async Task<CloudSyncReply> Sync(IExecutionContext context,string device,string request,long revision,string baseline,string local)=>Expand(await Module.SyncAccount(context,device,request,revision,baseline,local));
    async Task<CloudSyncReply> Read(IExecutionContext context,string device)=>Expand(await Module.ReadAccount(context,device));
    static string Upload(object value)
    {
        if(value is not PlayerProfile original)return AccountJson.Encode(value);
        var profile=AccountJson.Clone(original);var clears=profile.AllClears.ToArray();
        foreach(string mode in new[]{"Shapes","Words","Mixed"}) {
            profile.SelectJourney(mode);
            profile.Clears=clears.Where(c=>new AttemptService(PublishedCatalog.Primary(c.CampaignNodeId).Definition).Mode.ToString()==mode).ToList();
        }
        profile.SelectJourney(original.PreferredMode);return AccountJson.Encode(profile);
    }
    static PlayerProfile ClearedRange(int count)
    {
        var profile = new PlayerProfile();
        for (int index = 1; index <= count; index++) profile.Clears.AddRange(Cleared("C" + index.ToString("D2")).Clears);
        profile.Rewards.Synchronize(profile.Clears.Select(c => c.CampaignNodeId));return profile;
    }
    public static void Scale(string path)
    {
        var profile=AccountJson.Decode<PlayerProfile>(File.ReadAllText(path));profile.Validate();
        var state=new StoredAccount();state.Account.Profile=profile;state.Account.Revision=1;
        for(int i=0;i<CloudAccount.MaxDevices;i++){
            string id="scale_device_"+i;state.Account.UpsertDevice(DeviceCheckpoint.Capture(id,i+1,profile));
            var device=new DeviceSyncState{DeviceId=id,LastSequence=1};device.StoreBaseline(AccountJson.Clone(profile),profile.Monetization);state.DeviceStates.Add(device);
        }
        state.Validate();string json=AccountJson.Encode(state),packed=CloudProfilePayload.Pack(json);
        string reply=AccountJson.Encode(state.Account),packedReply=CloudProfilePayload.Pack(reply);
        if(System.Text.Encoding.UTF8.GetByteCount(packedReply)+1024>CascadeAccountModule.CascadeAccount.MaxReplyBytes)throw new Exception("Account reply exceeds supported transport budget");
        string restored=CloudProfilePayload.Unpack(packed,4*1024*1024);AccountJson.Decode<StoredAccount>(restored).Validate();
        var timer=System.Diagnostics.Stopwatch.StartNew();
        var merged=CloudReconciler.Merge(profile,state.DeviceStates[0].ReadBaseline(profile.Monetization),AccountJson.Clone(profile),PublishedCatalog.AllowedNodes);
        if(merged.EconomyConflict||merged.Profile.Monetization.Grants.Count!=profile.Monetization.Grants.Count)throw new Exception("Full-ledger merge lost economy");
        if(System.Text.Encoding.UTF8.GetByteCount(packed)>4*1024*1024||profile.AllClears.Any(c=>c.Replay==null)
            ||state.DeviceStates.Any(d=>d.ReadBaseline(profile.Monetization).Monetization.Grants.Count!=profile.Monetization.Grants.Count))throw new Exception("Account scale regression");
        Console.WriteLine("Eight-device account scale: "+System.Text.Encoding.UTF8.GetByteCount(json)+" decoded / "+System.Text.Encoding.UTF8.GetByteCount(packed)+" stored / "+System.Text.Encoding.UTF8.GetByteCount(packedReply)+" reply bytes; "+profile.AllClears.Count()+" proved clears, "+profile.Monetization.Grants.Count+" grants retained per baseline; full-ledger merge "+timer.ElapsedMilliseconds+" ms on this Mac.");
    }
    public async Task Run()
    {
        Check(PublishedCatalog.AllowedNodes.Count == 540 && PublishedCatalog.AllowedNodes.Contains("C540"), "actual embedded published catalog");
        Check((await Read(alice, "phone_a")).Status == "Empty", "absent account read");
        Check(!PublishedCatalog.ValidClear(new ClearReceipt{CampaignNodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64)},"Shapes"),"fabricated progress has no valid replay");
        Check(PublishedCatalog.ValidClear(Cleared("C01").Clears.Single(),"Shapes"),"three actual campaign stages verify against published recipes");
        var original=Delivered("C01",undos:2);var repeated=Delivered("C01",hint:true);
        Check(original.Result.Stars==3&&repeated.Result.Stars==2,"actual three-star first run and two-star replay");
        var replayPlayer=new PlayerProfile{Descent=original.Run};replayPlayer.RecordClear(original.Final,original.Result);replayPlayer.Rewards.Synchronize(new[]{"C01"});
        var replayBefore=AccountJson.Clone(replayPlayer);int initialCoins=replayPlayer.Rewards.EarnedCoins(PublishedCatalog.AllowedNodes);
        replayPlayer.Descent=repeated.Run;replayPlayer.RecordClear(repeated.Final,repeated.Result);
        Check(replayPlayer.Clears.Single().BestStars==3&&replayPlayer.Rewards.EarnedCoins(PublishedCatalog.AllowedNodes)==initialCoins+4,"worse replay gives four coins without lowering best stars");
        Check(PublishedCatalog.ValidProgress(replayPlayer,replayBefore),"both distinct full runs verify the capped replay credit");
        replayPlayer.RecordClear(repeated.Final,repeated.Result);
        Check(replayPlayer.Rewards.EarnedCoins(PublishedCatalog.AllowedNodes)==initialCoins+4,"duplicate result cannot repay replay credit");
        var doubled=CloudReconciler.Merge(replayPlayer,replayBefore,AccountJson.Clone(replayPlayer),PublishedCatalog.AllowedNodes);
        Check(doubled.Profile.Rewards.EarnedCoins(PublishedCatalog.AllowedNodes)==initialCoins+4,"two devices merge replay credit by maximum");
        var forgedReplay=AccountJson.Clone(replayPlayer);forgedReplay.Clears.Single().ReplayRewardStars=3;forgedReplay.Rewards.ReplayCredits.Single().Stars=3;
        Check(!PublishedCatalog.ValidProgress(forgedReplay,replayBefore),"replay stars cannot exceed the verified result");
        forgedReplay.Clears.Single().ReplayRewardProof=forgedReplay.Clears.Single().ReplayRewardPriorProof;
        Check(!PublishedCatalog.ValidProgress(forgedReplay,replayBefore),"one completion cannot pose as two different wins");
        var perfectReplay=Delivered("C01");var previousRepeat=AccountJson.Clone(replayPlayer);replayPlayer.Descent=perfectReplay.Run;replayPlayer.RecordClear(perfectReplay.Final,perfectReplay.Result);
        Check(replayPlayer.Rewards.EarnedCoins(PublishedCatalog.AllowedNodes)==initialCoins+10&&PublishedCatalog.ValidProgress(replayPlayer,previousRepeat),"improved replay reaches ten coins with both genuine proofs");
        Check(replayPlayer.Clears.Single().ReplayRewardProof==null&&replayPlayer.Clears.Single().ReplayRewardPriorProof!=null,"replay proof reuses the best result instead of duplicating stored evidence");
        using(var legacyStream=typeof(PublishedCatalog).Assembly.GetManifestResourceStream("CascadeAccount.Published.recipes_legacy.json")){
            using var legacyReader=new StreamReader(legacyStream);var legacy=AccountJson.Decode<Dictionary<string,DescentStage[]>>(legacyReader.ReadToEnd());
            var historic=Delivered("C187",historical:legacy["C187"]);var oldProfile=new PlayerProfile{Descent=historic.Run};oldProfile.RecordClear(historic.Final,historic.Result);
            Check(PublishedCatalog.ValidClear(oldProfile.Clears.Single(),"Shapes"),"old sixth-level clear survives the updated opening recipes");
            foreach(var archived in legacy.Where(pair=>pair.Key=="C251"||pair.Key.StartsWith("C251@",StringComparison.Ordinal))){
                var delivery=Delivered("C251",historical:archived.Value);var profile=new PlayerProfile{Descent=delivery.Run};profile.RecordClear(delivery.Final,delivery.Result);
                Check(PublishedCatalog.ValidClear(profile.Clears.Single(),"Shapes"),"each archived revision verifies its own full replay: "+archived.Key);
                profile.Clears.Single().Replay.Stages[0].ContentHash=new string('f',64);
                Check(!PublishedCatalog.ValidClear(profile.Clears.Single(),"Shapes"),"archived revisions reject a mismatched first-stage hash");
            }
        }
        var separate=Cleared("C01").Clears.Single();separate.BestScore=2000;separate.BestStars=4;
        foreach(var stage in separate.Replay.Stages)stage.ActiveSeconds=1000;
        Check(!PublishedCatalog.ValidClear(separate,"Shapes"),"improved score cannot borrow an insufficient primary proof");
        separate.ScoreReplay=AccountJson.Clone(separate.Replay);foreach(var stage in separate.ScoreReplay.Stages)stage.ActiveSeconds=1;
        Check(PublishedCatalog.ValidClear(separate,"Shapes"),"separate score proof replays and verifies the improved rating");
        separate.ScoreReplay=Cleared("C02").Clears.Single().Replay;
        Check(!PublishedCatalog.ValidClear(separate,"Shapes"),"score proof from another node cannot raise this record");
        var forged=new PlayerProfile();forged.Clears.Add(new ClearReceipt{CampaignNodeId="C01",AttemptId=Guid.NewGuid().ToString("D"),ContentHash=new string('a',64),BestStars=5,BestScore=2100});
        int beforeForgery=transport.Writes;
        var denied=await Sync(alice,"forged_phone",Request(1),0,"",Upload(forged));
        Check(denied.Status=="Rejected"&&denied.Error=="UnverifiedProgress"&&transport.Writes==beforeForgery,"unverified wallet rejected before any storage write");
        var proofProfile=Cleared("C01");var economicBase=new DeviceSyncState{DeviceId="proof_base"};
        economicBase.StoreBaseline(AccountJson.Clone(proofProfile));
        Check(economicBase.ReadBaseline().Clears.Single().Replay==null&&proofProfile.Clears.Single().Replay!=null,"economic baseline drops duplicate proof without mutating authoritative receipt");
        var ledger=new MonetizationLedger();ledger.Grants.Add(new GrantReceipt{Id="ad_prefix",Kind="coins",Amount=40});ledger.BonusCoins=40;
        var referenced=new DeviceSyncState{DeviceId="prefix"};referenced.StoreBaseline(new PlayerProfile{Monetization=AccountJson.Clone(ledger)},ledger);
        Check(referenced.GrantPrefixCount==1&&referenced.ReadBaseline(ledger).Monetization.Grants.Single().Id=="ad_prefix","server baseline reconstructs exact immutable receipts");
        ledger.Grants.Add(new GrantReceipt{Id="ad_append",Kind="coins",Amount=40});ledger.BonusCoins=80;
        Check(referenced.ReadBaseline(ledger).Monetization.Grants.Count==1,"appended server grants do not alter the old baseline");
        ledger.Grants[0].Amount=41;bool rejectedReference=false;
        try{referenced.ReadBaseline(ledger);}catch(InvalidDataException){rejectedReference=true;}
        Check(rejectedReference,"modified grant prefix is rejected before reconciliation");
        var local = Cleared("C01");local.Clears.AddRange(Cleared("C02").Clears);local.Rewards.Synchronize(new[] { "C02" });local.Preferences.NotificationsEnabled = true;
        var upload = Newtonsoft.Json.Linq.JObject.Parse(Upload(local));
        upload["CloudAppliedRequestId"] = "local_atomic_recovery_marker";upload["CloudAppliedEconomyConflict"] = true;
        string firstId = Request(1), json = upload.ToString(Newtonsoft.Json.Formatting.None);
        var created = await Sync(alice, "phone_a", firstId, 0, "", json);
        Check(created.Status == "Ok" && created.Account.Revision == 1 && created.LastSequence == 1, "bootstrap checkpoint committed");
        Check(!created.Account.Profile.Preferences.NotificationsEnabled && transport.LastPlayerId == "alice", "context identity and device consent");
        Check(created.Account.Profile.CloudAppliedRequestId == null && !created.Account.Profile.CloudAppliedEconomyConflict && !transport.ContainsLocalMarkers("alice"),
            "uploaded local recovery markers are absent from stored account/baselines and false in replies");
        transport.InjectLocalMarkers("alice");
        var legacyRead = await Read(alice, "phone_a");
        Check(legacyRead.Account.Profile.CloudAppliedRequestId == null && !legacyRead.Account.Profile.CloudAppliedEconomyConflict,
            "read replies scrub accidental legacy cloud markers");
        int writes = transport.Writes;
        var duplicate = await Sync(alice, "phone_a", firstId, 0, "", json);
        Check(duplicate.Status == "Ok" && duplicate.Account.Revision == 1 && transport.Writes == writes, "durable idempotency before revision conflict");
        var changed = Cleared("C02");
        Check((await Sync(alice, "phone_a", firstId, 1, "", Upload(changed))).Error == "RequestIdReused", "same request ID different payload rejected");
        Check((await Sync(alice, "phone_a", Request(1), 1, "", json)).Error == "StaleSequence", "monotonic sequence prevents receipt replay");
        Check((await Sync(alice, "phone_a", Request(2), 0, "", json)).Status == "Conflict", "stale account revision rejected");
        var offline = AccountJson.Clone(created.Account.Profile);offline.Monetization.SpentCoins = ShopCatalog.Price(TravelTool.UndoSpool);offline.Monetization.UndoSpools = 1;
        string forgedBase = Upload(Cleared("C184"));
        var spent = await Sync(alice, "phone_a", Request(2), 1, forgedBase, Upload(offline));
        Check(spent.Status == "Ok" && !spent.EconomyConflict && spent.Account.Profile.Monetization.UndoSpools == 1, "economic delta uses persisted server baseline");
        Check(!transport.ContainsLocalMarkers("alice"), "subsequent canonical save removes legacy markers without changing the economic baseline");
        var copied = await Sync(alice, "phone_b", Request(1), 2, "", Upload(offline));
        Check(copied.Status == "Ok" && copied.Account.Profile.Monetization.UndoSpools == 1, "new device cannot duplicate old tool purchases");
        transport.ForgetReceipts("alice");
        Check((await Sync(alice, "phone_a", Request(2), copied.Account.Revision, "", json)).Error == "StaleSequence", "receipt eviction does not permit old sequence replay");
        transport.ConflictOnNextWrite = true;
        var collision = await Sync(alice, "phone_a", Request(3), copied.Account.Revision, "", Upload(copied.Account.Profile));
        Check(collision.Status == "Conflict", "CAS conflict reloads account");
        var oversized = new string('x', Module.MaxRequestBytes + 1);
        try { await Sync(alice, "phone_a", Request(4), collision.Account.Revision, "", oversized); throw new Exception("budget not enforced"); }
        catch (ArgumentException) { Count++; }
        var beforePurchase = transport.Writes;
        transport.InjectLocalMarkers("alice");
        var purchase = await Module.VerifyPurchase(alice, PurchaseCatalog.Coins240, "transaction", "GooglePlay", "receipt", "", "com.cascade", "phone_a");
        Check(purchase.Error == "PurchaseNotVerified" && transport.Writes == beforePurchase, "missing verifier secrets cannot grant purchases");
        Check(purchase.Account.Profile.CloudAppliedRequestId == null && !purchase.Account.Profile.CloudAppliedEconomyConflict,
            "purchase replies cannot transfer legacy device-local recovery markers");
        var method = typeof(Module).GetMethod("ClaimTransaction", BindingFlags.Instance | BindingFlags.NonPublic)!;
        async Task<bool> Claim(IExecutionContext c) => await (Task<bool>)method.Invoke(Module, new object[] { c, PurchaseCatalog.Coins240, "same_transaction", "GooglePlay", "com.cascade" })!;
        Check(await Claim(alice), "global transaction owner created");
        Check(await Claim(alice), "same global owner can retry pending grant");
        Check(!await Claim(bob), "other player cannot redeem global transaction");
        transport.CreateRaceOnNextWrite = true;
        var race = await Sync(Context("carol"), "phone_c", Request(1), 0, "", json);
        Check(race.Status == "Conflict", "first-account create race never overwrites another creator");
        transport.RejectMissingGuard = true;
        var unavailable = await Sync(Context("dave"), "phone_d", Request(1), 0, "", json);
        Check(unavailable.Status == "Rejected" && unavailable.Error == "SafeInitializationUnavailable",
            "unsupported atomic creation is explicit and never writes without a guard");
        transport.RejectMissingGuard = false;
        var malformed=new PlayerProfile();malformed.SeenCoachPrompts.Add(new string('x',500000));
        int beforeMalformed=transport.Writes;
        Check((await Sync(Context("budget"),"budget_phone",Request(1),0,"",Upload(malformed))).Error=="InvalidProfile"&&transport.Writes==beforeMalformed,"oversized untrusted coach strings reject before persistence");
        var devicesContext = Context("devices");
        var deviceReply = await Sync(devicesContext, "device_00", Request(1), 0, "", Upload(offline));
        bool allDevicesJoined = deviceReply.Status == "Ok";
        for (int index = 1; index <= 8; index++) {
            deviceReply = await Sync(devicesContext, "device_" + index.ToString("D2"), Request(1), deviceReply.Account.Revision, "", Upload(new PlayerProfile()));
            allDevicesJoined &= deviceReply.Status == "Ok";
        }
        Check(allDevicesJoined && deviceReply.Account.Devices.Count == 8 && !deviceReply.Account.Devices.Any(d => d.DeviceId == "device_00"),
            "ninth installation replaces oldest checkpoint without a lifetime device lockout");
        Check((await Read(devicesContext, "device_00")).LastSequence == 1,
            "evicted device still reports sequence retained in the bounded receipt journal");
        var returningDevice = await Sync(devicesContext, "device_00", Request(2), deviceReply.Account.Revision, "", Upload(offline));
        Check(returningDevice.Status == "Ok" && returningDevice.Account.Devices.Count == 8 && returningDevice.Account.Profile.Monetization.UndoSpools == 1,
            "evicted device adopts remote economy and cannot duplicate old tool stock");
        var consumedOnUnregisteredPhone = AccountJson.Clone(offline);consumedOnUnregisteredPhone.Monetization.UndoSpools = 0;
        var differing = await Sync(devicesContext, "new_phone", Request(1), returningDevice.Account.Revision, "", Upload(consumedOnUnregisteredPhone));
        Check(differing.Status == "Ok" && differing.EconomyConflict && differing.Account.Profile.Monetization.UndoSpools == 1,
            "unregistered nonempty differing economy reports archive conflict while retaining server stock");

        Check(ShopCatalog.MikaTrophyIds.Length == 22 && ShopCatalog.TrophyIds.Length == 38 && ShopCatalog.TrophyPrice("explorer_hat") == 480
            && ShopCatalog.TrophyPrice("fox_mask") == 720 && ShopCatalog.TrophyPrice("frog_suit") == 960
            && ShopCatalog.TrophyPrice("star_knight") == 1200 && ShopCatalog.BonusTrophyIds.All(id => ShopCatalog.PurchaseClearMilestone(id) == 0),
            "linked server wardrobe catalog uses the client's published coin prices and no premium clear gates");
        var legacyRecipe = ClearedRange(84);
        foreach(var parcel in legacyRecipe.Rewards.Parcels)parcel.RecipeVersion=0;
        legacyRecipe.Rewards.Parcels = legacyRecipe.Rewards.Parcels.Select(p => p.Order <= 12 ? p : JourneyRewards.BonusParcel(p.Order, 0)).ToList();
        string legacyRecipeJson = AccountJson.Encode(legacyRecipe);
        Check(!legacyRecipeJson.Contains("RecipeVersion") && AccountJson.Hash(AccountJson.Decode<PlayerProfile>(legacyRecipeJson)) == AccountJson.Digest(legacyRecipeJson),
            "absent legacy recipe fields remain omitted and preserve stored cloud baseline hashes");
        var wardrobeContext = Context("wardrobe");var chestProfile = ClearedRange(48);
        var chestBaseline = await Sync(wardrobeContext, "chest_phone", Request(1), 0, "", Upload(chestProfile));
        Check(chestBaseline.Status == "Ok" && chestBaseline.Account.Profile.Rewards.Parcels.Single(p => p.Order == 16).Kind == "tool",
            "new deterministic tool chest is persisted with its earned progress");
        chestProfile = AccountJson.Clone(chestBaseline.Account.Profile);
        var chestService = new AdService(chestProfile.Monetization, next => chestProfile.Monetization = next);
        Check(chestService.TryClaimParcel(16, chestProfile.Rewards), "local tool chest inventory claim");
        string chestId = Request(2), chestJson = Upload(chestProfile);
        var chestClaim = await Sync(wardrobeContext, "chest_phone", chestId, 1, "", chestJson);
        Check(chestClaim.Status == "Ok" && !chestClaim.EconomyConflict && chestClaim.Account.Profile.Monetization.LandingLenses == 1
            && chestClaim.Account.Profile.Monetization.Grants.Count(g => g.Kind == "parcel_tool") == 1,
            "matching earned tool receipt can synchronize without a payment verifier");
        int chestWrites = transport.Writes;
        var chestReplay = await Sync(wardrobeContext, "chest_phone", chestId, 1, "", chestJson);
        Check(chestReplay.Status == "Ok" && transport.Writes == chestWrites && chestReplay.Account.Profile.Monetization.LandingLenses == 1,
            "replaying the tool claim request never grants a second tool");
        var chestJoin = await Sync(wardrobeContext, "joined_phone", Request(1), chestClaim.Account.Revision, "", chestJson);
        Check(chestJoin.Status == "Ok" && !chestJoin.EconomyConflict && chestJoin.Account.Profile.Monetization.LandingLenses == 1,
            "another registered installation retains the canonical tool claim");
        var pendingChest = AccountJson.Clone(chestJoin.Account.Profile);
        pendingChest.Clears.AddRange(ClearedRange(96).Clears.Skip(48));pendingChest.Rewards.Synchronize(pendingChest.Clears.Select(c => c.CampaignNodeId));
        pendingChest.Rewards.Parcels[pendingChest.Rewards.Parcels.FindIndex(p => p.Order == 32)] = JourneyRewards.BonusParcel(32, 1);
        Check(new AdService(pendingChest.Monetization, next => pendingChest.Monetization = next).TryClaimParcel(32, pendingChest.Rewards),
            "unregistered phone has a second pending earned chest");
        var pendingJoin = await Sync(wardrobeContext, "offline_phone", Request(1), chestJoin.Account.Revision, "", Upload(pendingChest));
        Check(pendingJoin.Status == "Ok" && pendingJoin.EconomyConflict && pendingJoin.Account.Profile.Monetization.LandingLenses == 1
            && pendingJoin.Account.Profile.Monetization.UndoSpools == 0 && !pendingJoin.Account.Profile.Rewards.Parcels.Single(p => p.Order == 32).Claimed,
            "new-device economy adoption preserves tools and leaves an unrepresented chest available to reclaim");
        var forgedChest = AccountJson.Clone(chestProfile);forgedChest.Monetization.Grants.Single(g => g.Kind == "parcel_tool").Product = "Wayfinder";
        Check((await Sync(wardrobeContext, "chest_phone", Request(3), pendingJoin.Account.Revision, "", Upload(forgedChest))).Error == "InvalidProfile",
            "a forged tool grant cannot change the chest's deterministic prize");
        var fragmentContext = Context("fragments");
        var remoteFragments = await Sync(fragmentContext, "fragment_a", Request(1), 0, "", Upload(ClearedRange(9)));
        var differingFragments = ClearedRange(18);
        foreach (var parcel in differingFragments.Rewards.Parcels) parcel.GiftId = parcel.Order <= 3 ? "sun" : "leaf";
        var fragmentMerge = await Sync(fragmentContext, "fragment_b", Request(1), remoteFragments.Account.Revision, "", Upload(differingFragments));
        Check(fragmentMerge.Status == "Ok" && fragmentMerge.EconomyConflict && fragmentMerge.Account.Profile.Rewards.Parcels.Count == 6
            && fragmentMerge.Account.Profile.Rewards.Parcels.Take(3).All(p => p.GiftId == "leaf"),
            "conflicting offline gift choices preserve remote receipts and reconstruct only missing earned orders");
        var completeFragments = AccountJson.Clone(fragmentMerge.Account.Profile);
        completeFragments.Clears.AddRange(ClearedRange(48).Clears.Skip(18));completeFragments.Rewards.Synchronize(completeFragments.Clears.Select(c => c.CampaignNodeId));
        var crossedBoundary = await Sync(fragmentContext, "fragment_b", Request(2), fragmentMerge.Account.Revision, "", Upload(completeFragments));
        Check(crossedBoundary.Status == "Ok" && !crossedBoundary.EconomyConflict && crossedBoundary.Account.Profile.Rewards.Parcels.Count(p => p.Order <= 12) == 12
            && crossedBoundary.Account.Profile.Rewards.Parcels.Single(p => p.Order == 16).Kind == "tool",
            "repaired fragment receipt capacity crosses into the bonus chest recipe without an invalid thirteenth fragment");

        Check(ShopCatalog.SceneryTrophyIds.Select(ShopCatalog.TrophyPrice).SequenceEqual(new[] { 480,600,720,840,720,960,840,1080,600,720,960,1080 })
            && ShopCatalog.FrameTrophyIds.Select(ShopCatalog.TrophyPrice).SequenceEqual(new[] { 720,840,960,1200 }),
            "linked client/server screen cosmetic prices match the coin catalog");
        Check(!legacyRecipeJson.Contains("EquippedScenery") && !legacyRecipeJson.Contains("EquippedFrame"),
            "missing screen slots remain omitted from legacy baseline JSON");
        var screenContext = Context("screens");var screenProfile = ClearedRange(60);
        var screenBase = await Sync(screenContext, "screen_a", Request(1), 0, "", Upload(screenProfile));
        screenProfile = AccountJson.Clone(screenBase.Account.Profile);
        var screenService = new AdService(screenProfile.Monetization, next => screenProfile.Monetization = next);
        Check(screenService.TryBuyKeepsake("petal_drift", screenProfile.Rewards.EarnedCoins(PublishedCatalog.AllowedNodes), screenProfile.Rewards)
            && screenService.TryBuyKeepsake("garden_frame", screenProfile.Rewards.EarnedCoins(PublishedCatalog.AllowedNodes), screenProfile.Rewards)
            && screenService.TryEquipScenery("petal_drift", screenProfile.Rewards) && screenService.TryEquipFrame("garden_frame", screenProfile.Rewards),
            "screen items purchase and equip using the existing durable coin economy");
        var screenSaved = await Sync(screenContext, "screen_a", Request(2), screenBase.Account.Revision, "", Upload(screenProfile));
        Check(screenSaved.Status == "Ok" && !screenSaved.EconomyConflict && screenSaved.Account.Profile.Rewards.EquippedScenery == "petal_drift"
            && screenSaved.Account.Profile.Rewards.EquippedFrame == "garden_frame" && screenSaved.Account.Profile.Monetization.SpentCoins == 1200,
            "independent screen slots and their financed ownership synchronize without duplicate spending");
        var screenJoined = await Sync(screenContext, "screen_b", Request(1), screenSaved.Account.Revision, "", Upload(new PlayerProfile()));
        Check(screenJoined.Status == "Ok" && !screenJoined.EconomyConflict && screenJoined.Account.Profile.Rewards.EquippedScenery == "petal_drift"
            && screenJoined.Account.Profile.Rewards.EquippedFrame == "garden_frame",
            "joining legacy/default profile adopts existing account screen slots");
        var forgedScreen = AccountJson.Clone(screenProfile);forgedScreen.Rewards.EquippedScenery = "firefly_dance";
        Check((await Sync(screenContext, "screen_a", Request(3), screenJoined.Account.Revision, "", Upload(forgedScreen))).Error == "InvalidProfile",
            "unowned screen equipment is rejected at the module boundary");
        forgedScreen = AccountJson.Clone(screenProfile);forgedScreen.Rewards.EquippedFrame = "petal_drift";
        Check((await Sync(screenContext, "screen_a", Request(3), screenJoined.Account.Revision, "", Upload(forgedScreen))).Error == "InvalidProfile",
            "a scenery ID cannot occupy the frame slot");

        Check(PurchaseCatalog.CoinPurchaseWalletThreshold == 1440 && PurchaseCatalog.CoinPurchaseWalletThreshold == ShopCatalog.MaximumTrophyPrice,
            "coin pack eligibility reaches the highest cosmetic price without changing store pack products");
        Check(ClearedRange(540).Rewards.EarnedCoins(PublishedCatalog.AllowedNodes) == 27000
            && ClearedRange(540).Rewards.Parcels.Count(p => p.Kind == "cosmetic") == 19,
            "shared full campaign income and recipe-two cosmetic pacing");
        using var routeStream = typeof(PublishedCatalog).Assembly.GetManifestResourceStream("CascadeAccount.Published.campaign_extension.json");
        using var routeReader = new StreamReader(routeStream);
        var firstRouteNodes = Newtonsoft.Json.Linq.JObject.Parse(routeReader.ReadToEnd())["journey_order"].Take(10).Select(n => (string)n).ToArray();
        var routeProfile = new PlayerProfile();foreach(var routeNode in firstRouteNodes)routeProfile.Clears.AddRange(Cleared(routeNode).Clears);
        routeProfile.Rewards.Synchronize(firstRouteNodes);
        Check(routeProfile.Rewards.EarnedCoins(PublishedCatalog.AllowedNodes) == 500
            && routeProfile.Rewards.EarnedCoins(firstRouteNodes.Reverse().ToArray()) == 500,
            "client reordered first-ten route and numeric server catalog earn the same milestone wallet");
        var routeContext = Context("reordered_wallet");
        var routeBase = await Sync(routeContext, "route_phone", Request(1), 0, "", Upload(routeProfile));
        routeProfile = AccountJson.Clone(routeBase.Account.Profile);
        Check(new AdService(routeProfile.Monetization, next => routeProfile.Monetization = next)
            .TryBuyKeepsake("petal_drift", routeProfile.Rewards.EarnedCoins(firstRouteNodes), routeProfile.Rewards),
            "reordered route first-ten income can finance a 480-coin cosmetic");
        var routeSaved = await Sync(routeContext, "route_phone", Request(2), routeBase.Account.Revision, "", Upload(routeProfile));
        Check(routeSaved.Status == "Ok" && !routeSaved.EconomyConflict && routeSaved.Account.Profile.Monetization.SpentCoins == 480,
            "server reconciliation retains a purchase financed by the reordered-route milestone");

        var earnedSceneContext = Context("earned_scene");
        var earnedSceneBase = await Sync(earnedSceneContext, "scene_a", Request(1), 0, "", Upload(ClearedRange(384)));
        var earnedSceneJoin = await Sync(earnedSceneContext, "scene_b", Request(1), earnedSceneBase.Account.Revision, "", Upload(new PlayerProfile()));
        var sceneA = AccountJson.Clone(earnedSceneBase.Account.Profile);var sceneB = AccountJson.Clone(earnedSceneJoin.Account.Profile);
        bool bothSceneClaims = true;
        foreach(var sceneProfile in new[]{sceneA,sceneB}){
            var sceneService = new AdService(sceneProfile.Monetization, next => sceneProfile.Monetization = next);
            bothSceneClaims &= sceneService.TryClaimParcel(128, sceneProfile.Rewards)
                && sceneService.TryEquipScenery("ribbon_kites", sceneProfile.Rewards) && sceneProfile.Rewards.EquippedGift == null;
        }
        Check(bothSceneClaims, "recipe-two earned scenery claims and equips without entering Mika's outfit slot");
        var earnedSceneA = await Sync(earnedSceneContext, "scene_a", Request(2), earnedSceneJoin.Account.Revision, "", Upload(sceneA));
        var earnedSceneB = await Sync(earnedSceneContext, "scene_b", Request(2), earnedSceneA.Account.Revision, "", Upload(sceneB));
        Check(earnedSceneB.Status == "Ok" && !earnedSceneB.EconomyConflict
            && earnedSceneB.Account.Profile.Rewards.EquippedScenery == "ribbon_kites"
            && earnedSceneB.Account.Profile.Rewards.Parcels.Count(p => p.Order == 128 && p.Claimed) == 1
            && earnedSceneB.Account.Profile.Monetization.Cosmetics.Count == 0,
            "two devices claiming earned scenery preserve one receipt and independent equipment without paid ownership");
        var sceneJoining = await Sync(earnedSceneContext, "scene_c", Request(1), earnedSceneB.Account.Revision, "", Upload(new PlayerProfile()));
        Check(sceneJoining.Status == "Ok" && !sceneJoining.EconomyConflict && sceneJoining.Account.Profile.Rewards.EquippedScenery == "ribbon_kites",
            "a joining installation adopts account scenery earned from a chest");
        var forgedExpansion = AccountJson.Clone(sceneA);forgedExpansion.Monetization.Grants.Add(new GrantReceipt{
            Id = "mock_new_outfit", Kind = "cosmetic", Product = "mushroom_cap" });
        Check((await Sync(earnedSceneContext, "scene_a", Request(3), sceneJoining.Account.Revision, "", Upload(forgedExpansion))).Error == "InvalidProfile",
            "new outfits cannot cross the server boundary as legacy paid cosmetic grants");
    }
}

public class Proxy : DispatchProxy
{
    public Func<MethodInfo, object[], object> Handler;
    protected override object Invoke(MethodInfo method, object[] args) => Handler(method, args);
    public static object Make(Type type, Func<MethodInfo, object[], object> handler)
    { var result = Create(type, typeof(Proxy));((Proxy)result).Handler = handler;return result; }
}
public sealed class FakeTransport
{
    readonly Dictionary<string, (object Value, string Lock)> items = new();
    public IGameApiClient Client;
    public int Writes;
    public string LastPlayerId;
    public bool ConflictOnNextWrite, CreateRaceOnNextWrite, RejectMissingGuard;
    public FakeTransport()
    {
        var data = Proxy.Make(typeof(IGameApiClient).GetProperty("CloudSaveData")!.PropertyType, Invoke);
        Client = (IGameApiClient)Proxy.Make(typeof(IGameApiClient), (m, _) => m.Name == "get_CloudSaveData" ? data : throw new InvalidOperationException("Unconfigured secrets"));
    }
    object Invoke(MethodInfo method, object[] args)
    {
        bool custom = method.Name.Contains("PrivateCustom");
        string owner = (string)args[3], key = custom ? "private:" + owner : "player:" + owner;
        if (!custom) LastPlayerId = owner;
        if (method.Name.StartsWith("Get")) {
            var result = new List<Item>();
            if (items.TryGetValue(key, out var stored)) result.Add(new Item(custom ? "owner" : Module.AccountKey,
                Newtonsoft.Json.Linq.JToken.Parse(AccountJson.Encode(stored.Value)), stored.Lock,
                new ModifiedMetadata(DateTime.UtcNow), new ModifiedMetadata(DateTime.UtcNow)));
            return Task.FromResult(Response(new GetItemsResponse(result, new GetItemsResponseLinks(""))));
        }
        var body = (SetItemBody)args[4];
        if (RejectMissingGuard && !items.ContainsKey(key)) throw Conflict();
        if (CreateRaceOnNextWrite && !items.ContainsKey(key)) { CreateRaceOnNextWrite = false;items[key] = (new StoredAccount(), "winning_creator"); }
        if (ConflictOnNextWrite) { ConflictOnNextWrite = false;throw Conflict(); }
        if (items.TryGetValue(key, out var current) && body.WriteLock != current.Lock) throw Conflict();
        if (string.IsNullOrEmpty(body.WriteLock)) throw new Exception("Production attempted an unlocked write.");
        string next = Guid.NewGuid().ToString("N");items[key] = (body.Value, next);Writes++;
        return Task.FromResult(Response(new SetItemResponse(next)));
    }
    public void ForgetReceipts(string player)
    {
        var entry = items["player:" + player];
        var state = AccountJson.Decode<StoredAccount>(CloudProfilePayload.Unpack(entry.Value is string packed?packed:AccountJson.Encode(entry.Value)));state.Receipts.Clear();
        items["player:" + player] = (state, entry.Lock);
    }
    public bool ContainsLocalMarkers(string player)
    {
        var json = Newtonsoft.Json.Linq.JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(items["player:" + player].Value));
        return json.SelectTokens("$..CloudAppliedRequestId").Any() || json.SelectTokens("$..CloudAppliedEconomyConflict").Any();
    }
    public void InjectLocalMarkers(string player)
    {
        var entry = items["player:" + player];
        var json = Newtonsoft.Json.Linq.JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(entry.Value));
        foreach (var profile in json.SelectTokens("$.Account.Profile").Concat(json.SelectTokens("$.DeviceStates[*].BaseProfile"))) {
            profile["CloudAppliedRequestId"] = "legacy_device_recovery_marker";profile["CloudAppliedEconomyConflict"] = true;
        }
        items["player:" + player] = (json, entry.Lock);
    }
    // The generated SDK response has read-only properties; populate its DTO backing fields in test transport only.
    static ApiResponse<T> Response<T>(T data)
    {
        object response = new ApiResponse<T>();
        typeof(ApiResponse<T>).GetField("<StatusCode>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(response, HttpStatusCode.OK);
        typeof(ApiResponse<T>).GetField("<Data>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(response, data);
        return (ApiResponse<T>)response;
    }
    static ApiException Conflict()
    {
        object response = new ApiResponse();
        typeof(ApiResponse).GetField("<StatusCode>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(response, HttpStatusCode.Conflict);
        return new ApiException((ApiExceptionType)0, "CAS conflict", (IApiResponse)response);
    }
}
