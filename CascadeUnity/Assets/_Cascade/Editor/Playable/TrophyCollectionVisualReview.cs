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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public sealed class TrophyCollectionVisualReview
    {
        const string Phase="CollectionBrightnessOct13",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
        CascadeGame game;GameObject host;PlayerProfile profile;readonly List<string> checks=new List<string>();
        public static void StartReview()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode first");
            var h=new GameObject("Isolated collection reference review");UnityEngine.Object.DontDestroyOnLoad(h);
            h.AddComponent<ReferenceShopLayout>().StartCoroutine(new TrophyCollectionVisualReview{host=h}.Run());
        }
        IEnumerator Run()
        {
            CascadeGame.EditorProfileDirectory=Path.Combine(Path.GetTempPath(),"CascadeCollectionReference",Guid.NewGuid().ToString("N"));
            UnityEngine.Application.runInBackground=true;yield return SceneManager.LoadSceneAsync(Scene);
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;game.Interaction.SetSuspended(false);game.Preferences.EffectQuality=2;
            profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            game.ShowCollection();yield return Shot("empty",390,844);
            profile.Monetization.Cosmetics.AddRange(new[]{"leaf","moon","wind_chime","rain_badge","explorer_hat","frog_suit","firefly_dance","garden_frame"});
            foreach(var locale in new[]{"ru","en","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=locale=="uk";profile.Rewards.EquippedGift=null;game.CreateGlobalCosmeticAmbiance();Open("leaf");
                yield return Shot("leaf-unapplied-"+locale,720,1280);yield return Shot("leaf-unapplied-"+locale+"-phone",588,1280);yield return Shot("leaf-unapplied-"+locale+"-small",320,693);
                checks.Add("Selection does not equip ("+locale+"): "+(profile.Rewards.EquippedGift==null));
                ActionButton().onClick.Invoke();yield return null;checks.Add("Apply persists leaf ("+locale+"): "+(profile.Rewards.EquippedGift=="leaf"));
                game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot("leaf-applied-"+locale+"-safe",390,844);game.SetEditorSafeInsets(Vector4.zero);
                Open("wind_chime");ActionButton().onClick.Invoke();yield return null;
                yield return Shot("wind-applied-"+locale,720,1280);yield return Shot("wind-applied-"+locale+"-phone",588,1280);
                ActionButton().onClick.Invoke();yield return null;checks.Add("Remove persists null ("+locale+"): "+(profile.Rewards.EquippedGift==null));
                yield return Shot("wind-unapplied-"+locale,390,844);
                checks.Add("Preview remains local ("+locale+"): "+game.GetComponentsInChildren<TrophyAmbiance>(true).Where(v=>!InPreview(v.transform)).All(v=>!v.isActiveAndEnabled));
                Open("frog_suit");yield return Shot("costume-"+locale,720,1280);yield return Shot("costume-"+locale+"-small",320,693);
                game.GetComponentsInChildren<Button>().Single(b=>b.name=="Next owned trophies").onClick.Invoke();yield return Shot("carousel-"+locale,390,844);
                game.ShowHome();checks.Add("Preview closes with collection ("+locale+"): "+!game.GetComponentsInChildren<TrophyMagnifierView>().Any());
            }
            foreach(var id in ShopCatalog.TrophyIds.Where(t=>!ShopCatalog.IsParcelExclusive(t)))if(!profile.Monetization.Cosmetics.Contains(id))profile.Monetization.Cosmetics.Add(id);
            foreach(var content in game.Catalog.PrimaryLevels.GroupBy(c=>c.Definition.CampaignNodeId).Select(g=>g.First()).Take(36)){
                var node=content.Definition.CampaignNodeId;profile.Rewards.SeenClearNodes.Add(node);
                profile.Clears.Add(new ClearReceipt{CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=content.Definition.ContentHash});
            }
            int[] orders={1,3,4,6,7,9,10,12};
            for(int i=0;i<orders.Length;i++)profile.Rewards.Parcels.Add(new ParcelReceipt{Order=orders[i],RecipeVersion=4,Kind="cosmetic",GiftId=ShopCatalog.ParcelTrophyIds[i],Claimed=true});
            profile.Validate();checks.Add("All-trophy fixture validates with earned parcel-exclusive trophies");
            foreach(var locale in new[]{"ru","en","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=locale=="uk";int fitted=0;
                foreach(var id in ShopCatalog.TrophyIds){
                    Open(id);yield return null;game.RefreshLayout();Canvas.ForceUpdateCanvases();if(CheckLabels(locale+" "+id))fitted++;
                    CheckHero(locale+" "+id);
                }
                checks.Add("All collection descriptions fit ("+locale+"): "+fitted+"/46");
                game.ShowShop();game.SelectShopTab(1);int shopFit=0;
                foreach(var id in ShopCatalog.TrophyIds.Where(t=>!ShopCatalog.IsParcelExclusive(t))){game.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,id));yield return null;game.RefreshLayout();Canvas.ForceUpdateCanvases();if(CheckLabels("shop "+locale+" "+id))shopFit++;}
                checks.Add("Updated shop descriptions fit ("+locale+"): "+shopFit+"/38");
            }
            game.Preferences.LargeText=false;game.SetUiLocale("ru");
            foreach(var id in new[]{"moon","sky_pilot","star_knight","pirate_hat","paper_crane","firefly_dance","garden_frame"}){Open(id);yield return Shot("detail-"+id,390,844);}
            game.Preferences.ReducedMotion=true;Open("leaf");yield return Shot("reduced-motion",390,844);
            game.Preferences.EffectQuality=0;Open("wind_chime");yield return Shot("effects-off",390,844);
            string folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);File.WriteAllLines(Path.Combine(folder,"verification.txt"),checks);
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.ShowHome();UnityEngine.Object.Destroy(host);
            Debug.Log("COLLECTION REFERENCE REVIEW COMPLETE\n"+string.Join("\n",checks));
        }
        static bool InPreview(Transform t){for(;t!=null;t=t.parent)if(t.name=="Miniature game")return true;return false;}
        void Open(string id){game.ShowCollection();game.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,id));}
        Button ActionButton()=>game.GetComponentsInChildren<Button>().Single(b=>b.name=="Trophy action");
        bool CheckLabels(string context)
        {
            bool fit=true;foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();if(t.isTextOverflowing||t.isTextTruncated){fit=false;checks.Add("OVERFLOW "+context+": "+t.name+" "+t.text);}}
            return fit;
        }
        void CheckHero(string context)
        {
            var hero=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Trophy Mika preview");var view=hero.GetComponent<MikaCosmeticView>();
            bool lens=game.GetComponentsInChildren<TrophyMagnifierView>().Any(),pin=view.PreviewBadge!=null&&view.PreviewBadge.gameObject.activeSelf;
            if(lens!=pin||view.WearsCostume&&lens)checks.Add("LENS FAIL "+context);
            if(!lens&&Mathf.Abs(hero.rectTransform.anchoredPosition.x+hero.rectTransform.rect.width*.5f-360)>.1f)checks.Add("CENTRE FAIL "+context);
            if(hero.color!=Color.white||!hero.preserveAspect)checks.Add("COLOUR/ASPECT FAIL "+context);
            if(lens){var detail=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Magnified actual Mika sprite");var detailPin=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Magnified actual trophy sprite");
                if(detail.sprite!=hero.sprite||detailPin.sprite!=view.PreviewBadge.sprite)checks.Add("REAL SOURCE FAIL "+context);}
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.35f);yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height,g=>{CheckLabels(name);if(game.GetComponentsInChildren<MikaCosmeticView>().Any(v=>v.name=="Trophy Mika preview"))CheckHero(name);});
            game.RefreshLayout();Canvas.ForceUpdateCanvases();Camera.main.Render();var hits=new List<RaycastResult>();int passed=0,total=0;
            foreach(var b in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){
                total++;hits.Clear();var rt=(RectTransform)b.transform;EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(Camera.main,rt.TransformPoint(rt.rect.center))},hits);
                if(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b)passed++;else checks.Add("RAYCAST FAIL "+name+": "+b.name);
            }
            checks.Add("Buttons "+name+": "+passed+"/"+total);
        }
    }
}
