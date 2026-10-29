using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        DescentRun descent;
        StageVarietyStore stageVariety;
        Task<StageVarietyStore> stageVarietyReady;
        PuzzleMode descentMode;
        StageVariation descentVariation;
        Task<DescentRun> descentTask;
        Task<PublishedTriadStore> publishedTriads;
        CancellationTokenSource descentCancellation;
        LevelDefinition descentSource;
        string descentLocale;
        DeliveryMood descentMood;
        DescentTransition descentMotion;
        int paintedDescentStage=-1;
        CanvasGroup descentHeader,descentActions,descentPrecise;
        bool descentPlanFailed,descentTravel,descentLoadPending;
        public DescentRun Descent=>descent;
        public bool IsDescending=>descentMotion!=null&&descentMotion.Playing;
        public bool IsPlanningDescent=>descentLoadPending;
        public void StartDescentLevel(LevelDefinition source,string locale="en",DeliveryMood mood=DeliveryMood.Steady,PuzzleMode? stageMode=null)
        {
            var nativeLocale=source.TargetSymbolIds.Where(s=>s.Kind==SymbolKind.Glyph).Select(s=>s.Locale).FirstOrDefault()??locale;
            var pending=new AttemptService(source,nativeLocale);MountAttempt(pending);
            descentPlanFailed=false;
            descentSource=source;descentLocale=locale;descentMood=mood;
            var mode=stageMode??pending.Mode;
            if(mode==PuzzleMode.Shapes)descentLocale=locale="en";
            descentMode=mode;descentVariation=null;
            descentLoadPending=true;ResumeDescentWork();
            Interaction.SetPreparing(true);status.SetCopy("Checking all three stages…");
        }
        bool RetryVariedStage()
        {
            var source=DescentPublishedSource();if(source==null||descent?.Version!=7)return false;
            var old=descent;bool travel=journeyActive;var previous=Interaction.Attempt;string locale=previous.ActiveLocale;
            var plan=profile.Variety.Renew(source,old.Variation,old.StageIndex,stageVariety.Words);
            var fresh=stageVariety.Personalize(source,plan,old.Stages);fresh.Completed=old.Completed.ToList();fresh.CompletedExplosions=old.CompletedExplosions;fresh.CompletedToolFlags=old.CompletedToolFlags;fresh.Validate(source,locale);
            if(PersonalMissionActive)profile.Missions.Offer.Run=fresh;
            MountAttempt(new AttemptService(fresh.Level(source,fresh.StageIndex),locale),fresh);journeyActive=travel;
            if(travel&&!sessionClears.Contains(node))profile.Director.ObserveRetry(previous.AttemptId,previous.RouteLength>0);
            ResetMechanicDiscovery();PersistProgress(true);CancelHint();CancelDeadEnd();StartCoach();RenderState();if(flowReady)ShowGame();return true;
        }
        void ResumeDescentWork()
        {
            if(!descentLoadPending||descentTask!=null)return;
            if(stageVariety==null){
                if(stageVarietyReady!=null&&stageVarietyReady.IsCompleted&&!stageVarietyReady.IsFaulted&&!stageVarietyReady.IsCanceled)stageVariety=stageVarietyReady.Result;
                else{if(stageVarietyReady?.IsFaulted==true){var ignored=stageVarietyReady.Exception;descentLoadPending=false;descentPlanFailed=true;}return;}
            }
            if(descentVariation==null){if(profile.Variety==null)profile.Variety=new StageVarietyMemory();descentVariation=profile.Variety.Reserve(descentSource,descentMode.ToString(),descentLocale,stageVariety.Words);}
            descentPlanFailed=false;descentCancellation=new CancellationTokenSource();var token=descentCancellation.Token;
            var source=descentSource;var mood=descentMood;var locale=descentLocale;var catalogHash=Catalog.ContentHash;
            var variation=descentVariation;var nativeLocale=Catalog.AllLevels.FirstOrDefault(c=>c.Definition.ContentHash==source.ContentHash)?.Locale??locale;
            var certificates=PublishedTriads();
            descentTask=Task.Run(async()=>{
                var packaged=await certificates;token.ThrowIfCancellationRequested();
                DescentRun run=null;
                try{run=packaged?.Bind(source,nativeLocale,catalogHash);}catch(ArgumentException){}
                token.ThrowIfCancellationRequested();
                run=run??DescentRun.Build(source,mood,token,version:6);token.ThrowIfCancellationRequested();
                return run==null?null:stageVariety.Create(source,variation,run);
            },token);
            Interaction.SetPreparing(true);
        }
        Task<PublishedTriadStore> PublishedTriads()
        {
            if(publishedTriads==null){
                string recipes=Resources.Load<TextAsset>("CascadeContent/production_triad_recipes")?.text;
                string manifest=Resources.Load<TextAsset>("CascadeContent/production_triad_manifest")?.text;
                publishedTriads=Task.Run(()=>{
                    if(recipes==null||manifest==null)return null;
                    try{return new PublishedTriadStore(recipes,manifest);}catch(Exception){return null;}
                });
            }
            return publishedTriads;
        }
        void CancelDescentWork(bool preserveRequest=false)
        {
            descentCancellation?.Cancel();descentCancellation?.Dispose();descentCancellation=null;
            if(descentTask!=null)descentTask.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
            descentTask=null;
            if(!preserveRequest)descentLoadPending=false;
        }
        void UpdateDescent()
        {
            if(stageVariety==null&&stageVarietyReady!=null&&stageVarietyReady.IsCompleted&&!stageVarietyReady.IsFaulted&&!stageVarietyReady.IsCanceled)stageVariety=stageVarietyReady.Result;
            if(descentLoadPending&&descentTask==null)ResumeDescentWork();
            if(descentTask!=null&&descentTask.IsCompleted) {
                var task=descentTask;descentTask=null;descentCancellation.Dispose();descentCancellation=null;
                var source=descentSource;string locale=descentLocale;
                DescentRun run=null;if(task.IsFaulted){var ignored=task.Exception;}else if(!task.IsCanceled)run=task.Result;
                bool travel=descentTravel;
                if(run==null){descentPlanFailed=true;status.SetCopy("Could not check all stages. Try again.");RenderControls();}
                else {
                    var screen=CurrentScreen;
                    MountAttempt(new AttemptService(run.Level(source,0),locale,direction:run.Direction),run);
                    journeyActive=travel;PersistProgress(true);SetScreen(screen);
                }
            }
            bool gated=descentPlanFailed||IsPlanningDescent||IsPlanningDirection||IsDescending||!preparation.IsReady||AwaitingMechanicScan&&CurrentScreen==GameScreen.Playing;
            if(Interaction.IsPreparing!=gated){Interaction.SetPreparing(gated);RenderControls();}
            if(descentTask!=null)status.SetCopy("Checking all three stages…");
            PaintDescent();
        }
        void PaintDescent()
        {
            if(descent==null)return;
            if(paintedDescentStage!=descent.StageIndex){levelLabel.SetCopy(PersonalMissionActive?new UiCopy("Personal mission, stage {0}/3",descent.StageIndex+1):new UiCopy("Level {0}, stage {1}/3",DisplayLevelNumber(node),descent.StageIndex+1));paintedDescentStage=descent.StageIndex;}
            if(IsDescending)status.SetCopy("Going lower…");
            else if(CurrentScreen==GameScreen.Checkpoint)status.SetCopy("The next stage is below. Take the row down.");
        }
        bool IsCheckpoint=>descent!=null&&!descent.IsFinal;
        LevelDefinition DescentPublishedSource()=>Catalog.AllLevels.Concat(legacyContent)
            .FirstOrDefault(c=>c.Definition.ContentHash==descent?.SourceHash)?.Definition;
        public void GoLower()
        {
            if(!Interaction.AllowsControl(PracticeControl.NextStage))return;
            if(!IsCheckpoint||Interaction.Attempt.Current.Status!=PuzzleStatus.Won||!Interaction.CanControl||IsDescending)return;
            if(IntroduceDescentEntry())return;
            if(TryCheckpointConversation())return;
            var source=DescentPublishedSource();if(source==null)return;
            var run=descent;string locale=Interaction.Attempt.ActiveLocale;bool travel=journeyActive;
            var departed=machine.CreateDescentGhost("Departing wooden rig");
            run.AcceptCheckpoint(source,Interaction.Attempt);
            if(PendingLesson==MechanicLesson.Mirror||PendingLesson==MechanicLesson.Rollers||PendingLesson==MechanicLesson.LandingWind)
                CompleteLessonPractice(PendingLesson.Value,false);
            // State and persistence precede travel. A tween cannot advance a stage.
            MountAttempt(new AttemptService(run.Level(source,run.StageIndex),locale,direction:run.Direction),run);
            journeyActive=travel;PersistProgress(true);if(HasSaveError){Destroy(departed.gameObject);return;}ShowGame();
            if(run.Version<4&&(run.Stages[run.StageIndex].Arrival==EntryTransform.ShiftLeftOne||run.Stages[run.StageIndex].Arrival==EntryTransform.ShiftRightOne))
                machine.CueWind(run.Stages[run.StageIndex].Arrival==EntryTransform.ShiftRightOne);
            else if(run.Stages[run.StageIndex].PublishedStart)machine.CueWind(run.StageIndex%2==0);
            if(descentMotion==null)descentMotion=gameObject.AddComponent<DescentTransition>();
            gameTransition?.Settle();machine.HoldDescentPose(true);
            descentMotion.Begin(this,departed,machine.CreateDescentGhost("Arriving wooden rig"));
            Interaction.SetPreparing(IsDescending||!preparation.IsReady);
        }
        void SettleDescent(){descentMotion?.Settle();}
        internal void RevealArrivalWind()
        {
            if(descent?.Version<4||descent?.Version>=5||ReducedMotion||!IsWindFloor(0)||CurrentScreen!=GameScreen.Playing)return;
            bool right=Interaction.Attempt.Level.Floors[0].EntryTransform==EntryTransform.ShiftRightOne;
            machine.CueWind(right);CueDeviceWind(right);AnnounceReaction("The wind moved the row","");
        }
        internal void AppendDescentGraphics(DescentGhost ghost)
        {
            // New labels and illustrated counters must finish layout in this frame,
            // before live UI is hidden. A later LateUpdate would be too late.
            var roots=new System.Collections.Generic.List<RectTransform>{floorArea,receiver};
            if(outcome?.DescentRibbon!=null)roots.Add(outcome.DescentRibbon);
            foreach(var root in roots){
                foreach(var caption in root.GetComponentsInChildren<LocalizedCaption>())caption.RefreshForSnapshot();
                foreach(var text in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>())text.ForceMeshUpdate();
                foreach(var counter in root.GetComponentsInChildren<MenuSpriteCounter>())counter.Refresh();
            }
            Canvas.ForceUpdateCanvases();
            foreach(var root in roots)foreach(var graphic in root.GetComponentsInChildren<UnityEngine.UI.Graphic>()){
                if(!graphic.isActiveAndEnabled||graphic is TMPro.TMP_SubMeshUI)continue;
                float opacity=graphic.canvasRenderer.GetAlpha()*graphic.canvasRenderer.GetInheritedAlpha();
                if(graphic.color.a*opacity<.01f)continue;
                if(graphic is TMPro.TextMeshProUGUI text){
                    if(string.IsNullOrEmpty(text.text))continue;
                    text.ForceMeshUpdate();
                    for(int i=0;i<text.textInfo.materialCount;i++){
                        var info=text.textInfo.meshInfo[i];AppendDescentMesh(ghost,graphic,info.mesh,info.material,opacity);
                    }
                } else AppendDescentMesh(ghost,graphic,graphic.canvasRenderer.GetMesh(),graphic.materialForRendering,opacity);
            }
        }
        static void AppendDescentMesh(DescentGhost ghost,UnityEngine.UI.Graphic graphic,Mesh source,Material sourceMaterial,float opacity)
        {
            if(source==null||source.vertexCount==0||sourceMaterial==null)return;
            var part=new GameObject(graphic.name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(ghost.transform,false);
            // uGUI draws descendants above their boards. Preserve that order for
            // transparent world meshes instead of relying on material batching ties.
            var position=graphic.transform.position;position.z-=ghost.UiLayer++*.001f;
            part.transform.SetPositionAndRotation(position,graphic.transform.rotation);part.transform.localScale=graphic.transform.lossyScale;
            var mesh=UnityEngine.Object.Instantiate(source);ghost.OwnedMeshes.Add(mesh);part.GetComponent<MeshFilter>().sharedMesh=mesh;
            if(opacity<.999f){var colors=mesh.colors32;for(int i=0;i<colors.Length;i++)colors[i].a=(byte)(colors[i].a*opacity);mesh.colors32=colors;}
            var material=new Material(sourceMaterial){renderQueue=2999};material.DisableKeyword("UNITY_UI_CLIP_RECT");
            if(!(graphic is TMPro.TextMeshProUGUI))material.SetTexture("_MainTex",graphic.mainTexture);
            if(material.HasProperty("_Stencil"))material.SetFloat("_Stencil",0);
            if(material.HasProperty("_StencilComp"))material.SetFloat("_StencilComp",8);
            ghost.OwnedMaterials.Add(material);var renderer=part.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        }
        internal void ShowDescentRig(bool visible)
        {
            machine.HoldDescentPose(!visible);machine.SetRigVisible(visible);
            machine.LivingWorld.DescentTravelling=!visible;
            floorArea.gameObject.SetActive(visible);receiver.gameObject.SetActive(visible);
            actions.gameObject.SetActive(true);precise.gameObject.SetActive(PreciseInputActive);
            status.gameObject.SetActive(visible);statusPaper.gameObject.SetActive(visible);
            PaintDescentHud(visible?1:0);
            if(visible){machine.LivingWorld.DescentDepth=descent?.StageIndex??0;RenderState();PaintDescent();}
        }
        internal void PaintDescentHud(float alpha)
        {
            DescentOpacity(header,ref descentHeader,alpha);
            DescentOpacity(actions,ref descentActions,alpha);
            DescentOpacity(precise,ref descentPrecise,alpha);
        }
        static void DescentOpacity(RectTransform rect,ref CanvasGroup group,float alpha)
        {
            if(group==null){group=rect.GetComponent<CanvasGroup>();if(group==null)group=rect.gameObject.AddComponent<CanvasGroup>();}
            group.alpha=alpha;group.blocksRaycasts=group.interactable=alpha>=1;
        }
        internal bool DescentMotionAllowed=>applicationFocused&&!applicationPaused&&!Interaction.IsSuspended&&CurrentScreen==GameScreen.Playing&&isActiveAndEnabled;
    }
}
