using System;
using System.Collections.Generic;
using Etheria.Game.World;
using UnityEngine;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "PickupSpawnCatalog",
        menuName = "Game/Gameplay/Pickup Spawn Catalog")]
    public sealed class PickupSpawnCatalog : ScriptableObject
    {
        [SerializeField]
        private PickupEntry[] _pickups =
            Array.Empty<PickupEntry>();

        public IReadOnlyList<PickupEntry> Pickups => _pickups;

        private void OnValidate()
        {
            if (_pickups == null)
                return;

            foreach (var pickup in _pickups)
                pickup?.Normalize();
        }

        [Serializable]
        public sealed class PickupEntry
        {
            [SerializeField] private string _definitionId;
            [SerializeField] private string _locationId;
            [SerializeField]
            private string _anchorKey =
                NavigationAnchorKeys.Default;

            public string DefinitionId => _definitionId;

            public string LocationId => _locationId;

            public string AnchorKey => _anchorKey;

            public void Normalize()
            {
                _definitionId = _definitionId?.Trim();
                _locationId = _locationId?.Trim();

                _anchorKey = string.IsNullOrWhiteSpace(_anchorKey)
                    ? NavigationAnchorKeys.Default
                    : _anchorKey.Trim();
            }
        }
    }
}