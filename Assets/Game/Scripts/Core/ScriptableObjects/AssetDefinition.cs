using System;
using UnityEngine;

namespace Game.Core
{
    public abstract class AssetDefinition<TInstance> :
        ScriptableObject
        where TInstance : class
    {
        [SerializeField] private string _definitionId;
        [SerializeField] private string _displayName;

        public string Id => _definitionId;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(_displayName)
                ? name
                : _displayName;

        public abstract TInstance CreateInstance(
            Guid? instanceId = null);

        protected virtual void OnValidate()
        {
            _definitionId = _definitionId?.Trim();
            _displayName = _displayName?.Trim();
        }
    }
}