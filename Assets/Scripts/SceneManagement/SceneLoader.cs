using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] Slider loadingSlider;
        [SerializeField] Camera loadingCamera;
        [SerializeField] Canvas loadingCanvas;

        [SerializeField] SceneGroup[] sceneGroups;
        [SerializeField] int firstSceneGroupIndex;

        readonly SceneGroupManager sceneGroupManager = new();
        float currentProgress;
        bool isLoading;

        public void StartLoadingSceneGroup(int index)
        {
            if (isLoading)
            {
                return;
            }

            _ = LoadSceneGroup(index);
        }

        public int FindSceneGroupIndexByName(string name)
        {
            for (int i = 0; i < sceneGroups.Length; i++)
            {
                if (sceneGroups[i].Name == name)
                {
                    return i;
                }
            }

            return -1;
        }

        async void Start()
        {
            await LoadSceneGroup(firstSceneGroupIndex);
        }

        void Update()
        {
            if (!isLoading)
            {
                return;
            }

            loadingSlider.value = Mathf.Lerp(loadingSlider.value, currentProgress, Time.deltaTime * 10);
        }

        private async Task LoadSceneGroup(int index)
        {
            loadingSlider.value = 0;
            isLoading = true;
            if (index < 0 || index >= sceneGroups.Length)
            {
                Debug.LogError($"Invalid scene group index {index}.");
                return;
            }

            LoadingProgress loadingProgress = new();
            loadingProgress.ProgressChanged += progress => currentProgress = progress;
            EnableLoadingPanel(true);
            await sceneGroupManager.LoadSceneGroup(sceneGroups[index], loadingProgress);
            isLoading = false;
            EnableLoadingPanel(false);
        }

        void EnableLoadingPanel(bool enable)
        {
            loadingCanvas.gameObject.SetActive(enable);
            loadingCamera.gameObject.SetActive(enable);
        }
    }
}