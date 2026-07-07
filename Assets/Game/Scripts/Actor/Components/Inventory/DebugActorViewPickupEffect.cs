using Game.Pickup;
using UnityEngine;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "DebugActorViewPickupEffect",
        menuName = "Game/Pickup/Effects/Debug Actor View Effect")]
    public sealed class DebugActorViewPickupEffect : PickupEffect
    {
        [field: SerializeField]
        public string Message { get; private set; } =
            "Pickup effect applied";
    }
}