#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using Cascade.Application;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed partial class AccountReferenceTests
    {
        [UnityTest] public IEnumerator VerifiedVideoRewardUpdatesTheVisibleWalletBeforeTheToast()
        {
            game.ShowHome();game.ShowShop();yield return null;Assert.That(game.TestMonetization,Is.False);
            int before=game.ShopCoins;var ledger=game.Monetization;
            var ticket=ledger.TryRewarded(new AdContext{Surface=AdSurface.Inventory,CoreAccepted=true,PrivacyReady=true,ProviderEnabled=true},"coins",true);
            Assert.That(ticket,Is.Not.Null);ledger.Opened(ticket.Id);
            Assert.That(ledger.RewardConfirmed(ticket.Id,"verified-ad-ui-test",true),Is.True);ledger.Closed(ticket.Id);
            Assert.That(game.ShopCoins,Is.EqualTo(before+40));game.ShowAdReward(ticket);
            var wallet=game.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Reference shop wallet count");
            Assert.That(wallet.text,Is.EqualTo((before+40).ToString()),"The header updates in the same callback, without leaving the shop");
            Assert.That(ledger.RewardConfirmed(ticket.Id,"verified-ad-ui-test",true),Is.False);
            Assert.That(game.ShopCoins,Is.EqualTo(before+40));
        }
        [UnityTest] public IEnumerator CoinReceiptKeepsReadableFontContrastAndCenteringAcrossMenuRefreshes()
        {
            game.ShowHome();game.ShowShop();game.ReducedMotion=true;
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetGameLanguage(locale);
                while((bool)typeof(CascadeGame).GetField("applyingUiLanguage",Private).GetValue(game))yield return null;
                foreach(var size in new[]{new Vector2Int(320,693),new Vector2Int(390,844),new Vector2Int(768,1024)}){
                    WelcomePhone(size.x,size.y);game.Preferences.LargeText=true;game.ShowShop();yield return null;
                    game.ShowCoinReward(480,true);yield return null;
                    for(int pass=0;pass<3;pass++){
                        typeof(CascadeGame).GetMethod("FinishThemedTypeStyles",Private).Invoke(game,null);
                        typeof(CascadeGame).GetMethod("ApplyMenuSpriteCounters",Private).Invoke(game,null);yield return null;
                    }
                    var toast=(RectTransform)Field("coinToast");var label=toast.GetComponentInChildren<TextMeshProUGUI>();label.ForceMeshUpdate();
                    string expected=locale=="ru"?"Получено 480 монет":locale=="uk"?"Отримано 480 монет":"480 coins added";
                    Assert.That(label.text,Is.EqualTo(expected));Assert.That(label.isTextOverflowing||label.isTextTruncated,Is.False,label.text);
                    Assert.That(label.GetComponent<InlineIllustratedDigits>(),Is.Null,"The transient receipt must keep one consistent font/material");
                    Assert.That(label.fontSharedMaterial.mainTexture,Is.EqualTo(label.font.material.mainTexture));
                    Assert.That(label.color.grayscale,Is.LessThan(.25f));Assert.That(toast.GetComponent<Image>().color,Is.EqualTo(Color.white));
                    var icon=toast.GetComponentsInChildren<Image>().Single(i=>i.name=="Received coin pouch");
                    var image=ScreenRect(icon.rectTransform);var text=ScreenRect(label.rectTransform);
                    Assert.That(image.Overlaps(text),Is.False,"Receipt icon and caption retain their gap");
                    var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/StoreFeedbackOct27"));Directory.CreateDirectory(folder);
                    Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=portrait;
                    var pixels=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);
                    pixels.ReadPixels(new Rect(0,0,size.x,size.y),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,"receipt-"+locale+"-"+size.x+".png"),pixels.EncodeToPNG());
                    RenderTexture.active=old;UnityEngine.Object.Destroy(pixels);
                }
            }
        }
    }
}
#endif
