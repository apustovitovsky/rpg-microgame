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
        private readonly IPlayerLookService _playerLookService;

        private IActorInputHandler _currentHandler;

        public PlayerAvatarCameraBinder(
            IPlayerAvatarProvider playerAvatarProvider,
            ICameraService cameraService,
            IPlayerLookService playerLookService)
        {
            _playerAvatarProvider = playerAvatarProvider;
            _cameraService = cameraService;
            _playerLookService = playerLookService;
        }

        public void Start()
        {
            _playerAvatarProvider.Changed += OnPlayerAvatarChanged;
            OnPlayerAvatarChanged(_playerAvatarProvider.Current);
        }

        public void Dispose()
        {
            _playerAvatarProvider.Changed -= OnPlayerAvatarChanged;
        }

        private void OnPlayerAvatarChanged(PlayerAvatarContext? context)
        {
            if (_currentHandler != null)
            {
                _currentHandler = null;
            }

            if (context.HasValue)
            {
                var avatar = context.Value;

                _currentHandler = avatar.InputHandler;
                _playerLookService.SetTarget(avatar.Root, avatar.CameraPivot);
                _cameraService.SetTarget(avatar.CameraPivot);
                return;
            }

            _playerLookService.RemoveTarget();
            _cameraService.RemoveTarget();
        }
    }
}
