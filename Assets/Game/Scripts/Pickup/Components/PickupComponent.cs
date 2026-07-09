using Game.World;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class PickupComponent : MonoBehaviour
    {
        public IWorldPickup Pickup { get; private set; }

        public WorldId WorldId =>
            Pickup?.WorldId ?? default;

        public void Initialize(IWorldPickup pickup)
        {
            Pickup = pickup;
        }
    }
}