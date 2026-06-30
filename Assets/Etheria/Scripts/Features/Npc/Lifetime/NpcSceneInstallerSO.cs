using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Npc
{
    [CreateAssetMenu(
        fileName = "NpcSceneInstaller",
        menuName = "Etheria/Npc/Scene Installer")]
    public class NpcSceneInstallerSO : InstallerSO
    {
        [SerializeField]
        private NpcCatalogSO _catalog;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_catalog);

            builder.Register<NpcTravelService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<NpcSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<WorldNpcPresenter>(Lifetime.Singleton);

            builder.Register<NpcStateRegistry>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<NpcRuntimeRegistry>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}

