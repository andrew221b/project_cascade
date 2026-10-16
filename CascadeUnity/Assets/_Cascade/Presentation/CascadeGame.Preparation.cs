using Cascade.Application;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly PuzzlePreparation preparation = new PuzzlePreparation();
        public bool PreparationReady => preparation.IsReady&&!IsPlanningDescent&&!IsPlanningDirection&&!descentPlanFailed;
        public int? MinimumMoves => PreparationReady ? preparation.Result.OptimalActions : null;
        void BeginPreparation()
        {
            preparation.Begin(Interaction.Attempt.Level, Interaction.Attempt.Goal);
            Interaction.SetPreparing(!preparation.IsReady||AwaitingMechanicScan||IsPlanningDescent||IsPlanningDirection);
            PaintPreparation();
        }
        void UpdatePreparation()
        {
            if (preparation.Poll()) {
                if(descent==null&&!IsPlanningDescent&&!IsPlanningDirection&&FallBackFromUnprovenDirection())return;
                Interaction.SetPreparing(descentPlanFailed||!preparation.IsReady||IsPlanningDescent||IsPlanningDirection||IsDescending||AwaitingMechanicScan);
                RenderState();
            }
            TryFirstMechanic();PaintPreparation();
        }
        void PaintPreparation()
        {
            if (Interaction != null && Interaction.IsPreparing&&!IsDescending&&!IsPlanningDescent&&!IsPlanningDirection)
                status.SetCopy(descentPlanFailed?"Could not check all stages. Try again.":preparation.HasFailed ? "Could not check this puzzle. Try again or choose another." : "Checking your puzzle…");
        }
    }
}
