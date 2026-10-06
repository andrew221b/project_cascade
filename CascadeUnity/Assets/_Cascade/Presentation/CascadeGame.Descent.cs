using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        DescentRun descent;
        Task<DescentRun> descentTask;
        CancellationTokenSource descentCancellation;
        LevelDefinition descentSource;
        string descentLocale;
        DeliveryMood descentMood;
        DescentTransition descentMotion;
        int paintedDescentStage=-1;
        bool descentPlanFailed,descentTravel;
        public DescentRun Descent=>descent;
        public bool IsDescending=>descentMotion!=null&&descentMotion.Playing;
        public bool IsPlanningDescent=>descentTask!=null;
        public void StartDescentLevel(LevelDefinition source,string locale="en",DeliveryMood mood=DeliveryMood.Steady)
        {
            LoadDirectedLevel(source,locale,mood);
            descentPlanFailed=false;
            descentSource=source;descentLocale=locale;descentMood=mood;
            descentCancellation=new CancellationTokenSource();var token=descentCancellation.Token;
            descentTask=Task.Run(()=>DescentRun.Build(source,mood,token,version:4),token);
            Interaction.SetPreparing(true);status.SetCopy("Checking all three stages…");
        }
        void CancelDescentWork()
        {
            descentCancellation?.Cancel();descentCancellation?.Dispose();descentCancellation=null;
            if(descentTask!=null)descentTask.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
            descentTask=null;
        }
        void UpdateDescent()
        {
            if(descentTask!=null&&descentTask.IsCompleted) {
                var task=descentTask;descentTask=null;descentCancellation.Dispose();descentCancellation=null;
                var source=descentSource;string locale=descentLocale;var mood=descentMood;
                DescentRun run=null;if(task.IsFaulted){var ignored=task.Exception;}else if(!task.IsCanceled)run=task.Result;
                bool travel=descentTravel;
                if(run==null){descentPlanFailed=true;status.SetCopy("Could not check all stages. Try again.");RenderControls();}
                else {MountAttempt(new AttemptService(run.Level(source,0),locale,direction:run.Direction),run);journeyActive=travel;PersistProgress();ShowGame();}
            }
            bool gated=descentPlanFailed||IsPlanningDescent||IsDescending||!preparation.IsReady;
            if(Interaction.IsPreparing!=gated)Interaction.SetPreparing(gated);
            if(descentTask!=null)status.SetCopy("Checking all three stages…");
            PaintDescent();
        }
        void PaintDescent()
        {
            if(descent==null)return;
            if(paintedDescentStage!=descent.StageIndex){levelLabel.SetCopy(new UiCopy("Level {0} · {1}/3",node.TrimStart('C','0'),descent.StageIndex+1));paintedDescentStage=descent.StageIndex;}
            if(IsDescending)status.SetCopy("Going lower…");
            else if(CurrentScreen==GameScreen.Checkpoint)status.SetCopy("The next stage is below. Take the row down.");
        }
        bool IsCheckpoint=>descent!=null&&!descent.IsFinal;
        LevelDefinition DescentPublishedSource()=>Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(legacyContent)
            .FirstOrDefault(c=>c.Definition.ContentHash==descent?.SourceHash)?.Definition;
        public void GoLower()
        {
            if(!IsCheckpoint||Interaction.Attempt.Current.Status!=PuzzleStatus.Won||!Interaction.CanControl||IsDescending)return;
            var source=DescentPublishedSource();if(source==null)return;
            var run=descent;string locale=Interaction.Attempt.ActiveLocale;bool travel=journeyActive;
            var departed=machine.CreateDescentGhost("Departing wooden rig");
            run.AcceptCheckpoint(source,Interaction.Attempt);
            // State and persistence precede travel. A tween cannot advance a stage.
            MountAttempt(new AttemptService(run.Level(source,run.StageIndex),locale,direction:run.Direction),run);
            journeyActive=travel;PersistProgress();ShowGame();
            if(run.Version<4&&(run.Stages[run.StageIndex].Arrival==EntryTransform.ShiftLeftOne||run.Stages[run.StageIndex].Arrival==EntryTransform.ShiftRightOne))
                machine.CueWind(run.Stages[run.StageIndex].Arrival==EntryTransform.ShiftRightOne);
            else if(run.Stages[run.StageIndex].PublishedStart)machine.CueWind(run.StageIndex%2==0);
            if(descentMotion==null)descentMotion=gameObject.AddComponent<DescentTransition>();
            descentMotion.Begin(this,departed,machine.CreateDescentGhost("Arriving wooden rig"));
            Interaction.SetPreparing(IsDescending||!preparation.IsReady);
        }
        void SettleDescent(){descentMotion?.Settle();}
        internal void RevealArrivalWind()
        {
            if(descent?.Version<4||ReducedMotion||!IsWindFloor(0)||CurrentScreen!=GameScreen.Playing)return;
            bool right=Interaction.Attempt.Level.Floors[0].EntryTransform==EntryTransform.ShiftRightOne;
            machine.CueWind(right);CueDeviceWind(right);AnnounceReaction(right?"Wind slides the row right":"Wind slides the row left","");
        }
        internal void AppendDescentGraphics(DescentGhost ghost)
        {
            Canvas.ForceUpdateCanvases();
            foreach(var graphic in floorArea.GetComponentsInChildren<UnityEngine.UI.Graphic>().Concat(receiver.GetComponentsInChildren<UnityEngine.UI.Graphic>())) {
                if(!graphic.enabled||graphic.color.a<.01f)continue;
                var text=graphic as TMPro.TextMeshProUGUI;
                if(text!=null){if(string.IsNullOrEmpty(text.text))continue;text.ForceMeshUpdate();}
                var source=text!=null?text.mesh:graphic.canvasRenderer.GetMesh();if(source==null||source.vertexCount==0)continue;
                var part=new GameObject(graphic.name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(ghost.transform,false);
                part.transform.SetPositionAndRotation(graphic.transform.position,graphic.transform.rotation);part.transform.localScale=graphic.transform.lossyScale;
                var mesh=UnityEngine.Object.Instantiate(source);ghost.OwnedMeshes.Add(mesh);part.GetComponent<MeshFilter>().sharedMesh=mesh;
                var material=text!=null?new Material(text.fontSharedMaterial):new Material(Shader.Find("UI/Default"));
                if(text==null)material.SetTexture("_MainTex",graphic.mainTexture);
                if(material.HasProperty("_Stencil"))material.SetFloat("_Stencil",0);
                if(material.HasProperty("_StencilComp"))material.SetFloat("_StencilComp",8);
                ghost.OwnedMaterials.Add(material);var renderer=part.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            }
        }
        internal void ShowDescentRig(bool visible)
        {
            machine.SetRigVisible(visible);floorArea.gameObject.SetActive(visible);receiver.gameObject.SetActive(visible);
            actions.gameObject.SetActive(visible);precise.gameObject.SetActive(visible&&PreciseInputActive);
            if(visible){machine.LivingWorld.DescentDepth=descent?.StageIndex??0;RenderState();PaintDescent();}
        }
        internal bool DescentMotionAllowed=>applicationFocused&&!applicationPaused&&!Interaction.IsSuspended&&CurrentScreen==GameScreen.Playing&&isActiveAndEnabled;
    }
}
