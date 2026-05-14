using System;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    [CreateAssetMenu(
        menuName = "Etheria/HWFC_Old/Module Set Data",
        fileName = "ModuleSetData")]
    public sealed class ModuleSetData : ScriptableObject
    {
        [SerializeField] private int _moduleCount;
        [SerializeField] private int _directionCount;
        [SerializeField] private ModuleVariantData[] _variants = Array.Empty<ModuleVariantData>();
        [SerializeField] private ModuleSet[] _constraints = Array.Empty<ModuleSet>();
        [SerializeField] private float[] _probabilities = Array.Empty<float>();

        public int ModuleCount => _moduleCount;
        public int DirectionCount => _directionCount;
        public ModuleVariantData[] Variants => _variants;
        public ModuleSet[] Constraints => _constraints;
        public float[] Probabilities => _probabilities;

        public ModuleSet GetConstraint(int moduleIndex, int directionIndex)
        {
            var index = GetConstraintIndex(moduleIndex, directionIndex);
            return _constraints[index];
        }

        public void Initialize(
            int moduleCount,
            int directionCount,
            ModuleVariantData[] variants,
            ModuleSet[] constraints,
            float[] probabilities)
        {
            _moduleCount = moduleCount;
            _directionCount = directionCount;
            _variants = variants ?? Array.Empty<ModuleVariantData>();
            _constraints = constraints ?? Array.Empty<ModuleSet>();
            _probabilities = probabilities ?? Array.Empty<float>();
        }

        private int GetConstraintIndex(int moduleIndex, int directionIndex)
        {
            if (moduleIndex < 0 || moduleIndex >= _moduleCount)
            {
                throw new ArgumentOutOfRangeException(nameof(moduleIndex));
            }

            if (directionIndex < 0 || directionIndex >= _directionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(directionIndex));
            }

            return moduleIndex * _directionCount + directionIndex;
        }
    }
}
