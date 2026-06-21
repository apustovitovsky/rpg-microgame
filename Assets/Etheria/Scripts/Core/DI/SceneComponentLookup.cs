using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public static class SceneComponentLookup
    {
        public static IReadOnlyList<T> FindAll<T>(IContainerBuilder builder)
            where T : Component
        {
            if (builder.ApplicationOrigin is not LifetimeScope scope)
            {
                throw new InvalidOperationException(
                    "Scene component lookup requires a LifetimeScope application origin.");
            }

            var components = new List<T>();
            var roots = scope.gameObject.scene.GetRootGameObjects();

            foreach (var root in roots)
                components.AddRange(root.GetComponentsInChildren<T>(true));

            return components;
        }
    }
}
