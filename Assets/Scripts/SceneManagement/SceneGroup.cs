using System;
using System.Linq;
using Eflatun.SceneReference;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    [Serializable]
    public partial class SceneGroup
    {
        public string Name;
        public SceneInfo[] Scenes;

        public string FindActiveSceneName()
        {
            return Scenes.FirstOrDefault(scene => scene.IsActive)?.SceneReference.Name;
        }

        public Scene? FindActiveScene()
        {
            return Scenes.FirstOrDefault(scene => scene.IsActive)?.SceneReference.LoadedScene;
        }

        public Scene? FindSceneAtIndex(int index)
        {
            if (index < 0 || index >= Scenes.Length)
            {
                return null;
            }

            return Scenes[index].SceneReference.LoadedScene;
        }

        public string FindPersistentSceneName()
        {
            return Scenes.FirstOrDefault(scene => scene.IsPersistent)?.SceneReference.Name;
        }

        [Serializable]
        public class SceneInfo
        {
            public SceneReference SceneReference;
            public bool IsActive;
            public bool IsPersistent;

            public Scene LoadedScene
            {
                get
                {
                    return SceneManager.GetSceneByName(SceneReference.Name);
                }
            }
        }
    }
}