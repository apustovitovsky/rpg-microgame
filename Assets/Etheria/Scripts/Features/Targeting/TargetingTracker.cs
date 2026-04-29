using System;
using Etheria.Game.Player;
using Etheria.Game.Targeting;
using VContainer.Unity;

namespace Etheria.Features.Targeting
{
    public sealed class TargetingTracker : IStartable, ITickable, IDisposable
    {
        private readonly ITargetingService _targetingService;
        private readonly IPlayerAvatarProvider _playerAvatarProvider;

        public TargetingTracker(
            ITargetingService targetingService,
            IPlayerAvatarProvider playerAvatarProvider)
        {
            _targetingService = targetingService;
            _playerAvatarProvider = playerAvatarProvider;
        }

        public void Start()
        {
            _playerAvatarProvider.Changed += OnPlayerAvatarChanged;
        }

        public void Dispose()
        {
            _playerAvatarProvider.Changed -= OnPlayerAvatarChanged;
        }

        public void Tick()
        {
            var currentTarget = _targetingService.CurrentTarget;
            if (currentTarget == null)
                return;

            if (_targetingService.IsValid(currentTarget))
                return;

            _targetingService.ClearTarget();
        }

        private void OnPlayerAvatarChanged(PlayerAvatarContext? avatar)
        {
            _targetingService.ClearTarget();
        }
    }
}
