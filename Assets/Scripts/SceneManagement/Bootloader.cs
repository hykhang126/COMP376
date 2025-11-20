#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;

//https://github.com/adammyhre/Unity-Inventory-System/blob/9548628f7786e0945e657d4d037ff61040eafd6f/Assets/_Project/Scripts/SceneManagement/Bootstrapper.cs
namespace SceneManagement
{
    public class Bootloader : MonoBehaviour
    {
        static Bootloader instance;
        public static Bootloader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<Bootloader>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = typeof(Bootloader).Name;
                        instance = go.AddComponent<Bootloader>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (this != instance)
                {
                    Destroy(gameObject);
                }
            }
        }

#if !NOBOOTSTRAP
        // NOTE: This script is intended to be placed in your first scene included in the build settings.
        static readonly int sceneIndex = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            Debug.Log("Bootstrapper...");
#if UNITY_EDITOR
            // Set the bootstrapper scene to be the play mode start scene when running in the editor
            // This will cause the bootstrapper scene to be loaded first (and only once) when entering
            // play mode from the Unity Editor, regardless of which scene is currently active.
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[sceneIndex].path);
#endif
        }
#endif
    }
}