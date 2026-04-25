using UnityEngine;
using VContainer;

namespace RPG.Core
{
    [CreateAssetMenu(
        fileName = "ModularInstaller",
        menuName = "RPG/Core/Modular Installer")]
    public sealed class ModularInstallerSO : InstallerSO
    {
        [SerializeField] private InstallerSO[] _installers;

        public override void Install(IContainerBuilder builder)
        {
            if (_installers == null)
                return;

            foreach (var installer in _installers)
            {
                if (installer == null)
                    continue;

                installer.Install(builder);
            }
        }
    }
}
