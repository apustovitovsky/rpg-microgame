using System;
using UnityEngine;

namespace RPG.Gameplay
{
    public sealed class ActorTargetable : IActorTargetable
    {
        public ActorId ActorId { get; }
        public string DisplayName { get; }
        public Transform AimPoint { get; }
        public Transform UiAnchor { get; }
        public bool IsTargetable => true;


        public ActorTargetable(ActorConfigSO actorConfig, ActorRuntimeRefs runtimeRefs)
        {
            ActorId = new ActorId(Guid.NewGuid());
            DisplayName = actorConfig.DisplayName;
            AimPoint = runtimeRefs.AimPoint;
            UiAnchor = runtimeRefs.UiAnchor;
        }
    }
}
