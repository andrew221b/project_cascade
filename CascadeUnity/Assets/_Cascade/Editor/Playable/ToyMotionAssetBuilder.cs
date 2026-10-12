using System;
using System.IO;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class ToyMotionAssetBuilder
    {
        const string Root = "Assets/_Cascade/Content/Resources/CascadeArt";
        const int Rate = 22050;
        [MenuItem("CASCADE/Build Motion And Original Audio")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Build motion assets outside Play Mode.");
            var profile = AssetDatabase.LoadAssetAtPath<MotionProfile>(Root + "/MotionProfile.asset");
            if (profile == null) { profile = ScriptableObject.CreateInstance<MotionProfile>(); AssetDatabase.CreateAsset(profile, Root + "/MotionProfile.asset"); }
            Directory.CreateDirectory(Root + "/Audio");
            var library = AssetDatabase.LoadAssetAtPath<ToyAudioLibrary>(Root + "/ToyAudio.asset");
            if (library == null) { library = ScriptableObject.CreateInstance<ToyAudioLibrary>(); AssetDatabase.CreateAsset(library, Root + "/ToyAudio.asset"); }
            Undo.RecordObject(library, "Build original toy sound library");
            library.cues = new AudioClip[12]; library.alternates = new AudioClip[12];
            for (int cue = 0; cue < 12; cue++) {
                library.cues[cue] = MakeCue((ToyCue)cue, 0);
                if (cue == (int)ToyCue.Select || cue == (int)ToyCue.Swap || cue == (int)ToyCue.Land || cue == (int)ToyCue.Fragile) library.alternates[cue] = MakeCue((ToyCue)cue, 1);
            }
            const float seconds = 24; var samples = new float[(int)(Rate * seconds)];
            int[] melody = { 72, 76, 79, 76, 74, 71, 67, 71, 69, 72, 76, 72, 67, 71, 74, 71 };
            for (int note = 0; note < 32; note++) {
                float start = note * .75f, hz = Midi(melody[note % melody.Length]);
                for (int i = 0; i < Rate * 1.4f && (int)(start * Rate) + i < samples.Length; i++) {
                    float t = (float)i / Rate;
                    float tone = Mathf.Sin(t * hz * Mathf.PI * 2) + .25f * Mathf.Sin(t * hz * 2.01f * Mathf.PI * 2);
                    samples[(int)(start * Rate) + i] += tone * Mathf.Exp(-t * 4) * Mathf.Min(1, t / .01f) * .15f;
                }
            }
            // Short boundary fades avoid a sample discontinuity at the loop seam.
            FadeEdges(samples, .03f); library.music = SaveClip("CloudWorkshopLoop", samples, true);
            EditorUtility.SetDirty(library); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }
        static AudioClip MakeCue(ToyCue cue, int variant)
        {
            float[] frequencies = { 680, 420, 260, 900, 1050, 850, 95, 560, 360, 310, 660, 175 };
            float length = cue == ToyCue.Success ? .58f : cue == ToyCue.Bomb ? .34f : cue == ToyCue.Repair ? .28f : .13f;
            var data = new float[(int)(length * Rate)]; uint seed = (uint)(117 + (int)cue * 31 + variant);
            float frequency = frequencies[(int)cue] * (variant == 0 ? 1 : 1.07f), phase = 0, smoothNoise = 0;
            for (int i = 0; i < data.Length; i++) {
                float t = (float)i / Rate, progress = t / length;
                seed = seed * 1664525u + 1013904223u; float noise = (seed >> 9) / 4194304f - 1; smoothNoise = Mathf.Lerp(smoothNoise, noise, .22f);
                float hz = frequency * (cue == ToyCue.Repair ? 1 + progress * .45f : cue == ToyCue.Bomb ? 1 - progress * .6f : 1 - progress * .12f);
                phase += hz * Mathf.PI * 2 / Rate;
                float tone = Mathf.Sin(phase) + .18f * Mathf.Sin(phase * 2.07f);
                if (cue == ToyCue.Success) tone = Mathf.Sin(t * Midi(72) * Mathf.PI * 2) + .6f * Mathf.Sin(t * Midi(76) * Mathf.PI * 2) + .4f * Mathf.Sin(t * Midi(79) * Mathf.PI * 2);
                float noiseMix = cue == ToyCue.Fragile ? .72f : cue == ToyCue.Bomb ? .55f : cue == ToyCue.Stamp ? .45f : .08f;
                float envelope = Mathf.Min(1, t / .004f) * Mathf.Exp(-progress * 6.5f) * Mathf.Min(1, (length-t)/.012f);
                data[i] = Mathf.Clamp((tone * (1-noiseMix) + smoothNoise * noiseMix) * envelope * .48f, -.8f, .8f);
            }
            return SaveClip(cue + (variant == 0 ? "" : "Alternate"), data, false);
        }
        static float Midi(int note) => 440 * Mathf.Pow(2, (note-69)/12f);
        static void FadeEdges(float[] samples, float seconds)
        {
            int n = (int)(Rate*seconds); for (int i=0;i<n;i++) { samples[i]*=(float)i/n; samples[samples.Length-1-i]*=(float)i/n; }
        }
        static AudioClip SaveClip(string name, float[] data, bool streaming)
        {
            string path = Root + "/Audio/" + name + ".wav";
            using (var stream = File.Create(path)) using (var writer = new BinaryWriter(stream)) {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + data.Length*2); writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(Rate); writer.Write(Rate*2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(data.Length*2); foreach (float sample in data) writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));
            }
            AssetDatabase.ImportAsset(path); var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            var settings = importer.defaultSampleSettings; settings.preloadAudioData = !streaming; settings.loadType = streaming ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = streaming ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
            settings.quality = .65f; settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate; importer.defaultSampleSettings = settings; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
