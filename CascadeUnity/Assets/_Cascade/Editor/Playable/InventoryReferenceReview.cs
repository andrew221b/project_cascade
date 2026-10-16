using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Persistence;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Cascade.Editor.Playable
{
    public sealed class InventoryReferenceReview
    {
        const string Phase="InventoryReferenceOct13",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
        CascadeGame game;GameObject host;readonly List<string> checks=new List<string>();
        public static void StartReview(){if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play required");var host=new GameObject("Isolated inventory reference review");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<ReferenceShopLayout>().StartCoroutine(new InventoryReferenceReview{host=host}.Run());}
        IEnumerator Run()
        {
            CascadeGame.EditorProfileDirectory=Path.Combine(Path.GetTempPath(),"CascadeInventoryReview",Guid.NewGuid().ToString("N"));
            UnityEngine.Application.runInBackground=true;yield return SceneManager.LoadSceneAsync(Scene);
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Interaction.SetSuspended(false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            profile.Monetization.BonusCoins=362;profile.Monetization.Cosmetics.AddRange(new[]{"leaf","sun","sky","moon","wind_chime","rain_badge","explorer_hat","firefly_dance"});profile.Rewards.EquippedGift="leaf";
            profile.Monetization.WayfinderScrolls=4;profile.Monetization.UndoSpools=3;profile.Monetization.ReturnHourglasses=3;profile.Monetization.SignalLanterns=3;profile.Monetization.LandingLenses=3;profile.Monetization.RepairKits=3;
            var completed=(HashSet<string>)typeof(CascadeGame).GetField("sessionClears",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            foreach(var content in game.Catalog.PrimaryLevels.GroupBy(c=>c.Definition.CampaignNodeId).Select(g=>g.First()).Take(8)){
                string node=content.Definition.CampaignNodeId;profile.Clears.Add(new ClearReceipt{CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=content.Definition.ContentHash});completed.Add(node);profile.Rewards.SeenClearNodes.Add(node);
            }
            profile.Monetization.BonusCoins+=362-game.ShopCoins;profile.Validate();checks.Add("Inventory isolated fixture validates with 8 lights, 19 items and 8 owned trophies");
            game.AdProvider.UseLocalPreviewForReview(true);
            foreach(var locale in new[]{"ru","en","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=locale=="uk";game.ShowHome();game.ShowTravelBag();yield return new WaitForSecondsRealtime(7);
                yield return Shot("inventory-"+locale,720,1280);yield return Shot("inventory-"+locale+"-phone",588,1280);yield return Shot("inventory-"+locale+"-small",320,693);
                game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot("inventory-"+locale+"-safe",390,844);game.SetEditorSafeInsets(Vector4.zero);
            }
            game.Preferences.LargeText=false;game.SetUiLocale("ru");
            var routes=new[]{"Inventory lights","Inventory items","Inventory balance","Inventory trophies"};
            var destinations=new[]{GameScreen.JourneyMap,GameScreen.Supplies,GameScreen.CoinPacks,GameScreen.Collection};
            for(int i=0;i<routes.Length;i++){
                game.ShowHome();game.ShowTravelBag();yield return null;
                game.GetComponentsInChildren<Button>().Single(b=>b.name==routes[i]).onClick.Invoke();yield return null;
                checks.Add("Route "+routes[i]+": "+(game.CurrentScreen==destinations[i]));yield return Shot("route-"+i,390,844);
            }
            game.ShowHome();game.ShowTravelBag();yield return new WaitForSecondsRealtime(2);
            var island=game.GetComponentsInChildren<RectTransform>().First(t=>t.name.StartsWith("Drifting home island"));var before=island.anchoredPosition;yield return new WaitForSecondsRealtime(1.5f);
            checks.Add("Inventory island moves: "+(Vector2.Distance(before,island.anchoredPosition)>.01f));
            var leaves=game.GetComponentsInChildren<MenuLeafDrift>().Single();checks.Add("Leaves visible and pooled: "+(leaves.VisibleCount>0&&leaves.GetComponentsInChildren<Image>().Length==8));
            foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.EndsWith(" count")))label.text="999999";
            yield return Shot("large-counts",320,693);
            game.ReducedMotion=true;yield return null;checks.Add("Reduced motion hides menu leaves: "+(leaves.VisibleCount==0));yield return Shot("reduced-motion",390,844);
            game.ReducedMotion=false;game.Preferences.EffectQuality=0;yield return null;checks.Add("Effects off hides menu leaves: "+(leaves.VisibleCount==0));yield return Shot("effects-off",390,844);
            game.Preferences.EffectQuality=2;game.ShowHome();yield return new WaitForSecondsRealtime(6);yield return Shot("home-leaves",390,844);
            game.ShowShop();yield return new WaitForSecondsRealtime(6);yield return Shot("shop-leaves",390,844);
            game.ShowSettings();yield return new WaitForSecondsRealtime(6);yield return Shot("settings-leaves",390,844);
            game.ShowHome();game.ShowTravelBag();profile.Rewards.EquippedGift="explorer_hat";game.ShowTravelBag();yield return Shot("inventory-costume",390,844);
            profile.Rewards.EquippedGift=null;game.ShowTravelBag();yield return Shot("inventory-unworn",390,844);
            game.Monetization.Ledger.HideVideoOffers=true;game.NotifyStoreChanged();yield return null;
            checks.Add("Hidden video offer cannot grant reward: "+!game.GetComponentsInChildren<Button>().Single(b=>b.name=="Inventory claim reward").interactable);
            yield return Shot("reward-unavailable",390,844);
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);File.WriteAllLines(Path.Combine(folder,"verification.txt"),checks);Debug.Log("INVENTORY REFERENCE REVIEW COMPLETE\n"+string.Join("\n",checks));
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);UnityEngine.Object.FindFirstObjectByType<CascadeGame>().ShowTravelBag();UnityEngine.Object.Destroy(host);
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height);
            Canvas.ForceUpdateCanvases();int ok=0,total=0;var hits=new List<RaycastResult>();
            foreach(var button in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){
                var rect=(RectTransform)button.transform;
                var scroll=button.GetComponentInParent<ScrollRect>();var screenPoint=RectTransformUtility.WorldToScreenPoint(Camera.main,rect.TransformPoint(rect.rect.center));
                if(scroll?.viewport!=null&&!RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport,screenPoint,Camera.main))continue;
                total++;hits.Clear();var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(Camera.main,rect.TransformPoint(rect.rect.center))};EventSystem.current.RaycastAll(pointer,hits);
                var actual=hits.Count>0?hits[0].gameObject.GetComponentInParent<Button>():null;
                bool sameIslandAction=game.CurrentScreen==GameScreen.JourneyMap&&button.name.StartsWith("Island")&&actual?.name=="Browse Mika island"&&actual.transform.parent==button.transform.parent;
                if(sameIslandAction)checks.Add("Map island centre uses its companion shortcut: "+button.name);
                if(actual==button||sameIslandAction)ok++;else checks.Add("RAYCAST FAIL "+name+": "+button.name);
            }
            checks.Add("Buttons "+name+": "+ok+"/"+total);
            foreach(var text in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){text.ForceMeshUpdate();if(text.isTextOverflowing||text.isTextTruncated)checks.Add("OVERFLOW "+name+": "+text.name);}
        }
    }
}
