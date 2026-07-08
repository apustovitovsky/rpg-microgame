using UnityEngine;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorDefinition",
        menuName = "Game/Actor/Actor Definition")]
    public sealed class ActorDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;

        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName)
            ? name
            : _displayName.Trim();
    }
}