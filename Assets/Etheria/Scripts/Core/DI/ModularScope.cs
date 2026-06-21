using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public sealed class ModularScope : LifetimeScope
    {
        [SerializeField] private Transform _ContentRoot;
        [SerializeField] private InstallerSO[] _ScopeInstallers;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(
                new ScopeHierarchy(_ContentRoot != null ? _ContentRoot : transform));

            if (_ScopeInstallers == null)
                return;

            foreach (var installer in _ScopeInstallers)
            {
                if (installer == null)
                    continue;

                installer.Install(builder);
            }
        }
    }
}
