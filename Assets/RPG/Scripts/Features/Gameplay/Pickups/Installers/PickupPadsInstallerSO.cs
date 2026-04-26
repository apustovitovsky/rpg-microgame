using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(
        fileName = "PickupPadsInstaller",
        menuName = "RPG/Gameplay/Pickup/Pickup Pads Installer")]
    public sealed class PickupPadsInstallerSO : ScopeInstallerSO
    {
        public override void Install(LifetimeScope scope, IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PickupPad>();
        }
    }
}