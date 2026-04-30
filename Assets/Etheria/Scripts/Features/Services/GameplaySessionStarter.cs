using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Game.Actor;
using Etheria.Game.Player;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features
{
    public sealed class GameplaySessionStarter : IAsyncStartable
    {
        private readonly GameplayConfigSO _gameplayConfig;
        private readonly IActorFactory _actorFactory;
        private readonly IPlayerController _possessionService;

        public GameplaySessionStarter(
            GameplayConfigSO gameplayConfig,
            IActorFactory actorFactory,
            IPlayerController possessionService)
        {
            _gameplayConfig = gameplayConfig;
            _actorFactory = actorFactory;
            _possessionService = possessionService;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            var player = _actorFactory.Create(_gameplayConfig.PlayerAvatarPrefab, Vector3.zero);
            _possessionService.Possess(player);

            for (var i = 0; i < _gameplayConfig.AdditionalPlayersCount; i++)
            {
                var spawnPosition = GetRandomSpawnPosition(_gameplayConfig.AdditionalPlayersSpawnRadius);
                _actorFactory.Create(_gameplayConfig.PlayerAvatarPrefab, spawnPosition);
            }

            await UniTask.CompletedTask;
        }

        private static Vector3 GetRandomSpawnPosition(float radius)
        {
            var point = Random.insideUnitCircle * radius;
            return new Vector3(point.x, 0f, point.y);
        }
    }
}
