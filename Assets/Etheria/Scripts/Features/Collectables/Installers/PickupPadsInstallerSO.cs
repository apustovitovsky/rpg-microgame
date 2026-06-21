using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Collectables
{
    [CreateAssetMenu(
        fileName = "PickupPadsInstaller",
        menuName = "Etheria/Gameplay/Pickup/Pickup Pads Installer")]
    public sealed class PickupPadsInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PickupPad>();
        }
    }
}

