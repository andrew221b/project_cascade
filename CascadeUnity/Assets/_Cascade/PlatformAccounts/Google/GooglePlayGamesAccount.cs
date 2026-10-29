#if UNITY_ANDROID
using System;
using System.Threading.Tasks;
using Cascade.Presentation;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Unity.Services.Authentication;
using UnityEngine;
namespace Cascade.PlatformAccounts
{
    public sealed class GooglePlayGamesAccount:ICloudPlatformAccount
    {
        Task<CloudPlatformProof> pending;
        public string DisplayName=>"Google Play Games";
        public bool Available {get {var settings=PlayGamesSettings.LoadInstance();return settings!=null&&!string.IsNullOrWhiteSpace(settings.AppId)&&!string.IsNullOrWhiteSpace(settings.WebClientId);}}
        public bool IsLinked(PlayerInfo player)=>!string.IsNullOrEmpty(player?.GetGooglePlayGamesId());
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register(){
#if UNITY_ANDROID && !UNITY_EDITOR
            CloudPlatformAccounts.Factory=()=>new GooglePlayGamesAccount();
#endif
        }
        public Task<CloudPlatformProof> AuthenticateAsync()
        {
            if(pending==null||pending.IsCompleted)pending=Authenticate();
            return pending;
        }
        static Task<CloudPlatformProof> Authenticate()
        {
            var result=new TaskCompletionSource<CloudPlatformProof>();
            PlayGamesPlatform.DebugLogEnabled=false;
            PlayGamesPlatform.Instance.ManuallyAuthenticate(status=>{
                if(status!=SignInStatus.Success){result.TrySetException(new InvalidOperationException("Play Games authentication did not finish."));return;}
                PlayGamesPlatform.Instance.RequestServerSideAccess(false,code=>{
                    if(string.IsNullOrEmpty(code))result.TrySetException(new InvalidOperationException("Play Games returned no server authorization code."));
                    else result.TrySetResult(new Proof(code,PlayGamesPlatform.Instance.GetUserId()));
                });
            });
            return result.Task;
        }
        public Task ShowAccountAsync()
        {
            var result=new TaskCompletionSource<bool>();
            PlayGamesPlatform.Instance.ShowCompareProfileWithAlternativeNameHintsUI(PlayGamesPlatform.Instance.GetUserId(),null,null,_=>result.TrySetResult(true));
            return result.Task;
        }
        sealed class Proof:CloudPlatformProof
        {
            readonly string code,playerId;public Proof(string code,string playerId){this.code=code;this.playerId=playerId;}
            public override bool MatchesLinkedPlayer(PlayerInfo player)=>!string.IsNullOrEmpty(playerId)&&string.Equals(player?.GetGooglePlayGamesId(),playerId,StringComparison.Ordinal);
            public override Task LinkAsync()=>AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(code);
            public override Task SignInAsync()=>AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code);
        }
    }
}
#endif
