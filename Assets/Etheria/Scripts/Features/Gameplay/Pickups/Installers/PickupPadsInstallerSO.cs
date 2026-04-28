using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(
        fileName = "PickupPadsInstaller",
        menuName = "Etheria/Gameplay/Pickup/Pickup Pads Installer")]
    public sealed class PickupPadsInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterComponentInHierarchy<PickupPad>();
        }
    }
}

