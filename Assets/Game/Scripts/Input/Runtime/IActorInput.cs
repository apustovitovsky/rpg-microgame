using System;
using UnityEngine;

namespace Game.Input
{
    public interface IActorInput
    {
        Vector2 LookDelta { get; }

        Vector2 MoveComposite { get; }

        float MovementInputDuration { get; set; }

        bool MovementInputDetected { get; }

        event Action OnAimActivated;
        event Action OnAimDeactivated;
        event Action OnCrouchActivated;
        event Action OnCrouchDeactivated;
        event Action OnJumpPerformed;
        event Action OnLockOnToggled;
        event Action OnSprintActivated;
        event Action OnSprintDeactivated;
        event Action OnWalkToggled;
    }
}