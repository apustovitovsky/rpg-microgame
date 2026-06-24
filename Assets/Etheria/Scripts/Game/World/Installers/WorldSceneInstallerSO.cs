using Etheria.Core.DI;
using UnityEngine;
using VContainer;

namespace Etheria.Game.World
{
    [CreateAssetMenu(
        fileName = "WorldSceneInstaller",
        menuName = "Etheria/World/World Scene Installer")]
    public sealed class WorldSceneInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            var locations =
                SceneComponentLookup.FindAll<WorldLocation>(builder);

            var routes =
                SceneComponentLookup.FindAll<WorldRoute>(builder);

            builder.RegisterInstance<IWorldLocationRegistry>(
                new WorldLocationRegistry(locations));

            builder.RegisterInstance<IWorldRouteRegistry>(
                new WorldRouteRegistry(routes));
        }
    }
}