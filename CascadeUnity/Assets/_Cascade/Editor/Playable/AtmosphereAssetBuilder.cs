using System;
using System.IO;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class AtmosphereAssetBuilder
    {
        const string Art = "Assets/_Cascade/Content/Resources/CascadeArt/";
        const string Audio = Art + "Audio/Atmosphere/";
        public static void Configure()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Configure audio outside Play Mode.");
            string[] scores={"Menu_HomeAmongCloudsBalanced","WindGarden_PaperWings","RainMill_RainOnCedar","CopperOrchard_GoldenAfternoon",
                "CloudPost_LettersOnTheBreeze","GlassLagoon_ReflectedMoon","MossArchive_StoriesInTheLeaves","SnowSpire_WarmLightInSnow","GreatBeacon_TheWayHome","StarObservatory_ConstellationLullaby"};
            foreach(var file in Directory.GetFiles(Audio,"*.wav")) {
                var importer=(AudioImporter)AssetImporter.GetAtPath(file);
                bool score=Array.IndexOf(scores,Path.GetFileNameWithoutExtension(file))>=0||Path.GetFileNameWithoutExtension(file)=="Menu_HomeAmongClouds"||Path.GetFileNameWithoutExtension(file).EndsWith("_Menu");
                var samples=importer.defaultSampleSettings;
                samples.loadType=score?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
                samples.compressionFormat=score?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;
                samples.quality=.72f;samples.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
                samples.preloadAudioData=!score;importer.defaultSampleSettings=samples;
                importer.forceToMono=false;importer.loadInBackground=score;importer.SaveAndReimport();
            }
            var library=AssetDatabase.LoadAssetAtPath<ToyAudioLibrary>(Art+"ToyAudio.asset");
            Undo.RecordObject(library,"Configure CASCADE atmosphere and score");
            library.menuMusic=Clip(scores[0]);library.music=library.menuMusic;library.worldMusic=new AudioClip[9];
            for(int i=0;i<9;i++)library.worldMusic[i]=Clip(scores[i+1]);
            library.naturalWind=Clip("LivingWind");
            string[] cues={"Select_Touch","Swap_Slide","Land_Clacks","Fragile_Creak","Unlock_Turn","Repair_Chimes",
                "Bomb_Puff","Defuse_Release","Bridge_Unfold","Stamp_Paper","Success_Home","Reject_Nudge"};
            library.cues=new AudioClip[cues.Length];library.alternates=new AudioClip[cues.Length];
            for(int i=0;i<cues.Length;i++)library.cues[i]=Clip(cues[i]);
            // Share the material-based samples; tiny pitch variation provides alternate contacts without duplicated audio memory.
            for(int i=0;i<4;i++)library.alternates[i]=library.cues[i];
            EditorUtility.SetDirty(library);
            var glider=(TextureImporter)AssetImporter.GetAtPath(Art+"Journey/Glider.png");
            glider.filterMode=FilterMode.Trilinear;glider.mipmapEnabled=true;glider.alphaIsTransparency=true;glider.SaveAndReimport();
            AssetDatabase.SaveAssets();
        }
        public static void ConfigureSharedWorldScore()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Configure music outside Play Mode.");
            var library=AssetDatabase.LoadAssetAtPath<ToyAudioLibrary>(Art+"ToyAudio.asset");
            Undo.RecordObject(library,"Use one score for each world");
            library.menuMusic=Clip("Menu_HomeAmongCloudsBalanced");library.music=library.menuMusic;
            string[] worlds={"WindGarden_PaperWings","RainMill_RainOnCedar","CopperOrchard_GoldenAfternoon",
                "CloudPost_LettersOnTheBreeze","GlassLagoon_ReflectedMoon","MossArchive_StoriesInTheLeaves",
                "SnowSpire_WarmLightInSnow","GreatBeacon_TheWayHome","StarObservatory_ConstellationLullaby"};
            library.worldMusic=new AudioClip[worlds.Length];
            for(int i=0;i<worlds.Length;i++)library.worldMusic[i]=Clip(worlds[i]);
            EditorUtility.SetDirty(library);AssetDatabase.SaveAssetIfDirty(library);
        }
        static AudioClip Clip(string name)
        {
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Audio+name+".wav");
            if(clip==null)throw new InvalidOperationException("Missing authored audio: "+name);
            return clip;
        }
    }
}
