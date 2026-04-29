using System;
using Etheria.Game.Input;
using Etheria.Game.Player;
using VContainer.Unity;

namespace Etheria.Features.Input
{
    public sealed class PlayerAvatarInputBinder : IStartable, IDisposable
    {
        private readonly IPlayerAvatarProvider _playerAvatarProvider;
        private readonly IGameInputRouter _gameInputRouter;

        public PlayerAvatarInputBinder(
            IPlayerAvatarProvider playerAvatarProvider,
            IGameInputRouter gameInputRouter)
        {
            _playerAvatarProvider = playerAvatarProvider;
            _gameInputRouter = gameInputRouter;
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
                _gameInputRouter.SetHandler(context.Value.InputHandler);
                return;
            }

            var currentHandler = _playerAvatarProvider.Current?.InputHandler;
            if (currentHandler != null)
                _gameInputRouter.RemoveHandler(currentHandler);
        }
    }
}
