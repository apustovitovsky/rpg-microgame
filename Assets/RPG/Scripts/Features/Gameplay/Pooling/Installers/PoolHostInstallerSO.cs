using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PoolHostInstaller", menuName = "RPG/Gameplay/Pooling/PoolHostInstaller")]
    public class PoolHostInstallerSO : InstallerSO
    {
        // [SerializeField] private ScenePoolHost _poolHost;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterComponentInHierarchy<ScenePoolHost>();

            // builder.RegisterInstance<IPickupPoolRoots>(_poolHost.Pickups);
            // builder.RegisterInstance<IActorPoolRoots>(_poolHost.Actors);

            builder.Register<IPickupPoolRoots>(
                resolver => resolver.Resolve<ScenePoolHost>().Pickups,
                Lifetime.Scoped);

            builder.Register<IActorPoolRoots>(
                resolver => resolver.Resolve<ScenePoolHost>().Actors,
                Lifetime.Scoped);

            
        }
    }
}