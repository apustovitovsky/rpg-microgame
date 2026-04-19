using System.Threading;
using Cysharp.Threading.Tasks;
using RPG.Core;
using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public class GameplaySceneInitiator : ISceneInitiator, IAsyncStartable
    {
        private readonly ISceneLoadingService _sceneLoadingService;
        private readonly GameplayConfigSO _gameplayConfig;
        private readonly IGameplayInputRouter _inputRouter;
        private readonly ICameraInputHandler _cameraInput;
        private readonly ICharacterSpawnService _characterSpawnService;
        private readonly UniTaskCompletionSource<bool> _readyTcs = new();
        public UniTask<bool> Ready => _readyTcs.Task;

        public GameplaySceneInitiator(
            ISceneLoadingService sceneLoadingService,
            GameplayConfigSO gameplayConfig,
            IGameplayInputRouter inputRouter,
            ICameraInputHandler cameraInput,
            ICharacterSpawnService characterSpawnService)
        {
            _sceneLoadingService = sceneLoadingService;
            _gameplayConfig = gameplayConfig;
            _inputRouter = inputRouter;
            _cameraInput = cameraInput;
            _characterSpawnService = characterSpawnService;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await _sceneLoadingService.LoadSceneAdditiveAsync(_gameplayConfig.WorldScene);
            _inputRouter.SetHandler(_cameraInput);

            _characterSpawnService.SpawnTurret(Vector3.zero);
            _characterSpawnService.SpawnDrone(Vector3.zero);

            _readyTcs.TrySetResult(true);
        }
    }
}
