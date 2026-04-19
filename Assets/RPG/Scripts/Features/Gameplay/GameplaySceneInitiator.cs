using System.Threading;
using Cysharp.Threading.Tasks;
using RPG.Core;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public class GameplaySceneInitiator : IAsyncStartable
    {
        private readonly ISceneLoadingService _sceneLoadingService;
        private readonly GameplayConfigSO _gameplayConfig;
        private readonly IGameplayInputRouter _inputRouter;
        private readonly ICameraInputHandler _cameraInput;
        private readonly IActorSpawnService _characterSpawnService;
        private readonly SceneReadinessChannel _readinessChannel;

        public GameplaySceneInitiator(
            ISceneLoadingService sceneLoadingService,
            GameplayConfigSO gameplayConfig,
            IGameplayInputRouter inputRouter,
            ICameraInputHandler cameraInput,
            IActorSpawnService characterSpawnService,
            SceneReadinessChannel readinessChannel)
        {
            _sceneLoadingService = sceneLoadingService;
            _gameplayConfig = gameplayConfig;
            _inputRouter = inputRouter;
            _cameraInput = cameraInput;
            _characterSpawnService = characterSpawnService;
            _readinessChannel = readinessChannel;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await _sceneLoadingService.LoadSceneAdditiveAsync(_gameplayConfig.WorldScene);
            _inputRouter.SetHandler(_cameraInput);

            _characterSpawnService.Spawn(_gameplayConfig.DronePrefab, Vector3.zero);
            _characterSpawnService.SpawnAndPossess(_gameplayConfig.PlayerPrefab, Vector3.zero);

            _readinessChannel.Complete(true);
        }
    }
}
