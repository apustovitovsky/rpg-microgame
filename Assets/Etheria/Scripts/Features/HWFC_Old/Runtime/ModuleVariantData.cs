using System;
using Etheria.Core.Helpers;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    [Serializable]
    public sealed class ModuleVariantData
    {
        [SerializeField] private string _sourceModuleName = string.Empty;
        [SerializeField] private int _sourceModuleIndex;
        [SerializeField] private LayoutSemanticFamily _layoutFamily;
        [SerializeField] private int _rotation;
        [SerializeField] private float _probability;
        [SerializeField] private int[] _connectors = new int[6];
        [SerializeField, ReadOnly] private string _left = string.Empty;
        [SerializeField, ReadOnly] private string _down = string.Empty;
        [SerializeField, ReadOnly] private string _back = string.Empty;
        [SerializeField, ReadOnly] private string _right = string.Empty;
        [SerializeField, ReadOnly] private string _up = string.Empty;
        [SerializeField, ReadOnly] private string _forward = string.Empty;

        public string SourceModuleName => _sourceModuleName;
        public int SourceModuleIndex => _sourceModuleIndex;
        public LayoutSemanticFamily LayoutFamily => _layoutFamily;
        public int Rotation => _rotation;
        public float Probability => _probability;

        public void Initialize(
            string sourceModuleName,
            int sourceModuleIndex,
            LayoutSemanticFamily layoutFamily,
            int rotation,
            float probability,
            int[] connectors)
        {
            _sourceModuleName = sourceModuleName ?? string.Empty;
            _sourceModuleIndex = sourceModuleIndex;
            _layoutFamily = layoutFamily;
            _rotation = rotation;
            _probability = probability;
            _connectors = connectors ?? Array.Empty<int>();
            UpdateDebugConnectorNames();
        }

        public int GetConnector(int direction)
        {
            if (_connectors == null || _connectors.Length != 6)
            {
                throw new InvalidOperationException("Variant must contain six connectors.");
            }

            if (direction < 0 || direction >= 6)
            {
                throw new ArgumentOutOfRangeException(nameof(direction));
            }

            return _connectors[direction];
        }

        private void UpdateDebugConnectorNames()
        {
            if (_connectors == null || _connectors.Length != 6)
            {
                _left = string.Empty;
                _down = string.Empty;
                _back = string.Empty;
                _right = string.Empty;
                _up = string.Empty;
                _forward = string.Empty;
                return;
            }

            _left = GetConnectorName(Orientations.LEFT);
            _down = GetConnectorName(Orientations.DOWN);
            _back = GetConnectorName(Orientations.BACK);
            _right = GetConnectorName(Orientations.RIGHT);
            _up = GetConnectorName(Orientations.UP);
            _forward = GetConnectorName(Orientations.FORWARD);
        }

        private string GetConnectorName(int direction)
        {
            return Enum.GetName(typeof(SemanticConnectorType), _connectors[direction]) ?? _connectors[direction].ToString();
        }
    }
}
