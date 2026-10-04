using System;
using System.IO;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Cascade.Editor.Validation
{
    public static class CascadeTestRunner
    {
        public static void RunEditMode(string phase)
        {
            if (string.IsNullOrEmpty(phase) || phase.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || phase.Contains(".."))
                throw new ArgumentException("Expected a report folder name.", nameof(phase));
            var packageRoot = Directory.GetParent(Application.dataPath).Parent.FullName;
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
            }
            finally { runner.UnregisterCallbacks(callback); UnityEngine.Object.DestroyImmediate(runner); }
        }

        sealed class Results : ICallbacks
        {
            readonly string report;
            internal bool Completed { get; private set; }
            internal Results(string report) { this.report = report; }
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                TestRunnerApi.SaveResultToFile(result, report);
                Completed = true;
                Debug.Log("CASCADE tests: " + result.PassCount + " passed, " + result.FailCount + " failed. " + report);
            }
        }
    }
}
