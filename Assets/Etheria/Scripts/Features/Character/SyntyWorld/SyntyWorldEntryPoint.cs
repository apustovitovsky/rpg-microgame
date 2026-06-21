using System.Threading;
using Cysharp.Threading.Tasks;
using Etheria.Features.Character;
using Etheria.Game.Input;
using Etheria.Game.Actor;
using Unity.Cinemachine;
using UnityEngine;
using VContainer.Unity;
using Etheria.Game.Character;

namespace Etheria.Features
{
    public sealed class SyntyWorldEntryPoint : IAsyncStartable
    {
        private readonly GameplayConfigSO _gameplayConfig;
        private readonly IPlayerInputSource _gameInput;
        private readonly IActorFactory _actorFactory;
        private readonly CinemachineCamera _vCamera;

        private readonly IPlayerCharacterProvider _playerCharacterProvider;

        public SyntyWorldEntryPoint(
            GameplayConfigSO gameplayConfig,
            IPlayerInputSource gameInput,
            IActorFactory actorFactory,
            CinemachineCamera virtualCamera,
            IPlayerCharacterProvider playerCharacterProvider)
        {
            _gameInput = gameInput;
            _gameplayConfig = gameplayConfig;
            _actorFactory = actorFactory;
            _vCamera = virtualCamera;
            _playerCharacterProvider = playerCharacterProvider;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            var playerScope = _actorFactory.Create(
                _gameplayConfig.PlayerCharacterPrefab,
                GetRandomSpawnPosition(10));
            var characterController =
                playerScope.GetComponentInChildren<CharacterController>(true);

            _playerCharacterProvider.Set(characterController.transform);

            var runtimeRefs = playerScope.gameObject.GetComponent<SyntyActorRuntimeRefs>();
            var syntyCameraController = playerScope.gameObject.GetComponentInChildren<PlayerCameraLookController>(true);

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
