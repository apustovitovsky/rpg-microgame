using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    [DisallowMultipleComponent]
    public sealed class ModuleScope :
        LifetimeScope
    {
        [SerializeField]
        private Transform _prefabRoot;

        protected override void Configure(
            IContainerBuilder builder)
        {
            foreach (var installer in FindInstallers())
            {
                installer.Install(builder);
            }
        }

        private IEnumerable<IModuleInstaller> FindInstallers()
        {
            if (_prefabRoot == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ModuleScope)} requires a composition root.");
            }

            var components = _prefabRoot
                .GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var component in components)
            {
                if (component is not IModuleInstaller installer ||
                    !BelongsToThisScope(component))
                {
                    continue;
                }

                yield return installer;
            }
        }

        private bool BelongsToThisScope(
            MonoBehaviour installer)
        {
            var current = installer.transform;

            while (current != null)
            {
                var scope = current.GetComponent<ModuleScope>();

                if (scope != null &&
                    scope != this)
                {
                    return false;
                }

                if (current == _prefabRoot)
                    return true;

                current = current.parent;
            }

            return false;
        }
    }
}