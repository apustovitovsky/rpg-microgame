using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    [CreateAssetMenu(
        menuName = "Etheria/HWFC/Module Data",
        fileName = "ModuleData")]
    public sealed class ModuleDataSO : ScriptableObject
    {
        [SerializeField, Min(0.0001f)] private float _probability = 1f;
        [SerializeField] private bool _allowRotations = true;
        [SerializeField] private LayoutSemanticFamily _layoutFamily;

        public float Probability => _probability;
        public bool AllowRotations => _allowRotations;
        public LayoutSemanticFamily LayoutFamily => _layoutFamily;

        private void OnValidate()
        {
            _probability = Mathf.Max(0.0001f, _probability);
        }
    }
}
