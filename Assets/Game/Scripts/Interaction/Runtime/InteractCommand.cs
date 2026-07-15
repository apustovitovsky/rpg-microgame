using System;
using Game.Commands;
using UnityEngine;

namespace Game.Interaction
{
    public readonly struct InteractCommand :
        ICommand
    {
        public InteractCommand(
            Guid interactorInstanceId,
            Vector3 interactionOrigin)
        {
            InteractorInstanceId = interactorInstanceId;
            InteractionOrigin = interactionOrigin;
        }

        public Guid InteractorInstanceId { get; }

        public Vector3 InteractionOrigin { get; }
    }
}