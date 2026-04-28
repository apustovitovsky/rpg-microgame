using Etheria.Core.Helpers;
using UnityEngine;

namespace Etheria.Game.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DisallowMultipleComponent]
    public sealed class GameCameraRig : MonoBehaviour, ICameraTransformProvider
    {
        [SerializeField, ReadOnly] private UnityEngine.Camera _camera;

        public Transform Transform => Camera.transform;
        public Vector3 Position => Transform.position;
        public Vector3 Forward => Transform.forward;

        private UnityEngine.Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = GetComponent<UnityEngine.Camera>();

                return _camera;
            }
        }

        private void Awake()
        {
            CacheCamera();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            CacheCamera();
        }

        private void OnValidate()
        {
            CacheCamera();
        }
#endif

        private void CacheCamera()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }
    }
}