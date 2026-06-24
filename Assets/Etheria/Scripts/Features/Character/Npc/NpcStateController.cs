using System;
using System.Collections.Generic;
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
            Dialogue,
            Travel
        }

        [SerializeField] private NpcTravelController _travel;
        [SerializeField] private NpcMotor _motor;
        [SerializeField] private NpcPatrol _patrol;
        [SerializeField, Min(0f)] private float _rotationSpeed = 360f;

        private Transform _interlocutor;

        public State CurrentState { get; private set; }

        private void Awake()
        {
            if (_travel == null)
                _travel = GetComponent<NpcTravelController>();

            if (_motor == null)
                _motor = GetComponent<NpcMotor>();

            if (_patrol == null)
                _patrol = GetComponent<NpcPatrol>();

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

            if (_patrol != null)
                _patrol.enabled = false;

            if (_motor != null)
            {
                _motor.Stop();
                _motor.BeginManualRotation();
            }
        }
        
        public bool TravelRoute(
            IReadOnlyList<Vector3> route,
            Action arrived)
        {
            if (_travel == null)
                return false;

            CurrentState = State.Travel;
            _interlocutor = null;

            if (_patrol != null)
                _patrol.enabled = false;

            if (_motor != null)
                _motor.EndManualRotation();

            return _travel.TravelRoute(
                route,
                () =>
                {
                    arrived?.Invoke();

                    if (CurrentState == State.Travel)
                        EnterPatrol();
                });
        }
        public void OnDialogueCompleted()
        {
            _interlocutor = null;

            if (_motor != null)
                _motor.EndManualRotation();

            if (CurrentState == State.Dialogue)
                EnterPatrol();
        }

        public bool TravelTo(
            Vector3 destination,
            Action arrived)
        {
            if (_travel == null)
                return false;

            CurrentState = State.Travel;
            _interlocutor = null;

            if (_patrol != null)
                _patrol.enabled = false;

            if (_motor != null)
                _motor.EndManualRotation();

            return _travel.TravelTo(
                destination,
                () =>
                {
                    arrived?.Invoke();

                    if (CurrentState == State.Travel)
                        EnterPatrol();
                });
        }

        private void EnterPatrol()
        {
            CurrentState = State.Patrol;

            if (_patrol == null)
                return;

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