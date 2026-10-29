// Test transport only. Compile the actual client against controllable SDK boundaries.
using Cascade.Persistence;
namespace UnityEngine {
    public class ScriptableObject { }
    public sealed class CreateAssetMenuAttribute:Attribute {public string menuName;}
    public sealed class TooltipAttribute:Attribute {public TooltipAttribute(string value){} }
    public static class Debug {public static void LogWarning(object message){} }
    public static class Time {public static double realtimeSinceStartupAsDouble;}
    public enum RuntimePlatform {OSXEditor,IPhonePlayer,Android}
    public static class Application {public static RuntimePlatform platform=RuntimePlatform.OSXEditor;public static string cloudProjectId="a1b8a760-5a0c-4c2d-8ae7-54c1b411259a";public static string LastUrl;public static void OpenURL(string url)=>LastUrl=url;}
}
namespace Unity.Services.Core {
    public enum ServicesInitializationState {Uninitialized,Initialized}
    public class RequestFailedException:Exception {public int ErrorCode;public RequestFailedException(int code,string message):base(message){ErrorCode=code;}}
    public sealed class InitializationOptions {public string Profile;public InitializationOptions SetProfile(string profile){Profile=profile;return this;}}
    public static class UnityServices {
        public static ServicesInitializationState State=ServicesInitializationState.Uninitialized;
        public static Func<Task> Initialize=()=>Task.CompletedTask;
        public static async Task InitializeAsync(InitializationOptions options){await Initialize();State=ServicesInitializationState.Initialized;Unity.Services.Authentication.AuthenticationService.Instance.Profile=options.Profile;}
    }
}
namespace Unity.Services.Core.Environments {public static class Options {public static Unity.Services.Core.InitializationOptions SetEnvironmentName(this Unity.Services.Core.InitializationOptions options,string name)=>options;}}
namespace Unity.Services.Authentication {
    public class AuthenticationException:Unity.Services.Core.RequestFailedException {public AuthenticationException(int code):base(code,"Authentication failure"){} }
    public static class AuthenticationErrorCodes {public const int AccountAlreadyLinked=100,AccountLinkLimitExceeded=101;}
    public sealed class PlayerInfo {public string UnityId;public string GetUnityId()=>UnityId;}
    public sealed class FakeAuthentication {
        public bool IsSignedIn;public string PlayerId="guest",Profile="default",AccessToken="test-only";public PlayerInfo PlayerInfo=new();
        public int AnonymousCalls;public Func<Task> Anonymous=()=>Task.CompletedTask;
        public int LinkCalls,ExistingCalls;public Func<Task> Link=()=>Task.CompletedTask;
        public readonly Dictionary<string,string> Sessions=new();
        public async Task SignInAnonymouslyAsync(){AnonymousCalls++;await Anonymous();IsSignedIn=true;PlayerId=Sessions.GetValueOrDefault(Profile,"guest");}
        public void SwitchProfile(string value){if(IsSignedIn)throw new InvalidOperationException("Cannot switch signed-in profile");Profile=value;}
        public void SignOut(bool clear=false){IsSignedIn=false;if(clear)Sessions.Remove(Profile);}
        public async Task LinkWithUnityAsync(string token){LinkCalls++;await Link();PlayerInfo.UnityId="linked";}
        public Task SignInWithUnityAsync(string token){ExistingCalls++;IsSignedIn=true;PlayerId="existing";Sessions[Profile]=PlayerId;PlayerInfo.UnityId="linked";return Task.CompletedTask;}
    }
    public static class AuthenticationService {public static FakeAuthentication Instance=new();}
}
namespace Unity.Services.Authentication.PlayerAccounts {
    public sealed class FakePlayerAccounts {
        public event Action SignedIn;public event Action<Unity.Services.Core.RequestFailedException> SignInFailed;
        public bool IsSignedIn;public string AccessToken="test-only",AccountPortalUrl="https://player-account.unity.com/";
        public int Starts,SignOuts;public Func<Task> Open=()=>Task.CompletedTask;
        public Task StartSignInAsync(){Starts++;return Open();}
        public void SignOut(){IsSignedIn=false;SignOuts++;}
        public void Succeed(){IsSignedIn=true;SignedIn?.Invoke();}
        public void Fail()=>SignInFailed?.Invoke(new Unity.Services.Core.RequestFailedException(400,"Browser failed"));
    }
    public static class PlayerAccountService {public static FakePlayerAccounts Instance=new();}
}
namespace Unity.Services.CloudCode {
    public sealed class FakeCloudCode {
        public int Calls;
        public Func<string,Dictionary<string,object>,Task<object>> AsyncHandler;
        public Func<string,Dictionary<string,object>,object> Handler=(name,args)=>name=="ReadAccount"?new Cascade.Presentation.CloudSyncReply{Status="Empty"}:throw new InvalidOperationException("Offline test transport");
        public Task<T> CallModuleEndpointAsync<T>(string module,string name,Dictionary<string,object> args){Calls++;return AsyncHandler==null?Task.FromResult((T)Handler(name,args)):Await<T>(AsyncHandler(name,args));}
        static async Task<T> Await<T>(Task<object> pending)=>(T)await pending;
    }
    public static class CloudCodeService {public static FakeCloudCode Instance=new();}
}

namespace Cascade.Presentation {
    public sealed class FakePlatformAccount:ICloudPlatformAccount {
        public string DisplayName=>"Game Center";public bool Available=>true;public bool Linked;public int AuthCalls,LinkCalls,SignInCalls;
        public Func<Task<CloudPlatformProof>> Authenticate;
        public Func<Task> Link=()=>Task.CompletedTask;
        public bool IsLinked(Unity.Services.Authentication.PlayerInfo player)=>Linked;
        public Task<CloudPlatformProof> AuthenticateAsync(){AuthCalls++;return Authenticate?.Invoke()??Task.FromResult<CloudPlatformProof>(new Proof(this));}
        public Task ShowAccountAsync()=>Task.CompletedTask;
        sealed class Proof:CloudPlatformProof {
            readonly FakePlatformAccount owner;public Proof(FakePlatformAccount owner){this.owner=owner;}
            public override bool MatchesLinkedPlayer(Unity.Services.Authentication.PlayerInfo player)=>owner.Linked;
            public override async Task LinkAsync(){owner.LinkCalls++;await owner.Link();owner.Linked=true;}
            public override async Task SignInAsync(){owner.SignInCalls++;await Unity.Services.Authentication.AuthenticationService.Instance.SignInWithUnityAsync("test-only-native-boundary");owner.Linked=true;}
        }
    }
}
