using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.AI
{
    [CreateAssetMenu(
        fileName = "NavigationPatrolRoute",
        menuName = "Game/AI/Navigation Patrol Route")]
    public sealed class NavigationPatrolRoute :
        ScriptableObject
    {
        [SerializeField]
        private List<Stop> _stops = new();

        public IReadOnlyList<Stop> Stops =>
            _stops;

        private void OnValidate()
        {
            foreach (var stop in _stops)
            {
                stop?.Normalize();
            }
        }

        [Serializable]
        public sealed class Stop
        {
            [SerializeField]
            private string _locationId;

            [SerializeField]
            private string _anchorKey;

            public string LocationId =>
                _locationId;

            public string AnchorKey =>
                _anchorKey;

            public bool IsValid =>
                !string.IsNullOrWhiteSpace(_locationId) &&
                !string.IsNullOrWhiteSpace(_anchorKey);

            public void Normalize()
            {
                _locationId = _locationId?.Trim();
                _anchorKey = _anchorKey?.Trim();
            }
        }
    }
}