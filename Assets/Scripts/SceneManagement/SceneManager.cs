using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    public class SceneGroupManager
    {
        SceneGroup activeSceneGroup;
        string persistentSceneName;

        public async Task LoadSceneGroup(SceneGroup sceneGroup, IProgress<float> progress = null)
        {
            if (activeSceneGroup != null)
            {
                await UnloadSceneGroup();
            }

            activeSceneGroup = sceneGroup;
            List<string> loadedScenes = new();
            int sceneCount = SceneManager.sceneCount;

            for (int i = 0; i < sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }

            int scenesToLoad = activeSceneGroup.Scenes.Length;
            AsyncOperationGroup asyncOperationGroup = new(scenesToLoad);

            for (int i = 0; i < scenesToLoad; i++)
            {
                SceneGroup.SceneInfo sceneInfo = activeSceneGroup.Scenes[i];
                string sceneName = sceneInfo.SceneReference.Name;
                if (loadedScenes.Contains(sceneName))
                {
                    continue;
                }

                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneInfo.SceneReference.Path, LoadSceneMode.Additive);
                asyncOperationGroup.asyncOperations.Add(asyncOperation);
            }

            while (!asyncOperationGroup.IsDone)
            {
                progress?.Report(asyncOperationGroup.Progress);
                await Task.Yield();
            }

            Scene activeScene = SceneManager.GetSceneByName(activeSceneGroup.FindActiveSceneName());
            if (activeScene.IsValid())
            {
                SceneManager.SetActiveScene(activeScene);
            }

            persistentSceneName = activeSceneGroup.FindPersistentSceneName();
        }

        public async Task UnloadSceneGroup()
        {
            List<string> loadedScenes = new();
            int sceneCount = SceneManager.sceneCount;

            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }
                if (scene.name == persistentSceneName || scene.name == "Bootloader")
                {
                    continue;
                }

                loadedScenes.Add(scene.name);
            }

            AsyncOperationGroup asyncOperationGroup = new(loadedScenes.Count);

            foreach (string sceneName in loadedScenes)
            {
                AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);
                if (asyncOperation == null)
                {
                    continue;
                }
                asyncOperationGroup.asyncOperations.Add(asyncOperation);
            }

            while (!asyncOperationGroup.IsDone)
            {
                await Task.Yield();
            }
        }
    }

    public class LoadingProgress : IProgress<float>
    {
        public event Action<float> ProgressChanged;
        public float Progress { get; private set; }

        public void Report(float value)
        {
            Progress = value / 1f;
            ProgressChanged?.Invoke(Progress);
        }
    }

    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> asyncOperations;

        public float Progress => asyncOperations.Count == 0 ? 0 : asyncOperations.Average(op => op.progress);
        public bool IsDone => asyncOperations.Count == 0 || asyncOperations.All(op => op.isDone);

        public AsyncOperationGroup(int capacity)
        {
            asyncOperations = new List<AsyncOperation>(capacity);
        }
    }
}