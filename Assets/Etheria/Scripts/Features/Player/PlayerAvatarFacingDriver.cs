using Etheria.Game.Targeting;
using UnityEngine;
using Etheria.Game.Camera;
using Etheria.Game.Player;
using VContainer.Unity;

namespace Etheria.Features.Player
{
    public sealed class PlayerAvatarFacingDriver : ITickable
    {
        private readonly IPlayerAvatarProvider _playerAvatarProvider;
        private readonly IPlayerLookService _playerLookService;
        private readonly ITargetingService _targetingService;

        public PlayerAvatarFacingDriver(
            IPlayerAvatarProvider playerAvatarProvider,
            IPlayerLookService playerLookService,
            ITargetingService targetingService)
        {
            _playerAvatarProvider = playerAvatarProvider;
            _playerLookService = playerLookService;
            _targetingService = targetingService;
        }

        public void Tick()
        {
            var avatar = _playerAvatarProvider.Current;
            if (!avatar.HasValue)
                return;

            var currentTarget = _targetingService.CurrentTarget;
            var facing = GetFacing(avatar.Value, currentTarget);
            if (facing.sqrMagnitude <= 0.001f)
                return;

            if (currentTarget != null)
                _playerLookService.SetYawFromWorldDirection(facing);

            avatar.Value.Handlers.Facing.HandleFace(facing);
        }

        private Vector3 GetFacing(PlayerAvatarContext avatar, ITargetable currentTarget)
        {
            if (currentTarget?.AimPoint != null)
            {
                var toTarget = currentTarget.AimPoint.position - avatar.Root.position;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude > 0.001f)
                    return toTarget.normalized;
            }

            var forward = _playerLookService.Forward;
            forward.y = 0f;

            return forward.sqrMagnitude > 0.001f
                ? forward.normalized
                : Vector3.zero;
        }

    }
}
