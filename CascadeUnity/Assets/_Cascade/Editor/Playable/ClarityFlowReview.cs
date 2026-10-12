using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    public static class ClarityFlowReview
    {
        public static void ReviewToolCoach()
        {
            var g=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||g==null||!g.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated review required.");
            UnityEngine.Application.runInBackground=true;g.StartCoroutine(ToolCoach(g));
        }
        static IEnumerator ToolCoach(CascadeGame g)
        {
            const BindingFlags f=BindingFlags.Instance|BindingFlags.NonPublic;
            g.Feedback.SoundEnabled=g.Feedback.MusicEnabled=false;g.ReducedMotion=true;
            g.SendMessage("OnApplicationFocus",true);g.SendMessage("OnApplicationPause",false);g.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));g.SetUiLocale("ru");
            g.LoadLevel(g.Catalog.GetPrimary("C03").Definition,"en");g.ShowGame();
            for(int i=0;i<150&&!g.PreparationReady;i++)yield return null;
            Check(g.PreparationReady,"Tool coach needs a prepared puzzle.");g.ShowSupplies();
            string hash=g.Interaction.Attempt.CurrentStateHash;int coins=g.ShopCoins;
            typeof(CascadeGame).GetMethod("BeginLessonPractice",f).Invoke(g,new object[]{Cascade.Application.MechanicLesson.Wayfinder});
            yield return null;
            var halo=typeof(CascadeGame).GetField("lessonHaloTarget",f);
            Check(((Button)halo.GetValue(g))?.name=="Shop object 0","Finger points to the required shelf item.");
            yield return PlayableCapture.CaptureSettled("ClarityFinal-Oct08","52-coach-shelf.png",390,844,Pose);
            g.SelectShopItem(1);Check((int)typeof(CascadeGame).GetField("shopItem",f).GetValue(g)==0,"Other tools stay blocked during teaching.");
            g.SelectShopItem(0);yield return null;
            Check(((Button)halo.GetValue(g))?.name=="Use selected tool","Finger points to Use after opening the item.");
            yield return PlayableCapture.CaptureSettled("ClarityFinal-Oct08","53-coach-detail.png",390,844,Pose);
            Check(g.Interaction.Attempt.CurrentStateHash==hash&&g.ShopCoins==coins,"Teaching navigation does not spend coins or move the row.");
            typeof(CascadeGame).GetMethod("ClearLessonPractice",f).Invoke(g,null);
            File.WriteAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/ClarityFinal-Oct08/tool-coach.txt")),"PASS: finger targets shelf then Use; other tool selection blocked; puzzle hash and coins unchanged.");
            void Pose(CascadeGame owner){
                typeof(CascadeGame).GetMethod("UpdateCoaching",f).Invoke(owner,new object[]{true});
                var hand=(UnityEngine.UI.Image)typeof(CascadeGame).GetField("tutorialHand",f).GetValue(owner);
                Check(hand.gameObject.activeInHierarchy&&hand.sprite!=null&&hand.color.a>.9f,"Teaching finger is visible after portrait layout.");
                var target=(RectTransform)((Button)halo.GetValue(owner)).transform;
                Check(Vector3.Distance(hand.rectTransform.position,target.TransformPoint(target.rect.center))<.001f,"Finger contacts the control centre, independent of its layout pivot.");
            }
        }
        public static void Run()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            if(!EditorApplication.isPlaying||game==null||!game.ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("Isolated UI review required.");
            string hash=game.Interaction.Attempt.CurrentStateHash;int coins=game.ShopCoins;
            var field=typeof(CascadeGame).GetField("shopDetailOpen",BindingFlags.Instance|BindingFlags.NonPublic);
            game.ShowShop();game.SelectShopTab(1);game.SelectShopItem(17);
            Check((bool)field.GetValue(game),"Selecting a shelf item opens the large detail.");
            game.ShowTravelBag();game.GetComponentsInChildren<Button>().Single(b=>b.name=="Back").onClick.Invoke();
            Check(game.CurrentScreen==GameScreen.Shop&&(bool)field.GetValue(game),"My things returns to the same detail.");
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Back").onClick.Invoke();
            Check(game.CurrentScreen==GameScreen.Shop&&!(bool)field.GetValue(game),"Detail Back returns to the catalogue.");
            game.SelectShopTab(0);game.SelectShopItem(4);
            Check((bool)field.GetValue(game),"Tool opens a large detail.");
            game.GetComponentsInChildren<Button>().Single(b=>b.name=="Back").onClick.Invoke();
            Check(!(bool)field.GetValue(game),"Tool Back returns to its shelf.");
            Check(game.Interaction.Attempt.CurrentStateHash==hash&&game.ShopCoins==coins,"Navigation never moves the row or spends coins.");
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/ClarityFinal-Oct08"));Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"flow.txt"),"PASS: trophy/tool detail, shelf Back, My things parent restore, unchanged puzzle hash and coin balance. Isolated Editor profile.");
        }
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    }
}
