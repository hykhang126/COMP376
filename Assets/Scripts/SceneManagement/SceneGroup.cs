using System;
using System.Linq;
using Eflatun.SceneReference;

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
        }
    }
}