using System.Collections.Generic;
using Etheria.Core.DI;
using Etheria.Game.World;
using UnityEngine;
using VContainer;

namespace Etheria.Navigation
{
    [CreateAssetMenu(
        fileName = "NavigationSceneInstaller",
        menuName = "Etheria/Navigation/Navigation Scene Installer")]
    public sealed class NavigationSceneInstallerSO : InstallerSO
    {
        public override void Install(
            IContainerBuilder builder)
        {
            var locations =
                SceneComponentLookup.GetComponentsInScene<NavigationLocation>(builder);

            var waypoints =
                SceneComponentLookup.GetComponentsInScene<NavigationWaypoint>(builder);

            var graph =
                NavigationGraphBuilder.Build(waypoints);


            builder.Register<NavigationGraphProvider>(Lifetime.Singleton)
                .WithParameter(graph)
                .AsImplementedInterfaces();

            builder.Register<NavigationPathfinder>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<NavigationLocationResolver>(Lifetime.Singleton)
                .WithParameter(locations)
                .AsImplementedInterfaces();
        }
    }
}