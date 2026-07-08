using Game.Interaction;
using UnityEngine;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorInteractor :
        MonoBehaviour,
        IInteractor
    {
        [SerializeField] private Transform _origin;

        public Vector3 InteractionOrigin =>
            _origin != null
                ? _origin.position
                : transform.position;
    }
}