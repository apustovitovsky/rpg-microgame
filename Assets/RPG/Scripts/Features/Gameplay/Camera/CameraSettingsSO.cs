using Unity.Cinemachine;
using UnityEngine;

namespace RPG.Gameplay
{
    [CreateAssetMenu(
        fileName = "CameraSettings",
        menuName = "RPG/Gameplay/Camera/Camera Settings")]
    public sealed class CameraSettingsSO : ScriptableObject
    {
        [field: SerializeField] public CinemachineCamera CameraPrefab { get; private set; }
    }
}