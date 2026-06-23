using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public sealed class ModularScope : LifetimeScope
    {
        [FormerlySerializedAs("_ContentRoot")]
        [SerializeField] private Transform _contentRoot;

        [FormerlySerializedAs("_ScopeInstallers")]
        [SerializeField] private InstallerSO[] _scopeInstallers;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(
                new ScopeContentRoot(
                    _contentRoot != null
                        ? _contentRoot
                        : transform));

            if (_scopeInstallers == null)
                return;

            foreach (var installer in _scopeInstallers)
            {
                if (installer == null)
                    continue;

                installer.Install(builder);
            }
        }
    }
}
