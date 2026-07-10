using System;
using UnityEngine;

namespace Game.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(
            Guid interactorInstanceId,
            Vector3 origin,
            Guid targetInstanceId)
        {
            InteractorInstanceId = interactorInstanceId;
            Origin = origin;
            TargetInstanceId = targetInstanceId;
        }

        public Guid InteractorInstanceId { get; }

        public Vector3 Origin { get; }

        public Guid TargetInstanceId { get; }
    }
}