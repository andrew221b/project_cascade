using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
namespace Cascade.Presentation
{
    public interface ICloudPlatformAccount
    {
        string DisplayName {get;}
        bool Available {get;}
        bool IsLinked(PlayerInfo player);
        Task<CloudPlatformProof> AuthenticateAsync();
        Task ShowAccountAsync();
    }
    // Proofs live only in memory and are refreshed for each account operation.
    public abstract class CloudPlatformProof
    {
        public virtual bool MatchesLinkedPlayer(PlayerInfo player)=>false;
        public abstract Task LinkAsync();
        public abstract Task SignInAsync();
    }
    // A provider may first report that its system UI opened with no signed-in player.
    // Only an authenticated callback or an actual error finishes the operation.
    public sealed class CloudPlatformAuthenticationSignal
    {
        readonly TaskCompletionSource<long> result=new TaskCompletionSource<long>();
        public Task<long> Completion=>result.Task;
        public void Authenticated(bool signedIn){if(signedIn)result.TrySetResult(0);}
        public void Rejected(long code)=>result.TrySetResult(code==0?-1:code);
    }
    public sealed class CloudPlatformSignInException:Exception
    {
        public string Reason {get;}
        public long NativeCode {get;}
        public CloudPlatformSignInException(string reason,long nativeCode):base(reason){Reason=reason;NativeCode=nativeCode;}
    }
    public static class CloudPlatformAccounts
    {
        public static Func<ICloudPlatformAccount> Factory;
        public static bool RequiresNativeAccount(RuntimePlatform platform)=>platform==RuntimePlatform.IPhonePlayer||platform==RuntimePlatform.Android;
        public static string DisplayNameFor(RuntimePlatform platform,string registeredProvider)=>platform==RuntimePlatform.IPhonePlayer?"Game Center":platform==RuntimePlatform.Android?"Google Play Games":registeredProvider;
        public static ICloudPlatformAccount Create()=>Factory?.Invoke();
    }
}
