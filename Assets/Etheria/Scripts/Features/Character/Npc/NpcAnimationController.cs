using UnityEngine;
using UnityEngine.AI;

namespace Etheria.Features.Character
{
    public sealed class NpcAnimationController : MonoBehaviour
    {
        private static readonly int IsGrounded =
            Animator.StringToHash("IsGrounded");

        private static readonly int IsStopped =
            Animator.StringToHash("IsStopped");

        private static readonly int MoveSpeed =
            Animator.StringToHash("MoveSpeed");

        private static readonly int MovementInputHeld =
            Animator.StringToHash("MovementInputHeld");

        private static readonly int MovementInputPressed =
            Animator.StringToHash("MovementInputPressed");

        private static readonly int CurrentGait =
            Animator.StringToHash("CurrentGait");

        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _agent;

        private void Update()
        {
            float speed = _agent.velocity.magnitude;
            bool isMoving = speed > 0.05f;

            _animator.SetBool(IsGrounded, _agent.isOnNavMesh);
            _animator.SetBool(IsStopped, !isMoving);
            _animator.SetBool(MovementInputHeld, isMoving);
            _animator.SetBool(MovementInputPressed, isMoving);

            _animator.SetFloat(MoveSpeed, speed);
            _animator.SetInteger(CurrentGait, GetGait(speed));
        }

        private static int GetGait(float speed)
        {
            if (speed < 0.05f)
                return 0; // Idle

            if (speed < 2f)
                return 1; // Walk

            if (speed < 4f)
                return 2; // Run

            return 3; // Sprint
        }
    }
}