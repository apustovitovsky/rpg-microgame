using System;
using System.Collections.Generic;
using UnityEngine;


namespace Etheria.Features.HWFC
{
    [CreateAssetMenu(
        menuName = "Etheria/HWFC/Tile Definition",
        fileName = "TileDefinition")]
    public sealed class TileDefinitionSO : ScriptableObject
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField, Min(0.0001f)] private float _weight = 1f;
        [SerializeField] private bool _allowRotations = true;
        [SerializeField] private bool _allowMirrors;
        [SerializeField] private Vector3Int _size = Vector3Int.one;
        [SerializeField] private TileFaceDefinition[] _faces = new TileFaceDefinition[6];

        public GameObject Prefab => _prefab;
        public float Weight => _weight;
        public bool AllowRotations => _allowRotations;
        public bool AllowMirrors => _allowMirrors;
        public Vector3Int Size => _size;
        public IReadOnlyList<TileFaceDefinition> Faces => _faces;

        private void OnValidate()
        {
            _weight = Mathf.Max(0.0001f, _weight);
            _size.x = Mathf.Max(1, _size.x);
            _size.y = Mathf.Max(1, _size.y);
            _size.z = Mathf.Max(1, _size.z);

            if (_faces == null || _faces.Length != 6)
            {
                Array.Resize(ref _faces, 6);
            }

            for (var i = 0; i < _faces.Length; i++)
            {
                _faces[i] ??= new TileFaceDefinition();
            }
        }
    }
}
