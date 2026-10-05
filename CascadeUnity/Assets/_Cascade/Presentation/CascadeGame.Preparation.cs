using Cascade.Application;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly PuzzlePreparation preparation = new PuzzlePreparation();
        TextMeshProUGUI minimumLabel;
        int? paintedMinimum;
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
                if(FallBackFromUnprovenDirection())return;
                Interaction.SetPreparing(!preparation.IsReady);
                RenderState();
            }
            PaintPreparation();
        }
        void PaintPreparation()
        {
            if (minimumLabel != null) {
                minimumLabel.gameObject.SetActive((!flowReady || CurrentScreen == GameScreen.Playing) && MinimumMoves.HasValue);
                if (MinimumMoves.HasValue && paintedMinimum != MinimumMoves) {
                    paintedMinimum=MinimumMoves; minimumLabel.SetCopy(new UiCopy("Shortest route: {0} moves", MinimumMoves.Value));
                }
            }
            if (Interaction != null && Interaction.IsPreparing)
                status.SetCopy(preparation.HasFailed ? "Could not check this puzzle. Try again or choose another." : "Checking your puzzle…");
        }
    }
}
