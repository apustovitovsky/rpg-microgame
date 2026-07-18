using System;
using Game.Core;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorRuntimeAnchorBinding :
        IInitializable
    {
        private readonly IInstanceIdentity _identity;
        private readonly IActorRuntimeRegistry _runtimes;
        private readonly ActorRuntimeAnchors _anchors;

        public ActorRuntimeAnchorBinding(
            IInstanceIdentity identity,
            IActorRuntimeRegistry runtimes,
            ActorRuntimeAnchors anchors)
        {
            _identity = identity
                ?? throw new ArgumentNullException(nameof(identity));

            _runtimes = runtimes
                ?? throw new ArgumentNullException(nameof(runtimes));

            _anchors = anchors;
        }

        public void Initialize()
        {
            if (!_runtimes.TryGet(
                    _identity.InstanceId,
                    out var runtime))
            {
                throw new InvalidOperationException(
                    $"Actor runtime for instance " +
                    $"'{_identity.InstanceId}' was not found.");
            }

            runtime.BindAnchors(
                _anchors.Root,
                _anchors.FocusPoint);
        }
    }
}