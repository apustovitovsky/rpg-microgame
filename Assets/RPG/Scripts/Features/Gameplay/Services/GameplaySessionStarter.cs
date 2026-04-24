using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class GameplaySessionStarter : IAsyncStartable
    {
        private readonly GameplayConfigSO _gameplayConfig;
        private readonly IActorFactory _actorFactory;
        private readonly IPossessionService _possessionService;

        public GameplaySessionStarter(
            GameplayConfigSO gameplayConfig,
            IActorFactory actorFactory,
            IPossessionService possessionService)
        {
            _gameplayConfig = gameplayConfig;
            _actorFactory = actorFactory;
            _possessionService = possessionService;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            _actorFactory.Create(_gameplayConfig.DronePrefab, Vector3.zero);
            var player = _actorFactory.Create(_gameplayConfig.PlayerPrefab, Vector3.zero);
            _possessionService.Possess(player);

            await UniTask.CompletedTask;
        }

    }
}
