using Cascade.Presentation;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using Cascade.Persistence;
using UnityEngine;
namespace Cascade.Tests {
 public sealed class NativeLoginPlatformTests {
  [TestCase("identityPreparations")][TestCase("purchaseVerifications")]
  public void SignInWaitsForWarmupWithoutInterruptingPaidGrant(string operation)
  {
   string directory=Path.Combine(Path.GetTempPath(),"CascadeQueuedSignIn",Guid.NewGuid().ToString("N"));
   var settings=ScriptableObject.CreateInstance<CloudPlayerSettings>();settings.enabledForPlayers=true;
   settings.projectId=UnityEngine.Application.cloudProjectId;settings.environment="production";settings.playerAccountsConfigured=true;
   try{
    using(var client=new CloudSyncClient(settings,Path.Combine(directory,"journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"})){
     typeof(CloudSyncClient).GetField(operation,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(client,1);
     bool payment=operation=="purchaseVerifications";
     Assert.That(client.CanLink,Is.EqualTo(!payment),"A paid grant owns the identity until confirmation; background warmup does not reject a tap.");
     client.LinkAccount();client.Tick();
     Assert.That(client.CanCancelSignIn,Is.EqualTo(!payment));
     Assert.That(client.State,Is.EqualTo(payment?CloudSyncState.LocalOnly:CloudSyncState.Connecting));
     Assert.That(client.LastFailureCode,Is.Null);Assert.That(client.CompletingSignIn,Is.False);
     if(!payment)client.CancelSignIn();Assert.That(client.CanCancelSignIn,Is.False);
    }
   }finally{UnityEngine.Object.DestroyImmediate(settings);if(Directory.Exists(directory))Directory.Delete(directory,true);}
  }

  [Test] public void IPhoneOnlyOffersGameCenterEvenBeforeServicesStart(){Assert.That(CloudPlatformAccounts.DisplayNameFor(RuntimePlatform.IPhonePlayer,null),Is.EqualTo("Game Center"));Assert.That(CloudPlatformAccounts.DisplayNameFor(RuntimePlatform.IPhonePlayer,"Google Play Games"),Is.EqualTo("Game Center"));Assert.That(CloudPlatformAccounts.RequiresNativeAccount(RuntimePlatform.IPhonePlayer),Is.True);}
  [Test] public void AndroidOnlyOffersPlayGamesEvenBeforeServicesStart(){Assert.That(CloudPlatformAccounts.DisplayNameFor(RuntimePlatform.Android,null),Is.EqualTo("Google Play Games"));Assert.That(CloudPlatformAccounts.RequiresNativeAccount(RuntimePlatform.Android),Is.True);}
  [Test] public void EditorKeepsItsExplicitProvider(){Assert.That(CloudPlatformAccounts.DisplayNameFor(RuntimePlatform.OSXEditor,"Game Center"),Is.EqualTo("Game Center"));Assert.That(CloudPlatformAccounts.RequiresNativeAccount(RuntimePlatform.OSXEditor),Is.False);}
 }
}
