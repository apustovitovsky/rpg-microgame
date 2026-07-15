using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public abstract class AssetDefinition :
        ScriptableObject,
        IFragmentProvider
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;

        public string Id => _id;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(_displayName)
                ? name
                : _displayName;

        public abstract bool TryGetFragment<TFragment>(
            out TFragment fragment)
            where TFragment : class;

        protected virtual void OnValidate()
        {
            _id = _id?.Trim();
            _displayName = _displayName?.Trim();
        }
    }

    public abstract class AssetDefinition<TInstance, TFragment> :
        AssetDefinition
        where TInstance : class
        where TFragment : class
    {
        [SerializeReference]
        private List<TFragment> _fragments = new();

        public abstract TInstance CreateInstance(
            Guid? instanceId = null);

        public override bool TryGetFragment<TConcreteFragment>(
            out TConcreteFragment fragment)
        {
            foreach (var current in _fragments)
            {
                if (current is TConcreteFragment typed)
                {
                    fragment = typed;
                    return true;
                }
            }

            fragment = default;
            return false;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            _fragments ??= new List<TFragment>();
        }
    }
}