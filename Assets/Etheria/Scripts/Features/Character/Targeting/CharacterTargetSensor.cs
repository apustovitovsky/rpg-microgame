using System.Collections.Generic;
using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CharacterTargetSensor :
    MonoBehaviour,
    ITargetCandidateSource
    {
        private readonly HashSet<ITargetCandidate> _candidates = new();

        public Transform Origin => transform.parent != null
            ? transform.parent
            : transform;

        public IReadOnlyCollection<ITargetCandidate> Candidates
        {
            get
            {
                _candidates.RemoveWhere(IsDestroyed);
                return _candidates;
            }
        }

        private static bool IsDestroyed(ITargetCandidate candidate)
        {
            return candidate == null ||
                   candidate is UnityEngine.Object unityObject &&
                   unityObject == null;
        }

        private void OnTriggerEnter(Collider other)
        {
            var candidate =
                other.GetComponentInParent<TargetCandidate>();

            if (candidate != null)
                _candidates.Add(candidate);
        }

        private void OnTriggerExit(Collider other)
        {
            var candidate =
                other.GetComponentInParent<TargetCandidate>();

            if (candidate != null)
                _candidates.Remove(candidate);
        }

        private void Reset()
        {
            var sensorCollider = GetComponent<SphereCollider>();
            sensorCollider.isTrigger = true;
            sensorCollider.radius = 10f;

            var rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        private void OnDisable()
        {
            _candidates.Clear();
        }
    }
}