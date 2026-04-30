using Etheria.Core.Helpers;
using Etheria.Features.Input;
using UnityEngine;

namespace Etheria.Features.Actor
{
    public class SyntyCameraController : MonoBehaviour
    {
        private const string DefaultCameraPivotName = "CameraPivot";
        private const string DefaultPlayerTargetName = "SyntyPlayer_LookAt";
        private const string DefaultLockOnTargetName = "TargetLockOnPos";

        [Tooltip("The character game object")]
        [SerializeField]
        private GameObject _syntyCharacter;

        [field: SerializeField, ReadOnly]
        public Transform CameraPivot { get; private set; }

        [field: SerializeField, ReadOnly]
        public Transform PlayerTarget { get; private set; }

        [field: SerializeField, ReadOnly]
        public Transform LockOnTarget { get; private set; }

        [SerializeField]
        private SyntyLookSettingsSO _syntyLookSettings;

        private IGameInputRouter _inputRouter;

        public void Bind(IGameInputRouter inputRouter)
        {
            _inputRouter = inputRouter;
        }

        public void Unbind()
        {
            _inputRouter = null;
        }

        private void AutoAssignIfMissing()
        {
            if (CameraPivot == null)
                CameraPivot = FindDeepChildByName(_syntyCharacter.transform, DefaultCameraPivotName);

            if (PlayerTarget == null)
                PlayerTarget = FindDeepChildByName(_syntyCharacter.transform, DefaultPlayerTargetName);

            if (LockOnTarget == null)
                LockOnTarget = FindDeepChildByName(_syntyCharacter.transform, DefaultLockOnTargetName);
        }

        private static Transform FindDeepChildByName(Transform root, string childName)
        {
            var children = root.GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if (child.name == childName)
                    return child;
            }

            return null;
        }

        private void OnValidate()
        {
            AutoAssignIfMissing();
        }

        private void Reset()
        {
            AutoAssignIfMissing();
        }

        private const int _LAG_DELTA_TIME_ADJUSTMENT = 20;


        [SerializeField]
        private bool _invertCamera;
        [SerializeField]
        private bool _hideCursor;
        [SerializeField]
        private bool _isLockedOn;
        [SerializeField]
        private float _mouseSensitivity = 5f;
        [SerializeField]
        private Vector2 _cameraTiltBounds = new(-10f, 45f);
        [SerializeField]
        private float _positionalCameraLag = 1f;
        [SerializeField]
        private float _rotationalCameraLag = 1f;
        private float _cameraInversion;

        private float _lastAngleX;
        private float _lastAngleY;

        private Vector3 _lastPosition;

        private float _newAngleX;

        private float _newAngleY;
        private Vector3 _newPosition;

        private float _rotationX;
        private float _rotationY;

        /// <inheritdoc cref="Start" />
        private void Start()
        {
            AutoAssignIfMissing();

            if (_syntyLookSettings != null && _syntyLookSettings.HideCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            _cameraInversion = _syntyLookSettings != null && _syntyLookSettings.InvertCamera ? 1 : -1;


            CameraPivot.position = PlayerTarget.position;
            CameraPivot.rotation = PlayerTarget.rotation;

            _lastPosition = CameraPivot.position;
        }

        /// <inheritdoc cref="Update" />
        private void Update()
        {
            float positionalCameraLag = _syntyLookSettings != null ? _syntyLookSettings.PositionalCameraLag : _positionalCameraLag;
            float rotationalCameraLag = _syntyLookSettings != null ? _syntyLookSettings.RotationalCameraLag : _rotationalCameraLag;

            float positionalFollowSpeed = 1 / (positionalCameraLag / _LAG_DELTA_TIME_ADJUSTMENT);
            float rotationalFollowSpeed = 1 / (rotationalCameraLag / _LAG_DELTA_TIME_ADJUSTMENT);

            float mouseSensitivity = _syntyLookSettings != null ? _syntyLookSettings.MouseSensitivity : _mouseSensitivity;

            var mouseDelta = _inputRouter != null ? _inputRouter.MouseDelta : Vector2.zero;

            _rotationX = mouseDelta.y * _cameraInversion * mouseSensitivity;
            _rotationY = mouseDelta.x * mouseSensitivity;

            _newAngleX += _rotationX;

            Vector2 cameraTiltBounds = _syntyLookSettings != null ? _syntyLookSettings.CameraTiltBounds : _cameraTiltBounds;
            _newAngleX = Mathf.Clamp(_newAngleX, cameraTiltBounds.x, cameraTiltBounds.y);

            _newAngleX = Mathf.Lerp(_lastAngleX, _newAngleX, rotationalFollowSpeed * Time.deltaTime);


            if (_isLockedOn)
            {
                Vector3 aimVector = LockOnTarget.position - PlayerTarget.position;
                Quaternion targetRotation = Quaternion.LookRotation(aimVector);
                targetRotation = Quaternion.Lerp(CameraPivot.rotation, targetRotation, rotationalFollowSpeed * Time.deltaTime);
                _newAngleY = targetRotation.eulerAngles.y;
            }
            else
            {
                _newAngleY += _rotationY;
                _newAngleY = Mathf.Lerp(_lastAngleY, _newAngleY, rotationalFollowSpeed * Time.deltaTime);


            }

            _newPosition = PlayerTarget.position;
            _newPosition = Vector3.Lerp(_lastPosition, _newPosition, positionalFollowSpeed * Time.deltaTime);

            CameraPivot.position = _newPosition;
            CameraPivot.eulerAngles = new Vector3(_newAngleX, _newAngleY, 0);

            _lastPosition = _newPosition;
            _lastAngleX = _newAngleX;
            _lastAngleY = _newAngleY;

        }

        /// <summary>
        ///     Locks the camera to aim at a specified target.
        /// </summary>
        /// <param name="enable">Whether lock on is enabled or not.</param>
        /// <param name="newLockOnTarget">The target to lock on to.</param>
        public void LockOn(bool enable, Transform newLockOnTarget)
        {
            _isLockedOn = enable;

            if (newLockOnTarget != null)
            {
                LockOnTarget = newLockOnTarget;
            }
        }

        /// <summary>
        /// Gets the position of the camera.
        /// </summary>
        /// <returns>The position of the camera.</returns>
        public Vector3 GetCameraPosition()
        {
            // return _mainCamera != null ? _mainCamera.Position : CameraPivot.position;
            return CameraPivot.position;
        }

        /// <summary>
        /// Gets the forward vector of the camera.
        /// </summary>
        /// <returns>The forward vector of the camera.</returns>
        public Vector3 GetCameraForward()
        {
            // return _mainCamera != null ? _mainCamera.Forward : CameraPivot.forward;
            return CameraPivot.forward;
        }

        /// <summary>
        /// Gets the forward vector of the camera with the Y value zeroed.
        /// </summary>
        /// <returns>The forward vector of the camera with the Y value zeroed.</returns>
        public Vector3 GetCameraForwardZeroedY()
        {
            var forward = GetCameraForward();
            return new Vector3(forward.x, 0, forward.z);
        }

        /// <summary>
        /// Gets the normalised forward vector of the camera with the Y value zeroed.
        /// </summary>
        /// <returns>The normalised forward vector of the camera with the Y value zeroed.</returns>
        public Vector3 GetCameraForwardZeroedYNormalised()
        {
            return GetCameraForwardZeroedY().normalized;
        }


        /// <summary>
        /// Gets the right vector of the camera with the Y value zeroed.
        /// </summary>
        /// <returns>The right vector of the camera with the Y value zeroed.</returns>
        public Vector3 GetCameraRightZeroedY()
        {
            // var right = _mainCamera != null ? _mainCamera.Right : CameraPivot.right;
            var right = CameraPivot.right;
            return new Vector3(right.x, 0, right.z);
        }

        /// <summary>
        /// Gets the normalised right vector of the camera with the Y value zeroed.
        /// </summary>
        /// <returns>The normalised right vector of the camera with the Y value zeroed.</returns>
        public Vector3 GetCameraRightZeroedYNormalised()
        {
            return GetCameraRightZeroedY().normalized;
        }

        /// <summary>
        /// Gets the X value of the camera tilt.
        /// </summary>
        /// <returns>The X value of the camera tilt.</returns>
        public float GetCameraTiltX()
        {
            // return _mainCamera != null ? _mainCamera.EulerAngles.x : CameraPivot.eulerAngles.x;
            return CameraPivot.eulerAngles.x;
        }
    }
}
