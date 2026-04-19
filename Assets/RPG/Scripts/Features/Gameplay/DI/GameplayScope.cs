using RPG.Core;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public class GameplayScope : LifetimeScope
    {
        [SerializeField] private GameplayConfigSO _gameplayConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameplayConfig);

            builder.Register<InputSystem_Actions>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameplaySceneInitiator>()
                .As<ISceneInitiator>();

            builder.Register<CharacterFactory>(Lifetime.Singleton)
                .As<ICharacterFactory>();

            builder.Register<CharacterSpawnService>(Lifetime.Singleton)
                .As<ICharacterSpawnService>();

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
