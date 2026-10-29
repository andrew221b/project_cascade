using System.Reflection;
using Cascade.Presentation;
using NUnit.Framework;
using UnityEngine;
namespace Cascade.Tests
{
    public sealed class AdPreparationStatusTests
    {
        [Test] public void FinishedConsentRequestWithoutPermissionDoesNotPretendVideoIsLoading()
        {
            var root=new GameObject("Isolated ad status");
            try{
                var provider=root.AddComponent<NativeAdProvider>();provider.enabled=false;
                typeof(NativeAdProvider).GetField("privacyRequested",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(provider,true);
                Assert.That(provider.RewardLoading,Is.False,"Completing a consent request without ad permission is unavailable, not an outstanding video load");
            }finally{Object.DestroyImmediate(root);}
        }
    }
}
