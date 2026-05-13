using System;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    [Serializable]
    public sealed class LayoutFamilyVisualSettings
    {
        [SerializeField] private LayoutSemanticFamily _family;
        [SerializeField] private Color _color = Color.white;
        [SerializeField, Range(0f, 1f)] private float _alpha = 1f;
        [SerializeField, Range(0f, 1f)] private float _size = 1f;

        public LayoutSemanticFamily Family => _family;
        public Color Color => _color;
        public float Alpha => _alpha;
        public float Size => _size;

        public void Initialize(LayoutSemanticFamily family, Color color, float alpha, float size)
        {
            _family = family;
            _color = color;
            _alpha = Mathf.Clamp01(alpha);
            _size = Mathf.Clamp01(size);
        }
    }
}
