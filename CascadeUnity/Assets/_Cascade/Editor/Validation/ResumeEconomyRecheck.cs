using UnityEditor;
using UnityEngine.SceneManagement;

namespace Cascade.Editor.Validation
{
    // Reuses isolated-profile fixtures; does not connect to a live player account.
    public static class ResumeEconomyRecheck
    {
        const string Report="ResumeEconomyOct15";
        [MenuItem("CASCADE/QA/Recheck resume and trophy flow")]
        public static void Runtime()
        {
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new System.InvalidOperationException("The open scene has unsaved changes. Preserve them before running tests; refusing to block MCP on a save dialog.");
            PlayModeValidation.RunSelected(Report,new[]{
            "Cascade.Tests.NavigationResumeTests.ForegroundRefreshCannotResurrectRoundControlsOnTheMap",
            "Cascade.Tests.NavigationResumeTests.FinalDeliveryInterruptedByBackgroundReturnsToOneCompletion",
            "Cascade.Tests.ScreenCosmeticFlowTests.EquippedSceneryRemainsAboveMenuBackgroundAndResumesAfterAbsence",
            "Cascade.Tests.ScreenCosmeticFlowTests.EquippedLeavesAreOpaqueAndDriftWithWind",
            "Cascade.Tests.RoundLoadingLifecycleTests",
            "Cascade.Tests.PauseScreenFlowTests.ContinuePreservesCommittedAttemptAndPauseBlocksPuzzleCommands",
            "Cascade.Tests.PauseScreenFlowTests.SettingsReturnsToPauseAndHomeRetainsTheAttempt",
            "Cascade.Tests.PauseScreenFlowTests.AppPauseAndReducedMotionFreezeDecorationsWithoutChangingTheAttempt"
            });
        }
        [MenuItem("CASCADE/QA/Recheck economy and cloud conflicts")]
        public static void Economy()=>CascadeTestRunner.RunSelectedEditMode(Report,new[]{
            "Cascade.Tests.AdServiceTests","Cascade.Tests.StageHelpEconomyTests",
            "Cascade.Tests.CoinEconomyTests","Cascade.Tests.TrophyEconomyTests",
            "Cascade.Tests.ParcelLootTests","Cascade.Tests.RewardTests",
            "Cascade.Tests.CloudJournalTests","Cascade.Tests.CloudReconcilerTests",
            "Cascade.Tests.CloudReplyReconcilerTests","Cascade.Tests.CloudSchedulingTests",
            "Cascade.Tests.ReleasePersistenceTests"
        });
    }
}
