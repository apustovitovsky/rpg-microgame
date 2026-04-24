using System;

namespace RPG.Core
{
    public readonly struct SceneStackLoadRequest
    {
        public readonly struct AdditiveSceneRequest
        {
            public readonly string ScenePath;
            public readonly bool SetActive;

            public AdditiveSceneRequest(
                string scenePath,
                bool setActive = false)
            {
                ScenePath = scenePath;
                SetActive = setActive;
            }
        }

        public readonly string RootScenePath;
        public readonly AdditiveSceneRequest[] AdditiveScenes;

        public SceneStackLoadRequest(
            string rootScenePath,
            AdditiveSceneRequest[] additiveScenes = null)
        {
            RootScenePath = rootScenePath;
            AdditiveScenes = additiveScenes ?? Array.Empty<AdditiveSceneRequest>();
        }
    }
}