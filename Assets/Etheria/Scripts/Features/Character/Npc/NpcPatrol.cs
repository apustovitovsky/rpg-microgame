using UnityEngine;

namespace Etheria.Features.Character
{
    public sealed class NpcPatrol : MonoBehaviour
    {
        [SerializeField] private NpcMotor _motor;
        [SerializeField] private Transform[] _points;
        [SerializeField, Min(0f)] private float _waitDuration = 2f;

        private int _pointIndex;
        private float _waitTimer;
        private bool _isWaiting;

        private void Start()
        {
            if (enabled)
                MoveToCurrentPoint();
        }

        public void Resume()
        {
            _isWaiting = false;
            MoveToCurrentPoint();
        }

        private void Update()
        {
            if (_points.Length == 0)
                return;

            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;

                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    SelectNextPoint();
                    MoveToCurrentPoint();
                }

                return;
            }

            if (_motor.HasArrived)
            {
                _motor.Stop();
                _isWaiting = true;
                _waitTimer = _waitDuration;
            }
        }

        private void SelectNextPoint()
        {
            _pointIndex = (_pointIndex + 1) % _points.Length;
        }

        private void MoveToCurrentPoint()
        {
            if (_points.Length == 0 || _points[_pointIndex] == null)
                return;

            _motor.MoveTo(_points[_pointIndex].position);
        }
    }
}