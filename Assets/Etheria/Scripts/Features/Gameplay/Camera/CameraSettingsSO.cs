using Unity.Cinemachine;
using UnityEngine;

namespace Etheria.Gameplay
{
    [CreateAssetMenu(
        fileName = "CameraSettings",
        menuName = "Etheria/Gameplay/Camera/Camera Settings")]
    public sealed class CameraSettingsSO : ScriptableObject
    {
        [field: SerializeField] public CinemachineCamera CameraPrefab { get; private set; }
        [field: SerializeField] public float HorizontalLookSensitivity { get; private set; } = 0.2f;
        [field: SerializeField] public float VerticalLookSensitivity { get; private set; } = -0.15f;
        [field: SerializeField] public float MinPitch { get; private set; } = -35f;
        [field: SerializeField] public float MaxPitch { get; private set; } = 60f;
        [field: SerializeField] public float MinFieldOfView { get; private set; } = 40f;
        [field: SerializeField] public float MaxFieldOfView { get; private set; } = 65f;
    }
}
