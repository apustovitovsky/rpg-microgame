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
        [SerializeField] private string _displayName;

        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        [SerializeField] private PickupEffect[] _effects = Array.Empty<PickupEffect>();

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName)
            ? name
            : _displayName.Trim();

        public IReadOnlyList<PickupEffect> Effects => _effects;
    }
}