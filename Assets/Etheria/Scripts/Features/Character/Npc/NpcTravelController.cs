using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Features.Character
{
    [RequireComponent(typeof(NpcMotor))]
    public sealed class NpcTravelController : MonoBehaviour
    {
        [SerializeField] private NpcMotor _motor;
        [SerializeField] private NpcPatrol _patrol;

        private Vector3[] _route;
        private int _routeIndex;

        private Action _arrived;
        private bool _isTraveling;

        private void Awake()
        {
            _motor = _motor != null ? _motor : GetComponent<NpcMotor>();
            _patrol = _patrol != null ? _patrol : GetComponent<NpcPatrol>();
        }

        private void Update()
        {
            if (!_isTraveling || _motor == null)
                return;

            if (!_motor.HasArrived)
                return;

            if (_route != null && _routeIndex + 1 < _route.Length)
            {
                _routeIndex++;
                _motor.MoveTo(_route[_routeIndex]);
                return;
            }

            _isTraveling = false;
            _motor.Stop();

            Action arrived = _arrived;
            _arrived = null;
            _route = null;
            _routeIndex = 0;

            arrived?.Invoke();
        }

        public bool TravelTo(
            Vector3 destination,
            Action onArrived)
        {
            if (_motor == null)
                return false;

            _arrived = onArrived;
            _isTraveling = true;
            _route = null;
            _routeIndex = 0;

            if (_patrol != null)
                _patrol.enabled = false;

            _motor.EndManualRotation();
            _motor.MoveTo(destination);

            return true;
        }

        public bool TravelRoute(
            IReadOnlyList<Vector3> route,
            Action onArrived)
        {
            if (_motor == null || route == null || route.Count == 0)
                return false;

            _arrived = onArrived;
            _isTraveling = true;
            _route = new Vector3[route.Count];
            _routeIndex = 0;

            for (int i = 0; i < route.Count; i++)
                _route[i] = route[i];

            if (_patrol != null)
                _patrol.enabled = false;

            _motor.EndManualRotation();
            _motor.MoveTo(_route[_routeIndex]);

            return true;
        }

        public void Stop()
        {
            _isTraveling = false;
            _arrived = null;
            _route = null;
            _routeIndex = 0;

            if (_motor != null)
                _motor.Stop();
        }
    }
}
