using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
namespace CascadeAccountModule;

public sealed class AccountRecord { public long Version; public string Payload; public AccountEffects Effects; }
public sealed class WriteResult { public bool Written; public long Version; }
public sealed class ClaimResult { public bool Claimed; }
public sealed class AccountEffects { public List<RewardEffect> Rewards=new(); public List<SubscriptionEffect> Subscriptions=new(); }
public sealed class RewardEffect { public string Id,Kind,Subject; }
public sealed class SubscriptionEffect { public string Product; public long Expires; }
public sealed class RewardStatus { public string ReceiptId; }
public sealed class RewardIntent { public string Id,UserId; }
public sealed class DeletionResult { public string Status,RequestId,Code; public long ExpiresUnix; }
public sealed class MaintenanceResult { public string Status; public int Selected,Completed,Deferred,Failed; public long OldestDueAge; }
public interface IAccountBackend {
 Task<T> Send<T>(IGameApiClient api,IExecutionContext context,string path,object data=null);
}
