using UnityEngine;

namespace Etheria.Features.Actor
{
    [CreateAssetMenu(
        fileName = "SyntyLookSettings",
        menuName = "Etheria/Features/Synty/Synty Look Settings")]
    public sealed class SyntyLookSettingsSO : ScriptableObject
    {
        [field: SerializeField] public bool InvertCamera { get; private set; }
        [field: SerializeField] public bool HideCursor { get; private set; }
        [field: SerializeField] public float MouseSensitivity { get; private set; } = 2f;
        [field: SerializeField] public float CameraDistance { get; private set; } = 2.5f;
        [field: SerializeField] public float CameraHeightOffset { get; private set; } = 0f;
        [field: SerializeField] public float CameraHorizontalOffset { get; private set; } = 0f;
        [field: SerializeField] public float CameraTiltOffset { get; private set; } = 15f;
        [field: SerializeField] public Vector2 CameraTiltBounds { get; private set; } = new(-70f, 70f);
        [field: SerializeField] public float PositionalCameraLag { get; private set; } = 0.2f;
        [field: SerializeField] public float RotationalCameraLag { get; private set; } = 0.2f;
    }
}
