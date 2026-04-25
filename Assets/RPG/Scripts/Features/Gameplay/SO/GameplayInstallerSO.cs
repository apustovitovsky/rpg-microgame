using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayInstaller", menuName = "RPG/Gameplay/Installers/Gameplay Installer")]
    public class GameplayInstallerSO : InstallerSO
    {
        [SerializeField] private GameplayConfigSO _gameplayConfig;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameplayConfig);


            Debug.Log("GameplayInstallerSO.Install");
            builder.Register<GameplaySessionStarter>(Lifetime.Singleton);

            builder.Register<ActorFactory>(Lifetime.Singleton)
                .As<IActorFactory>();

            builder.Register<PossessionService>(Lifetime.Singleton)
                .As<IPossessionService>();

            builder.Register<GameplayInputRouter>(Lifetime.Singleton)
                .As<IGameplayInputRouter>();

            builder.RegisterComponentInNewPrefab(_gameplayConfig.VirtualCamera, Lifetime.Singleton);

            builder.Register<CinemachineCameraService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }
    }
}
