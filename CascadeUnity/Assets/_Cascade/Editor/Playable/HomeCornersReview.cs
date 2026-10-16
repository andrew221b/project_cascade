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
    public sealed class HomeCornersReview
    {
        const string Phase="HomeCornersOct13",Scene="Assets/_Cascade/Scenes/CascadePlayable.unity";
        CascadeGame game;GameObject host;bool baseline;readonly List<string> checks=new List<string>();
        public static void Start(bool before)
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode first");
            var h=new GameObject("Isolated home corners review");UnityEngine.Object.DontDestroyOnLoad(h);
            h.AddComponent<ReferenceShopLayout>().StartCoroutine(new HomeCornersReview{host=h,baseline=before}.Run());
        }
        IEnumerator Run()
        {
            var temporary=Path.Combine(Path.GetTempPath(),"CascadeHomeCorners",Guid.NewGuid().ToString("N"));
            CascadeGame.EditorProfileDirectory=temporary;UnityEngine.Application.runInBackground=true;
            yield return SceneManager.LoadSceneAsync(Scene);
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.MusicEnabled=game.Feedback.SoundEnabled=false;game.Preferences.Tutorials=false;game.Interaction.SetSuspended(false);
            var profile=(PlayerProfile)typeof(CascadeGame).GetField("profile",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            profile.Monetization.BonusCoins=362;profile.Monetization.Cosmetics.Add("wind_chime");
            foreach(var locale in baseline?new[]{"ru"}:new[]{"ru","en","uk"}){
                game.SetUiLocale(locale);game.Preferences.LargeText=locale=="uk";game.ShowHome();
                yield return Shot((baseline?"before-":"home-")+locale,588,1280);
                if(!baseline){
                    yield return Shot("home-"+locale+"-reference",720,1280);yield return Shot("home-"+locale+"-medium",700,1280);yield return Shot("home-"+locale+"-small",320,693);
                    game.SetEditorSafeInsets(new Vector4(0,.035f,0,.055f));yield return Shot("home-"+locale+"-safe",390,844);game.SetEditorSafeInsets(Vector4.zero);
                    game.ShowShop();game.SelectShopTab(0);yield return Shot("shop-"+locale,588,1280);
                    game.SelectShopTab(1);game.SelectShopItem(Array.IndexOf(ShopCatalog.TrophyIds,"wind_chime"));
                    game.GetComponentsInChildren<Button>().Single(b=>b.name=="Buy selected item").onClick.Invoke();yield return Shot("preview-"+locale,588,1280);
                    game.ShowCoinPacks();yield return Shot("coins-"+locale,390,844);
                    game.ShowJournal();yield return Shot("journal-"+locale,390,844);
                }
            }
            var folder=Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports",Phase);Directory.CreateDirectory(folder);
            File.WriteAllLines(Path.Combine(folder,baseline?"baseline.txt":"verification.txt"),checks);
            CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync(Scene);
            game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.ShowHome();UnityEngine.Object.Destroy(host);
            Debug.Log("HOME CORNERS REVIEW COMPLETE "+(baseline?"baseline":"final")+"\n"+string.Join("\n",checks));
        }
        IEnumerator Shot(string name,int width,int height)
        {
            yield return new WaitForSecondsRealtime(.3f);yield return PlayableCapture.CaptureSettled(Phase,name+".png",width,height,g=>Inspect(name));
            game.RefreshLayout();Canvas.ForceUpdateCanvases();Camera.main.Render();int passed=0,total=0;
            var hits=new List<RaycastResult>();foreach(var b in game.GetComponentsInChildren<Button>().Where(b=>b.interactable)){
                total++;hits.Clear();var r=(RectTransform)b.transform;
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(Camera.main,r.TransformPoint(r.rect.center))},hits);
                if(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b)passed++;else checks.Add("RAYCAST FAIL "+name+": "+b.name);
            }
            checks.Add("Buttons "+name+": "+passed+"/"+total);
        }
        void Inspect(string name)
        {
            foreach(var t in game.GetComponentsInChildren<TextMeshProUGUI>()){t.ForceMeshUpdate();if(t.enabled&&t.canvasRenderer.GetAlpha()>.01f&&t.isTextOverflowing){
                    bool scope=t.name.StartsWith("Home reference")||t.name.StartsWith("Reference shop")||t.GetComponentInParent<PrimaryButtonSkin>()!=null;
                    checks.Add((scope?"OVERFLOW ":"UNRELATED LABEL ")+name+": "+t.name);
                }}
            foreach(var skin in game.GetComponentsInChildren<PrimaryButtonSkin>()){
                var image=skin.GetComponent<Image>();
                checks.Add(name+" primary "+skin.name+" sprite="+image.sprite.name+" type="+image.type+" border="+image.sprite.border);
            }
            if(game.CurrentScreen!=GameScreen.Home)return;
            var buttons=game.GetComponentsInChildren<Button>();var nav=new[]{"Mythings","Shop","Settings"}.Select(n=>(RectTransform)buttons.Single(b=>b.name==n).transform).ToArray();
            var play=(RectTransform)buttons.Single(b=>b.name=="Startgame").transform;
            checks.Add(name+" nav Y spread="+(nav.Max(r=>r.position.y)-nav.Min(r=>r.position.y))+" Shop/Play centre delta="+Mathf.Abs(nav[1].position.x-play.position.x));
            var logo=game.GetComponentsInChildren<Image>().Single(i=>i.name=="Flying Ladders illustrated logo").rectTransform;
            var ads=(RectTransform)buttons.Single(b=>b.name=="Remove ads shortcut").transform;
            var a=new Vector3[4];var l=new Vector3[4];ads.GetWorldCorners(a);logo.GetWorldCorners(l);
            bool overlap=a[0].x<l[2].x&&a[2].x>l[0].x&&a[0].y<l[2].y&&a[2].y>l[0].y;
            checks.Add(name+" advertisement overlaps logo rectangle="+overlap);
            foreach(var image in nav.Select(r=>r.GetComponent<Image>()))checks.Add(name+" "+image.name+" preserveAspect="+image.preserveAspect+" sprite="+image.sprite.name);
        }
    }
}
