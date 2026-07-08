using Etheria.Core.DI;
using Etheria.Game.World;
using Etheria.Navigation;
using Game.Core;
using UnityEngine;
using VContainer;

namespace Game.Navigation
{
    [CreateAssetMenu(
        fileName = "NavigationModuleBuilder",
        menuName = "Game/Navigation/Navigation Module Builder")]
    public sealed class NavigationModuleBuilder : ModuleBuilder
    {
        public override void Install(IContainerBuilder builder)
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