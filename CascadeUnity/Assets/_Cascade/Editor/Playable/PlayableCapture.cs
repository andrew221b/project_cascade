using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class PlayableCapture
    {
        [Serializable] sealed class HitRegion { public string name; public float x, y, width, height; }
        [Serializable] sealed class CaptureMetrics { public int width, height; public float minimumDeviceTextPixels; public List<HitRegion> buttons = new List<HitRegion>(); }
        public static void Capture(string filename, int width = 390, int height = 844)
            => CapturePhase("P11", filename, width, height);
        public static void CapturePhase(string phase, string filename, int width, int height)
        {
            var game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.StartCoroutine(CaptureSettled(phase, filename, width, height));
        }
        public static IEnumerator CaptureSettled(string phase, string filename, int width, int height, Action<CascadeGame> pose = null)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Capture the running playable.");
            if (Path.GetFileName(filename) != filename || Path.GetFileName(phase) != phase || width < 1 || height < 1) throw new ArgumentException("Expected a report filename and positive dimensions.");
            var game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>(); var camera = Camera.main;
            var scaler = game.GetComponentInChildren<UnityEngine.UI.CanvasScaler>();
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            bool oldBackground = UnityEngine.Application.runInBackground; UnityEngine.Application.runInBackground = true;
            bool oldSuspended = game.Interaction.IsSuspended;
            var render = new RenderTexture(width, height, 24); var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            string before = game.Interaction.Attempt.CurrentStateHash;
            try {
                scaler.enabled = false; camera.targetTexture = render; scaler.enabled = true;
                yield return null; game.RefreshLayout(); yield return null; game.RefreshLayout();
                // The explicit camera render also works when the Game view is not repainting.
                yield return null; Canvas.ForceUpdateCanvases();
                game.Interaction.SetSuspended(false); game.RefreshLayout();
                pose?.Invoke(game);
                camera.Render(); RenderTexture.active = render; pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                string folder = Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName, "Reports", phase); Directory.CreateDirectory(folder);
                File.WriteAllBytes(Path.Combine(folder, filename), pixels.EncodeToPNG());
                var metrics = new CaptureMetrics { width = width, height = height, minimumDeviceTextPixels = float.MaxValue };
                var corners = new Vector3[4];
                foreach (var button in game.GetComponentsInChildren<UnityEngine.UI.Button>()) {
                    ((RectTransform)button.transform).GetWorldCorners(corners);
                    var low = camera.WorldToScreenPoint(corners[0]); var high = camera.WorldToScreenPoint(corners[2]);
                    metrics.buttons.Add(new HitRegion { name = button.name, x = low.x, y = low.y, width = high.x-low.x, height = high.y-low.y });
                }
                foreach (var label in game.GetComponentsInChildren<TMPro.TextMeshProUGUI>())
                    if ((label.name == "Devices" || label.name == "DeviceGlyph") && label.text.Length > 0)
                        metrics.minimumDeviceTextPixels = Mathf.Min(metrics.minimumDeviceTextPixels, label.fontSize * scaler.GetComponent<Canvas>().scaleFactor);
                if (metrics.minimumDeviceTextPixels == float.MaxValue) metrics.minimumDeviceTextPixels = 0;
                File.WriteAllText(Path.Combine(folder, Path.GetFileNameWithoutExtension(filename) + ".json"), JsonUtility.ToJson(metrics, true));
                if (game.Interaction.Attempt.CurrentStateHash != before) throw new InvalidOperationException("Rendering changed gameplay state.");
            }
            finally {
                if(pose!=null)game.EndTokenHold();
                scaler.enabled = false; camera.targetTexture = oldTarget; scaler.enabled = true; RenderTexture.active = oldActive;
                game.Interaction.SetSuspended(oldSuspended); UnityEngine.Application.runInBackground = oldBackground;
                game.RefreshLayout(); UnityEngine.Object.DestroyImmediate(pixels); render.Release(); UnityEngine.Object.DestroyImmediate(render);
            }
        }
        public static void CaptureReviewSet()
        {
            if (!EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
                throw new InvalidOperationException("Capture requires the current compiled game in Play Mode.");
            UnityEngine.Application.runInBackground = true;
            UnityEngine.Object.FindFirstObjectByType<CascadeGame>().StartCoroutine(ReviewSet());
        }
        static IEnumerator ReviewSet()
        {
            var game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.LoadNode("C01"); yield return null;
            yield return CaptureSettled("P12", "c01-phone.png", 390, 844);
            yield return CaptureSettled("P12", "c01-wide.png", 1440, 900);
            game.Interaction.SetSuspended(false); game.Interaction.FocusSlot(0); game.RefreshLayout();
            yield return CaptureSettled("P12", "selected-phone.png", 390, 844);
            game.LoadNode("C37"); yield return null;
            yield return CaptureSettled("P12", "c37-phone.png", 390, 844);
            yield return CaptureSettled("P12", "c37-small.png", 320, 693);
            if (game.Catalog.TryGetLocalizedBinding("C37", "en", out var words)) game.LoadLevel(words.Definition, "en"); yield return null;
            yield return CaptureSettled("P12", "words-devices.png", 390, 844);
            game.LoadNode("C32"); yield return null;
            yield return CaptureSettled("P12", "mixed8-small.png", 320, 693);
            game.Interaction.SetSuspended(false); game.Interaction.FocusSlot(0); game.RefreshLayout();
            yield return CaptureSettled("P12", "selected-letter.png", 320, 693);
            game.ReducedMotion = true; yield return null;
            yield return CaptureSettled("P12", "mixed8-reduced.png", 390, 844);
            game.SendMessage("ToggleTargetZoom"); yield return null;
            yield return CaptureSettled("P12", "target-zoom.png", 390, 844);
            game.SendMessage("ToggleTargetZoom"); game.ReducedMotion = false; game.LoadNode("C01");
            UnityEngine.Application.runInBackground = false;
            Debug.Log("CASCADE P12 review captures complete.");
        }
    }
}
