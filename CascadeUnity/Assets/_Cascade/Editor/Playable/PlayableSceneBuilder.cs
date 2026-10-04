using System;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Cascade.Editor.Playable
{
    public static class PlayableSceneBuilder
    {
        public const string ScenePath = "Assets/_Cascade/Scenes/CascadePlayable.unity";
        [MenuItem("CASCADE/Open Playable")]
        public static void OpenPlayable()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before opening the playable scene.");
            if (Enumerable.Range(0, EditorSceneManager.sceneCount).Any(i => EditorSceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("Save the current scene before opening CASCADE.");
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            Directory.CreateDirectory("Assets/_Cascade/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create CASCADE camera"); cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 5;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.16f, .18f, .20f); camera.transform.position = new Vector3(0, 0, -10);
            var inputObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(inputObject, "Create CASCADE input"); inputObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var root = new GameObject("CASCADE", typeof(CascadeGame)); Undo.RegisterCreatedObjectUndo(root, "Create CASCADE playable");
            var serialized = new SerializedObject(root.GetComponent<CascadeGame>());
            serialized.FindProperty("uiCamera").objectReferenceValue = camera;
            serialized.FindProperty("font").objectReferenceValue = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            serialized.ApplyModifiedProperties();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(EditorBuildSettings.scenes.Where(s => s.path != ScenePath)).ToArray();
            AssetDatabase.Refresh();
        }
    }
}
