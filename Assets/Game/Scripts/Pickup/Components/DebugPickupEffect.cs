using UnityEngine;

namespace Game.Pickup
{
    [CreateAssetMenu(
        fileName = "DebugPickupEffect",
        menuName = "Game/Pickup/Effects/Debug Pickup Effect")]
    public sealed class DebugPickupEffect : PickupEffect
    {
        [field: SerializeField]
        public string Message { get; private set; } = "Pickup collected";
    }
}