using Etheria.Game.Actor;
using Etheria.Game.Player;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Actor
{
    public sealed class PlayerAvatar : IPlayerAvatar
    {
        private readonly LifetimeScope _scope;
        private readonly IPlayerAvatarInfo _info;
        private readonly ICameraPivotProvider _cameraPivotProvider;
        private readonly IActorInputHandler _inputHandler;
        private readonly IActorFacingHandler _facingHandler;

        public PlayerAvatar(
            LifetimeScope scope,
            IPlayerAvatarInfo info,
            ICameraPivotProvider cameraPivotProvider,
            IActorInputHandler inputHandler,
            IActorFacingHandler facingHandler)
        {
            _scope = scope;
            _info = info;
            _cameraPivotProvider = cameraPivotProvider;
            _inputHandler = inputHandler;
            _facingHandler = facingHandler;
        }

        public IPlayerAvatarInfo Info => _info;
        public IActorInputHandler InputHandler => _inputHandler;
        public IActorFacingHandler FacingHandler => _facingHandler;

        public Transform Root => _scope.transform;

        public Transform CameraPivot => _cameraPivotProvider.CameraPivot != null
            ? _cameraPivotProvider.CameraPivot
            : _scope.transform;

        public bool TryGet<T>(out T capability) where T : class
        {
            if (_inputHandler is T input)
            {
                capability = input;
                return true;
            }

            if (_facingHandler is T facing)
            {
                capability = facing;
                return true;
            }

            capability = null;
            return false;
        }
    }
}
