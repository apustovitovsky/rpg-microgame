using UnityEngine;

namespace Game.Core
{
    public abstract class Definition : ScriptableObject
    {
        [SerializeField] private string _displayName;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(_displayName)
                ? name
                : _displayName;

        protected virtual void OnValidate()
        {
            _displayName = _displayName?.Trim();
        }
    }
}