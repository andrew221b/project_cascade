#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Persistence;
using Cascade.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Cascade.Tests
{
    public sealed partial class AccountReferenceTests
    {
        [UnityTest] public IEnumerator AccountDoorOpensOnFirstTapWhileCatalogConnects()
        {
            Profile.AccountIntroSeen=true;
            foreach(string locale in new[]{"en","ru","uk"}){
                game.SetUiLocale(locale);game.ShowHome();yield return null;
                typeof(NativePurchaseStore).GetProperty("State").SetValue(game.PurchaseStore,PurchaseState.Connecting);
                game.ShowHome();yield return null;
                var door=Control("Sign in");Assert.That(door.interactable,Is.True,"Catalog loading cannot block the account door.");
                door.onClick.Invoke();yield return null;
                Assert.That(game.CurrentScreen,Is.EqualTo(GameScreen.AccountGate),"The first tap must open account actions.");
                Check(game);
            }
        }
        [UnityTest] public IEnumerator AccountBackStaysAboveWelcomeInAllLocalesAndPhoneSizes()
        {
            Profile.AccountIntroSeen=true;
            foreach(var size in new[]{new Vector2Int(320,693),new Vector2Int(390,844),new Vector2Int(430,932)}){
                var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=null;portrait.Release();UnityEngine.Object.Destroy(portrait);portrait=new RenderTexture(size.x,size.y,24);camera.targetTexture=portrait;scaler.enabled=true;yield return null;
                foreach(string locale in new[]{"en","ru","uk"}){
                    game.SetUiLocale(locale);game.ShowHome();game.ShowAccountGate();yield return null;game.RefreshLayout();Canvas.ForceUpdateCanvases();
                    var back=ScreenRect((RectTransform)Control("Back").transform);
                    var heading=game.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Welcome hanging sign");
                    var board=ScreenRect(heading);
                    Assert.That(back.yMin,Is.GreaterThan(board.yMax+4),locale+" back must clear the sign");
                    Assert.That(back.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(back.yMax,Is.LessThanOrEqualTo(size.y));
                }
            }
        }
        [UnityTest] public IEnumerator NativeAccountPrimaryActionAndStatusHierarchyFit()
        {
            var adapter=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.PlatformAccounts.AppleGameCenterAccount")).First(t=>t!=null);
            var settings=ScriptableObject.CreateInstance<CloudPlayerSettings>();
            using(var client=new CloudSyncClient(settings,Path.Combine(directory,"hierarchy-ui-journal"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},(ICloudPlatformAccount)Activator.CreateInstance(adapter))){
                typeof(CascadeGame).GetField("cloudSave",Private).SetValue(game,client);
                foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(320,693),new Vector2Int(768,1024)}){
                    var scaler=game.GetComponentInChildren<CanvasScaler>();scaler.enabled=false;camera.targetTexture=null;portrait.Release();UnityEngine.Object.Destroy(portrait);
                    portrait=new RenderTexture(size.x,size.y,24);camera.targetTexture=portrait;scaler.enabled=true;yield return null;
                    foreach(string locale in new[]{"en","ru","uk"})foreach(bool large in new[]{false,true}){
                        Profile.Preferences.LargeText=large;game.SetUiLocale(locale);yield return new WaitForSecondsRealtime(.1f);
                        foreach(var state in new[]{CloudSyncState.Saving,CloudSyncState.Saved}){
                            typeof(CloudSyncClient).GetProperty("State").SetValue(client,state);game.ShowAccountGate();
                            var native=Control("Sign in with Game Center");var stage=(RectTransform)native.transform.parent;
                            typeof(CascadeGame).GetMethod("DrawAccountCloudStatus",Private).Invoke(game,new object[]{stage});
                            yield return null;game.RefreshLayout();Canvas.ForceUpdateCanvases();
                            var paper=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Account cloud status paper");
                            var action=ScreenRect((RectTransform)native.transform);var status=ScreenRect(paper.rectTransform);
                            var face=native.GetComponentsInChildren<Image>().Single(i=>i.name=="Account button face");var artwork=ScreenRect(face.rectTransform);
                            Assert.That(action.width,Is.GreaterThan(status.width*1.1f),locale+" primary action width");
                            Assert.That(action.height,Is.GreaterThan(status.height*1.6f),locale+" primary action height");
                            Assert.That(artwork.height,Is.GreaterThan(status.height*2),locale+" painted action also grows");
                            Assert.That(action.center.x,Is.EqualTo(status.center.x).Within(.5f),locale+" shared centre");
                            float unit=paper.GetComponentInParent<Canvas>().scaleFactor;
                            Assert.That((status.yMin-artwork.yMax)/unit,Is.GreaterThanOrEqualTo(20),locale+" clear logical gap above action artwork");
                            foreach(var text in native.GetComponentsInChildren<TextMeshProUGUI>().Concat(paper.GetComponentsInChildren<TextMeshProUGUI>())){
                                text.ForceMeshUpdate();Assert.That(text.isTextOverflowing||text.isTextTruncated,Is.False,locale+" "+text.text);
                            }
                            Check(game);
                            if(!large&&state==CloudSyncState.Saving)yield return Shot("account-hierarchy-"+locale+"-"+size.x,size.x,size.y);
                        }
                    }
                }
                typeof(CascadeGame).GetField("cloudSave",Private).SetValue(game,null);
            }
            UnityEngine.Object.Destroy(settings);
        }
        [UnityTest] public IEnumerator NativeAccountActionsAndDialogsFitPhoneAndLocales()
        {
            var adapter=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Cascade.PlatformAccounts.AppleGameCenterAccount")).First(t=>t!=null);
            var settings=ScriptableObject.CreateInstance<CloudPlayerSettings>();
            using(var client=new CloudSyncClient(settings,Path.Combine(directory,"native-ui-journal"),()=>new PlayerProfile(),(_,__)=>{},new[]{"C01"},(ICloudPlatformAccount)Activator.CreateInstance(adapter))){
                typeof(CascadeGame).GetField("cloudSave",Private).SetValue(game,client);
                foreach(string locale in new[]{"en","ru","uk"})foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(320,693),new Vector2Int(768,1024)}){
                    typeof(CloudSyncClient).GetProperty("LastFailureCode").SetValue(client,null);
                    game.SetUiLocale(locale);Profile.AccountIntroSeen=false;game.ShowAccountGate();
                    string id=locale+"-"+size.x;
                    yield return Shot("native-account-"+id,size.x,size.y);
                    var native=Control("Sign in with Game Center");var bounds=ScreenRect((RectTransform)native.transform);
                    Assert.That(bounds.height,Is.GreaterThanOrEqualTo(44));Assert.That(bounds.center.x,Is.EqualTo(portrait.width*.5f).Within(1));
                    var action=native.GetComponentInChildren<TextMeshProUGUI>();action.ForceMeshUpdate();
                    var ink=CenteredContentRow.GlyphBounds(action);
                    var low=(Vector2)action.transform.TransformPoint(ink.min);var high=(Vector2)action.transform.TransformPoint(ink.max);
                    var bottom=RectTransformUtility.WorldToScreenPoint(camera,low);var top=RectTransformUtility.WorldToScreenPoint(camera,high);
                    Assert.That(bottom.x-bounds.xMin,Is.GreaterThan(8),action.text+": left padding");
                    Assert.That(bounds.xMax-top.x,Is.GreaterThan(8),action.text+": right padding");
                    Assert.That(bottom.y-bounds.yMin,Is.GreaterThan(8),action.text+": bottom padding");
                    Assert.That(bounds.yMax-top.y,Is.GreaterThan(8),action.text+": top padding");
                    var stage=(RectTransform)native.transform.parent;
                    typeof(CascadeGame).GetMethod("DrawAccountCloudStatus",Private).Invoke(game,new object[]{stage});
                    yield return Shot("native-sync-paper-"+id,size.x,size.y);
                    var paper=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Account cloud status paper");
                    var status=paper.GetComponentInChildren<TextMeshProUGUI>();
                    Assert.That(paper.sprite,Is.Not.Null);Assert.That(status.color.a,Is.EqualTo(1));
                    Assert.That(status.isTextOverflowing||status.isTextTruncated,Is.False,status.text);
                    UnityEngine.Object.Destroy(paper.gameObject);yield return null;

                    foreach(string code in new[]{"GameCenterSignInRequired","GameCenterUnavailable"}){
                        typeof(CloudSyncClient).GetProperty("LastFailureCode").SetValue(client,code);
                        typeof(CascadeGame).GetField("accountErrorAcknowledged",Private).SetValue(game,false);game.ShowAccountGate();
                        yield return Shot("native-dialog-"+code+"-"+id,size.x,size.y);
                        var panel=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Account dialog panel");var panelBounds=ScreenRect(panel.rectTransform);
                        Assert.That(panelBounds.center.x,Is.EqualTo(portrait.width*.5f).Within(1));Assert.That(panelBounds.center.y,Is.EqualTo(portrait.height*.5f).Within(2));
                        foreach(var text in panel.GetComponentsInChildren<TextMeshProUGUI>()){text.ForceMeshUpdate();Assert.That(text.isTextOverflowing||text.isTextTruncated,Is.False,text.text);}
                        yield return Click("Close account notice");
                    }
                }
                typeof(CascadeGame).GetField("cloudSave",Private).SetValue(game,null);
            }
            UnityEngine.Object.Destroy(settings);
        }
    }
}
#endif
