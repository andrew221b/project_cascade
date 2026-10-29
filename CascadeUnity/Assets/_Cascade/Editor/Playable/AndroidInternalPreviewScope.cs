using System;
using System.IO;
using Cascade.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    // Editor-only lifetime for a deliberately offline, upload-signed internal playtest.
    public sealed class AndroidInternalPreviewScope:IDisposable
    {
        public static bool Active { get; private set; }
        AndroidInternalPreviewScope() { }
        public static AndroidInternalPreviewScope Begin()
        {
            if(Active||EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)
                throw new InvalidOperationException("An isolated Android internal preview scope is required.");
            Active=true;
            return new AndroidInternalPreviewScope();
        }
        public void Dispose() { Active=false; }
        public static void Validate(BuildReport report)
        {
            if(!Active||report.summary.platform!=BuildTarget.Android||report.summary.options.HasFlag(BuildOptions.Development))
                throw new BuildFailedException("Explicit non-debuggable Android internal preview required.");
            var monetization=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            var cloud=Resources.Load<CloudPlayerSettings>("CascadeArt/CloudPlayerSettings");
            if(monetization==null||monetization.productionRelease||!monetization.testMonetization||!monetization.localAdTestScreen||!monetization.admobTestAds)
                throw new BuildFailedException("Internal preview must use the local test shop and local ad simulation.");
            if(cloud==null||cloud.enabledForPlayers)
                throw new BuildFailedException("Internal preview cannot enable connected player services.");
            if(!PlayerSettings.Android.useCustomKeystore||!File.Exists(PlayerSettings.Android.keystoreName))
                throw new BuildFailedException("Internal Play preview requires upload signing.");
            if(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)!="com.vickand.flyingladders")
                throw new BuildFailedException("Internal preview package must match the authorized Play app.");
        }

    }
}
