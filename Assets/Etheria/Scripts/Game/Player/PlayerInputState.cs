using System;
using UnityEngine;

namespace Etheria.Features.Player
{
    public sealed class PlayerInputState
    {
        public Vector2 _mouseDelta;
        public Vector2 _moveComposite;

        public float _movementInputDuration;
        public bool _movementInputDetected;

        public Action onAimActivated;
        public Action onAimDeactivated;
        public Action onCrouchActivated;
        public Action onCrouchDeactivated;
        public Action onJumpPerformed;
        public Action onLockOnToggled;
        public Action onSprintActivated;
        public Action onSprintDeactivated;
        public Action onWalkToggled;
    }
}
