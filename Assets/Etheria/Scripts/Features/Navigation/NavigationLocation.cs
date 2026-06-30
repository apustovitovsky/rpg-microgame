using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Navigation
{
    public sealed class NavigationLocation : MonoBehaviour
    {
        [SerializeField] private string _id;
        [SerializeField] private List<Anchor> _anchors = new();

        public string Id =>
            _id;

        public IReadOnlyList<Anchor> Anchors =>
            _anchors;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_id))
                _id = name;

            _id = _id?.Trim();
        }

        [Serializable]
        public sealed class Anchor
        {
            [SerializeField] private string _key;
            [SerializeField] private NavigationWaypoint _waypoint;

            public string Key =>
                _key;

            public NavigationWaypoint Waypoint =>
                _waypoint;

            public string NodeId =>
                _waypoint != null
                    ? _waypoint.Id
                    : string.Empty;
        }
    }
}