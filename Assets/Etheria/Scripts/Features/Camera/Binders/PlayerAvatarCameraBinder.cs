using System;
using Etheria.Game.Camera;
using Etheria.Game.Player;
using VContainer.Unity;

namespace Etheria.Features.Camera
{
    public sealed class PlayerAvatarCameraBinder : IStartable, IDisposable
    {
        private readonly IPlayerAvatarProvider _playerAvatarProvider;
        private readonly ICameraFollowService _cameraFollowService;
        private readonly IPlayerLookService _playerLookService;

        public PlayerAvatarCameraBinder(
            IPlayerAvatarProvider playerAvatarProvider,
            ICameraFollowService cameraFollowService,
            IPlayerLookService playerLookService)
        {
            _playerAvatarProvider = playerAvatarProvider;
            _cameraFollowService = cameraFollowService;
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
            if (context.HasValue)
            {
                var avatar = context.Value;

                _playerLookService.SetTarget(avatar.Root, avatar.CameraPivot);
                _cameraFollowService.SetTarget(avatar.CameraPivot);
                return;
            }

            _playerLookService.RemoveTarget();
            _cameraFollowService.RemoveTarget();
        }
    }
}
