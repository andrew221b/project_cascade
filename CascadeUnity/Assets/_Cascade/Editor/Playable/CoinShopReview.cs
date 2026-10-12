using System.Collections;
using Cascade.Application;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class CoinShopReview
    {
        public static void Capture(){var g=Object.FindFirstObjectByType<CascadeGame>();g.StartCoroutine(Matrix(g));}
        static IEnumerator Matrix(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.AdProvider.UseLocalPreviewForReview(true);
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("en");g.ShowShop();yield return Shot("shop-en",390,844);
            g.ShowCoinPacks();yield return Shot("coins-en",390,844);
            g.SetUiLocale("ru");g.Preferences.LargeText=true;g.ShowCoinPacks();yield return Shot("coins-small-ru",320,693);
            g.Preferences.LargeText=false;g.PurchaseStore.Buy(PurchaseCatalog.Coins240);yield return new WaitForSecondsRealtime(.20f);yield return Shot("coins-added-ru",390,844);
            g.SetUiLocale("uk");g.ShowCoinPacks();yield return Shot("coins-uk",390,844);
            g.SetEditorSafeInsets(Vector4.zero);g.ShowCoinPacks();yield return Shot("coins-wide",1440,900);
            g.ShowHome();g.SetUiLocale("en");Debug.Log("CASCADE coin shop captures complete.");
        }
        static IEnumerator Shot(string name,int width,int height){yield return null;yield return new WaitForSecondsRealtime(.30f);yield return PlayableCapture.CaptureSettled("AdMobStore",name+".png",width,height);}
    }
}
