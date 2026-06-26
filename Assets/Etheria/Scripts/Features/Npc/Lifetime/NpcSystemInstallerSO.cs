using Etheria.Core.DI;
using Etheria.Game.Npc;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Npc
{
    [CreateAssetMenu(
        fileName = "NpcSystemInstaller",
        menuName = "Etheria/Npc/Npc System Installer")]
    public class NpcSystemInstallerSO : InstallerSO
    {
        [SerializeField]
        private NpcCatalogSO _catalog;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_catalog);

            builder.Register<NpcAgentRegistry>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<NpcTravelService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<NpcSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<WorldNpcPresenter>(Lifetime.Singleton);
        }
    }
}

