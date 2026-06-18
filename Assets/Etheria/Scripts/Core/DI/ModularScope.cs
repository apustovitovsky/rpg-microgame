using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public sealed class ModularScope : LifetimeScope
    {
        [SerializeField] private ScopeInstallerSO[] _ScopeInstallers;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_ScopeInstallers != null)
            {
                foreach (var installer in _ScopeInstallers)
                {
                    if (installer == null)
                        continue;

                    installer.Install(builder, gameObject);
                }
            }
        }
    }
}
