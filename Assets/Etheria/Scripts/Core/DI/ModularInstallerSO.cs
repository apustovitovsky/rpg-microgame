using System;
using UnityEngine;
using VContainer;


namespace Etheria.Core.DI
{
    [CreateAssetMenu(
    menuName = "Etheria/Core/Scene Loading/Modular Installer",
    fileName = "ModularInstaller")]
    public sealed class ModularInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private ScopeInstallerSO[] _ScopeInstallers;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            if (_ScopeInstallers != null)
            {
                foreach (var installer in _ScopeInstallers)
                {
                    if (installer == null || installer == this)
                    {
                        throw new InvalidOperationException(
                            "Modular installer cannot be null or installed recursively.");
                    }

                    installer.Install(builder, rootObject);
                }
            }
        }
    }
}
