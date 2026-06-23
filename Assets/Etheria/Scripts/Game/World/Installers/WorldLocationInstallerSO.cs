using Etheria.Core.DI;
using UnityEngine;
using VContainer;

namespace Etheria.Game.World
{
    [CreateAssetMenu(
        fileName = "WorldLocationInstaller",
        menuName = "Etheria/World/Location Installer")]
    public sealed class WorldLocationInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            var locations =
                SceneComponentLookup.FindAll<WorldLocation>(builder);

            var registry = new WorldLocationRegistry(locations);

            builder.RegisterInstance<IWorldLocationRegistry>(
                registry);
        }
    }
}
