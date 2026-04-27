using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface IMainCameraProvider
    {
        UnityEngine.Camera Camera { get; }
        Transform Transform { get; }
    }
}