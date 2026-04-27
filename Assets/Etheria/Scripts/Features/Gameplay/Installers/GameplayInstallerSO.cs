using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    [CreateAssetMenu(
        fileName = "GameplayInstaller",
        menuName = "Etheria/Gameplay/Installers/Gameplay Installer")]
    public class GameplayInstallerSO : InstallerSO
    {
        [SerializeField] private GameplayConfigSO _gameplayConfig;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterInstance(_gameplayConfig);

            builder.RegisterEntryPoint<GameplaySessionStarter>(Lifetime.Singleton);

            builder.Register<ActorFactory>(Lifetime.Singleton)
                .As<IActorFactory>();

            builder.Register<PossessionService>(Lifetime.Singleton)
                .As<IPossessionService>();

            builder.Register<GameplayInputRouter>(Lifetime.Singleton)
                .As<IGameplayInputRouter>();
        }
    }
}
