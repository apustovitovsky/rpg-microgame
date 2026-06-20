using Etheria.Game.Interaction;
using UnityEngine;

namespace Etheria.Features.Interaction
{
    public sealed class DebugInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _message = "Object interacted.";

        public bool CanInteract => isActiveAndEnabled;

        public void Interact()
        {
            Debug.Log(_message, this);
        }
    }
}