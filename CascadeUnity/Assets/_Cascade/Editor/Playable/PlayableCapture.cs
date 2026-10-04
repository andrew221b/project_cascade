using System;
using System.IO;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class PlayableCapture
    {
        public static void Capture(string filename, int width = 390, int height = 844)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Capture the running playable.");
            if (Path.GetFileName(filename) != filename || width < 1 || height < 1) throw new ArgumentException("Expected a report filename and positive dimensions.");
            var game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>(); var camera = Camera.main;
            var scaler = game.GetComponentInChildren<UnityEngine.UI.CanvasScaler>();
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var render = new RenderTexture(width, height, 24); var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            string before = game.Interaction.Attempt.CurrentStateHash;
            try {
                scaler.enabled = false; camera.targetTexture = render; scaler.enabled = true; game.RefreshLayout();
                camera.Render(); RenderTexture.active = render; pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                string folder = Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName, "Reports/P11"); Directory.CreateDirectory(folder);
                File.WriteAllBytes(Path.Combine(folder, filename), pixels.EncodeToPNG());
                if (game.Interaction.Attempt.CurrentStateHash != before) throw new InvalidOperationException("Rendering changed gameplay state.");
            }
            finally {
                scaler.enabled = false; camera.targetTexture = oldTarget; scaler.enabled = true; RenderTexture.active = oldActive;
                game.RefreshLayout(); UnityEngine.Object.DestroyImmediate(pixels); render.Release(); UnityEngine.Object.DestroyImmediate(render);
            }
        }
    }
}
