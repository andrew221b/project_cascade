using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class ShopReview
    {
        public static void Run()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Requires an isolated native review profile.");
            game.StartCoroutine(Inspect(game));
        }
        static IEnumerator Inspect(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            game.Monetization.Ledger.BonusCoins=1200; // Isolated QA wallet. Never a production grant.
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("ru");game.ShowHome();game.ShowShop();game.SelectShopItem(2);
            yield return Shot("shop-ru",390,844);
            game.SetUiLocale("en");game.Preferences.LargeText=true;game.SelectShopItem(0);yield return Shot("shop-en-small",320,693);game.Preferences.LargeText=false;
            Click("Buy selected item");Require(game.Stock(TravelTool.Wayfinder)==1&&game.ShopCoins==1080,"wayfinder purchase");
            yield return Shot("shop-after-buy-small",320,693);
            game.SelectShopItem(1);Click("Buy selected item");game.SelectShopItem(2);Click("Buy selected item");Require(game.ShopCoins==800,"bounded tool prices");
            game.SelectShopTab(1);Click("Buy selected item");Require(game.OwnsGift("leaf")&&game.ShopCoins==560,"permanent ownership");Click("Buy selected item");Require(game.Rewards.EquippedGift=="leaf"&&game.ShopCoins==560,"equip without charging twice");
            game.SetUiLocale("ru");yield return Shot("keepsake-owned-ru",390,844);
            game.ShowSupplies();game.SetUiLocale("uk");yield return Shot("bag-uk-small",320,693);
            var source=game.Catalog.GetPrimary("C02").Definition;int n=source.TargetSymbolIds.Count;
            var board=new LevelDefinition("shop-review","shop-review",new string('a',64),source.RulesVersion,"review",source.TargetSymbolIds.Reverse(),source.TargetSymbolIds,source.TargetSegments,
                new[]{new FloorDefinition(12,Enumerable.Repeat(true,n-1)),new FloorDefinition(12,Enumerable.Repeat(true,n-1))});
            game.LoadLevel(board);game.ShowGame();yield return Ready();
            string start=game.Interaction.Attempt.CurrentStateHash;game.SetUiLocale("en");game.ShowSupplies();game.SelectShopItem(0);Click("Use selected tool");yield return Ready();
            Require(game.Stock(TravelTool.Wayfinder)==0&&game.Interaction.Attempt.GuideActive&&game.Interaction.Attempt.CurrentStateHash==start,"guide consumes stock without moving the row");
            game.SetUiLocale("en");yield return Shot("guide-playing-en",390,844);
            yield return new WaitForSecondsRealtime(.5f);yield return Shot("guide-status-en",390,844);
            Require(game.GetComponentInChildren<GameReactionBanner>().StatusVisible,"guide status follows the arrival pulse");
            for(int i=0;i<2;i++){game.Interaction.Execute(PuzzleCommand.Swap(0));yield return Ready();game.Undo();yield return null;}
            Require(game.Interaction.Attempt.FreeUndoRemaining==0,"free Undo allowance");
            game.Interaction.Execute(PuzzleCommand.Swap(0));yield return Ready();game.ShowSupplies();game.SelectShopItem(1);Click("Use selected tool");yield return Ready();
            Require(game.Stock(TravelTool.UndoSpool)==0&&game.Interaction.Attempt.SpoolUsed&&game.Interaction.Attempt.UndoCount==3,"spool consumes once");
            string beforeFall=game.Interaction.Attempt.CurrentStateHash;game.Interaction.Execute(PuzzleCommand.Drop);yield return Ready();
            Require(game.Interaction.Attempt.Current.ActiveFloor==1&&!game.Interaction.Attempt.CanUndo,"ordinary Undo cannot restore destroyed wood");
            game.ShowSupplies();game.SelectShopItem(2);Click("Use selected tool");yield return Ready();
            Require(game.Stock(TravelTool.ReturnHourglass)==0&&game.Interaction.Attempt.CurrentStateHash==beforeFall,"hourglass exact return");
            Require(!game.Interaction.Attempt.GuideActive,"spent guide does not revive with the board");
            var saved=new ProfileStore(game.ProfilePath).Load().Profile;var restored=AttemptService.Restore(board,saved.Attempt);
            Require(saved.Monetization.SpentCoins==640&&saved.Monetization.WayfinderScrolls==0&&saved.Monetization.UndoSpools==0&&saved.Monetization.ReturnHourglasses==0,"durable inventory and expense");
            Require(restored.FreeUndoRemaining==0&&restored.HourglassUsed&&restored.SpoolUsed&&restored.WayfinderUsed&&restored.CurrentStateHash==beforeFall,"durable tool limits and replay");
            game.SetUiLocale("ru");game.ShowSupplies();game.SelectShopItem(2);yield return Shot("bag-empty-ru",390,844);
            game.ShowShop();game.SelectShopTab(0);game.SelectShopItem(2);Click("Buy selected item");Click("Buy selected item");Require(game.ShopCoins==160,"wallet after purchases");
            game.SetUiLocale("en");yield return Shot("shop-insufficient-en",390,844);
            game.SetEditorSafeInsets(Vector4.zero);game.SetUiLocale("ru");yield return Shot("shop-tablet-ru",768,1024);
            File.WriteAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName,"Reports/Shop/runtime.txt"),
                "PASS: native UI purchases, equip without repeat charge, guide and revealed status, two free undos, one paid spool, collapsed-floor restriction, exact hourglass rewind, disk reload, one-use limits, guide expiration, low-balance disabled state. Synthetic 1200-coin wallet in an isolated profile. Ten native captures. No live store, physical input or phone build.\n");
            Debug.Log("CASCADE shop native review complete.");
            IEnumerator Ready(){float limit=Time.realtimeSinceStartup+12;while(game.Interaction.IsPreparing||game.Interaction.IsPresenting){if(Time.realtimeSinceStartup>limit)throw new InvalidOperationException("Review input gate timeout");yield return null;}yield return new WaitForSecondsRealtime(.12f);}
            IEnumerator Shot(string name,int w,int h){yield return new WaitForSecondsRealtime(.30f);yield return PlayableCapture.CaptureSettled("Shop",name+".png",w,h);CheckText();}
            void Click(string name){var b=game.GetComponentsInChildren<Button>().Single(b=>b.name==name);Require(b.interactable,"enabled "+name);b.onClick.Invoke();}
            void CheckText(){foreach(var text in game.GetComponentsInChildren<TextMeshProUGUI>()){text.ForceMeshUpdate();if(text.isTextOverflowing)throw new InvalidOperationException("Overflow: "+text.name+" / "+text.text);}}
        }
        static void Require(bool condition,string check){if(!condition)throw new InvalidOperationException("Shop runtime: "+check);}
    }
}
