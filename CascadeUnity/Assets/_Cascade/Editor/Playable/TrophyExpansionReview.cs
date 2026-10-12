using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Cascade.Application;
using Cascade.Content;
using Cascade.Persistence;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class TrophyExpansionReview
    {
        const string Folder="TrophyEconomy-Oct08";
        public static void Prepare()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Leave Play first.");
            NavigationCapture.Prepare();
            string Read(string n)=>Resources.Load<TextAsset>("CascadeContent/"+n).text;
            var catalog=ContentImporter.Import(Read("levels"),Read("localized_levels"),Read("words_en"),Read("words_ru"),Read("words_uk"),Read("campaign_extension"));
            // Availability fixture only; fabricated clears never touch a player/cloud profile.
            var p=new PlayerProfile{FirstGestureShown=true};p.Preferences.UiLocale="ru";
            p.SeenMechanics.AddRange((MechanicLesson[])Enum.GetValues(typeof(MechanicLesson)));
            foreach(var item in catalog.PrimaryLevels.Take(240))p.Clears.Add(new ClearReceipt{CampaignNodeId=item.Definition.CampaignNodeId,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=item.Definition.ContentHash,BestStars=3});
            p.Rewards.Synchronize(p.Clears.Select(c=>c.CampaignNodeId));
            p.Monetization.Cosmetics.AddRange(new[]{"mushroom_cap","bee_suit","winter_hood","ribbon_kites","origami_cranes"});
            p.Monetization.SpentCoins=p.Monetization.Cosmetics.Sum(ShopCatalog.TrophyPrice);
            p.SeenCoachPrompts.Add("trophy_preview_tap");
            new ProfileStore(Path.Combine(CascadeGame.EditorProfileDirectory,"cascade-profile.json")).Save(p);
        }
        public static void Capture()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required.");
            game.StartCoroutine(Review(game));
        }
        public static void CaptureNarrow()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated profile required.");
            game.StartCoroutine(Review(game,true));
        }
        static IEnumerator Review(CascadeGame g,bool narrowOnly=false)
        {
            bool background=UnityEngine.Application.runInBackground;int rate=UnityEngine.Application.targetFrameRate;
            UnityEngine.Application.runInBackground=true;UnityEngine.Application.targetFrameRate=30;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;
            g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            var log=new StringBuilder();var camera=Camera.main;var scaler=g.GetComponentInChildren<CanvasScaler>();
            var previous=camera.targetTexture;var measure=new RenderTexture(320,693,24);
            try
            {
                g.ShowShop();g.SelectShopTab(1);g.SetUiLocale("ru");g.ReducedMotion=false;
                string hash=g.Interaction.Attempt.CurrentStateHash;
                if(!narrowOnly)foreach(string id in ShopCatalog.ExpansionTrophyIds){
                    g.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,id));yield return null;
                    if(!g.OwnsGift(id)){
                        if(id=="jester_suit")yield return Shot("locked-jester-ru",390,844);
                        Check(g.BuySelectedShopItem(),"buy "+id);
                    }
                    Check(g.BuySelectedShopItem(),"equip "+id);yield return null;
                    yield return Shot("shop-"+id+"-ru",390,844);
                    Check(g.GetComponentsInChildren<Image>().Where(i=>i.name=="Object on shelf").All(i=>i.sprite!=null),"shelf sprites "+id);
                    if(ShopCatalog.IsScenery(id)){
                        g.ShowGame();yield return null;
                        var layer=g.GetComponentsInChildren<TrophyAmbiance>().Single(a=>a.name=="Equipped trophy atmosphere"&&a.transform.parent.name!="Cosmetic atmosphere behind the interface");
                        var actors=layer.GetComponentsInChildren<Image>().Where(i=>i.enabled).ToArray();
                        var before=actors.Select(i=>i.rectTransform.anchoredPosition).ToArray();
                        yield return new WaitForSecondsRealtime(.45f);
                        Check(actors.Where((actor,i)=>(actor.rectTransform.anchoredPosition-before[i]).sqrMagnitude>.001f).Any(),"moving scenery "+id);
                        Check(actors.All(i=>!i.raycastTarget&&i.sprite!=null),"nonblocking scenery "+id);
                        yield return Shot("game-"+id,390,844);
                        g.Preferences.ReducedMotion=true;g.ReducedMotion=true;g.RefreshLayout();yield return null;yield return new WaitForEndOfFrame();
                        actors=layer.GetComponentsInChildren<Image>().Where(i=>i.enabled).ToArray();
                        before=actors.Select(i=>i.rectTransform.anchoredPosition).ToArray();yield return new WaitForSecondsRealtime(.15f);
                        Check(actors.Select((a,i)=>(a.rectTransform.anchoredPosition-before[i]).sqrMagnitude).All(d=>d<.001f),"quiet scenery "+id);
                        g.Preferences.EffectQuality=0;yield return null;yield return new WaitForEndOfFrame();Check(layer.VisibleCount==0,"effects off "+id);
                        g.Preferences.EffectQuality=2;g.Preferences.ReducedMotion=false;g.ReducedMotion=false;g.ShowShop();g.SelectShopTab(1);
                    }
                    Check(g.Interaction.Attempt.CurrentStateHash==hash,"cosmetic preserves puzzle "+id);
                }
                g.Preferences.ReducedMotion=true;g.ReducedMotion=true;g.Preferences.LargeText=true;
                scaler.enabled=false;camera.targetTexture=measure;scaler.enabled=true;
                foreach(string locale in new[]{"en","ru","uk"}){
                    g.SetUiLocale(locale);yield return null;yield return null;
                    foreach(string id in ShopCatalog.ExpansionTrophyIds){
                        g.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,id));yield return null;g.RefreshLayout();yield return null;yield return new WaitForEndOfFrame();Canvas.ForceUpdateCanvases();
                        foreach(var t in g.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name=="Selected object title"||t.name=="Selected object benefit")){
                            t.ForceMeshUpdate();Check(!t.isTextOverflowing&&t.textBounds.min.x>=t.rectTransform.rect.xMin-1&&t.textBounds.max.x<=t.rectTransform.rect.xMax+1,"narrow copy "+locale+" "+id+" "+t.name+" ("+t.text+"; "+t.textBounds+"; "+t.rectTransform.rect+")");
                        }
                    }
                }
                yield return Shot("small-aurora-uk-large",320,693);
                g.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,"constellation_mage"));yield return Shot("small-mage-uk-large",320,693);
                g.ShowCollection();g.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,"mushroom_cap"));yield return Shot("collection-page3-uk",320,693);
                var saved=new ProfileStore(g.ProfilePath).Load().Profile;
                Check(ShopCatalog.ExpansionTrophyIds.All(saved.Monetization.Cosmetics.Contains),"all purchases durable");
                Check(saved.Monetization.SpentCoins==9720,"exact new collection cost");
                Check(g.Interaction.Attempt.CurrentStateHash==hash,"review preserves puzzle");
                log.AppendLine("Isolated availability fixture; actual virtual purchases/equips tested. No live cloud/device/build claim.");
                File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Folder,"native-runtime.txt"),log.ToString());
                Debug.Log("CASCADE trophy expansion review complete.");
            }
            finally{
                Directory.CreateDirectory(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Folder));
                File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Folder,"native-progress.txt"),log.ToString());
                scaler.enabled=false;camera.targetTexture=previous;scaler.enabled=true;measure.Release();UnityEngine.Object.Destroy(measure);
                UnityEngine.Application.runInBackground=background;UnityEngine.Application.targetFrameRate=rate;g.RefreshLayout();
            }
            void Check(bool good,string label){if(!good)throw new InvalidOperationException(label);log.AppendLine("PASS "+label);}
            IEnumerator Shot(string name,int w,int h){yield return PlayableCapture.CaptureSettled(Folder,name+".png",w,h);}
        }
    }
}
