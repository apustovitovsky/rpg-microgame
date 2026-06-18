using Etheria.Core.DI;
using Etheria.Game.Pooling;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Pooling
{
    [CreateAssetMenu(
        fileName = "PoolHostInstaller",
        menuName = "Etheria/Gameplay/Pooling/Scene Pool Host Installer")]
    public class ScenePoolHostInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private ScenePoolHost _poolHostPrefab;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            
            builder.RegisterComponentInNewPrefab(_poolHostPrefab, Lifetime.Singleton)
                .UnderTransform(rootObject.transform);

            builder.Register<PickupPool>(Lifetime.Singleton);

            builder.Register<IPickupPoolRoots>(
                resolver => resolver.Resolve<ScenePoolHost>().Pickups,
                Lifetime.Singleton);

            builder.Register<IActorPoolRoots>(
                resolver => resolver.Resolve<ScenePoolHost>().Actors,
                Lifetime.Singleton);
        }
    }
}

