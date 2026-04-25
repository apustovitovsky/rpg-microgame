using System;
using UnityEngine;

namespace RPG.Gameplay
{
    [Serializable]
    public sealed class ActorPoolRoots : IActorPoolRoots
    {
        [field: SerializeField] public Transform ActiveRoot { get; private set; }
        [field: SerializeField] public Transform InactiveRoot { get; private set; }
    }
}