using Game.Actor;
using UnityEngine;

namespace Game.AI
{
    [DisallowMultipleComponent]
    public sealed class NavMeshActorInputBinder : MonoBehaviour
    {
        [SerializeField] private NavMeshActorInput _input;
        [SerializeField] private MovementController _movement;
        [SerializeField] private ActorLookController _look;

        private void Awake()
        {
            if (_input == null || _movement == null || _look == null)
                return;

            _movement.Bind(_input);
            _look.Bind(_input);
        }

        private void OnDestroy()
        {
            if (_movement != null)
                _movement.Unbind();

            if (_look != null)
                _look.Unbind();
        }
    }
}