using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Cascade.Editor.Playable
{
    // Only the explicit offline capture builder may bypass live-service release
    // gates, and only under an application ID that cannot update the store app.
    public sealed class StoreCaptureBuildScope:IDisposable
    {
        public static bool Active {get;private set;}
        public StoreCaptureBuildScope(){if(Active)throw new InvalidOperationException("Nested capture build.");Active=true;}
        public static void Validate(BuildReport report)
        {
            var group=report.summary.platform==BuildTarget.iOS?BuildTargetGroup.iOS:report.summary.platform==BuildTarget.Android?BuildTargetGroup.Android:BuildTargetGroup.Unknown;
            var settings=Resources.Load<Cascade.Presentation.MonetizationSettings>("CascadeArt/MonetizationSettings");
            if(!Active||group==BuildTargetGroup.Unknown||PlayerSettings.GetApplicationIdentifier(group)!="com.vickand.flyingladders.capture"||settings?.productionRelease==true)
                throw new BuildFailedException("Capture build must use its offline, non-production application identity.");
        }
        public void Dispose(){Active=false;}
    }
}
