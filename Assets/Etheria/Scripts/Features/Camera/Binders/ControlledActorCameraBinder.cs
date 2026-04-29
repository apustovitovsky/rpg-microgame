using System;
using Etheria.Game.Camera;
using Etheria.Game.Player;
using VContainer.Unity;

namespace Etheria.Features.Camera
{
    public sealed class PlayerAvatarCameraBinder : IStartable, IDisposable
    {
        private readonly IPlayerAvatarProvider _playerAvatarProvider;
        private readonly ICameraService _cameraService;

        public PlayerAvatarCameraBinder(
            IPlayerAvatarProvider playerAvatarProvider,
            ICameraService cameraService)
        {
            _playerAvatarProvider = playerAvatarProvider;
            _cameraService = cameraService;
        }

        public void Start()
        {
            _playerAvatarProvider.Changed += OnControlledActorChanged;
            OnControlledActorChanged(_playerAvatarProvider.Current);
        }

        public void Dispose()
        {
            _playerAvatarProvider.Changed -= OnControlledActorChanged;
        }

        private void OnControlledActorChanged(PlayerAvatarContext? context)
        {
            if (context.HasValue)
            {
                _cameraService.SetTarget(context.Value.CameraPivot);
                return;
            }

            _cameraService.RemoveTarget();
        }
    }
}
