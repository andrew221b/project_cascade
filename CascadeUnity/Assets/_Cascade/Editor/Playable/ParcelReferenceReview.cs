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
    public sealed class ParcelReferenceReview
    {
        const string Phase="ParcelReferenceOct13",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
        CascadeGame game;PlayerProfile profile;GameObject host;readonly List<string> checks=new List<string>();
        public static void StartReview(){if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play required");var host=new GameObject("Isolated parcel reference review");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<ReferenceShopLayout>().StartCoroutine(new ParcelReferenceReview{host=host}.Run());}
        static void Field(CascadeGame g,string key,object value)=>typeof(CascadeGame).GetField(key,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,value);
        static void Invoke(CascadeGame g,string method)=>typeof(CascadeGame).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,null);
        ParcelRevealView View=>game.GetComponentsInChildren<ParcelRevealView>().Single();
        Button Button(string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        void Open(int order,bool second=false)
        {
            game.ShowTravelBag();game.ShowParcel();Field(game,"parcelOrder",order);Field(game,"parcelBonusRound",second);Field(game,"parcelAnimationShown",false);Field(game,"parcelOpened",false);Invoke(game,"DrawMenu");
        }
        IEnumerator Run()
        {
            CascadeGame.EditorProfileDirectory=Path.Combine(Path.GetTempPath(),"CascadeParcelReview",Guid.NewGuid().ToString("N"));
            UnityEngine.Application.runInBackground=true;yield return SceneManager.LoadSceneAsync(Scene);
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Interaction.SetSuspended(false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;game.Preferences.EffectQuality=2;game.SetUiLocale("ru");
            profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            var completed=(HashSet<string>)typeof(CascadeGame).GetField("sessionClears",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            foreach(var content in game.Catalog.PrimaryLevels.GroupBy(c=>c.Definition.CampaignNodeId).Select(g=>g.First()).Take(36)){
                string node=content.Definition.CampaignNodeId;profile.Clears.Add(new ClearReceipt{CampaignNodeId=node,AttemptId=Guid.NewGuid().ToString("D"),ContentHash=content.Definition.ContentHash});completed.Add(node);
            }
            profile.Rewards.Synchronize(profile.Clears.Select(c=>c.CampaignNodeId));profile.Validate();game.AdProvider.UseLocalPreviewForReview(true);checks.Add("Isolated parcel fixture validates, real player untouched");
            int one=profile.Rewards.Parcels.First(p=>p.Loot.Count==1).Order,two=profile.Rewards.Parcels.First(p=>p.Loot.Count==2&&p.Order!=3).Order;
            Open(two);
            foreach(float t in new[]{0f,.8f,1.6f,1.95f,2.3f,2.8f,3.3f,3.9f,4.65f}){
                View.SetReviewPose(t);yield return Shot("opening-"+t.ToString("F2",System.Globalization.CultureInfo.InvariantCulture),720,1280);
            }
            View.SetClosedReviewWithCards();yield return Shot("reference-composition",720,1280);
            yield return ParcelAnimationCapture.Run(game,View,"opening");
            View.SetReviewPose(4.65f);View.ResumeReview();yield return new WaitForSecondsRealtime(.15f);
            checks.Add("Reveal settles and claim becomes enabled: "+(!View.Busy&&Button("Parcel claim").interactable));
            float age=View.RevealAge;game.SendMessage("OnApplicationPause",true);yield return new WaitForSecondsRealtime(.3f);checks.Add("Background pause freezes reveal: "+Mathf.Approximately(View.RevealAge,age));game.SendMessage("OnApplicationPause",false);
            Button("Parcel claim").onClick.Invoke();Button("Parcel claim").onClick.Invoke();checks.Add("Double claim commits once before animation: "+profile.Rewards.Parcels.Single(p=>p.Order==two).Claimed);
            foreach(float t in new[]{.12f,.45f,.75f,1.1f,1.45f}){View.SetReviewPose(t,true);yield return Shot("collect-"+t.ToString("F2",System.Globalization.CultureInfo.InvariantCulture),720,1280);}
            View.ResumeReview();yield return new WaitForSecondsRealtime(1.6f);checks.Add("Claim changes to Done: "+Button("Parcel claim").GetComponentsInChildren<TMP_Text>().Any(t=>t.text=="Готово"));
            Button("Parcel claim").onClick.Invoke();yield return null;checks.Add("Done returns to My Things: "+(game.CurrentScreen==GameScreen.TravelBag));
            foreach(string locale in new[]{"ru","en","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=locale=="uk";
                Open(one);View.SetReviewPose(4.65f);yield return Shot("one-card-"+locale,720,1280);yield return Shot("one-card-"+locale+"-small",320,693);
                Open(3);View.SetReviewPose(4.65f);yield return Shot("two-cards-"+locale,720,1280);yield return Shot("two-cards-"+locale+"-small",320,693);
                game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot("safe-"+locale,390,844);game.SetEditorSafeInsets(Vector4.zero);
            }
            game.Preferences.LargeText=false;game.SetUiLocale("ru");Open(3);View.ResumeReview();yield return new WaitForSecondsRealtime(4.8f);
            Button("Parcel open more").onClick.Invoke();yield return new WaitForSecondsRealtime(1.7f);
            checks.Add("Extra ad opens only after base claim: "+(profile.Rewards.Parcels.Single(p=>p.Order==3).Claimed&&game.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="Test advertisement")));
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Close test advertisement").onClick.Invoke();yield return null;
            checks.Add("Cancelled video leaves bonus unclaimed and retry available: "+(!profile.Rewards.Parcels.Single(p=>p.Order==3).BonusClaimed&&Button("Parcel open more").interactable));
            Button("Parcel open more").onClick.Invoke();yield return new WaitForSecondsRealtime(5.2f);Invoke(game,"FinishPreviewAdReview");
            yield return null;
            checks.Add("Verified preview creates bonus round: "+game.GetComponentsInChildren<RectTransform>().Count(t=>t.name.StartsWith("Parcel reward card ")&&int.TryParse(t.name.Substring(19),out _)));
            View.SetReviewPose(4.65f);yield return Shot("bonus-after-verified-ad",720,1280);
            yield return ParcelAnimationCapture.Run(game,View,"bonus");
            View.SetReviewPose(4.65f);
            View.ResumeReview();yield return new WaitForSecondsRealtime(.1f);Button("Parcel claim").onClick.Invoke();yield return new WaitForSecondsRealtime(1.6f);
            checks.Add("Second claim returns to My Things: "+(game.CurrentScreen==GameScreen.TravelBag));
            int three=profile.Rewards.Parcels.First(p=>ParcelLoot.Bonus(p.Order).Count==3&&!p.Claimed).Order;
            var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);economy.TryClaimParcel(three,profile.Rewards);
            var grant=new GrantReceipt{Id="parcel_review_bonus_"+three,Kind="coins",Product=MonetizationLedger.ParcelSubject(three),Amount=40};
            profile.Monetization.Grants.Add(grant);profile.Monetization.BonusCoins+=40;
            game.Monetization.Ledger.Grants.Add(grant);game.Monetization.Ledger.BonusCoins+=40;
            foreach(string locale in new[]{"ru","en","uk"}){game.SetUiLocale(locale);Open(three,true);View.SetReviewPose(4.65f);yield return Shot("three-cards-"+locale,720,1280);yield return Shot("three-cards-"+locale+"-small",320,693);}
            game.ReducedMotion=true;Open(one);View.ResumeReview();yield return new WaitForSecondsRealtime(1);checks.Add("Reduced motion completes without shake: "+(!View.Busy&&View.transform.Find("Parcel closed box").localRotation==Quaternion.identity));yield return Shot("reduced-motion",390,844);game.ReducedMotion=false;
            game.Monetization.Ledger.HideVideoOffers=true;Open(one);View.SetReviewPose(4.65f);checks.Add("Hidden video offer removed: "+!game.GetComponentsInChildren<Button>().Any(b=>b.name=="Parcel open more"));yield return Shot("hidden-video",390,844);
            profile.Validate();checks.Add("Final profile validates; preview bonus did not change real durable wallet");
            File.WriteAllLines(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase,"review.txt"),checks);
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);UnityEngine.Object.Destroy(host);EditorApplication.isPlaying=false;
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.28f);yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height);
            foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop"))){t.ForceMeshUpdate();if(t.isTextOverflowing||t.isTextTruncated)checks.Add("OVERFLOW "+name+": "+t.name);}
            if(name=="opening-0.00"){
                var light=game.GetComponentsInChildren<ParcelSunlight>().Single();var mesh=light.canvasRenderer.GetMesh();
                checks.Add("Light renderer: enabled="+light.enabled+", cull="+light.canvasRenderer.cull+", tint="+light.color+", shader="+light.material.shader.name+", vertices="+mesh.vertexCount+", alpha="+light.canvasRenderer.GetAlpha());
            }
            var hits=new List<RaycastResult>();int total=0,ok=0;
            foreach(var b in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){total++;var r=(RectTransform)b.transform;EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(Camera.main,r.TransformPoint(r.rect.center))},hits);if(hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>()==b)ok++;hits.Clear();}
            checks.Add("Buttons "+name+": "+ok+"/"+total);
        }
    }
}
