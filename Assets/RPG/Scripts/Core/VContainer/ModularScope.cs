using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    public sealed class ModularScope : LifetimeScope
    {
        [SerializeField] private InstallerSO[] _installers;
        [SerializeField] private ScopeInstallerSO[] _scopeInstallers;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_installers != null)
            {
                foreach (var installer in _installers)
                {
                    if (installer == null)
                        continue;

                    installer.Install(builder);
                }
            }

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
