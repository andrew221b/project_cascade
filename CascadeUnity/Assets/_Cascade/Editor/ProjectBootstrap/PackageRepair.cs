using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Cascade.Editor.Bootstrap
{
    // P00 recovery for the incompatible Input System in the installed URP template.
    // Runs only when explicitly invoked; it is never part of gameplay.
    public static class PackageRepair
    {
        private static AddRequest request;
        private static double deadline;

        public static void PinInputSystem()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string embedded = Path.Combine(projectRoot, "Packages/com.unity.inputsystem");
            string archive = Path.GetFullPath(Path.Combine(projectRoot, "../Reports/P00/temporary_inputsystem_1.18.0~"));
            if (Directory.Exists(embedded))
            {
                if (Directory.Exists(archive))
                {
                    throw new InvalidOperationException("Temporary package archive already exists; refusing to overwrite it.");
                }

                // The embedded official copy unblocked compilation. UPM requires it
                // outside Packages before it can write the registry pin.
                Directory.Move(embedded, archive);
                Debug.Log("[Cascade P00] Temporary embedded Input System archived outside the project.");
            }

            Debug.Log("[Cascade P00] Pinning com.unity.inputsystem@1.18.0 through Package Manager.");
            request = Client.Add("com.unity.inputsystem@1.18.0");
            deadline = EditorApplication.timeSinceStartup + 180;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!request.IsCompleted)
            {
                if (EditorApplication.timeSinceStartup > deadline)
                {
                    EditorApplication.update -= Poll;
                    Debug.LogError("[Cascade P00] Package Manager timed out.");
                    EditorApplication.Exit(2);
                }

                return;
            }

            EditorApplication.update -= Poll;
            if (request.Status != StatusCode.Success)
            {
                Debug.LogError("[Cascade P00] Package Manager failed: " + request.Error?.message);
                EditorApplication.Exit(1);
                return;
            }

            string manifest = File.ReadAllText(Path.Combine(Application.dataPath, "../Packages/manifest.json"));
            if (!manifest.Contains("\"com.unity.inputsystem\": \"1.18.0\""))
            {
                Debug.LogError("[Cascade P00] Registry pin was not written to the manifest.");
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Cascade P00] Input System registry pin verified. Archive temporary embedded override before the final import.");
            EditorApplication.Exit(0);
        }
    }
}
