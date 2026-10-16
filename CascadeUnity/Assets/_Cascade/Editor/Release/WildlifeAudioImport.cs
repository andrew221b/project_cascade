using UnityEditor;

namespace Cascade.Editor.Release
{
    // Only these short animal phrases are resident; long music/ambience keep their existing settings.
    public sealed class WildlifeAudioImport:AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if(!assetPath.StartsWith("Assets/_Cascade/Content/Resources/CascadeArt/Audio/Wildlife/",System.StringComparison.Ordinal))return;
            var importer=(AudioImporter)assetImporter;importer.forceToMono=true;importer.loadInBackground=false;
            var settings=importer.defaultSampleSettings;settings.loadType=UnityEngine.AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=UnityEngine.AudioCompressionFormat.Vorbis;settings.quality=.85f;
            settings.sampleRateSetting=AudioSampleRateSetting.OverrideSampleRate;settings.sampleRateOverride=32000;
            settings.preloadAudioData=true;importer.defaultSampleSettings=settings;
        }
    }
}
