using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneStackLoader
    {
        private readonly LifetimeScope _parent;

        public SceneStackLoader(LifetimeScope parent)
        {
            _parent = parent;
        }

        public async UniTask<bool> LoadStackAsync(
            SceneStackSO request,
            CancellationToken ct)
        {
            if (request == null)
            {
                Debug.LogError("Scene stack request asset is null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.RootScenePath))
            {
                Debug.LogError("Root scene name is null or empty.");
                return false;
            }

            try
            {
                using (LifetimeScope.EnqueueParent(_parent))
                {
                    if (!await LoadSceneInternalAsync(request.RootScenePath, LoadSceneMode.Single, ct))
                        return false;
                }

                var rootScene = SceneManager.GetSceneByName(request.RootScenePath);
                if (!rootScene.isLoaded)
                {
                    Debug.LogError($"Root scene '{request.RootScenePath}' is not loaded.");
                    return false;
                }

                var rootScope = LifetimeScope.Find<LifetimeScope>(rootScene);
                if (rootScope == null)
                {
                    Debug.LogError($"LifetimeScope not found in root scene '{request.RootScenePath}'.");
                    return false;
                }

                using (LifetimeScope.EnqueueParent(rootScope))
                {
                    var additiveScenes = request.AdditiveScenes;
                    if (additiveScenes == null)
                        return true;

                    for (var i = 0; i < additiveScenes.Length; i++)
                    {
                        var additiveRequest = additiveScenes[i];

                        if (string.IsNullOrWhiteSpace(additiveRequest.ScenePath))
                        {
                            Debug.LogError($"Additive scene name at index {i} is null or empty.");
                            return false;
                        }

                        if (!await LoadSceneInternalAsync(additiveRequest.ScenePath, LoadSceneMode.Additive, ct))
                            return false;

                        if (additiveRequest.SetActive)
                        {
                            var activeScene = SceneManager.GetSceneByName(additiveRequest.ScenePath);
                            if (!activeScene.isLoaded)
                            {
                                Debug.LogError($"Active additive scene '{additiveRequest.ScenePath}' is not loaded.");
                                return false;
                            }

                            if (!SceneManager.SetActiveScene(activeScene))
                            {
                                Debug.LogError($"Failed to set active scene '{additiveRequest.ScenePath}'.");
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        private static async UniTask<bool> LoadSceneInternalAsync(
            string sceneName,
            LoadSceneMode loadSceneMode,
            CancellationToken ct)
        {
            var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (loadOperation == null)
            {
                Debug.LogError($"Failed to start loading scene '{sceneName}' with mode {loadSceneMode}.");
                return false;
            }

            await loadOperation.ToUniTask(cancellationToken: ct);

            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
            {
                Debug.LogError($"Scene '{sceneName}' is not loaded after {loadSceneMode}.");
                return false;
            }

            var scope = LifetimeScope.Find<LifetimeScope>(scene);
            if (scope != null && !scope.autoRun)
            {
                scope.Build();
            }

            return true;
        }
    }
}
