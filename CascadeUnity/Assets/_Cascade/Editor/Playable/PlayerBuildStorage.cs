using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    // Packaging only: lossless chunks preserve imported texture/audio bytes.
    public static class PlayerBuildStorage
    {
        public static BuildOptions OptionsFor(BuildTarget target,BuildOptions options)
        {
            // Android already ships in a compressed APK/AAB. Keep its existing
            // packaging until installed-size and loading comparisons justify a change.
            return target==BuildTarget.iOS
                ? (options&~BuildOptions.CompressWithLz4)|BuildOptions.CompressWithLz4HC
                : options;
        }

        [Serializable] sealed class SizeEntry {public string name;public long bytes;public int count;}
        [Serializable] sealed class SizeReport
        {
            public string target,result,options,scope;
            public long unityReportedBytes,outputFileBytes;
            public bool packedAssetDetailsAvailable;
            public SizeEntry[] fileRoles,largestFiles,sourceTypes,largestSourceAssets;
        }

        public static void WriteSizeReport(BuildReport build,string directory)
        {
            if(build==null)throw new ArgumentNullException(nameof(build));
            var files=build.GetFiles();
            var assets=(build.packedAssets??Array.Empty<PackedAssets>()).SelectMany(p=>p.contents).ToArray();
            var output=build.summary.outputPath;
            var data=new SizeReport{
                target=build.summary.platform.ToString(),result=build.summary.result.ToString(),options=build.summary.options.ToString(),
                scope="Unity build/export only. Not installed size or store download size. Source asset sizes are before outer archive compression. iOS Xcode output must be measured separately.",
                unityReportedBytes=checked((long)build.summary.totalSize),outputFileBytes=File.Exists(output)?new FileInfo(output).Length:-1,
                packedAssetDetailsAvailable=assets.Length>0,
                fileRoles=files.GroupBy(f=>f.role??"unknown").Select(g=>new SizeEntry{name=g.Key,bytes=g.Sum(f=>checked((long)f.size)),count=g.Count()}).OrderByDescending(e=>e.bytes).ToArray(),
                largestFiles=files.OrderByDescending(f=>f.size).Take(50).Select(f=>new SizeEntry{name=RelativeFile(f.path,output),bytes=checked((long)f.size),count=1}).ToArray(),
                sourceTypes=assets.GroupBy(a=>a.type==null?"unknown":a.type.Name).Select(g=>new SizeEntry{name=g.Key,bytes=g.Sum(a=>checked((long)a.packedSize)),count=g.Count()}).OrderByDescending(e=>e.bytes).ToArray(),
                largestSourceAssets=assets.GroupBy(a=>a.sourceAssetPath??"unknown").Select(g=>new SizeEntry{name=g.Key,bytes=g.Sum(a=>checked((long)a.packedSize)),count=g.Count()}).OrderByDescending(e=>e.bytes).Take(100).ToArray()
            };
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory,"player-build-size.json"),JsonUtility.ToJson(data,true)+"\n");
        }

        static string RelativeFile(string path,string output)
        {
            if(string.IsNullOrEmpty(path))return "unknown";
            string root=(Path.HasExtension(output)?Path.GetDirectoryName(output):output)?.Replace('\\','/').TrimEnd('/')+"/";
            string normalized=path.Replace('\\','/');
            return normalized.StartsWith(root,StringComparison.Ordinal)?normalized.Substring(root.Length):normalized;
        }
    }
}
