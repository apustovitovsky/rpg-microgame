using System.Collections.Generic;
using Game.Targeting;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class TargetPerception :
        MonoBehaviour
    {
        [SerializeField]
        private LayerMask _layerMask = ~0;

        [SerializeField]
        private ActorTarget _self;

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

            if (!other.TryGetComponent(out TargetCapsule marker))
                return;

            if (!marker.TryGetTarget(out var target))
                return;

            if (ReferenceEquals(target, _self))
                return;

            _overlapCounts.TryGetValue(target, out int count);
            _overlapCounts[target] = count + 1;
            _candidates.Add(target);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out TargetCapsule marker))
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