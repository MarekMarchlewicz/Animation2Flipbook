using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Animation2Flipbook.Editor
{
    internal static class SceneHelper
    {
        const string k_SceneName = "Animation2Flipbook.RecorderScene";
        const string k_ScenePath = "Assets/" + k_SceneName + ".unity";

        static bool OpenCustomScene(RecorderSettings settings) => settings.sceneUseCustom && !string.IsNullOrEmpty(settings.scene) && AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.scene) != null;

        public static void LoadRecorderScene(RecorderSettings settings)
        {
            if (OpenCustomScene(settings))
            {
                EditorSceneManager.OpenScene(settings.scene);
                if (!settings.sceneUseSceneCamera)
                {
                    foreach (var sceneCamera in Object.FindObjectsOfType<Camera>())
                        sceneCamera.enabled = false;
                }
            }
            else
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                EditorSceneManager.MarkSceneDirty(scene);
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(scene, k_ScenePath, false);

                EditorSceneManager.OpenScene(k_ScenePath);
            }
        }

        public static void ReopenOriginalScene(string originalSceneName)
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            if (!string.IsNullOrEmpty(originalSceneName))
                EditorSceneManager.OpenScene(originalSceneName);

            var guid = AssetDatabase.AssetPathToGUID(k_ScenePath);
            if (!string.IsNullOrEmpty(guid))
                AssetDatabase.DeleteAsset(k_ScenePath);
        }
    }
}