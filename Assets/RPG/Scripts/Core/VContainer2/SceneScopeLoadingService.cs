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
        private const string SceneEntryName = "[SceneEntry]";
        private const string SceneScopeName = "[SceneScope]";

        private readonly LifetimeScope _parentScope;

        public SceneScopeLoadingService(
            LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public async UniTask<IReadOnlyList<LifetimeScope>> LoadSceneStackWithScopesAsync(
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

            LifetimeScope currentParent = _parentScope;

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

                var scene = await LoadSceneAsync(
                    definition.ScenePath,
                    loadMode,
                    ct);

                if (i == activeSceneIndex)
                    SceneManager.SetActiveScene(scene);

                var sceneRoot = CreateSceneRoot(scene);

                var sceneScope = CreateSceneScope(
                    currentParent,
                    sceneRoot,
                    definition);

                scopes.Add(sceneScope);
                currentParent = sceneScope;
            }

            return scopes;
        }

        private LifetimeScope CreateSceneScope(
            LifetimeScope parentScope,
            GameObject sceneRoot,
            SceneDefinitionSO definition)
        {
            var context = new SceneScopeContext(
                definition,
                sceneRoot);

            using (LifetimeScope.EnqueueParent(parentScope))
            using (LifetimeScope.Enqueue(builder =>
            {
                builder.RegisterInstance(context);
                InstallExtraInstallers(
                    builder,
                    definition.ExtraInstallers,
                    context);
            }))
            {
                var scopeObject = new GameObject(SceneScopeName);
                scopeObject.transform.SetParent(sceneRoot.transform, false);
                scopeObject.transform.SetSiblingIndex(0);

                return scopeObject.AddComponent<LifetimeScope>();
            }
        }

        private static void InstallExtraInstallers(
            IContainerBuilder builder,
            InstallerSO[] installers,
            SceneScopeContext context)
        {
            if (installers == null)
                return;

            foreach (var installer in installers)
            {
                if (installer == null)
                    continue;

                installer.Install(builder, context);
            }
        }

        // private static async UniTask<Scene> LoadSceneAsync(
        //     string scenePath,
        //     LoadSceneMode loadMode,
        //     CancellationToken ct)
        // {
        //     var operation = SceneManager.LoadSceneAsync(scenePath, loadMode)
        //         ?? throw new InvalidOperationException($"Failed to start loading scene '{scenePath}'.");

        //     await operation.ToUniTask(cancellationToken: ct);

        //     var scene = SceneManager.GetSceneByPath(scenePath);

        //     if (!scene.IsValid() || !scene.isLoaded)
        //         throw new InvalidOperationException($"Scene '{scenePath}' was not loaded.");

        //     return scene;
        // }

        private static async UniTask<Scene> LoadSceneAsync(
            string scenePath,
            LoadSceneMode loadMode,
            CancellationToken ct)
        {
            var beforeHandles = new HashSet<int>();

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                beforeHandles.Add(loadedScene.handle);
            }

            var operation = SceneManager.LoadSceneAsync(scenePath, loadMode)
                ?? throw new InvalidOperationException($"Failed to start loading scene '{scenePath}'.");

            await operation.ToUniTask(cancellationToken: ct);

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                if (!beforeHandles.Contains(loadedScene.handle) &&
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

        private static GameObject CreateSceneRoot(Scene scene)
        {
            var sceneRoot = new GameObject(SceneEntryName);

            SceneManager.MoveGameObjectToScene(sceneRoot, scene);
            sceneRoot.transform.SetSiblingIndex(0);

            return sceneRoot;
        }
    }
}
