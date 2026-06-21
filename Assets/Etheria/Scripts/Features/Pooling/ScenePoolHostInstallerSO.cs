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
    public class ScenePoolHostInstallerSO : InstallerSO
    {
        [SerializeField] private ScenePoolHost _poolHostPrefab;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_poolHostPrefab, Lifetime.Singleton)
                .UnderScopeRoot();

            builder.Register<PickupPool>(Lifetime.Singleton);

            builder.Register<IPickupPoolRoots>(
                resolver => resolver.Resolve<ScenePoolHost>().Pickups,
                Lifetime.Singleton);
        }
    }
}

