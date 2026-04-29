using Etheria.Game.Actor;
using Etheria.Game.Player;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Actor
{
    public sealed class ControllableActor : IControllableActor
    {
        private readonly LifetimeScope _scope;
        private readonly ActorRuntimeRefs _runtimeRefs;
        private readonly PlayerAvatarHandlers _handlers;
        private readonly ITargetable _targetable;

        public ControllableActor(
            LifetimeScope scope,
            ActorRuntimeRefs runtimeRefs,
            IActorInputHandler inputHandler,
            IActorFacingHandler facingHandler,
            ITargetable targetable)
        {
            _scope = scope;
            _runtimeRefs = runtimeRefs;
            _handlers = new PlayerAvatarHandlers(inputHandler, facingHandler);
            _targetable = targetable;
        }

        public Transform Root => _scope.transform;

        public Transform CameraPivot => _runtimeRefs.CameraPivot != null
            ? _runtimeRefs.CameraPivot
            : _scope.transform;

        public PlayerAvatarHandlers Handlers => _handlers;

        public ITargetable Targetable => _targetable;
    }
}
