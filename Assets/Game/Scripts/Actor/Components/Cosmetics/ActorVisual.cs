using UnityEngine;

namespace Game.Actor
{
    public enum ActorColor
    {
        Black,
        Brown,
        White
    }

    public sealed class ActorVisual : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _leftHand;

        [Header("Materials")]
        [SerializeField] private Renderer[] _renderers;

        [SerializeField] private Material _blackMaterial;
        [SerializeField] private Material _brownMaterial;
        [SerializeField] private Material _whiteMaterial;

        [SerializeField] private ActorColor _color;

        public Animator Animator => _animator;
        public Transform Head => _head;
        public Transform RightHand => _rightHand;
        public Transform LeftHand => _leftHand;

        private void Awake()
        {
            ApplyColor();
        }

        [ContextMenu("Apply Color")]
        private void ApplyColor()
        {
            Material material = _color switch
            {
                ActorColor.Black => _blackMaterial,
                ActorColor.Brown => _brownMaterial,
                ActorColor.White => _whiteMaterial,
                _ => _brownMaterial
            };

            foreach (Renderer targetRenderer in _renderers)
            {
                if (targetRenderer != null)
                    targetRenderer.sharedMaterial = material;
            }
        }
    }
}