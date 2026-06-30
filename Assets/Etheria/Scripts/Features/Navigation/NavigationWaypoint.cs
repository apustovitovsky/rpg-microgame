using Etheria.Game.World;
using UnityEngine;
using System.Collections.Generic;
using System;
using Etheria.Core.Helpers;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Etheria.Navigation
{
    public sealed class NavigationWaypoint : MonoBehaviour
    {
        [SerializeField] private string _id;

        [SerializeField]
        [Range(0.25f, 3f)]
        private float _radius = 0.3f;

        [SerializeField] private NavigationFlag _flags;
        [SerializeField] private List<Neighbor> _neighbors = new();

        public IReadOnlyList<Neighbor> Neighbors =>
            _neighbors;

        public string Id => _id;

        public NavigationFlag Flags =>
            _flags;

        public Vector3 Position =>
            transform.position;

        public Quaternion Rotation =>
            transform.rotation;

        public float Radius =>
            _radius;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_id))
                _id = name;

            _id = _id?.Trim();

            if (_radius < 0.01f)
                _radius = 0.01f;
        }

        [Serializable]
        public sealed class Neighbor
        {
            [SerializeField] private NavigationWaypoint _waypoint;
            [SerializeField] private NavigationFlag _flags;

            public NavigationWaypoint Waypoint =>
                _waypoint;

            public NavigationFlag Flags =>
                _flags;
        }

#if UNITY_EDITOR
        private const float LabelHeight = 1.6f;
        private const float LineEndOffset = 0.2f;

        private static readonly Color WaypointColor =
            Color.white;

        private static readonly Color NeighborColor =
            Color.white;

        private static readonly Color ReachColor =
            Color.white;

        private void OnDrawGizmosSelected()
        {
            DrawWaypointGizmos(WaypointColor);
            DrawNeighborGizmos();
        }

        public void DrawReferenceGizmos(
            Color color)
        {
            DrawWaypointGizmos(color);
        }

        private void DrawNeighborGizmos()
        {
            if (_neighbors == null)
                return;

            foreach (var neighbor in _neighbors)
            {
                if (neighbor == null || neighbor.Waypoint == null)
                    continue;

                var waypoint = neighbor.Waypoint;

                if (waypoint == this)
                    continue;

                DrawConnectionTo(waypoint, NeighborColor);
                waypoint.DrawReferenceGizmos(NeighborColor);
            }
        }

        private void DrawConnectionTo(
            NavigationWaypoint waypoint,
            Color color)
        {
            Gizmos.color = color;

            Gizmos.DrawLine(
                transform.position,
                waypoint.transform.position);
        }

        private void DrawWaypointGizmos(
            Color color)
        {
            var groundPosition = transform.position;
            var labelPosition = groundPosition + Vector3.up * LabelHeight;
            var lineEndPosition = labelPosition - Vector3.up * LineEndOffset;

            DrawDisc(ReachColor, _radius);

            Gizmos.color = color;
            Handles.color = color;

            Gizmos.DrawLine(
                groundPosition,
                lineEndPosition);

            DrawFacing(color);
            DrawLabel(labelPosition, color);
        }

        private void DrawDisc(
            Color color,
            float radius)
        {
            Handles.color = color;

            Handles.DrawWireDisc(
                transform.position,
                Vector3.up,
                radius);
        }

        private void DrawFacing(
            Color color)
        {
            var origin = transform.position;
            var target =
                origin + transform.forward * _radius;

            Gizmos.color = color;

            Gizmos.DrawLine(
                origin,
                target);
        }

        private void DrawLabel(
            Vector3 labelPosition,
            Color color)
        {
            var label = string.IsNullOrWhiteSpace(_id)
                ? name
                : _id;

            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = color
                }
            };

            Handles.Label(
                labelPosition,
                label,
                style);
        }
#endif
    }
}