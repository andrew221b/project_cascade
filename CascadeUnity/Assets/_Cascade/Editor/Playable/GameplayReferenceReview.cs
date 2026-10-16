using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Presentation;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class GameplayReferenceReview
    {
        const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        static SymbolId[] Row(string s)=>s.Select(c=>SymbolId.Parse("g:en:"+c)).ToArray();
        static LevelDefinition Fixture(int style,int hp=1)=>new LevelDefinition("visual-reference","C01","1","2.0","visual-reference",Row("SNU"),Row("SUN"),new[]{new TargetSegment(SegmentKind.Word,0,3,"sun")},style==2?new[]{new FloorDefinition(hp,new[]{true,true},gustAfterSwaps:2,gustTransform:EntryTransform.ShiftRightOne),new FloorDefinition(8,new[]{true,true})}:new[]{new FloorDefinition(hp,new[]{true,true},entryTransform:style==0?EntryTransform.MirrorNext:style==1?EntryTransform.ShiftLeftOne:EntryTransform.None)});
        static Button Button(CascadeGame game,string name)=>game.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        static IEnumerator Ready(CascadeGame game){float end=Time.realtimeSinceStartup+12;while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<end)yield return null;if(!game.Interaction.CanPlay)throw new InvalidOperationException("Reference fixture not playable");}
        public static IEnumerator Extra(CascadeGame game,string phase)
        {
            WorldMenusReview.ConfigureWorld(game,0);game.SetUiLocale("ru");game.LoadLevel(Fixture(-1));game.SetUiLocale("ru");game.ShowGame();yield return Ready(game);yield return new WaitForSecondsRealtime(.5f);
            yield return PlayableCapture.CaptureSettled(phase,"reference-gameplay.png",720,1280);
            var economy=(AdService)typeof(CascadeGame).GetProperty("GameEconomy",Flags).GetValue(game);economy.Ledger.UndoSpools=3;economy.Ledger.SignalLanterns=1;economy.Ledger.LandingLenses=2;economy.Ledger.WayfinderScrolls=economy.Ledger.ReturnHourglasses=economy.Ledger.RepairKits=0;
            game.ToggleFieldToolTray();yield return new WaitForSecondsRealtime(.5f);yield return PlayableCapture.CaptureSettled(phase,"reference-help-empty-selection.png",720,1280);
            Button(game,"Field tool 1").onClick.Invoke();yield return PlayableCapture.CaptureSettled(phase,"reference-help-spool-selected.png",720,1280);
            foreach(string locale in new[]{"en","ru","uk"}){game.SetUiLocale(locale);game.Preferences.LargeText=true;foreach(int item in new[]{0,1,2,3,4,5}){Button(game,"Field tool "+item).onClick.Invoke();yield return null;foreach(var t in game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Where(t=>t.name.StartsWith("Reference shop field"))){t.ForceMeshUpdate();if(t.isTextOverflowing||t.isTextTruncated)throw new InvalidOperationException("Help text overflow "+locale+" "+item+" "+t.name);}}var shop=game.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single(t=>t.name=="Reference shop field shop caption");if(shop.text!=(locale=="en"?"Shop":"Магазин"))throw new InvalidOperationException("Stale help shop language: "+locale);yield return PlayableCapture.CaptureSettled(phase,locale+"-help-small.png",320,693);}
            game.Preferences.LargeText=false;game.SetUiLocale("ru");game.ToggleFieldToolTray();yield return new WaitForSecondsRealtime(.3f);
            for(int style=-1;style<3;style++){game.LoadLevel(Fixture(style,8));game.SetUiLocale("ru");game.ShowGame();yield return Ready(game);yield return new WaitForSecondsRealtime(.4f);var rigs=(System.Collections.IList)typeof(ToyMachineView).GetField("floors",Flags).GetValue(game.Machine);var rig=rigs[0];var wear=rig.GetType().GetField("wear");for(int state=0;state<4;state++){yield return PlayableCapture.CaptureSettled(phase,"board-"+(style+1)+"-state-"+state+".png",720,1280,g=>{var frame=new PresentationFrame(g.Interaction.Attempt.Current);typeof(PresentationFrame).GetProperty("Durability").SetValue(frame,state==0?8:state==1?5:2);g.RenderFrame(frame);g.Machine.FractureFloor(0,state==3?.24f:0);});} }
            game.LoadLevel(Fixture(-1));game.SetUiLocale("ru");game.ShowGame();yield return Ready(game);var result=game.Interaction.Execute(PuzzleCommand.Drop);if(result==null||!result.Accepted)throw new InvalidOperationException("Failure fixture rejected");float endTime=Time.realtimeSinceStartup+15;while(game.CurrentScreen!=GameScreen.Fail&&Time.realtimeSinceStartup<endTime)yield return null;yield return new WaitForSecondsRealtime(.6f);yield return PlayableCapture.CaptureSettled(phase,"reference-failure.png",720,1280);
            foreach(string locale in new[]{"en","ru","uk"}){game.SetUiLocale(locale);game.Preferences.LargeText=true;typeof(CascadeGame).GetMethod("SetScreen",Flags).Invoke(game,new object[]{GameScreen.Playing});typeof(CascadeGame).GetMethod("SetScreen",Flags).Invoke(game,new object[]{GameScreen.Fail});yield return new WaitForSecondsRealtime(.35f);yield return PlayableCapture.CaptureSettled(phase,locale+"-failure-small.png",320,693);}
            game.Preferences.LargeText=false;game.SetUiLocale("ru");
        }
    }
}
