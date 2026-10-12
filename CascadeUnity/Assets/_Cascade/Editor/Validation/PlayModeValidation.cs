using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Cascade.Editor.Validation
{
    [InitializeOnLoad]
    public static class PlayModeValidation
    {
        const string PendingKey = "Cascade.PlayModeReport";
        static TestRunnerApi runner;
        static Results callbacks;
        static PlayModeValidation()
        {
            string report = SessionState.GetString(PendingKey, "");
            if (!string.IsNullOrEmpty(report)) Register(report);
        }
        static void Register(string report)
        {
            if (runner != null && callbacks != null) runner.UnregisterCallbacks(callbacks);
            runner = ScriptableObject.CreateInstance<TestRunnerApi>(); runner.hideFlags = HideFlags.HideAndDontSave;
            callbacks = new Results(report); runner.RegisterCallbacks(callbacks);
        }
        public static void Run(string phase)
            =>RunTests(phase,null);
        public static void RunFocused(string phase,string testName)
            =>RunTests(phase,new[]{testName});
        public static void RunSelected(string phase,string[] testNames)
            =>RunTests(phase,testNames);
        [MenuItem("CASCADE/QA/Run night ambience test")]
        public static void RunNightAmbience()
            =>RunFocused("WorldAtmosphere-Oct07","Cascade.Tests.NightCricketLifecycleTests.CricketsBeginAfterQuietNeverOverlapAndStopWithTheirOwner");
        [MenuItem("CASCADE/QA/Run audio and haptic tests")]
        public static void RunAudioHaptic()
            =>RunSelected("AudioHaptic-Oct07",new[]{
                "Cascade.Tests.AudioFeedbackTests.MenuClickFinishesAcrossNavigationAndReducedEffectsWithoutDuplicateVoices",
                "Cascade.Tests.AudioFeedbackTests.SoundMuteStopsCuesWhileMusicContinuesAndNewClicksResume",
                "Cascade.Tests.AudioFeedbackTests.OutputResetAndForegroundReturnRecoverTheEnabledThemeNearItsOldPosition",
                "Cascade.Tests.AudioFeedbackTests.HapticsAreThrottledAndRespectPreferenceAndSuspension",
                "Cascade.Tests.PlayableTests.WorldScoresCrossfadeAndMutePreservesPlaybackPosition",
                "Cascade.Tests.PlayableTests.WindKeepsItsPositionAcrossRegionsAndFadesBeforeSleeping"
            });
        static void RunTests(string phase,string[] tests)
        {
            if (EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling || EditorApplication.isPlaying)
                throw new InvalidOperationException("PlayMode validation requires a compiled editor outside Play Mode.");
            if (string.IsNullOrEmpty(phase) || phase.Contains("..") || phase.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Expected a report folder name.");
            string root = Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
            string report = Path.Combine(root, "Reports", phase, "playmode.xml"); Directory.CreateDirectory(Path.GetDirectoryName(report));
            SessionState.SetString(PendingKey, report); Register(report);
            runner.Execute(new ExecutionSettings(new Filter { testMode = TestMode.PlayMode, assemblyNames = new[] { "Cascade.Tests.PlayMode" },testNames=tests }));
        }
        sealed class Results : ICallbacks
        {
            readonly string report;
            public Results(string report) { this.report = report; }
            public void RunStarted(ITestAdaptor tests) { }
            public void TestStarted(ITestAdaptor test) { File.AppendAllText(report+".progress",DateTime.UtcNow.ToString("O")+" "+test.FullName+"\n"); }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result, report); SessionState.EraseString(PendingKey);
                Debug.Log("CASCADE PlayMode: " + result.PassCount + " passed, " + result.FailCount + " failed. " + report);
            }
        }
    }
}
