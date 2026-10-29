using Cascade.Presentation;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Cascade.Editor.Release
{
    // Development previews can use visibly marked drafts; a store submission cannot.
    public sealed class LegalReleaseGuard:IPreprocessBuildWithReport
    {
        public int callbackOrder=>-900;
        public void OnPreprocessBuild(BuildReport report)
        {
            if(report.summary.platform!=UnityEditor.BuildTarget.iOS&&report.summary.platform!=UnityEditor.BuildTarget.Android)return;
            if(Cascade.Editor.Playable.StoreCaptureBuildScope.Active){Cascade.Editor.Playable.StoreCaptureBuildScope.Validate(report);return;}
            // The explicitly-scoped Android catalog bootstrap is an internal, non-production
            // artifact. It keeps visibly marked draft legal copies and cannot enable live
            // monetization, ads or connected accounts. Production builds still pass through
            // every legal approval check below.
            if(Cascade.Editor.Playable.AndroidInternalPreviewScope.Active)return;
            var monetization=Resources.Load<MonetizationSettings>("CascadeArt/MonetizationSettings");
            if(!Cascade.Editor.Playable.MonetizationReleaseGuard.RequiresReleaseChecks(monetization?.productionRelease==true,report.summary.options))return;
            var settings=Resources.Load<LegalSettings>("CascadeArt/LegalSettings");
            if(settings?.HasPublishedDocuments!=true)throw new BuildFailedException("Approve CASCADE's legal documents, fill publisher/contact details and verify public HTTPS Privacy, Terms and Support URLs before store submission.");
            foreach(string name in new[]{"privacy","terms"}){
                var copy=Resources.Load<TextAsset>("CascadeLegal/"+name);
                if(copy==null||copy.text.Contains("[publisher")||copy.text.Contains("contact pending]")||copy.text.Contains("DRAFT —"))
                    throw new BuildFailedException("Regenerate CASCADE's offline legal documents from the approved Website configuration.");
            }
        }
    }
}
