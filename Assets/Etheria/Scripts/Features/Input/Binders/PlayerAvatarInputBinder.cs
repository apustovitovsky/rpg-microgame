using System;
using Etheria.Game.Actor;
using Etheria.Game.Input;
using Etheria.Game.Player;
using VContainer.Unity;

namespace Etheria.Features.Input
{
    public sealed class PlayerAvatarInputBinder : IStartable, IDisposable
    {
        private readonly IPlayerAvatarProvider _playerAvatarProvider;
        private readonly IGameInputRouter _gameInputRouter;
        private IActorInputHandler _currentHandler;

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

        private void OnPlayerAvatarChanged(IPlayerAvatar avatar)
        {
            if (_currentHandler != null)
            {
                _gameInputRouter.RemoveHandler(_currentHandler);
                _currentHandler = null;
            }

            if (avatar != null)
            {
                _currentHandler = avatar.InputHandler;
                _gameInputRouter.SetHandler(_currentHandler);
                return;
            }
        }
    }
}
