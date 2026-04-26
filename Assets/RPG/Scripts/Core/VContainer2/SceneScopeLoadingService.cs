using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace RPG.Core.VContainer
{
    public sealed class SceneScopeLoadingService
    {
        private const string SceneRootName = "[SceneRoot]";

        private readonly LifetimeScope _parentScope;

        public SceneScopeLoadingService(
            LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public async UniTask<IReadOnlyList<LifetimeScope>> LoadSceneStackWithScopesAsync(
            SceneDefinitionSO[] definitions,
            CancellationToken ct)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            if (definitions.Length == 0)
                throw new ArgumentException("Scene stack is empty.", nameof(definitions));

            var scopes = new List<LifetimeScope>(definitions.Length);

            LifetimeScope currentParent = _parentScope;

            for (var i = 0; i < definitions.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                var definition = definitions[i];

                if (definition == null)
                    throw new ArgumentException($"Scene definition at index {i} is null.", nameof(definitions));

                if (string.IsNullOrWhiteSpace(definition.ScenePath))
                    throw new ArgumentException($"Scene name at index {i} is empty.", nameof(definitions));

                if (definition.LifetimeScope == null)
                    throw new InvalidOperationException(
                        $"Scene definition '{definition.name}' does not reference a {nameof(LifetimeScope)} prefab.");

                var loadMode = i == 0
                    ? LoadSceneMode.Single
                    : LoadSceneMode.Additive;

                var scene = await LoadSceneAsync(definition.ScenePath, loadMode, ct);

                var sceneRoot = GetOrCreateSceneRoot(scene);

                var sceneScope = CreateSceneScope(
                    currentParent,
                    sceneRoot.transform,
                    definition.LifetimeScope,
                    definition.ExtraInstallers);

                scopes.Add(sceneScope);
                currentParent = sceneScope;
            }

            return scopes;
        }

        private LifetimeScope CreateSceneScope(
            LifetimeScope parentScope,
            Transform sceneRoot,
            LifetimeScope sceneScopePrefab,
            InstallerSO[] installers)
        {
            using (LifetimeScope.EnqueueParent(parentScope))
            using (LifetimeScope.Enqueue(builder =>
            {
                InstallExtraInstallers(builder, installers);
            }))
            {
                var scope = UnityEngine.Object.Instantiate(sceneScopePrefab, sceneRoot);
                scope.name = sceneScopePrefab.name;
                return scope;
            }
        }

        private static void InstallExtraInstallers(
            IContainerBuilder builder,
            InstallerSO[] installers)
        {
            if (installers == null)
                return;

            for (var i = 0; i < installers.Length; i++)
            {
                var installer = installers[i];

                if (installer == null)
                    continue;

                installer.Install(builder);
            }
        }

        private static async UniTask<Scene> LoadSceneAsync(
            string sceneName,
            LoadSceneMode loadMode,
            CancellationToken ct)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, loadMode)
                ?? throw new InvalidOperationException($"Failed to start loading scene '{sceneName}'.");

            await operation.ToUniTask(cancellationToken: ct);

            var scene = FindLoadedScene(sceneName);

            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException($"Scene '{sceneName}' was not loaded.");

            return scene;
        }

        private static GameObject GetOrCreateSceneRoot(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == SceneRootName)
                    return root;
            }

            var sceneRoot = new GameObject(SceneRootName);
            SceneManager.MoveGameObjectToScene(sceneRoot, scene);
            sceneRoot.transform.SetSiblingIndex(0);

            return sceneRoot;
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
                normalized = normalized["Assets/".Length..];

            if (normalized.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                normalized = normalized[..^".unity".Length];

            return normalized;
        }
    }
}
