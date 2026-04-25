using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PoolHostInstaller", menuName = "RPG/Gameplay/Pooling/PoolHostInstaller")]
    public class PoolHostInstallerSO : InstallerSO
    {
        [SerializeField] private ScenePoolHost _poolHostPrefab;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_poolHostPrefab, Lifetime.Singleton);

            builder.Register<PickupPool>(Lifetime.Singleton);

            builder.Register<IPickupPoolRoots>(
                resolver => resolver.Resolve<ScenePoolHost>().Pickups,
                Lifetime.Scoped);

            builder.Register<IActorPoolRoots>(
                resolver => resolver.Resolve<ScenePoolHost>().Actors,
                Lifetime.Scoped);
        }
    }
}
