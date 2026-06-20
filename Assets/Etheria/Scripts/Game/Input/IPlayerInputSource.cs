using System;
using UnityEngine;

namespace Etheria.Game.Input
{
    public interface IPlayerInputSource
    {
        Vector2 MouseDelta { get; }
        Vector2 MoveComposite { get; }
        float MovementInputDuration { get; set; }
        bool MovementInputDetected { get; }

        event Action InteractPerformed;

        public Action OnAimActivated { get; set; }
        public Action OnAimDeactivated { get; set; }
        public Action OnCrouchActivated { get; set; }
        public Action OnCrouchDeactivated { get; set; }
        public Action OnJumpPerformed { get; set; }
        public Action OnLockOnToggled { get; set; }
        public Action OnSprintActivated { get; set; }
        public Action OnSprintDeactivated { get; set; }
        public Action OnWalkToggled { get; set; }
    }
}