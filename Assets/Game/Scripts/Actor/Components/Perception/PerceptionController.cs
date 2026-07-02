using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PerceptionController :
        MonoBehaviour,
        IActorPerception
    {
        [SerializeField] private CharacterController _selfCollider;
        [SerializeField] private LayerMask _layerMask = ~0;

        private readonly Dictionary<GameObject, int> _overlapCounts = new();
        private readonly HashSet<GameObject> _candidates = new();

        public Transform Origin =>
            _selfCollider != null
                ? _selfCollider.transform
                : transform;

        public IReadOnlyCollection<GameObject> Candidates
        {
            get
            {
                RemoveDestroyedCandidates();
                return _candidates;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsSelfCollider(other))
            {
                return;
            }

            if (!IsInTargetMask(other.gameObject.layer))
            {
                return;
            }

            var actor = other.GetComponentInParent<ActorView>();
            if (actor == null)
            {
                return;
            }

            var candidate = actor.gameObject;

            _overlapCounts.TryGetValue(candidate, out var count);
            _overlapCounts[candidate] = count + 1;
            _candidates.Add(candidate);
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsSelfCollider(other))
            {
                return;
            }

            var actor = other.GetComponentInParent<ActorView>();
            if (actor == null)
            {
                return;
            }

            var candidate = actor.gameObject;

            if (!_overlapCounts.TryGetValue(candidate, out var count))
            {
                return;
            }

            count--;

            if (count > 0)
            {
                _overlapCounts[candidate] = count;
                return;
            }

            _overlapCounts.Remove(candidate);
            _candidates.Remove(candidate);
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

        private bool IsSelfCollider(Collider other)
        {
            return _selfCollider != null &&
                other == _selfCollider;
        }

        private bool IsInTargetMask(int layer)
        {
            return (_layerMask.value & (1 << layer)) != 0;
        }

        private void RemoveDestroyedCandidates()
        {
            _candidates.RemoveWhere(IsDestroyed);

            foreach (var candidate in new List<GameObject>(_overlapCounts.Keys))
            {
                if (IsDestroyed(candidate))
                {
                    _overlapCounts.Remove(candidate);
                }
            }
        }

        private static bool IsDestroyed(GameObject candidate)
        {
            return candidate == null;
        }
    }
}