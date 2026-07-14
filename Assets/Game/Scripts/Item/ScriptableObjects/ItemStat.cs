using UnityEngine;

namespace Game.Item
{
    [CreateAssetMenu(
        fileName = "ItemStat",
        menuName = "Game/Item/Stat")]
    public sealed class ItemStat :
        ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;

        public string Id => _id;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(_displayName)
                ? name
                : _displayName;

        private void OnValidate()
        {
            _id = _id?.Trim();
            _displayName = _displayName?.Trim();
        }
    }
}