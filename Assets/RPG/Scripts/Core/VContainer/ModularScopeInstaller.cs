using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    public sealed class ModularScopeInstaller : ScopeInstallerSO
    {
        [SerializeField] private InstallerSO[] _installers;
        [SerializeField] private ScopeInstallerSO[] _scopeInstallers;

        public override void Install(LifetimeScope scope, IContainerBuilder builder)
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

                installer.Install(scope, builder);
            }
        }
    }
}
