#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Cascade.Application;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
namespace Cascade.Tests
{
    public sealed partial class AccountReferenceTests
    {
        [UnityTest] public IEnumerator PurchaseFeedbackWaitsForForegroundAndSurvivesLateStoreRefresh()
        {
            game.ShowHome();game.ShowCoinPacks();yield return null;
            game.SendMessage("OnApplicationFocus",false);
            typeof(CascadeGame).GetMethod("QueuePurchasedCoins",Private).Invoke(game,new object[]{240});yield return null;
            Assert.That(Field("coinToast"),Is.Null,"A native payment sheet must not consume the confirmation toast");
            game.SendMessage("OnApplicationFocus",true);yield return null;
            var toast=(RectTransform)Field("coinToast");Assert.That(toast,Is.Not.Null);Assert.That(toast.gameObject.activeInHierarchy,Is.True);
            game.NotifyStoreChanged();yield return null;Assert.That((RectTransform)Field("coinToast"),Is.SameAs(toast));Assert.That(toast.gameObject.activeInHierarchy,Is.True,"Late store acknowledgement must not erase the visible confirmation");
            game.ShowHome();typeof(CascadeGame).GetMethod("QueuePurchasedCoins",Private).Invoke(game,new object[]{480});yield return null;
            Assert.That(((RectTransform)Field("coinToast")).gameObject.activeInHierarchy,Is.True,"A completed purchase still notifies after leaving the shop");
        }
        [UnityTest] public IEnumerator CoinPurchaseBusyCaptionsFitPhonesAndLocalesWithoutComingSoon()
        {
            game.ShowHome();
            foreach(string locale in new[]{"en","ru","uk"}){
                WelcomePhone(320,693);game.SetUiLocale(locale);
                typeof(NativePurchaseStore).GetMethod("Set",Private).Invoke(game.PurchaseStore,new object[]{PurchaseState.Busy});game.ShowCoinPacks();yield return null;game.RefreshLayout();yield return null;
                foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("store price "))){
                    label.ForceMeshUpdate();Assert.That(label.text,Does.Not.Contain("Coming soon"));Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,locale+": "+label.text);
                }
                Assert.That(game.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.name.StartsWith("Purchase cascade.")).All(b=>!b.interactable),Is.True);
            }
        }
    }
}
#endif
