using UnityEngine;

namespace Etheria.Game.Pooling
{
    public class ScenePoolHost : MonoBehaviour
    {
        [field: SerializeField] public PickupPoolRoots Pickups { get; private set; }
    }
}
