using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorView :
        MonoBehaviour,
        IActorView
    {
        [SerializeField]
        private Transform _cameraPivot;

        [SerializeField]
        private Transform _targetPoint;

        [SerializeField]
        private Transform _uiAnchor;

        [SerializeField]
        private MonoBehaviour[] _capabilities = Array.Empty<MonoBehaviour>();

        private readonly Dictionary<Type, object> _cache = new();
        private string _actorId = "Unknown";
        private bool _cacheBuilt;

        public string ActorId => _actorId;

        public Transform Root =>
            transform;

        public Transform TargetPoint => _targetPoint != null
            ? _targetPoint
            : Root;

        public Transform CameraPivot => _cameraPivot != null
            ? _cameraPivot
            : Root;

        public Transform UiAnchor => _uiAnchor != null
            ? _uiAnchor
            : Root;

        public void Initialize(string actorId)
        {
            actorId = actorId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(actorId))
                throw new ArgumentException("Actor id is required.", nameof(actorId));

            _actorId = actorId;
        }

        public bool TryGet<T>(out T capability)
            where T : class
        {
            BuildCacheIfNeeded();

            if (_cache.TryGetValue(typeof(T), out var value) &&
                IsAlive(value) &&
                value is T typedCapability)
            {
                capability = typedCapability;
                return true;
            }

            capability = null;
            return false;
        }

        public T Get<T>()
            where T : class
        {
            if (TryGet(out T capability))
            {
                return capability;
            }

            throw new InvalidOperationException(
                $"Actor capability '{typeof(T).Name}' is not registered on '{name}'.");
        }

        public void Rebuild()
        {
            _cache.Clear();
            _cacheBuilt = false;
            BuildCacheIfNeeded();
        }

        private void BuildCacheIfNeeded()
        {
            if (_cacheBuilt)
            {
                return;
            }

            _cacheBuilt = true;

            foreach (var capability in _capabilities)
            {
                if (capability == null)
                {
                    continue;
                }

                Register(capability.GetType(), capability);

                foreach (var interfaceType in capability.GetType().GetInterfaces())
                {
                    Register(interfaceType, capability);
                }
            }

            Register(typeof(IActorView), this);
        }

        private void Register(Type type, object capability)
        {
            if (_cache.ContainsKey(type))
            {
                Debug.LogWarning(
                    $"Duplicate actor capability '{type.Name}' on '{name}'. First one will be used.",
                    this);

                return;
            }

            _cache.Add(type, capability);
        }

        private static bool IsAlive(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is UnityEngine.Object unityObject)
            {
                return unityObject != null;
            }

            return true;
        }
    }
}