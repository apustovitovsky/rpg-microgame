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
        private ITargetable _currentAvatarTargetable;

        public TargetingTracker(
            ITargetingService targetingService,
            IPlayerAvatarProvider playerAvatarProvider)
        {
            _targetingService = targetingService;
            _playerAvatarProvider = playerAvatarProvider;
        }

        public void Start()
        {
            _currentAvatarTargetable = _playerAvatarProvider.Current?.Targetable;
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
            var nextTargetable = avatar?.Targetable;
            if (ReferenceEquals(_currentAvatarTargetable, nextTargetable))
                return;

            _currentAvatarTargetable = nextTargetable;
            _targetingService.ClearTarget();
        }
    }
}
