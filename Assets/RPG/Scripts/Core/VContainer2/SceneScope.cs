using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core.VContainer
{
    public sealed class SceneScope : LifetimeScope
    {
        [SerializeField] private ScopeInstallerSO[] _scopeInstallers;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_scopeInstallers == null)
                return;

            foreach (var installer in _scopeInstallers)
            {
                if (installer == null)
                    continue;

                installer.Install(this, builder);
            }
        }
    }

}