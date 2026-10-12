using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Bounded native review: never rebuild the layout in the middle of a gesture/fall.
    public static class TouchRevisionCapture
    {
        public static void Run()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Requires Play Mode.");
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.StartCoroutine(Capture(game));
        }
        public static void ConfirmCorrections()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.StartCoroutine(Corrected(game));
        }
        public static void ConfirmReviewFindings()
        {
            var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();game.StartCoroutine(Footer(game));
        }
        static IEnumerator Footer(CascadeGame game)
        {
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("ru");game.ShowNoAds();yield return new WaitForSecondsRealtime(.3f);
            yield return PlayableCapture.CaptureSettled("TouchRevision","purchase-ru.png",390,844);
            yield return PlayableCapture.CaptureSettled("TouchRevision","purchase-small-ru.png",320,693);
            Debug.Log("CASCADE reviewed footer bounds captured at both phone sizes.");
        }
        static IEnumerator Corrected(CascadeGame game)
        {
            game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;
            game.ReducedMotion=false;game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("en");game.LoadNode("C03");game.ShowGame();yield return Ready(game);
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var previous=camera.targetTexture;var active=RenderTexture.active;
            var render=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            try{
                scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;yield return null;game.RefreshLayout();yield return null;
                game.Feedback.RowPhysics.BeginShake(new Vector2(.85f,-.3f));game.Machine.ShakeSuspension(new Vector2(.85f,-.3f));
                yield return new WaitForSecondsRealtime(.16f);camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();
                File.WriteAllBytes(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/TouchRevision/shake.png")),pixels.EncodeToPNG());
            }finally{scaler.enabled=false;camera.targetTexture=previous;scaler.enabled=true;RenderTexture.active=active;UnityEngine.Object.Destroy(pixels);render.Release();UnityEngine.Object.Destroy(render);game.RefreshLayout();}
            game.SetUiLocale("ru");game.ShowNoAds();yield return new WaitForSecondsRealtime(.3f);yield return PlayableCapture.CaptureSettled("TouchRevision","purchase-ru.png",390,844);
            Debug.Log("CASCADE touch corrections confirmed natively.");
        }
        static IEnumerator Ready(CascadeGame game)
        {
            float deadline=Time.realtimeSinceStartup+30;
            while(!game.Interaction.CanPlay&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!game.Interaction.CanPlay)throw new InvalidOperationException("Review input did not become ready.");
        }
        static IEnumerator Capture(CascadeGame game)
        {
            UnityEngine.Application.runInBackground=true;game.SendMessage("OnApplicationFocus",true);game.SendMessage("OnApplicationPause",false);
            game.Feedback.SoundEnabled=game.Feedback.MusicEnabled=false;game.ReducedMotion=false;
            game.SetEditorSafeInsets(new Vector4(.018f,.04f,.018f,.066f));game.SetUiLocale("en");
            var camera=Camera.main;var scaler=game.GetComponentInChildren<CanvasScaler>();var previous=camera.targetTexture;
            var active=RenderTexture.active;var render=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../Reports/TouchRevision"));Directory.CreateDirectory(folder);
            try{
                scaler.enabled=false;camera.targetTexture=render;scaler.enabled=true;yield return null;
                var source=game.Catalog.GetPrimary("C01").Definition;
                // Synthetic review fixture, never written into campaign content or the player's profile.
                var fixture=new LevelDefinition(source.PuzzleId,source.CampaignNodeId,source.SchemaVersion,source.RulesVersion,source.ContentHash,
                    source.InitialSymbolIds,source.TargetSymbolIds,source.TargetSegments,
                    new[]{new FloorDefinition(8,Enumerable.Repeat(true,source.InitialSymbolIds.Count-1)),new FloorDefinition(8,Enumerable.Repeat(true,source.InitialSymbolIds.Count-1))});
                game.LoadLevel(fixture);game.ShowGame();yield return Ready(game);game.RefreshLayout();yield return new WaitForSecondsRealtime(.25f);
                var first=Pointer(0);game.SlotViews[0].OnPointerDown(first);game.SlotViews[0].OnPointerUp(first);
                yield return new WaitForSecondsRealtime(.25f);Shot("tap-selected.png");
                var second=Pointer(1);game.SlotViews[1].OnPointerDown(second);game.SlotViews[1].OnPointerUp(second);
                yield return new WaitForSecondsRealtime(.13f);Shot("tap-exchange.png");yield return Ready(game);
                game.LoadLevel(fixture);game.ShowGame();yield return Ready(game);game.RefreshLayout();yield return null;
                var drag=Pointer(0);game.SlotViews[0].OnPointerDown(drag);game.SlotViews[0].OnBeginDrag(drag);
                drag.position=Point(1)+Vector2.up*45;drag.pointerCurrentRaycast=new RaycastResult{gameObject=game.SlotViews[1].gameObject};
                game.SlotViews[0].OnDrag(drag);yield return new WaitForSecondsRealtime(.1f);Shot("drag-held.png");
                game.SlotViews[0].OnPointerUp(drag);yield return new WaitForSecondsRealtime(.12f);Shot("drag-exchange.png");yield return Ready(game);
                game.Interaction.Execute(PuzzleCommand.Drop);float timeout=Time.realtimeSinceStartup+15;
                while((!game.Feedback.RowPhysics.Active||game.Feedback.RowPhysics.LandedCount==0)&&Time.realtimeSinceStartup<timeout)yield return null;
                if(!game.Feedback.RowPhysics.Active)throw new InvalidOperationException("Landing review missed physical phase.");
                yield return new WaitForSecondsRealtime(.07f);Shot("landing-contact.png");yield return Ready(game);
                var shake=game.GetComponent<PhoneShakeFeedback>();shake.SampleAcceleration(new Vector3(0,-1,0),.04f);shake.SampleAcceleration(new Vector3(1.4f,-1.4f,0),.04f);
                yield return new WaitForSecondsRealtime(.2f);Shot("shake.png");
                yield return new WaitForSecondsRealtime(1.1f);
                game.SetUiLocale("ru");game.Catalog.TryGetLocalizedBinding("C37","ru",out var word);game.LoadLevel(word.Definition,"ru");game.ShowGame();yield return Ready(game);game.RefreshLayout();yield return null;
                var letter=Pointer(0);game.SlotViews[0].OnPointerDown(letter);yield return new WaitForSecondsRealtime(.2f);Shot("letter-held.png");game.EndTokenHold();game.Interaction.CancelPointer();
            }finally{
                game.EndTokenHold();scaler.enabled=false;camera.targetTexture=previous;scaler.enabled=true;RenderTexture.active=active;
                UnityEngine.Object.Destroy(pixels);render.Release();UnityEngine.Object.Destroy(render);game.RefreshLayout();
            }
            game.ShowShop();yield return new WaitForSecondsRealtime(.3f);yield return PlayableCapture.CaptureSettled("TouchRevision","shop-small-ru.png",320,693);
            game.ShowNoAds();yield return new WaitForSecondsRealtime(.3f);yield return PlayableCapture.CaptureSettled("TouchRevision","purchase-ru.png",390,844);
            File.WriteAllText(Path.Combine(folder,"capture-complete.txt"),"9 native Unity Editor renders. Isolated profile; synthetic two-floor motion fixture. No physical-device sensor, store, ad or performance validation.\n");
            Debug.Log("CASCADE touch revision native capture complete.");
            Vector2 Point(int slot){var rect=(RectTransform)game.SlotViews[slot].transform;return RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center));}
            PointerEventData Pointer(int slot)=>new PointerEventData(EventSystem.current){pointerId=991,button=PointerEventData.InputButton.Left,position=Point(slot),pointerCurrentRaycast=new RaycastResult{gameObject=game.SlotViews[slot].gameObject}};
            void Shot(string name){camera.Render();RenderTexture.active=render;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name),pixels.EncodeToPNG());}
        }
    }
}
