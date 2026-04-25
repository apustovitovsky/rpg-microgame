using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
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

        public async UniTask<bool> LoadExperienceAsync(
            ExperienceDefinitionSO request,
            CancellationToken ct)
        {
            if (request == null)
            {
                Debug.LogError("Experience definition asset is null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.RootScene.ScenePath))
            {
                Debug.LogError("Root scene path is null or empty.");
                return false;
            }

            try
            {
                var rootScene = await LoadSceneInternalAsync(request.RootScene, LoadSceneMode.Single, _parent, ct);
                if (!rootScene.isLoaded)
                {
                    return false;
                }

                var rootScope = LifetimeScope.Find<LifetimeScope>(rootScene);
                if (rootScope == null)
                {
                    Debug.LogError(
                        $"Root scene '{request.RootScene.ScenePath}' is loaded but does not contain a {nameof(LifetimeScope)}. " +
                        "Gameplay additive scenes must have an explicit root scope to avoid invalid parent resolution.");
                    return false;
                }

                var parentScope = rootScope;

                var additiveScenes = request.AdditiveScenes;
                if (additiveScenes == null)
                    return true;

                for (var i = 0; i < additiveScenes.Length; i++)
                {
                    var additiveRequest = additiveScenes[i];
                    var additiveScene = await LoadSceneInternalAsync(additiveRequest, LoadSceneMode.Additive, parentScope, ct);
                    if (!additiveScene.isLoaded)
                        return false;
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

        private static async UniTask<Scene> LoadSceneInternalAsync(
            ExperienceDefinitionSO.SceneDefinition request,
            LoadSceneMode loadSceneMode,
            LifetimeScope parentScope,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.ScenePath))
            {
                Debug.LogError($"Scene path is null or empty for {loadSceneMode} load.");
                return default;
            }

            using (LifetimeScope.EnqueueParent(parentScope))
            using (EnqueueInstallers(request.Installers))
            {
                var loadOperation = SceneManager.LoadSceneAsync(request.ScenePath, loadSceneMode);
                if (loadOperation == null)
                {
                    Debug.LogError($"Failed to start loading scene '{request.ScenePath}' with mode {loadSceneMode}.");
                    return default;
                }

                await loadOperation.ToUniTask(cancellationToken: ct);

                var loadedScene = FindLoadedScene(request.ScenePath);
                if (!loadedScene.isLoaded)
                {
                    Debug.LogError($"Scene '{request.ScenePath}' is not loaded after {loadSceneMode}.");
                    return default;
                }

                var scope = LifetimeScope.Find<LifetimeScope>(loadedScene);
                if (scope == null && HasInstallers(request.Installers))
                {
                    Debug.LogError(
                        $"Scene '{request.ScenePath}' defines installers in the experience, but no {nameof(LifetimeScope)} was found.");
                    return default;
                }

                if (scope != null && !scope.autoRun)
                {
                    scope.Build();
                }

                if (request.SetActive && !SceneManager.SetActiveScene(loadedScene))
                {
                    Debug.LogError($"Failed to set active scene '{request.ScenePath}'.");
                    return default;
                }

                return loadedScene;
            }
        }

        private static IDisposable EnqueueInstallers(InstallerSO[] installers)
        {
            if (!HasInstallers(installers))
                return NullDisposable.Instance;

            return LifetimeScope.Enqueue(builder =>
            {
                foreach (var installer in installers)
                {
                    if (installer == null)
                        continue;

                    installer.Install(builder);
                }
            });
        }

        private static bool HasInstallers(InstallerSO[] installers)
        {
            if (installers == null)
                return false;

            for (var i = 0; i < installers.Length; i++)
            {
                if (installers[i] != null)
                    return true;
            }

            return false;
        }

        private static Scene FindLoadedScene(string sceneIdentifier)
        {
            var normalizedIdentifier = NormalizeSceneIdentifier(sceneIdentifier);

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;

                if (string.Equals(scene.name, sceneIdentifier, StringComparison.OrdinalIgnoreCase))
                    return scene;

                if (string.Equals(NormalizeSceneIdentifier(scene.path), normalizedIdentifier, StringComparison.OrdinalIgnoreCase))
                    return scene;
            }

            return default;
        }

        private static string NormalizeSceneIdentifier(string sceneIdentifier)
        {
            if (string.IsNullOrWhiteSpace(sceneIdentifier))
                return string.Empty;

            var normalized = sceneIdentifier.Replace('\\', '/');

            if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("Assets/".Length);
            }

            if (normalized.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(0, normalized.Length - ".unity".Length);
            }

            return normalized;
        }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
