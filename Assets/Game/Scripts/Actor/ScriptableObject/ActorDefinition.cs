using System;
using Game.World;
using UnityEngine;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorDefinition",
        menuName = "Game/Actor/Actor Definition")]
    public sealed class ActorDefinition :
        WorldDefinition<ActorInstance>
    {
        public override ActorInstance CreateInstance(
            Guid? instanceId = null)
        {
            return new ActorInstance(
                instanceId ?? Guid.NewGuid(),
                this);
        }
    }
}