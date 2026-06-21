using UnityEngine;
using UnityEngine.AI;

namespace Etheria.Features.Character
{
    public sealed class NpcCharacterAnimationController : MonoBehaviour
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

        [SerializeField] private Transform _visualRoot;

        private CharacterVisual _visual;

        private void Awake()
        {
            _visual = _visualRoot.GetComponentInChildren<CharacterVisual>(true);
        }

        [SerializeField] private NavMeshAgent _agent;

        private void Update()
        {
            if (_visual == null || _agent == null)
                return;

            float speed = _agent.velocity.magnitude;
            bool isMoving = speed > 0.05f;

            _visual.Animator.SetBool(IsGrounded, _agent.isOnNavMesh);
            _visual.Animator.SetBool(IsStopped, !isMoving);
            _visual.Animator.SetBool(MovementInputHeld, isMoving);
            _visual.Animator.SetBool(MovementInputPressed, isMoving);

            _visual.Animator.SetFloat(MoveSpeed, speed);
            _visual.Animator.SetInteger(CurrentGait, GetGait(speed));
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