using System.Threading;
using Cysharp.Threading.Tasks;
using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupComponent :
        MonoBehaviour,
        IPickup
    {
        [SerializeField] private PickupDefinition _definition;
        [SerializeField] private string _displayName = "Pickup";

        private WorldId _worldId;

        public WorldId WorldId => _worldId;

        public PickupDefinition Definition => _definition;

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName)
            ? WorldId.ToString()
            : _displayName.Trim();

        public bool IsCollectable =>
            isActiveAndEnabled &&
            gameObject.activeInHierarchy &&
            !WorldId.IsEmpty;

        public void Initialize(WorldId worldId)
        {
            _worldId = worldId;
        }

        public UniTask MarkCollectedAsync(CancellationToken token)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);

            return UniTask.CompletedTask;
        }
    }
}