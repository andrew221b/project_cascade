using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Domain;
using Cascade.Presentation;
using Cascade.Solver;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Editor.Playable
{
    // Captures actual intermediate runtime poses. No layout rebuild occurs during a command.
    public static class MotionCapture
    {
        [Serializable] sealed class Sample
        {
            public string file, level, stage, hash;
            public int command, eventIndex, frame;
            public float elapsed, maximumGlyphOffset;
        }
        [Serializable] sealed class Report { public List<Sample> samples = new List<Sample>(); }
        public static void CaptureReviewSet()
        {
            if (!EditorApplication.isPlaying || EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
                throw new InvalidOperationException("Motion capture requires the current compiled Play Mode.");
            var game = UnityEngine.Object.FindFirstObjectByType<CascadeGame>();
            game.StartCoroutine(Capture(game));
        }
        static IEnumerator Capture(CascadeGame game)
        {
            var camera = Camera.main; var scaler = game.GetComponentInChildren<CanvasScaler>();
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            bool background = UnityEngine.Application.runInBackground, sound = game.Feedback.SoundEnabled, music = game.Feedback.MusicEnabled;
            var render = new RenderTexture(390, 844, 24); var pixels = new Texture2D(390, 844, TextureFormat.RGB24, false);
            string folder = Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName, "Reports", "P13", "motion");
            Directory.CreateDirectory(folder); var report = new Report();
            UnityEngine.Application.runInBackground = true; game.Feedback.SoundEnabled = game.Feedback.MusicEnabled = false;
            try {
                scaler.enabled = false; camera.targetTexture = render; scaler.enabled = true;
                yield return null; game.RefreshLayout(); yield return null; game.RefreshLayout();
                var cases = new[] { "C03", "C09", "C11", "C14", "C18", "C25", "C38", "C32", "C36", "C40", "mirror-fixture", "C37" };
                foreach (string id in cases) {
                    if (id == "mirror-fixture") {
                        var source = game.Catalog.GetPrimary("C03").Definition;
                        var edges = Enumerable.Repeat(true, source.InitialSymbolIds.Count - 1).ToArray();
                        // QA-only fixture. No campaign data, persisted IDs or production levels are changed.
                        game.LoadLevel(new LevelDefinition("QA_MIRROR", source.CampaignNodeId, source.SchemaVersion,
                            source.RulesVersion, source.ContentHash, source.TargetSymbolIds.Reverse(), source.TargetSymbolIds,
                            source.TargetSegments, new[] { new FloorDefinition(1, edges), new FloorDefinition(1, edges, entryTransform: EntryTransform.MirrorNext) }));
                    } else game.LoadNode(id);
                    game.ReducedMotion = id == "C37";
                    game.SendMessage("OnApplicationFocus", true); game.SendMessage("OnApplicationPause", false);
                    yield return null; game.RefreshLayout(); yield return null;
                    var solution = PuzzleSolver.FindShortest(game.Interaction.Attempt.Resolver);
                    if (solution.Status != SearchStatus.Solved) throw new InvalidOperationException("No capture route for " + id);
                    int commandIndex = 0, frameIndex = 0;
                    Save("before", -1, 0);
                    foreach (var command in solution.Path) {
                        var result = game.Interaction.Execute(command);
                        if (result == null || !result.Accepted) throw new InvalidOperationException("Capture command rejected: " + id);
                        string committed = game.Interaction.Attempt.CurrentStateHash;
                        float start = Time.realtimeSinceStartup, nextSample = start;
                        do {
                            yield return null;
                            if (game.Interaction.Attempt.CurrentStateHash != committed) throw new InvalidOperationException("Motion changed committed state");
                            if (Time.realtimeSinceStartup >= nextSample) {
                                Save(game.EventPlayer.CurrentEvent.ToString(), game.EventPlayer.EventIndex, Time.realtimeSinceStartup-start);
                                nextSample = Time.realtimeSinceStartup + .035f;
                            }
                            if (Time.realtimeSinceStartup - start > 4) throw new InvalidOperationException("Animation did not finish");
                        } while (game.EventPlayer.IsRunning);
                        Save("settled", -1, Time.realtimeSinceStartup-start); commandIndex++;
                    }
                    void Save(string stage, int eventIndex, float elapsed)
                    {
                        string filename = id + (game.ReducedMotion ? "-reduced" : "") + "-" + (frameIndex++).ToString("D3") + ".png";
                        camera.Render(); RenderTexture.active = render; pixels.ReadPixels(new Rect(0, 0, 390, 844), 0, 0); pixels.Apply();
                        File.WriteAllBytes(Path.Combine(folder, filename), pixels.EncodeToPNG());
                        float offset = 0;
                        for (int i = 0; i < game.SlotViews.Count; i++) {
                            var face = game.SlotViews[i].GetComponentInChildren<TMPro.TextMeshProUGUI>(true).rectTransform;
                            offset = Mathf.Max(offset, Mathf.Abs(face.position.y-game.Machine.PresentedTokenPosition(i).y));
                        }
                        report.samples.Add(new Sample { file = filename, level = id, stage = stage, hash = game.Interaction.Attempt.CurrentStateHash,
                            command = commandIndex, eventIndex = eventIndex, frame = frameIndex-1, elapsed = elapsed, maximumGlyphOffset = offset });
                    }
                }
                File.WriteAllText(Path.Combine(folder, "timeline.json"), JsonUtility.ToJson(report, true));
                Debug.Log("CASCADE P13 captured " + report.samples.Count + " actual motion frames.");
            }
            finally {
                game.EventPlayer.Skip(); game.ReducedMotion = false;
                scaler.enabled = false; camera.targetTexture = oldTarget; scaler.enabled = true; RenderTexture.active = oldActive;
                game.Feedback.SoundEnabled = sound; game.Feedback.MusicEnabled = music;
                UnityEngine.Application.runInBackground = background; game.LoadNode("C01"); game.RefreshLayout();
                UnityEngine.Object.DestroyImmediate(pixels); render.Release(); UnityEngine.Object.DestroyImmediate(render);
            }
        }
    }
}
