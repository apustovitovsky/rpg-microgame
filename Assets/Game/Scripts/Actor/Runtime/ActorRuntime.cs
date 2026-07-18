using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor
{
    public sealed class ActorRuntime
    {
        private readonly ActorLocation[] _patrolLocations;

        public ActorRuntime(
            Guid instanceId,
            ActorLocation spawnLocation,
            IReadOnlyList<ActorLocation> patrolLocations)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Actor instance id cannot be empty.",
                    nameof(instanceId));
            }

            if (!spawnLocation.IsValid)
            {
                throw new ArgumentException(
                    "Actor spawn location is invalid.",
                    nameof(spawnLocation));
            }

            if (patrolLocations == null)
            {
                throw new ArgumentNullException(
                    nameof(patrolLocations));
            }

            _patrolLocations =
                new ActorLocation[patrolLocations.Count];

            for (var index = 0;
                 index < patrolLocations.Count;
                 index++)
            {
                var location = patrolLocations[index];

                if (!location.IsValid)
                {
                    throw new ArgumentException(
                        $"Patrol location {index} is invalid.",
                        nameof(patrolLocations));
                }

                _patrolLocations[index] = location;
            }

            InstanceId = instanceId;
            SpawnLocation = spawnLocation;
        }

        public Guid InstanceId { get; }

        public Transform Root { get; private set; }

        public Transform FocusPoint { get; private set; }

        public ActorLocation SpawnLocation { get; }

        public IReadOnlyList<ActorLocation> PatrolLocations =>
            _patrolLocations;

        public bool HasPatrol =>
            _patrolLocations.Length > 0;

        public void BindAnchors(
            Transform root,
            Transform focusPoint)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (focusPoint == null)
                throw new ArgumentNullException(nameof(focusPoint));

            Root = root;
            FocusPoint = focusPoint;
        }
    }

    public readonly struct ActorLocation
    {
        public ActorLocation(
            string locationId,
            string anchorKey)
        {
            LocationId = locationId;
            AnchorKey = anchorKey;
        }

        public string LocationId { get; }

        public string AnchorKey { get; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(LocationId) &&
            !string.IsNullOrWhiteSpace(AnchorKey);
    }
}