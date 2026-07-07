using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Pickup
{
    [CreateAssetMenu(
        fileName = "PickupDefinition",
        menuName = "Game/Pickup/Pickup Definition")]
    public sealed class PickupDefinition : ScriptableObject
    {
        [SerializeField] private PickupEffect[] _effects = Array.Empty<PickupEffect>();

        public IReadOnlyList<PickupEffect> Effects => _effects;
    }
}