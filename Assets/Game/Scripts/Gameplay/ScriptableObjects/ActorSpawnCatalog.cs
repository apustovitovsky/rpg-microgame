using System;
using System.Collections.Generic;
using Game.Navigation;
using UnityEngine;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "EntitySpawnCatalog",
        menuName = "Game/Gameplay/Entity Spawn Catalog")]
    public sealed class ActorSpawnCatalog : ScriptableObject
    {
        [SerializeField] private ActorEntry _player = new();

        [SerializeField]
        private ActorEntry[] _actors =
            Array.Empty<ActorEntry>();

        public ActorEntry Player => _player;

        public IReadOnlyList<ActorEntry> Actors => _actors;

        private void OnValidate()
        {
            _player?.Normalize();

            if (_actors == null)
                return;

            foreach (var actor in _actors)
                actor?.Normalize();
        }

        [Serializable]
        public sealed class ActorEntry
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