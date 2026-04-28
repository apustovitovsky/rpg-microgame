using System;
using UnityEngine;

namespace Etheria.Features.Gameplay
{
    [Serializable]
    public sealed class PickupPoolRoots : IPickupPoolRoots
    {
        [field: SerializeField] public Transform ActiveRoot { get; private set; }
        [field: SerializeField] public Transform InactiveRoot { get; private set; }
    }
}
