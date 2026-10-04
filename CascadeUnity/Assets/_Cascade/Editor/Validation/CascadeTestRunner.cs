using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Cascade.Editor.Validation
{
    public static class CascadeTestRunner
    {
        public static void RunEditMode(string phase)
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Tests require a completed, successful Unity script compilation.");
            if (string.IsNullOrEmpty(phase) || phase.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || phase.Contains(".."))
                throw new ArgumentException("Expected a report folder name.", nameof(phase));
            var packageRoot = Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
            string report = Path.Combine(packageRoot, "Reports", phase, "editmode.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(report));
            var runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            var callback = new Results(report);
            runner.RegisterCallbacks(callback);
            try
            {
                runner.Execute(new ExecutionSettings(new Filter
                { testMode = TestMode.EditMode, assemblyNames = new[] { "Cascade.Tests.EditMode" } })
                { runSynchronously = true });
                if (!callback.Completed) throw new InvalidOperationException("Test runner returned without results.");
                if (callback.TestCount == 0 || callback.Failures != 0)
                    throw new InvalidOperationException("CASCADE tests did not pass. Inspect " + report);
            }
            finally { runner.UnregisterCallbacks(callback); UnityEngine.Object.DestroyImmediate(runner); }
        }

        sealed class Results : ICallbacks
        {
            readonly string report;
            internal bool Completed { get; private set; }
            internal int TestCount { get; private set; }
            internal int Failures { get; private set; }
            internal Results(string report) { this.report = report; }
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result, report);
                Completed = true;
                TestCount = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
                Failures = result.FailCount;
                Debug.Log("CASCADE tests: " + result.PassCount + " passed, " + result.FailCount + " failed. " + report);
            }
        }
    }
}
