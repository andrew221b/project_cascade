using System;
using System.IO;
using System.Reflection;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
namespace Cascade.Tests
{
    public sealed class CloudSchedulingTests
    {
        [TestCase(0)][TestCase(3)]public void RepeatedGameplaySavesCannotDefeatOfflineBackoffOrSuccessCooldown(int failureCount)
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeSyncSchedule",Guid.NewGuid().ToString("N"));
            try{
                using(var client=new CloudSyncClient(null,Path.Combine(directory,"journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"})){
                    var next=typeof(CloudSyncClient).GetField("next",BindingFlags.Instance|BindingFlags.NonPublic);
                    typeof(CloudSyncClient).GetField("failures",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(client,failureCount);
                    typeof(CloudSyncClient).GetField("dirty",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(client,false);
                    foreach(int delay in new[]{30,120}){
                        double deadline=Time.realtimeSinceStartupAsDouble+delay;next.SetValue(client,deadline);
                        for(int move=0;move<100;move++)client.Queue();client.Tick();
                        Assert.That((double)next.GetValue(client),Is.GreaterThanOrEqualTo(deadline));
                        Assert.That(client.State,Is.EqualTo(CloudSyncState.LocalOnly));
                    }
                }
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]public void ContinuousGameplayChangesKeepTheFirstCoalescingDeadline()
        {
            string directory=Path.Combine(Path.GetTempPath(),"CascadeSyncSchedule",Guid.NewGuid().ToString("N"));
            try{
                using(var client=new CloudSyncClient(null,Path.Combine(directory,"journal.json"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"})){
                    var next=typeof(CloudSyncClient).GetField("next",BindingFlags.Instance|BindingFlags.NonPublic);
                    typeof(CloudSyncClient).GetField("dirty",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(client,false);
                    double before=Time.realtimeSinceStartupAsDouble;client.Queue();double deadline=(double)next.GetValue(client);
                    Assert.That(deadline,Is.GreaterThanOrEqualTo(before+3));
                    for(int move=0;move<100;move++)client.Queue();
                    Assert.That((double)next.GetValue(client),Is.EqualTo(deadline));
                }
            }finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
    }
}
