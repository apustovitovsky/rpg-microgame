using System.Collections.Generic;
using UnityEngine;

namespace Game.Targeting
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TargetPerception :
        MonoBehaviour
    {
        [SerializeField]
        private LayerMask _layerMask = ~0;

        [SerializeField]
        private Targetable _self;

        private readonly Dictionary<ITargetable, int> _overlapCounts = new();
        private readonly HashSet<ITargetable> _candidates = new();

        public IReadOnlyCollection<ITargetable> Candidates
        {
            get
            {
                RemoveDestroyedCandidates();
                return _candidates;
            }
        }

        private void OnTriggerEnter(Collider other)
        {


            if (!IsInMask(other.gameObject.layer))
                return;

            if (!other.TryGetComponent(out TargetCollider marker))
                return;

            if (!marker.TryGetTarget(out var target))
                return;

            if (ReferenceEquals(target, _self))
                return;

            _overlapCounts.TryGetValue(target, out int count);
            _overlapCounts[target] = count + 1;
            _candidates.Add(target);

            Debug.Log(
                $"{gameObject.name} contains {_candidates.Count}",
            this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out TargetCollider marker))
                return;

            if (!marker.TryGetTarget(out var target))
                return;

            if (!_overlapCounts.TryGetValue(target, out int count))
                return;

            count--;

            if (count > 0)
            {
                _overlapCounts[target] = count;
                return;
            }

            _overlapCounts.Remove(target);
            _candidates.Remove(target);

            Debug.Log(
                $"{gameObject.name} contains {_candidates.Count}",
            this);
        }

        private void OnDisable()
        {
            _overlapCounts.Clear();
            _candidates.Clear();
        }

        private void Reset()
        {
            var sphere = GetComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 5f;

            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private bool IsInMask(int layer)
        {
            return (_layerMask.value & (1 << layer)) != 0;
        }

        private void RemoveDestroyedCandidates()
        {
            _candidates.RemoveWhere(IsDestroyed);

            foreach (var candidate in new List<ITargetable>(_overlapCounts.Keys))
            {
                if (IsDestroyed(candidate))
                    _overlapCounts.Remove(candidate);
            }
        }

        private static bool IsDestroyed(ITargetable target)
        {
            if (target == null)
                return true;

            if (target is Object unityObject)
                return unityObject == null;

            return false;
        }
    }
}