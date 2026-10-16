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
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Uses an isolated save slot: no real inventory, purchases or cloud players change.
    public sealed class ReferenceShopVisualReview
    {
        CascadeGame game;GameObject host;string temporary;readonly List<string> checks=new List<string>();
        const string Phase="ShopBrightnessOct13";
        const string Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
        public static void StartReview()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode first.");
            var host=GameObject.Find("Reference shop isolated visual review")??new GameObject("Reference shop isolated visual review");
            UnityEngine.Object.DontDestroyOnLoad(host);
            var runner=host.GetComponent<ReferenceShopLayout>()??host.AddComponent<ReferenceShopLayout>();
            runner.StartCoroutine(new ReferenceShopVisualReview{host=host}.Run());
        }
        IEnumerator Run()
        {
            temporary=Path.Combine(Path.GetTempPath(),"CascadeReferenceShopQA",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=temporary;UnityEngine.Application.runInBackground=true;
            yield return SceneManager.LoadSceneAsync(Scene);
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;
            game.Interaction.SetSuspended(false);game.Preferences.EffectQuality=2;
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            profile.Monetization.BonusCoins=362;profile.Monetization.UndoSpools=1;profile.Monetization.SignalLanterns=1;
            foreach(var id in new[]{"leaf"})
                if(!profile.Monetization.Cosmetics.Contains(id))profile.Monetization.Cosmetics.Add(id);
            checks.Add("Fixture wallet = "+game.ShopCoins+", owned = "+ShopCatalog.TrophyIds.Count(game.OwnsGift));
            yield return null;
            foreach(var locale in new[]{"ru","en","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=locale=="uk";
                game.ShowShop();game.SelectShopTab(0);yield return Shot("help-"+locale,720,1280);yield return Shot("help-"+locale+"-phone",390,844);
                game.SelectShopTab(1);game.SelectShopItem(1);yield return Shot("trophies-"+locale,720,1280);yield return Shot("trophies-"+locale+"-phone",588,1280);yield return Shot("trophies-"+locale+"-small",320,693);
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Shop object 1").onClick.Invoke();
                checks.Add("Shop card stays in catalogue ("+locale+"): "+!game.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="Trophy Mika preview"));
                checks.Add("Shop has no preview action ("+locale+"): "+!game.GetComponentsInChildren<Button>().Any(b=>b.name.Contains("Preview")));
                game.SelectShopItem(0);yield return Shot("purchased-"+locale,720,1280);yield return Shot("purchased-"+locale+"-phone",390,844);
                checks.Add("Purchased action disabled ("+locale+"): "+!game.GetComponentsInChildren<Button>().Single(b=>b.name=="Buy selected item").interactable);
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Next trophies").onClick.Invoke();yield return Shot("page-two-"+locale,390,844);
                game.SelectShopItem(ShopCatalog.TrophyIds.Length-1);yield return Shot("last-page-"+locale,390,844);
                checks.Add("Last page next disabled ("+locale+"): "+!game.GetComponentsInChildren<Button>().Single(b=>b.name=="Next trophies").interactable);
                int fitted=0;
                foreach(var id in ShopCatalog.TrophyIds.Where(t=>!ShopCatalog.IsParcelExclusive(t))){
                    game.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,id));yield return null;
                    game.RefreshLayout();Canvas.ForceUpdateCanvases();
                    bool fit=true;
                    foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){
                        label.ForceMeshUpdate();if(label.isTextOverflowing||label.isTextTruncated){fit=false;checks.Add("OVERFLOW full catalogue "+locale+" "+id+": "+label.name);}
                    }
                    if(fit)fitted++;
                }
                checks.Add("Complete trophy catalogue text fit ("+locale+"): "+fitted+"/38");
                game.ShowCoinPacks();yield return Shot("coins-"+locale,720,1280);yield return Shot("coins-"+locale+"-phone",390,844);
                checks.Add("Six coin offers ("+locale+"): "+game.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("Purchase cascade.coins_")));
                foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop store price ")))
                    label.text=locale=="ru"?"Купить 1 499,00 грн":locale=="uk"?"Купити 1 499,00 грн":"Buy $19.99";
                yield return Shot("prices-long-"+locale,320,693);
                profile.Monetization.Cosmetics.Add("wind_chime");
                game.ShowCollection();yield return Shot("collection-"+locale,390,844);
                var listed=game.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Shop object ")).Select(b=>int.Parse(b.name.Substring(12))).ToArray();
                checks.Add("Collection owned only ("+locale+"): "+listed.All(i=>game.OwnsGift(ShopCatalog.TrophyIds[i])));
                game.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,"wind_chime"));
                yield return Shot("collection-preview-"+locale,390,844);yield return Shot("collection-preview-"+locale+"-reference",720,1280);
                profile.Monetization.Cosmetics.Remove("wind_chime");
            }
            game.Preferences.LargeText=false;game.SetUiLocale("ru");
            game.ShowShop();game.SelectShopTab(1);game.SelectShopItem(1);game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot("trophies-safe",390,844);game.SetEditorSafeInsets(Vector4.zero);
            game.ShowShop();game.SelectShopTab(0);game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot("help-safe",390,844);
            game.ShowCoinPacks();yield return Shot("coins-safe",320,693);game.SetEditorSafeInsets(Vector4.zero);
            foreach(var i in Enumerable.Range(0,ShopCatalog.ToolCount)){
                game.ShowShop();game.SelectShopTab(0);game.SelectShopItem(i);yield return Shot("tool-"+i,390,844);
            }
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);
            File.WriteAllLines(Path.Combine(folder,"verification.txt"),checks);Debug.Log("REFERENCE SHOP REVIEW COMPLETE\n"+string.Join("\n",checks));
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.ShowShop();game.SelectShopTab(0);
            UnityEngine.Object.Destroy(host);
        }
        void CheckButtons(string name)
        {
            game.RefreshLayout();Canvas.ForceUpdateCanvases();Camera.main.Render();
            var eventSystem=EventSystem.current;
            int passed=0,total=0;var hits=new List<RaycastResult>();
            foreach(var button in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){
                total++;hits.Clear();var r=(RectTransform)button.transform;
                var pointer=new PointerEventData(eventSystem){position=RectTransformUtility.WorldToScreenPoint(Camera.main,r.TransformPoint(r.rect.center))};
                eventSystem.RaycastAll(pointer,hits);
                if(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button)passed++;
                else checks.Add("RAYCAST FAIL "+name+": "+button.name);
            }
            checks.Add("Buttons "+name+": "+passed+"/"+total);
        }
        void CheckStockBadges(string context)
        {
            var corners=new Vector3[4];int contained=0;
            foreach(var badge in game.GetComponentsInChildren<Image>().Where(i=>i.name=="Reference shop stock badge")){
                var body=(RectTransform)badge.transform.parent;
                badge.rectTransform.GetWorldCorners(corners);
                bool fit=corners.All(c=>body.rect.Contains((Vector2)body.InverseTransformPoint(c)))
                    &&badge.GetComponent<Shadow>()?.isActiveAndEnabled==true;
                foreach(var label in body.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop stock "))){
                    label.ForceMeshUpdate();
                    for(int i=0;i<label.textInfo.characterCount;i++){
                        var glyph=label.textInfo.characterInfo[i];if(!glyph.isVisible)continue;
                        foreach(var vertex in new[]{glyph.bottomLeft,glyph.topRight})
                            if(!badge.rectTransform.rect.Contains((Vector2)badge.rectTransform.InverseTransformPoint(label.transform.TransformPoint(vertex))))fit=false;
                    }
                }
                if(fit)contained++;else checks.Add("COUNTER FAIL "+context+": "+body.parent.name);
            }
            if(contained>0)checks.Add("Contained counters with silhouette shadows "+context+": "+contained+"/6");
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.45f);
            yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height);
            CheckButtons(name);CheckStockBadges(name);
            foreach(var label in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){
                label.ForceMeshUpdate();if(label.isTextOverflowing||label.isTextTruncated)checks.Add("OVERFLOW "+name+": "+label.name+" "+label.text);
            }
        }
    }
}
