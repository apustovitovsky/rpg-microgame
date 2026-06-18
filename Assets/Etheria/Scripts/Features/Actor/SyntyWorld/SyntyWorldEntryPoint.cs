using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Features.Actor;
using Etheria.Features.Input;
using Etheria.Game.Actor;
using Unity.Cinemachine;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features
{
    public sealed class SyntyWorldEntryPoint : IAsyncStartable
    {
        private readonly GameplayConfigSO _gameplayConfig;
        private readonly IGameInputRouter _gameInput;
        private readonly IActorFactory _actorFactory;
        private readonly CinemachineCamera _vCamera;

        public SyntyWorldEntryPoint(
            GameplayConfigSO gameplayConfig,
            IGameInputRouter gameInput,
            IActorFactory actorFactory,
            CinemachineCamera virtualCamera)
        {
            _gameInput = gameInput;
            _gameplayConfig = gameplayConfig;
            _actorFactory = actorFactory;
            _vCamera = virtualCamera;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            var playerScope = _actorFactory.Create(_gameplayConfig.PlayerAvatarPrefab, GetRandomSpawnPosition(10));
            var runtimeRefs = playerScope.gameObject.GetComponent<SyntyActorRuntimeRefs>();
            var syntyCameraController = playerScope.gameObject.GetComponentInChildren<SyntyCameraController>(true);

            _vCamera.Follow = runtimeRefs.CameraPivot;
            _vCamera.LookAt = null;
            
            syntyCameraController.Bind(_gameInput);

            await UniTask.CompletedTask;
        }

        private static Vector3 GetRandomSpawnPosition(float radius)
        {
            var point = Random.insideUnitCircle * radius;
            return new Vector3(point.x, 0f, point.y);
        }
    }
}
