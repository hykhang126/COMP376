using UnityEngine;

namespace SceneManagement
{
    public class SceneChange : MonoBehaviour
    {
        SceneLoader sceneLoader;

        void Awake()
        {
            sceneLoader = FindAnyObjectByType<SceneLoader>();
        }

        public void LoadSceneGroup(int sceneGroup)
        {
            sceneLoader.StartLoadingSceneGroup(sceneGroup);
        }

        public void LoadSceneGroupName(string sceneGroupName)
        {
            int sceneGroup = sceneLoader.FindSceneGroupByName(sceneGroupName);
            if (sceneGroup != -1)
            {
                LoadSceneGroup(sceneGroup);
            }
            else
            {
                Debug.LogError($"Scene group {sceneGroupName} not found.");
            }
        }
    }
}