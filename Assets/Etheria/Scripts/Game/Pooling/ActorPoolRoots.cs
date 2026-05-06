using System;
using UnityEngine;

namespace Etheria.Game.Pooling
{
    [Serializable]
    public sealed class ActorPoolRoots : IActorPoolRoots
    {
        [field: SerializeField] public Transform ActiveRoot { get; private set; }
        [field: SerializeField] public Transform InactiveRoot { get; private set; }
    }
}
