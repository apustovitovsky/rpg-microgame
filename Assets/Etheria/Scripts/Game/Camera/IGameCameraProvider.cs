using UnityEngine;

namespace Etheria.Game.Camera
{
    public interface IGameCameraProvider
    {
        UnityEngine.Camera Camera { get; }
        Transform Transform { get; }
    }
}