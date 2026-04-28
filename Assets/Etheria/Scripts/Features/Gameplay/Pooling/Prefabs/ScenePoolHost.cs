using UnityEngine;

namespace Etheria.Features.Gameplay
{
    public class ScenePoolHost : MonoBehaviour
    {
        [field: SerializeField] public ActorPoolRoots Actors { get; private set; }
        [field: SerializeField] public PickupPoolRoots Pickups { get; private set; }
    }
}
