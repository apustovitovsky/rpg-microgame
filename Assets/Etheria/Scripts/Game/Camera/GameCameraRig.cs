using Etheria.Core;
using UnityEngine;

namespace Etheria.Game.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DisallowMultipleComponent]
    public sealed class GameCameraRig : MonoBehaviour, IGameCameraProvider
    {
        [SerializeField, ReadOnly] private UnityEngine.Camera _camera;

        public UnityEngine.Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = GetComponent<UnityEngine.Camera>();

                return _camera;
            }
        }

        public Transform Transform => transform;

        private void Awake()
        {
            AssignCamera();
        }

        private void Reset()
        {
            AssignCamera();
        }

        private void OnValidate()
        {
            AssignCamera();
        }

        private void AssignCamera()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }
    }
}