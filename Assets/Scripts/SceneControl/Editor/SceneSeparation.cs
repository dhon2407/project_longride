using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneControl.Editor
{
    public class SceneSeparation : ScriptableObject
    {
        [InitializeOnLoadMethod]
        public static void OnSceneSeparationLoaded()
        {
            EditorSceneManager.sceneOpened += (scene, mode) => {
                SceneControlData sceneControl = SceneControlData.GetContext();
                if (sceneControl == null)
                    return;
                
                EditorSceneManager_sceneOpened(scene, mode, sceneControl);
            };
            EditorSceneManager.sceneDirtied += EditorSceneManager_sceneDirtied;
        }

        private static void EditorSceneManager_sceneDirtied(Scene _scene)
        {
            if (File.Exists(_scene.path))
            {
                return;
            }

            string[] path = _scene.path.Replace("\\", "/").Split("/");
            if (path.Length <= 2 || !path[path.Length - 2].Equals("_scenes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (EditorUtility.DisplayDialog("Scene Deletion Detected", $"You deleted an additive scene, would you like to permanently remove this.", "Remove", "Restore"))
            {
                EditorSceneManager.CloseScene(_scene, true);
            }
            else
            {
                EditorSceneManager.SaveScene(_scene);
            }
        }

        private static void CleanupScene(Scene _scene)
        {
            foreach(GameObject gameObj in _scene.GetRootGameObjects())
            {
                if (gameObj.tag == "MainCamera")
                {
                    DestroyImmediate(gameObj);
                }
            }
        }

        private static void EditorSceneManager_sceneOpened(Scene _scene, OpenSceneMode _mode, SceneControlData _sceneControl)
        {
#if AddressableIsPresent
        if (!_sceneControl.Relationships.TryGetValue(_scene.path, out string[] subscenes))
		{
            return;
		}

        SceneController.HandleSubLoading(_scene.path);
#else
            string rootPath = _scene.path.Replace($"/{_scene.name}.unity", "");
            string scenesDirectory = rootPath + $"/_scenes";
            if (Directory.Exists(scenesDirectory))
            {
                // Warn the user about badly named scenes!
                foreach (string entry in Directory.GetFiles(scenesDirectory))
                {
                    string filePath = entry.Replace("\\", "/");
                    string fileName = filePath.Replace(scenesDirectory + "/", "");

                    // Load only Unity Scenes.
                    if (!filePath.EndsWith(".unity"))
                        continue;
                    // Don't load things that aren't following our naming conventions.
                    if (fileName.StartsWith(_scene.name + "_"))
                        continue;

                    if (EditorUtility.DisplayDialog("Incorrect Scene Naming Scheme", $"Would you like to update `{fileName}` to start with `{_scene.name + "_" + fileName}`", "Update", "Cancel"))
                    {
                        string output = scenesDirectory + "/" + _scene.name + "_" + fileName;
                        File.Move(filePath, output);
                        AssetDatabase.Refresh();
                    }
                }

                // Load the scenes.
                foreach (string entry in Directory.GetFiles(scenesDirectory))
                {
                    string filePath = entry.Replace("\\", "/");
                    string fileName = filePath.Replace(scenesDirectory + "/", "");

                    // Load only Unity Scenes.
                    if (!fileName.EndsWith(".unity"))
                        continue;
                    // Don't load things that aren't following our naming conventions.
                    if (!fileName.StartsWith(_scene.name + "_"))
                        continue;
                    // Don't load what has been loaded.
                    if (EditorSceneManager.GetSceneByPath(filePath).isLoaded)
                        continue;
                    // Load the sub-scene.
                    EditorSceneManager.OpenScene(filePath, OpenSceneMode.Additive);
                }
            }

            // Special handling the global scenes.

            string globalSceneDir = "Assets/Scenes/Global";
            string globalScene = $"{globalSceneDir}/Singleton/Singleton.unity";

            // Do not load the global directory more than once.
            if (_scene.path.StartsWith(globalSceneDir))
                return;

            // Do not load something that is already loaded.
            if (EditorSceneManager.GetSceneByPath(globalScene).isLoaded)
                return;

            // Do not load the global scene, if it doesn't exist.
            if (!File.Exists(globalScene))
                return;

            // Load the global scene.
            EditorSceneManager.OpenScene(globalScene, OpenSceneMode.Additive);
#endif
        }
    }
}
