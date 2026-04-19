using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Core
{
    public class ModularScope : LifetimeScope
    {
        [SerializeField] private InstallerSO[] _installers;

        protected override void Configure(IContainerBuilder builder)
        {
            InstallContext context = new(this, builder);

            foreach (var installer in _installers)
            {
                if (installer == null || installer == this) continue;
                installer.Install(context);
            }
        }
    }
}
