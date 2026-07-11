using System;
using System.Collections.Generic;
using Etheria.Game.World;
using Game.Loot;
using UnityEngine;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "LootContainerSpawnCatalog",
        menuName = "Game/Gameplay/Loot Container Spawn Catalog")]
    public sealed class LootContainerSpawnCatalog :
        ScriptableObject
    {
        [SerializeField]
        private LootContainerEntry[] _containers =
            Array.Empty<LootContainerEntry>();

        public IReadOnlyList<LootContainerEntry> Containers =>
            _containers;

        private void OnValidate()
        {
            if (_containers == null)
                return;

            foreach (var container in _containers)
                container?.Normalize();
        }

        [Serializable]
        public sealed class LootContainerEntry
        {
            [SerializeField]
            private LootContainerDefinition _definition;

            [SerializeField]
            private string _locationId;

            [SerializeField]
            private string _anchorKey =
                NavigationAnchorKeys.Default;

            public LootContainerDefinition Definition =>
                _definition;

            public string LocationId => _locationId;

            public string AnchorKey => _anchorKey;

            public void Normalize()
            {
                _locationId = _locationId?.Trim();

                _anchorKey = string.IsNullOrWhiteSpace(_anchorKey)
                    ? NavigationAnchorKeys.Default
                    : _anchorKey.Trim();
            }
        }
    }
}