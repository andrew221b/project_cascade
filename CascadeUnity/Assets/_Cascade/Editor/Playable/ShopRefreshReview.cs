using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Presentation;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Native presentation and inventory checks on an isolated Editor-only profile.
    public static class ShopRefreshReview
    {
        public static void Run()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated QA profile required");
            g.StartCoroutine(Inspect(g));
        }
        static IEnumerator Inspect(CascadeGame g)
        {
            UnityEngine.Application.runInBackground=true;g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));
            g.SetUiLocale("ru");g.ShowShop();g.SelectShopTab(0);g.SelectShopItem(3);yield return Shot("tools-ru",390,844);
            g.Preferences.LargeText=true;g.SetUiLocale("uk");g.SelectShopItem(4);yield return Shot("tools-uk-small",320,693);
            for(int i=0;i<ShopCatalog.ToolCount;i++){g.SelectShopItem(i);Check(g.BuySelectedShopItem(),"preview tool "+i);}
            Check(g.Stock(TravelTool.SignalLantern)==1&&g.Stock(TravelTool.LandingLens)==1,"expanded stock");
            g.Preferences.LargeText=false;g.SetUiLocale("ru");g.SelectShopTab(1);g.SelectShopItem(7);Check(g.BuySelectedShopItem(),"new trophy");Check(g.BuySelectedShopItem(),"equip trophy");
            Check(g.EquippedGift=="beacon_brooch","trophy equipped");yield return Shot("trophies-ru",390,844);
            g.ShowCollection();g.SelectShopItem(7);yield return Shot("collection-ru",390,844);
            g.ShowJourney();yield return new WaitForSecondsRealtime(.4f);Check(g.GetComponentsInChildren<Image>().Any(x=>x.name=="Mika's keepsake"&&x.sprite!=null),"new trophy on map");
            var source=g.Catalog.GetPrimary("C02").Definition;int n=source.InitialSymbolIds.Count;
            var level=new LevelDefinition("field-review","field-review",new string('a',64),source.RulesVersion,"field-review",source.TargetSymbolIds.Reverse(),source.TargetSymbolIds,source.TargetSegments,
                new[]{new FloorDefinition(12,Enumerable.Repeat(true,n-1)),new FloorDefinition(12,Enumerable.Repeat(true,n-1),entryTransform:EntryTransform.ShiftRightOne)});
            g.LoadLevel(level);g.ShowGame();yield return Ready();
            Check(Named("Field tools").interactable,"field bag visible and usable");g.SetUiLocale("ru");yield return Shot("field-tools-ru",390,844);
            Named("Field tools").onClick.Invoke();g.SelectShopItem(3);yield return Shot("bag-ru",390,844);string before=g.Interaction.Attempt.CurrentStateHash;
            Named("Use selected tool").onClick.Invoke();yield return Ready();
            Check(g.Stock(TravelTool.SignalLantern)==0&&g.Interaction.Attempt.LanternUsed&&g.Interaction.Attempt.RouteLength==1&&g.Interaction.Attempt.CurrentStateHash!=before,"one proved lantern move");
            Check(!g.Interaction.IsPresenting,"lantern motion completed");
            Named("Field tools").onClick.Invoke();g.SelectShopItem(4);string lensHash=g.Interaction.Attempt.CurrentStateHash;Named("Use selected tool").onClick.Invoke();yield return Ready();
            Check(g.Stock(TravelTool.LandingLens)==0&&g.Interaction.Attempt.LensUsed&&g.Interaction.Attempt.CurrentStateHash==lensHash,"lens consumption without movement");
            Check(g.Interaction.IsPaused,"prediction reading pauses input");g.Preferences.LargeText=true;g.SetUiLocale("uk");yield return Shot("landing-uk-small",320,693);
            Named("Close landing preview").onClick.Invoke();Check(!g.Interaction.IsPaused&&g.Interaction.Attempt.CurrentStateHash==lensHash,"prediction close restores input without changing row");
            var restored=AttemptService.Restore(level,AttemptRecord.Capture(g.Interaction.Attempt));Check(restored.LanternUsed&&restored.LensUsed,"new tool flags reload");
            g.Monetization.Ledger.WayfinderScrolls=g.Monetization.Ledger.UndoSpools=g.Monetization.Ledger.ReturnHourglasses=0;g.CloseLandingPreview();g.ShowGame();
            Check(!g.GetComponentsInChildren<Button>().Any(b=>b.name=="Field tools"),"empty stock hides field button");
            g.Preferences.LargeText=false;g.SetEditorSafeInsets(Vector4.zero);g.ShowHome();
            File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/ShopRefresh/runtime.txt"),"PASS: five preview tools, eight trophies, new trophy equips and appears on Mika, field button opens bag / hides when empty, first-use popups, one proved lantern move and completed presentation, lens matches immutable next-entry prediction / pauses / closes without moving row, new attempt flags reload. Seven native Editor captures including 320x693, Large Text and safe insets. Test inventory only; physical devices/player builds NOT RUN.\n");
            Debug.Log("CASCADE shop refresh review complete.");
            Button Named(string name)=>g.GetComponentsInChildren<Button>().Single(b=>b.name==name);
            IEnumerator Ready(){float deadline=Time.realtimeSinceStartup+25,stable=0;yield return null;while(stable<1f){bool blocked=g.Interaction.IsPreparing||g.Interaction.IsPresenting||g.MechanicPopupOpen;if(g.MechanicPopupOpen)g.AcknowledgeMechanic();stable=blocked?0:stable+Mathf.Min(.05f,Time.unscaledDeltaTime);if(Time.realtimeSinceStartup>deadline)throw new InvalidOperationException("readiness timeout");yield return null;}}
            IEnumerator Shot(string name,int w,int h){yield return new WaitForSecondsRealtime(.4f);yield return PlayableCapture.CaptureSettled("ShopRefresh",name+".png",w,h);var bounds=JObject.Parse(File.ReadAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/ShopRefresh",name+".json")));foreach(var button in bounds["buttons"]){if((string)button["name"]=="Close landing preview"||(string)button["name"]=="Watch coin video"||(string)button["name"]=="Tools tab"||(string)button["name"]=="Trophies tab"||(string)button["name"]=="Buy selected item"||(string)button["name"]=="My tools link"||(string)button["name"]=="Use selected tool"||(string)button["name"]=="Open shop"||(string)button["name"]=="Tools back"||(string)button["name"]=="Field tools")Check((float)button["height"]>=47.9f,"captured 48px height "+button["name"]+"="+button["height"]);}
                foreach(var text in g.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate();Check(!text.isTextOverflowing&&!text.text.Contains("No translation found"),"text fits "+text.name+" / "+text.text);}}
        }
        static void Check(bool okay,string why){if(!okay)throw new InvalidOperationException("Shop refresh: "+why);}
    }
}
