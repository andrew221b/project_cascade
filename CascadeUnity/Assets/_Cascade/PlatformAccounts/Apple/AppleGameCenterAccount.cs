using System;
using System.Threading.Tasks;
using Apple.GameKit;
using Apple.Core.Runtime;
using Cascade.Presentation;
using Unity.Services.Authentication;
using UnityEngine;
namespace Cascade.PlatformAccounts
{
    public sealed class AppleGameCenterAccount:ICloudPlatformAccount
    {
        Task<CloudPlatformProof> pending;bool cancelled;
        public string DisplayName=>"Game Center";
        public bool Available=>true;
        public bool IsLinked(PlayerInfo player)=>!string.IsNullOrEmpty(player?.GetAppleGameCenterId());
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register(){
#if UNITY_IOS && !UNITY_EDITOR
            CloudPlatformAccounts.Factory=()=>new AppleGameCenterAccount();
#endif
        }
        public Task<CloudPlatformProof> AuthenticateAsync()
        {
            if(pending==null||pending.IsCompleted)pending=Authenticate();
            return pending;
        }
        async Task<CloudPlatformProof> Authenticate()
        {
            try{
                if(!GKLocalPlayer.Local.IsAuthenticated){
                    if(cancelled)throw new CloudPlatformSignInException("GameCenterSignInRequired",2);
                    var signal=new CloudPlatformAuthenticationSignal();
                    void Updated(GKLocalPlayer player)=>signal.Authenticated(player.IsAuthenticated);
                    void Rejected(NSError error)=>signal.Rejected(error.Code);
                    GKLocalPlayer.AuthenticateUpdate+=Updated;GKLocalPlayer.AuthenticateError+=Rejected;
                    try{
                        var initial=await GKLocalPlayer.Authenticate();
                        // The official wrapper can resolve when it presents the login
                        // controller. Await the later authenticated/error callback.
                        signal.Authenticated(initial.IsAuthenticated);
                        if(await Task.WhenAny(signal.Completion,Task.Delay(175000))!=signal.Completion)
                            throw new CloudPlatformSignInException("SignInTimeout",0);
                        long error=await signal.Completion;
                        if(error!=0){
                            if(error==(long)GKErrorCode.Cancelled){cancelled=true;throw new CloudPlatformSignInException("SignInCancelled",error);}
                            throw new CloudPlatformSignInException("GameCenterUnavailable",error);
                        }
                    }finally{GKLocalPlayer.AuthenticateUpdate-=Updated;GKLocalPlayer.AuthenticateError-=Rejected;}
                }
                cancelled=false;
                var player=GKLocalPlayer.Local;
                if(!player.IsAuthenticated)throw new CloudPlatformSignInException("GameCenterSignInRequired",0);
                var items=await player.FetchItems();
                return new Proof(Convert.ToBase64String(items.GetSignature()),player.TeamPlayerId,items.PublicKeyUrl,Convert.ToBase64String(items.GetSalt()),items.Timestamp);
            }catch(GameKitException e){
                if(e.GKErrorCode==GKErrorCode.Cancelled){cancelled=true;throw new CloudPlatformSignInException("SignInCancelled",e.Code);}
                throw new CloudPlatformSignInException("GameCenterUnavailable",e.Code);
            }
        }
        public Task ShowAccountAsync()=>GKAccessPoint.Shared.Trigger(GKGameCenterViewControllerState.LocalPlayerProfile);
        sealed class Proof:CloudPlatformProof
        {
            readonly string signature,playerId,url,salt;readonly ulong timestamp;
            public Proof(string signature,string playerId,string url,string salt,ulong timestamp){this.signature=signature;this.playerId=playerId;this.url=url;this.salt=salt;this.timestamp=timestamp;}
            public override bool MatchesLinkedPlayer(PlayerInfo player)=>!string.IsNullOrEmpty(playerId)&&string.Equals(player?.GetAppleGameCenterId(),playerId,StringComparison.Ordinal);
            public override Task LinkAsync()=>AuthenticationService.Instance.LinkWithAppleGameCenterAsync(signature,playerId,url,salt,timestamp);
            public override Task SignInAsync()=>AuthenticationService.Instance.SignInWithAppleGameCenterAsync(signature,playerId,url,salt,timestamp);
        }
    }
}
