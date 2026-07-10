using System;
using Game.CommandSystem;
using UnityEngine;

namespace Game.Interaction
{
    public readonly struct InteractCommand :
        IWorldCommand
    {
        public InteractCommand(
            Guid interactorInstanceId,
            Vector3 interactorPosition)
        {
            InteractorInstanceId = interactorInstanceId;
            InteractorPosition = interactorPosition;
        }

        public Guid InteractorInstanceId { get; }

        public Vector3 InteractorPosition { get; }
    }
}