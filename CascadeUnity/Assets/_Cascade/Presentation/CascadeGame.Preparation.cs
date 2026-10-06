using Cascade.Application;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly PuzzlePreparation preparation = new PuzzlePreparation();
        public bool PreparationReady => preparation.IsReady;
        public int? MinimumMoves => preparation.IsReady ? preparation.Result.OptimalActions : null;
        void BeginPreparation()
        {
            preparation.Begin(Interaction.Attempt.Level, Interaction.Attempt.Goal);
            Interaction.SetPreparing(!preparation.IsReady);
            PaintPreparation();
        }
        void UpdatePreparation()
        {
            if (preparation.Poll()) {
                if(descent==null&&descentTask==null&&FallBackFromUnprovenDirection())return;
                Interaction.SetPreparing(descentPlanFailed||!preparation.IsReady||IsPlanningDescent||IsDescending);
                RenderState();
            }
            PaintPreparation();
        }
        void PaintPreparation()
        {
            if (Interaction != null && Interaction.IsPreparing&&!IsDescending&&!IsPlanningDescent)
                status.SetCopy(descentPlanFailed?"Could not check all stages. Try again.":preparation.HasFailed ? "Could not check this puzzle. Try again or choose another." : "Checking your puzzle…");
        }
    }
}
