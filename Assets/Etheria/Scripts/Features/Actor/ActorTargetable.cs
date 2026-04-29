using Etheria.Features.Actor;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace Etheria.Features.Targeting
{
    public sealed class ActorTargetable : ITargetable
    {
        public string DisplayName { get; }
        public Transform AimPoint { get; }
        public Transform UiAnchor { get; }
        public bool IsTargetable => true;


        public ActorTargetable(
            LifetimeScope scope,
            ActorRuntimeRefs runtimeRefs)
        {
            DisplayName = scope.name;
            AimPoint = runtimeRefs.AimPoint;
            UiAnchor = runtimeRefs.UiAnchor;
        }
    }
}

