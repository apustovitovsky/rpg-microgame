using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Interaction;
using UnityEngine;

namespace Game.Pickup
{
    [DisallowMultipleComponent]
    public sealed class DialogueInteractable :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private Transform _uiAnchor;
        [SerializeField] private Transform _targetAnchor;
        [SerializeField] private bool _isTargetable = true;

        public Vector3 InteractionPosition => throw new System.NotImplementedException();

        public float MaxRange => throw new System.NotImplementedException();

        public Vector3 InteractionPoint => throw new System.NotImplementedException();

        public bool CanInteract(InteractionContext context)
        {
            return false;
        }

        public UniTask InteractAsync(InteractionContext context, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
    }
}