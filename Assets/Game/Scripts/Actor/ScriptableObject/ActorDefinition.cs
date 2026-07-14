using System;
using Game.Core;
using UnityEngine;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorDefinition",
        menuName = "Game/Actor/Actor Definition")]
    public sealed class ActorDefinition :
        AssetDefinition<ActorInstance, ActorFragment>
    {
        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        public override ActorInstance CreateInstance(
            Guid? instanceId = null)
        {
            return new ActorInstance(
                instanceId ?? Guid.NewGuid(),
                this);
        }
    }
}