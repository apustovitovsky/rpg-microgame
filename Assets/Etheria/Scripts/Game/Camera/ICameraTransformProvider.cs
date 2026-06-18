using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface ICameraTransformProvider
    {
        Transform Transform { get; }
        Vector3 Forward { get; }
        Vector3 Right { get; }
        Vector3 EulerAngles { get; }
        Vector3 Position { get; }
    }
}
