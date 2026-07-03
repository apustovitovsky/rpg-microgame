using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public sealed class SceneStackLoadingService : ISceneStackLoadingService
    {
        private readonly LifetimeScope _parentScope;

        public SceneStackLoadingService(
            LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public async UniTask<IReadOnlyList<LifetimeScope>> LoadSceneStackAsync(
            SceneCatalogEntry entry,
            CancellationToken ct)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            var definitions = entry.SceneStack;

            if (definitions == null || definitions.Length == 0)
            {
                throw new ArgumentException(
                    $"Scene stack '{entry.DisplayName}' is empty.",
                    nameof(entry));
            }

            var activeSceneIndex = entry.ActiveSceneIndex;

            if (activeSceneIndex < 0 || activeSceneIndex >= definitions.Length)
            {
                throw new InvalidOperationException(
                    $"Scene stack '{entry.DisplayName}' has invalid active scene index '{activeSceneIndex}'.");
            }

            var scopes = new List<LifetimeScope>(definitions.Length);

            for (var i = 0; i < definitions.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                var definition = definitions[i];

                if (definition == null)
                {
                    throw new ArgumentException(
                        $"Scene stack '{entry.DisplayName}' contains null scene definition at index {i}.",
                        nameof(entry));
                }

                if (!definition.IsValid)
                {
                    throw new ArgumentException(
                        $"Scene stack '{entry.DisplayName}' contains invalid scene definition at index {i}.",
                        nameof(entry));
                }

                var loadMode = i == 0
                    ? LoadSceneMode.Single
                    : LoadSceneMode.Additive;

                Scene scene;

                using (LifetimeScope.EnqueueParent(_parentScope))
                using (LifetimeScope.Enqueue(builder =>
                {
                    builder.RegisterEntryPointExceptionHandler(ex =>
                    {
                        Debug.LogError(
                            $"[VContainer EntryPoint Error] SceneDefinition='{definition.name}'\n{ex.Message}");

                        Debug.LogException(ex);
                    });
                }))
                {
                    scene = await LoadSceneAsync(
                        definition.ScenePath,
                        loadMode,
                        ct);
                }

                if (i == activeSceneIndex)
                    SceneManager.SetActiveScene(scene);

                var sceneScope = FindSceneScope(scene, definition);

                scopes.Add(sceneScope);
            }

            return scopes;
        }

        private LifetimeScope FindSceneScope(
            Scene scene,
            SceneDefinitionSO definition)
        {
            var scopes = new List<LifetimeScope>();

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent<LifetimeScope>(out var rootScope))
                    scopes.Add(rootScope);
            }

            if (scopes.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Loaded scene must contain exactly one {nameof(LifetimeScope)}.\n" +
                    $"SceneDefinition: {definition.name}\n" +
                    $"ScenePath: {definition.ScenePath}\n" +
                    $"Found: {scopes.Count}");
            }

            var scope = scopes[0];

            if (scope.Container == null)
            {
                throw new InvalidOperationException(
                    $"Scene scope was found but its container was not built.\n" +
                    $"SceneDefinition: {definition.name}\n" +
                    $"ScenePath: {definition.ScenePath}\n" +
                    $"ScopeObject: {scope.name}");
            }

            if (scope.Parent != _parentScope)
            {
                throw new InvalidOperationException(
                    $"Scene scope has an unexpected parent.\n" +
                    $"SceneDefinition: {definition.name}\n" +
                    $"ScenePath: {definition.ScenePath}\n" +
                    $"Expected: {_parentScope.name}\n" +
                    $"Actual: {(scope.Parent != null ? scope.Parent.name : "<null>")}");
            }

            return scope;
        }

        private static async UniTask<Scene> LoadSceneAsync(
            string scenePath,
            LoadSceneMode loadMode,
            CancellationToken ct)
        {
            var beforeHandles = new HashSet<ulong>();

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                beforeHandles.Add(loadedScene.handle.GetRawData());
            }

            var operation = SceneManager.LoadSceneAsync(scenePath, loadMode)
                ?? throw new InvalidOperationException($"Failed to start loading scene '{scenePath}'.");

            await operation.ToUniTask(cancellationToken: ct);

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                if (!beforeHandles.Contains(loadedScene.handle.GetRawData()) &&
                    loadedScene.IsValid() &&
                    loadedScene.isLoaded &&
                    loadedScene.path == scenePath)
                {
                    return loadedScene;
                }
            }

            throw new InvalidOperationException(
                $"Scene '{scenePath}' was loaded, but new scene instance was not found.");
        }
    }
}
