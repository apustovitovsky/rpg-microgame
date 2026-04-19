using UnityEngine;

namespace RPG.Core
{
    [CreateAssetMenu(fileName = "ModularInstaller", menuName = "RPG/Core/Modular Installer")]
    public sealed class ModularInstallerSO : InstallerSO
    {
        [SerializeField] private InstallerSO[] _installers;

        public override void Install(in InstallContext context)
        {
            if (_installers == null) return;

            foreach (var installer in _installers)
            {
                if (installer == null || installer == this) continue;
                installer.Install(context);
            }
        }
    }
}