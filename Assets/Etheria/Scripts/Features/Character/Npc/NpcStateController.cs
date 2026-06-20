using Etheria.Game.Dialogue;
using UnityEngine;

namespace Etheria.Features.Character
{
    public sealed class NpcStateController :
        MonoBehaviour,
        IDialogueParticipant
    {
        public enum State
        {
            Patrol,
            Dialogue
        }

        [SerializeField] private NpcMotor _motor;
        [SerializeField] private NpcPatrol _patrol;
        [SerializeField, Min(0f)] private float _rotationSpeed = 360f;

        private Transform _interlocutor;

        public State CurrentState { get; private set; }

        private void Awake()
        {
            EnterPatrol();
        }

        private void Update()
        {
            if (CurrentState == State.Dialogue)
                RotateToInterlocutor();
        }

        public void OnDialogueStarted(Transform interlocutor)
        {
            CurrentState = State.Dialogue;
            _interlocutor = interlocutor;

            _patrol.enabled = false;
            _motor.Stop();
            _motor.BeginManualRotation();
        }

        public void OnDialogueCompleted()
        {
            _interlocutor = null;
            _motor.EndManualRotation();
            EnterPatrol();
        }

        private void EnterPatrol()
        {
            CurrentState = State.Patrol;
            _patrol.enabled = true;
            _patrol.Resume();
        }

        private void RotateToInterlocutor()
        {
            if (_interlocutor == null)
                return;

            Vector3 direction =
                _interlocutor.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }
    }
}