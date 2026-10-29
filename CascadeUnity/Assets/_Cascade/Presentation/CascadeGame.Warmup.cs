using System.Collections;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        // Work starts after the first visible frame. A fast tap never waits for this
        // optional warmup: normal entry points can still load their required assets.
        IEnumerator WarmJourneyResources()
        {
            yield return null;
            if(!CanPresent)yield break;
            var recipes=Resources.LoadAsync<TextAsset>("CascadeContent/production_triad_recipes");
            yield return recipes;
            if(!CanPresent)yield break;
            var manifest=Resources.LoadAsync<TextAsset>("CascadeContent/production_triad_manifest");
            yield return manifest;
            if(!CanPresent)yield break;
            // Certificate parsing/validation stays on the existing worker, not the UI thread.
            PublishedTriads();
            if(CurrentScreen!=GameScreen.Home&&CurrentScreen!=GameScreen.AccountGate)yield break;
            int world=EnvironmentWorld;
            string[] next={"Navigation/WorldComic"+(world/3),"ComicReferenceOct16/TimberFrame",
                "MapReferenceOct14/Backdrop","WorldMenusOct14/MapFrameMask",
                "MapReferenceOct14/Lock","MapReferenceOct14/Progress"};
            foreach(string asset in next){
                if(!CanPresent||CurrentScreen!=GameScreen.Home&&CurrentScreen!=GameScreen.AccountGate)yield break;
                // One outstanding request bounds IO/decompression and upload work.
                yield return Resources.LoadAsync<Texture2D>("CascadeArt/"+asset);
                yield return null;
            }
        }
    }
}
