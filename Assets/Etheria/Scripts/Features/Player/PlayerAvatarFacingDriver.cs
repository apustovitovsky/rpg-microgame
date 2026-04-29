using Etheria.Game.Camera;
using Etheria.Game.Player;
using VContainer.Unity;

namespace Etheria.Features.Player
{
    public sealed class PlayerAvatarFacingDriver : ITickable
    {
        private readonly IPlayerAvatarProvider _playerAvatarProvider;
        private readonly IPlayerLookService _playerLookService;

        public PlayerAvatarFacingDriver(
            IPlayerAvatarProvider playerAvatarProvider,
            IPlayerLookService playerLookService)
        {
            _playerAvatarProvider = playerAvatarProvider;
            _playerLookService = playerLookService;
        }

        public void Tick()
        {
            var avatar = _playerAvatarProvider.Current;
            if (!avatar.HasValue)
                return;

            var forward = _playerLookService.Forward;
            if (forward.sqrMagnitude <= 0.001f)
                return;

            avatar.Value.InputHandler.HandleFace(forward);
        }
    }
}
